using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LeapingGorilla.Testing.XUnit.XunitExtensions
{
    public class LeapingGorillaTestAssemblyRunner : XunitTestAssemblyRunner
    {
        readonly Dictionary<Type, object> assemblyFixtureMappings = new Dictionary<Type, object>();

        public LeapingGorillaTestAssemblyRunner(ITestAssembly testAssembly,
                                                          IEnumerable<IXunitTestCase> testCases,
                                                          IMessageSink diagnosticMessageSink,
                                                          IMessageSink executionMessageSink,
                                                          ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        { }

        protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
                                                                   ITestCollection testCollection,
                                                                   IEnumerable<IXunitTestCase> testCases,
                                                                   CancellationTokenSource cancellationTokenSource)
            => new LeapingGorillaTestCollectionRunner(assemblyFixtureMappings, testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();
    }
}