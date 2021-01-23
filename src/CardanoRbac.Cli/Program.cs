using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CardanoRbac.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<FileInfo>(
                    "--policy-file",
                    "An option whose argument is parsed as a FileInfo")
            };

            rootCommand.Handler = CommandHandler.Create(
                async (ParseResult parseResult, IConsole console) =>
                {
                    if (console.IsInputRedirected)
                    {
                        string stdin = await ReadStdInputAsync();
                        console.Out.Write($"{stdin}");
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(new FileStream(parseResult.ValueForOption<FileInfo>("--policy-file").FullName, FileMode.Open, FileAccess.Read)))
                        {
                            string fileIn = await sr.ReadToEndAsync();
                            console.Out.Write($"{fileIn}");
                        }
                    }
                });

            // Parse the incoming args and invoke the handler
            return await rootCommand.InvokeAsync(args);
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
