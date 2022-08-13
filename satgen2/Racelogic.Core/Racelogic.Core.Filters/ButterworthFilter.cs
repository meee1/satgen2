using System;
using System.Collections.Generic;
using System.Numerics;

namespace Racelogic.Core.Filters;

public class ButterworthFilter
{
	private static readonly Complex complexMinusOne = new Complex(-1.0, 0.0);

	private static readonly Complex complexOne = new Complex(1.0, 0.0);

	private static readonly Complex complexTwo = new Complex(2.0, 0.0);

	private static readonly Complex complexZero = new Complex(0.0, 0.0);

	private static Complex complexHalf = new Complex(0.5, 0.0);

	private readonly double cutOffFrequency;

	private readonly int filterOrder;

	private readonly double rawAlpha;

	private readonly Complex[] sPoles;

	private readonly double sampleRate;

	private readonly double warpedAlpha;

	private readonly double[] xv;

	private readonly double[] yv;

	private readonly Complex[] zPoles;

	private readonly Complex[] zZeros;

	private Complex dcGain;

	private Complex fcGain;

	private Complex hfGain;

	private double[] xvalueCoefficients;

	private double[] yvalueCoefficients;

	public Complex DcGain
	{
		get
		{
			return dcGain;
		}
		internal set
		{
			dcGain = value;
		}
	}

	public Complex FcGain => fcGain;

	public Complex HfGain => hfGain;

	public Complex[] SPoles => sPoles;

	public double[] XvalueCoefficients
	{
		get
		{
			return xvalueCoefficients;
		}
		internal set
		{
			xvalueCoefficients = value;
		}
	}

	public double[] YvalueCoefficients
	{
		get
		{
			return yvalueCoefficients;
		}
		internal set
		{
			yvalueCoefficients = value;
		}
	}

	public Complex[] ZPoles => zPoles;

	public ButterworthFilter(double sampleRate, double cutOffFrequency, int filterOrder)
	{
		this.sampleRate = sampleRate;
		this.cutOffFrequency = cutOffFrequency;
		this.filterOrder = filterOrder;
		sPoles = new Complex[this.filterOrder];
		zPoles = new Complex[this.filterOrder];
		zZeros = new Complex[this.filterOrder];
		xvalueCoefficients = new double[this.filterOrder + 1];
		yvalueCoefficients = new double[this.filterOrder + 1];
		xv = new double[this.filterOrder + 1];
		yv = new double[this.filterOrder + 1];
		rawAlpha = this.cutOffFrequency / this.sampleRate;
		warpedAlpha = Math.Tan(Math.PI * rawAlpha) / Math.PI;
		ComputeSPoles();
		ComputeZPoles();
		ComputeGainAndCoefficients();
	}

	public List<double> Filter(List<double> values)
	{
		List<double> list = new List<double>();
		for (int i = 0; i <= filterOrder; i++)
		{
			xv[i] = 0.0;
			yv[i] = 0.0;
		}
		foreach (double value in values)
		{
			list.Add(GetNextValue(value));
		}
		return list;
	}

	public double GetNextValue(double currentValue)
	{
		for (int i = 0; i <= filterOrder; i++)
		{
			if (i != filterOrder)
			{
				xv[i] = xv[i + 1];
				yv[i] = yv[i + 1];
				continue;
			}
			xv[i] = currentValue / DcGain.Magnitude;
			double num = 0.0;
			double num2 = 0.0;
			for (int j = 0; j <= filterOrder; j++)
			{
				num += xvalueCoefficients[j] * xv[j];
				if (j != filterOrder)
				{
					num2 += yvalueCoefficients[j] * yv[j];
				}
			}
			yv[i] = num + num2;
		}
		return yv[filterOrder];
	}

	private void ComputeGainAndCoefficients()
	{
		Complex[] array = new Complex[filterOrder + 1];
		Complex[] array2 = new Complex[filterOrder + 1];
		Expand(zZeros, array);
		Expand(zPoles, array2);
		dcGain = Evaluate(array, array2, filterOrder, complexOne);
		Complex zValue = Complex.Exp(new Complex(0.0, Math.PI * 2.0 * rawAlpha));
		fcGain = Evaluate(array, array2, filterOrder, zValue);
		hfGain = Evaluate(array, array2, filterOrder, complexMinusOne);
		for (int i = 0; i <= filterOrder; i++)
		{
			xvalueCoefficients[i] = array[i].Real / array2[filterOrder].Real;
			yvalueCoefficients[i] = (0.0 - array2[i].Real) / array2[filterOrder].Real;
		}
	}

	private void ComputeSPoles()
	{
		int num = 0;
		for (int i = 0; i < 2 * filterOrder; i++)
		{
			Complex complex = Complex.Exp(new Complex(0.0, ((filterOrder & 1) == 1) ? ((double)i * Math.PI / (double)filterOrder) : (((double)i + 0.5) * Math.PI / (double)filterOrder)));
			if (complex.Real < 0.0)
			{
				sPoles[num] = complex;
				num++;
			}
		}
		Complex complex2 = new Complex(Math.PI * 2.0 * warpedAlpha, 0.0);
		for (int j = 0; j < filterOrder; j++)
		{
			sPoles[j] *= complex2;
		}
	}

	private void ComputeZPoles()
	{
		for (int i = 0; i < filterOrder; i++)
		{
			Complex complex = complexTwo + sPoles[i];
			Complex complex2 = complexTwo - sPoles[i];
			zPoles[i] = complex / complex2;
			zZeros[i] = complexMinusOne;
		}
	}

	private Complex Eval(Complex[] Coefficients, int np, Complex zValue)
	{
		Complex complex = complexZero;
		for (int num = np; num >= 0; num--)
		{
			complex = complex * zValue + Coefficients[num];
		}
		return complex;
	}

	private Complex Evaluate(Complex[] topCoeffs, Complex[] bottomCoeffs, int np, Complex zValue)
	{
		return Eval(topCoeffs, np, zValue) / Eval(bottomCoeffs, np, zValue);
	}

	private void Expand(Complex[] zvalue, Complex[] coefficients)
	{
		coefficients[0] = complexOne;
		for (int i = 0; i < filterOrder; i++)
		{
			coefficients[i + 1] = complexZero;
		}
		for (int j = 0; j < filterOrder; j++)
		{
			multin(zvalue[j], coefficients);
		}
	}

	private void multin(Complex wvalue, Complex[] Coefficients)
	{
		Complex complex = Complex.Negate(wvalue);
		for (int num = filterOrder; num >= 1; num--)
		{
			Coefficients[num] = complex * Coefficients[num] + Coefficients[num - 1];
		}
		Coefficients[0] = complex * Coefficients[0];
	}
}
