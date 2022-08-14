using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Racelogic.Maths;

namespace Racelogic.Geodetics;

[JsonObject(MemberSerialization.OptIn)]
[DebuggerDisplay("X:{X}  Y:{Y}  Z:{Z}  Mag:{Magnitude()}")]
public readonly struct Vector3D : IEquatable<Vector3D>
{
	[JsonProperty(PropertyName = "X")]
	public readonly double X;

	[JsonProperty(PropertyName = "Y")]
	public readonly double Y;

	[JsonProperty(PropertyName = "Z")]
	public readonly double Z;

	[JsonConstructor]
	public Vector3D(double x, double y, double z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public (double X, double Y, double Z) Deconstruct()
	{
		return (X, Y, Z);
	}

	public double Magnitude()
	{
		return Math.Sqrt(X * X + Y * Y + Z * Z);
	}

	public Vector3D Normalize()
	{
		return this / Magnitude();
	}

	public Vector3D Normalize(double magnitude)
	{
		return this / magnitude;
	}

	public Vector3D CrossProduct(in Vector3D right)
	{
		return new Vector3D(Y * right.Z - Z * right.Y, Z * right.X - X * right.Z, X * right.Y - Y * right.X);
	}

	public double DotProduct(in Vector3D right)
	{
		return X * right.X + Y * right.Y + Z * right.Z;
	}

	public static Vector3D operator +(Vector3D left, Vector3D right)
	{
		return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static Vector3D operator -(Vector3D left, Vector3D right)
	{
		return new Vector3D(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
	}

	public static Vector3D operator *(Vector3D left, double right)
	{
		return new Vector3D(left.X * right, left.Y * right, left.Z * right);
	}

	public static Vector3D operator *(double left, Vector3D right)
	{
		return new Vector3D(left * right.X, left * right.Y, left * right.Z);
	}

	public static Vector3D operator /(Vector3D dividend, double divisor)
	{
		double num = 1.0 / divisor;
		return new Vector3D(dividend.X * num, dividend.Y * num, dividend.Z * num);
	}

	public static bool operator ==(Vector3D left, Vector3D right)
	{
		if (left.X.AlmostEquals(right.X) && left.Y.AlmostEquals(right.Y))
		{
			return left.Z.AlmostEquals(right.Z);
		}
		return false;
	}

	public static bool operator !=(Vector3D left, Vector3D right)
	{
		return !(left == right);
	}

	public bool Equals(Vector3D other)
	{
		if (other.X.AlmostEquals(X) && other.Y.AlmostEquals(Y))
		{
			return other.Z.AlmostEquals(Z);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.GetType() != typeof(Vector3D))
		{
			return false;
		}
		return Equals((Vector3D)obj);
	}

	public override int GetHashCode()
	{
		double x = X;
		int num = (5993773 + x.GetHashCode()) * 9973;
		x = Y;
		int num2 = (num + x.GetHashCode()) * 9973;
		x = Z;
		return num2 + x.GetHashCode();
	}

	public override string ToString()
	{
		return $"({X};{Y};{Z})";
	}
}
