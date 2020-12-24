using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker
{
	public class ObservableSortedSet<T> : SortedSet<T>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
		}

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
			});
		}


		public new bool Add(T item)
		{
			var result = base.Add(item);
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
			return result;
		}

		public new void ExceptWith(IEnumerable<T> other)
		{
			base.ExceptWith(other);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public new void IntersectWith(IEnumerable<T> other)
		{
			base.IntersectWith(other);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public new void SymmetricExceptWith(IEnumerable<T> other)
		{
			base.SymmetricExceptWith(other);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public new void UnionWith(IEnumerable<T> other)
		{
			base.UnionWith(other);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public new void Clear()
		{
			base.Clear();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public new bool Remove(T item)
		{
			var result = base.Remove(item);
			if(result)
			{
				// reset on removals so that we don't have to pass the "index" that the removal occured at.
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
			return result;
		}
	}
}
