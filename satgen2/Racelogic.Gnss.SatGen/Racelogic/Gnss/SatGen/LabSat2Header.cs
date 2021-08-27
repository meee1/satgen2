using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Racelogic.Gnss.SatGen
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal readonly struct LabSat2Header
	{
		private static readonly byte[] preamble = new byte[8];

		private readonly byte[] id;

		private readonly byte version;

		private readonly int length;

		private readonly LabSat2HeaderSection[] sections;

		internal byte[] Identifier
		{
			[DebuggerStepThrough]
			get
			{
				return id;
			}
		}

		internal byte Version
		{
			[DebuggerStepThrough]
			get
			{
				return version;
			}
		}

		internal int Length
		{
			[DebuggerStepThrough]
			get
			{
				return length;
			}
		}

		internal LabSat2HeaderSection[] Sections
		{
			[DebuggerStepThrough]
			get
			{
				return sections;
			}
		}

		internal LabSat2Header(byte[] identifier, in byte headerVersion, LabSat2HeaderSection[] headerSections)
		{
			id = identifier;
			version = headerVersion;
			sections = headerSections;
			int num = headerSections.Sum((LabSat2HeaderSection section) => section.Length + 6);
			length = num + preamble.Length + identifier.Length + 1 + 4;
		}

		internal byte[] ToBytes()
		{
			byte[] array = new byte[Length];
			int num = 0;
			Array.Copy(preamble, 0, array, num, preamble.Length);
			num += preamble.Length;
			Array.Copy(Identifier, 0, array, num, Identifier.Length);
			num += Identifier.Length;
			array[num++] = Version;
			array[num++] = (byte)array.Length;
			array[num++] = (byte)(array.Length >> 8);
			array[num++] = (byte)(array.Length >> 16);
			array[num++] = (byte)(array.Length >> 24);
			foreach (byte[] item in from section in Sections
				select section.ToBytes() into b
				where b != null && b.Length != 0
				select (b))
			{
				Array.Copy(item, 0, array, num, item.Length);
				num += item.Length;
			}
			return array;
		}
	}
}
