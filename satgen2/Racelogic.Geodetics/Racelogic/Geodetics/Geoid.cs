using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Racelogic.DataTypes;
using Racelogic.Maths;

namespace Racelogic.Geodetics
{
	public class Geoid : IDisposable
	{
		private const int StencilSize = 12;

		private const int Nterms = 10;

		private readonly double[] t = new double[10];

		private const double C0 = 240.0;

		private const double OneOverC0 = 0.0041666666666666666;

		private static readonly double[] C3 = new double[120]
		{
			9.0, -18.0, -88.0, 0.0, 96.0, 90.0, 0.0, 0.0, -60.0, -20.0,
			-9.0, 18.0, 8.0, 0.0, -96.0, 30.0, 0.0, 0.0, 60.0, -20.0,
			9.0, -88.0, -18.0, 90.0, 96.0, 0.0, -20.0, -60.0, 0.0, 0.0,
			186.0, -42.0, -42.0, -150.0, -96.0, -150.0, 60.0, 60.0, 60.0, 60.0,
			54.0, 162.0, -78.0, 30.0, -24.0, -90.0, -60.0, 60.0, -60.0, 60.0,
			-9.0, -32.0, 18.0, 30.0, 24.0, 0.0, 20.0, -60.0, 0.0, 0.0,
			-9.0, 8.0, 18.0, 30.0, -96.0, 0.0, -20.0, 60.0, 0.0, 0.0,
			54.0, -78.0, 162.0, -90.0, -24.0, 30.0, 60.0, -60.0, 60.0, -60.0,
			-54.0, 78.0, 78.0, 90.0, 144.0, 90.0, -60.0, -60.0, -60.0, -60.0,
			9.0, -8.0, -18.0, -30.0, -24.0, 0.0, 20.0, 60.0, 0.0, 0.0,
			-9.0, 18.0, -32.0, 0.0, 24.0, 30.0, 0.0, 0.0, -60.0, 20.0,
			9.0, -18.0, -8.0, 0.0, -24.0, -30.0, 0.0, 0.0, 60.0, 20.0
		};

		private const double C0n = 372.0;

		private const double OneOverC0n = 0.0026881720430107529;

		private static readonly double[] C3n = new double[120]
		{
			0.0, 0.0, -131.0, 0.0, 138.0, 144.0, 0.0, 0.0, -102.0, -31.0,
			0.0, 0.0, 7.0, 0.0, -138.0, 42.0, 0.0, 0.0, 102.0, -31.0,
			62.0, 0.0, -31.0, 0.0, 0.0, -62.0, 0.0, 0.0, 0.0, 31.0,
			124.0, 0.0, -62.0, 0.0, 0.0, -124.0, 0.0, 0.0, 0.0, 62.0,
			124.0, 0.0, -62.0, 0.0, 0.0, -124.0, 0.0, 0.0, 0.0, 62.0,
			62.0, 0.0, -31.0, 0.0, 0.0, -62.0, 0.0, 0.0, 0.0, 31.0,
			0.0, 0.0, 45.0, 0.0, -183.0, -9.0, 0.0, 93.0, 18.0, 0.0,
			0.0, 0.0, 216.0, 0.0, 33.0, 87.0, 0.0, -93.0, 12.0, -93.0,
			0.0, 0.0, 156.0, 0.0, 153.0, 99.0, 0.0, -93.0, -12.0, -93.0,
			0.0, 0.0, -45.0, 0.0, -3.0, 9.0, 0.0, 93.0, -18.0, 0.0,
			0.0, 0.0, -55.0, 0.0, 48.0, 42.0, 0.0, 0.0, -84.0, 31.0,
			0.0, 0.0, -7.0, 0.0, -48.0, -42.0, 0.0, 0.0, 84.0, 31.0
		};

		private const double C0s = 372.0;

		private const double OneOverC0s = 0.0026881720430107529;

		private static readonly double[] C3s = new double[120]
		{
			18.0, -36.0, -122.0, 0.0, 120.0, 135.0, 0.0, 0.0, -84.0, -31.0,
			-18.0, 36.0, -2.0, 0.0, -120.0, 51.0, 0.0, 0.0, 84.0, -31.0,
			36.0, -165.0, -27.0, 93.0, 147.0, -9.0, 0.0, -93.0, 18.0, 0.0,
			210.0, 45.0, -111.0, -93.0, -57.0, -192.0, 0.0, 93.0, 12.0, 93.0,
			162.0, 141.0, -75.0, -93.0, -129.0, -180.0, 0.0, 93.0, -12.0, 93.0,
			-36.0, -21.0, 27.0, 93.0, 39.0, 9.0, 0.0, -93.0, -18.0, 0.0,
			0.0, 0.0, 62.0, 0.0, 0.0, 31.0, 0.0, 0.0, 0.0, -31.0,
			0.0, 0.0, 124.0, 0.0, 0.0, 62.0, 0.0, 0.0, 0.0, -62.0,
			0.0, 0.0, 124.0, 0.0, 0.0, 62.0, 0.0, 0.0, 0.0, -62.0,
			0.0, 0.0, 62.0, 0.0, 0.0, 31.0, 0.0, 0.0, 0.0, -31.0,
			-18.0, 36.0, -64.0, 0.0, 66.0, 51.0, 0.0, 0.0, -102.0, 31.0,
			18.0, -36.0, 2.0, 0.0, -66.0, -51.0, 0.0, 0.0, 102.0, 31.0
		};

