using CustomerProfileAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerProfileAPI.Controllers;

[Route("api/certs")]
[ApiController]
public class CertsController(ILogger<CertsController> logger, ICertStoreContext certStoreContext) : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<CertFile>> GetCertsAsync()
    {
        logger.LogInformation("GetCertsAsync");
        certStoreContext.LoadCertFiles();
        return certStoreContext.CertFiles;
    }
}