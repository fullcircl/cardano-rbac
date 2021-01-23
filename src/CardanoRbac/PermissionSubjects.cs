using System;
namespace CardanoRbac
{
    public class PermissionSubjects
    {
        public PermissionSubjects(RbacPermission permission, Uri[] subjects)
        {
            Permission = permission;
            Subjects = subjects;
        }

        public RbacPermission Permission { get; set; }
        public Uri[] Subjects { get; set; }
    }
}
