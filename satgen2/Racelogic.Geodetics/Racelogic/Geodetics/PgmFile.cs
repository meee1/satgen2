using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Racelogic.DataTypes;

namespace Racelogic.Geodetics
{
	internal class PgmFile : IDisposable
	{
		private readonly FileStream pgmStream;

		private readonly int width;

		private readonly int halfWidth;

		private readonly int height;

		private readonly int doubleHeight;

		private readonly double offset = -108.0;

		private readonly double scale = 0.003;

		private readonly int dataOffset;

		private const string PgmFileId = "P5";

		private const string CommentChar = "#";

		private const int BytesPerPixel = 2;

		private const int MaxPixelValue = 65535;

		private readonly SyncLock fileLock = new SyncLock("PGM file access lock", 10000);

		private const int fileCacheCapacity = 24;

		private readonly FixedSizeDictionary<long, int> fileCache = new FixedSizeDictionary<long, int>(24);

		private bool isDisposed;

		public int Width
		{
			[DebuggerStepThrough]
			get
			{
				return width;
			}
		}

		public int Height
		{
			[DebuggerStepThrough]
			get
			{
				return height;
			}
		}

		public double Offset
		{
			[DebuggerStepThrough]
			get
			{
				return offset;
			}
		}

		public double Scale
		{
			[DebuggerStepThrough]
			get
			{
				return scale;
			}
		}

		public PgmFile(string pgmFilePath)
		{
			if (!File.Exists(pgmFilePath))
			{
				throw new ArgumentException("Specified file does not exist: " + pgmFilePath);
			}
			string fileName = Path.GetFileName(pgmFilePath);
			fileLock = new SyncLock("PGM file access lock " + fileName, 10000);
			using (fileLock.Lock())
			{
				pgmStream = new FileStream(pgmFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				ReadHeader(out dataOffset, out width, out height);
			}
			halfWidth = width >> 1;
			doubleHeight = height - 1 << 1;
		}

		public int ReadPixel(int x, int y)
		{
			NormalizeCoordinates(ref x, ref y);
			long key = dataOffset + y * width * 2 + x * 2;
			using (fileLock.Lock())
			{
				if (fileCache.TryGetValue(key, out var value))
				{
					return value;
				}
				byte[] array = new byte[2];
				pgmStream.Seek(key, SeekOrigin.Begin);
				pgmStream.Read(array, 0, 2);
				int num = (array[0] << 8) | array[1];
				fileCache[key] = num;
				return num;
			}
		}

		public double[] ReadPixels(int x, int y, int[] xOffsets, int[] yOffsets)
		{
			double[] array = new double[xOffsets.Length];
			using (fileLock.Lock())
			{
				for (int i = 0; i < xOffsets.Length; i++)
				{
					int x2 = x + xOffsets[i];
					int y2 = y + yOffsets[i];
					NormalizeCoordinates(ref x2, ref y2);
					long key = dataOffset + y2 * width * 2 + x2 * 2;
					if (fileCache.TryGetValue(key, out var value))
					{
						array[i] = value;
						continue;
					}
					byte[] array2 = new byte[2];
					pgmStream.Seek(key, SeekOrigin.Begin);
					pgmStream.Read(array2, 0, 2);
					int num = (array2[0] << 8) | array2[1];
					fileCache[key] = num;
					array[i] = num;
				}
				return array;
			}
		}

		private void NormalizeCoordinates(ref int x, ref int y)
		{
			if (x < 0)
			{
				x += width;
			}
			else if (x >= width)
			{
				x -= width;
			}
			if (y < 0 || y >= height)
			{
				y = ((y >= 0) ? doubleHeight : 0) - y;
				x += ((x < halfWidth) ? halfWidth : (-halfWidth));
			}
		}

		private bool ReadHeader(out int imageDataOffset, out int imageWidth, out int imageHeight)
		{
			using (fileLock.Lock())
			{
				using StreamReader streamReader = new StreamReader(pgmStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
				string text = streamReader.ReadLine();
				if (string.IsNullOrWhiteSpace(text) || text != "P5")
				{
					throw new InvalidDataException("The file is not a PGM file. File: " + pgmStream.Name);
				}
				imageDataOffset = text.Length + 1;
				text = streamReader.ReadLine();
				while (!string.IsNullOrWhiteSpace(text) && text.Substring(0, 1) == "#" && !streamReader.EndOfStream)
				{
					imageDataOffset += text.Length + 1;
					text = streamReader.ReadLine();
				}
				if (string.IsNullOrWhiteSpace(text) || streamReader.EndOfStream)
				{
					throw new InvalidDataException("Invalid PGM file header. File: " + pgmStream.Name);
				}
				string[] array = (from p in text.Split(new char[1] { ' ' })
					where p != null && p.Length != 0
					select p).ToArray();
				if (array.Length != 2 || !int.TryParse(array[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out imageWidth) || !int.TryParse(array[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out imageHeight))
				{
					throw new InvalidDataException("Can't read PGM file image dimensions.  File: " + pgmStream.Name + "  Line:\n" + text);
				}
				imageDataOffset += text.Length + 1;
				text = streamReader.ReadLine();
				if (text != 65535.ToString())
				{
					throw new InvalidDataException($"The maximum pixel value of the PGM file is wrong.  It should be {65535}.  File: {pgmStream.Name}  Line:\n{text}");
				}
				imageDataOffset += text.Length + 1;
				long num = dataOffset + imageWidth * imageHeight * 2;
				if (pgmStream.Length != num)
				{
					throw new InvalidDataException($"The length of the PGM file is wrong. It is {pgmStream.Length}, but should be {num}.  File: {pgmStream.Name}");
				}
			}
			return true;
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
					pgmStream.Close();
				}
			}
		}
	}
}
