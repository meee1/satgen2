using System;
using System.Runtime.Serialization;
using Racelogic.Geodetics;

namespace Racelogic.DataSource;

[Obsolete("This struct is about to be removed.  Please use LocalTangentPlane defined in Racelogic.Geodetic.")]
[DataContract]
public struct LocalTangentPlane : IEquatable<LocalTangentPlane>
{
	[DataMember]
	private readonly double down;

	[DataMember]
	private readonly double east;

	[DataMember]
	private readonly double north;

	public double Down => down;

	public double East => east;

	public double North => north;

	public LocalTangentPlane(double north, double east, double down)
	{
		this.north = north;
		this.east = east;
		this.down = down;
	}

	public static implicit operator Racelogic.Geodetics.LocalTangentPlane(LocalTangentPlane ltp)
	{
		return new Racelogic.Geodetics.LocalTangentPlane(ltp.North, ltp.East, ltp.Down);
	}

	public static implicit operator LocalTangentPlane(Racelogic.Geodetics.LocalTangentPlane ltp)
	{
		return new LocalTangentPlane(ltp.North, ltp.East, ltp.Down);
	}

	public static bool operator ==(LocalTangentPlane left, LocalTangentPlane right)
	{
		return left.Equals(right);
	}

	public static LocalTangentPlane operator +(LocalTangentPlane left, LocalTangentPlane right)
	{
		return new LocalTangentPlane(left.North + right.North, left.East + right.East, left.Down + right.Down);
	}

	public static LocalTangentPlane operator -(LocalTangentPlane left, LocalTangentPlane right)
	{
		return new LocalTangentPlane(left.North - right.North, left.East - right.East, left.Down - right.Down);
	}

	public static bool operator !=(LocalTangentPlane left, LocalTangentPlane right)
	{
		return !left.Equals(right);
	}

	public bool Equals(LocalTangentPlane other)
	{
		if (other.North.Equals(North) && other.East.Equals(East))
		{
			return other.Down.Equals(Down);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(LocalTangentPlane))
		{
			return false;
		}
		return Equals((LocalTangentPlane)obj);
	}

	public override int GetHashCode()
	{
		return (((North.GetHashCode() * 397) ^ East.GetHashCode()) * 397) ^ Down.GetHashCode();
	}

	public override string ToString()
	{
		return $"North: {north}m   East: {east}m   Down: {down}m";
	}

	public Topocentric ToAzimuthElevation()
	{
		double num = Math.Atan2(East, North);
		if (num < 0.0)
		{
			num += Math.PI * 2.0;
		}
		double x = Math.Sqrt(north * north + east * east);
		double elevation = Math.Atan2(0.0 - Down, x);
		return new Topocentric(num, elevation);
	}

	[Obsolete("ToEcef is deprecated, please use ToEcef2 instead.")]
	public Ecef ToEcef(Geodetic reference)
	{
		return new Ecef(reference.ToEcef().Position + new Vector3D(0.0 - North * Math.Sin(reference.Latitude) - East * Math.Sin(reference.Longitude) * Math.Cos(reference.Latitude) - Math.Cos(reference.Latitude) * Math.Cos(reference.Longitude) * Down, Math.Cos(reference.Longitude) * East - Math.Sin(reference.Longitude) * Down, Math.Cos(reference.Latitude) * North - Math.Sin(reference.Longitude) * Math.Sin(reference.Latitude) * East - Math.Sin(reference.Latitude) * Math.Cos(reference.Longitude) * Down));
	}

	public Ecef ToEcef2(Geodetic reference)
	{
		double num = Math.Sin(reference.Longitude);
		double num2 = Math.Sqrt(1.0 - num * num);
		double num3 = reference.Longitude % Ecef.TwoPI;
		if (num3 > 0.0)
		{
			if (num3 > Ecef.PIByTwo && num3 < Ecef.ThreePIByTwo)
			{
				num2 = 0.0 - num2;
			}
		}
		else if (num3 < 0.0 - Ecef.PIByTwo && num3 > 0.0 - Ecef.ThreePIByTwo)
		{
			num2 = 0.0 - num2;
		}
		double num4 = Math.Sin(reference.Latitude);
		double num5 = Math.Sqrt(1.0 - num4 * num4);
		num3 = reference.Latitude % Ecef.TwoPI;
		if (num3 > 0.0)
		{
			if (num3 > Ecef.PIByTwo && num3 < Ecef.ThreePIByTwo)
			{
				num5 = 0.0 - num5;
			}
		}
		else if (num3 < 0.0 - Ecef.PIByTwo && num3 > 0.0 - Ecef.ThreePIByTwo)
		{
			num5 = 0.0 - num5;
		}
		return new Ecef(reference.ToEcef2().Position + new Vector3D(0.0 - North * num4 - East * num * num5 - num5 * num2 * Down, num2 * East - num * Down, num5 * North - num * num4 * East - num4 * num2 * Down));
	}

	public double Distance2DFrom(LocalTangentPlane other)
	{
		return Math.Sqrt((north - other.North) * (north - other.North) + (east - other.East) * (east - other.East));
	}

	public double DistanceFrom(LocalTangentPlane other)
	{
		return Math.Sqrt((north - other.North) * (north - other.North) + (east - other.East) * (east - other.East) + (down - other.Down) * (down - other.Down));
	}

	public double HeadingFrom(LocalTangentPlane fromPoint)
	{
		double num = east - fromPoint.east;
		double num2 = north - fromPoint.north;
		if (num == 0.0 && num2 > 0.0)
		{
			return 0.0;
		}
		if (num == 0.0 && num2 < 0.0)
		{
			return 180.0;
		}
		if (num2 == 0.0 && num < 0.0)
		{
			return 270.0;
		}
		if (num2 == 0.0 && num > 0.0)
		{
			return 90.0;
		}
		double num3 = Math.Atan(Math.Abs(num / num2)) / AngleConstants.DegreeToRadians;
		if (num > 0.0 && num2 > 0.0)
		{
			return num3;
		}
		if (num < 0.0 && num2 > 0.0)
		{
			return 360.0 - num3;
		}
		if (num < 0.0 && num2 < 0.0)
		{
			return 180.0 + num3;
		}
		return 180.0 - num3;
	}
}
