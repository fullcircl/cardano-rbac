using System;
namespace CardanoRbac
{
    public class RbacPermission
    {
        public RbacPermission(PermissionMode mode, PermissionAction action, string resource)
        {
            Mode = mode;
            Action = action;
            Resource = resource;
        }

        public PermissionMode Mode { get; set; }
        public PermissionAction Action { get; set; }
        public string Resource { get; set; }
    }
}
