using System;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen;

internal sealed class LabSat3wQuantizer : Quantizer
{
	private unsafe readonly ulong* bufferPointer;

	private readonly double threshold;

	private readonly double lowThreshold;

	private readonly double highThreshold;

	private readonly double minusThreshold;

	private readonly double minusLowThreshold;

	private readonly double minusHighThreshold;

	private const int wordLength = 64;

	private ulong word;

	private int bufferIndex;

	private readonly ulong startBitMask;

	private ulong bitMask;

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal unsafe LabSat3wQuantizer(in Memory<byte> buffer, Channel channel, ChannelPlan channelPlan, in double rms)
		: base(in buffer)
	{
		bufferPointer = (ulong*)BufferHandle.Pointer;
		Quantization quantization = channel.Quantization;
		if (quantization <= Quantization.TwoBit)
		{
			threshold = rms;
		}
		else
		{
			threshold = 0.75 * rms;
		}
		lowThreshold = 0.5 * threshold;
		highThreshold = 1.5 * threshold;
		minusThreshold = 0.0 - threshold;
		minusLowThreshold = 0.0 - lowThreshold;
		minusHighThreshold = 0.0 - highThreshold;
		int count = channelPlan.Channels.Count;
		int i;
		for (i = 0; i < count && channelPlan.Channels[i] != channel; i++)
		{
		}
		int num = count * (int)quantization << 1;
		int num2 = 64 / num;
		int num3 = 64 - num2 * num;
		startBitMask = (ulong)(1L << 63 - num3);
		bitMask = startBitMask;
		switch (quantization)
		{
		case Quantization.OneBit:
			switch (count)
			{
			case 1:
				Add = delegate(double inPhase, double quadrature)
				{
					Quantize1Bit(in inPhase);
					bitMask >>= 1;
					Quantize1Bit(in quadrature);
					bitMask >>= 1;
					FinalizeWord64();
				};
				break;
			case 2:
				if (i == 0)
				{
					Add = delegate(double inPhase, double quadrature)
					{
						Quantize1Bit(in inPhase);
						bitMask >>= 1;
						Quantize1Bit(in quadrature);
						bitMask >>= 3;
						FinalizeWord64();
					};
					break;
				}
				Add = delegate(double inPhase, double quadrature)
				{
					bitMask >>= 2;
					Quantize1Bit(in inPhase);
					bitMask >>= 1;
					Quantize1Bit(in quadrature);
					bitMask >>= 1;
					FinalizeWord64();
				};
				break;
			default:
				switch (i)
				{
				case 0:
					Add = delegate(double inPhase, double quadrature)
					{
						Quantize1Bit(in inPhase);
						bitMask >>= 1;
						Quantize1Bit(in quadrature);
						bitMask >>= 5;
						FinalizeWord64();
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
						FinalizeWord64();
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
						FinalizeWord64();
					};
					break;
				}
				break;
			}
			return;
		case Quantization.TwoBit:
			switch (count)
			{
			case 1:
				Add = delegate(double inPhase, double quadrature)
				{
					Quantize2Bit(in inPhase);
					bitMask >>= 2;
					Quantize2Bit(in quadrature);
					bitMask >>= 2;
					FinalizeWord64();
				};
				break;
			case 2:
				if (i == 0)
				{
					Add = delegate(double inPhase, double quadrature)
					{
						Quantize2Bit(in inPhase);
						bitMask >>= 2;
						Quantize2Bit(in quadrature);
						bitMask >>= 6;
						FinalizeWord64();
					};
					break;
				}
				Add = delegate(double inPhase, double quadrature)
				{
					bitMask >>= 4;
					Quantize2Bit(in inPhase);
					bitMask >>= 2;
					Quantize2Bit(in quadrature);
					bitMask >>= 2;
					FinalizeWord64();
				};
				break;
			default:
				switch (i)
				{
				case 0:
					Add = delegate(double inPhase, double quadrature)
					{
						Quantize2Bit(in inPhase);
						bitMask >>= 2;
						Quantize2Bit(in quadrature);
						bitMask >>= 10;
						FinalizeWord64();
					};
					break;
				case 1:
					Add = delegate(double inPhase, double quadrature)
					{
						bitMask >>= 4;
						Quantize2Bit(in inPhase);
						bitMask >>= 2;
						Quantize2Bit(in quadrature);
						bitMask >>= 6;
						FinalizeWord64();
					};
					break;
				default:
					Add = delegate(double inPhase, double quadrature)
					{
						bitMask >>= 8;
						Quantize2Bit(in inPhase);
						bitMask >>= 2;
						Quantize2Bit(in quadrature);
						bitMask >>= 2;
						FinalizeWord64();
					};
					break;
				}
				break;
			}
			return;
		}
		switch (count)
		{
		case 1:
			Add = delegate(double inPhase, double quadrature)
			{
				Quantize3Bit(in inPhase);
				bitMask >>= 3;
				Quantize3Bit(in quadrature);
				bitMask >>= 3;
				FinalizeWord64();
			};
			return;
		case 2:
			if (i == 0)
			{
				Add = delegate(double inPhase, double quadrature)
				{
					Quantize3Bit(in inPhase);
					bitMask >>= 3;
					Quantize3Bit(in quadrature);
					bitMask >>= 9;
					FinalizeWord64();
				};
				return;
			}
			Add = delegate(double inPhase, double quadrature)
			{
				bitMask >>= 6;
				Quantize3Bit(in inPhase);
				bitMask >>= 3;
				Quantize3Bit(in quadrature);
				bitMask >>= 3;
				FinalizeWord64();
			};
			return;
		}
		switch (i)
		{
		case 0:
			Add = delegate(double inPhase, double quadrature)
			{
				Quantize3Bit(in inPhase);
				bitMask >>= 3;
				Quantize3Bit(in quadrature);
				bitMask >>= 15;
				FinalizeWord64();
			};
			break;
		case 1:
			Add = delegate(double inPhase, double quadrature)
			{
				bitMask >>= 6;
				Quantize3Bit(in inPhase);
				bitMask >>= 3;
				Quantize3Bit(in quadrature);
				bitMask >>= 9;
				FinalizeWord64();
			};
			break;
		default:
			Add = delegate(double inPhase, double quadrature)
			{
				bitMask >>= 12;
				Quantize3Bit(in inPhase);
				bitMask >>= 3;
				Quantize3Bit(in quadrature);
				bitMask >>= 3;
				FinalizeWord64();
			};
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Quantize1Bit(in double value)
	{
		if (value <= 0.0)
		{
			word |= bitMask;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Quantize2Bit(in double value)
	{
		if (value > 0.0)
		{
			if (value > threshold)
			{
				word |= bitMask >> 1;
			}
			return;
		}
		word |= bitMask;
		if (value > minusThreshold)
		{
			word |= bitMask >> 1;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private void Quantize3Bit(in double value)
	{
		if (value > 0.0)
		{
			if (value > threshold)
			{
				word |= bitMask >> 1;
				if (value > highThreshold)
				{
					word |= bitMask >> 2;
				}
			}
			else if (value > lowThreshold)
			{
				word |= bitMask >> 2;
			}
			return;
		}
		word |= bitMask;
		if (value > minusThreshold)
		{
			word |= bitMask >> 1;
			if (value > minusLowThreshold)
			{
				word |= bitMask >> 2;
			}
		}
		else if (value > minusHighThreshold)
		{
			word |= bitMask >> 2;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe void FinalizeWord64()
	{
		if (bitMask == 0L)
		{
			bufferPointer[bufferIndex++] = word;
			word = 0uL;
			bitMask = startBitMask;
		}
	}
}
