using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Kantan.Collections;

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

	public static class Program
	{
		public static void Main(string[] args)
		{
			BenchmarkRunner.Run<Benchmarks2>();

		}
	}
}