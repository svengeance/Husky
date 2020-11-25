using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Husky.Core.Workflow;

namespace Husky.Installer
{
    public class HuskyInstaller
    {
        private readonly HuskyWorkflow _workflow;

        public HuskyInstaller(HuskyWorkflow workflow)
        {
            _workflow = workflow;
        }

        public void Validate()
        {
            var validations = _workflow.Stages
                                       .SelectMany(s => s.Jobs
                                                         .SelectMany(s2 => s2.Steps.
                                                                              Select(s3 => new
                                                                              {
                                                                                  Validation = s3.HuskyTaskConfiguration.Validate(),
                                                                                  TaskName = s3.HuskyTaskConfiguration.GetType().Name,
                                                                                  StepName = s3.Name,
                                                                                  JobName = s2.Name,
                                                                                  StageName = s.Name
                                                                              })));

            var exceptionString = validations.Where(w => !w.Validation.IsValid)
                                             .Aggregate(new StringBuilder(), (sb, next) => sb.AppendLine($"{next.StageName}.{next.JobName}.{next.StepName}.{next.TaskName}")
                                                                                             .Append(next.Validation))
                                             .ToString();

            if (!string.IsNullOrEmpty(exceptionString))
                throw new ValidationException(exceptionString);
        }

        public async Task Install()
        {
            // Validate workflow
            // "For Each Stage"
            //   "For Each Job"
            //      "For Each Step"
            //        Execute
        }
    }
}