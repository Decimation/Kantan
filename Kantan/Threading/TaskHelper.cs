using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kantan.Monad;

// ReSharper disable UnusedMember.Global

namespace Kantan.Threading;

/*
 * https://github.com/UnoSD/ResultMonad/blob/master/Result/TaskExtensions.cs
 * https://github.com/UnoSD/ResultMonad/blob/master/Result/TaskResultExtensions.cs
 */
public static class TaskHelper
{
	//https://github.com/tejacques/AsyncBridge
	//https://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
	//https://github.com/Microsoft/referencesource/blob/master/Microsoft.Bcl.Async/Microsoft.Threading.Tasks/Threading/Tasks/TaskEx.cs

#if DISABLED
	/// <summary>
	/// Executes an async <see cref="Task{TResult}"/> method which has a void return value synchronously
	/// </summary>
	/// <param name="task"><see cref="Task{TResult}"/> method to execute</param>
	public static void RunSync2(Func<Task> task)
	{
		var oldContext = SynchronizationContext.Current;
		var synch = new ExclusiveSynchronizationContext();

		SynchronizationContext.SetSynchronizationContext(synch);

		synch.Post(async _ =>
		{
			try {
				await task();
			}
			catch (Exception e) {
				synch.InnerException = e;
				throw;
			}
			finally {
				synch.EndMessageLoop();
			}
		}, null);

		synch.BeginMessageLoop();

		SynchronizationContext.SetSynchronizationContext(oldContext);
	}

	/// <summary>
	/// Executes an async <see cref="Task{TResult}"/> method which has a T return type synchronously
	/// </summary>
	/// <typeparam name="T">Return Type</typeparam>
	/// <param name="task"><see cref="Task{TResult}"/> method to execute</param>
	/// <returns></returns>
	public static T RunSync2<T>(Func<Task<T>> task)
	{
		var oldContext = SynchronizationContext.Current;
		var synch = new ExclusiveSynchronizationContext();

		SynchronizationContext.SetSynchronizationContext(synch);

		T ret = default(T);

		synch.Post(async _ =>
		{
			try {
				ret = await task();
			}
			catch (Exception e) {
				synch.InnerException = e;
				throw;
			}
			finally {
				synch.EndMessageLoop();
			}
		}, null);

		synch.BeginMessageLoop();
		SynchronizationContext.SetSynchronizationContext(oldContext);
		return ret;
	}

	private class ExclusiveSynchronizationContext : SynchronizationContext
	{
		private bool m_done;

		public Exception InnerException { get; set; }

		private readonly AutoResetEvent m_workItemsWaiting = new(false);

		private readonly Queue<Tuple<SendOrPostCallback, object>> m_items = new();

		public override void Send(SendOrPostCallback d, object state)
		{
			throw new NotSupportedException("We cannot send to our same thread");
		}

		public override void Post(SendOrPostCallback d, object state)
		{
			lock (m_items) {
				m_items.Enqueue(Tuple.Create(d, state));
			}

			m_workItemsWaiting.Set();
		}

		public void EndMessageLoop()
		{
			Post(_ => m_done = true, null);
		}

		public void BeginMessageLoop()
		{
			while (!m_done) {
				Tuple<SendOrPostCallback, object> task = null;

				lock (m_items) {
					if (m_items.Count > 0) {
						task = m_items.Dequeue();
					}
				}

				if (task != null) {
					task.Item1(task.Item2);

					if (InnerException != null) // the method threw an exception
					{
						throw new AggregateException($"{nameof(AsyncHelper)} method threw an exception.",
						                             InnerException);
					}
				}
				else {
					m_workItemsWaiting.WaitOne();
				}
			}
		}

		public override SynchronizationContext CreateCopy()
		{
			return this;
		}
	}

#endif
	/*
	 * https://stackoverflow.com/questions/35645899/awaiting-task-with-timeout
	 * https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.run?view=net-5.0
	 * https://devblogs.microsoft.com/pfxteam/crafting-a-task-timeoutafter-method/
	 * https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/cancel-async-tasks-after-a-period-of-time
	 * https://stackoverflow.com/questions/20638952/cancellationtoken-and-cancellationtokensource-how-to-use-it
	 * https://stackoverflow.com/questions/22637642/using-cancellationtoken-for-timeout-in-task-run-does-not-work
	 * https://stackoverflow.com/questions/32520612/how-to-handle-task-cancellation-in-the-tpl
	 */

	//https://github.com/aspnet/AspNetIdentity/blob/main/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs

