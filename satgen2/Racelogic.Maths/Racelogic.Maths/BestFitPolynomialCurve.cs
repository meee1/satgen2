using System;
using System.Collections.Generic;
using System.Linq;

namespace Racelogic.Maths;

public class BestFitPolynomialCurve
{
	private List<Point> originalPoints;

	private int degree;

	private string error;

	private List<double> coefficients;

	public string Error => error;

	public BestFitPolynomialCurve(List<Point> originalPoints, int degree)
	{
		this.originalPoints = originalPoints;
		this.degree = degree;
		CalculateCoEfficients();
	}

	public List<Point> GetCurveOfBestFit(double? startX = null, double? endX = null, double? internvalX = null)
	{
		if (string.IsNullOrEmpty(error))
		{
			List<Point> list = new List<Point>();
			if (!startX.HasValue || !endX.HasValue || !internvalX.HasValue)
			{
				foreach (Point originalPoint in originalPoints)
				{
					list.Add(new Point(originalPoint.X, CalculateY(originalPoint.X)));
				}
				return list;
			}
			for (double num = startX.Value; num <= endX.Value; num += internvalX.Value)
			{
				list.Add(new Point(num, CalculateY(num)));
			}
			return list;
		}
		return null;
	}

	public double CalculateY(double x)
	{
		double num = 0.0;
		double num2 = 1.0;
		for (int i = 0; i < coefficients.Count; i++)
		{
			num += num2 * coefficients[i];
			num2 *= x;
		}
		return num;
	}

	public double GetAreaUnderCurve(double minX, double maxX)
	{
		if (!string.IsNullOrEmpty(error))
		{
			return double.NaN;
		}
		return GetIntegrationValue(maxX) - GetIntegrationValue(minX);
	}

	public double CalculateXForPercentageofPeakYBeforeThePeak(double percentage, bool extraPolateIfNotWithinPoints)
	{
		if (!string.IsNullOrEmpty(error))
		{
			return double.NaN;
		}
		double peakY = GetPeakY(out var X);
		if (percentage == 100.0)
		{
			return X;
		}
		peakY = peakY * percentage / 100.0;
		List<double> source = CalculateX(peakY, null, X);
		if (extraPolateIfNotWithinPoints)
		{
			if (source.Count() == 0)
			{
				return double.NaN;
			}
			return source.Max();
		}
		double xmin = originalPoints.Min((Point c) => c.X);
		double xmax = originalPoints.Max((Point c) => c.X);
		IEnumerable<double> source2 = source.Where((double c) => c >= xmin && c <= xmax);
		if (source2.Count() == 0)
		{
			return double.NaN;
		}
		return source2.Max();
	}

	public double CalculateXForPercentageofPeakYAfterThePeak(double percentage, bool extraPolateIfNotWithinPoints)
	{
		if (!string.IsNullOrEmpty(error))
		{
			return double.NaN;
		}
		double peakY = GetPeakY(out var X);
		if (percentage == 100.0)
		{
			return X;
		}
		peakY = peakY * percentage / 100.0;
		List<double> source = CalculateX(peakY, X);
		if (extraPolateIfNotWithinPoints)
		{
			if (source.Count() == 0)
			{
				return double.NaN;
			}
			return source.Min();
		}
		double xmin = originalPoints.Min((Point c) => c.X);
		double xmax = originalPoints.Max((Point c) => c.X);
		IEnumerable<double> source2 = source.Where((double c) => c >= xmin && c <= xmax);
		if (source2.Count() == 0)
		{
			return double.NaN;
		}
		return source2.Min();
	}

