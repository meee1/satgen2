using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Racelogic.Gnss.SatGen
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal readonly struct LabSat2HeaderSection
	{
		private readonly short sectionId;

		private readonly int length;

		private readonly byte[] payload;

		internal const int HeaderSectionHeaderLength = 6;

		internal short SectionId
		{
			[DebuggerStepThrough]
			get
			{
				return sectionId;
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

		internal byte[] Payload
		{
			[DebuggerStepThrough]
			get
			{
				return payload;
			}
		}

		internal LabSat2HeaderSection(in short sectionId, in int length, byte[] payload)
		{
			this.sectionId = sectionId;
			this.length = length;
			this.payload = payload;
		}

		internal byte[]? ToBytes()
		{
			if (Payload == null || Payload.Length == 0)
			{
				return null;
			}
			byte[] array = new byte[Length + 6];
			array[0] = (byte)SectionId;
			array[1] = (byte)(SectionId >> 8);
			array[2] = (byte)Length;
			array[3] = (byte)(Length >> 8);
			array[4] = (byte)(Length >> 16);
			array[5] = (byte)(Length >> 24);
			Array.Copy(Payload, 0, array, 6, Length);
			return array;
		}
	}
}
