using System;
using System.Collections.Generic;

namespace Racelogic.Libraries.Nmea;

public class GpsSample
{
	private IList<Sv> svs;

	private IList<int> svsInSolution;

	public bool Active { get; set; }

	public DateTime Date { get; set; }

	public int Fix { get; set; }

	public char FixMode { get; set; }

	public int FixQuality { get; set; }

	public char FixSelection { get; set; }

	public double GeoidHeight { get; set; }

	public float Hdop { get; set; }

	public double Heading { get; set; }

	public double Height { get; set; }

	public double Knots { get; set; }

	public Latitude Latitude { get; set; }

	public Longitude Longitude { get; set; }

	public double MagneticHeading { get; set; }

	public float MagneticVariation { get; set; }

	public float Pdop { get; set; }

	public IList<Sv> SVs
	{
		get
		{
			return svs;
		}
		set
		{
			svs = value;
			UpdateSvUse();
		}
	}

	public int Satellites { get; set; }

	public int SatellitesInView => GpsSatellitesInView + GlonassSatellitesInView + BeidouSatellitesInView;

	public int GpsSatellitesInView { get; set; }

	public int GlonassSatellitesInView { get; set; }

	public int BeidouSatellitesInView { get; set; }

	public double Speed { get; set; }

	public char Status { get; set; }

	public TimeSpan Time { get; set; }

	public float Vdop { get; set; }

	internal IList<int> SVsInSolution
	{
		get
		{
			return svsInSolution;
		}
		set
		{
			svsInSolution = value;
			UpdateSvUse();
		}
	}

	private void UpdateSvUse()
	{
		if (svs != null && svsInSolution != null)
		{
			for (int i = 0; i < svs.Count; i++)
			{
				Sv sv = svs[i];
				sv.UsedInSolution = svsInSolution.Contains(sv.Prn);
			}
		}
	}
}
