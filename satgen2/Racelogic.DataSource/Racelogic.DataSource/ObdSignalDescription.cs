using Racelogic.Core;

namespace Racelogic.DataSource;

public enum ObdSignalDescription
{
	[LocalizableDescription("EngineRpm", typeof(Resources))]
	EngineRpm,
	[LocalizableDescription("EngineCoolantTemp", typeof(Resources))]
	EngineCoolantTemp,
	[LocalizableDescription("EngineLoad", typeof(Resources))]
	EngineLoad,
	[LocalizableDescription("EngineOilTemperature", typeof(Resources))]
	EngineOilTemperature,
	[LocalizableDescription("FuelPressure", typeof(Resources))]
	FuelPressure,
	[LocalizableDescription("FuelLevel", typeof(Resources))]
	FuelLevel,
	[LocalizableDescription("IntakeAirTemperature", typeof(Resources))]
	IntakeAirTemperature,
	[LocalizableDescription("IntakeManifoldPressure", typeof(Resources))]
	IntakeManifoldPressure,
	[LocalizableDescription("MassAirFlowRate", typeof(Resources))]
	MassAirFlowRate,
	[LocalizableDescription("ThrottlePosition", typeof(Resources))]
	ThrottlePosition,
	[LocalizableDescription("VehicleSpeed", typeof(Resources))]
	VehicleSpeed
}
