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


	public class EventableSortedSet<T> : SortedSet<T>
	{
		public EventableSortedSet(IComparer<T> comparer) : base(comparer) { }
		public EventableSortedSet() { }

		public delegate void CollectionChangedHandler(EventableSortedSet<T> sender, CollectionChangedAction action, T? itemChanged);

		public ThreadSafeEvent<CollectionChangedHandler> CollectionChanged = new();

		private void OnCollectionChanged(CollectionChangedAction action, T? item)
		{
			CollectionChanged?.Invoke(this, action, item);
		}

		private void OnCollectionChanged(CollectionChangedAction action)
		{
			CollectionChanged?.Invoke(this, action, default);
		}

		public new void ExceptWith(IEnumerable<T> other)
		{
			base.ExceptWith(other);
			OnCollectionChanged(CollectionChangedAction.Multiple);
		}

		public new void IntersectWith(IEnumerable<T> other)
		{
			base.IntersectWith(other);
			OnCollectionChanged(CollectionChangedAction.Multiple);
		}

		public new void SymmetricExceptWith(IEnumerable<T> other)
		{
			base.SymmetricExceptWith(other);
			OnCollectionChanged(CollectionChangedAction.Multiple);
		}

		public new void UnionWith(IEnumerable<T> other)
		{
			base.UnionWith(other);
			OnCollectionChanged(CollectionChangedAction.Multiple);
		}


		public new bool Add(T item)
		{
			var action = base.Contains(item) ? CollectionChangedAction.Modified : CollectionChangedAction.Add;
			var result = base.Add(item);
			OnCollectionChanged(action, item);
			return result;
		}

		public new void Clear()
		{
			base.Clear();
			OnCollectionChanged(CollectionChangedAction.Multiple);
		}

		public new bool Remove(T item)
		{
			var result = base.Remove(item);
			if(result)
			{
				OnCollectionChanged(CollectionChangedAction.Remove, item);
			}
			return result;
		}
	}
}
