using System;
using System.Collections.Generic;

namespace Racelogic.DataSource.Can;

public interface IDataSignal
{
	List<Tuple<ObdUnitTypes, double, double>> AvailableScaledUnits { get; set; }

	ByteOrder ByteOrder { get; set; }

	DataFormat DataFormat { get; set; }

	bool IsEncrypted { get; set; }

	int Length { get; set; }

	double Maximum { get; set; }

	double Minimum { get; set; }

	float? Multiplexor { get; set; }

	MultiplexorType MultiplexorType { get; set; }

	string Name { get; set; }

	double Offset { get; set; }

	int OriginalStartBit { get; set; }

	double Scale { get; set; }

	int StartBit { get; set; }

	string Units { get; set; }

	string Group { get; set; }
}