	/*private static readonly TaskFactory Factory = new(CancellationToken.None,
	                                                  TaskCreationOptions.None,
	                                                  TaskContinuationOptions.None,
	                                                  TaskScheduler.Default);

	public static TResult RunSync<TResult>(Func<Task<TResult>> func)
	{
		var cultureUi = CultureInfo.CurrentUICulture;
		var culture   = CultureInfo.CurrentCulture;

		return Factory.StartNew(() =>
		{
			Thread.CurrentThread.CurrentCulture   = culture;
			Thread.CurrentThread.CurrentUICulture = cultureUi;
			return func();
		}).Unwrap().GetAwaiter().GetResult();
	}

	public static void RunSync(Func<Task> func)
	{
		var cultureUi = CultureInfo.CurrentUICulture;
		var culture   = CultureInfo.CurrentCulture;

		Factory.StartNew(() =>
		{
			Thread.CurrentThread.CurrentCulture   = culture;
			Thread.CurrentThread.CurrentUICulture = cultureUi;
			return func();
		}).Unwrap().GetAwaiter().GetResult();
	}*/

	public static Task<Task<T>>[] Interleaved<T>(IEnumerable<Task<T>> tasks)
	{
		var inputTasks = tasks.ToList();

		var buckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];
		var results = new Task<Task<T>>[buckets.Length];

		for (int i = 0; i < buckets.Length; i++) {
			buckets[i] = new TaskCompletionSource<Task<T>>();
			results[i] = buckets[i].Task;
		}

		int nextTaskIndex = -1;

		Action<Task<T>> continuation = completed =>
		{
			var bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
			bucket.TrySetResult(completed);
		};

		foreach (var inputTask in inputTasks)
			inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously,
			                       TaskScheduler.Default);

		return results;
	}

	public static TResult RunSync<TResult>(this Task<TResult> func)
	{
		return func.GetAwaiter().GetResult();
	}

	public static void RunSync(this Task func)
	{
		func.GetAwaiter().GetResult();
	}

	public static TResult RunSync<TResult>(Func<Task<TResult>> func)
	{
		return Task.Factory.StartNew(func).Unwrap().RunSync();
	}

	public static void RunSync(Func<Task> func)
	{
		Task.Factory.StartNew(func).Unwrap().RunSync();
	}

	/*public static TResult RunSync<TResult>(this Task<TResult> t) => RunSync(() => t);

	public static void RunSync(this Task t) => RunSync(() => t);*/

	public static Task ForEachAsync<T>(this IEnumerable<T> sequence, Func<T, Task> action)
	{
		return Task.WhenAll(sequence.Select(action));
	}

	public static async Task AwaitWithTimeout(this Task task, int timeout, Action success, Action error)
	{
		if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
			success();
		else
			error();
	}

	public static Task<T> ToTask<T>(this T source)
		=> Task.FromResult(source);

	public static Task<TResult> Select<T, TResult>
	(
		this Task<T> source,
		Func<T, TResult> func
	)
		=> source.Map(func);

	public static async Task<TResult> Map<T, TResult>
	(
		this Task<T> source,
		Func<T, TResult> func
	)
	{
		var result =
			await source.ConfigureAwait(false);

		return func(result);
	}

	public static Task<TResult> SelectMany<T, TResult>
	(
		this Task<T> source,
		Func<T, Task<TResult>> func
	)
		=> source.Bind(func);

	public static Task<TOutput> SelectMany<T, TResult, TOutput>
	(
		this Task<T> source,
		Func<T, Task<TResult>> func,
		Func<T, TResult, TOutput> projection
	)
		=> source.Bind(func)
			.Map(result => projection(source.Result, result));

	public static Task<TResult> Bind<T, TResult>
	(
		this Task<T> source,
		Func<T, Task<TResult>> func
	)
		=> source.Map(func)
			.Unwrap();

	public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
		=> Task.WhenAll(tasks);

	public static Task WhenAll(this IEnumerable<Task> tasks)
		=> Task.WhenAll(tasks);

	public static async Task WhenAllSequential(this IEnumerable<Task> tasks)
	{
		foreach (var task in tasks)
			await task.ConfigureAwait(false);
	}

	// TODO: Refactor list out of this
	public static async Task<IReadOnlyCollection<T>> WhenAllSequential<T>
		(this IEnumerable<Task<T>> tasks)
	{
		var results =
			new List<T>();

		foreach (var task in tasks)
			results.Add(await task.ConfigureAwait(false));

		return results;
	}

	public static Task<IResult<TResult>> SelectMany<T, TResult>
	(
		this Task<T> source,
		Func<T, IResult<TResult>> func
	)
		=> source.Map(func);

	public static Task<IResult<TOutput>> SelectMany<T, TResult, TOutput>
	(
		this Task<T> source,
		Func<T, IResult<TResult>> func,
		Func<T, TResult, TOutput> project
	)
		=> source.Map(tres => func(tres).Map(fres => project(tres, fres)));

	public static Task<IResult<TResult>> Select<T, TResult>
	(
		this IResult<T> source,
		Func<T, Task<TResult>> func
	)
		=> source.MapAsync(func);

	public static Task<IResult<TResult>> MapAsync<T, TResult>
	(
		this IResult<T> source,
		Func<T, Task<TResult>> func
	)
		=> source.Match(result => func(result).Map(o => o.ToResult()),
		                error => error.ToFailureResult<TResult>()
			                .ToTask());
}