using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CardanoRbac.Cli
{
    class Program
    {
        private static Stream _inputStream = null;

        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
            };

            var validateCommand = new Command("validate")
            {
                new Option<FileInfo>(
                    "--file",
                    "An option whose argument is parsed as a FileInfo"
                ).ExistingOnly()
            };
            validateCommand.Handler =
                CommandHandler.Create<FileInfo>(async (fileInfo) =>
                {
                    if (fileInfo != null)
                    {
                        _inputStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                    }

                    if (_inputStream == null)
                    {
                        throw new Exception(nameof(fileInfo));
                    }

                    using (StreamReader sr = new StreamReader(_inputStream))
                    {
                        Console.Out.Write(await sr.ReadToEndAsync());
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
            return await parser.InvokeAsync(args);
        }

        private static async Task<string> ReadStdInputAsync()
        {
            using (StreamReader sr = new StreamReader(Console.OpenStandardInput()))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
}
