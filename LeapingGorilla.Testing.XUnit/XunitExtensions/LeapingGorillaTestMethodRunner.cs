using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LeapingGorilla.Testing.XUnit.XunitExtensions
{
    public class LeapingGorillaTestMethodRunner : XunitTestMethodRunner
    {
        public LeapingGorillaTestMethodRunner(
            ITestMethod testMethod,
            IReflectionTypeInfo @class,
            IReflectionMethodInfo method,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource,
            object[] constructorArguments) : 
            base(
                testMethod, 
                @class, 
                method, 
                testCases, 
                diagnosticMessageSink, 
                messageBus,
                aggregator, 
                cancellationTokenSource, 
                constructorArguments)
        {
        }
    }
}