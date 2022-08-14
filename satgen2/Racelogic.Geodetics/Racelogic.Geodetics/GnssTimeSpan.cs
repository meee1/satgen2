using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Racelogic.Geodetics;

[JsonObject(MemberSerialization.OptIn)]
[DebuggerDisplay("TimeSpan:{TimeSpan}  Seconds:{Seconds}")]
public readonly struct GnssTimeSpan : IComparable<GnssTimeSpan>, IComparable, IEquatable<GnssTimeSpan>, IFormattable
{
	[JsonProperty(PropertyName = "Nanoseconds")]
	public readonly long Nanoseconds;

	public double Seconds
	{
		[DebuggerStepThrough]
		get
		{
			return (double)Nanoseconds * 1E-09;
		}
	}

	public decimal SecondsDecimal
	{
		[DebuggerStepThrough]
		get
		{
			return (decimal)Nanoseconds * 0.000000001m;
		}
	}

	public TimeSpan TimeSpan
	{
		[DebuggerStepThrough]
		get
		{
			return TimeSpan.FromTicks(Nanoseconds / 100);
		}
	}

	public bool IsEmpty
	{
		[DebuggerStepThrough]
		get
		{
			return Nanoseconds == 0;
		}
	}

	[JsonConstructor]
	public GnssTimeSpan(long nanoseconds)
	{
		Nanoseconds = nanoseconds;
	}

	public GnssTimeSpan(TimeSpan timeSpan)
	{
		Nanoseconds = timeSpan.Ticks * 100;
	}

	public static GnssTimeSpan FromSeconds(uint seconds)
	{
		return new GnssTimeSpan(1000000000L * (long)seconds);
	}

	public static GnssTimeSpan FromSeconds(int seconds)
	{
		return new GnssTimeSpan(1000000000L * (long)seconds);
	}

	public static GnssTimeSpan FromSeconds(double seconds)
	{
		return new GnssTimeSpan((long)(1000000000.0 * seconds));
	}

	public static GnssTimeSpan FromSeconds(decimal seconds)
	{
		return new GnssTimeSpan((long)(1000000000m * seconds));
	}

	public static GnssTimeSpan FromMinutes(uint minutes)
	{
		return new GnssTimeSpan(60000000000L * (long)minutes);
	}

	public static GnssTimeSpan FromMinutes(int minutes)
	{
		return new GnssTimeSpan(60000000000L * minutes);
	}

	public static GnssTimeSpan FromMinutes(double minutes)
	{
		return new GnssTimeSpan((long)(60000000000.0 * minutes));
	}

	public static GnssTimeSpan FromHours(uint hours)
	{
		return new GnssTimeSpan(3600000000000L * (long)hours);
	}

	public static GnssTimeSpan FromHours(int hours)
	{
		return new GnssTimeSpan(3600000000000L * hours);
	}

	public static GnssTimeSpan FromHours(double hours)
	{
		return new GnssTimeSpan((long)(3600000000000.0 * hours));
	}

	public static GnssTimeSpan FromDays(uint days)
	{
		return new GnssTimeSpan(86400000000000L * (long)days);
	}

	public static GnssTimeSpan FromDays(int days)
	{
		return new GnssTimeSpan(86400000000000L * days);
	}

	public static GnssTimeSpan FromDays(double days)
	{
		return new GnssTimeSpan((long)(86400000000000.0 * days));
	}

	public static GnssTimeSpan operator +(GnssTimeSpan left, GnssTimeSpan right)
	{
		return new GnssTimeSpan(left.Nanoseconds + right.Nanoseconds);
	}

	public static GnssTimeSpan operator -(GnssTimeSpan left, GnssTimeSpan right)
	{
		return new GnssTimeSpan(left.Nanoseconds - right.Nanoseconds);
	}

	public static GnssTimeSpan operator *(long multiplier, GnssTimeSpan timeSpan)
	{
		return new GnssTimeSpan(multiplier * timeSpan.Nanoseconds);
	}

	public static GnssTimeSpan operator *(int multiplier, GnssTimeSpan timeSpan)
	{
		return new GnssTimeSpan(multiplier * timeSpan.Nanoseconds);
	}

	public static GnssTimeSpan operator *(double multiplier, GnssTimeSpan timeSpan)
	{
		return new GnssTimeSpan((long)((decimal)multiplier * (decimal)timeSpan.Nanoseconds));
	}

	public static GnssTimeSpan operator *(decimal multiplier, GnssTimeSpan timeSpan)
	{
		return new GnssTimeSpan((long)(multiplier * (decimal)timeSpan.Nanoseconds));
	}

	public static GnssTimeSpan operator *(GnssTimeSpan timeSpan, long multiplier)
	{
		return new GnssTimeSpan(multiplier * timeSpan.Nanoseconds);
	}

	public static GnssTimeSpan operator *(GnssTimeSpan timeSpan, int multiplier)
	{
		return new GnssTimeSpan(multiplier * timeSpan.Nanoseconds);
	}

	public static GnssTimeSpan operator *(GnssTimeSpan timeSpan, double multiplier)
	{
		return new GnssTimeSpan((long)((decimal)multiplier * (decimal)timeSpan.Nanoseconds));
	}

	public static GnssTimeSpan operator *(GnssTimeSpan timeSpan, decimal multiplier)
	{
		return new GnssTimeSpan((long)(multiplier * (decimal)timeSpan.Nanoseconds));
	}

	public static GnssTimeSpan operator /(GnssTimeSpan timeSpan, double divisor)
	{
		return new GnssTimeSpan((long)((decimal)timeSpan.Nanoseconds / (decimal)divisor));
	}

	public bool Equals(GnssTimeSpan other)
	{
		return Nanoseconds == other.Nanoseconds;
	}

	public override bool Equals(object obj)
	{
		if (obj is GnssTimeSpan gnssTimeSpan)
		{
			return Nanoseconds == gnssTimeSpan.Nanoseconds;
		}
		return false;
	}

	public override int GetHashCode()
	{
		long nanoseconds = Nanoseconds;
		return nanoseconds.GetHashCode();
	}

	public int CompareTo(GnssTimeSpan other)
	{
		if (Nanoseconds > other.Nanoseconds)
		{
			return 1;
		}
		if (Nanoseconds < other.Nanoseconds)
		{
			return -1;
		}
		return 0;
	}

	public int CompareTo(object obj)
	{
		if (!(obj is GnssTimeSpan other))
		{
			return -1;
		}
		return CompareTo(other);
	}

	public static bool operator <(GnssTimeSpan left, GnssTimeSpan right)
	{
		return left.Nanoseconds < right.Nanoseconds;
	}

	public static bool operator <=(GnssTimeSpan left, GnssTimeSpan right)
	{
		return left.Nanoseconds <= right.Nanoseconds;
	}

	public static bool operator >(GnssTimeSpan left, GnssTimeSpan right)
	{
		return left.Nanoseconds > right.Nanoseconds;
	}

	public static bool operator >=(GnssTimeSpan left, GnssTimeSpan right)
	{
		return left.Nanoseconds >= right.Nanoseconds;
	}

	public static bool operator ==(GnssTimeSpan left, GnssTimeSpan right)
	{
		return left.Nanoseconds == right.Nanoseconds;
	}

	public static bool operator !=(GnssTimeSpan left, GnssTimeSpan right)
	{
		return left.Nanoseconds != right.Nanoseconds;
	}

	public override string ToString()
	{
		return TimeSpan.FromSeconds(Seconds).ToString();
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		return TimeSpan.FromSeconds(Seconds).ToString(format, formatProvider);
	}
}
