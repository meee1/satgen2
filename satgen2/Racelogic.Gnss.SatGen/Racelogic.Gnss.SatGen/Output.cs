using System;
using System.Diagnostics;
using System.IO;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

public abstract class Output : BasePropertyChanged, IDisposable
{
	public abstract ChannelPlan ChannelPlan { get; }

	internal abstract int SampleSize
	{
		[DebuggerStepThrough]
		get;
	}

	internal abstract int WordLength
	{
		[DebuggerStepThrough]
		get;
	}

	internal abstract int SamplesInWord
	{
		[DebuggerStepThrough]
		get;
	}

	public event EventHandler<ErrorEventArgs>? Error;

	internal abstract bool Write(SimulationSlice slice);

	internal abstract Quantizer GetQuantizer(in Memory<byte> buffer, Channel channel, in double rms);

	public int GetOutputByteCountForInterval(in double seconds)
	{
		long num = (long)Math.Round(seconds * (double)ChannelPlan.SampleRate);
		int num2 = WordLength >> 3;
		return (int)(num / SamplesInWord * num2);
	}

	protected void OnError(Exception ex)
	{
		RLLogger.GetLogger().LogException(ex, handled: true);
		this.Error?.Invoke(this, new ErrorEventArgs(ex));
	}

	public abstract void Close();

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}
