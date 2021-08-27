using System;
using System.Runtime.CompilerServices;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
	internal readonly struct FastAkima
	{
		private readonly AkimaCoeff[] coefficients;

		public const int MinSampleCount = 5;

		public const int ExtraSampleCount = 2;

		internal AkimaCoeff[] Coefficients => coefficients;

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public FastAkima(double[] positions, double[] values)
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
			double[] derivatives = EvaluateSplineDerivatives(positions, values, in sampleCount);
			coefficients = EvaluateSplineCoefficients(positions, values, derivatives, in sampleCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static AkimaCoeff[] EvaluateSplineCoefficients(double[] positions, double[] values, double[] derivatives, in int sampleCount)
		{
			AkimaCoeff[] array = new AkimaCoeff[sampleCount - 1];
			for (int i = 0; i < array.Length; i++)
			{
				int num = i + 1;
				double num2 = positions[i];
				double num3 = positions[num] - num2;
				double c = derivatives[i];
				double num4 = c * num3;
				double num5 = num4 + derivatives[num] * num3;
				double c2 = values[i];
				double num6 = values[num] - c2;
				double num7 = num6 + num6;
				double num8 = num3 * num3;
				double c3 = (num5 - num7) / (num3 * num8);
				double c4 = (num7 + num6 - num4 - num5) / num8;
				array[i] = new AkimaCoeff(in c3, in c4, in c, in c2);
			}
			return array;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static double[] EvaluateSplineDerivatives(double[] positions, double[] values, in int sampleCount)
		{
			int num = sampleCount - 1;
			double[] array = new double[num];
			double[] array2 = new double[num];
			double num7 = values[1];
			double num8 = 1.0 / (positions[1] - positions[0]);
			double num9 = (array[0] = (num7 - values[0]) * num8);
			for (int i = 1; i < num; i++)
			{
				double num14 = values[i + 1];
				double num10 = (array[i] = (num14 - num7) * num8);
				array2[i] = Math.Abs(num10 - num9);
				num9 = num10;
				num7 = num14;
			}
			double[] array3 = new double[sampleCount];
			double num11 = array[1];
			for (int j = 2; j < array3.Length - 2; j++)
			{
				double num12 = array[j];
				double num13 = array2[j + 1];
				double num2 = array2[j - 1];
				if (num2.AlmostEquals(0.0) && num13.AlmostEquals(0.0))
				{
					array3[j] = (num11 + num12) * 0.5;
				}
				else
				{
					array3[j] = (num13 * num11 + num2 * num12) / (num13 + num2);
				}
				num11 = num12;
			}
			int differentiationIndex = 0;
			var (num3, num4) = DifferentiateThreePoint(positions, values, in differentiationIndex, 0);
			array3[0] = num3;
			array3[1] = num4;
			differentiationIndex = sampleCount - 2;
			var (num5, num6) = DifferentiateThreePoint(positions, values, in differentiationIndex, sampleCount - 3);
			array3[^2] = num5;
			array3[^1] = num6;
			return array3;
		}

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		private static (double Derivative1, double Derivative2) DifferentiateThreePoint(double[] positions, double[] values, in int differentiationIndex, int sampleIndex)
		{
			double num = values[sampleIndex];
			double num4 = positions[sampleIndex];
			sampleIndex++;
			double num5 = values[sampleIndex];
			double num6 = positions[sampleIndex] - num4;
			sampleIndex++;
			double num11 = values[sampleIndex];
			double num7 = positions[sampleIndex] - num4;
			double num8 = (num5 - num) / num6;
			double num9 = ((num11 - num) / num7 - num8) / (num7 - num6);
			double num10 = num8 - num9 * num6;
			double num2 = positions[differentiationIndex] - num4;
			double num12 = num9 * num2;
			double item3 = num12 + num12 + num10;
			double num3 = positions[differentiationIndex + 1] - num4;
			double num13 = num9 * num3;
			double item2 = num13 + num13 + num10;
			return (item3, item2);
		}
	}
}
