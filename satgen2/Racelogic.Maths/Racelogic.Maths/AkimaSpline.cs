using System;
using System.Diagnostics;

namespace Racelogic.Maths;

public sealed class AkimaSpline
{
	private readonly struct AkimaCoefficient
	{
		public readonly double C0;

		public readonly double C1;

		public readonly double C2;

		public readonly double C3;

		public AkimaCoefficient(double c0, double c1, double c2, double c3)
		{
			C0 = c0;
			C1 = c1;
			C2 = c2;
			C3 = c3;
		}
	}

	private readonly double[] positions;

	private readonly double[] values;

	private readonly AkimaCoefficient[] coefficients;

	private readonly double sampleRate;

	private readonly double minPosition;

	private double[] indefiniteIntegrals;

	public const int MinSampleCount = 5;

	public const int ExtraSampleCount = 2;

	public double[] Positions
	{
		[DebuggerStepThrough]
		get
		{
			return positions;
		}
	}

	public double[] Values
	{
		[DebuggerStepThrough]
		get
		{
			return values;
		}
	}

	public AkimaSpline(double[] positions, double[] values, bool isConstantRate = false)
	{
		if (positions == null)
		{
			throw new ArgumentNullException("positions");
		}
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		int num = positions.Length;
		if (num < 5)
		{
			throw new ArgumentOutOfRangeException(string.Format("{0} must contain at least {1} points", "positions", 5));
		}
		if (num != values.Length)
		{
			throw new ArgumentException("positions and values have different number of elements");
		}
		this.positions = positions;
		this.values = values;
		if (!isConstantRate)
		{
			double num2 = positions[num - 2];
			double num3 = positions[num - 1];
			sampleRate = Math.Round(1.0 / (num3 - num2));
			minPosition = positions[0];
		}
		double[] derivatives = EvaluateSplineDerivatives();
		coefficients = EvaluateSplineCoefficients(derivatives);
	}

	public double Interpolate(double position)
	{
		int num = ((sampleRate == 0.0) ? LeftSegmentIndex(position) : ((int)((position - minPosition) * sampleRate)));
		double num2 = position - Positions[num];
		AkimaCoefficient akimaCoefficient = coefficients[num];
		double num3 = akimaCoefficient.C1 + num2 * akimaCoefficient.C0;
		double num4 = akimaCoefficient.C2 + num2 * num3;
		return akimaCoefficient.C3 + num2 * num4;
	}

	public double Integrate(double fromPosition, double toPosition)
	{
		if (indefiniteIntegrals == null)
		{
			indefiniteIntegrals = ComputeIndefiniteIntegrals();
		}
		return Integrate(toPosition) - Integrate(fromPosition);
	}

	private double Integrate(double position)
	{
		int num = ((sampleRate == 0.0) ? LeftSegmentIndex(position) : ((int)((position - minPosition) * sampleRate)));
		double num2 = position - Positions[num];
		AkimaCoefficient akimaCoefficient = coefficients[num];
		double num3 = 0.25 * akimaCoefficient.C0;
		double num4 = 1.0 / 3.0 * akimaCoefficient.C1 + num2 * num3;
		double num5 = 0.5 * akimaCoefficient.C2 + num2 * num4;
		double num6 = akimaCoefficient.C3 + num2 * num5;
		return indefiniteIntegrals[num] + num2 * num6;
	}

	private double[] ComputeIndefiniteIntegrals()
	{
		double[] array = new double[positions.Length];
		int num = 0;
		while (num < array.Length - 1)
		{
			AkimaCoefficient akimaCoefficient = coefficients[num];
			double num2 = Positions[num + 1] - Positions[num];
			double num3 = 0.25 * akimaCoefficient.C0;
			double num4 = 1.0 / 3.0 * akimaCoefficient.C1 + num2 * num3;
			double num5 = 0.5 * akimaCoefficient.C2 + num2 * num4;
			double num6 = akimaCoefficient.C3 + num2 * num5;
			double num7 = array[num] + num2 * num6;
			array[++num] = num7;
		}
		return array;
	}

