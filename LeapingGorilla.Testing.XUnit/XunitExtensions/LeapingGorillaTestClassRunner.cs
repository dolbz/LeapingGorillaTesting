using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LeapingGorilla.Testing.Core;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LeapingGorilla.Testing.XUnit.XunitExtensions
{
    public class LeapingGorillaTestClassRunner : XunitTestClassRunner
    {
        public LeapingGorillaTestClassRunner(
            ITestClass testClass,
            IReflectionTypeInfo @class,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ITestCaseOrderer testCaseOrderer,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            IDictionary<Type, object> collectionFixtureMappings) :
            base(
                testClass,
                @class,
                testCases,
                diagnosticMessageSink,
                messageBus,
                testCaseOrderer,
                new ExceptionAggregator(aggregator),
                cancellationTokenSource,
                collectionFixtureMappings)
        {
        }

        protected override Task<RunSummary> RunTestMethodAsync(ITestMethod testMethod, IReflectionMethodInfo method, IEnumerable<IXunitTestCase> testCases,
            object[] constructorArguments)
        {
            if (Class.Type.IsSubclassOf(typeof(WhenTestingTheBehaviourOfBase)))
            {
                return new LeapingGorillaTestMethodRunner(testMethod, this.Class, method, testCases, this.DiagnosticMessageSink, this.MessageBus, new ExceptionAggregator(this.Aggregator), this.CancellationTokenSource, constructorArguments).RunAsync();
            }

            return base.RunTestMethodAsync(testMethod, method, testCases, constructorArguments);
        }
    }
}