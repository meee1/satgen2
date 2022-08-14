using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

public abstract class Trajectory : IDisposable
{
	private Range<GnssTime, GnssTimeSpan> interval;

	private int sampleRate = 1;

	private decimal samplePeriod = 1.0m;

	private GnssTimeSpan sampleSpan = GnssTimeSpan.FromSeconds(1);

	private string? errorMessage;

	private bool isExternal;

	public virtual Range<GnssTime, GnssTimeSpan> Interval
	{
		[DebuggerStepThrough]
		get
		{
			return interval;
		}
		[DebuggerStepThrough]
		protected set
		{
			interval = value;
		}
	}

	public int SampleRate
	{
		[DebuggerStepThrough]
		get
		{
			return sampleRate;
		}
		[DebuggerStepThrough]
		protected set
		{
			sampleRate = value;
			samplePeriod = 1.0m / (decimal)value;
			sampleSpan = GnssTimeSpan.FromSeconds(samplePeriod);
		}
	}

	public decimal SamplePeriod
	{
		[DebuggerStepThrough]
		get
		{
			return samplePeriod;
		}
		[DebuggerStepThrough]
		protected set
		{
			samplePeriod = value;
			sampleSpan = GnssTimeSpan.FromSeconds(value);
			sampleRate = (int)Math.Round(1.0m / value);
		}
	}

	public GnssTimeSpan SampleSpan
	{
		[DebuggerStepThrough]
		get
		{
			return sampleSpan;
		}
	}

	public string? ErrorMessage
	{
		[DebuggerStepThrough]
		get
		{
			return errorMessage;
		}
		[DebuggerStepThrough]
		[param: DisallowNull]
		protected set
		{
			errorMessage = value;
		}
	}

	public bool IsExternal
	{
		[DebuggerStepThrough]
		get
		{
			return isExternal;
		}
		[DebuggerStepThrough]
		protected set
		{
			isExternal = value;
		}
	}

	public event EventHandler<EventArgs<GnssTime>>? NewSample;

	public abstract IReadOnlyList<Pvt>? GetSamples(in Range<GnssTime, GnssTimeSpan> interval);

	protected Pvt ExtrapolateLinear(in Pvt firstSample, in Pvt secondSample, in int sampleIndex)
	{
		Vector3D position = secondSample.Ecef.Position;
		Vector3D position2 = firstSample.Ecef.Position;
		Vector3D vector3D;
		GnssTime time;
		if (sampleIndex >= 0)
		{
			vector3D = position;
			time = secondSample.Time;
		}
		else
		{
			vector3D = position2;
			time = firstSample.Time;
		}
		decimal num = SamplePeriod * (decimal)sampleIndex;
		Vector3D velocity = SampleRate * (position - position2);
		Vector3D position3 = vector3D + velocity * (double)num;
		Ecef ecef = new Ecef(in position3, in velocity);
		GnssTime time2 = time + GnssTimeSpan.FromSeconds(num);
		return new Pvt(in time2, in ecef);
	}

	protected void OnNewSample(object sender, EventArgs<GnssTime> args)
	{
		this.NewSample?.Invoke(sender, args);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}
