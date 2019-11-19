using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.AssemblyFixture
{
    public class AssemblyFixtureFramework : XunitTestFramework
	{
		public AssemblyFixtureFramework(IMessageSink messageSink)
			: base(messageSink)
		{ }

        public const string TypeName = "Xunit.Extensions.AssemblyFixture.AssemblyFixtureFramework";
        public const string AssemblyName = "Xunit.Extensions.AssemblyFixture";

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
		{
			return new TestFrameworkExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
		}
	}
}
