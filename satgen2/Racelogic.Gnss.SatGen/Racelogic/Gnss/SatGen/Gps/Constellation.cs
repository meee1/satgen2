using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.Gps;

namespace Racelogic.Gnss.SatGen.Gps
{
	public sealed class Constellation : ConstellationBase
	{
		private static readonly double relativisticEccentricityFactorSpeedOfLight = (Datum.SqrtGM + Datum.SqrtGM) / 299792458.0;

		private const int lockTimeout = 10000;

		private readonly SyncLock navigationDataLibraryLockCA = new SyncLock("L1CA/L1P/L2P Navigation Data Library Lock", 10000);

		private readonly SyncLock modulationSignalLibraryLockP = new SyncLock("L1P/L2P Modulation Signal Library Lock", 10000);

		private readonly SyncLock modulationSignalLibraryLockM = new SyncLock("L1M/L2M Modulation Signal Library Lock", 10000);

		public static Datum Datum
		{
			[DebuggerStepThrough]
			get
			{
				return Racelogic.Geodetics.Datum.WGS84;
			}
		}

		internal Constellation()
			: base(ConstellationType.Gps, Datum)
		{
		}

		internal sealed override ModulationSignal GetModulation(ModulationBank modulationBank, Signal signal, IEnumerable<Observation> satObservations, in Range<GnssTime, GnssTimeSpan> sliceInterval)
		{
			Almanac almanac = base.Almanac as Almanac;
			if (almanac == null)
			{
				throw new InvalidOperationException("Uninitialized Almanac when calling GetModulation()");
			}
			SignalType signalType = signal.SignalType;
			int bitRate;
			switch (signalType)
			{
			case SignalType.GpsL1CA:
			case SignalType.GpsL1P:
			case SignalType.GpsL2P:
				bitRate = NavigationDataL1CA.Info.BitRate;
				break;
			case SignalType.GpsL2C:
				bitRate = NavigationDataL2C.Info.BitRate;
				break;
			case SignalType.GpsL5I:
			case SignalType.GpsL5Q:
				bitRate = NavigationDataL5I.Info.BitRate;
				break;
			case SignalType.GpsL1M:
			case SignalType.GpsL2M:
				bitRate = signal.ModulationRate / CodeM.SignedCode.Length;
				break;
			default:
				bitRate = NavigationData.MinBitRate;
				break;
			}
			Range<double> signalTravelTime = GetSignalTravelTime(satObservations, in bitRate);
			int offset = (int)Math.Round((double)signal.ModulationRate * signalTravelTime.Max);
			Range<GnssTime, GnssTimeSpan> interval = new Range<GnssTime, GnssTimeSpan>(sliceInterval.Start - GnssTimeSpan.FromSeconds(signalTravelTime.Max), sliceInterval.End - GnssTimeSpan.FromSeconds(signalTravelTime.Min));
			int satIndex = satObservations.First().Satellite.Index;
			sbyte[] modulation;
			switch (signalType)
			{
			case SignalType.GpsL1M:
			case SignalType.GpsL2M:
			{
				double intervalLength = interval.Width.Seconds;
				modulation = GetOrCreateModulationSignalM(modulationBank, signal, in intervalLength);
				break;
			}
			case SignalType.GpsL5Q:
			{
				int bitRate2 = NavigationDataL5I.Info.BitRate;
				sbyte[] chipCode4 = CodeL5.SignedCodesQ5[satIndex];
				sbyte[] negatedChipCode4 = CodeL5.NegatedSignedCodesQ5[satIndex];
				byte[] nH2 = Code.NH20;
				double intervalLength = interval.Width.Seconds;
				decimal secondOfWeek = interval.Start.GpsSecondOfWeekDecimal;
				GnssTime timeStamp = sliceInterval.Start;
				modulation = new ModulationTiered(modulationBank, signal, in bitRate2, chipCode4, negatedChipCode4, nH2, in intervalLength, in secondOfWeek, in timeStamp).Modulate();
				break;
			}
			case SignalType.GpsL1CA:
			{
				byte[] orCreateNavigationDataCA = GetOrCreateNavigationDataCA(modulationBank, in satIndex, almanac, in interval);
				sbyte[] chipCode2 = CodeL1CA.SignedCodes[satIndex];
				sbyte[] negatedChipCode2 = CodeL1CA.NegatedSignedCodes[satIndex];
				int bitRate2 = NavigationDataL1CA.Info.BitRate;
				double intervalLength = interval.Width.Seconds;
				GnssTime timeStamp = sliceInterval.Start;
				modulation = new ModulationBPSK(modulationBank, signal, orCreateNavigationDataCA, in bitRate2, chipCode2, negatedChipCode2, in intervalLength, in timeStamp).Modulate();
				break;
			}
			case SignalType.GpsL1P:
			case SignalType.GpsL2P:
			{
				GnssTime timeStamp = sliceInterval.Start;
				modulation = GetOrCreateModulationSignalP(modulationBank, signal, in satIndex, in interval, in timeStamp, almanac);
				break;
			}
			case SignalType.GpsL2C:
			{
				byte[] data2 = new NavigationDataL2C(in satIndex, almanac, in interval, base.Signals).Generate();
				sbyte[] cmCode = CodeL2CM.SignedCodes[satIndex];
				sbyte[] negatedCmCode = CodeL2CM.NegatedSignedCodes[satIndex];
				sbyte[] clCode = CodeL2CL.SignedCodes[satIndex];
				int bitRate2 = NavigationDataL2C.Info.BitRate;
				GnssTime timeStamp = sliceInterval.Start;
				modulation = new ModulationCMCL(modulationBank, signal, data2, in bitRate2, cmCode, negatedCmCode, clCode, in interval, in timeStamp).Modulate();
				break;
			}
			case SignalType.GpsL5I:
			{
				byte[] data = new NavigationDataL5I(in satIndex, almanac, in interval, base.Signals).Generate();
				int bitRate2 = NavigationDataL5I.Info.BitRate;
				sbyte[] chipCode3 = CodeL5.SignedCodesI5[satIndex];
				sbyte[] negatedChipCode3 = CodeL5.NegatedSignedCodesI5[satIndex];
				byte[] nH = Code.NH10;
				byte[] negatedNH = Code.NegatedNH10;
				double intervalLength = interval.Width.Seconds;
				GnssTime timeStamp = sliceInterval.Start;
				modulation = new ModulationTiered(modulationBank, signal, data, in bitRate2, chipCode3, negatedChipCode3, nH, negatedNH, in intervalLength, in timeStamp).Modulate();
				break;
			}
			default:
				throw new NotSupportedException(string.Format("Unsupported signal type {0} in {1}.{2}.{3}()", signal.SignalType, "Gps", "Constellation", "GetModulation"));
			}
			return new ModulationSignal(modulation, in offset);
		}

