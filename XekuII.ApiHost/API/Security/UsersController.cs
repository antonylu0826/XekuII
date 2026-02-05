using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XekuII.ApiHost.BusinessObjects;

namespace XekuII.ApiHost.API.Security
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public UsersController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<ApplicationUser>();
            var users = objectSpace.GetObjectsQuery<ApplicationUser>()
                .Select(u => new UserDto
                {
                    Oid = u.Oid,
                    UserName = u.UserName,
                    IsActive = u.IsActive,
                    Roles = u.Roles.Cast<PermissionPolicyRole>().Select(r => r.Name).ToList()
                })
                .ToList();
            return Ok(users);
        }

        [HttpPost("{id}/roles")]
        public IActionResult AssignRole(Guid id, [FromBody] AssignRoleRequest request)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<ApplicationUser>();
            var user = objectSpace.GetObjectByKey<ApplicationUser>(id);
            if (user == null) return NotFound("User not found.");

            var role = objectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == request.RoleName);
            if (role == null) return NotFound($"Role {request.RoleName} not found.");

            if (!user.Roles.Cast<PermissionPolicyRole>().Any(r => r.Oid == role.Oid))
            {
                user.Roles.Add(role);
                objectSpace.CommitChanges();
            }

            return Ok();
        }

        [HttpDelete("{id}/roles/{roleName}")]
        public IActionResult RemoveRole(Guid id, string roleName)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<ApplicationUser>();
            var user = objectSpace.GetObjectByKey<ApplicationUser>(id);
            if (user == null) return NotFound("User not found.");

            var role = user.Roles.Cast<PermissionPolicyRole>().FirstOrDefault(r => r.Name == roleName);
            if (role != null)
            {
                user.Roles.Remove(role);
                objectSpace.CommitChanges();
            }

            return Ok();
        }
    }
}
