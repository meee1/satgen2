namespace Racelogic.DataSource;

public interface IGlobalStoresCOM
{
	SplitPoint[] Splits { get; set; }

	void ClearSplits();

	void AddSplit(ISplitDefinition split);
}
