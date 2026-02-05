using DevExpress.Persistent.Base;

namespace XekuII.ApiHost.API.Security
{
    public class RoleDto
    {
        public Guid Oid { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsAdministrative { get; set; }
        public bool CanEditModel { get; set; }
        public SecurityPermissionPolicy PermissionPolicy { get; set; }
    }

    public class TypePermissionDto
    {
        public Guid Oid { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public List<TypeOperationPermissionDto> Operations { get; set; } = new();
    }

    public class TypeOperationPermissionDto
    {
        public string Operation { get; set; } = string.Empty; // Read, Write, Create, Delete, Navigate
        public SecurityPermissionState? State { get; set; }
    }

    public class NavigationPermissionDto
    {
        public Guid Oid { get; set; }
        public string ItemPath { get; set; } = string.Empty;
        public SecurityPermissionState? State { get; set; }
    }

    public class ObjectPermissionDto
    {
        public Guid Oid { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public string Criteria { get; set; } = string.Empty;
        public SecurityPermissionState? ReadState { get; set; }
        public SecurityPermissionState? WriteState { get; set; }
        public SecurityPermissionState? DeleteState { get; set; }
        public SecurityPermissionState? NavigateState { get; set; }
    }

    public class UserDto
    {
        public Guid Oid { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class AssignRoleRequest
    {
        public string RoleName { get; set; } = string.Empty;
    }
}
