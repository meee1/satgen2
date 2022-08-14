using System.Diagnostics;
using Newtonsoft.Json;

namespace Racelogic.Geodetics;

[JsonObject(MemberSerialization.OptIn)]
[DebuggerDisplay("UTC:{Time.UtcTime}")]
public readonly struct Pvt
{
	[JsonProperty(PropertyName = "Time")]
	public readonly GnssTime Time;

	[JsonProperty(PropertyName = "Ecef")]
	public readonly Ecef Ecef;

	[JsonConstructor]
	public Pvt(in GnssTime time, in Ecef ecef)
	{
		Time = time;
		Ecef = ecef;
	}

	public static bool operator ==(Pvt left, Pvt right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Pvt left, Pvt right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(Pvt))
		{
			return false;
		}
		return Equals((Pvt)obj);
	}

	public bool Equals(Pvt other)
	{
		if (other.Ecef.Equals(Ecef))
		{
			return other.Time.Equals(Time);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (5993773 + Ecef.GetHashCode()) * 9973 + Time.GetHashCode();
	}
}
