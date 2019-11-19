﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.AssemblyFixture
{
	class TestAssemblyRunner : XunitTestAssemblyRunner
	{
		readonly Dictionary<Type, object> assemblyFixtureMappings = new Dictionary<Type, object>();

		public TestAssemblyRunner(ITestAssembly testAssembly,
			IEnumerable<IXunitTestCase> testCases,
			IMessageSink diagnosticMessageSink,
			IMessageSink executionMessageSink,
			ITestFrameworkExecutionOptions executionOptions)
			: base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
		{
		}

		protected override Task BeforeTestAssemblyFinishedAsync()
		{
			// Make sure we clean up everybody who is disposable, and use Aggregator.Run to isolate Dispose failures
			foreach (var disposable in assemblyFixtureMappings.Values.OfType<IDisposable>())
				Aggregator.Run(disposable.Dispose);

			return base.BeforeTestAssemblyFinishedAsync();
		}


		protected override Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
																   ITestCollection testCollection,
																   IEnumerable<IXunitTestCase> testCases,
																   CancellationTokenSource cancellationTokenSource)
		{
			return new TestCollectionRunner(assemblyFixtureMappings, testCollection, testCases, DiagnosticMessageSink, messageBus, TestCaseOrderer, new ExceptionAggregator(Aggregator), cancellationTokenSource).RunAsync();
		}
	}
}
