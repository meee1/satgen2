using System;
using System.Collections.Generic;

namespace Racelogic.DataSource;

public interface ICanSignal
{
	string Name { get; set; }

	string Units { get; set; }

	double Scale { get; set; }

	double Offset { get; set; }

	int Identifier { get; set; }

	byte StartBit { get; set; }

	byte Length { get; set; }

	byte DataLengthCode { get; set; }

	bool SupportsDataLengthCode { get; }

	ByteOrder ByteOrder { get; set; }

	bool IsEncrypted { get; set; }

	bool IsExtended { get; set; }

	DataFormat DataFormat { get; set; }

	float? Multiplexor { get; set; }

	MultiplexorType MultiplexorType { get; set; }

	List<Tuple<ObdUnitTypes, double, double>> AvailableScaledUnits { get; set; }
}
