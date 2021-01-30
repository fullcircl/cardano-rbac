using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

        public static async Task<PolicyTransaction> FromJsonAsync(Stream utf8Json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            PolicyTransaction tx = await JsonSerializer.DeserializeAsync<PolicyTransaction>(utf8Json, options);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            switch (tx.Method)
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            {
                case TransactionMode.Delete:
                    return tx;
                case TransactionMode.Patch:
#pragma warning disable CS8603 // Possible null reference return.
                    return await JsonSerializer.DeserializeAsync<PolicyPatchTransaction>(utf8Json, options);
#pragma warning restore CS8603 // Possible null reference return.
                case TransactionMode.Put:
#pragma warning disable CS8603 // Possible null reference return.
                    return await JsonSerializer.DeserializeAsync<PolicyPutTransaction>(utf8Json, options);
#pragma warning restore CS8603 // Possible null reference return.
                default:
                    throw new InvalidOperationException("");
            }
        }
    }
}
