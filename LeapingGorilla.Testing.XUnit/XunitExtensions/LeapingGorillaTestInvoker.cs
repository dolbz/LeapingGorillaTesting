using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LeapingGorilla.Testing.XUnit.XunitExtensions
{
    public class LeapingGorillaTestInvoker : XunitTestInvoker
    {
        public LeapingGorillaTestInvoker(ITest test,
            IMessageBus messageBus,
            Type testClass,
            object[] constructorArguments,
            MethodInfo testMethod,
            object[] testMethodArguments,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments,
                beforeAfterAttributes,
                aggregator,
                cancellationTokenSource)
        {
        }

        private static object CachedTestClass = null;
        
        protected override object CreateTestClass()
        {
            if (Test.TestCase is LeapingGorillaTestCase)
            {
                if (CachedTestClass is null)
                {
                    CachedTestClass = base.CreateTestClass();
                }

                return CachedTestClass;
            }

            return base.CreateTestClass();
        }
    }
}