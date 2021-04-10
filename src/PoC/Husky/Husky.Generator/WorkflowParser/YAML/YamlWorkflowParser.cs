using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

using YamlBlocks = Husky.Generator.GeneratorConstants.Workflow.YamlBlocks;

namespace Husky.Generator.WorkflowParser.YAML
{
    internal class YamlWorkflowParser: IWorkflowParser
    {
        public ParsedWorkflow ParseWorkflow(string yamlWorkflow)
        {
            var reader = new StringReader(yamlWorkflow);
            var stream = new YamlStream();
            stream.Load(reader);
            var doc = stream.Documents.First();

            if (doc.RootNode is not YamlMappingNode rootMappingNode)
                throw new ArgumentException("Unable to parse YamlWorkflow as root node was not a Mapping Node", nameof(yamlWorkflow));

            var parsedWorkflow = new ParsedWorkflow();

            foreach (var child in rootMappingNode.Where(w => (w.Key as YamlScalarNode)?.Value is not null))
            {
                var keyNode = (YamlScalarNode) child.Key;
                var valueNode = child.Value;
                GetNodeMapping(keyNode.Value, keyNode.Start.Column, valueNode.NodeType).Invoke(parsedWorkflow, keyNode.Value!, valueNode);
            }

            return parsedWorkflow;
        }

        private Action<ParsedWorkflow, string, YamlNode> GetNodeMapping(string? nodeKey, int nodeColumn, YamlNodeType nodeType)
            => nodeKey switch
               {
                   YamlBlocks.Dependencies                                        => MapDependency,
                   YamlBlocks.Variables                                           => MapGlobalVariables,
                   YamlBlocks.Stages                                              => MapStages,
                   YamlBlocks.Jobs when nodeColumn == YamlBlocks.RootColumnIndex  => MapDefaultStage,
                   YamlBlocks.Steps when nodeColumn == YamlBlocks.RootColumnIndex => MapDefaultStageAndJob,
                   _ when nodeType == YamlNodeType.Mapping                        => MapConfigurationBlock,
                   _ => throw new InvalidOperationException($"Unable to handle node {nodeKey}:{nodeType}")
               };

        private void MapConfigurationBlock(ParsedWorkflow workflow, string nodeKey, YamlNode configurationNode)
        {
            AssertNodeIs(configurationNode, out YamlMappingNode mappingNode);
            workflow.ConfigurationBlocks.Add(nodeKey, MapMappingNode(mappingNode));
        }

        private void MapDependency(ParsedWorkflow workflow, string nodeKey, YamlNode dependencyNode)
        {
            AssertNodeIs(dependencyNode, out YamlSequenceNode sequence);
            foreach (var dependency in sequence.Cast<YamlMappingNode>().SelectMany(s => s.Children))
            {
                var dependencyName = (dependency.Key as YamlScalarNode)?.Value ?? throw new InvalidOperationException($"Expected non-null key for dependency {dependencyNode}");
                workflow.Dependencies.Add(dependencyName, MapMappingNode((YamlMappingNode) dependency.Value));
            }
        }

        private void MapGlobalVariables(ParsedWorkflow workflow, string nodeKey, YamlNode variablesNode)
        {
            AssertNodeIs(variablesNode, out YamlMappingNode mappingNode);
            workflow.Variables = MapMappingNode(mappingNode);
        }

        private void MapStages(ParsedWorkflow workflow, string nodeKey, YamlNode stagesNode)
        {
            AssertNodeIs(stagesNode, out YamlMappingNode mappingNode);
            foreach (var stage in mappingNode)
            {
                AssertNodeIs(stage.Key, out YamlScalarNode scalarNode);
                AssertNodeIs(stage.Value, out YamlMappingNode jobsNode);
                AssertNodeIs(jobsNode.Children[0].Value, out YamlMappingNode nestedJobsNode);
                var parsedStage = new ParsedStage { Jobs = ParseJobs(nestedJobsNode) };
                workflow.Stages.Add(scalarNode.Value!, parsedStage);
            }
        }

        private void MapDefaultStage(ParsedWorkflow workflow, string nodeKey, YamlNode jobsNode)
        {
            AssertNodeIs(jobsNode, out YamlMappingNode jobMappingNode);
            var stage = new ParsedStage { Jobs = ParseJobs(jobMappingNode) };
            workflow.Stages.Add(GeneratorConstants.Workflow.DefaultStageName, stage);
        }

        private void MapDefaultStageAndJob(ParsedWorkflow workflow, string nodeKey, YamlNode stepsNode)
        {
            AssertNodeIs(stepsNode, out YamlMappingNode stepMappingNode);
            var job = new ParsedJob { Steps = ParseSteps(stepMappingNode) };
            var stage = new ParsedStage
            {
                Jobs = new()
                {
                    [GeneratorConstants.Workflow.DefaultJobName] = job
                }
            };

            workflow.Stages.Add(GeneratorConstants.Workflow.DefaultStageName, stage);
        }

