using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace XekuII.ApiHost.API.Security
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IObjectSpaceFactory _objectSpaceFactory;

        public RolesController(IObjectSpaceFactory objectSpaceFactory)
        {
            _objectSpaceFactory = objectSpaceFactory;
        }

        #region Role CRUD
        [HttpGet]
        public IActionResult GetRoles()
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var roles = objectSpace.GetObjectsQuery<PermissionPolicyRole>()
                .Select(r => new RoleDto
                {
                    Oid = r.Oid,
                    Name = r.Name,
                    IsAdministrative = r.IsAdministrative,
                    CanEditModel = r.CanEditModel,
                    PermissionPolicy = r.PermissionPolicy
                })
                .ToList();
            return Ok(roles);
        }

        [HttpPost]
        public IActionResult CreateRole([FromBody] RoleDto dto)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.CreateObject<PermissionPolicyRole>();
            role.Name = dto.Name;
            role.IsAdministrative = dto.IsAdministrative;
            role.CanEditModel = dto.CanEditModel;
            role.PermissionPolicy = dto.PermissionPolicy;

            objectSpace.CommitChanges();
            return CreatedAtAction(nameof(GetRoles), new { id = role.Oid }, new { role.Oid });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateRole(Guid id, [FromBody] RoleDto dto)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.GetObjectByKey<PermissionPolicyRole>(id);
            if (role == null) return NotFound();

            role.Name = dto.Name;
            role.IsAdministrative = dto.IsAdministrative;
            role.CanEditModel = dto.CanEditModel;
            role.PermissionPolicy = dto.PermissionPolicy;

            objectSpace.CommitChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteRole(Guid id)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.GetObjectByKey<PermissionPolicyRole>(id);
            if (role == null) return NotFound();

            objectSpace.Delete(role);
            objectSpace.CommitChanges();
            return NoContent();
        }
        #endregion

        #region Type Permissions
        [HttpGet("{id}/type-permissions")]
        public IActionResult GetTypePermissions(Guid id)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.GetObjectByKey<PermissionPolicyRole>(id);
            if (role == null) return NotFound();

            var permissions = role.TypePermissions.Select(tp => new TypePermissionDto
            {
                Oid = tp.Oid,
                TargetType = tp.TargetType?.FullName ?? tp.TargetType?.Name ?? "Unknown",
                Operations = new List<TypeOperationPermissionDto>
                {
                    new TypeOperationPermissionDto { Operation = SecurityOperations.Read, State = tp.ReadState },
                    new TypeOperationPermissionDto { Operation = SecurityOperations.Write, State = tp.WriteState },
                    new TypeOperationPermissionDto { Operation = SecurityOperations.Create, State = tp.CreateState },
                    new TypeOperationPermissionDto { Operation = SecurityOperations.Delete, State = tp.DeleteState },
                    new TypeOperationPermissionDto { Operation = SecurityOperations.Navigate, State = tp.NavigateState }
                }.Where(o => o.State.HasValue).ToList()
            }).ToList();
            return Ok(permissions);
        }

        [HttpPost("{id}/type-permissions")]
        public IActionResult AddTypePermission(Guid id, [FromBody] TypePermissionDto dto)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.GetObjectByKey<PermissionPolicyRole>(id);
            if (role == null) return NotFound();

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == dto.TargetType || t.Name == dto.TargetType);

            if (type == null) return BadRequest($"Type {dto.TargetType} not found.");

            foreach (var op in dto.Operations)
            {
                if (op.State.HasValue)
                {
                    role.AddTypePermission(type, op.Operation, op.State.Value);
                }
            }

            objectSpace.CommitChanges();
            return Ok();
        }

        [HttpDelete("type-permissions/{permissionId}")]
        public IActionResult DeleteTypePermission(Guid permissionId)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyTypePermissionObject>();
            var permission = objectSpace.GetObjectByKey<PermissionPolicyTypePermissionObject>(permissionId);
            if (permission == null) return NotFound();

            objectSpace.Delete(permission);
            objectSpace.CommitChanges();
            return NoContent();
        }
        #endregion

        #region Navigation Permissions
        [HttpGet("{id}/navigation-permissions")]
        public IActionResult GetNavigationPermissions(Guid id)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.GetObjectByKey<PermissionPolicyRole>(id);
            if (role == null) return NotFound();

            var permissions = role.NavigationPermissions.Select(np => new NavigationPermissionDto
            {
                Oid = np.Oid,
                ItemPath = np.ItemPath,
                State = np.NavigateState
            }).ToList();
            return Ok(permissions);
        }

        [HttpPost("{id}/navigation-permissions")]
        public IActionResult AddNavigationPermission(Guid id, [FromBody] NavigationPermissionDto dto)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.GetObjectByKey<PermissionPolicyRole>(id);
            if (role == null) return NotFound();

            if (dto.State.HasValue)
            {
                role.AddNavigationPermission(dto.ItemPath, dto.State.Value);
            }

            objectSpace.CommitChanges();
            return Ok();
        }

        [HttpDelete("navigation-permissions/{permissionId}")]
        public IActionResult DeleteNavigationPermission(Guid permissionId)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyNavigationPermissionObject>();
            var permission = objectSpace.GetObjectByKey<PermissionPolicyNavigationPermissionObject>(permissionId);
            if (permission == null) return NotFound();

            objectSpace.Delete(permission);
            objectSpace.CommitChanges();
            return NoContent();
        }
        #endregion

        #region Object Permissions
        [HttpGet("{id}/object-permissions")]
        public IActionResult GetObjectPermissions(Guid id)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.GetObjectByKey<PermissionPolicyRole>(id);
            if (role == null) return NotFound();

            var permissions = role.TypePermissions.SelectMany(tp => tp.ObjectPermissions.Select(op => new ObjectPermissionDto
            {
                Oid = op.Oid,
                TargetType = tp.TargetType?.FullName ?? tp.TargetType?.Name ?? "Unknown",
                Criteria = op.Criteria,
                ReadState = op.ReadState,
                WriteState = op.WriteState,
                DeleteState = op.DeleteState,
                NavigateState = op.NavigateState
            })).ToList();
            return Ok(permissions);
        }

        [HttpPost("{id}/object-permissions")]
        public IActionResult AddObjectPermission(Guid id, [FromBody] ObjectPermissionRequest dto)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyRole>();
            var role = objectSpace.GetObjectByKey<PermissionPolicyRole>(id);
            if (role == null) return NotFound();

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName == dto.TargetType || t.Name == dto.TargetType);

            if (type == null) return BadRequest($"Type {dto.TargetType} not found.");

            role.AddObjectPermission(type, dto.Operation, dto.Criteria, dto.State);

            objectSpace.CommitChanges();
            return Ok();
        }

        [HttpDelete("object-permissions/{permissionId}")]
        public IActionResult DeleteObjectPermission(Guid permissionId)
        {
            using var objectSpace = _objectSpaceFactory.CreateObjectSpace<PermissionPolicyObjectPermissionsObject>();
            var permission = objectSpace.GetObjectByKey<PermissionPolicyObjectPermissionsObject>(permissionId);
            if (permission == null) return NotFound();

            objectSpace.Delete(permission);
            objectSpace.CommitChanges();
            return NoContent();
        }
        #endregion
    }

    public class ObjectPermissionRequest
    {
        public string TargetType { get; set; } = string.Empty;
        public string Criteria { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty; // Read, Write, Delete, Navigate
        public SecurityPermissionState State { get; set; }
    }
}
