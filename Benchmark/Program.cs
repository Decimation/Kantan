using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark
{
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

	public static class Program
	{
		public static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchmarks1>();

		}
	}
}