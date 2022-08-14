using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.Glonass;

namespace Racelogic.Gnss.SatGen.Glonass;

public sealed class Constellation : ConstellationBase
{
	private const int lockTimeout = 10000;

	private readonly SyncLock modulationSignalLibraryLockOF = new SyncLock("Glonass L1OF/L2OF Modulation Signal Library Lock", 10000);

	public static Datum Datum
	{
		[DebuggerStepThrough]
		get
		{
			return Racelogic.Geodetics.Datum.PZ90;
		}
	}

	internal Constellation()
		: base(ConstellationType.Glonass, Datum)
	{
	}

	internal sealed override ModulationSignal GetModulation(ModulationBank modulationBank, Signal signal, IEnumerable<Observation> satObservations, in Range<GnssTime, GnssTimeSpan> sliceInterval)
	{
		if (!(base.Almanac is Almanac almanac))
		{
			throw new InvalidOperationException("Uninitialized Almanac when calling GetModulation()");
		}
		SignalType signalType = signal.SignalType;
		int modulationRate = signal.ModulationRate;
		int bitRate = ((signalType != SignalType.GlonassL1OF && signalType != SignalType.GlonassL2OF) ? NavigationData.MinBitRate : NavigationDataL1OF.Info.BitRate);
		Range<double> signalTravelTime = GetSignalTravelTime(satObservations, in bitRate);
		int offset = (int)Math.Round((double)modulationRate * signalTravelTime.Max);
		Range<GnssTime, GnssTimeSpan> modulationInterval = new Range<GnssTime, GnssTimeSpan>(sliceInterval.Start - GnssTimeSpan.FromSeconds(signalTravelTime.Max), sliceInterval.End - GnssTimeSpan.FromSeconds(signalTravelTime.Min));
		int satIndex = satObservations.First().Satellite.Index;
		if (signal.SignalType == SignalType.GlonassL1OF || signal.SignalType == SignalType.GlonassL2OF)
		{
			GnssTime timeStamp = sliceInterval.Start;
			sbyte[] orCreateModulationSignalOF = GetOrCreateModulationSignalOF(modulationBank, signal, in satIndex, in modulationInterval, in timeStamp, almanac);
			return new ModulationSignal(orCreateModulationSignalOF, in offset);
		}
		throw new NotSupportedException(string.Format("Unsupported signal type {0} in {1}.{2}.{3}()", signal.SignalType, "Glonass", "Constellation", "GetModulation"));
	}

	private sbyte[] GetOrCreateModulationSignalOF(ModulationBank modulationBank, Signal signal, in int satIndex, in Range<GnssTime, GnssTimeSpan> modulationInterval, in GnssTime timeStamp, Almanac almanac)
	{
		bool flag = false;
		sbyte[][] value;
		using (modulationSignalLibraryLockOF.Lock())
		{
			if (modulationBank.ModulationSignalLibraryOF.TryGetValue(modulationInterval, out value))
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
				modulationBank.ModulationSignalLibraryOF.Add(modulationInterval, value);
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
		byte[] data = new NavigationDataL1OF(in satIndex, almanac, in modulationInterval, base.Signals).Generate();
		int bitRate = NavigationDataL1OF.Info.BitRate;
		sbyte[] signedCode = CodeL1OF.SignedCode;
		sbyte[] negatedSignedCode = CodeL1OF.NegatedSignedCode;
		double intervalLength = modulationInterval.Width.Seconds;
		sbyte[] array2 = new ModulationBPSK(modulationBank, signal, data, in bitRate, signedCode, negatedSignedCode, in intervalLength, in timeStamp).Modulate();
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
			throw new ArgumentException("satellite is not a Glonass satellite", "satellite");
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
		item += satellite2.GlonassTimeCorrection * 299792458.0;
		item += Unb3M.GetTroposphericDelay(in referenceLocation.Latitude, in referenceLocation.Altitude, time.UtcTime.DayOfYear, azimuthElevation.Elevation);
		Dictionary<SignalType, SignalObservation> signalObservations = (makeSignalObservations ? MakeSignalObservations(satellite2, in time, in referenceLocation, in azimuthElevation, in item, in dopplerVelocity) : new Dictionary<SignalType, SignalObservation>(0));
		return new Observation(satellite2, in observer, in position, in azimuthElevation, signalObservations);
	}

	public sealed override bool LoadAlmanac(Stream stream, in GnssTime simulationTime, string? extension = null)
	{
		string text = extension?.ToUpperInvariant();
		if (text != null && !(text == ".ALM"))
		{
			_ = text == ".AGL";
		}
		base.Almanac = Racelogic.Gnss.SatGen.Glonass.Almanac.LoadAgl(stream);
		if (base.Almanac!.OriginalSatellites.Any())
		{
			return true;
		}
		return false;
	}
}
