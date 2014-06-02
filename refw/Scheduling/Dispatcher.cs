using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace refw {
	/// <summary>
	/// This class models a basic cross-thread dispatcher as used in WinForms and WPF. One thread executes code through InvokeInvokes,
	/// while other threads queue up functions to be called in the main thread asynchronously with BeginInvoke or synchronously with Invoke.
	/// </summary>
	public class Dispatcher {
		private ConcurrentQueue<Tuple<Action, EventWaitHandle>> invokeQueue = new ConcurrentQueue<Tuple<Action, EventWaitHandle>>();
		private int? invokeThreadId = null;

		/// <summary>
		/// Call all pending delegates and invoke their listening thread of their completion afterwards.
		/// </summary>
		public void InvokeInvokes() {
			invokeThreadId = Thread.CurrentThread.ManagedThreadId;
			Tuple<Action, EventWaitHandle> action;
			while (invokeQueue.TryDequeue(out action)) {
				action.Item1.Invoke();
				action.Item2.Set();
			}
			invokeThreadId = null;
		}

		/// <summary>
		/// Queue up a delegate without waiting for it to finish.
		/// </summary>
		/// <param name="action">delegate to be called by the owning thread</param>
		/// <returns>a wait handle that can be used to check for completion</returns>
		public EventWaitHandle BeginInvoke(Action action) {
			var wait_handle = new AutoResetEvent(false);
			invokeQueue.Enqueue(new Tuple<Action, EventWaitHandle>(action, wait_handle));
			return wait_handle;
		}

		/// <summary>
		/// Queue up a delegate and return once it finishes
		/// </summary>
		/// <param name="action">delegate to be called by the owning thread</param>
		public void Invoke(Action action) {
			// If we're queuing a blocking invoke from inside an action, immediately invoke it to avoid a deadlock
			if (invokeThreadId.HasValue && invokeThreadId == Thread.CurrentThread.ManagedThreadId) {
				action.Invoke();
				return;
			}
			var handle = BeginInvoke(action);
			handle.WaitOne();
		}
	}
}
