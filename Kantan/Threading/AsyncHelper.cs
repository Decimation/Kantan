using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace Kantan.Threading;

public static class AsyncHelper
{
	//https://github.com/tejacques/AsyncBridge
	//https://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously


#if DISABLED

	/// <summary>
	/// Executes an async <see cref="Task{TResult}"/> method which has a void return value synchronously
	/// </summary>
	/// <param name="task"><see cref="Task{TResult}"/> method to execute</param>
	public static void RunSync(Func<Task> task)
	{
		var oldContext = SynchronizationContext.Current;
		var synch      = new ExclusiveSynchronizationContext();

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
	public static T RunSync<T>(Func<Task<T>> task)
	{
		var oldContext = SynchronizationContext.Current;
		var synch      = new ExclusiveSynchronizationContext();

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

	//https://github.com/aspnet/AspNetIdentity/blob/main/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
	private static readonly TaskFactory Factory = new(CancellationToken.None,
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
	}
}