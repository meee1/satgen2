using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Racelogic.Geodetics;

[JsonObject(MemberSerialization.OptIn)]
[DebuggerDisplay("X:{Position.X}  Y:{Position.Y}  Z:{Position.Z}  Mag:{Position.Magnitude()}  VX:{Velocity.X}  VY:{Velocity.Y}  VZ:{Velocity.Z}  VMag:{Velocity.Magnitude()}")]
public readonly struct Ecef : IEquatable<Ecef>
{
	[JsonProperty(PropertyName = "Position")]
	public readonly Vector3D Position;

	[JsonProperty(PropertyName = "Velocity")]
	public readonly Vector3D Velocity;

	[JsonProperty(PropertyName = "IsAbsolute")]
	public readonly bool IsAbsolute;

	public Ecef(double positionX, double positionY, double positionZ)
		: this(positionX, positionY, positionZ, isAbsolute: true)
	{
	}

	public Ecef(double positionX, double positionY, double positionZ, bool isAbsolute)
	{
		IsAbsolute = isAbsolute;
		Position = new Vector3D(positionX, positionY, positionZ);
		Velocity = default(Vector3D);
	}

	public Ecef(double positionX, double positionY, double positionZ, double velocityX, double velocityY, double velocityZ)
		: this(positionX, positionY, positionZ, velocityX, velocityY, velocityZ, isAbsolute: true)
	{
	}

	public Ecef(double positionX, double positionY, double positionZ, double velocityX, double velocityY, double velocityZ, bool isAbsolute)
		: this(positionX, positionY, positionZ, isAbsolute)
	{
		Velocity = new Vector3D(velocityX, velocityY, velocityZ);
	}

	public Ecef(in Vector3D position)
		: this(in position, isAbsolute: true)
	{
	}

	public Ecef(in Vector3D position, bool isAbsolute)
	{
		Position = position;
		IsAbsolute = isAbsolute;
		Velocity = default(Vector3D);
	}

	public Ecef(in Vector3D position, in Vector3D velocity)
		: this(in position)
	{
		Velocity = velocity;
	}

	[JsonConstructor]
	public Ecef(in Vector3D position, in Vector3D velocity, bool isAbsolute)
		: this(in position, isAbsolute)
	{
		Velocity = velocity;
	}

	public Geodetic ToGeodetic()
	{
		return ToGeodetic(Datum.WGS84);
	}

	public Geodetic ToGeodetic(Datum datum, Geoid geoid = null)
	{
		if (!IsAbsolute)
		{
			throw new InvalidOperationException("Relative vectors cannot be converted to geodetic coordinates");
		}
		(double X, double Y, double Z) tuple = Position.Deconstruct();
		double item = tuple.X;
		double item2 = tuple.Y;
		double item3 = tuple.Z;
		double longitude = Math.Atan2(item2, item);
		double firstEccentricitySquared = datum.FirstEccentricitySquared;
		double semiMajorAxis = datum.SemiMajorAxis;
		double num = semiMajorAxis * firstEccentricitySquared;
		double num2 = num * firstEccentricitySquared * 0.5;
		double num3 = item3 * item3;
		double num4 = item * item + item2 * item2;
		double num5 = Math.Sqrt(num4);
		double num6 = 1.0 / (num4 + num3);
		double num7 = Math.Sqrt(num6);
		double num8 = num * num * num7;
		double num9 = num4 * num6;
		double num10;
		double num12;
		double num13;
		double num11;
		if (num9 > 0.3)
		{
			num10 = Math.Abs(item3) * (num7 + num6 * num9 * (num + num8 + num3 * num6 * (num2 - 2.5 * num8)));
			num11 = Math.Asin(num10);
			num12 = num10 * num10;
			num13 = Math.Sqrt(1.0 - num12);
		}
		else
		{
			num13 = num5 * (num7 - num6 * num6 * num3 * (num + num2 - num8 - num9 * (num2 - 2.5 * num8)));
			num11 = Math.Acos(num13);
			num12 = 1.0 - num13 * num13;
			num10 = Math.Sqrt(num12);
		}
		double num14 = 1.0 / (1.0 - firstEccentricitySquared * num12);
		double num15 = semiMajorAxis * Math.Sqrt(num14);
		double num16 = (1.0 - firstEccentricitySquared) * num15;
		double num17 = num5 - num15 * num13;
		double num18 = Math.Abs(item3) - num16 * num10;
		double num19 = num13 * num17 + num10 * num18;
		double num20 = num13 * num18 - num10 * num17;
		double num21 = num20 / (num16 * num14 + num19);
		num11 += num21;
		if (item3 < 0.0)
		{
			num11 = 0.0 - num11;
		}
		double altitude = num19 + num20 * num21 * 0.5;
		Geodetic position = new Geodetic(num11, longitude, altitude);
		if (geoid != null)
		{
			double separation = geoid.GetSeparation(in position);
			return position.SetAltitude(position.Altitude - separation);
		}
		return position;
	}

	public LocalTangentPlane ToNed(in Geodetic referenceLocation, in Ecef referenceEcef)
	{
		if (!IsAbsolute)
		{
			throw new InvalidOperationException("Relative Ecefs cannot be converted to geodetic coordinates using this ToNed override");
		}
		if (!referenceEcef.IsAbsolute)
		{
			throw new ArgumentException("Relative Ecefs cannot be used as reference origin", "referenceEcef");
		}
		return (this - referenceEcef).ToNed(in referenceLocation);
	}

	public LocalTangentPlane ToNed(in Geodetic referenceLocation, Datum datum = null, Geoid geoid = null)
	{
		if (IsAbsolute)
		{
			Ecef ecef = referenceLocation.ToEcef(datum, geoid);
			return (this - ecef).ToNed(in referenceLocation);
		}
		double num = Math.Cos(referenceLocation.Longitude);
		double num2 = Math.Sin(referenceLocation.Longitude);
		double num3 = Math.Cos(referenceLocation.Latitude);
		double num4 = Math.Sin(referenceLocation.Latitude);
		double north = (0.0 - Position.X) * num4 * num - Position.Y * num4 * num2 + Position.Z * num3;
		double east = (0.0 - Position.X) * num2 + Position.Y * num;
		double down = (0.0 - Position.X) * num3 * num - Position.Y * num3 * num2 - Position.Z * num4;
		return new LocalTangentPlane(north, east, down);
	}

	public double DistanceFrom(in Ecef other)
	{
		if (!IsAbsolute || !other.IsAbsolute)
		{
			throw new InvalidOperationException("Cannot calculate distance between relative vectors.");
		}
		return (Position - other.Position).Magnitude();
	}

	public static Ecef FromCoordinates(double latitude, double longitude, double height, double geoidHeight, GravitationalModel gravitationalModel = GravitationalModel.Wgs84)
	{
		return gravitationalModel switch
		{
			GravitationalModel.Egm84 => Geodetic.FromDegrees(latitude, longitude, height).ToEcef(Datum.WGS84, Geoid.Egm84), 
			GravitationalModel.Egm96 => Geodetic.FromDegrees(latitude, longitude, height).ToEcef(Datum.WGS84, Geoid.Egm96), 
			GravitationalModel.Egm2008 => Geodetic.FromDegrees(latitude, longitude, height).ToEcef(Datum.WGS84, Geoid.Egm2008), 
			GravitationalModel.Nmea => Geodetic.FromDegrees(latitude, longitude, height + geoidHeight).ToEcef(), 
			_ => Geodetic.FromDegrees(latitude, longitude, height).ToEcef(), 
		};
	}

	public override string ToString()
	{
		return string.Format("P({0};{1};{2}) V({3};{4};{5}) {6}", Position.X, Position.Y, Position.Z, Velocity.X, Velocity.Y, Velocity.Z, IsAbsolute ? "Absolute" : "Relative");
	}

	public static bool operator ==(Ecef left, Ecef right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Ecef left, Ecef right)
	{
		return !left.Equals(right);
	}

	public static Ecef operator -(Ecef left, Ecef right)
	{
		return Subtract(left, right);
	}

	public static Ecef Subtract(Ecef left, Ecef right)
	{
		Vector3D position = left.Position - right.Position;
		Vector3D velocity = left.Velocity - right.Velocity;
		return new Ecef(in position, in velocity, isAbsolute: false);
	}

	public bool Equals(Ecef other)
	{
		if (other.IsAbsolute == IsAbsolute && other.Position.Equals(Position))
		{
			return other.Velocity.Equals(Velocity);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(Ecef))
		{
			return false;
		}
		return Equals((Ecef)obj);
	}

	public override int GetHashCode()
	{
		int num = ((5993773 + Position.GetHashCode()) * 9973 + Velocity.GetHashCode()) * 9973;
		bool isAbsolute = IsAbsolute;
		return num + isAbsolute.GetHashCode();
	}
}
