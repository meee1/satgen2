using System;

namespace Racelogic.DataSource.Nmea;

public class NmeaData
{
	public GgaData GGA { get; set; }

	public GllData GLL { get; set; }

	public GsaData GSA { get; set; }

	public RmcData RMC { get; set; }

	public VtgData VTG { get; set; }

	public ZdaData ZDA { get; set; }

	public GsvData GSV { get; set; }

	public GstData GST { get; set; }

	public TxtData TXT { get; set; }

	public static int GetMessageLength(NmeaMessages requiredMessages)
	{
		NmeaMessages[] obj = (NmeaMessages[])Enum.GetValues(typeof(NmeaMessages));
		int num = 0;
		NmeaMessages[] array = obj;
		foreach (NmeaMessages nmeaMessages in array)
		{
			if ((requiredMessages & nmeaMessages) == nmeaMessages)
			{
				num += nmeaMessages switch
				{
					NmeaMessages.None => 0, 
					NmeaMessages.Gsv => 384, 
					_ => 96, 
				};
			}
		}
		return num;
	}

	public void Clear()
	{
		GGA.Clear();
		GLL.Clear();
		RMC.Clear();
		VTG.Clear();
		ZDA.Clear();
		GSA.Clear();
		GSV.Clear();
		GST.Clear();
		TXT.Clear();
	}

	public NmeaData()
	{
		GGA = new GgaData();
		GLL = new GllData();
		RMC = new RmcData();
		VTG = new VtgData();
		ZDA = new ZdaData();
		GSA = new GsaData();
		GSV = new GsvData();
		GST = new GstData();
		TXT = new TxtData();
	}
}
