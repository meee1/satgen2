using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.DataTypes;

public class ItemRecycledEventArgs<T> : EventArgs
{
	private readonly T item;

	public T Item
	{
		[DebuggerStepThrough]
		get
		{
			return item;
		}
	}

	public ItemRecycledEventArgs(T recycledItem)
	{
		item = recycledItem;
	}
}
public class ItemRecycledEventArgs<TKey, TValue> : EventArgs
{
	private readonly TKey key;

	private readonly TValue val;

	public TKey Key
	{
		[DebuggerStepThrough]
		get
		{
			return key;
		}
	}

	public TValue Value
	{
		[DebuggerStepThrough]
		get
		{
			return val;
		}
	}

	public ItemRecycledEventArgs(TKey recycledKey, TValue recycledValue)
	{
		key = recycledKey;
		val = recycledValue;
	}

	public ItemRecycledEventArgs(KeyValuePair<TKey, TValue> keyValue)
	{
		key = keyValue.Key;
		val = keyValue.Value;
	}
}
