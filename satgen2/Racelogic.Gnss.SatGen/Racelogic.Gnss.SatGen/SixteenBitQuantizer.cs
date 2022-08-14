using System;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen;

internal sealed class SixteenBitQuantizer : Quantizer
{
	private readonly double gain;

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal unsafe SixteenBitQuantizer(Memory<byte> buffer, in double rms)
		: base(in buffer)
	{
		SixteenBitQuantizer sixteenBitQuantizer = this;
		short* bufferPointer = (short*)BufferHandle.Pointer;
		int bufferIndex = 0;
		gain = 13107.2 / rms;
		Add = delegate(double inPhase, double quadrature)
		{
			bufferPointer[bufferIndex++] = sixteenBitQuantizer.QuantizeTo16Bits(inPhase);
			bufferPointer[bufferIndex++] = sixteenBitQuantizer.QuantizeTo16Bits(quadrature);
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private short QuantizeTo16Bits(double value)
	{
		value *= gain;
		int num = (int)Math.Round(value);
		if (num < -32768)
		{
			num = -32768;
		}
		if (num > 32767)
		{
			num = 32767;
		}
		return (short)num;
	}
}
