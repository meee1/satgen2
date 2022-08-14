namespace Racelogic.DataTypes;

public class ArrayPool<T> : IBufferPool<T[]>
{
	private readonly BufferPool<T[]> bufferPool;

	private const int defaultLockTimeout = 10;

	public ArrayPool(int bufferTimeout = 0, int lockTimeout = 10)
	{
		bufferPool = new BufferPool<T[]>((int size) => new T[size], (T[] array) => array.Length, bufferTimeout, lockTimeout);
	}

	public T[] Take(int bufferSize)
	{
		return bufferPool.Take(bufferSize);
	}

	public T[] Take(uint bufferSize)
	{
		return bufferPool.Take(bufferSize);
	}

	public T[] Take(int minBufferSize, int maxBufferSize)
	{
		return bufferPool.Take(minBufferSize, maxBufferSize);
	}

	public T[] Take(uint minBufferSize, uint maxBufferSize)
	{
		return bufferPool.Take(minBufferSize, maxBufferSize);
	}

	public void Recycle(T[] buffer)
	{
		bufferPool.Recycle(buffer);
	}

	public void Clear(bool disposeBuffersInUse = false)
	{
		bufferPool.Clear(disposeBuffersInUse);
	}
}
