using System.Diagnostics;
using System.Linq;
using Racelogic.Utilities;

namespace Racelogic.Gnss.BeiDou;

public static class CodeB3I
{
	private const int registerSize = 13;

	private static readonly uint[] g1Seed;

	private static readonly uint[][] g2Seeds;

	private const int codeLength = 10230;

	private const int g1Period = 8190;

	private static readonly int[] g1Taps;

	private static readonly int[] g2Taps;

	private static readonly int satCount;

	private static readonly sbyte[][] signedCodes;

	private static readonly sbyte[][] negatedSignedCodes;

	public static sbyte[][] SignedCodes
	{
		[DebuggerStepThrough]
		get
		{
			return signedCodes;
		}
	}

	public static sbyte[][] NegatedSignedCodes
	{
		[DebuggerStepThrough]
		get
		{
			return negatedSignedCodes;
		}
	}

	static CodeB3I()
	{
		g1Seed = new uint[13]
		{
			1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u,
			1u, 1u, 1u
		}.Reverse().ToArray(13);
		g2Seeds = new uint[37][]
		{
			new uint[13]
			{
				1u, 0u, 1u, 0u, 1u, 1u, 1u, 1u, 1u, 1u,
				1u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 1u, 1u, 0u, 0u, 0u, 1u, 0u, 1u,
				0u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 1u, 1u, 1u, 1u, 0u, 0u, 0u, 1u,
				0u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u,
				0u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 0u, 0u, 1u, 0u, 0u, 0u, 1u, 1u,
				1u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 0u, 1u, 0u, 0u, 1u, 1u, 0u, 0u,
				1u, 0u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 1u, 1u, 1u, 1u, 1u, 0u, 1u, 0u,
				0u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 1u, 0u, 1u, 1u, 1u, 1u, 1u, 1u,
				1u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 1u, 0u, 0u, 0u, 0u, 0u, 0u, 0u,
				0u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 1u, 0u, 0u, 0u, 0u, 0u, 1u, 1u,
				0u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 1u, 0u, 1u, 0u, 1u, 1u, 1u, 0u,
				0u, 0u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 1u, 0u, 1u, 1u, 0u, 0u, 1u, 1u,
				1u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 1u, 1u, 0u, 0u, 1u, 0u, 0u, 1u, 0u,
				1u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 1u, 1u, 1u, 0u, 0u, 0u, 1u, 0u, 0u,
				1u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 0u, 0u, 1u, 1u, 0u, 0u, 0u, 1u,
				0u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 1u, 0u, 0u, 0u, 1u, 1u, 1u, 1u,
				1u, 0u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 1u, 0u, 0u, 1u, 1u, 0u, 0u, 0u,
				1u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 0u, 0u, 0u, 1u, 1u, 1u, 0u, 1u,
				1u, 0u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 0u, 0u, 1u, 0u, 1u, 0u, 1u, 0u,
				1u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 0u, 1u, 0u, 1u, 1u, 0u, 1u, 1u,
				1u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 1u, 0u, 0u, 0u, 0u, 1u, 0u, 1u,
				1u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 1u, 0u, 1u, 1u, 0u, 0u, 0u, 1u,
				0u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 0u, 1u, 0u, 1u, 1u, 0u, 0u, 1u,
				1u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 1u, 1u, 0u, 0u, 1u, 1u, 0u, 0u,
				0u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 1u, 1u, 1u, 0u, 1u, 0u, 0u, 1u,
				0u, 0u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 1u, 0u, 0u, 1u, 0u, 0u, 1u, 0u, 1u,
				0u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 1u, 1u, 0u, 1u, 1u, 0u, 1u, 0u,
				0u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 1u, 0u, 1u, 1u, 1u, 1u, 0u, 0u,
				0u, 1u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 0u, 1u, 0u, 1u, 1u, 1u, 1u, 0u,
				1u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u, 1u,
				1u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 1u, 1u, 0u, 1u, 1u, 0u, 0u, 0u, 1u,
				1u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 1u, 0u, 1u, 1u, 0u, 0u, 0u, 1u,
				0u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 0u, 0u, 1u, 0u, 1u, 0u, 1u, 0u, 1u,
				0u, 1u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 0u, 0u, 1u, 1u, 0u, 1u, 0u, 0u,
				1u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 0u, 1u, 0u, 0u, 1u, 0u, 1u, 1u,
				1u, 0u, 1u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				1u, 1u, 1u, 1u, 1u, 0u, 1u, 1u, 1u, 0u,
				1u, 0u, 0u
			}.Reverse().ToArray(13),
			new uint[13]
			{
				0u, 0u, 1u, 0u, 1u, 0u, 1u, 1u, 0u, 0u,
				1u, 1u, 1u
			}.Reverse().ToArray(13)
		};
		g1Taps = new int[4] { 1, 3, 4, 13 };
		g2Taps = new int[8] { 1, 5, 6, 7, 9, 10, 12, 13 };
		satCount = g2Seeds.Length;
		signedCodes = new sbyte[satCount][];
		negatedSignedCodes = new sbyte[satCount][];
		byte[] array = new byte[10230];
		int num = 0;
		foreach (byte item in FibonacciShiftRegister.Generate(g1Seed, g1Taps, 8190))
		{
			array[num++] = item;
		}
		foreach (byte item2 in FibonacciShiftRegister.Generate(g1Seed, g1Taps, 2040))
		{
			array[num++] = item2;
		}
		for (int i = 0; i < satCount; i++)
		{
			signedCodes[i] = new sbyte[10230];
			negatedSignedCodes[i] = new sbyte[10230];
		}
		for (int j = 0; j < satCount; j++)
		{
			sbyte[] array2 = signedCodes[j];
			sbyte[] array3 = negatedSignedCodes[j];
			int num2 = 0;
			foreach (byte item3 in FibonacciShiftRegister.Generate(g2Seeds[j], g2Taps, 10230))
			{
				int num3 = array[num2] ^ item3;
				num3 <<= 1;
				num3--;
				array2[num2] = (sbyte)num3;
				array3[num2] = (sbyte)(-num3);
				num2++;
			}
		}
	}
}
