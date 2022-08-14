using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.Gnss;

public class ConvolutionalEncoder : IEnumerable<byte>, IEnumerable, IDisposable
{
	private readonly ConvolutionalEnumerator enumerator;

	private bool isDisposed;

	public static int ByteRatio
	{
		[DebuggerStepThrough]
		get
		{
			return 2;
		}
	}

	public static int ConstraintLength
	{
		[DebuggerStepThrough]
		get
		{
			return 7;
		}
	}

	public int? CapturedState
	{
		[DebuggerStepThrough]
		get
		{
			return enumerator.CapturedState;
		}
	}

	public ConvolutionalEncoder(IEnumerable<byte> inputSequence, in int registerState = 0, in int captureIndex = 0, ConvolutionalEncoderOptions options = ConvolutionalEncoderOptions.None)
	{
		enumerator = new ConvolutionalEnumerator(inputSequence, in registerState, in captureIndex, options);
	}

	public IEnumerator<byte> GetEnumerator()
	{
		return enumerator;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return enumerator;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing)
			{
				enumerator.Dispose();
			}
		}
	}
}
