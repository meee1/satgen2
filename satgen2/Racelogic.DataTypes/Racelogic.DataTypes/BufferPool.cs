using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Racelogic.DataTypes;

public class BufferPool<T> : IBufferPool<T> where T : class
{
	private readonly Func<int, T> newBufferFunc;

	private readonly Func<T, int> getBufferSizeFunc;

	private readonly SyncLock bufferLock;

	private readonly Dictionary<int, Stack<BufferEntry<T>>> availableBuffers = new Dictionary<int, Stack<BufferEntry<T>>>();

	private readonly Dictionary<int, List<BufferEntry<T>>> buffersInUse = new Dictionary<int, List<BufferEntry<T>>>();

	private static readonly Stopwatch stopwatch = Stopwatch.StartNew();

	private readonly long bufferTimeout;

	private const int defaultLockTimeout = 10;

	public BufferPool(Func<int, T> newBufferFunc, Func<T, int> getBufferSizeFunc, int bufferTimeout = 0, int lockTimeout = 10)
	{
		this.newBufferFunc = newBufferFunc;
		this.getBufferSizeFunc = getBufferSizeFunc;
		this.bufferTimeout = 1000 * bufferTimeout;
		string name = $"BufferPool<{typeof(T)}>_Lock";
		int defaultTimeout = ((lockTimeout == -1) ? (-1) : (1000 * lockTimeout));
		bufferLock = new SyncLock(name, defaultTimeout);
	}

	public T Take(int bufferSize)
	{
		using (bufferLock.Lock())
		{
			Stack<BufferEntry<T>> value;
			BufferEntry<T> item = ((!availableBuffers.TryGetValue(bufferSize, out value) || !value.Any()) ? new BufferEntry<T>(newBufferFunc(bufferSize)) : value.Pop());
			if (!buffersInUse.TryGetValue(bufferSize, out var value2))
			{
				value2 = new List<BufferEntry<T>>();
				buffersInUse[bufferSize] = value2;
			}
			value2.Add(item);
			CollectGarbage();
			return item.Buffer;
		}
	}

	public T Take(int minBufferSize, int maxBufferSize)
	{
		using (bufferLock.Lock())
		{
			int key = (from kvp in availableBuffers
				where kvp.Key >= minBufferSize && kvp.Key <= maxBufferSize && kvp.Value.Count > 0
				orderby kvp.Key descending
				select kvp).FirstOrDefault().Key;
			int bufferSize = ((key > 0) ? key : maxBufferSize);
			return Take(bufferSize);
		}
	}

	public T Take(uint bufferSize)
	{
		return Take((int)bufferSize);
	}

	public T Take(uint bufferSize, uint maxBufferSize)
	{
		return Take((int)bufferSize, (int)maxBufferSize);
	}

	public void Recycle(T buffer)
	{
		if (buffer == null)
		{
			return;
		}
		using (bufferLock.Lock())
		{
			int key = getBufferSizeFunc(buffer);
			if (!buffersInUse.TryGetValue(key, out var value))
			{
				return;
			}
			BufferEntry<T> item = value.FirstOrDefault((BufferEntry<T> entry) => entry.Buffer == buffer);
			if (value.Remove(item))
			{
				item.LastUsedTime = stopwatch.ElapsedMilliseconds;
				if (!availableBuffers.TryGetValue(key, out var value2))
				{
					value2 = new Stack<BufferEntry<T>>();
					availableBuffers[key] = value2;
				}
				value2.Push(item);
			}
		}
	}

	public void Clear(bool disposeBuffersInUse = false)
	{
		using (bufferLock.Lock())
		{
			foreach (IDisposable item in from l in availableBuffers.Values.SelectMany((Stack<BufferEntry<T>> l) => l)
				select l.Buffer as IDisposable)
			{
				item?.Dispose();
			}
			availableBuffers.Clear();
			if (disposeBuffersInUse)
			{
				foreach (IDisposable item2 in from l in buffersInUse.Values.SelectMany((List<BufferEntry<T>> l) => l)
					select l.Buffer as IDisposable)
				{
					item2?.Dispose();
				}
			}
			buffersInUse.Clear();
		}
	}

	private void CollectGarbage()
	{
		if (bufferTimeout == 0L)
		{
			return;
		}
		using (bufferLock.Lock())
		{
			long num = stopwatch.ElapsedMilliseconds - bufferTimeout;
			if (num < 0)
			{
				num = 0L;
			}
			Stack<BufferEntry<T>> stack = new Stack<BufferEntry<T>>();
			KeyValuePair<int, Stack<BufferEntry<T>>>[] array = availableBuffers.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<int, Stack<BufferEntry<T>>> keyValuePair = array[i];
				Stack<BufferEntry<T>> value = keyValuePair.Value;
				foreach (BufferEntry<T> item in value)
				{
					if (item.LastUsedTime < num)
					{
						(item.Buffer as IDisposable)?.Dispose();
					}
					else
					{
						stack.Push(item);
					}
				}
				if (!stack.Any())
				{
					availableBuffers.Remove(keyValuePair.Key);
				}
				else if (stack.Count < value.Count)
				{
					value.Clear();
					foreach (BufferEntry<T> item2 in stack)
					{
						value.Push(item2);
					}
				}
				stack.Clear();
			}
		}
	}
}
