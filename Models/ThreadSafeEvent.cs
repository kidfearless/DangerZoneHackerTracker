using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DangerZoneHackerTracker
{
	public class ThreadSafeEvent<T>
	{
		private struct SafeEvent
		{
			public T Delegate;
			public Thread Thread;
		}

		private Dictionary<T, SafeEvent> Events = new();
		/// <summary>
		/// Add the function delegate to the event callback list. delegates cannot be added more than once.
		/// </summary>
		/// <param name="func">function to call when the event is fired</param>
		public void AddEvent(T func)
		{
			var safe = new SafeEvent()
			{
				Delegate = func,
				Thread = Thread.CurrentThread
			};
			Events[func] = safe;
		}

		/// <summary>
		/// Tries to remove the given delegate from the event callback list.
		/// </summary>
		/// <param name="func">function to remove</param>
		public void RemoveEvent(T func)
		{
			if(Events.ContainsKey(func))
			{
				Events.Remove(func);
			}
		}

		/// <summary>
		/// Invokes the event on the thread it was added on.
		/// </summary>
		/// <param name="args">objects to pass into the function delegates</param>
		public void Invoke(params object[] args)
		{
			foreach (var ev in Events.Values.ToArray())
			{
				Dispatcher dispatcher = Dispatcher.FromThread(ev.Thread);
				try
				{

					dispatcher.Invoke(() =>
					{
						dynamic t = ev.Delegate;
						t.DynamicInvoke(args);
					});
				}
				catch(Exception e)
				{
					Debug.Fail(e.ToString());
				}
			}
		}
	}
}
