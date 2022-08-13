using System;
using System.Collections.Generic;
using System.IO;

namespace Racelogic.Libraries.Nmea;

public class NmeaFile : IDisposable
{
	private readonly Stream stream;

	public TimeSpan Duration { get; set; }

	public TimeSpan End { get; set; }

	public int Length { get; set; }

	public double SamplePeriod { get; set; }

	public IEnumerable<GpsSample> Samples
	{
		get
		{
			stream.Seek(0L, SeekOrigin.Begin);
			StreamReader reader = new StreamReader(stream);
			_ = string.Empty;
			NmeaParser parser = new NmeaParser(ValidateChecksum);
			string text;
			while ((text = reader.ReadLine()) != null)
			{
				foreach (GpsSample item in parser.ParseIncomingData(text + Environment.NewLine))
				{
					if (item != null && !((decimal)item.Time.Ticks < 0.0m))
					{
						yield return item;
					}
				}
			}
		}
	}

	public TimeSpan Start { get; set; }

	public bool ValidateChecksum { get; set; }

	public NmeaFile(string filename)
	{
		stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
		GpsSample gpsSample = null;
		Length = 0;
		foreach (GpsSample sample in Samples)
		{
			if (gpsSample == null)
			{
				Start = sample.Time;
			}
			else
			{
				TimeSpan timeSpan = sample.Time - gpsSample.Time;
				if (timeSpan.TotalSeconds < 0.0)
				{
					timeSpan = timeSpan.Add(TimeSpan.FromDays(1.0));
				}
				Duration += timeSpan;
			}
			Length++;
			gpsSample = sample;
		}
		End = Start + Duration;
		SamplePeriod = Duration.TotalSeconds / ((double)Length - 1.0);
	}

	public void Dispose()
	{
		if (stream != null)
		{
			stream.Close();
			stream.Dispose();
		}
	}
}
