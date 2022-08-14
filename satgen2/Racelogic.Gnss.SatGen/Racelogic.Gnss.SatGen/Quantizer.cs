using System;
using System.Buffers;

namespace Racelogic.Gnss.SatGen;

internal abstract class Quantizer : IDisposable
{
	protected readonly MemoryHandle BufferHandle;

	internal Action<double, double> Add = delegate
	{
	};

	private bool disposedValue;

	protected Quantizer(in Memory<byte> buffer)
	{
		BufferHandle = buffer.Pin();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			disposedValue = true;
			if (disposing)
			{
				BufferHandle.Dispose();
			}
		}
	}
}
