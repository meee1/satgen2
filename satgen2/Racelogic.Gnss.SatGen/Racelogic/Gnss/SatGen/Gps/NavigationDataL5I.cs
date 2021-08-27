using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Gps
{
	internal sealed class NavigationDataL5I : NavigationDataCNav
	{
		private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GpsL5);

		private static readonly SignalType signalType = Signal.AllSignals.First((Signal s) => s.NavigationDataInfos.Any((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GpsL5)).SignalType;

		private static readonly FixedSizeDictionary<int, int?[]> fecLibrary = new FixedSizeDictionary<int, int?[]>(5);

		private const int lockTimeout = 10000;

		private static readonly SyncLock fecLock = new SyncLock("L5 FEC dictionary lock", 10000);

		private readonly int firstPacketFecState;

		private static readonly int packetLengthSeconds = 300 / dataInfo.DataRate;

		public static NavigationDataInfo Info
		{
			[DebuggerStepThrough]
			get
			{
				return dataInfo;
			}
		}

		protected override SignalType SignalType
		{
			[DebuggerStepThrough]
			get
			{
				return signalType;
			}
		}

		public override int BitRate
		{
			[DebuggerStepThrough]
			get
			{
				return dataInfo.BitRate;
			}
		}

		protected override int FirstPacketFecState
		{
			[DebuggerStepThrough]
			get
			{
				return firstPacketFecState;
			}
		}

		protected override FixedSizeDictionary<int, int?[]> FecLibrary
		{
			[DebuggerStepThrough]
			get
			{
				return fecLibrary;
			}
		}

		protected override SyncLock FecLock
		{
			[DebuggerStepThrough]
			get
			{
				return fecLock;
			}
		}

		public NavigationDataL5I(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> signals)
			: base(in satIndex, almanac, in interval, signals, in packetLengthSeconds)
		{
			GnssTime time = interval.Start;
			int firstPacketIndex = GetPacketIndex(in time);
			int? num = ReadFEC(in firstPacketIndex);
			while (!num.HasValue)
			{
				Thread.Sleep(2);
				num = ReadFEC(in firstPacketIndex);
			}
			firstPacketFecState = num.Value;
		}

		private static int GetPacketIndex(in GnssTime time)
		{
			return time.GpsSecondOfWeek / packetLengthSeconds;
		}

		internal static void FinalizeFEC(in GnssTime intervalEnd)
		{
			int packetIndex = GetPacketIndex(in intervalEnd);
			using (fecLock.Lock())
			{
				if (fecLibrary.TryGetValue(packetIndex, out var value))
				{
					for (int i = 0; i < value.Length; i++)
					{
						ref int? reference = ref value[i];
						int valueOrDefault = reference.GetValueOrDefault();
						if (!reference.HasValue)
						{
							valueOrDefault = 0;
							reference = valueOrDefault;
						}
					}
				}
				else if (fecLibrary.Any())
				{
					RLLogger.GetLogger().LogMessage($"ERROR: No L5 FEC library entry found for the interval ending on GPS week {intervalEnd.GpsWeek} and second {intervalEnd.GpsSecondOfWeekDecimal} (index of data packet (within a week) for the FEC entry: {packetIndex})");
				}
			}
		}

		internal static void ClearFEC()
		{
			fecLibrary.Clear();
		}
	}
}
