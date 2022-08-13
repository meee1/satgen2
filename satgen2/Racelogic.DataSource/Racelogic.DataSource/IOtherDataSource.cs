namespace Racelogic.DataSource;

public interface IOtherDataSource
{
	string Name { get; set; }

	string Units { get; set; }

	CanData Value { get; set; }

	IOtherDataSource Clone();
}
