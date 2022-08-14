using System;
using System.Diagnostics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.Gps;

public static class CodeM
{
	private const int codeLength = 409200;

	private static readonly sbyte[] signedCode;

	public static sbyte[] SignedCode
	{
		[DebuggerStepThrough]
		get
		{
			return signedCode;
		}
	}

	static CodeM()
	{
		signedCode = new sbyte[409200];
		Random threadRandom = RandomProvider.ThreadRandom;
		for (int i = 0; i < 409200; i += 4)
		{
			int num = threadRandom.Next(2);
			num <<= 1;
			num--;
			signedCode[i] = (sbyte)num;
			signedCode[i + 1] = (sbyte)(-num);
			signedCode[i + 2] = (sbyte)num;
			signedCode[i + 3] = (sbyte)(-num);
		}
	}
}
