using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using RopeSnake.Mother3;
using RopeSnake.Core;
using SharpFileSystem;
using System.IO;

namespace RopeSnake.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            string invokedVerb = null;
            object invokedVerbInstance = null;

            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options,
                (verb, subOptions) =>
                {
                    invokedVerb = verb;
                    invokedVerbInstance = subOptions;
                }))
            {
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            var validateOptions = invokedVerbInstance as ValidateOptions;
            if (validateOptions != null)
            {
                string fullPath = Path.GetFullPath(validateOptions.Project);
                string directory = Path.GetDirectoryName(fullPath);
                string fileName = Path.GetFileName(fullPath);

                var fileSystem = new PhysicalFileSystemWrapper(directory);
                var project = Mother3Project.Load(fileSystem, fileName.ToPath(), null, directory);

                project.Validate();

                var compileOptions = invokedVerbInstance as CompileOptions;
                if (compileOptions != null)
                    project.Compile(fileSystem, compileOptions.Cached, compileOptions.Threads);

                return;
            }

            var decompileOptions = invokedVerbInstance as DecompileOptions;
            string Out=decompileOptions.Output;
            if (decompileOptions != null)
            {
                var project = Mother3Project.CreateNew(decompileOptions.Rom, decompileOptions.Config, decompileOptions.Output);
                var fileSystem = new PhysicalFileSystemWrapper(decompileOptions.Output);

                project.Decompile(Out,fileSystem);
                project.WriteToFiles(fileSystem, Mother3Project.DefaultProjectFile);

                return;
            }
        }
    }

    class Options
    {
        [VerbOption("compile", HelpText = "Compile a project")]
        public CompileOptions Compile { get; set; }

        [VerbOption("decompile", HelpText = "Decompile a ROM")]
        public DecompileOptions Decompile { get; set; }

        [VerbOption("validate", HelpText = "Validate a project")]
        public ValidateOptions Validate { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }

    class CompileOptions : ValidateOptions
    {
        [Option('c', "cached", HelpText = "Use caching")]
        public bool Cached { get; set; }

        [Option('t', "threads", HelpText = "Max threads to use during compilation", DefaultValue = 1)]
        public int Threads { get; set; }
    }

    class DecompileOptions
    {
        [Option('r', "rom", HelpText = "ROM file to decompile", Required = true)]
        public string Rom { get; set; }

        [Option('c', "config", HelpText = "Config file to use for decompiling", Required = true)]
        public string Config { get; set; }

        [Option('o', "output", HelpText = "Output path", Required = true)]
        public string Output { get; set; }
    }

    class ValidateOptions
    {
        [Option('p', "project", HelpText = "Path to the project JSON file", Required = true)]
        public string Project { get; set; }
    }
}
