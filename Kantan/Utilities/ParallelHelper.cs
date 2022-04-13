using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kantan.Utilities;

public static class ParallelHelper
{
	/*
	 * https://devblogs.microsoft.com/pfxteam/implementing-parallel-while-with-parallel-foreach/
	 */
	public static async Task ForeachAsync<T>(IEnumerable<T> source, int maxParallelCount, Func<T, Task> action)
	{
		using (SemaphoreSlim completeSemphoreSlim = new SemaphoreSlim(1))
		using (SemaphoreSlim taskCountLimitsemaphoreSlim = new SemaphoreSlim(maxParallelCount)) {
			await completeSemphoreSlim.WaitAsync();
			int runningtaskCount = source.Count();

			foreach (var item in source) {
				await taskCountLimitsemaphoreSlim.WaitAsync();

				Task.Run(async () =>
				{
					try {
						await action(item).ContinueWith(task =>
						{
							Interlocked.Decrement(ref runningtaskCount);

							if (runningtaskCount == 0) {
								completeSemphoreSlim.Release();
							}
						});
					}
					finally {
						taskCountLimitsemaphoreSlim.Release();
					}
				}).GetHashCode();
			}

			await completeSemphoreSlim.WaitAsync();
		}
	}

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

	public static ParallelLoopResult While(ParallelOptions parallelOptions, Func<bool> condition,
	                                       Action<ParallelLoopState> body)
	{
		return Parallel.ForEach(new InfinitePartitioner(), parallelOptions, (ignored, loopState) =>
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