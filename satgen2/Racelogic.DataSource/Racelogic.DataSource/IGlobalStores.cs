using System.Collections.ObjectModel;

namespace Racelogic.DataSource;

public interface IGlobalStores
{
	ObservableCollection<SplitPoint> Splits { get; set; }

	DistanceMetres GateWidthMetres { get; set; }
}
