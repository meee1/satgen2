using System.Diagnostics;

namespace Racelogic.Gnss.SatGen
{
	internal readonly struct NavigationDataInfo
	{
		private readonly NavigationDataType navigationDataType;

		private readonly int dataRate;

		private readonly int bitRate;

		public NavigationDataType NavigationDataType
		{
			[DebuggerStepThrough]
			get
			{
				return navigationDataType;
			}
		}

		public int DataRate
		{
			[DebuggerStepThrough]
			get
			{
				return dataRate;
			}
		}

		public int BitRate
		{
			[DebuggerStepThrough]
			get
			{
				return bitRate;
			}
		}

		public NavigationDataInfo(NavigationDataType navigationDataType, in int dataRate, in int bitRate)
		{
			this.navigationDataType = navigationDataType;
			this.dataRate = dataRate;
			this.bitRate = bitRate;
		}
	}
}
