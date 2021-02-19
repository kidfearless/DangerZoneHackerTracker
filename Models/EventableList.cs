using System.Collections;
using System.Collections.Generic;

namespace DangerZoneHackerTracker
{
	// A list with events for when when it's modified
	public class EventableList<T> : List<T>
	{
		public delegate void CollectionChangedHandler(EventableSortedSet<T> sender, CollectionChangedAction action, T? itemChanged);

		private ThreadSafeEvent<CollectionChangedHandler> collectionChanged = new();

		public event CollectionChangedHandler CollectionChanged
		{
			add => collectionChanged.AddEvent(value);
			remove => collectionChanged.RemoveEvent(value);
		}

		private void OnCollectionChanged(CollectionChangedAction action, T? item) => collectionChanged.Invoke(this, action, item);

		private void OnCollectionChanged(CollectionChangedAction action) => collectionChanged.Invoke(this, action, default);

		public new T this[int index]
		{
			get => base[index];
			set
			{
				base[index] = value;
				OnCollectionChanged(CollectionChangedAction.Modified, value);
			}
		}

		public new void Add(T item)
		{
			base.Add(item);
			OnCollectionChanged(CollectionChangedAction.Add, item);
		}

		public new void Clear()
		{
			base.Clear();
			OnCollectionChanged(CollectionChangedAction.Multiple);

		}
	

		public new void Insert(int index, T item)
		{
			base.Insert(index, item);
			OnCollectionChanged(CollectionChangedAction.Add, item);
		}

		public new bool Remove(T item)
		{
			var result = base.Remove(item);
			OnCollectionChanged(CollectionChangedAction.Remove, item);
			return result;
		}

		public new void RemoveAt(int index)
		{
			var item = this[index];
			base.RemoveAt(index);
			OnCollectionChanged(CollectionChangedAction.Remove, item);

		}
	}
}
