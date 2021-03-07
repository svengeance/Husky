using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Husky.Generator.WorkflowParser.YAML
{
    internal class YamlWorkflowParser: IWorkflowParser<YamlWorkflow>
    {
        public YamlWorkflow ParseWorkflow(string yamlWorkflow)
        {
            var deserializer = new DeserializerBuilder()
                              .WithNamingConvention(CamelCaseNamingConvention.Instance)
                              .Build();

            var workflow = deserializer.Deserialize<YamlWorkflow>(yamlWorkflow);

            return workflow;
        }
    }
}