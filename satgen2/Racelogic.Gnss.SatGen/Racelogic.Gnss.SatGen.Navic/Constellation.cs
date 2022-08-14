using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.Navic;

namespace Racelogic.Gnss.SatGen.Navic;

public sealed class Constellation : ConstellationBase
{
	private static readonly double relativisticEccentricityFactorSpeedOfLight = (Datum.SqrtGM + Datum.SqrtGM) / 299792458.0;

	private const int lockTimeout = 10000;

	private readonly SyncLock navigationDataLibraryLockSPS = new SyncLock("L5sps/Ssps Navigation Data Library Lock", 10000);

	public static Datum Datum
	{
		[DebuggerStepThrough]
		get
		{
			return Racelogic.Geodetics.Datum.WGS84;
		}
	}

	internal Constellation()
		: base(ConstellationType.Navic, Datum)
	{
	}

	internal sealed override ModulationSignal GetModulation(ModulationBank modulationBank, Signal signal, IEnumerable<Observation> satObservations, in Range<GnssTime, GnssTimeSpan> sliceInterval)
	{
		if (!(base.Almanac is Almanac almanac))
		{
			throw new InvalidOperationException("Uninitialized Almanac when calling GetModulation()");
		}
		SignalType signalType = signal.SignalType;
		int bitRate = ((signalType != SignalType.NavicL5SPS && signalType != SignalType.NavicSSPS) ? NavigationData.MinBitRate : NavigationDataSPS.Info.BitRate);
		Range<double> signalTravelTime = GetSignalTravelTime(satObservations, in bitRate);
		int offset = (int)Math.Round((double)signal.ModulationRate * signalTravelTime.Max);
		Range<GnssTime, GnssTimeSpan> modulationInterval = new Range<GnssTime, GnssTimeSpan>(sliceInterval.Start - GnssTimeSpan.FromSeconds(signalTravelTime.Max), sliceInterval.End - GnssTimeSpan.FromSeconds(signalTravelTime.Min));
		int satIndex = satObservations.First().Satellite.Index;
		sbyte[] modulation;
		switch (signalType)
		{
		case SignalType.NavicL5SPS:
		{
			byte[] orCreateNavigationDataSPS2 = GetOrCreateNavigationDataSPS(modulationBank, in satIndex, almanac, in modulationInterval);
			sbyte[] chipCode2 = CodeL5SPS.SignedCodes[satIndex];
			sbyte[] negatedChipCode2 = CodeL5SPS.NegatedSignedCodes[satIndex];
			int bitRate2 = NavigationDataSPS.Info.BitRate;
			double intervalLength = modulationInterval.Width.Seconds;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationBPSK(modulationBank, signal, orCreateNavigationDataSPS2, in bitRate2, chipCode2, negatedChipCode2, in intervalLength, in timeStamp).Modulate();
			break;
		}
		case SignalType.NavicSSPS:
		{
			byte[] orCreateNavigationDataSPS = GetOrCreateNavigationDataSPS(modulationBank, in satIndex, almanac, in modulationInterval);
			sbyte[] chipCode = CodeSSPS.SignedCodes[satIndex];
			sbyte[] negatedChipCode = CodeSSPS.NegatedSignedCodes[satIndex];
			int bitRate2 = NavigationDataSPS.Info.BitRate;
			double intervalLength = modulationInterval.Width.Seconds;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationBPSK(modulationBank, signal, orCreateNavigationDataSPS, in bitRate2, chipCode, negatedChipCode, in intervalLength, in timeStamp).Modulate();
			break;
		}
		default:
			throw new NotSupportedException(string.Format("Unsupported signal type {0} in {1}.{2}.{3}()", signal.SignalType, "Navic", "Constellation", "GetModulation"));
		}
		return new ModulationSignal(modulation, in offset);
	}

	private byte[] GetOrCreateNavigationDataSPS(ModulationBank modulationBank, in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> modulationInterval)
	{
		bool flag = false;
		byte[][] value;
		using (navigationDataLibraryLockSPS.Lock())
		{
			if (modulationBank.NavigationDataLibrarySPS.TryGetValue(modulationInterval, out value))
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
				modulationBank.NavigationDataLibrarySPS.Add(modulationInterval, value);
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
		byte[] array2 = new NavigationDataSPS(in satIndex, almanac, in modulationInterval, base.Signals).Generate();
		value[satIndex] = array2;
		return array2;
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
		if (!(satellite is Satellite satellite2))
		{
			throw new ArgumentException("satellite is not a Navic satellite", "satellite");
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

	private static double GetRelativisticEccentricityDelay(Satellite navSat, in double eccentricAnomaly)
	{
		return relativisticEccentricityFactorSpeedOfLight * navSat.Eccentricity * navSat.SqrtA * Math.Sin(eccentricAnomaly);
	}

	public sealed override bool LoadAlmanac(Stream stream, in GnssTime simulationTime, string? extension = null)
	{
		string text = extension?.ToUpperInvariant();
		if (text != null)
		{
			_ = text == ".ALM";
		}
		base.Almanac = Racelogic.Gnss.SatGen.Navic.Almanac.LoadYuma(stream, in simulationTime);
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
