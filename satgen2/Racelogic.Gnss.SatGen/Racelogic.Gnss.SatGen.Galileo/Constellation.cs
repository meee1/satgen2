using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Gnss.Galileo;

namespace Racelogic.Gnss.SatGen.Galileo;

public sealed class Constellation : ConstellationBase
{
	private static readonly double relativisticEccentricityFactorSpeedOfLight = (Datum.SqrtGM + Datum.SqrtGM) / 299792458.0;

	public static Datum Datum
	{
		[DebuggerStepThrough]
		get
		{
			return Racelogic.Geodetics.Datum.GTRF;
		}
	}

	internal Constellation()
		: base(ConstellationType.Galileo, Datum)
	{
	}

	internal sealed override ModulationSignal GetModulation(ModulationBank modulationBank, Signal signal, IEnumerable<Observation> satObservations, in Range<GnssTime, GnssTimeSpan> sliceInterval)
	{
		if (!(base.Almanac is Almanac almanac))
		{
			throw new InvalidOperationException("Uninitialized Almanac when calling GetModulation()");
		}
		SignalType signalType = signal.SignalType;
		int bitRate;
		switch (signalType)
		{
		case SignalType.GalileoE1BC:
			bitRate = NavigationDataE1B.Info.BitRate;
			break;
		case SignalType.GalileoE5aI:
		case SignalType.GalileoE5aQ:
		case SignalType.GalileoE5AltBocI:
		case SignalType.GalileoE5AltBocQ:
			bitRate = NavigationDataE5a.Info.BitRate;
			break;
		case SignalType.GalileoE5bI:
		case SignalType.GalileoE5bQ:
			bitRate = NavigationDataE5b.Info.BitRate;
			break;
		case SignalType.GalileoE6BC:
			bitRate = NavigationDataE6B.Info.BitRate;
			break;
		default:
			bitRate = NavigationData.MinBitRate;
			break;
		}
		Range<double> signalTravelTime = GetSignalTravelTime(satObservations, in bitRate);
		int offset = (int)Math.Round((double)signal.ModulationRate * signalTravelTime.Max);
		Range<GnssTime, GnssTimeSpan> interval = new Range<GnssTime, GnssTimeSpan>(sliceInterval.Start - GnssTimeSpan.FromSeconds(signalTravelTime.Max), sliceInterval.End - GnssTimeSpan.FromSeconds(signalTravelTime.Min));
		double intervalLength = interval.Width.Seconds;
		int satIndex = satObservations.First().Satellite.Index;
		Modulation modulation;
		switch (signalType)
		{
		case SignalType.GalileoE1BC:
		{
			byte[] data4 = new NavigationDataE1B(in satIndex, almanac, in interval, base.Signals).Generate();
			sbyte[][] modulationSequences4 = CodeE1BC.ModulationCodes[satIndex];
			byte[] secondaryCode10 = CodeE1C.SecondaryCode;
			decimal secondOfWeek = interval.Start.GalileoNavicSecondOfWeekDecimal;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationCBOC(modulationBank, signal, data4, in bitRate, modulationSequences4, secondaryCode10, in intervalLength, in secondOfWeek, in timeStamp);
			break;
		}
		case SignalType.GalileoE6BC:
		{
			byte[] data3 = new NavigationDataE6B(in satIndex, almanac, in interval, base.Signals).Generate();
			sbyte[][] modulationSequences3 = CodeE6BC.ModulationCodes[satIndex];
			byte[] secondaryCode9 = CodeE6C.SecondaryCodes[satIndex];
			decimal secondOfWeek = interval.Start.GalileoNavicSecondOfWeekDecimal;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationCBOC(modulationBank, signal, data3, in bitRate, modulationSequences3, secondaryCode9, in intervalLength, in secondOfWeek, in timeStamp);
			break;
		}
		case SignalType.GalileoE5aI:
		{
			byte[] data2 = new NavigationDataE5a(in satIndex, almanac, in interval, base.Signals).Generate();
			sbyte[] chipCode4 = CodeE5aI.PrimarySignedCodes[satIndex];
			sbyte[] negatedChipCode4 = CodeE5aI.NegatedPrimarySignedCodes[satIndex];
			byte[] secondaryCode8 = CodeE5aI.SecondaryCode;
			byte[] negatedSecondaryCode2 = CodeE5aI.NegatedSecondaryCode;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationTiered(modulationBank, signal, data2, in bitRate, chipCode4, negatedChipCode4, secondaryCode8, negatedSecondaryCode2, in intervalLength, in timeStamp);
			break;
		}
		case SignalType.GalileoE5bI:
		{
			byte[] data = new NavigationDataE5b(in satIndex, almanac, in interval, base.Signals).Generate();
			sbyte[] chipCode3 = CodeE5bI.PrimarySignedCodes[satIndex];
			sbyte[] negatedChipCode3 = CodeE5bI.NegatedPrimarySignedCodes[satIndex];
			byte[] secondaryCode7 = CodeE5bI.SecondaryCode;
			byte[] negatedSecondaryCode = CodeE5bI.NegatedSecondaryCode;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationTiered(modulationBank, signal, data, in bitRate, chipCode3, negatedChipCode3, secondaryCode7, negatedSecondaryCode, in intervalLength, in timeStamp);
			break;
		}
		case SignalType.GalileoE5aQ:
		{
			sbyte[] chipCode2 = CodeE5aQ.PrimarySignedCodes[satIndex];
			sbyte[] negatedChipCode2 = CodeE5aQ.NegatedPrimarySignedCodes[satIndex];
			byte[] secondaryCode6 = CodeE5aQ.SecondaryCodes[satIndex];
			decimal secondOfWeek = interval.Start.GalileoNavicSecondOfWeekDecimal;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationTiered(modulationBank, signal, in bitRate, chipCode2, negatedChipCode2, secondaryCode6, in intervalLength, in secondOfWeek, in timeStamp);
			break;
		}
		case SignalType.GalileoE5bQ:
		{
			sbyte[] chipCode = CodeE5bQ.PrimarySignedCodes[satIndex];
			sbyte[] negatedChipCode = CodeE5bQ.NegatedPrimarySignedCodes[satIndex];
			byte[] secondaryCode5 = CodeE5bQ.SecondaryCodes[satIndex];
			decimal secondOfWeek = interval.Start.GalileoNavicSecondOfWeekDecimal;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationTiered(modulationBank, signal, in bitRate, chipCode, negatedChipCode, secondaryCode5, in intervalLength, in secondOfWeek, in timeStamp);
			break;
		}
		case SignalType.GalileoE5AltBocI:
		{
			byte[] dataE5aI2 = new NavigationDataE5a(in satIndex, almanac, in interval, base.Signals).Generate();
			byte[] dataE5bI2 = new NavigationDataE5b(in satIndex, almanac, in interval, base.Signals).Generate();
			int secondaryRate2 = signal.ChippingRate / CodeE5aI.PrimaryCodes[0].Length;
			int bitRateE5aI = NavigationDataE5a.Info.BitRate;
			int bitRateE5bI = NavigationDataE5b.Info.BitRate;
			sbyte[][] modulationSequences2 = CodeE5AltBOC.ModulationCodesI[satIndex];
			byte[] secondaryCode3 = CodeE5aI.SecondaryCode;
			byte[] secondaryCode4 = CodeE5bI.SecondaryCode;
			byte[] secondaryCodeE5aQ2 = CodeE5aQ.SecondaryCodes[satIndex];
			byte[] secondaryCodeE5bQ2 = CodeE5bQ.SecondaryCodes[satIndex];
			decimal secondOfWeek = interval.Start.GalileoNavicSecondOfWeekDecimal;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationAltBOC(modulationBank, signal, dataE5aI2, dataE5bI2, in bitRateE5aI, in bitRateE5bI, in secondaryRate2, modulationSequences2, secondaryCode3, secondaryCode4, secondaryCodeE5aQ2, secondaryCodeE5bQ2, in intervalLength, in secondOfWeek, in timeStamp);
			break;
		}
		case SignalType.GalileoE5AltBocQ:
		{
			byte[] dataE5aI = new NavigationDataE5a(in satIndex, almanac, in interval, base.Signals).Generate();
			byte[] dataE5bI = new NavigationDataE5b(in satIndex, almanac, in interval, base.Signals).Generate();
			int secondaryRate = signal.ChippingRate / CodeE5aI.PrimaryCodes[0].Length;
			int bitRateE5aI = NavigationDataE5a.Info.BitRate;
			int bitRateE5bI = NavigationDataE5b.Info.BitRate;
			sbyte[][] modulationSequences = CodeE5AltBOC.ModulationCodesQ[satIndex];
			byte[] secondaryCode = CodeE5aI.SecondaryCode;
			byte[] secondaryCode2 = CodeE5bI.SecondaryCode;
			byte[] secondaryCodeE5aQ = CodeE5aQ.SecondaryCodes[satIndex];
			byte[] secondaryCodeE5bQ = CodeE5bQ.SecondaryCodes[satIndex];
			decimal secondOfWeek = interval.Start.GalileoNavicSecondOfWeekDecimal;
			GnssTime timeStamp = sliceInterval.Start;
			modulation = new ModulationAltBOC(modulationBank, signal, dataE5aI, dataE5bI, in bitRateE5aI, in bitRateE5bI, in secondaryRate, modulationSequences, secondaryCode, secondaryCode2, secondaryCodeE5aQ, secondaryCodeE5bQ, in intervalLength, in secondOfWeek, in timeStamp);
			break;
		}
		default:
			throw new NotSupportedException(string.Format("Unsupported signal type {0} in {1}.{2}.{3}()", signal.SignalType, "Galileo", "Constellation", "GetModulation"));
		}
		return new ModulationSignal(modulation.Modulate(), in offset);
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
			throw new ArgumentException("satellite is not a Galileo satellite", "satellite");
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

	private static double GetRelativisticEccentricityDelay(Satellite galileoSat, in double eccentricAnomaly)
	{
		return relativisticEccentricityFactorSpeedOfLight * galileoSat.Eccentricity * galileoSat.SqrtA * Math.Sin(eccentricAnomaly);
	}

	private protected override double GetIonosphericDelay(in GnssTime time, in Geodetic position, in Topocentric azimuthElevation, in double frequency)
	{
		return NeQuickG.GetIonosphericDelay(in time, in position, in azimuthElevation, in frequency);
	}

	public sealed override bool LoadAlmanac(Stream stream, in GnssTime simulationTime, string? extension = null)
	{
		string text = extension?.ToUpperInvariant();
		if (text != null)
		{
			_ = text == ".XML";
		}
		base.Almanac = Racelogic.Gnss.SatGen.Galileo.Almanac.LoadXml(stream, in simulationTime);
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
