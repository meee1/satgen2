using System;
using System.Diagnostics;

namespace Racelogic.Gnss.SatGen.Glonass;

internal sealed class SinCosStore
{
	private readonly double sin;

	private readonly double cos;

	private readonly double sin2;

	private readonly double cos2;

	private readonly double sin3;

	private readonly double cos3;

	private readonly double sin4;

	private readonly double cos4;

	public double Sin
	{
		[DebuggerStepThrough]
		get
		{
			return sin;
		}
	}

	public double Cos
	{
		[DebuggerStepThrough]
		get
		{
			return cos;
		}
	}

	public double Sin2
	{
		[DebuggerStepThrough]
		get
		{
			return sin2;
		}
	}

	public double Cos2
	{
		[DebuggerStepThrough]
		get
		{
			return cos2;
		}
	}

	public double Sin3
	{
		[DebuggerStepThrough]
		get
		{
			return sin3;
		}
	}

	public double Cos3
	{
		[DebuggerStepThrough]
		get
		{
			return cos3;
		}
	}

	public double Sin4
	{
		[DebuggerStepThrough]
		get
		{
			return sin4;
		}
	}

	public double Cos4
	{
		[DebuggerStepThrough]
		get
		{
			return cos4;
		}
	}

	public SinCosStore(in double value)
	{
		double num = value;
		sin = Math.Sin(num);
		cos = Math.Cos(num);
		num += value;
		sin2 = Math.Sin(num);
		cos2 = Math.Cos(num);
		num += value;
		sin3 = Math.Sin(num);
		cos3 = Math.Cos(num);
		num += value;
		sin4 = Math.Sin(num);
		cos4 = Math.Cos(num);
	}
}
