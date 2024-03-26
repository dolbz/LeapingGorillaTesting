using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LeapingGorilla.Testing.XUnit.XunitExtensions
{
    public class LeapingGorillaTestFramework : XunitTestFramework
    {
        public LeapingGorillaTestFramework(IMessageSink messageSink)
            : base(messageSink)
        { }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
            => new LeapingGorillaTestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
    }
}