using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssemblyFixture;

[assembly: TestFramework(AssemblyFixtureFramework.TypeName, AssemblyFixtureFramework.AssemblyName)]
namespace AssemblyFixture.Tests
{
	public class Sample1 : IAssemblyFixture<MyAssemblyFixture>
	{
		MyAssemblyFixture fixture;

		// Fixtures are injectable into the test classes, just like with class and collection fixtures
		public Sample1(MyAssemblyFixture fixture)
		{
			this.fixture = fixture;
		}

		[Fact]
		public void EnsureSingleton()
		{
			Assert.Equal(1, MyAssemblyFixture.InstantiationCount);
		}
	}

	public class Sample2 : IAssemblyFixture<MyAssemblyFixture>
	{
		MyAssemblyFixture fixture;

		public Sample2(MyAssemblyFixture fixture)
		{
			this.fixture = fixture;
		}

		[Fact]
		public void EnsureSingleton()
		{
			Assert.Equal(1, MyAssemblyFixture.InstantiationCount);
		}
	}

	public class Sample3 : IAssemblyFixture<MyAssemblyFixtureWithMessageSink>
	{
		MyAssemblyFixtureWithMessageSink fixture;

		public Sample3(MyAssemblyFixtureWithMessageSink fixture)
		{
			this.fixture = fixture;
		}

		[Fact]
		public void EnsureThatHaveIMessageSink()
		{
			Assert.NotNull(fixture.MessageSink);
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
		private IMessageSink _messageSink;

		public IMessageSink MessageSink => _messageSink;

		public MyAssemblyFixtureWithMessageSink(IMessageSink messageSink)
		{
			_messageSink = messageSink;
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