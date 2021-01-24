using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace CardanoRbac.Cli
{
    class Program
    {
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
                string fileContents = file == null ? "null" : await ReadFileAsync(file);
                Console.WriteLine($"The value for --file is:\n{fileContents}");
            });
            rootCommand.Add(validateCommand);

            // Parse the incoming args and invoke the handler
            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<string> ReadFileAsync(FileInfo file)
        {
            using (StreamReader sr = new StreamReader(File.OpenRead(file.FullName)))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
}
