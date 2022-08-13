namespace Racelogic.DataSource;

public interface ISplitDefinition
{
	LongitudeMinutes LongitudeMinutes { get; set; }

	LongitudeMinutes LongitudeMinutesMinus10 { get; set; }

	LatitudeMinutes LatitudeMinutes { get; set; }

	LatitudeMinutes LatitudeMinutesMinus10 { get; set; }

	string Name { get; set; }

	SplitType Type { get; set; }

	string Description { get; }

	string FileName { get; set; }
}