	public List<double> CalculateX(double y, double? minimumX = null, double? maximumX = null)
	{
		List<double> list = new List<double>();
		if (!string.IsNullOrEmpty(error))
		{
			return list;
		}
		if (degree == 1)
		{
			double num = (y - coefficients[0]) / coefficients[1];
			if ((!minimumX.HasValue || num >= minimumX.Value) && (!maximumX.HasValue || num <= maximumX.Value))
			{
				list.Add(num);
			}
		}
		else if (degree == 2)
		{
			double num2 = coefficients[2];
			double num3 = coefficients[1];
			double num4 = coefficients[0] - y;
			double num5 = num3 * num3 - 4.0 * num2 * num4;
			if (num5 >= 0.0)
			{
				double num6 = (0.0 - num3 + Math.Sqrt(num5)) / (2.0 * num2);
				double num7 = (0.0 - num3 - Math.Sqrt(num5)) / (2.0 * num2);
				if ((!minimumX.HasValue || num6 >= minimumX.Value) && (!maximumX.HasValue || num6 <= maximumX.Value))
				{
					list.Add(num6);
				}
				if ((!minimumX.HasValue || num7 >= minimumX.Value) && (!maximumX.HasValue || num7 <= maximumX.Value))
				{
					list.Add(num7);
				}
			}
		}
		if (list.Count == 0)
		{
			double num8 = originalPoints.Min((Point c) => c.X);
			double num9 = originalPoints.Max((Point c) => c.X);
			List<Point> curveOfBestFit = GetCurveOfBestFit(num8, num9, 0.01);
			for (int i = 0; i < curveOfBestFit.Count - 2; i++)
			{
				if ((!(curveOfBestFit[i].Y <= y) || !(curveOfBestFit[i + 1].Y >= y)) && (!(curveOfBestFit[i].Y >= y) || !(curveOfBestFit[i + 1].Y <= y)))
				{
					continue;
				}
				if (Math.Abs(curveOfBestFit[i].Y - curveOfBestFit[i + 1].Y) <= double.Epsilon)
				{
					list.Add(curveOfBestFit[i].X);
					continue;
				}
				double num10 = (y - curveOfBestFit[i].Y) / (curveOfBestFit[i + 1].Y - curveOfBestFit[i].Y);
				double num11 = curveOfBestFit[i + 1].X - curveOfBestFit[i].X;
				double num12 = num10 * num11 + curveOfBestFit[i].X;
				if ((!minimumX.HasValue || num12 >= minimumX.Value) && (!maximumX.HasValue || num12 <= maximumX.Value))
				{
					list.Add(num12);
				}
			}
			if (list.Count == 0)
			{
				double num13 = num9 - num8;
				num8 -= num13 / 4.0;
				num9 += num13 / 4.0;
				curveOfBestFit = GetCurveOfBestFit(num8, num9, 0.01);
				for (int j = 0; j < curveOfBestFit.Count - 2; j++)
				{
					if ((!(curveOfBestFit[j].Y <= y) || !(curveOfBestFit[j + 1].Y >= y)) && (!(curveOfBestFit[j].Y >= y) || !(curveOfBestFit[j + 1].Y <= y)))
					{
						continue;
					}
					if (Math.Abs(curveOfBestFit[j].Y - curveOfBestFit[j + 1].Y) <= double.Epsilon)
					{
						list.Add(curveOfBestFit[j].X);
						continue;
					}
					double num14 = (y - curveOfBestFit[j].Y) / (curveOfBestFit[j + 1].Y - curveOfBestFit[j].Y);
					double num15 = curveOfBestFit[j + 1].X - curveOfBestFit[j].X;
					double num16 = num14 * num15 + curveOfBestFit[j].X;
					if ((!minimumX.HasValue || num16 >= minimumX.Value) && (!maximumX.HasValue || num16 <= maximumX.Value))
					{
						list.Add(num16);
					}
				}
			}
		}
		return list;
	}

