using System.Diagnostics;

namespace Racelogic.Gnss.SatGen
{
	internal readonly struct ModulationSignal
	{
		private readonly sbyte[] modulation;

		private readonly int offset;

		public sbyte[] Modulation
		{
			[DebuggerStepThrough]
			get
			{
				return modulation;
			}
		}

		public int Offset
		{
			[DebuggerStepThrough]
			get
			{
				return offset;
			}
		}

		public ModulationSignal(sbyte[] modulation, in int offset)
		{
			this.modulation = modulation;
			this.offset = offset;
		}
	}
}
