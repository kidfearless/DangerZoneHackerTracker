using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DangerZoneHackerTracker
{
	public class ObservableSet<T> : HashSet<T>, INotifyCollectionChanged
	{
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object item)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
		}


		private void OnCollectionReset()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
		}


		public new bool Add(T item)
		{
			var result = base.Add(item);
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item);
			return result;
		}

		public new void ExceptWith(IEnumerable<T> other)
		{
			var others = this.Except(other).ToArray();
			base.ExceptWith(other);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, others));
		}

		public new void IntersectWith(IEnumerable<T> other)
		{
			var old = this.ToArray();
			var others = this.Intersect(other).ToArray();
			base.IntersectWith(other);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, newItem: others, oldItem: old));
		}

		public new void SymmetricExceptWith(IEnumerable<T> other)
		{
			throw new NotImplementedException();
			/*this.S
			base.SymmetricExceptWith(other);
			Save();*/
		}

		public new void UnionWith(IEnumerable<T> other)
		{
			throw new NotImplementedException();
			/*
						base.UnionWith(other);
						Save();*/
		}

		public new void Clear()
		{
			base.Clear();
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		public new bool Remove(T item)
		{
			var result = base.Remove(item);
			if (result)
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
			}
			return result;
		}
	}
}
