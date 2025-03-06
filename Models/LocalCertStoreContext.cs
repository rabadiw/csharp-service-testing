using System.Security.Cryptography.X509Certificates;

namespace CustomerProfileAPI.Models;

public interface ICertStoreContext
{
    HashSet<CertFile> CertFiles { get; set; }

    void LoadCertFiles();
}

public record CertFile(string Name, string Issuer, string Subject /*, byte[] Content*/);

public class LocalCertStoreContext : ICertStoreContext
{
    public HashSet<CertFile> CertFiles { get; set; } = [];

    public void LoadCertFiles()
    {
        using X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);
        CertFiles = store.Certificates
            .Select(cert => new CertFile(cert.Thumbprint, cert.Issuer, cert.Subject /*, cert.RawData*/))
            .ToHashSet();
    }
}