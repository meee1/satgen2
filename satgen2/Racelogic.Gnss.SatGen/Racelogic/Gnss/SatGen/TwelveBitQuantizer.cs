using System;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class TwelveBitQuantizer : Quantizer
	{
		private readonly double gain;

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		internal unsafe TwelveBitQuantizer(Memory<byte> buffer, in double rms)
			: base(in buffer)
		{
			TwelveBitQuantizer twelveBitQuantizer = this;
			short* bufferPointer = (short*)BufferHandle.Pointer;
			int bufferIndex = 0;
			gain = 819.2 / rms;
			Add = delegate(double inPhase, double quadrature)
			{
				bufferPointer[bufferIndex++] = twelveBitQuantizer.QuantizeTo12Bits(inPhase);
				bufferPointer[bufferIndex++] = twelveBitQuantizer.QuantizeTo12Bits(quadrature);
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private short QuantizeTo12Bits(double value)
		{
			value *= gain;
			int num = (int)Math.Round(value);
			if (num < -2048)
			{
				num = -2048;
			}
			if (num > 2047)
			{
				num = 2047;
			}
			return (short)num;
		}
	}
}
