using System;
namespace CardanoRbac
{
    public class PolicyPatchTransaction : PolicyTransaction
    {
        public PolicyPatchTransaction(Uri policyUrn, object body) : base(policyUrn, TransactionMode.Patch)
        {
            Body = body;
        }

        public object Body { get; set; }
    }
}
