using System;

namespace CardanoRbac
{
    public class RbacPolicy
    {
        public RbacPolicy(Uri urn, PermissionSubjects[]? permissionSubjects, RbacRole[]? roles)
        {
            Urn = urn;
            PermissionSubjects = permissionSubjects ?? Array.Empty<PermissionSubjects>();
            Roles = roles ?? Array.Empty<RbacRole>();
        }
        public Uri Urn { get; set; }
        public PermissionSubjects[] PermissionSubjects { get; set; }
        public RbacRole[] Roles { get; set; }
    }
}
