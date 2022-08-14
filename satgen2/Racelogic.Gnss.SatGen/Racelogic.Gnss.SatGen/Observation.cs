using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

public sealed class Observation
{
	public readonly SatelliteBase Satellite;

	public readonly Pvt ObservedFrom;

	public readonly Vector3D LineOfSight;

	public readonly Topocentric AzimuthElevation;

	public readonly ReadOnlyDictionary<SignalType, SignalObservation> SignalObservations;

	public GnssTime Timestamp
	{
		[DebuggerStepThrough]
		get
		{
			return ObservedFrom.Time;
		}
	}

	public Observation(SatelliteBase satellite, in Pvt observer, in Vector3D lineOfSight, in Topocentric azimuthElevation, IDictionary<SignalType, SignalObservation> signalObservations)
	{
		Satellite = satellite;
		ObservedFrom = observer;
		LineOfSight = lineOfSight;
		AzimuthElevation = azimuthElevation;
		SignalObservations = new ReadOnlyDictionary<SignalType, SignalObservation>(signalObservations);
	}
}
