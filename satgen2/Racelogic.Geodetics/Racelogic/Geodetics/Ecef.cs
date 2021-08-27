using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Racelogic.Geodetics
{
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
			double num12 = num * firstEccentricitySquared * 0.5;
			double num15 = item3 * item3;
			double num16 = item * item + item2 * item2;
			double num17 = Math.Sqrt(num16);
			double num18 = 1.0 / (num16 + num15);
			double num19 = Math.Sqrt(num18);
			double num20 = num * num * num19;
			double num21 = num16 * num18;
			double num2;
			double num4;
			double num5;
			double num3;
			if (num21 > 0.3)
			{
				num2 = Math.Abs(item3) * (num19 + num18 * num21 * (num + num20 + num15 * num18 * (num12 - 2.5 * num20)));
				num3 = Math.Asin(num2);
				num4 = num2 * num2;
				num5 = Math.Sqrt(1.0 - num4);
			}
			else
			{
				num5 = num17 * (num19 - num18 * num18 * num15 * (num + num12 - num20 - num21 * (num12 - 2.5 * num20)));
				num3 = Math.Acos(num5);
				num4 = 1.0 - num5 * num5;
				num2 = Math.Sqrt(num4);
			}
			double num6 = 1.0 / (1.0 - firstEccentricitySquared * num4);
			double num7 = semiMajorAxis * Math.Sqrt(num6);
			double num8 = (1.0 - firstEccentricitySquared) * num7;
			double num9 = num17 - num7 * num5;
			double num10 = Math.Abs(item3) - num8 * num2;
			double num11 = num5 * num9 + num2 * num10;
			double num13 = num5 * num10 - num2 * num9;
			double num14 = num13 / (num8 * num6 + num11);
			num3 += num14;
			if (item3 < 0.0)
			{
				num3 = 0.0 - num3;
			}
			double altitude = num11 + num13 * num14 * 0.5;
			Geodetic position = new Geodetic(num3, longitude, altitude);
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
			return ((5993773 + Position.GetHashCode()) * 9973 + Velocity.GetHashCode()) * 9973 + IsAbsolute.GetHashCode();
		}
	}
}
