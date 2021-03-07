using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Husky.Generator.WorkflowParser;
using Husky.Generator.WorkflowParser.YAML;
using Microsoft.CodeAnalysis;

namespace Husky.Generator
{
    [Generator]
    internal class HelloWorldGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            return;

            var source = new StringBuilder();
            try
            {

                var workflowConfiguration = context.AdditionalFiles
                                                   .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Path) == GeneratorConstants.WorkflowConfigurationFileName);

                if (workflowConfiguration == null)
                {
                    source.Append($"// No configuration file detected. " +
                                  $"Please add a Husky Configuration File with the appropriate file extension (e.x. {GeneratorConstants.WorkflowConfigurationFileName}.yml)");

                    context.AddSource(GeneratorConstants.GeneratedWorkflowFileName, source.ToString());

                    return;
                }

                var parser = GetParser(Path.GetExtension(workflowConfiguration.Path));

                var huskyWorkflow = parser.ParseWorkflow(workflowConfiguration.GetText().ToString());
            }
            catch (Exception e)
            {
                Debugger.Launch();
                Debug.WriteLine("Exception!");
                source.Insert(0, "/*").AppendLine();
                source.AppendLine("*/");

                var exceptionString = new StringBuilder()
                                     .AppendLine("/* EXCEPTION CREATED")
                                     .Append(new YamlDotNet.Serialization.Serializer().Serialize(e))
                                     .AppendLine("*/");

                source.Insert(0, exceptionString.ToString());


                context.AddSource("EXCEPTION", source.ToString());

                Console.WriteLine("HEHOIWAUSEHROSADRBNOFSBDNOF");
                throw new ApplicationException(source.ToString());
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            Console.WriteLine("Hi");
        }

        // This isn't sexy, but consider that there aren't many parsers, it won't change much over time, and that this is running frequently as the user edits their files.
        // Is assembly scanning the right call?
        private IWorkflowParser<ParsedWorkflow> GetParser(string fileExtension)
            => fileExtension switch
               {
                   ".yml"  => new YamlWorkflowParser(),
                   ".yaml" => new YamlWorkflowParser(),
                   _ => throw new ArgumentOutOfRangeException(nameof(fileExtension), fileExtension, "Unsupported file extension")
               };
    }
}