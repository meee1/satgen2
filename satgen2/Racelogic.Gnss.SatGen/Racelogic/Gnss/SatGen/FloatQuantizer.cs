using System;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class FloatQuantizer : Quantizer
	{
		private readonly double gain;

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		internal unsafe FloatQuantizer(Memory<byte> buffer, in double rms = 0.0)
			: base(in buffer)
		{
			FloatQuantizer floatQuantizer = this;
			float* bufferPointer = (float*)BufferHandle.Pointer;
			int bufferIndex = 0;
			if (rms == 0.0)
			{
				gain = 1.0;
				Add = delegate(double inPhase, double quadrature)
				{
					bufferPointer[bufferIndex++] = (float)inPhase;
					bufferPointer[bufferIndex++] = (float)quadrature;
				};
			}
			else
			{
				gain = 0.4 / rms;
				Add = delegate(double inPhase, double quadrature)
				{
					bufferPointer[bufferIndex++] = floatQuantizer.QuantizeToFloat(inPhase);
					bufferPointer[bufferIndex++] = floatQuantizer.QuantizeToFloat(quadrature);
				};
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private float QuantizeToFloat(double value)
		{
			value *= gain;
			if (value < -1.0)
			{
				value = -1.0;
			}
			if (value > 1.0)
			{
				value = 1.0;
			}
			return (float)value;
		}
	}
}