		private byte[] GetOrCreateNavigationDataCA(ModulationBank modulationBank, in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> modulationInterval)
		{
			bool flag = false;
			byte[][] value;
			using (navigationDataLibraryLockCA.Lock())
			{
				if (modulationBank.NavigationDataLibraryCA.TryGetValue(modulationInterval, out value))
				{
					byte[] array = value[satIndex];
					if (array == null)
					{
						value[satIndex] = Array.Empty<byte>();
					}
					else
					{
						if (array.Length != 0)
						{
							return array;
						}
						flag = true;
					}
				}
				else
				{
					value = new byte[50][];
					modulationBank.NavigationDataLibraryCA.Add(modulationInterval, value);
					value[satIndex] = Array.Empty<byte>();
				}
			}
			if (flag)
			{
				while (value[satIndex].Length == 0)
				{
					Thread.Yield();
				}
				return value[satIndex];
			}
			byte[] array2 = new NavigationDataL1CA(in satIndex, almanac, in modulationInterval, base.Signals).Generate();
			value[satIndex] = array2;
			return array2;
		}

		private sbyte[] GetOrCreateModulationSignalP(ModulationBank modulationBank, Signal signal, in int satIndex, in Range<GnssTime, GnssTimeSpan> modulationInterval, in GnssTime timeStamp, Almanac almanac)
		{
			bool flag = false;
			sbyte[][] value;
			using (modulationSignalLibraryLockP.Lock())
			{
				if (modulationBank.ModulationSignalLibraryP.TryGetValue(modulationInterval, out value))
				{
					sbyte[] array = value[satIndex];
					if (array == null)
					{
						value[satIndex] = Array.Empty<sbyte>();
					}
					else
					{
						if (array.Length != 0)
						{
							return array;
						}
						flag = true;
					}
				}
				else
				{
					value = new sbyte[50][];
					modulationBank.ModulationSignalLibraryP.Add(modulationInterval, value);
					value[satIndex] = Array.Empty<sbyte>();
				}
			}
			if (flag)
			{
				while (value[satIndex].Length == 0)
				{
					Thread.Yield();
				}
				return value[satIndex];
			}
			byte[] orCreateNavigationDataCA = GetOrCreateNavigationDataCA(modulationBank, in satIndex, almanac, in modulationInterval);
			int bitRate = NavigationDataL1CA.Info.BitRate;
			sbyte[] array2 = new ModulationP(modulationBank, signal, in satIndex, orCreateNavigationDataCA, in bitRate, in modulationInterval, in timeStamp).Modulate();
			value[satIndex] = array2;
			return array2;
		}