	public double Differentiate(double position)
	{
		int num = ((sampleRate == 0.0) ? LeftSegmentIndex(position) : ((int)((position - minPosition) * sampleRate)));
		double num2 = position - Positions[num];
		AkimaCoefficient akimaCoefficient = coefficients[num];
		double num3 = 3.0 * akimaCoefficient.C0;
		double num4 = akimaCoefficient.C1 + akimaCoefficient.C1 + num2 * num3;
		return akimaCoefficient.C2 + num2 * num4;
	}

	public double Differentiate2(double position)
	{
		int num = ((sampleRate == 0.0) ? LeftSegmentIndex(position) : ((int)((position - minPosition) * sampleRate)));
		double num2 = position - Positions[num];
		AkimaCoefficient akimaCoefficient = coefficients[num];
		double num3 = 6.0 * akimaCoefficient.C0;
		return akimaCoefficient.C1 + akimaCoefficient.C1 + num2 * num3;
	}

	private int LeftSegmentIndex(double position)
	{
		int num = Array.BinarySearch(Positions, position);
		if (num < 0)
		{
			num = ~num - 1;
		}
		return Math.Min(Math.Max(num, 0), Positions.Length - 2);
	}

	private double[] EvaluateSplineDerivatives()
	{
		int num = positions.Length;
		int num2 = num - 1;
		double[] array = new double[num2];
		double[] array2 = new double[num2];
		double num3 = values[1];
		double num4 = 1.0 / (positions[1] - positions[0]);
		double num5 = (array[0] = (num3 - values[0]) * num4);
		for (int i = 1; i < num2; i++)
		{
			double num6 = values[i + 1];
			double num7 = (array[i] = (num6 - num3) * num4);
			array2[i] = Math.Abs(num7 - num5);
			num5 = num7;
			num3 = num6;
		}
		double[] array3 = new double[num];
		double num8 = array[1];
		for (int j = 2; j < array3.Length - 2; j++)
		{
			double num9 = array[j];
			double num10 = array2[j + 1];
			double num11 = array2[j - 1];
			if (num11.AlmostEquals(0.0) && num10.AlmostEquals(0.0))
			{
				array3[j] = (num8 + num9) * 0.5;
			}
			else
			{
				array3[j] = (num10 * num8 + num11 * num9) / (num10 + num11);
			}
			num8 = num9;
		}
		var (num12, num13) = DifferentiateThreePoint(0, 0);
		array3[0] = num12;
		array3[1] = num13;
		var (num14, num15) = DifferentiateThreePoint(num - 2, num - 3);
		array3[num - 2] = num14;
		array3[num - 1] = num15;
		return array3;
	}

	private (double Derivative1, double Derivative2) DifferentiateThreePoint(int differentiationIndex, int sampleIndex)
	{
		double num = values[sampleIndex];
		double num2 = positions[sampleIndex];
		sampleIndex++;
		double num3 = values[sampleIndex];
		double num4 = positions[sampleIndex] - num2;
		sampleIndex++;
		double num5 = values[sampleIndex];
		double num6 = positions[sampleIndex] - num2;
		double num7 = (num3 - num) / num4;
		double num8 = ((num5 - num) / num6 - num7) / (num6 - num4);
		double num9 = num7 - num8 * num4;
		double num10 = positions[differentiationIndex] - num2;
		double num11 = num8 * num10;
		double item = num11 + num11 + num9;
		double num12 = positions[differentiationIndex + 1] - num2;
		double num13 = num8 * num12;
		double item2 = num13 + num13 + num9;
		return (item, item2);
	}

	private AkimaCoefficient[] EvaluateSplineCoefficients(double[] derivatives)
	{
		AkimaCoefficient[] array = new AkimaCoefficient[positions.Length - 1];
		for (int i = 0; i < array.Length; i++)
		{
			int num = i + 1;
			double num2 = positions[i];
			double num3 = positions[num] - num2;
			double num4 = derivatives[i];
			double num5 = num4 * num3;
			double num6 = num5 + derivatives[num] * num3;
			double num7 = values[i];
			double num8 = values[num] - num7;
			double num9 = num8 + num8;
			double num10 = num3 * num3;
			double c = (num6 - num9) / (num3 * num10);
			double c2 = (num9 + num8 - num5 - num6) / num10;
			array[i] = new AkimaCoefficient(c, c2, num4, num7);
		}
		return array;
	}
}