		private static Geoid egm84;

		private static Geoid egm96;

		private static Geoid egm2008;

		private readonly GravitationalModel gravitationalModel;

		private readonly PgmFile pgmFileReader;

		private readonly int width;

		private readonly int height;

		private readonly int halfHeight;

		private readonly double longRes;

		private readonly double latRes;

		private readonly double offset;

		private readonly double scale;

		private readonly SyncLock separationLock = new SyncLock("Geoid separation lock", 10000);

		private int lastX;

		private int lastY;

		private readonly int[] xOffsets = new int[12]
		{
			0, 1, -1, 0, 1, 2, -1, 0, 1, 2,
			0, 1
		};

		private readonly int[] yOffsets = new int[12]
		{
			-1, -1, 0, 0, 0, 0, 1, 1, 1, 1,
			2, 2
		};

		private bool isDisposed;

		public static Geoid Egm84
		{
			get
			{
				if (egm84 == null)
				{
					string highestResolutionPgmFile = GetHighestResolutionPgmFile("Egm84");
					if (highestResolutionPgmFile != null)
					{
						egm84 = new Geoid(GravitationalModel.Egm84, highestResolutionPgmFile);
					}
				}
				return egm84;
			}
		}

		public static Geoid Egm96
		{
			get
			{
				if (egm96 == null)
				{
					string highestResolutionPgmFile = GetHighestResolutionPgmFile("Egm96");
					if (highestResolutionPgmFile != null)
					{
						egm96 = new Geoid(GravitationalModel.Egm96, highestResolutionPgmFile);
					}
				}
				return egm96;
			}
		}

		public static Geoid Egm2008
		{
			get
			{
				if (egm2008 == null)
				{
					string highestResolutionPgmFile = GetHighestResolutionPgmFile("Egm2008");
					if (highestResolutionPgmFile != null)
					{
						egm2008 = new Geoid(GravitationalModel.Egm2008, highestResolutionPgmFile);
					}
				}
				return egm2008;
			}
		}

		public GravitationalModel GravitationalModel
		{
			[DebuggerStepThrough]
			get
			{
				return gravitationalModel;
			}
		}

		public Geoid(GravitationalModel gravitationalModel, string egmFilePath)
		{
			this.gravitationalModel = gravitationalModel;
			pgmFileReader = new PgmFile(egmFilePath);
			width = pgmFileReader.Width;
			height = pgmFileReader.Height;
			halfHeight = height - 1 >> 1;
			longRes = (double)pgmFileReader.Width * 0.15915494309189535;
			latRes = (double)(pgmFileReader.Height - 1) * (1.0 / Math.PI);
			offset = pgmFileReader.Offset;
			scale = pgmFileReader.Scale;
		}

		public static Geoid FromGravitationalModel(GravitationalModel gravitationalModel)
		{
			return gravitationalModel switch
			{
				GravitationalModel.Egm84 => Egm84, 
				GravitationalModel.Egm96 => Egm96, 
				GravitationalModel.Egm2008 => Egm2008, 
				_ => null, 
			};
		}

		public double GetSeparation(in Geodetic position)
		{
			double num = position.Longitude * longRes;
			double num2 = (0.0 - position.Latitude) * latRes;
			int num3 = (int)num.Floor();
			int num4 = Math.Min(halfHeight - 1, (int)num2.Floor());
			num -= (double)num3;
			num2 -= (double)num4;
			num3 += ((num3 < 0) ? width : ((num3 >= width) ? (-width) : 0));
			num4 += halfHeight;
			double num7;
			using (separationLock.Lock())
			{
				if (num3 != lastX || num4 != lastY)
				{
					lastX = num3;
					lastY = num4;
					double[] array = pgmFileReader.ReadPixels(num3, num4, xOffsets, yOffsets);
					double[] array2;
					double num5;
					if (num4 == 0)
					{
						array2 = C3n;
						num5 = 0.0026881720430107529;
					}
					else if (num4 == height - 2)
					{
						array2 = C3s;
						num5 = 0.0026881720430107529;
					}
					else
					{
						array2 = C3;
						num5 = 0.0041666666666666666;
					}
					for (int i = 0; i < 10; i++)
					{
						double num6 = 0.0;
						for (int j = 0; j < 12; j++)
						{
							num6 += array[j] * array2[10 * j + i];
						}
						num6 *= num5;
						t[i] = num6;
					}
				}
				num7 = t[0] + num * (t[1] + num * (t[3] + num * t[6])) + num2 * (t[2] + num * (t[4] + num * t[7]) + num2 * (t[5] + num * t[8] + num2 * t[9]));
			}
			return offset + scale * num7;
		}

		private static string GetHighestResolutionPgmFile([CallerMemberName] string geoidName = null)
		{
			string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Geoids");
			string searchPattern = geoidName + "*.pgm";
			return (from s in Directory.GetFiles(path, searchPattern)
				orderby s
				select s).ToArray().FirstOrDefault();
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!isDisposed)
			{
				isDisposed = true;
				if (disposing)
				{
					pgmFileReader.Dispose();
				}
			}
		}
	}
}