	public double GetPeakY(out double X)
	{
		if (degree == 1)
		{
			double num = originalPoints.Min((Point c) => c.X);
			double num2 = originalPoints.Max((Point c) => c.X);
			double num3 = CalculateY(num);
			double num4 = CalculateY(num2);
			if (num3 > num4)
			{
				X = num;
				return num3;
			}
			X = num2;
			return num4;
		}
		if (degree == 2)
		{
			double num5 = originalPoints.Min((Point d) => d.X);
			double num6 = originalPoints.Max((Point d) => d.X);
			double num7 = coefficients[2];
			double num8 = coefficients[1];
			double num9 = coefficients[0];
			if (num7 < -4.94065645841247E-324)
			{
				double num10 = (0.0 - (num8 * num8 - 4.0 * num7 * num9)) / (4.0 * num7);
				List<double> list = CalculateX(num10);
				if (list.Count != 0)
				{
					X = list.First();
					double num11 = num6 - num5;
					if (X >= num5 - num11 * 4.0 && X <= num6 + num11 * 4.0)
					{
						return num10;
					}
				}
			}
			double num12 = CalculateY(num5);
			double num13 = CalculateY(num6);
			if (num12 > num13)
			{
				X = num5;
				return num12;
			}
			X = num6;
			return num13;
		}
		if (degree == 3)
		{
			double num14 = coefficients[3];
			double num15 = coefficients[2];
			double num16 = coefficients[1];
			_ = coefficients[0];
			double num17 = Math.Pow(num15, 2.0) - 3.0 * num14 * num16;
			if (num17 >= 0.0)
			{
				double num18 = (0.0 - num15 + Math.Sqrt(num17)) / (3.0 * num14);
				double num19 = (0.0 - num15 - Math.Sqrt(num17)) / (3.0 * num14);
				double num20 = CalculateY(num18);
				double num21 = CalculateY(num19);
				if (num20 > num21)
				{
					X = num18;
					return num20;
				}
				X = num19;
				return num21;
			}
			X = double.NaN;
			return double.NaN;
		}
		double value = originalPoints.Min((Point c) => c.X);
		double value2 = originalPoints.Max((Point c) => c.X);
		List<Point> curveOfBestFit = GetCurveOfBestFit(value, value2, 0.01);
		double maxY = curveOfBestFit.Max((Point c) => c.Y);
		X = curveOfBestFit.First((Point c) => Math.Abs(c.Y - maxY) <= double.Epsilon).X;
		return maxY;
	}

	private void CalculateCoEfficients()
	{
		double[,] array = new double[degree + 1, degree + 2];
		for (int i = 0; i <= degree; i++)
		{
			array[i, degree + 1] = 0.0;
			foreach (Point originalPoint in originalPoints)
			{
				array[i, degree + 1] -= Math.Pow(originalPoint.X, i) * originalPoint.Y;
			}
			for (int j = 0; j <= degree; j++)
			{
				array[i, j] = 0.0;
				foreach (Point originalPoint2 in originalPoints)
				{
					array[i, j] -= Math.Pow(originalPoint2.X, j + i);
				}
			}
		}
		coefficients = GaussianElimination(array).ToList();
	}

	private double[] GaussianElimination(double[,] coeffs)
	{
		int upperBound = coeffs.GetUpperBound(0);
		int upperBound2 = coeffs.GetUpperBound(1);
		for (int i = 0; i <= upperBound; i++)
		{
			if (coeffs[i, i] == 0.0)
			{
				for (int j = i + 1; j <= upperBound; j++)
				{
					if (coeffs[j, i] != 0.0)
					{
						for (int k = i; k <= upperBound2; k++)
						{
							double num = coeffs[i, k];
							coeffs[i, k] = coeffs[j, k];
							coeffs[j, k] = num;
						}
						break;
					}
				}
			}
			double num2 = coeffs[i, i];
			if (num2 == 0.0)
			{
				error = "There is no unique solution for these points.";
			}
			for (int l = i; l <= upperBound2; l++)
			{
				coeffs[i, l] /= num2;
			}
			for (int m = 0; m <= upperBound; m++)
			{
				if (m != i)
				{
					double num3 = coeffs[m, i];
					for (int n = 0; n <= upperBound2; n++)
					{
						coeffs[m, n] -= coeffs[i, n] * num3;
					}
				}
			}
		}
		double[] array = new double[upperBound + 1];
		for (int num4 = 0; num4 <= upperBound; num4++)
		{
			array[num4] = coeffs[num4, upperBound2];
		}
		return array;
	}

	private double GetIntegrationValue(double x)
	{
		double num = 0.0;
		int num2 = 1;
		foreach (double coefficient in coefficients)
		{
			double num3 = coefficient * x / (double)num2;
			for (int i = 1; i < num2; i++)
			{
				num3 *= x;
			}
			num += num3;
			num2++;
		}
		return num;
	}
}