        private Dictionary<string, ParsedJob> ParseJobs(YamlMappingNode jobsNode)
        {
            var parsedJobs = new Dictionary<string, ParsedJob>(jobsNode.Children.Count);
            foreach (var job in jobsNode.Children)
            {
                AssertNodeIs(job.Key, out YamlScalarNode scalarNode);
                AssertNodeIs(job.Value, out YamlMappingNode jobPropertiesMappingNode);
                ParsedJob parsedJob = new();
                
                foreach (var jobProperty in jobPropertiesMappingNode.Children)
                {
                    AssertNodeIs(jobProperty.Key, out YamlScalarNode jobPropertyNode);
                    if (jobPropertyNode.Value == YamlBlocks.JobProperties.Tags)
                    {
                        AssertNodeIs(jobProperty.Value, out YamlSequenceNode jobTags);
                        parsedJob.Tags = jobTags.Children.Select(s => ((YamlScalarNode) s).Value!).ToList();
                    } else if (jobPropertyNode.Value == YamlBlocks.JobProperties.Os)
                    {
                        AssertNodeIs(jobProperty.Value, out YamlScalarNode jobOs);
                        parsedJob.Os = jobOs.Value!;
                    } else if (jobPropertyNode.Value == YamlBlocks.Steps)
                    {
                        AssertNodeIs(jobProperty.Value, out YamlMappingNode stepsNode);
                        parsedJob.Steps = ParseSteps(stepsNode);
                        parsedJobs.Add(scalarNode.Value!, parsedJob);
                    }
                }
            }

            return parsedJobs;
        }

        private Dictionary<string, ParsedStep> ParseSteps(YamlMappingNode stepsNode)
        {
            var parsedSteps = new Dictionary<string, ParsedStep>();
            foreach (var step in stepsNode)
            {
                AssertNodeIs(step.Key, out YamlScalarNode scalarNode);
                AssertNodeIs(step.Value, out YamlMappingNode stepNode);
                parsedSteps.Add(scalarNode.Value!, ParseStep(stepNode));
            }

            return parsedSteps;
        }

        private ParsedStep ParseStep(YamlMappingNode stepNode)
        {
            var parsedStep = new ParsedStep();
            foreach (var property in stepNode.Children)
            {
                AssertNodeIs(property.Key, out YamlScalarNode scalarNode);
                var key = scalarNode.Value;

                switch (key)
                {
                    case YamlBlocks.StepProperties.Os:
                        AssertNodeIs(property.Value, out YamlScalarNode osNode);
                        parsedStep.Os = osNode.Value!;
                        break;
                    case YamlBlocks.StepProperties.Tags:
                        AssertNodeIs(property.Value, out YamlSequenceNode tagsNode);
                        parsedStep.Tags = tagsNode.Children.Select(s => ((YamlScalarNode) s).Value!).ToList();
                        break;
                    case YamlBlocks.StepProperties.Task:
                        AssertNodeIs(property.Value, out YamlScalarNode taskNode);
                        parsedStep.Task = taskNode.Value ?? throw new InvalidOperationException($"Expected non-null task for step {stepNode}");
                        break;
                    case YamlBlocks.StepProperties.With:
                        AssertNodeIs(property.Value, out YamlMappingNode withNode);
                        parsedStep.With = MapMappingNode(withNode);
                        break;
                }
            }

            return parsedStep;
        }

        private Dictionary<string, object?> MapMappingNode(YamlMappingNode node)
            => node.Children.ToDictionary(
                k => ((YamlScalarNode) k.Key).Value ?? throw new InvalidOperationException($"Expected non-null key for node {node}"),
                v => MapNodeAsScalarOrSequence(v.Value));

        private object? MapNodeAsScalarOrSequence(YamlNode node)
            => node switch
               {
                   YamlScalarNode scalar     => DeserializeScalarObject(scalar.Value),
                   YamlSequenceNode sequence => sequence.Children.Cast<YamlScalarNode>().Select(s => s.Value).ToArray(),
                   _                         => throw new InvalidOperationException($"Unable to map node as scalar or sequence: {node}")
               };

        private object? DeserializeScalarObject(string? s) =>
            s switch
            {
                _ when int.TryParse(s, out var i)    => i,
                _ when double.TryParse(s, out var d) => d,
                _ when bool.TryParse(s, out var b)   => b,
                _                                    => s?.Replace("\n", Environment.NewLine)
            };

        private void AssertNodeIs<T>(YamlNode node, out T outputNode) where T: class
            => outputNode = node as T ?? throw new InvalidOperationException($"Expected {node} to be of type {typeof(T).Name}");
    }
}