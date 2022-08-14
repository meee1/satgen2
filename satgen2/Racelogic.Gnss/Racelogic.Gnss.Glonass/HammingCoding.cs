using System.Collections.Generic;
using System.Linq;

namespace Racelogic.Gnss.Glonass;

public static class HammingCoding
{
	private static readonly int[] messageIndicesI = new int[41]
	{
		9, 10, 12, 13, 15, 17, 19, 20, 22, 24,
		26, 28, 30, 32, 34, 35, 37, 39, 41, 43,
		45, 47, 49, 51, 53, 55, 57, 59, 61, 63,
		65, 66, 68, 70, 72, 74, 76, 78, 80, 82,
		84
	};

	private static readonly int[] messageIndicesJ = new int[41]
	{
		9, 11, 12, 14, 15, 18, 19, 21, 22, 25,
		26, 29, 30, 33, 34, 36, 37, 40, 41, 44,
		45, 48, 49, 52, 53, 56, 57, 60, 61, 64,
		65, 67, 68, 71, 72, 75, 76, 79, 80, 83,
		84
	};

	private static readonly int[] messageIndicesK = new int[40]
	{
		10, 11, 12, 16, 17, 18, 19, 23, 24, 25,
		26, 31, 32, 33, 34, 38, 39, 40, 41, 46,
		47, 48, 49, 54, 55, 56, 57, 62, 63, 64,
		65, 69, 70, 71, 72, 77, 78, 79, 80, 85
	};

	private static readonly int[] messageIndicesL = new int[39]
	{
		13, 14, 15, 16, 17, 18, 19, 27, 28, 29,
		30, 31, 32, 33, 34, 42, 43, 44, 45, 46,
		47, 48, 49, 58, 59, 60, 61, 62, 63, 64,
		65, 73, 74, 75, 76, 77, 78, 79, 80
	};

	private static readonly int[] messageIndicesM = new int[36]
	{
		20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
		30, 31, 32, 33, 34, 50, 51, 52, 53, 54,
		55, 56, 57, 58, 59, 60, 61, 62, 63, 64,
		65, 81, 82, 83, 84, 85
	};

	private static readonly int[] messageIndicesN = new int[31]
	{
		35, 36, 37, 38, 39, 40, 41, 42, 43, 44,
		45, 46, 47, 48, 49, 50, 51, 52, 53, 54,
		55, 56, 57, 58, 59, 60, 61, 62, 63, 64,
		65
	};

	private static readonly int[] messageIndicesP = new int[20]
	{
		66, 67, 68, 69, 70, 71, 72, 73, 74, 75,
		76, 77, 78, 79, 80, 81, 82, 83, 84, 85
	};

	private static readonly int[][] messageIndices = new int[7][] { messageIndicesI, messageIndicesJ, messageIndicesK, messageIndicesL, messageIndicesM, messageIndicesN, messageIndicesP };

	public static IEnumerable<byte> GetCheckBits(IReadOnlyList<byte> message)
	{
		int chekBitSum = 0;
		for (int parityBitIndex = 0; parityBitIndex < 7; parityBitIndex++)
		{
			int num = 0;
			int[] array = messageIndices[parityBitIndex];
			for (int i = 0; i < array.Length; i++)
			{
				num += message[array[i] - 9];
			}
			int num2 = num & 1;
			chekBitSum += num2;
			yield return (byte)num2;
		}
		int num3 = (message.Sum((byte b) => b) + chekBitSum) & 1;
		yield return (byte)num3;
	}
}
