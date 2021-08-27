using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Racelogic.DataTypes
{
	[JsonObject(MemberSerialization.OptIn)]
	public class FixedSizeStack<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
	{
		private readonly T[] table;

		private int lastItemIndex = -1;

		public int Count
		{
			[DebuggerStepThrough]
			get;
			private set; }

		[JsonProperty(PropertyName = "Capacity")]
		public int Capacity
		{
			[DebuggerStepThrough]
			get;
		}

		[JsonProperty(PropertyName = "Collection")]
		public ReadOnlyCollection<T> ItemList
		{
			get
			{
				T[] array = new T[Count];
				int num = lastItemIndex + Capacity;
				for (int i = 0; i < Count; i++)
				{
					array[i] = table[(num - i) % Capacity];
				}
				return new ReadOnlyCollection<T>(array);
			}
		}

		public ReadOnlyCollection<T> ReverseItemList
		{
			get
			{
				T[] array = new T[Count];
				int num = lastItemIndex + Capacity;
				int num2 = 0;
				for (int num3 = Count - 1; num3 >= 0; num3--)
				{
					array[num2++] = table[(num - num3) % Capacity];
				}
				return new ReadOnlyCollection<T>(array);
			}
		}

		public T this[int i]
		{
			[DebuggerStepThrough]
			get
			{
				return table[(lastItemIndex + Capacity - i) % Capacity];
			}
		}

		public T this[uint i]
		{
			[DebuggerStepThrough]
			get
			{
				return table[(lastItemIndex + Capacity - i) % Capacity];
			}
		}

		public event EventHandler<ItemRecycledEventArgs<T>> ItemRecycled;

		public FixedSizeStack(int capacity)
		{
			if (capacity <= 0 || capacity != capacity || (double)capacity > double.MaxValue)
			{
				throw new ArgumentOutOfRangeException("capacity");
			}
			Capacity = capacity;
			table = new T[capacity];
		}

		public FixedSizeStack(IEnumerable<T> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			table = collection.ToArray();
			Capacity = table.Length;
			Count = Capacity;
			lastItemIndex = Capacity - 1;
		}

		[JsonConstructor]
		public FixedSizeStack(int capacity, IEnumerable<T> collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			Capacity = capacity;
			table = new T[capacity];
			foreach (T item in collection)
			{
				table[Count++] = item;
			}
			lastItemIndex = Count - 1;
		}

		public void Push(T item)
		{
			lastItemIndex = (lastItemIndex + 1) % Capacity;
			T val = table[lastItemIndex];
			table[lastItemIndex] = item;
			if (Count < Capacity)
			{
				Count++;
			}
			if (val != null)
			{
				OnItemRecycled(val);
			}
		}

		public void PushDistinct(T item)
		{
			ReadOnlyCollection<T> reverseItemList = ReverseItemList;
			Clear();
			foreach (T item2 in reverseItemList.Where((T e) => !EqualityComparer<T>.Default.Equals(e, item)))
			{
				Push(item2);
			}
			Push(item);
		}

		public T Pop()
		{
			if (Count == 0)
			{
				return default(T);
			}
			T result = table[lastItemIndex];
			table[lastItemIndex] = default(T);
			lastItemIndex = (lastItemIndex - 1 + Capacity) % Capacity;
			Count--;
			return result;
		}

		public T Peek()
		{
			if (Count == 0)
			{
				return default(T);
			}
			return table[lastItemIndex];
		}

		public void Clear()
		{
			lastItemIndex = 0;
			Count = 0;
			for (int i = 0; i < Capacity; i++)
			{
				T val = table[i];
				if (val != null)
				{
					OnItemRecycled(val);
					table[i] = default(T);
				}
			}
		}

		public T ReverseElementAt(int index)
		{
			return this[Count - index - 1];
		}

		public T ReverseElementAt(uint index)
		{
			return this[(uint)(Count - (int)index - 1)];
		}

		public void ReplaceAt(int index, T newItem)
		{
			table[(lastItemIndex + Capacity - index) % Capacity] = newItem;
		}

		public void ReplaceAt(uint index, T newItem)
		{
			table[(lastItemIndex + Capacity - index) % Capacity] = newItem;
		}

		public void ReverseReplaceAt(int index, T newItem)
		{
			table[(lastItemIndex + Capacity - Count + index + 1) % Capacity] = newItem;
		}

		public void ReverseReplaceAt(uint index, T newItem)
		{
			table[(lastItemIndex + Capacity - Count + index + 1) % Capacity] = newItem;
		}

		protected void OnItemRecycled(T recycledItem)
		{
			this.ItemRecycled?.Invoke(this, new ItemRecycledEventArgs<T>(recycledItem));
		}

		private T[] GetItems()
		{
			T[] array = new T[Count];
			int num = lastItemIndex + Capacity;
			for (int i = 0; i < Count; i++)
			{
				array[i] = table[(num - i) % Capacity];
			}
			return array;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return ((IEnumerable<T>)GetItems()).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetItems().GetEnumerator();
		}
	}
}
