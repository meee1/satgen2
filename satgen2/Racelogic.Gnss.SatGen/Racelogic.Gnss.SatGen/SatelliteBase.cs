using System.Diagnostics;
using Racelogic.DataTypes;
using Racelogic.Geodetics;

namespace Racelogic.Gnss.SatGen;

[DebuggerDisplay("ID={Id} {ConstellationType} {OrbitType} {IsEnabled ? string.Empty : \"Disabled\", nq}")]
public abstract class SatelliteBase
{
	public int Id
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public int Index
	{
		[DebuggerStepThrough]
		get
		{
			return Id - 1;
		}
	}

	public bool IsHealthy
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	} = true;


	public bool IsEnabled
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	} = true;


	internal Range<GnssTime, GnssTimeSpan> TransmissionInterval
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		set;
	}

	public abstract Datum Datum
	{
		[DebuggerStepThrough]
		get;
	}

	public abstract ConstellationType ConstellationType
	{
		[DebuggerStepThrough]
		get;
	}

	public OrbitType OrbitType
	{
		[DebuggerStepThrough]
		get;
		[DebuggerStepThrough]
		internal set;
	}

	public abstract Ecef GetEcef(in GnssTime time, out double eccentricAnomaly);

	public SatelliteBase Clone()
	{
		return (SatelliteBase)MemberwiseClone();
	}
}
