using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Kantan.Collections;
using Kantan.Numeric;
using Kantan.Utilities;

namespace Benchmark;

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

public static class Program
{
	public static void Main(string[] args)
	{
		BenchmarkRunner.Run<Benchmarks4>();

	}
}