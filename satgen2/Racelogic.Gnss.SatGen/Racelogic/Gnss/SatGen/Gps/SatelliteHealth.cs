namespace Racelogic.Gnss.SatGen.Gps
{
	internal enum SatelliteHealth
	{
		OK,
		ParityFailure,
		TelemetryHowFormatFailure,
		ZCountInHowBad,
		SubFrames123Bad,
		SubFrames45Bad,
		AllUploadedDataBad,
		AllDataBad
	}
}
