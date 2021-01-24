using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NJsonSchema;

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
                    "An option whose argument is parsed as a FileInfo"
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

                using JsonDocument document = JsonDocument.Parse(_inputStream);
                JsonElement root = document.RootElement;
                JsonElement schemaElement = root.GetProperty("$schema");
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
                    Console.WriteLine("$schema is not valid. To get a list of valid schemas, run: crbac schema list");
                    Console.WriteLine("$schema: " + schemaId);
                }

                if (schemaUrl != null)
                {
                    var schema = await JsonSchema.FromUrlAsync(schemaUrl);
                    using var stream = new MemoryStream();
                    using var writer = new Utf8JsonWriter(stream);
                    document.WriteTo(writer);
                    writer.Flush();
                    string jsonText = Encoding.UTF8.GetString(stream.ToArray());

                    var validationResult = schema.Validate(jsonText);

                    if (validationResult.Count > 0)
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
                }
            });
            rootCommand.Add(validateCommand);

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
