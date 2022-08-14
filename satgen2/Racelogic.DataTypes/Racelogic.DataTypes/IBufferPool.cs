namespace Racelogic.DataTypes;

internal interface IBufferPool<T> where T : class
{
	T Take(int bufferSize);

	T Take(uint bufferSize);

	T Take(int minBufferSize, int maxBufferSize);

	T Take(uint bufferSize, uint maxBufferSize);

	void Recycle(T buffer);

	void Clear(bool disposeBuffersInUse = false);
}
