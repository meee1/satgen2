using System.Collections.Generic;
using System.Linq;

namespace Racelogic.Gnss.BeiDou;

public static class BchCoding
{
	public static IReadOnlyList<byte> GetParityBits(IEnumerable<byte> data)
	{
		byte[] array = new byte[4];
		foreach (byte item in data.Take(11))
		{
			byte b = (byte)(item ^ array[0]);
			array[0] = array[1];
			array[1] = array[2];
			array[2] = (byte)(b ^ array[3]);
			array[3] = b;
		}
		return array;
	}

	public static IReadOnlyList<byte> Encode(IEnumerable<byte> data)
	{
		byte[] array = new byte[15];
		byte b = 0;
		byte b2 = 0;
		byte b3 = 0;
		byte b4 = 0;
		using IEnumerator<byte> enumerator = data.GetEnumerator();
		for (int i = 0; i < 11; i++)
		{
			enumerator.MoveNext();
			byte num = (byte)((array[i] = enumerator.Current) ^ b4);
			b4 = b3;
			b3 = b2;
			b2 = (byte)(num ^ b);
			b = num;
		}
		array[11] = b4;
		array[12] = b3;
		array[13] = b2;
		array[14] = b;
		return array;
	}

	public static IReadOnlyList<byte> EncodeInterleaved(IEnumerable<byte> data)
	{
		IReadOnlyList<byte> readOnlyList = Encode(data.Take(11));
		IReadOnlyList<byte> readOnlyList2 = Encode(data.Skip(11));
		byte[] array = new byte[30];
		int num = 0;
		for (int i = 0; i < 15; i++)
		{
			array[num++] = readOnlyList[i];
			array[num++] = readOnlyList2[i];
		}
		return array;
	}
}
