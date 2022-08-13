using System;

namespace Racelogic.DataSource;

[Obsolete("This struct is about to be removed.  Please use Vector3D defined in Racelogic.Geodetic.")]
public struct Vector3D : IEquatable<Vector3D>
{
	public const double Epsilon = 1E-09;

	public readonly double X;

	public readonly double Y;

	public readonly double Z;

	public double Magnitude => Math.Sqrt(Math.Pow(X, 2.0) + Math.Pow(Y, 2.0) + Math.Pow(Z, 2.0));

	public Vector3D Normalised => new Vector3D((X == 0.0) ? 0.0 : (X / Magnitude), (Y == 0.0) ? 0.0 : (Y / Magnitude), (Z == 0.0) ? 0.0 : (Z / Magnitude));

	public Vector3D(double x, double y, double z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public static Vector3D operator +(Vector3D left, Vector3D right)
	{
		return new Vector3D(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
	}

	public static bool operator ==(Vector3D left, Vector3D right)
	{
		if (Math.Abs(left.X - right.X) < 1E-09 && Math.Abs(left.Y - right.Y) < 1E-09)
		{
			return Math.Abs(left.Z - right.Z) < 1E-09;
		}
		return false;
	}

	public static bool operator !=(Vector3D left, Vector3D right)
	{
		return !(left == right);
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

	public Vector3D CrossProduct(Vector3D right)
	{
		return new Vector3D(Y * right.Z - Z * right.Y, Z * right.X - X * right.Z, X * right.Y - Y * right.X);
	}

	public double DotProduct(Vector3D right)
	{
		return X * right.X + Y * right.Y + Z * right.Z;
	}

	public bool Equals(Vector3D other)
	{
		double x = other.X;
		if (x.Equals(X))
		{
			x = other.Y;
			if (x.Equals(Y))
			{
				x = other.Z;
				return x.Equals(Z);
			}
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
		int num = x.GetHashCode() * 397;
		x = Y;
		int num2 = (num ^ x.GetHashCode()) * 397;
		x = Z;
		return num2 ^ x.GetHashCode();
	}

	public override string ToString()
	{
		return $"({X},{Y},{Z})";
	}
}
