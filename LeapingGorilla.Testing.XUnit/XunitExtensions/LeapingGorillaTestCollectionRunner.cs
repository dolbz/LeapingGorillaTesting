using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LeapingGorilla.Testing.XUnit.XunitExtensions
{
    public class LeapingGorillaTestCollectionRunner : XunitTestCollectionRunner
    {
        readonly Dictionary<Type, object> assemblyFixtureMappings;
        readonly IMessageSink diagnosticMessageSink;

        public LeapingGorillaTestCollectionRunner(Dictionary<Type, object> assemblyFixtureMappings,
            ITestCollection testCollection,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            ITestCaseOrderer testCaseOrderer,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(testCollection, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
        {
            this.assemblyFixtureMappings = assemblyFixtureMappings;
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        protected override Task<RunSummary> RunTestClassAsync(ITestClass testClass, IReflectionTypeInfo @class, IEnumerable<IXunitTestCase> testCases)
        {
            return new LeapingGorillaTestClassRunner(testClass, @class, testCases, diagnosticMessageSink, MessageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), CancellationTokenSource, CollectionFixtureMappings).RunAsync();
        }
    }
}