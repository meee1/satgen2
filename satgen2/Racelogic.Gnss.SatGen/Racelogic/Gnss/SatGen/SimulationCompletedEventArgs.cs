using System;
using System.Diagnostics;

namespace Racelogic.Gnss.SatGen
{
	public sealed class SimulationCompletedEventArgs : EventArgs
	{
		private readonly bool cancelled;

		public bool Cancelled
		{
			[DebuggerStepThrough]
			get
			{
				return cancelled;
			}
		}

		public SimulationCompletedEventArgs(in bool cancelled)
		{
			this.cancelled = cancelled;
		}
	}
}
