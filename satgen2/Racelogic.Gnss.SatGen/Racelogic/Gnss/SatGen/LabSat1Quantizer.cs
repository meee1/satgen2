using System;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class LabSat1Quantizer : Quantizer
	{
		private unsafe readonly byte* bufferPointer;

		private int bufferIndex;

		private uint word;

		private uint bitMask = 1u;

		private const uint bitMaskLimit = 256u;

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		internal unsafe LabSat1Quantizer(in Memory<byte> buffer)
			: base(in buffer)
		{
			bufferPointer = (byte*)BufferHandle.Pointer;
			Add = delegate(double inPhase, double quadrature)
			{
				if (inPhase > 0.0)
				{
					word |= bitMask;
				}
				bitMask <<= 1;
				FinalizeWord8();
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private unsafe void FinalizeWord8()
		{
			if (bitMask == 256)
			{
				bufferPointer[bufferIndex++] = (byte)word;
				word = 0u;
				bitMask = 1u;
			}
		}
	}
}
