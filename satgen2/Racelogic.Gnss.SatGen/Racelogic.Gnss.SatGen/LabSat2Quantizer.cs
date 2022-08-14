using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen;

internal sealed class LabSat2Quantizer : Quantizer
{
	private unsafe readonly ushort* bufferPointer;

	private readonly double threshold;

	private readonly double minusThreshold;

	private uint word;

	private int bufferIndex;

	private const uint startBitMask = 32768u;

	private uint bitMask = 32768u;

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal unsafe LabSat2Quantizer(in Memory<byte> buffer, Channel channel, ChannelPlan channelPlan, in double rms)
		: base(in buffer)
	{
		bufferPointer = (ushort*)BufferHandle.Pointer;
		threshold = rms;
		minusThreshold = 0.0 - threshold;
		IReadOnlyList<Channel> channels = channelPlan.Channels;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < channels.Count; i++)
		{
			Channel channel2 = channels[i];
			if (channel2 != null)
			{
				num2++;
				if (channel2 == channel)
				{
					num = i;
				}
			}
		}
		if (channel.Quantization == Quantization.OneBit)
		{
			if (num2 == 1)
			{
				Add = delegate(double inPhase, double quadrature)
				{
					Quantize1Bit(in inPhase);
					bitMask >>= 1;
					Quantize1Bit(in quadrature);
					bitMask >>= 1;
					FinalizeWord16();
				};
				return;
			}
			if (num == 0)
			{
				Add = delegate(double inPhase, double quadrature)
				{
					Quantize1Bit(in inPhase);
					bitMask >>= 1;
					Quantize1Bit(in quadrature);
					bitMask >>= 3;
					FinalizeWord16();
				};
				return;
			}
			Add = delegate(double inPhase, double quadrature)
			{
				bitMask >>= 2;
				Quantize1Bit(in inPhase);
				bitMask >>= 1;
				Quantize1Bit(in quadrature);
				bitMask >>= 1;
				FinalizeWord16();
			};
		}
		else
		{
			Add = delegate(double inPhase, double quadrature)
			{
				Quantize2Bit(in inPhase);
				bitMask >>= 1;
				Quantize2Bit(in quadrature);
				bitMask >>= 3;
				FinalizeWord16();
			};
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Quantize1Bit(in double value)
	{
		if (value > 0.0)
		{
			word |= bitMask;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Quantize2Bit(in double value)
	{
		if (value > 0.0)
		{
			word |= bitMask;
			if (value > threshold)
			{
				word |= bitMask >> 2;
			}
		}
		else if (value > minusThreshold)
		{
			word |= bitMask >> 2;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe void FinalizeWord16()
	{
		if (bitMask == 0)
		{
			bufferPointer[bufferIndex++] = (ushort)word;
			word = 0u;
			bitMask = 32768u;
		}
	}
}