		private sbyte[] GetOrCreateModulationSignalM(ModulationBank modulationBank, Signal signal, in double intervalLength)
		{
			using (modulationSignalLibraryLockM.Lock())
			{
				sbyte[] array4 = modulationBank.MCode;
				if (array4 == null)
				{
					double intervalLength2 = intervalLength;
					GnssTime timeStamp = GnssTime.MinValue;
					sbyte[] array3 = (modulationBank.MCode = new ModulationM(modulationBank, signal, intervalLength2, in timeStamp).Modulate());
					array4 = array3;
				}
				return array4;
			}
		}

		private protected sealed override IReadOnlyList<IReadOnlyList<Observation>> GetObservations(IReadOnlyList<Pvt> trajectorySliceSamples, SimulationParams simulationParameters)
		{
			IEnumerable<SatelliteBase> almanacSatellites = from s in ((base.Almanac as Almanac) ?? throw new InvalidOperationException("Uninitialized Almanac when calling GetObservations()")).BaselineSatellites
				where s != null
				select (s);
			double? elevationMask = simulationParameters.ElevationMask;
			return CreateObservables(trajectorySliceSamples, almanacSatellites, in elevationMask);
		}

		public sealed override Observation? Observe(SatelliteBase satellite, in Pvt observer, in double? elevationMask, in bool makeSignalObservations = true)
		{
			Satellite satellite2 = satellite as Satellite;
			if (satellite2 == null)
			{
				throw new ArgumentException("satellite is not a Gps satellite", "satellite");
			}
			GnssTime time = observer.Time;
			double eccentricAnomaly;
			double trueRange = (satellite2.GetEcef(in time, out eccentricAnomaly) - observer.Ecef).Position.Magnitude();
			(double Pseudorange, double DopplerVelocity, Vector3D LineOfSight) tuple = EarthRotation.SagnacCorrection(satellite2, in observer, in trueRange);
			double item = tuple.Pseudorange;
			double dopplerVelocity = tuple.DopplerVelocity;
			Vector3D position = tuple.LineOfSight;
			Geodetic referenceLocation = observer.Ecef.ToGeodetic();
			Topocentric azimuthElevation = new Ecef(in position, isAbsolute: false).ToNed(in referenceLocation).ToAzimuthElevation();
			if (azimuthElevation.Elevation < elevationMask)
			{
				return null;
			}
			item += GetRelativisticEccentricityDelay(satellite2, in eccentricAnomaly);
			item += Unb3M.GetTroposphericDelay(in referenceLocation.Latitude, in referenceLocation.Altitude, time.UtcTime.DayOfYear, azimuthElevation.Elevation);
			Dictionary<SignalType, SignalObservation> signalObservations = (makeSignalObservations ? MakeSignalObservations(satellite2, in time, in referenceLocation, in azimuthElevation, in item, in dopplerVelocity) : new Dictionary<SignalType, SignalObservation>(0));
			return new Observation(satellite2, in observer, in position, in azimuthElevation, signalObservations);
		}

		private static double GetRelativisticEccentricityDelay(Satellite gpsSat, in double eccentricAnomaly)
		{
			return relativisticEccentricityFactorSpeedOfLight * gpsSat.Eccentricity * gpsSat.SqrtA * Math.Sin(eccentricAnomaly);
		}

		public sealed override bool LoadAlmanac(Stream stream, in GnssTime simulationTime, string? extension = null)
		{
			string text = extension?.ToUpperInvariant();
			if (text != null)
			{
				_ = text == ".ALM";
			}
			base.Almanac = Racelogic.Gnss.SatGen.Gps.Almanac.LoadYuma(stream, in simulationTime);
			if (base.Almanac!.OriginalSatellites.Any())
			{
				foreach (Satellite item in from s in base.Almanac!.OriginalSatellites.Concat<SatelliteBase>(base.Almanac!.BaselineSatellites)
					where s != null
					select (s))
				{
					item.Af0 = 0.0;
					item.Af1 = 0.0;
					item.Af2 = 0.0;
				}
				return true;
			}
			return false;
		}
	}
}
