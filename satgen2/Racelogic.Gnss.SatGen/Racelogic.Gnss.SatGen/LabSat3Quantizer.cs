using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen;

internal sealed class LabSat3Quantizer : Quantizer
{
	private unsafe readonly uint* buffer32Pointer;

	private unsafe readonly ushort* buffer16Pointer;

	private readonly double threshold;

	private readonly double minusThreshold;

	private uint word;

	private int bufferIndex;

	private const uint startBitMask16 = 32768u;

	private const uint startBitMask32 = 536870912u;

	private uint bitMask;

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal unsafe LabSat3Quantizer(in Memory<byte> buffer, Channel channel, ChannelPlan channelPlan, in double rms)
		: base(in buffer)
	{
		buffer32Pointer = (uint*)BufferHandle.Pointer;
		buffer16Pointer = (ushort*)BufferHandle.Pointer;
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
		if (num2 == 3)
		{
			bitMask = 536870912u;
		}
		else
		{
			bitMask = 32768u;
		}
		if (channel.Quantization == Quantization.OneBit)
		{
			switch (num2)
			{
			case 1:
				Add = delegate(double inPhase, double quadrature)
				{
					Quantize1Bit(in inPhase);
					bitMask >>= 1;
					Quantize1Bit(in quadrature);
					bitMask >>= 1;
					FinalizeWord16();
				};
				return;
			case 2:
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
				return;
			}
			switch (num)
			{
			case 0:
				Add = delegate(double inPhase, double quadrature)
				{
					Quantize1Bit(in inPhase);
					bitMask >>= 1;
					Quantize1Bit(in quadrature);
					bitMask >>= 5;
					FinalizeWord32();
				};
				break;
			case 1:
				Add = delegate(double inPhase, double quadrature)
				{
					bitMask >>= 2;
					Quantize1Bit(in inPhase);
					bitMask >>= 1;
					Quantize1Bit(in quadrature);
					bitMask >>= 3;
					FinalizeWord32();
				};
				break;
			default:
				Add = delegate(double inPhase, double quadrature)
				{
					bitMask >>= 4;
					Quantize1Bit(in inPhase);
					bitMask >>= 1;
					Quantize1Bit(in quadrature);
					bitMask >>= 1;
					FinalizeWord32();
				};
				break;
			}
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
			buffer16Pointer[bufferIndex++] = (ushort)word;
			bitMask = 32768u;
			word = 0u;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe void FinalizeWord32()
	{
		if (bitMask == 0)
		{
			buffer32Pointer[bufferIndex++] = word;
			word = 0u;
			bitMask = 536870912u;
		}
	}
}
