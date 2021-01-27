using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CardanoRbac.Cli.Extensions;

namespace CardanoRbac.Cli
{
    class Program
    {
        public static Stream? _inputStream = null;

        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
            };
            rootCommand.Description = "Cardano RBAC CLI";

            var validateCommand = new Command("validate")
            {
                new Option<FileInfo>(
                    "--file",
                    "The JSON file to validate."
                ).ExistingOnly(),
            };
            validateCommand.Description = "Validate an RBAC policy file.";
            validateCommand.Handler = CommandHandler.Create<FileInfo>(async file =>
            {
                if (file != null)
                {
                    _inputStream = File.OpenRead(file.FullName);
                }

                if (_inputStream == null)
                {
                    throw new Exception("No input provided.");
                }

                var validationResult = await RbacPolicy.ValidateAsync(_inputStream);

                if (validationResult.Any())
                {
                    foreach (var validationError in validationResult)
                    {
                        if (validationError != null)
                        {
                            Console.WriteLine(validationError.ToString());
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Document is valid.");
                }
            });
            rootCommand.Add(validateCommand);

            var queryCommand = new Command("query")
            {
                new Option<FileInfo>(
                    "--file",
                    "The JSON file to query."
                ).ExistingOnly(),
                new Option<Uri>(
                    "--subject",
                    "Find all permission and roles assiciated with a subject"
                ),
                new Option<string>(
                    "--resource",
                    "Find all permission and roles assiciated with a resource"
                ),
            };
            queryCommand.Description = "Query an RBAC policy file.";
            queryCommand.Handler = CommandHandler.Create<FileInfo, Uri?, string?>(async (file, subject, resource) =>
            { 
                if (file != null)
                {
                    _inputStream = File.OpenRead(file.FullName);
                }

                if (_inputStream == null)
                {
                    throw new Exception("No input provided.");
                }

                var policy = await RbacPolicy.FromJsonAsync(_inputStream);

                IEnumerable<PermissionSubjects> permissions = policy.PermissionSubjects;

                if (subject != null)
                {
                    permissions = permissions.Where(p => p.Subjects.Any(s => s == subject));
                }
                if (resource != null)
                {
                    permissions = permissions.Where(p => p.Permission.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase));
                }

                IEnumerable<RbacRole> roles = policy.Roles.Traverse(r => r.Roles);

                if (subject != null)
                {
                    roles = roles.Where(r => r.Subjects.Any(s => s == subject));
                }
                if (resource != null)
                {
                    roles = roles.Where(r => r.Permissions.Any(p => p.Resource.Equals(resource, StringComparison.OrdinalIgnoreCase)));
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters =
                    {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                    }
                };

                Console.WriteLine("Permissions:");
                foreach (var permission in permissions)
                {
                    Console.WriteLine(JsonSerializer.Serialize(permission, options));
                }

                Console.WriteLine(Environment.NewLine + "Roles:");
                foreach (var role in roles)
                {
                    Console.WriteLine(JsonSerializer.Serialize(role, options));
                }
            });
            rootCommand.Add(queryCommand);

            var commandLineBuilder = new CommandLineBuilder(rootCommand);
            commandLineBuilder.UseMiddleware(async (context, next) =>
            {
                if (context.Console.IsInputRedirected)
                {
                    _inputStream = Console.OpenStandardInput();
                }

                await next(context);
            });

            commandLineBuilder.UseDefaults();
            var parser = commandLineBuilder.Build();
            // Parse the incoming args and invoke the handler
            return await parser.InvokeAsync(args);
        }
    }
}
