using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class ParallelHelper
{
	/*
	 * https://devblogs.microsoft.com/pfxteam/implementing-parallel-while-with-parallel-foreach/
	 */

	public sealed class InfinitePartitioner : Partitioner<bool>
	{
		public override IList<IEnumerator<bool>> GetPartitions(int partitionCount)
		{
			if (partitionCount < 1) {
				throw new ArgumentOutOfRangeException(nameof(partitionCount));
			}

			return (from i in Enumerable.Range(0, partitionCount)
			        select InfiniteEnumerator()).ToArray();
		}

		public override bool SupportsDynamicPartitions => true;

		public override IEnumerable<bool> GetDynamicPartitions() => new InfiniteEnumerators();

		private static IEnumerator<bool> InfiniteEnumerator()
		{
			while (true) {
				yield return true;
			}
		}

		private class InfiniteEnumerators : IEnumerable<bool>
		{
			public IEnumerator<bool> GetEnumerator() => InfiniteEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
	}

	public static void While(ParallelOptions parallelOptions, Func<bool> condition,
	                         Action<ParallelLoopState> body)
	{
		Parallel.ForEach(new InfinitePartitioner(), parallelOptions,
		                 (ignored, loopState) =>
		                 {
			                 if (condition()) {
				                 body(loopState);
			                 }
			                 else {
				                 loopState.Stop();
			                 }
		                 });
	}
}