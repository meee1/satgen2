using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Racelogic.Libraries.Nmea.Sentences;
using Racelogic.Utilities;

namespace Racelogic.Libraries.Nmea;

public class NmeaParser : INmeaParser
{
	private const string NewLine = "\r\n";

	private const double KnotsToKph = 1.852;

	private static readonly Regex TimeRegex = new Regex("\\$G(.)(...),(\\d{6}(\\.\\d+)?)?");

	private readonly StringBuilder dataBuffer = new StringBuilder();

	private readonly IList<string> sentences = new List<string>();

	private readonly bool validateChecksum = true;

	private string lastGpsSampleTime;

	private DateTime date;

	public event GpsSampleReceived SampleReceived;

	public event SerialDataReceived SerialDataReceived;

	public NmeaParser(bool validateChecksum = true)
	{
		this.validateChecksum = validateChecksum;
	}

	public static string NmeaChecksum(string sentence)
	{
		if (sentence == null || sentence.IndexOf("*", StringComparison.Ordinal) < 0)
		{
			return null;
		}
		int num = Math.Max(sentence.IndexOf('$'), 0) + 1;
		int num2 = sentence.IndexOf('*');
		if (num2 < 0)
		{
			num2 = sentence.Length;
		}
		int num3 = Convert.ToByte(sentence[num]);
		for (int i = num + 1; i < num2; i++)
		{
			char value = sentence[i];
			num3 ^= Convert.ToByte(value);
		}
		return num3.ToString("X2");
	}

