using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Husky.Generator.Extensions;
using Husky.Generator.WorkflowParser;

namespace Husky.Generator
{
    internal class ParsedWorkflowWriter
    {
        private readonly ParsedWorkflow _parsedWorkflow;
        private readonly SourceBuilder _sb = new();

        public override string ToString() => _sb.ToString();

        private ParsedWorkflowWriter(ParsedWorkflow parsedWorkflow)
        {
            _parsedWorkflow = parsedWorkflow;

            WriteHuskyWorkflow();
        }

        public static string Write(ParsedWorkflow workflow) => new ParsedWorkflowWriter(workflow).ToString();

        private void WriteHuskyWorkflow()
        {
            using (_sb.Block("namespace Husky.Generated"))
            using (_sb.Block("public static class Workflow"))
            using (_sb.Block("public static global::Husky.Core.Workflow.HuskyWorkflow Create()"))
            {
                _sb.Line("return global::Husky.Core.Workflow.HuskyWorkflow.Create()");

                WriteConfigurationBlocks();
                WriteDependencies();
                WriteGlobalVariables();
                WriteStages();

                _sb.Line(".Build();");
            }
        }

        private void WriteConfigurationBlocks()
        {
            var iterations = 0;
            foreach (var configurationBlock in _parsedWorkflow.ConfigurationBlocks)
            {
                if (iterations++ > 0)
                    _sb.Line();

                using (_sb.Text($".Configure<global::Husky.Core.HuskyConfiguration.{configurationBlock.Key.CapitalizeFirstLetter()}Configuration>").InlineParens())
                    WriteLambda(configurationBlock.Key, configurationBlock.Value, configurationBlock.Key);
            }

            if (iterations > 0)
                _sb.Line();
        }

        private void WriteDependencies()
        {
            foreach (var dependency in _parsedWorkflow.Dependencies)
            {
                var iterations = 0;
                _sb.Text($".AddDependency(new global::Husky.Core.Dependencies.{dependency.Key}");
                using (_sb.InlineParens())
                    foreach (var dependencyProperty in dependency.Value)
                        _sb.InlineText(iterations++ > 0 ? ", " : "")
                           .InlineText($"{dependencyProperty.Key}: ")
                           .InlineText(WriteObjectValue(dependencyProperty.Value, $"Husky.Core.Dependencies.{dependency.Key}.{dependencyProperty.Key}"));

                _sb.InlineText(")");
                _sb.Line();
            }
        }

        private void WriteGlobalVariables()
        {
            foreach (var variable in _parsedWorkflow.Variables)
                _sb.Line($".AddGlobalVariable(\"{variable.Key}\", {WriteObjectValue(variable.Value)})");
        }

        private void WriteStages()
        {
            foreach (var stage in _parsedWorkflow.Stages)
            {
                if (stage.Key == GeneratorConstants.Workflow.DefaultStageName)
                    _sb.Text(".WithDefaultStage");
                else
                    _sb.Text(".AddStage");

                _sb.IndentUp();
                
                using (_sb.InlineParensIndentedEnd())
                {
                    _sb.Line();
                    
                    if (stage.Key != GeneratorConstants.Workflow.DefaultStageName)
                        _sb.Line($"\"{stage.Key}\",");

                    const string stageVarName = "stage";
                    _sb.Text($"{stageVarName} => {stageVarName}");
                    _sb.Line();
                    WriteJobs(stage.Value.Jobs);
                }

                _sb.IndentDown();
                _sb.Line();
            }
        }

