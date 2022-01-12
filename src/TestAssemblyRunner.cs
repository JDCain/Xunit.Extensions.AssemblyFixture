using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xunit.Abstractions;
using Xunit.Sdk;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xunit.Extensions.AssemblyFixture
{
    class TestAssemblyRunner : XunitTestAssemblyRunner
    {
#if NET5_0_OR_GREATER
        private readonly Dictionary<Type, object> _assemblyFixtureMappings = new();
#else
        private readonly Dictionary<Type, object> _assemblyFixtureMappings = new Dictionary<Type, object>();

#endif

        public TestAssemblyRunner(
            ITestAssembly testAssembly,
            IEnumerable<IXunitTestCase> testCases,
            IMessageSink diagnosticMessageSink,
            IMessageSink executionMessageSink,
            ITestFrameworkExecutionOptions executionOptions)
            : base(testAssembly, testCases, diagnosticMessageSink, executionMessageSink, executionOptions)
        { }

        ///<inheritdoc/>
        protected override async Task AfterTestAssemblyStartingAsync()
        {
            // Let everything initialize
            await base.AfterTestAssemblyStartingAsync().ConfigureAwait(false);

            // Go find all the AssemblyFixtureAttributes adorned on the test assembly
            await Aggregator.RunAsync(async () =>
            {
                ISet<Type> assemblyFixtures = new HashSet<Type>(((IReflectionAssemblyInfo)TestAssembly.Assembly).Assembly
                    .GetTypes()
                    .Select(type => type.GetInterfaces())
                    .SelectMany(x => x)
                    .Where(@interface => @interface.IsAssignableToGenericType(typeof(IAssemblyFixture<>)))
                    .ToArray());

                // Instantiate all the fixtures
                foreach (Type fixtureAttribute in assemblyFixtures)
                {
                    Type fixtureType = fixtureAttribute.GetGenericArguments()[0];
                    var hasConstructorWithMessageSink = fixtureType.GetConstructor(new[] { typeof(IMessageSink) }) != null;
                    _assemblyFixtureMappings[fixtureType] = hasConstructorWithMessageSink
                        ? Activator.CreateInstance(fixtureType, ExecutionMessageSink)
                        : Activator.CreateInstance(fixtureType);
                }

                // Initialize IAsyncLifetime fixtures
                foreach (IAsyncLifetime asyncLifetime in _assemblyFixtureMappings.Values.OfType<IAsyncLifetime>())
                {
                    await Aggregator.RunAsync(async () => await asyncLifetime.InitializeAsync().ConfigureAwait(false)).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        protected override async Task BeforeTestAssemblyFinishedAsync()
        {
            // Make sure we clean up everybody who is disposable, and use Aggregator.Run to isolate Dispose failures
            Parallel.ForEach(_assemblyFixtureMappings.Values.OfType<IDisposable>(),
                             disposable => Aggregator.Run(disposable.Dispose));


#if NETSTANDARD2_1 || NET5_0_OR_GREATER

#if NET6_0_OR_GREATER
               await Parallel.ForEachAsync(_assemblyFixtureMappings.Values.OfType<IAsyncDisposable>(),
                                  async (disposable, _) => await Aggregator.RunAsync(async () => await disposable.DisposeAsync().ConfigureAwait(false))
                                                                           .ConfigureAwait(false));
#else
            await Task.WhenAll(_assemblyFixtureMappings.Values.OfType<IAsyncDisposable>().Select(
                async asyncDisposable =>
                {
                    await Aggregator.RunAsync(async () => await asyncDisposable.DisposeAsync().ConfigureAwait(false));
                })
            );
    
#endif

#endif


#if NET6_0_OR_GREATER
               await Parallel.ForEachAsync(_assemblyFixtureMappings.Values.OfType<IAsyncLifetime>(),
                                  async (disposable, _) => await Aggregator.RunAsync(async () => await disposable.DisposeAsync().ConfigureAwait(false))
                                                                           .ConfigureAwait(false));
#else
            await Task.WhenAll(_assemblyFixtureMappings.Values.OfType<IAsyncLifetime>().Select(
                async asyncDisposable =>
                {
                    await Aggregator.RunAsync(async () => await asyncDisposable.DisposeAsync().ConfigureAwait(false));
                })
            );
            Parallel.ForEach(_assemblyFixtureMappings.Values.OfType<IAsyncLifetime>(),
                             async (disposable, _) => await Aggregator.RunAsync(async () => await disposable.DisposeAsync().ConfigureAwait(false)));
    
#endif

            await base.BeforeTestAssemblyFinishedAsync().ConfigureAwait(false);
        }

        protected override async Task<RunSummary> RunTestCollectionAsync(
            IMessageBus messageBus,
            ITestCollection testCollection,
            IEnumerable<IXunitTestCase> testCases,
            CancellationTokenSource cancellationTokenSource)
            => await new TestCollectionRunner(
                _assemblyFixtureMappings,
                testCollection,
                testCases,
                DiagnosticMessageSink,
                messageBus,
                TestCaseOrderer,
                new ExceptionAggregator(Aggregator),
                cancellationTokenSource)
            .RunAsync().ConfigureAwait(false);
    }
}