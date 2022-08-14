using System;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen;

internal sealed class EightBitQuantizer : Quantizer
{
	private readonly double gain;

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal unsafe EightBitQuantizer(Memory<byte> buffer, in double rms)
		: base(in buffer)
	{
		EightBitQuantizer eightBitQuantizer = this;
		sbyte* bufferPointer = (sbyte*)BufferHandle.Pointer;
		int bufferIndex = 0;
		gain = 51.2 / rms;
		Add = delegate(double inPhase, double quadrature)
		{
			bufferPointer[bufferIndex++] = eightBitQuantizer.QuantizeTo8Bits(inPhase);
			bufferPointer[bufferIndex++] = eightBitQuantizer.QuantizeTo8Bits(quadrature);
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private sbyte QuantizeTo8Bits(double value)
	{
		value *= gain;
		int num = (int)Math.Round(value);
		if (num < -128)
		{
			num = -128;
		}
		if (num > 127)
		{
			num = 127;
		}
		return (sbyte)num;
	}
}
