using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NJsonSchema;

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

        public static async Task<RbacPolicy> FromJsonAsync(Stream utf8Json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

#pragma warning disable CS8603 // Possible null reference return.
            return await JsonSerializer.DeserializeAsync<RbacPolicy>(utf8Json, options);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static async Task<IEnumerable<ValidationError>> ValidateAsync(Stream utf8Json)
        {
            using JsonDocument document = await JsonDocument.ParseAsync(utf8Json);

            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("$schema", out JsonElement schemaElement))
            {
                return new[] { new ValidationError { Message = "Document is missing the required \"$schema\" property." } };
            }

            string schemaId = schemaElement.GetString() ?? "";

            string? schemaUrl = null;
            if (schemaId.Equals(RbacSchemas.POLICY_DRAFT, StringComparison.OrdinalIgnoreCase))
            {
                schemaUrl = RbacSchemas.POLICY_DRAFT;
            }
            else if (schemaId.Equals(RbacSchemas.TX_DRAFT, StringComparison.OrdinalIgnoreCase))
            {
                schemaUrl = RbacSchemas.TX_DRAFT;
            }
            else
            {
                return new[] { new ValidationError { Message = "$schema is not valid: " + schemaId } };
            }

            var schema = await JsonSchema.FromUrlAsync(schemaUrl);
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            document.WriteTo(writer);
            writer.Flush();
            string jsonText = Encoding.UTF8.GetString(stream.ToArray());

            var validationResult = schema.Validate(jsonText);

            return validationResult.Select(ve => new ValidationError { Message = ve?.ToString() });
        }
    }
}
