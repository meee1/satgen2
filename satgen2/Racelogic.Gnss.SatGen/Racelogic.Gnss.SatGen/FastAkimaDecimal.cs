using System;
using System.Runtime.CompilerServices;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen;

internal readonly struct FastAkimaDecimal
{
	private readonly decimal[] indefiniteIntegrals;

	public const int MinSampleCount = 5;

	public const int ExtraSampleCount = 2;

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public FastAkimaDecimal(decimal[] positions, decimal[] values)
	{
		if (positions.Length < 5)
		{
			throw new ArgumentOutOfRangeException(string.Format("{0} must contain at least {1} points", "positions", 5));
		}
		if (positions.Length != values.Length)
		{
			throw new ArgumentException("positions and values have different number of elements");
		}
		int sampleCount = positions.Length;
		decimal[] derivatives = EvaluateSplineDerivatives(positions, values, in sampleCount);
		AkimaCoeffDecimal[] coefficients = EvaluateSplineCoefficients(positions, values, derivatives, in sampleCount);
		indefiniteIntegrals = ComputeIndefiniteIntegrals(positions, coefficients, in sampleCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public decimal Integrate(in int fromIndex, in int toIndex)
	{
		return indefiniteIntegrals[toIndex] - indefiniteIntegrals[fromIndex];
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	private static decimal[] ComputeIndefiniteIntegrals(decimal[] positions, AkimaCoeffDecimal[] coefficients, in int sampleCount)
	{
		decimal[] array = new decimal[sampleCount];
		int num = 0;
		while (num < sampleCount - 1)
		{
			AkimaCoeffDecimal akimaCoeffDecimal = coefficients[num];
			decimal num2 = positions[num + 1] - positions[num];
			decimal num3 = 0.25m * akimaCoeffDecimal.C0;
			decimal num4 = 0.3333333333333333333333333333m * akimaCoeffDecimal.C1 + num2 * num3;
			decimal num5 = 0.5m * akimaCoeffDecimal.C2 + num2 * num4;
			decimal num6 = akimaCoeffDecimal.C3 + num2 * num5;
			decimal num7 = array[num] + num2 * num6;
			array[++num] = num7;
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static AkimaCoeffDecimal[] EvaluateSplineCoefficients(decimal[] positions, decimal[] values, decimal[] derivatives, in int sampleCount)
	{
		AkimaCoeffDecimal[] array = new AkimaCoeffDecimal[sampleCount - 1];
		for (int i = 0; i < array.Length; i++)
		{
			int num = i + 1;
			decimal num2 = positions[i];
			decimal num3 = positions[num] - num2;
			decimal c = derivatives[i];
			decimal num4 = c * num3;
			decimal num5 = num4 + derivatives[num] * num3;
			decimal c2 = values[i];
			decimal num6 = values[num] - c2;
			decimal num7 = num6 + num6;
			decimal num8 = num3 * num3;
			decimal c3 = (num5 - num7) / (num3 * num8);
			decimal c4 = (num7 + num6 - num4 - num5) / num8;
			array[i] = new AkimaCoeffDecimal(in c3, in c4, in c, in c2);
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static decimal[] EvaluateSplineDerivatives(decimal[] positions, decimal[] values, in int sampleCount)
	{
		int num = sampleCount - 1;
		decimal[] array = new decimal[num];
		decimal[] array2 = new decimal[num];
		decimal num2 = values[1];
		decimal num3 = 1.0m / (positions[1] - positions[0]);
		decimal num4 = (array[0] = (num2 - values[0]) * num3);
		for (int i = 1; i < num; i++)
		{
			decimal num5 = values[i + 1];
			decimal num6 = (array[i] = (num5 - num2) * num3);
			array2[i] = Math.Abs(num6 - num4);
			num4 = num6;
			num2 = num5;
		}
		decimal[] array3 = new decimal[sampleCount];
		decimal num7 = array[1];
		for (int j = 2; j < array3.Length - 2; j++)
		{
			decimal num8 = array[j];
			decimal num9 = array2[j + 1];
			decimal num10 = array2[j - 1];
			if (num10.AlmostEquals(0.0m) && num9.AlmostEquals(0.0m))
			{
				array3[j] = (num7 + num8) * 0.5m;
			}
			else
			{
				array3[j] = (num9 * num7 + num10 * num8) / (num9 + num10);
			}
			num7 = num8;
		}
		int differentiationIndex = 0;
		var (num11, num12) = DifferentiateThreePoint(positions, values, in differentiationIndex, 0);
		array3[0] = num11;
		array3[1] = num12;
		differentiationIndex = sampleCount - 2;
		var (num13, num14) = DifferentiateThreePoint(positions, values, in differentiationIndex, sampleCount - 3);
		array3[^2] = num13;
		array3[^1] = num14;
		return array3;
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	private static (decimal Derivative1, decimal Derivative2) DifferentiateThreePoint(decimal[] positions, decimal[] values, in int differentiationIndex, int sampleIndex)
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
}
