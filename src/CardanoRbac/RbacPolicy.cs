using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CardanoRbac.Extensions;
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

        public IEnumerable<RbacPermission> QueryPermissions(Uri subject)
        {
            var permissions = PermissionSubjects
                .Where(ps => ps.Subjects.Any(s => s == subject))
                .Select(ps => ps.Permission);

            permissions = permissions.Concat(Roles
                .Traverse(r => r.Roles)
                .Where(r => r.Subjects.Any(s => s == subject))
                .SelectMany(r => r.Permissions));

            return permissions;
        }

        public IEnumerable<RbacPermission> QueryPermissions(string resource)
        {
            var permissions = PermissionSubjects
                .Where(ps => ps.Permission.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase))
                .Select(ps => ps.Permission);

            permissions = permissions.Concat(Roles
                .Traverse(r => r.Roles)
                .SelectMany(r => r.Permissions)
                .Where(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase)));

            return permissions;
        }

        public IEnumerable<RbacPermission> QueryPermissions(Uri subject, string resource)
        {
            var permissions = PermissionSubjects
                .Where(ps => ps.Subjects.Any(s => s == subject)
                    && ps.Permission.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase))
                .Select(ps => ps.Permission);

            permissions = permissions.Concat(Roles
                .Traverse(r => r.Roles)
                .Where(r => r.Subjects.Any(s => s == subject))
                .SelectMany(r => r.Permissions)
                .Where(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase)));

            return permissions;
        }

        public PermissionMode? ResolvePermissionMode(Uri subject, string resource, PermissionAction action)
        {
            var permissions = PermissionSubjects
                .Where(ps => ps.Subjects.Any(s => s == subject)
                    && ps.Permission.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase)
                    && ps.Permission.Action == action)
                .Select(ps => ps.Permission);

            permissions = permissions.Concat(Roles
                .Traverse(r => r.Roles)
                .Where(r => r.Subjects.Any(s => s == subject))
                .SelectMany(r => r.Permissions)
                .Where(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase)
                    && p.Action == action));

            if (permissions.Any())
            {
                return permissions.Any(p => p.Mode == PermissionMode.deny) ? PermissionMode.deny : PermissionMode.grant;
            }

            return null;
        }
    }
}
