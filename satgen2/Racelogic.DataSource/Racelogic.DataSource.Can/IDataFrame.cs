using System.Collections.ObjectModel;

namespace Racelogic.DataSource.Can;

public interface IDataFrame
{
	byte DataLengthCode { get; set; }

	int Identifier { get; set; }

	bool IsEncrypted { get; set; }

	bool IsExtendedIdentifier { get; set; }

	bool IsFlexibleDataRate { get; set; }

	ObservableCollection<IDataSignal> ContainedSignals { get; set; }

	string Name { get; set; }
}
