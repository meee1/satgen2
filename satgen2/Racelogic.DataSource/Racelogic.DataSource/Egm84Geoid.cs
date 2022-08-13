using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Racelogic.DataSource;

[Obsolete("This class is about to be removed.  Please use Geoid defined in Racelogic.Geodetic.")]
public class Egm84Geoid
{
	private const int DefaultOrder = 180;

	private const double Sqrt03 = 1.7320508075688772;

	private const double Sqrt05 = 2.23606797749979;

	private const double Sqrt13 = 3.6055512754639891;

	private const double Sqrt17 = 4.1231056256176606;

	private const double Sqrt21 = 4.58257569495584;

	private readonly double[] clenshawA;

	private readonly double[] @as;

	private readonly double[] clenshawB;

	private readonly double c2;

	private readonly double[] cnmGeopCoef;

	private readonly double[] cr;

	private readonly double grava;

	private readonly int max;

	private readonly double rkm;

	private readonly double[] s11;

	private readonly double[] s12;

	private readonly double[] snmGeopCoef;

	private readonly double[] sr;

	private readonly double star;

	public Egm84Geoid()
		: this(180)
	{
	}

	public Egm84Geoid(int max)
	{
		this.max = max;
		if (max > 2147483644)
		{
			throw new ArgumentOutOfRangeException("max", "Argument nmax must be < int.MaxValue - 3");
		}
		c2 = 0.00108262998905;
		rkm = 398600441800000.0;
		grava = 9.7803267714;
		star = 0.001931851386;
		int num = LocatingArray(this.max + 3);
		int num2 = LocatingArray(this.max + 1);
		clenshawA = new double[num];
		clenshawB = new double[num];
		cnmGeopCoef = new double[num2];
		snmGeopCoef = new double[num2];
		@as = new double[max + 1];
		cr = new double[max + 1];
		sr = new double[max + 1];
		s11 = new double[max + 3];
		s12 = new double[max + 3];
		Load();
	}

	public double GetSeparation(Geodetic position)
	{
		double latitude = position.Latitude;
		double num = Math.Sin(latitude);
		double num2 = num * num;
		double num3 = Math.Sqrt(1.0 - Datum.Wgs84.EccentricitySquared * num2);
		double num4 = Datum.Wgs84.SquareRootA / num3;
		double num5 = (num4 + position.Altitude) * Math.Cos(latitude);
		double num6 = num5 * num5;
		double num7 = (num4 * (1.0 - Datum.Wgs84.EccentricitySquared) + position.Altitude) * num;
		double num8 = Math.PI / 2.0 - Math.Atan(num7 / Math.Sqrt(num6));
		double num9 = Math.Sin(num8);
		double num10 = Math.Cos(num8);
		double num11 = Datum.Wgs84.SquareRootA / Math.Sqrt(num6 + num7 * num7);
		double num12 = num11 * num11;
		double longitude = position.Longitude;
		double num13 = grava * (1.0 + star * num2) / num3;
		sr[0] = 0.0;
		sr[1] = Math.Sin(longitude);
		cr[0] = 1.0;
		cr[1] = Math.Cos(longitude);
		for (int i = 2; i <= max; i++)
		{
			sr[i] = 2.0 * cr[1] * sr[i - 1] - sr[i - 2];
			cr[i] = 2.0 * cr[1] * cr[i - 1] - cr[i - 2];
		}
		double num14 = 0.0;
		double num15 = 0.0;
		for (int num16 = max; num16 >= 0; num16--)
		{
			for (int num17 = max; num17 >= num16; num17--)
			{
				int num18 = LocatingArray(num17) + num16;
				int num19 = num18 + num17 + 1;
				int num20 = num19 + num17 + 2;
				double num21 = clenshawA[num19] * num11 * num10;
				double num22 = clenshawB[num20] * num12;
				s11[num17] = num21 * s11[num17 + 1] - num22 * s11[num17 + 2] + cnmGeopCoef[num18];
				s12[num17] = num21 * s12[num17 + 1] - num22 * s12[num17 + 2] + snmGeopCoef[num18];
			}
			num15 = num14;
			num14 = (0.0 - @as[num16]) * num9 * num11 * num14 + s11[num16] * cr[num16] + s12[num16] * sr[num16];
		}
		return ((s11[0] + s12[0]) * num11 + num15 * 1.7320508075688772 * num9 * num12) * rkm / (Datum.Wgs84.SquareRootA * (num13 - position.Altitude * 3.086E-06));
	}

	protected void Load()
	{
		using (StreamReader streamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Racelogic.DataSource.Gps.LatLongConversions.EGM180.nor") ?? throw new FileNotFoundException()))
		{
			string input;
			while ((input = streamReader.ReadLine()) != null)
			{
				MatchCollection matchCollection = Regex.Matches(input, "[\\d+-.Ee]+");
				if (matchCollection.Count >= 4)
				{
					if (!short.TryParse(matchCollection[0].Value, out var result))
					{
						result = 0;
					}
					if (!short.TryParse(matchCollection[1].Value, out var result2))
					{
						result2 = 0;
					}
					if (!double.TryParse(matchCollection[2].Value, out var result3))
					{
						result3 = 0.0;
					}
					if (!double.TryParse(matchCollection[3].Value, out var result4))
					{
						result4 = 0.0;
					}
					if (result <= max)
					{
						int num = LocatingArray(result) + result2;
						cnmGeopCoef[num] = result3;
						snmGeopCoef[num] = result4;
					}
				}
			}
		}
		Initialize();
	}

	private static int LocatingArray(int n)
	{
		return (n + 1) * n >> 1;
	}

	private void Initialize()
	{
		double[] array = new double[6] { 0.0, c2, 0.0, 0.0, 0.0, 0.0 };
		int num = 1;
		double num2 = Datum.Wgs84.EccentricitySquared;
		for (int i = 2; i < array.Length; i++)
		{
			num *= -1;
			num2 *= Datum.Wgs84.EccentricitySquared;
			array[i] = (double)num * (3.0 * num2) / (double)((2 * i + 1) * (2 * i + 3)) * ((double)(1 - i) + (double)(5 * i) * c2 / Datum.Wgs84.EccentricitySquared);
		}
		cnmGeopCoef[3] += array[1] / 2.23606797749979;
		cnmGeopCoef[10] += array[2] / 3.0;
		cnmGeopCoef[21] += array[3] / 3.6055512754639891;
		if (max > 6)
		{
			cnmGeopCoef[36] += array[4] / 4.1231056256176606;
		}
		if (max > 9)
		{
			cnmGeopCoef[55] += array[5] / 4.58257569495584;
		}
		for (int j = 0; j <= max; j++)
		{
			@as[j] = 0.0 - Math.Sqrt(1.0 + 1.0 / (double)(2 * (j + 1)));
		}
		for (int k = 0; k <= max; k++)
		{
			for (int l = k + 1; l <= max; l++)
			{
				int num3 = LocatingArray(l) + k;
				int num4 = 2 * l + 1;
				int num5 = (l - k) * (l + k);
				clenshawA[num3] = Math.Sqrt((double)(num4 * (2 * l - 1)) / (double)num5);
				clenshawB[num3] = Math.Sqrt((double)(num4 * (l + k - 1) * (l - k - 1)) / (double)(num5 * (2 * l - 3)));
			}
		}
	}
}
