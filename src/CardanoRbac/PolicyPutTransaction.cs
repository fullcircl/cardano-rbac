using System;
namespace CardanoRbac
{
    public class PolicyPutTransaction : PolicyTransaction
    {
        public PolicyPutTransaction(Uri policyUrn, RbacPolicy body) : base(policyUrn, TransactionMode.Put)
        {
            Body = body;
        }

        public RbacPolicy Body { get; set; }
    }
}
