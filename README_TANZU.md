To inject custom certificates into a workload on **Tanzu Application Platform (TAP)** so that the certificates are
included in the container's trust store during the build process, you can use TAP's **Supply Chain** and **Build Service
** integrations. Here's how to achieve this:

---

### **Step 1: Prepare Certificates as a Kubernetes Secret**

[Certificate Creation Step](./README_CERT-CREATION.md)

1. **Create a Kubernetes Secret** containing your custom certificates (e.g., `.crt` or `.pem` files):
   ```bash
   kubectl create secret generic custom-certs \
     --from-file=my-custom-cert1.crt=./certs/my-custom-cert1.crt \
     --from-file=my-custom-cert2.crt=./certs/my-custom-cert2.crt \
     -n YOUR-NAMESPACE
   ```
   
---

### **Step 2: Configure the Workload to Use the Secret**

Define a `workload.yaml` that:

1. Mounts the certificate secret into the build container.
2. Configures the Buildpack to trust the certificates.

```yaml
# workload.yaml
apiVersion: carto.run/v1alpha1
kind: Workload
metadata:
  name: my-app
  namespace: YOUR-NAMESPACE
  labels:
    apps.tanzu.vmware.com/workload-type: web
spec:
  source:
    git:
      url: https://github.com/your-repo/my-app.git
      ref:
        branch: main
  build:
    env:
      - name: BP_ENABLE_CA_CERTIFICATES
        value: "true" # Enable CA Certificates Buildpack
      - name: SSL_CERT_DIR
        value: "/etc/ssl/certs" # Default trust store path
    volumes:
      - name: custom-certs-volume
        secret:
          secretName: custom-certs # Reference the secret
    volumeMounts:
      - name: custom-certs-volume
        mountPath: /etc/ssl/certs # Mount certificates into the trust store
```

---

### **Step 3: Apply the Workload**

Deploy the workload to TAP:

```bash
tanzu apps workload apply -f workload.yaml
```

---

### **Step 4: Verify Certificates in the Built Image**

1. **Check the Build Logs**:
   ```bash
   tanzu apps workload tail my-app --since 1h
   ```
   Look for the CA Certificates Buildpack output:
   ```
   [INFO] Adding 2 certificate(s) to system truststore...
   ```

2. **Inspect the Container**:
   If your workload is running, exec into the container to verify certificates:
   ```bash
   kubectl exec -it POD_NAME -- sh
   ls /etc/ssl/certs # Should include your custom certificates
   ```

---

### **How It Works**

1. **TAP Supply Chain Integration**:
    - TAP uses **kpack** (via Tanzu Build Service) to build OCI images using Cloud Native Buildpacks.
    - The `BP_ENABLE_CA_CERTIFICATES` environment variable triggers
      the [CA Certificates Buildpack](https://github.com/paketo-buildpacks/ca-certificates) to add certificates to the
      trust store.

2. **Volume Mounts**:
    - The `custom-certs` secret is mounted into the build container at `/etc/ssl/certs`.
    - The CA Certificates Buildpack copies these certificates into the final image's trust store.

3. **Runtime Trust**:
    - Applications in the container will automatically trust certificates in `/etc/ssl/certs` (used by .NET, Java,
      Node.js, etc.).

---

### **Advanced: Customizing the Supply Chain**

If your TAP installation uses a **custom supply chain**, ensure it includes the `ca-certificates` buildpack. Example
`cluster-supply-chain.yaml` snippet:

```yaml
spec:
  resources:
    - name: source-provider
      ...
    - name: image-builder
      params:
        - name: clusterbuilder
          value: default
        - name: env
          value:
            - name: BP_ENABLE_CA_CERTIFICATES
              value: "true"
```

---

### **Notes**

1. **Certificate Formats**:
    - Use `.crt` or `.pem` files for public certificates.
    - Private keys should be managed securely (e.g., via Secrets or Vault).

2. **Security**:
    - Avoid embedding long-lived certificates in images. Use dynamic certificate injection (e.g., using `cert-manager`)
      for production.

3. **TAP Version Compatibility**:
    - This approach works with TAP 1.5+ and assumes the Paketo Buildpacks are configured in your Tanzu Build Service.

---

### **Troubleshooting**

- **Missing Certificates**:
    - Ensure the `mountPath` in `volumeMounts` matches the `SSL_CERT_DIR` path.
    - Verify the secret is accessible in the workload's namespace.

- **Buildpack Not Enabled**:
    - Confirm `BP_ENABLE_CA_CERTIFICATES=true` is set in the workload's `env`.

---
