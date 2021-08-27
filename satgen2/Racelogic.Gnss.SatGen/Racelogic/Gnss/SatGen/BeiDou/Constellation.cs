using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.BeiDou;

namespace Racelogic.Gnss.SatGen.BeiDou
{
	public sealed class Constellation : ConstellationBase
	{
		private static readonly double relativisticEccentricityFactorSpeedOfLight = (Datum.SqrtGM + Datum.SqrtGM) / 299792458.0;

		private const int lockTimeout = 10000;

		private readonly SyncLock modulationSignalLibraryLockBI = new SyncLock("BeiDou B1-I/B2-I Modulation Signal Library Lock", 10000);

		public const double ReferencePlaneTilt = Math.PI / 36.0;

		public static Datum Datum
		{
			[DebuggerStepThrough]
			get
			{
				return Racelogic.Geodetics.Datum.CGCS2000;
			}
		}

		internal Constellation()
			: base(ConstellationType.BeiDou, Datum)
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
			int modulationRate = signal.ModulationRate;
			SatelliteBase satellite = satObservations.First().Satellite;
			int bitRate = ((signalType != SignalType.BeiDouB1I && signalType != SignalType.BeiDouB2I && signalType != SignalType.BeiDouB3I) ? NavigationData.MinBitRate : ((satellite.OrbitType != OrbitType.GEO) ? NavigationDataB1ID1.Info.BitRate : NavigationDataB1ID2.Info.BitRate));
			Range<double> signalTravelTime = GetSignalTravelTime(satObservations, in bitRate);
			int offset = (int)Math.Round((double)modulationRate * signalTravelTime.Max);
			Range<GnssTime, GnssTimeSpan> modulationInterval = new Range<GnssTime, GnssTimeSpan>(sliceInterval.Start - GnssTimeSpan.FromSeconds(signalTravelTime.Max), sliceInterval.End - GnssTimeSpan.FromSeconds(signalTravelTime.Min));
			sbyte[] modulation;
			if (signal.SignalType == SignalType.BeiDouB1I || signal.SignalType == SignalType.BeiDouB2I)
			{
				GnssTime timeStamp = sliceInterval.Start;
				modulation = GetOrCreateModulationSignalBI(modulationBank, signal, satellite, in modulationInterval, in timeStamp, almanac);
			}
			else
			{
				if (signal.SignalType != SignalType.BeiDouB3I)
				{
					throw new NotSupportedException("Unsupported signal type " + signal.SignalType.ToShortName() + " in BeiDou.Constellation.GetModulation()");
				}
				Modulation modulation2;
				if (satellite.OrbitType == OrbitType.GEO)
				{
					int satIndex = satellite.Index;
					byte[] data = new NavigationDataB1ID2(in satIndex, almanac, in modulationInterval, base.Signals).Generate();
					satIndex = NavigationDataB1ID2.Info.BitRate;
					sbyte[] chipCode = CodeB3I.SignedCodes[satellite.Index];
					sbyte[] negatedChipCode = CodeB3I.NegatedSignedCodes[satellite.Index];
					double intervalLength = modulationInterval.Width.Seconds;
					GnssTime timeStamp = sliceInterval.Start;
					modulation2 = new ModulationBPSK(modulationBank, signal, data, in satIndex, chipCode, negatedChipCode, in intervalLength, in timeStamp);
				}
				else
				{
					int satIndex = satellite.Index;
					byte[] data2 = new NavigationDataB1ID1(in satIndex, almanac, in modulationInterval, base.Signals).Generate();
					satIndex = NavigationDataB1ID1.Info.BitRate;
					sbyte[] chipCode2 = CodeB3I.SignedCodes[satellite.Index];
					sbyte[] negatedChipCode2 = CodeB3I.NegatedSignedCodes[satellite.Index];
					byte[] nH = Code.NH20;
					byte[] negatedNH = Code.NegatedNH20;
					double intervalLength = modulationInterval.Width.Seconds;
					GnssTime timeStamp = sliceInterval.Start;
					modulation2 = new ModulationTiered(modulationBank, signal, data2, in satIndex, chipCode2, negatedChipCode2, nH, negatedNH, in intervalLength, in timeStamp);
				}
				modulation = modulation2.Modulate();
			}
			return new ModulationSignal(modulation, in offset);
		}

