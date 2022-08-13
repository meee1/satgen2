using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Racelogic.DataTypes;

namespace Racelogic.Gnss.LabSat;

internal class LS3WFile : IDisposable
{
	private readonly string iniFileName;

	private readonly string ls3wFileName;

	private readonly FileStream fileStream;

	private readonly SyncLock fileStreamLock = new SyncLock("LS3WFile stream lock");

	private readonly int channelCount;

	private readonly int samplingRate;

	private readonly int quantization;

	private readonly int samplesInWord64;

	private readonly int remainingBitsInWord64;

	private static readonly LS3WSamplingMode[] samplingModes;

	private static readonly ulong[] bitMasks;

	private readonly ulong bitMask;

	private readonly int longShift;

	private readonly Memory<byte> buffer = new Memory<byte>(new byte[8]);

	private bool isDisposed;

	public int SamplingRate
	{
		[DebuggerStepThrough]
		get
		{
			return samplingRate;
		}
	}

	public int ChannelCount
	{
		[DebuggerStepThrough]
		get
		{
			return channelCount;
		}
	}

	public int Quantization
	{
		[DebuggerStepThrough]
		get
		{
			return quantization;
		}
	}

	public string IniFileName
	{
		[DebuggerStepThrough]
		get
		{
			return iniFileName;
		}
	}

	public string Ls3wFileName
	{
		[DebuggerStepThrough]
		get
		{
			return ls3wFileName;
		}
	}

	public LS3WFile(string fileName)
	{
		string text = Path.GetExtension(fileName)!.ToUpperInvariant();
		if (text == ".INI")
		{
			iniFileName = fileName;
			ls3wFileName = Path.ChangeExtension(fileName, ".LS3W");
		}
		else
		{
			if (!(text == ".LS3W"))
			{
				throw new ArgumentException("Unknown file type " + text + ". Only *.ini and *.LS3W files are accepted", "fileName");
			}
			ls3wFileName = fileName;
			iniFileName = Path.ChangeExtension(fileName, ".ini");
		}
		foreach (string item in File.ReadLines(iniFileName))
		{
			if (item.StartsWith("SMP="))
			{
				samplingRate = int.Parse(item.Split(new char[1] { '=' })[1]);
			}
			else if (item.StartsWith("QUA="))
			{
				quantization = int.Parse(item.Split(new char[1] { '=' })[1]);
			}
			else if (item.StartsWith("CHN="))
			{
				channelCount = int.Parse(item.Split(new char[1] { '=' })[1]);
			}
		}
		LS3WSamplingMode lS3WSamplingMode = samplingModes.Where((LS3WSamplingMode m) => m.ChannelCount == channelCount && m.Quantization == quantization).First();
		samplesInWord64 = lS3WSamplingMode.SamplesInWord64;
		remainingBitsInWord64 = lS3WSamplingMode.RemainingBitsInWord64;
		bitMask = bitMasks[quantization];
		longShift = 64 - quantization;
		fileStream = File.Open(ls3wFileName, FileMode.Open, FileAccess.Read);
	}

	public IEnumerable<Complex[]> GetSamples()
	{
		using (fileStreamLock.Lock())
		{
			fileStream.Seek(0L, SeekOrigin.Begin);
			while (fileStream.Read(buffer.Span) == 8)
			{
				ulong word66 = MemoryMarshal.Cast<byte, ulong>(buffer.Span)[0];
				word66 <<= remainingBitsInWord64;
				Complex[] samples = new Complex[ChannelCount];
				for (int c = 0; c < samplesInWord64; c++)
				{
					for (int i = 0; i < ChannelCount; i++)
					{
						ulong num = (word66 & bitMask) >> longShift;
						word66 <<= Quantization;
						ulong num2 = (word66 & bitMask) >> longShift;
						word66 <<= Quantization;
						samples[i] = new Complex(num, num2);
					}
					yield return samples;
				}
			}
		}
	}

	public IEnumerable<Complex> GetSamples(LS3WChannel channel)
	{
		using (fileStreamLock.Lock())
		{
			fileStream.Seek(0L, SeekOrigin.Begin);
			while (fileStream.Read(buffer.Span) == 8)
			{
				ulong word66 = MemoryMarshal.Cast<byte, ulong>(buffer.Span)[0];
				int shift = (1 + 2 * (ChannelCount - 1)) * Quantization;
				word66 <<= remainingBitsInWord64 + 2 * (int)channel * Quantization;
				for (int c = 0; c < samplesInWord64; c++)
				{
					ulong num = (word66 & bitMask) >> longShift;
					word66 <<= Quantization;
					ulong num2 = (word66 & bitMask) >> longShift;
					word66 <<= shift;
					yield return new Complex(num, num2);
				}
			}
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing)
			{
				fileStream.Close();
			}
		}
	}

	static LS3WFile()
	{
		LS3WSamplingMode[] array = new LS3WSamplingMode[9];
		int num = 1;
		int num2 = 1;
		int num3 = 32;
		int num4 = 0;
		array[0] = new LS3WSamplingMode(in num, in num2, in num3, in num4);
		int num5 = 2;
		int num6 = 1;
		int num7 = 16;
		int num8 = 0;
		array[1] = new LS3WSamplingMode(in num5, in num6, in num7, in num8);
		int num9 = 1;
		int num10 = 2;
		int num11 = 16;
		int num12 = 0;
		array[2] = new LS3WSamplingMode(in num9, in num10, in num11, in num12);
		int num13 = 3;
		int num14 = 1;
		int num15 = 10;
		int num16 = 4;
		array[3] = new LS3WSamplingMode(in num13, in num14, in num15, in num16);
		int num17 = 1;
		int num18 = 3;
		int num19 = 10;
		int num20 = 4;
		array[4] = new LS3WSamplingMode(in num17, in num18, in num19, in num20);
		int num21 = 2;
		int num22 = 2;
		int num23 = 8;
		int num24 = 0;
		array[5] = new LS3WSamplingMode(in num21, in num22, in num23, in num24);
		int num25 = 3;
		int num26 = 2;
		int num27 = 5;
		int num28 = 4;
		array[6] = new LS3WSamplingMode(in num25, in num26, in num27, in num28);
		int num29 = 2;
		int num30 = 3;
		int num31 = 5;
		int num32 = 4;
		array[7] = new LS3WSamplingMode(in num29, in num30, in num31, in num32);
		int num33 = 3;
		int num34 = 3;
		int num35 = 3;
		int num36 = 10;
		array[8] = new LS3WSamplingMode(in num33, in num34, in num35, in num36);
		samplingModes = array;
		bitMasks = new ulong[4] { 0uL, 9223372036854775808uL, 13835058055282163712uL, 16140901064495857664uL };
	}
}
