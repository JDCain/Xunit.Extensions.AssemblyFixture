using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssemblyFixture;

[assembly: TestFramework(AssemblyFixtureFramework.TypeName, AssemblyFixtureFramework.AssemblyName)]
namespace AssemblyFixture.Tests
{
	public class Sample1 : IAssemblyFixture<MyAssemblyFixture>
	{
        private readonly MyAssemblyFixture _fixture;

		// Fixtures are injectable into the test classes, just like with class and collection fixtures
		public Sample1(MyAssemblyFixture fixture)
		{
			_fixture = fixture;
		}

		[Fact]
		public void EnsureSingleton()
		{
			Assert.Equal(1, MyAssemblyFixture.InstantiationCount);
		}
	}

	public class Sample2 : IAssemblyFixture<MyAssemblyFixture>
	{
        private readonly MyAssemblyFixture _fixture;

		public Sample2(MyAssemblyFixture fixture)
		{
			_fixture = fixture;
		}

		[Fact]
		public void EnsureSingleton()
		{
			Assert.Equal(1, MyAssemblyFixture.InstantiationCount);
		}
	}

	public class Sample3 : IAssemblyFixture<MyAssemblyFixtureWithMessageSink>
	{
        private readonly MyAssemblyFixtureWithMessageSink _fixture;

		public Sample3(MyAssemblyFixtureWithMessageSink fixture)
		{
			_fixture = fixture;
		}

		[Fact]
		public void EnsureThatHaveIMessageSink()
		{
			Assert.NotNull(_fixture.MessageSink);
		}
	}

	public class MyAssemblyFixture : IDisposable
	{
		public static int InstantiationCount;

		public MyAssemblyFixture()
		{
			InstantiationCount++;
		}

		public void Dispose()
		{
			// Uncomment this and it will surface as an assembly cleanup failure
			//throw new DivideByZeroException();
			//InstantiationCount = 0;
		}
	}

	public class MyAssemblyFixtureWithMessageSink : IDisposable
	{
		public static int InstantiationCount;

        public IMessageSink MessageSink { get; }

        public MyAssemblyFixtureWithMessageSink(IMessageSink messageSink)
		{
			MessageSink = messageSink;
			InstantiationCount++;
		}

		public void Dispose()
		{
			// Uncomment this and it will surface as an assembly cleanup failure
			//throw new DivideByZeroException();
			//InstantiationCount = 0;
		}
	}
}