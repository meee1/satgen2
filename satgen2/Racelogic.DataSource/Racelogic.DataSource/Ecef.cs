using System;
using Racelogic.Geodetics;

namespace Racelogic.DataSource;

[Obsolete("This struct is about to be removed.  Please use Ecef defined in Racelogic.Geodetic.")]
public struct Ecef : IEquatable<Ecef>
{
	private readonly Vector3D position;

	private readonly Vector3D velocity;

	internal static double PIByTwo = Math.PI / 2.0;

	internal static double ThreePIByTwo = 4.71238898038469;

	internal static double TwoPI = Math.PI * 2.0;

	public Vector3D Position => position;

	public Vector3D Velocity => velocity;

	public Ecef(double positionX, double positionY, double positionZ)
	{
		position = new Vector3D(positionX, positionY, positionZ);
		velocity = default(Vector3D);
	}

	public Ecef(double positionX, double positionY, double positionZ, double velocityX, double velocityY, double velocityZ)
		: this(positionX, positionY, positionZ)
	{
		velocity = new Vector3D(velocityX, velocityY, velocityZ);
	}

	public Ecef(Vector3D position)
	{
		this.position = position;
		velocity = default(Vector3D);
	}

	public Ecef(Vector3D position, Vector3D velocity)
		: this(position)
	{
		this.velocity = velocity;
	}

	public static implicit operator Racelogic.Geodetics.Ecef(Ecef e)
	{
		return new Racelogic.Geodetics.Ecef(e.Position.X, e.position.Y, e.position.Z);
	}

	public static implicit operator Ecef(Racelogic.Geodetics.Ecef e)
	{
		return new Ecef(e.Position.X, e.Position.Y, e.Position.Z);
	}

	public static bool operator ==(Ecef left, Ecef right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(Ecef left, Ecef right)
	{
		return !left.Equals(right);
	}

	public bool Equals(Ecef other)
	{
		if (other.Position.Equals(Position))
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
		return (Position.GetHashCode() * 397) ^ Velocity.GetHashCode();
	}

	public Geodetic ToGeodetic()
	{
		return ToGeodetic(Datum.Wgs84, geoidHeight: true);
	}

	public Geodetic ToGeodetic(Datum datum, bool geoidHeight)
	{
		int num = 1;
		double num2 = 1.0;
		double num3 = Math.Sqrt(Math.Pow(Position.X, 2.0) + Math.Pow(Position.Y, 2.0));
		double num4 = Math.Atan2(Position.Z, num3 * (1.0 - datum.EccentricitySquared));
		double longitude = Math.Atan2(Position.Y, Position.X);
		double latitude = 0.0;
		double num5 = 0.0;
		while (num2 > 1E-14 && num <= 15)
		{
			double num6 = datum.SemiMajorAxis / Math.Sqrt(datum.SemiMajorAxis * Math.Pow(Math.Cos(num4), 2.0) + datum.SemiMinorAxis * Math.Pow(Math.Sin(num4), 2.0));
			num5 = num3 / Math.Cos(num4) - num6;
			double num7 = Math.Atan2(Position.Z, num3 * (1.0 - datum.EccentricitySquared * (num6 / (num6 + num5))));
			num2 = Math.Abs(num4 - num7);
			num4 = num7;
			latitude = num4;
			num++;
			if (num > 15)
			{
				latitude = num4;
				return new Geodetic(latitude, longitude, num5);
			}
		}
		Geodetic geodetic = new Geodetic(latitude, longitude, num5);
		if (geoidHeight)
		{
			geodetic = new Geodetic(geodetic.Latitude, geodetic.Longitude, geodetic.Altitude - 47.3248);
		}
		return geodetic;
	}

	public LocalTangentPlane ToNed(Ecef reference)
	{
		Vector3D vector3D = reference.Position;
		double num = Math.Atan2(vector3D.Z, Math.Sqrt(vector3D.X * vector3D.X + vector3D.Y * vector3D.Y));
		double num2 = Math.Atan2(vector3D.Y, vector3D.X);
		double num3 = Math.Sin(num2);
		double num4 = Math.Sqrt(1.0 - num3 * num3);
		double num5 = num2 % TwoPI;
		if (num5 > 0.0)
		{
			if (num5 > PIByTwo && num5 < ThreePIByTwo)
			{
				num4 = 0.0 - num4;
			}
		}
		else if (num5 < 0.0 - PIByTwo && num5 > 0.0 - ThreePIByTwo)
		{
			num4 = 0.0 - num4;
		}
		double num6 = Math.Sin(num);
		double num7 = Math.Sqrt(1.0 - num6 * num6);
		num5 = num % TwoPI;
		if (num5 > 0.0)
		{
			if (num5 > PIByTwo && num5 < ThreePIByTwo)
			{
				num7 = 0.0 - num7;
			}
		}
		else if (num5 < 0.0 - PIByTwo && num5 > 0.0 - ThreePIByTwo)
		{
			num7 = 0.0 - num7;
		}
		double num8 = Position.X - vector3D.X;
		double num9 = Position.Y - vector3D.Y;
		double num10 = Position.Z - vector3D.Z;
		double east = (0.0 - num3) * num8 + num4 * num9;
		double north = (0.0 - num6) * num4 * num8 - num6 * num3 * num9 + num7 * num10;
		double down = num7 * num4 * num8 + num7 * num3 * num9 + num6 * num10;
		return new LocalTangentPlane(north, east, down);
	}

	public double DistanceFrom(Ecef other)
	{
		return Math.Sqrt((position.X - other.Position.X) * (position.X - other.Position.X) + (position.Y - other.Position.Y) * (position.Y - other.Position.Y) + (position.Z - other.Position.Z) * (position.Z - other.Position.Z));
	}
}
