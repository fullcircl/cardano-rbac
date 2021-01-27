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

                IEnumerable<RbacPermission> permissions;

                if (subject != null && resource != null)
                {
                    permissions = policy.QueryPermissions(subject, resource);
                }
                else if (subject != null)
                {
                    permissions = policy.QueryPermissions(subject);
                }
                else if (resource != null)
                {
                    permissions = policy.QueryPermissions(resource);
                }
                else
                {
                    Console.WriteLine("Missing --subject or --resource option.");
                    return;
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
