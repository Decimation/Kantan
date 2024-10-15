using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Kantan.Collections;
using Kantan.Net;
using Kantan.Net.Web;
using Kantan.Numeric;
using Kantan.Threading;
using Kantan.Utilities;
using Array = System.Array;

namespace Benchmark;

public class Benchmarks7
{

	[IterationSetup]
	public async Task Setup()
	{
		f          = new FirefoxCookieReader();
		m_consumer = new Consumer();
		await f.OpenAsync();
	}

	[IterationCleanup]
	public void Cleanup()
	{
		f.Dispose();

	}

	private FirefoxCookieReader f;
	private Consumer            m_consumer;

	[Benchmark]
	public async Task Read()
	{
		(await f.ReadCookiesAsync()).Consume(m_consumer);
	}

}

public class Benchmarks5
{

	[Benchmark]
	public MyEnum Test1()
	{
		return EnumHelper.Or(MyEnum.a, MyEnum.b);
	}

	[Flags]
	public enum MyEnum
	{

		a = 1,
		b = 2,
		c = 3,

	}

}

public class Benchmarks1
{

	[Benchmark]
	public int Test2()
	{
		return new Func<int, int, int>(static (a, b) => a + b)(A, B);
	}

	[Params(100, 200)]
	public int A { get; set; }

	[Params(10, 20)]
	public int B { get; set; }

	[Benchmark]
	public int Test()
	{
		return new Func<int, int, int>((a, b) => a + b)(A, B);
	}

}

public class Benchmarks4
{

	private Action m_action;

	[GlobalSetup]
	public void Setup()
	{
		m_action = static () =>
		{
			Thread.Sleep(TimeSpan.FromSeconds(1));
		};
	}

	[Benchmark]
	public void RunSync2()
	{
		Task.Run(m_action).RunSync();
	}

	[Benchmark]
	public void RunSync()
	{
		TaskHelper.RunSync(() => Task.Run(m_action));
	}

	[Benchmark]
	public void Wait()
	{

		Task task = Task.Run(m_action);
		task.Wait();
	}

}

public class Benchmarks2
{

	[Benchmark]
	public IList<int> Test()
	{
		var rg      = new List<int> { 1, 2, 3, 4, 5, 6, 3, 4, 5 };
		var search  = new List<int> { 3, 4, 5 };
		var replace = new List<int> { 5, 4, 3 };

		return rg.ReplaceAllSequences(search, replace);

	}

}

public class Benchmarks3
{

	[Benchmark]
	public BigInteger gcd2()
	{
		return MathHelper.GCD((long) (BigInteger) 123, (long) (BigInteger) 456);
	}

	[Benchmark]
	public long gcd1()
	{
		return MathHelper.GCD(123, 456);
	}

}

public class Benchmarks6
{

	/*
| Method |     Mean |     Error |    StdDev |   Median |
|------- |---------:|----------:|----------:|---------:|
|      a | 987.6 ms | 210.53 ms | 617.46 ms | 425.1 ms |
|      b | 137.7 ms |   1.21 ms |   0.95 ms | 137.2 ms |
	 */

	[GlobalSetup]
	public void GlobalSetup() { }

	/*
	[Benchmark]
	public void a()
	{
		MediaSniffer.Scan(@"https://static.zerochan.net/Atago.%28Azur.Lane%29.full.2750747.png",
		                  HttpMediaResourceFilter.Default);

	}*/

}

public static class Program
{

	public static void Main(string[] args)
	{
		var cfg = DefaultConfig.Instance

			// .AddExporter(new HtmlExporter())
			.AddDiagnoser(MemoryDiagnoser.Default)
			.AddJob(Job.Default.WithRuntime(CoreRuntime.Core80).WithToolchain(InProcessNoEmitToolchain.Instance));

		BenchmarkRunner.Run<Benchmarks7>(cfg);

	}

}