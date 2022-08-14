using System;
using System.Diagnostics;

namespace Racelogic.Gnss.SatGen;

internal abstract class Generator : IDisposable
{
	private readonly Memory<byte> buffer;

	private readonly GeneratorParams parameters;

	internal Memory<byte> Buffer
	{
		[DebuggerStepThrough]
		get
		{
			return buffer;
		}
	}

	public GeneratorParams Parameters
	{
		[DebuggerStepThrough]
		get
		{
			return parameters;
		}
	}

	protected Generator(in Memory<byte> buffer, GeneratorParams parameters)
	{
		this.buffer = buffer;
		this.parameters = parameters;
	}

	public abstract Memory<byte> Generate();

	public abstract double MeasureRMS();

	public abstract void ApplyRMS(double rms);

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}