	public void DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		if (!(sender is ISerialPort serialPort))
		{
			throw new ArgumentException("Sender is not a serial port");
		}
		try
		{
			try
			{
				string data = serialPort.ReadExisting();
				foreach (GpsSample item in ParseIncomingData(data))
				{
					InvokeSampleReceived(new GpsSampleReceivedEventArgs
					{
						Sample = item
					});
				}
			}
			catch (Exception ex)
			{
				RLLogger.GetLogger().LogMessage("Exception reading from serial port");
				RLLogger.GetLogger().LogException(ex);
			}
		}
		catch (Exception ex2)
		{
			RLLogger.GetLogger().LogException(ex2);
		}
	}

	public IEnumerable<GpsSample> ParseIncomingData(string data)
	{
		lock (dataBuffer)
		{
			dataBuffer.Append(data);
			int num;
			while ((num = dataBuffer.ToString().IndexOf("\r\n", StringComparison.Ordinal)) >= 0)
			{
				string text = dataBuffer.ToString(0, num);
				dataBuffer.Remove(0, num + "\r\n".Length);
				if (!NmeaChecksumIsValid(text))
				{
					continue;
				}
				Match match = TimeRegex.Match(text);
				if (match.Success)
				{
					sentences.Add(text);
					string value = match.Groups[3].Value;
					if (!string.IsNullOrEmpty(value) && !value.Equals(lastGpsSampleTime))
					{
						GpsSample gpsSample = GenerateSample(sentences);
						sentences.Clear();
						lastGpsSampleTime = value;
						yield return gpsSample;
					}
				}
			}
		}
	}

	public void Reset()
	{
		dataBuffer.Clear();
		sentences.Clear();
		lastGpsSampleTime = string.Empty;
	}

	private static string TrimToNmea(string input)
	{
		Match match = new Regex("(\\$)G[ABLNP](GST|GGA|GLL|GNS|GSA|GSV|RMC|VTG|ZDA),").Match(input);
		if (!match.Success)
		{
			return string.Empty;
		}
		return input.Substring(match.Index);
	}

	protected GpsSample GenerateSample(IEnumerable<string> incomingSentences)
	{
		GpsSample gpsSample = new GpsSample();
		foreach (string incomingSentence in incomingSentences)
		{
			string text = TrimToNmea(incomingSentence);
			Match match = TimeRegex.Match(text);
			if (!match.Success)
			{
				continue;
			}
			switch (match.Groups[2].Value)
			{
			case "GGA":
			{
				Gga gga = Gga.FromNmea(text);
				gpsSample.Time = gga.Time;
				gpsSample.Latitude = gga.Latitude;
				gpsSample.Longitude = gga.Longitude;
				gpsSample.FixQuality = gga.FixQuality;
				gpsSample.Satellites = gga.Satellites;
				gpsSample.Hdop = gga.Hdop;
				gpsSample.Height = gga.Height;
				gpsSample.GeoidHeight = gga.GeoidHeight;
				break;
			}
			case "GSA":
			{
				Gsa gsa = Gsa.FromNmea(text);
				gpsSample.FixSelection = gsa.FixSelection;
				gpsSample.Fix = gsa.Fix;
				gpsSample.Pdop = gsa.Pdop;
				gpsSample.Hdop = gsa.Hdop;
				gpsSample.Vdop = gsa.Vdop;
				gpsSample.SVsInSolution = ((gpsSample.SVsInSolution == null) ? new List<int>(gsa.SVs) : new List<int>(gpsSample.SVsInSolution.Union(gsa.SVs)));
				break;
			}
			case "GSV":
			{
				Gsv gsv = Gsv.FromNmea(text);
				switch (gsv.Constellation)
				{
				case Constellation.GPS:
					gpsSample.GpsSatellitesInView = gsv.SvsInView;
					break;
				case Constellation.GLONASS:
					gpsSample.GlonassSatellitesInView = gsv.SvsInView;
					break;
				case Constellation.BEIDOU:
					gpsSample.BeidouSatellitesInView = gsv.SvsInView;
					break;
				}
				gpsSample.SVs = ((gpsSample.SVs == null) ? new List<Sv>(gsv.Svs) : new List<Sv>(gpsSample.SVs.Union(gsv.Svs)));
				break;
			}
			case "RMC":
			{
				Rmc rmc = Rmc.FromNmea(text);
				gpsSample.Time = rmc.Time;
				gpsSample.Latitude = rmc.Latitude;
				gpsSample.Longitude = rmc.Longitude;
				gpsSample.Status = rmc.Status;
				gpsSample.Speed = rmc.Speed;
				gpsSample.Knots = rmc.Knots;
				gpsSample.Heading = rmc.Heading;
				gpsSample.Date = rmc.Date;
				gpsSample.MagneticVariation = rmc.MagneticVariation;
				gpsSample.FixMode = rmc.FixMode;
				break;
			}
			case "GLL":
			{
				Gll gll = Gll.FromNmea(text);
				gpsSample.Time = gll.Time;
				gpsSample.Latitude = gll.Latitude;
				gpsSample.Longitude = gll.Longitude;
				gpsSample.Active = gll.Active;
				gpsSample.FixMode = gll.FixMode;
				break;
			}
			case "VTG":
			{
				Vtg vtg = Vtg.FromNmea(text);
				gpsSample.Heading = vtg.Heading;
				gpsSample.MagneticHeading = vtg.MagneticHeading;
				gpsSample.Speed = vtg.Speed;
				gpsSample.Knots = vtg.Knots;
				gpsSample.FixMode = vtg.FixMode;
				break;
			}
			}
		}
		if (gpsSample.Date == DateTime.MinValue)
		{
			gpsSample.Date = date;
		}
		else
		{
			date = gpsSample.Date;
		}
		return gpsSample;
	}

	protected void InvokeSampleReceived(GpsSampleReceivedEventArgs eventargs)
	{
		this.SampleReceived?.Invoke(this, eventargs);
	}

	protected void InvokeSerialDataReceived(SerialDataReceivedEventArgs eventargs)
	{
		this.SerialDataReceived?.Invoke(this, eventargs);
	}

	protected bool NmeaChecksumIsValid(string sentence)
	{
		if (!validateChecksum)
		{
			return true;
		}
		string text = NmeaChecksum(sentence);
		if (string.IsNullOrEmpty(text) || sentence.IndexOf("*", StringComparison.Ordinal) > sentence.Length - 3)
		{
			return false;
		}
		string text2 = sentence.Substring(sentence.IndexOf("*", StringComparison.Ordinal) + 1, 2);
		return text == text2;
	}
}
