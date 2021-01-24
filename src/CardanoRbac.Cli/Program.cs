using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
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
