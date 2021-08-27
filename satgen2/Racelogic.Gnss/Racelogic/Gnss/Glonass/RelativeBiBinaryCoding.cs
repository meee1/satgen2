using System.Collections.Generic;

namespace Racelogic.Gnss.Glonass
{
	public static class RelativeBiBinaryCoding
	{
		public static IEnumerable<byte> Encode(IEnumerable<byte> dataSequence)
		{
			int lastValue = 0;
			foreach (byte item in dataSequence)
			{
				lastValue ^= item;
				yield return (byte)lastValue;
				yield return (byte)((uint)lastValue ^ 1u);
			}
		}

		public static IReadOnlyList<byte> Decode(IReadOnlyList<byte> dataSequence)
		{
			int num = dataSequence.Count >> 1;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = dataSequence[i << 1];
			}
			byte[] array2 = new byte[num];
			for (int j = 1; j < num; j++)
			{
				array2[j] = (byte)(array[j] ^ array[j - 1]);
			}
			return array2;
		}
	}
}