		private sbyte[] GetOrCreateModulationSignalBI(ModulationBank modulationBank, Signal signal, SatelliteBase sat, in Range<GnssTime, GnssTimeSpan> modulationInterval, in GnssTime timeStamp, Almanac almanac)
		{
			bool flag = false;
			sbyte[][] value;
			using (modulationSignalLibraryLockBI.Lock())
			{
				if (modulationBank.ModulationSignalLibraryBI.TryGetValue(modulationInterval, out value))
				{
					sbyte[] array = value[sat.Index];
					if (array == null)
					{
						value[sat.Index] = Array.Empty<sbyte>();
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
					modulationBank.ModulationSignalLibraryBI.Add(modulationInterval, value);
					value[sat.Index] = Array.Empty<sbyte>();
				}
			}
			if (flag)
			{
				while (value[sat.Index].Length == 0)
				{
					Thread.Yield();
				}
				return value[sat.Index];
			}
			double intervalLength = modulationInterval.Width.Seconds;
			Modulation modulation;
			if (sat.OrbitType == OrbitType.GEO)
			{
				int satIndex = sat.Index;
				byte[] data = new NavigationDataB1ID2(in satIndex, almanac, in modulationInterval, base.Signals).Generate();
				satIndex = NavigationDataB1ID2.Info.BitRate;
				modulation = new ModulationBPSK(modulationBank, signal, data, in satIndex, CodeB1I.SignedCodes[sat.Index], CodeB1I.NegatedSignedCodes[sat.Index], in intervalLength, in timeStamp);
			}
			else
			{
				int satIndex = sat.Index;
				byte[] data2 = new NavigationDataB1ID1(in satIndex, almanac, in modulationInterval, base.Signals).Generate();
				satIndex = NavigationDataB1ID1.Info.BitRate;
				modulation = new ModulationTiered(modulationBank, signal, data2, in satIndex, CodeB1I.SignedCodes[sat.Index], CodeB1I.NegatedSignedCodes[sat.Index], Code.NH20, Code.NegatedNH20, in intervalLength, in timeStamp);
			}
			sbyte[] array2 = modulation.Modulate();
			value[sat.Index] = array2;
			return array2;
		}

		private protected sealed override IReadOnlyList<IReadOnlyList<Observation>> GetObservations(IReadOnlyList<Pvt> trajectorySliceSamples, SimulationParams simulationParameters)
		{
			SimulationParams simulationParameters2 = simulationParameters;
			Almanac almanac = base.Almanac as Almanac;
			if (almanac == null)
			{
				throw new InvalidOperationException("Uninitialized Almanac when calling GetObservations()");
			}
			IEnumerable<SatelliteBase> almanacSatellites = ((!base.Signals.Any((Signal s) => simulationParameters2.Output.ChannelPlan.SampleRate % s.ModulationRate == 0)) ? (from s in almanac.BaselineSatellites
				where s != null
				select (s)) : (from s in almanac.BaselineSatellites
				where s != null && s.OrbitType != OrbitType.GEO
				select (s)));
			double? elevationMask = simulationParameters2.ElevationMask;
			return CreateObservables(trajectorySliceSamples, almanacSatellites, in elevationMask);
		}

		public sealed override Observation? Observe(SatelliteBase satellite, in Pvt observer, in double? elevationMask, in bool makeSignalObservations = true)
		{
			Satellite satellite2 = satellite as Satellite;
			if (satellite2 == null)
			{
				throw new ArgumentException("satellite is not a BeiDou satellite", "satellite");
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

		private static double GetRelativisticEccentricityDelay(Satellite beiDouSat, in double eccentricAnomaly)
		{
			return relativisticEccentricityFactorSpeedOfLight * beiDouSat.Eccentricity * beiDouSat.SqrtA * Math.Sin(eccentricAnomaly);
		}

		public sealed override bool LoadAlmanac(Stream stream, in GnssTime simulationTime, string? extension = null)
		{
			string text = extension?.ToUpperInvariant();
			if (text != null)
			{
				_ = text == ".ALM";
			}
			base.Almanac = Racelogic.Gnss.SatGen.BeiDou.Almanac.LoadYuma(stream, in simulationTime);
			if (base.Almanac!.OriginalSatellites.Any())
			{
				foreach (Satellite item in from s in base.Almanac!.OriginalSatellites.Concat<SatelliteBase>(base.Almanac!.BaselineSatellites)
					where s != null
					select (s))
				{
					item.A0 = 0.0;
					item.A1 = 0.0;
					item.A2 = 0.0;
				}
				return true;
			}
			return false;
		}
	}
}