        private void WriteJobs(Dictionary<string, ParsedJob> jobs)
        {
            foreach (var job in jobs)
            {
                if (job.Key == GeneratorConstants.Workflow.DefaultJobName)
                    _sb.Text($".WithDefaultJob");
                else
                    _sb.Text($".AddJob");

                _sb.IndentUp();

                using (_sb.InlineParensIndentedEnd())
                {
                    _sb.Line();

                    if (job.Key != GeneratorConstants.Workflow.DefaultJobName)
                        _sb.Line($"\"{job.Key}\",");

                    const string jobVarName = "job";
                    _sb.Text($"{jobVarName} => {jobVarName}");

                    if (!string.IsNullOrWhiteSpace(job.Value.Os) || job.Value.Tags.Count > 0)
                    {
                        var os = string.IsNullOrWhiteSpace(job.Value.Os)
                            ? "global::Husky.Core.Platform.CurrentPlatform.Os"
                            : "global::Husky.Core.Enums.OS." + job.Value.Os;

                        _sb.Line();
                        _sb.Text($".SetDefaultStepConfiguration(new({os}");

                        if (job.Value.Tags.Count > 0)
                            _sb.InlineText($", {string.Join(",", job.Value.Tags.Select(s => WriteObjectValue(s)))}");

                        _sb.InlineText("))");
                    }

                    _sb.Line();
                    WriteSteps(job.Value.Steps);
                }

                _sb.IndentDown();
                _sb.Line();
            }
        }

        private void WriteSteps(Dictionary<string, ParsedStep> steps)
        {
            foreach (var step in steps)
            {
                using (_sb.Text($".AddStep<global::Husky.Core.TaskOptions.{step.Value.Task}Options>").InlineParens())
                {
                    _sb.IndentUp();
                    _sb.Line();
                    _sb.Line($"\"{step.Key}\",");

                    const string taskVarName = "task";
                    _sb.Text("");
                    WriteLambda(taskVarName, step.Value.With, step.Value.Task);

                    if (!string.IsNullOrWhiteSpace(step.Value.Os) || step.Value.Tags.Count > 0)
                    {
                        var os = string.IsNullOrWhiteSpace(step.Value.Os)
                            ? "global::Husky.Core.Platform.CurrentPlatform.Os"
                            : "global::Husky.Core.Enums.OS." + step.Value.Os;

                        _sb.Line();
                        _sb.Text($",new({os}");

                        if (step.Value.Tags.Count > 0)
                            _sb.InlineText($", {string.Join(",", step.Value.Tags.Select(s => WriteObjectValue(s)))}");

                        _sb.InlineText(")");
                    }
                }

                _sb.IndentDown();
                _sb.Line();
            }
        }

        private void WriteLambda(string varName, Dictionary<string, object?> properties, string pathPrefix)
        {
            _sb.InlineText($"{varName} =>");
            _sb.Line();
            _sb.BlockOpen();
            _sb.DelimitedLines(";", properties.Select(s => $"{varName}.{s.Key.CapitalizeFirstLetter()} = {WriteObjectValue(s.Value, $"{pathPrefix}.{s.Key}")}"));
            _sb.BlockClose(appendLine: false);
        }

        private static string WriteObjectValue(object? property, string path = "")
            => property switch
               {
                   _ when TryHandleSpecialCase(property, path, out var parsed) => parsed!,
                   string s when path.EndsWith("Kind") => $"{path.Substring(0, path.Length - 4)}.{s}", // We make the bold assumption an enum property ends with Kind
                   string s   => "@\"" + s.Replace("\"", "\"\"") + "\"",
                   int i      => i.ToString(),
                   double d   => d.ToString(CultureInfo.InvariantCulture),
                   string[] a => $"new[] {{{string.Join(",", a.Select(s => $" \"{s}\""))} }}",
                   bool b     => b.ToString().ToLowerInvariant(),
                   null       => "null",
                   _          => throw new InvalidOperationException("Unable to serialize type " + property.GetType().Name)
               };

        private static bool TryHandleSpecialCase(object? property, string path, out string? parsedValue)
            => (parsedValue = 
                path switch
                {
                    _ when property is null => "null",
                    "application.version"                                 => WriteObjectValue(property.ToString()), // This is always a string that "may" look like a number e.x. 0.1
                    "clientMachineRequirements.supportedOperatingSystems" =>$"new[] {{ {string.Join(", ", ((string[]) property).Select(s => $"global::Husky.Core.Enums.OS.{s}"))} }}", // these are enum values, not strings
                    _                                                     => null
                }) != null;
    }
}