using System;
using System.Diagnostics;

namespace Racelogic.Maths;

public sealed class AkimaSplineDecimal
{
	private readonly struct AkimaCoefficientDecimal
	{
		public readonly decimal C0;

		public readonly decimal C1;

		public readonly decimal C2;

		public readonly decimal C3;

		public AkimaCoefficientDecimal(decimal c0, decimal c1, decimal c2, decimal c3)
		{
			C0 = c0;
			C1 = c1;
			C2 = c2;
			C3 = c3;
		}
	}

	private readonly decimal[] positions;

	private readonly decimal[] values;

	private readonly AkimaCoefficientDecimal[] coefficients;

	private readonly decimal sampleRate;

	private readonly decimal minPosition;

	private decimal[] indefiniteIntegrals;

	public const int MinSampleCount = 5;

	public const int ExtraSampleCount = 2;

	public decimal[] Positions
	{
		[DebuggerStepThrough]
		get
		{
			return positions;
		}
	}

	public decimal[] Values
	{
		[DebuggerStepThrough]
		get
		{
			return values;
		}
	}

	public AkimaSplineDecimal(decimal[] positions, decimal[] values, bool isConstantRate = false)
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
			decimal num2 = positions[num - 2];
			decimal num3 = positions[num - 1];
			sampleRate = Math.Round(1.0m / (num3 - num2));
			minPosition = positions[0];
		}
		decimal[] derivatives = EvaluateSplineDerivatives();
		coefficients = EvaluateSplineCoefficients(derivatives);
	}

	public decimal Interpolate(decimal position)
	{
		int num = ((sampleRate == 0.0m) ? LeftSegmentIndex(position) : ((int)((position - minPosition) * sampleRate)));
		decimal num2 = position - Positions[num];
		AkimaCoefficientDecimal akimaCoefficientDecimal = coefficients[num];
		decimal num3 = akimaCoefficientDecimal.C1 + num2 * akimaCoefficientDecimal.C0;
		decimal num4 = akimaCoefficientDecimal.C2 + num2 * num3;
		return akimaCoefficientDecimal.C3 + num2 * num4;
	}

	public decimal Integrate(decimal fromPosition, decimal toPosition)
	{
		if (indefiniteIntegrals == null)
		{
			indefiniteIntegrals = ComputeIndefiniteIntegrals();
		}
		return Integrate(toPosition) - Integrate(fromPosition);
	}

	private decimal Integrate(decimal position)
	{
		int num = ((sampleRate == 0.0m) ? LeftSegmentIndex(position) : ((int)((position - minPosition) * sampleRate)));
		decimal num2 = position - Positions[num];
		AkimaCoefficientDecimal akimaCoefficientDecimal = coefficients[num];
		decimal num3 = 0.25m * akimaCoefficientDecimal.C0;
		decimal num4 = 0.3333333333333333333333333333m * akimaCoefficientDecimal.C1 + num2 * num3;
		decimal num5 = 0.5m * akimaCoefficientDecimal.C2 + num2 * num4;
		decimal num6 = akimaCoefficientDecimal.C3 + num2 * num5;
		return indefiniteIntegrals[num] + num2 * num6;
	}

	private decimal[] ComputeIndefiniteIntegrals()
	{
		decimal[] array = new decimal[positions.Length];
		int num = 0;
		while (num < array.Length - 1)
		{
			AkimaCoefficientDecimal akimaCoefficientDecimal = coefficients[num];
			decimal num2 = Positions[num + 1] - Positions[num];
			decimal num3 = 0.25m * akimaCoefficientDecimal.C0;
			decimal num4 = 0.3333333333333333333333333333m * akimaCoefficientDecimal.C1 + num2 * num3;
			decimal num5 = 0.5m * akimaCoefficientDecimal.C2 + num2 * num4;
			decimal num6 = akimaCoefficientDecimal.C3 + num2 * num5;
			decimal num7 = array[num] + num2 * num6;
			array[++num] = num7;
		}
		return array;
	}

	public decimal Differentiate(decimal position)
	{
		int num = ((sampleRate == 0.0m) ? LeftSegmentIndex(position) : ((int)((position - minPosition) * sampleRate)));
		decimal num2 = position - Positions[num];
		AkimaCoefficientDecimal akimaCoefficientDecimal = coefficients[num];
		decimal num3 = 3.0m * akimaCoefficientDecimal.C0;
		decimal num4 = akimaCoefficientDecimal.C1 + akimaCoefficientDecimal.C1 + num2 * num3;
		return akimaCoefficientDecimal.C2 + num2 * num4;
	}

	public decimal Differentiate2(decimal position)
	{
		int num = ((sampleRate == 0.0m) ? LeftSegmentIndex(position) : ((int)((position - minPosition) * sampleRate)));
		decimal num2 = position - Positions[num];
		AkimaCoefficientDecimal akimaCoefficientDecimal = coefficients[num];
		decimal num3 = 6.0m * akimaCoefficientDecimal.C0;
		return akimaCoefficientDecimal.C1 + akimaCoefficientDecimal.C1 + num2 * num3;
	}

	private int LeftSegmentIndex(decimal position)
	{
		int num = Array.BinarySearch(Positions, position);
		if (num < 0)
		{
			num = ~num - 1;
		}
		return Math.Min(Math.Max(num, 0), Positions.Length - 2);
	}

	private decimal[] EvaluateSplineDerivatives()
	{
		int num = positions.Length;
		int num2 = num - 1;
		decimal[] array = new decimal[num2];
		decimal[] array2 = new decimal[num2];
		decimal num3 = values[1];
		decimal num4 = 1.0m / (positions[1] - positions[0]);
		decimal num5 = (array[0] = (num3 - values[0]) * num4);
		for (int i = 1; i < num2; i++)
		{
			decimal num6 = values[i + 1];
			decimal num7 = (array[i] = (num6 - num3) * num4);
			array2[i] = Math.Abs(num7 - num5);
			num5 = num7;
			num3 = num6;
		}
		decimal[] array3 = new decimal[num];
		decimal num8 = array[1];
		for (int j = 2; j < array3.Length - 2; j++)
		{
			decimal num9 = array[j];
			decimal num10 = array2[j + 1];
			decimal num11 = array2[j - 1];
			if (num11.AlmostEquals(0.0m) && num10.AlmostEquals(0.0m))
			{
				array3[j] = (num8 + num9) * 0.5m;
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

	private (decimal Derivative1, decimal Derivative2) DifferentiateThreePoint(int differentiationIndex, int sampleIndex)
	{
		decimal num = values[sampleIndex];
		decimal num2 = positions[sampleIndex];
		sampleIndex++;
		decimal num3 = values[sampleIndex];
		decimal num4 = positions[sampleIndex] - num2;
		sampleIndex++;
		decimal num5 = values[sampleIndex];
		decimal num6 = positions[sampleIndex] - num2;
		decimal num7 = (num3 - num) / num4;
		decimal num8 = ((num5 - num) / num6 - num7) / (num6 - num4);
		decimal num9 = num7 - num8 * num4;
		decimal num10 = positions[differentiationIndex] - num2;
		decimal num11 = num8 * num10;
		decimal item = num11 + num11 + num9;
		decimal num12 = positions[differentiationIndex + 1] - num2;
		decimal num13 = num8 * num12;
		decimal item2 = num13 + num13 + num9;
		return (item, item2);
	}

	private AkimaCoefficientDecimal[] EvaluateSplineCoefficients(decimal[] derivatives)
	{
		AkimaCoefficientDecimal[] array = new AkimaCoefficientDecimal[positions.Length - 1];
		for (int i = 0; i < array.Length; i++)
		{
			int num = i + 1;
			decimal num2 = positions[i];
			decimal num3 = positions[num] - num2;
			decimal num4 = derivatives[i];
			decimal num5 = num4 * num3;
			decimal num6 = num5 + derivatives[num] * num3;
			decimal num7 = values[i];
			decimal num8 = values[num] - num7;
			decimal num9 = num8 + num8;
			decimal num10 = num3 * num3;
			decimal c = (num6 - num9) / (num3 * num10);
			decimal c2 = (num9 + num8 - num5 - num6) / num10;
			array[i] = new AkimaCoefficientDecimal(c, c2, num4, num7);
		}
		return array;
	}
}
