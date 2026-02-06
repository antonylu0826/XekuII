using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XekuII.ApiHost.API.Security;

/// <summary>
/// Provides the current user's effective CRUD permissions for all registered entity types.
/// Uses XAF's IsGrantedExtensions to check permissions against the security system.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly ISecurityProvider _securityProvider;
    private readonly IObjectSpaceFactory _objectSpaceFactory;

    public PermissionsController(
        ISecurityProvider securityProvider,
        IObjectSpaceFactory objectSpaceFactory)
    {
        _securityProvider = securityProvider;
        _objectSpaceFactory = objectSpaceFactory;
    }

    /// <summary>
    /// Get the current user's effective CRUD permissions for all generated entity types.
    /// </summary>
    [HttpGet("my-permissions")]
    public IActionResult GetMyPermissions()
    {
        var security = (IRequestSecurityStrategy)_securityProvider.GetSecurity();
        using var objectSpace = _objectSpaceFactory.CreateObjectSpace<object>();

        var boAssembly = typeof(Startup).Assembly;
        var entityTypes = boAssembly.GetTypes()
            .Where(t => t.Namespace == "XekuII.ApiHost.BusinessObjects"
                     && !t.IsAbstract
                     && typeof(XPBaseObject).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToList();

        var result = new Dictionary<string, object>();
        foreach (var type in entityTypes)
        {
            result[type.Name] = new
            {
                read = security.CanRead(type, objectSpace),
                create = security.CanCreate(type, objectSpace),
                write = security.CanWrite(type, objectSpace),
                delete = security.CanDelete(type, objectSpace),
                navigate = security.CanNavigate(type.FullName),
            };
        }

        return Ok(result);
    }
}
