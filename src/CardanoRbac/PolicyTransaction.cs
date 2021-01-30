using System;
namespace CardanoRbac
{
    public class PolicyTransaction
    {
        public PolicyTransaction(Uri policyUrn, TransactionMode method)
        {
            PolicyUrn = policyUrn;
            Method = method;
        }

        public Uri PolicyUrn { get; set; }
        public TransactionMode Method { get; set; }
    }
}
