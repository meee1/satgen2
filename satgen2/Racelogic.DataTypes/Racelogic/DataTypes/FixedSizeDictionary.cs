using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace Racelogic.DataTypes
{
	[JsonObject(MemberSerialization.OptIn)]
	public class FixedSizeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary, ICollection
	{
		[JsonProperty(PropertyName = "Keys")]
		private readonly List<TKey> keys;

		[JsonProperty(PropertyName = "Dictionary")]
		private readonly Dictionary<TKey, TValue> dictionary;

		[JsonProperty(PropertyName = "Capacity")]
		public int Capacity
		{
			[DebuggerStepThrough]
			get;
		}

		public ICollection<TKey> Keys
		{
			[DebuggerStepThrough]
			get
			{
				return dictionary.Keys;
			}
		}

		public ICollection<TValue> Values
		{
			[DebuggerStepThrough]
			get
			{
				return dictionary.Values;
			}
		}

		public TValue this[TKey key]
		{
			[DebuggerStepThrough]
			get
			{
				return dictionary[key];
			}
			[DebuggerStepThrough]
			set
			{
				if (dictionary.TryGetValue(key, out var value2))
				{
					dictionary[key] = value;
					if (!value.Equals(value2))
					{
						OnItemRecycled(key, value2);
					}
				}
				else
				{
					Add(key, value);
				}
			}
		}

		public int Count
		{
			[DebuggerStepThrough]
			get
			{
				return dictionary.Count;
			}
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			[DebuggerStepThrough]
			get
			{
				return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;
			}
		}

		bool IDictionary.IsFixedSize
		{
			[DebuggerStepThrough]
			get
			{
				return ((IDictionary)dictionary).IsFixedSize;
			}
		}

		ICollection IDictionary.Keys
		{
			[DebuggerStepThrough]
			get
			{
				return ((IDictionary)dictionary).Keys;
			}
		}

		ICollection IDictionary.Values
		{
			[DebuggerStepThrough]
			get
			{
				return ((IDictionary)dictionary).Values;
			}
		}

		object IDictionary.this[object key]
		{
			[DebuggerStepThrough]
			get
			{
				return ((IDictionary)dictionary)[key];
			}
			[DebuggerStepThrough]
			set
			{
				IDictionary dictionary = this.dictionary;
				if (dictionary.Contains(key))
				{
					TValue val = (TValue)dictionary[key];
					dictionary[key] = value;
					if (!value.Equals(val))
					{
						OnItemRecycled((TKey)key, val);
					}
				}
				else
				{
					Add((TKey)key, (TValue)value);
				}
			}
		}

		bool IDictionary.IsReadOnly
		{
			[DebuggerStepThrough]
			get
			{
				return ((IDictionary)dictionary).IsReadOnly;
			}
		}

		bool ICollection.IsSynchronized
		{
			[DebuggerStepThrough]
			get
			{
				return ((ICollection)dictionary).IsSynchronized;
			}
		}

		object ICollection.SyncRoot
		{
			[DebuggerStepThrough]
			get
			{
				return ((ICollection)dictionary).SyncRoot;
			}
		}

		int ICollection.Count
		{
			[DebuggerStepThrough]
			get
			{
				return dictionary.Count;
			}
		}

		public event EventHandler<ItemRecycledEventArgs<TKey, TValue>> ItemRecycled;

		public FixedSizeDictionary(int capacity)
		{
			if (capacity <= 0)
			{
				throw new ArgumentException("Capacity must be greater than 0", "capacity");
			}
			Capacity = capacity;
			dictionary = new Dictionary<TKey, TValue>(capacity);
			keys = new List<TKey>(capacity);
		}

		[JsonConstructor]
		private FixedSizeDictionary(int capacity, List<TKey> keys, Dictionary<TKey, TValue> dictionary)
		{
			if (capacity <= 0)
			{
				throw new ArgumentException("Capacity must be greater than 0", "capacity");
			}
			Capacity = capacity;
			this.keys = keys;
			this.dictionary = dictionary;
		}

		protected void OnItemRecycled(KeyValuePair<TKey, TValue> recycledItem)
		{
			OnItemRecycled(recycledItem.Key, recycledItem.Value);
		}

		protected void OnItemRecycled(TKey recycledKey, TValue recycledValue)
		{
			this.ItemRecycled?.Invoke(this, new ItemRecycledEventArgs<TKey, TValue>(recycledKey, recycledValue));
		}

		public void Add(TKey key, TValue value)
		{
			if (dictionary.Count == Capacity)
			{
				TKey val = keys[0];
				keys.RemoveAt(0);
				TValue recycledValue = dictionary[val];
				dictionary.Remove(val);
				OnItemRecycled(val, recycledValue);
			}
			dictionary.Add(key, value);
			keys.Add(key);
		}

		public bool ContainsKey(TKey key)
		{
			return dictionary.ContainsKey(key);
		}

		public bool Remove(TKey key)
		{
			keys.Remove(key);
			if (dictionary.TryGetValue(key, out var value))
			{
				dictionary.Remove(key);
				OnItemRecycled(key, value);
				return true;
			}
			return false;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return dictionary.TryGetValue(key, out value);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		public void Clear()
		{
			foreach (KeyValuePair<TKey, TValue> item in dictionary)
			{
				OnItemRecycled(item);
			}
			dictionary.Clear();
			keys.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return dictionary.Contains(item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)dictionary).GetEnumerator();
		}

		void IDictionary.Add(object key, object value)
		{
			Add((TKey)key, (TValue)value);
		}

		bool IDictionary.Contains(object key)
		{
			return ((IDictionary)dictionary).Contains(key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return ((IDictionary)dictionary).GetEnumerator();
		}

		void IDictionary.Remove(object key)
		{
			if (key is TKey)
			{
				Remove((TKey)key);
			}
		}

		void IDictionary.Clear()
		{
			Clear();
		}

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)dictionary).CopyTo(array, index);
		}
	}
}
