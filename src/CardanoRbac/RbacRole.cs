using System;
namespace CardanoRbac
{
    public class RbacRole
    {
        public RbacRole(string name, RbacPermission[] permissions, Uri[] subjects, RbacRole[]? roles)
        {
            Name = name;
            Permissions = permissions;
            Subjects = subjects;
            Roles = roles ?? Array.Empty<RbacRole>();
        }

        public string Name { get; set; }
        public RbacPermission[] Permissions { get; set; }
        public Uri[] Subjects { get; set; }
        public RbacRole[] Roles { get; set; }
    }
}
