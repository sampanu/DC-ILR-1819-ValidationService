﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR.ValidationService.Interface;
using ESFA.DC.ILR.ValidationService.Modules;
using ESFA.DC.ILR.ValidationService.Stubs;

namespace ESFA.DC.ILR.ValidationService.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var argsList = args.ToList();

            if (!argsList.Any())
            {
                argsList.Add(@"Files/ILR.xml");
            }

            RunValidation(argsList.First());
        }

        private static void RunValidation(string filePath)
        {
            var validationContext = new ValidationContextStub
            {
                Input = filePath,
                Output = filePath + ".vs.csv",
                ValidLearnRefNumbersKey = "ValidLearnRefNumbers",
                InvalidLearnRefNumbersKey = "InvalidLearnRefNumbers",
            };

            var container = BuildContainer();

            using (var scope = container.BeginLifetimeScope(c => RegisterContext(c, validationContext)))
            {
                var ruleSetOrchestrationService = scope.Resolve<IRuleSetOrchestrationService<ILearner, IValidationError>>();

                var errors = ruleSetOrchestrationService.Execute(validationContext);

                OutputResultsToFile(errors, $"{validationContext.Output}");
            }

            System.Console.WriteLine($"{validationContext.Output}");
        }

        private static void OutputResultsToFile(IEnumerable<IValidationError> errors, string path)
        {
            StringBuilder contents = new StringBuilder();
            contents.AppendLine(@"Error\Warning,Learner Ref,Rule Name,Field Values,Error Message,Aim Sequence Number,Aim Reference Number,Software Supplier Aim ID,Funding Model,Subcontracted UKPRN,Provider Specified Learner Monitoring A,Provider Specified Learner Monitoring B,Provider Specified Learning Delivery Monitoring A,Provider Specified Learning Delivery Monitoring B,Provider Specified Learning Delivery Monitoring C,Provider Specified Learning Delivery Monitoring D,OFFICIAL-SENSITIVE");
            foreach (var error in errors)
            {
                var errorFirst = string.Empty;
                if (error.ErrorMessageParameters != null)
                {
                    foreach (var s in error.ErrorMessageParameters)
                    {
                        errorFirst += s;
                    }
                }

                contents.AppendLine($"E,{error.LearnerReferenceNumber},{error.RuleName},{errorFirst},,{error?.AimSequenceNumber},,,,");
            }

            System.IO.File.WriteAllText(path, contents.ToString());
        }

        private static void RegisterContext(ContainerBuilder containerBuilder, IValidationContext validationContext)
        {
            containerBuilder.RegisterInstance(validationContext).As<IValidationContext>();
        }

        private static IContainer BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule<ValidationServiceConsoleModule>();

            return containerBuilder.Build();
        }
    }
}
