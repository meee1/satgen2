using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen
{
	public abstract class AlmanacBase
	{
		private const int lockTimeout = 10000;

		private const int ephemerisLibraryCapacity = 4;

		private readonly SyncLock ephemerisLibraryLock = new SyncLock("EphemerisLibraryLock", 10000);

		private readonly FixedSizeDictionary<GnssTime, SatelliteBase>[] ephemerisLibrary = new FixedSizeDictionary<GnssTime, SatelliteBase>[50];

		private const int almanacLibraryCapacity = 2;

		private readonly SyncLock almanacLibraryLock = new SyncLock("AlmanacLibraryLock", 10000);

		private readonly FixedSizeDictionary<GnssTime, SatelliteBase>[] almanacLibrary = new FixedSizeDictionary<GnssTime, SatelliteBase>[50];

		protected static readonly double SqrtAThreshold = Math.Pow(1176621286912439.0, 0.25);

		protected const double InclinationThreshold = Math.PI / 18.0;

		public IReadOnlyList<SatelliteBase?> OriginalSatellites
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			protected set;
		} = Array.Empty<SatelliteBase>();


		public IReadOnlyList<SatelliteBase?> BaselineSatellites
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			protected set;
		} = Array.Empty<SatelliteBase>();


		public string? RawAlmanac
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			[param: DisallowNull]
			protected set;
		}

		public string? FilePath
		{
			[DebuggerStepThrough]
			get;
			[DebuggerStepThrough]
			[param: DisallowNull]
			set;
		}

		internal SatelliteBase GetEphemeris(in int satIndex, SignalType signalType, in GnssTime transmissionTime)
		{
			GnssTime referenceTime = GetTimeOfEphemeris(in transmissionTime, signalType);
			using (ephemerisLibraryLock.Lock())
			{
				FixedSizeDictionary<GnssTime, SatelliteBase> fixedSizeDictionary = ephemerisLibrary[satIndex];
				if (fixedSizeDictionary == null)
				{
					fixedSizeDictionary = new FixedSizeDictionary<GnssTime, SatelliteBase>(4);
					ephemerisLibrary[satIndex] = fixedSizeDictionary;
				}
				if (!fixedSizeDictionary.TryGetValue(referenceTime, out var value))
				{
					SatelliteBase satelliteBase = BaselineSatellites[satIndex];
					if (satelliteBase == null)
					{
						throw new InvalidOperationException($"Missing almanac for sat ID {satIndex + 1}.  Signal: {signalType.ToShortName()}");
					}
					value = CreateEphemeris(in transmissionTime, in referenceTime, satelliteBase);
					fixedSizeDictionary.Add(referenceTime, value);
				}
				return value;
			}
		}

		private protected abstract GnssTime GetEphemerisIntervalStart(in GnssTime transmissionTime);

		private protected abstract GnssTime GetTimeOfEphemeris(in GnssTime transmissionTime, SignalType signalType);

		private protected abstract SatelliteBase CreateEphemeris(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat);

		internal SatelliteBase? GetAlmanac(in int satIndex, in GnssTime transmissionTime)
		{
			GnssTime referenceTime = GetTimeOfAlmanac(in transmissionTime, in satIndex);
			using (almanacLibraryLock.Lock())
			{
				FixedSizeDictionary<GnssTime, SatelliteBase> fixedSizeDictionary = almanacLibrary[satIndex];
				if (fixedSizeDictionary == null)
				{
					fixedSizeDictionary = new FixedSizeDictionary<GnssTime, SatelliteBase>(2);
					almanacLibrary[satIndex] = fixedSizeDictionary;
				}
				if (!fixedSizeDictionary.TryGetValue(referenceTime, out var value))
				{
					SatelliteBase satelliteBase = BaselineSatellites[satIndex];
					if (satelliteBase == null)
					{
						return null;
					}
					value = CreateAlmanac(in transmissionTime, in referenceTime, satelliteBase);
					fixedSizeDictionary.Add(referenceTime, value);
				}
				return value;
			}
		}

		internal abstract GnssTime GetTimeOfAlmanac(in GnssTime transmissionTime, in int satIndex = 0);

		private protected abstract SatelliteBase CreateAlmanac(in GnssTime transmissionTime, in GnssTime referenceTime, SatelliteBase baselineSat);

		public abstract void UpdateAlmanacForTime(in GnssTime simulationTime);
	}
}
