namespace Racelogic.DataSource.Nmea;

public class GstData
{
	public TimeSeconds UtcFixTime { get; set; }

	public double Rms { get; set; }

	public double StandardDeviationOfSemiMajorAxisOfErrorEllipseMetres { get; set; }

	public double StandardDeviationOfSemiMinorAxisOfErrorEllipseMetres { get; set; }

	public double OrientationOfSemiMajorAxisOfErrorEllipseDegrees { get; set; }

	public double StandardDeviationOfLatitudeErrorMetres { get; set; }

	public double StandardDeviationOfLongitudeErrorMetres { get; set; }

	public double StandardDeviationOfAltitudeErrorMetres { get; set; }

	public void Clear()
	{
		UtcFixTime = 0.0;
		Rms = 0.0;
		StandardDeviationOfSemiMajorAxisOfErrorEllipseMetres = 0.0;
		StandardDeviationOfSemiMinorAxisOfErrorEllipseMetres = 0.0;
		OrientationOfSemiMajorAxisOfErrorEllipseDegrees = 0.0;
		StandardDeviationOfLatitudeErrorMetres = 0.0;
		StandardDeviationOfLongitudeErrorMetres = 0.0;
		StandardDeviationOfAltitudeErrorMetres = 0.0;
	}
}
