using System.Diagnostics;
using System.Linq;

namespace Racelogic.Gnss.Glonass;

public static class FrequencySlot
{
	private static readonly int[] frequencySlots;

	private const int channelSpacingL1 = 562500;

	private const int channelSpacingL2 = 437500;

	private static readonly int[] frequencyOffsetsL1;

	private static readonly int[] frequencyOffsetsL2;

	private static readonly int centerOffsetL1;

	private static readonly int centerOffsetL2;

	public static int[] SlotNumbers
	{
		[DebuggerStepThrough]
		get
		{
			return frequencySlots;
		}
	}

	public static int[] FrequencyOffsetsL1
	{
		[DebuggerStepThrough]
		get
		{
			return frequencyOffsetsL1;
		}
	}

	public static int[] FrequencyOffsetsL2
	{
		[DebuggerStepThrough]
		get
		{
			return frequencyOffsetsL2;
		}
	}

	public static int CenterFrequencyOffsetL1
	{
		[DebuggerStepThrough]
		get
		{
			return centerOffsetL1;
		}
	}

	public static int CenterFrequencyOffsetL2
	{
		[DebuggerStepThrough]
		get
		{
			return centerOffsetL2;
		}
	}

	static FrequencySlot()
	{
		frequencySlots = new int[24]
		{
			1, -4, 5, 6, 1, -4, 5, 6, -2, -7,
			0, -1, -2, -7, 0, -1, 4, -3, 3, 2,
			4, -3, 3, 2
		};
		frequencyOffsetsL1 = new int[frequencySlots.Length];
		frequencyOffsetsL2 = new int[frequencySlots.Length];
		for (int i = 1; i < frequencySlots.Length; i++)
		{
			int num = frequencySlots[i];
			frequencyOffsetsL1[i] = num * 562500;
			frequencyOffsetsL2[i] = num * 437500;
		}
		centerOffsetL1 = frequencyOffsetsL1.Min() + (frequencyOffsetsL1.Max() - frequencyOffsetsL1.Min()) / 2;
		centerOffsetL2 = frequencyOffsetsL2.Min() + (frequencyOffsetsL2.Max() - frequencyOffsetsL2.Min()) / 2;
	}
}
