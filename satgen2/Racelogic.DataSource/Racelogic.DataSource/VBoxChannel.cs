using System;
using Racelogic.Core;

namespace Racelogic.DataSource;

[Flags]
public enum VBoxChannel : ulong
{
	[LocalizableDescription("VBoxChannel_None", typeof(Resources))]
	None = 0uL,
	[LocalizableDescription("VBoxChannel_Satellites", typeof(Resources))]
	Satellites = 1uL,
	[LocalizableDescription("VBoxChannel_UtcTime", typeof(Resources))]
	[VBoxChannel(typeof(TimeUnit))]
	UtcTime = 2uL,
	[LocalizableDescription("VBoxChannel_Latitude", typeof(Resources))]
	[VBoxChannel(typeof(LatLongUnit))]
	Latitude = 4uL,
	[LocalizableDescription("VBoxChannel_Longitude", typeof(Resources))]
	[VBoxChannel(typeof(LatLongUnit))]
	Longitude = 8uL,
	[LocalizableDescription("VBoxChannel_Speed", typeof(Resources))]
	[VBoxChannel(typeof(SpeedUnit))]
	Speed = 0x10uL,
	[LocalizableDescription("VBoxChannel_Heading", typeof(Resources))]
	[VBoxChannel(typeof(AngleUnit))]
	Heading = 0x20uL,
	[LocalizableDescription("VBoxChannel_Height", typeof(Resources))]
	[VBoxChannel(typeof(DistanceUnit))]
	Height = 0x40uL,
	[LocalizableDescription("VBoxChannel_VerticalVelocity", typeof(Resources))]
	[VBoxChannel(typeof(SpeedUnit))]
	VerticalVelocity = 0x80uL,
	[LocalizableDescription("VBoxChannel_LongitudinalAcceleration", typeof(Resources))]
	[VBoxChannel(typeof(AccelerationUnit))]
	LongitudinalAcceleration = 0x100uL,
	[LocalizableDescription("VBoxChannel_LateralAcceleration", typeof(Resources))]
	[VBoxChannel(typeof(AccelerationUnit))]
	LateralAcceleration = 0x200uL,
	[LocalizableDescription("VBoxChannel_BrakeDistance", typeof(Resources))]
	[VBoxChannel(typeof(DistanceUnit))]
	BrakeDistance = 0x400uL,
	[LocalizableDescription("VBoxChannel_Distance", typeof(Resources))]
	[VBoxChannel(typeof(DistanceUnit))]
	Distance = 0x800uL,
	[LocalizableDescription("VBoxChannel_InternalA2D1", typeof(Resources))]
	InternalA2D1 = 0x1000uL,
	[LocalizableDescription("VBoxChannel_InternalA2D2", typeof(Resources))]
	InternalA2D2 = 0x2000uL,
	[LocalizableDescription("VBoxChannel_InternalA2D3", typeof(Resources))]
	InternalA2D3 = 0x4000uL,
	[LocalizableDescription("VBoxChannel_InternalA2D4", typeof(Resources))]
	InternalA2D4 = 0x8000uL,
	[LocalizableDescription("VBoxChannel_GlonassSatellites", typeof(Resources))]
	GlonassSatellites = 0x10000uL,
	[LocalizableDescription("VBoxChannel_GpsSatellites", typeof(Resources))]
	GpsSatellites = 0x20000uL,
	[LocalizableDescription("VBoxChannel_Yaw01YawRate", typeof(Resources))]
	Yaw01YawRate = 0x40000uL,
	[LocalizableDescription("VBoxChannel_Yaw01LateralAcceleration", typeof(Resources))]
	Yaw01LateralAcceleration = 0x80000uL,
	[LocalizableDescription("VBoxChannel_Yaw01Status", typeof(Resources))]
	Yaw01Status = 0x100000uL,
	[LocalizableDescription("VBoxChannel_Drift", typeof(Resources))]
	Drift = 0x200000uL,
	[LocalizableDescription("VBoxChannel_VBox3Rms_VBMiniYaw", typeof(Resources))]
	VBox3Rms_VBMiniYaw = 0x400000uL,
	[LocalizableDescription("VBoxChannel_SolutionType", typeof(Resources))]
	SolutionType = 0x800000uL,
	[LocalizableDescription("VBoxChannel_VelocityQuality", typeof(Resources))]
	VelocityQuality = 0x1000000uL,
	[LocalizableDescription("VBoxChannel_InternalTemperature", typeof(Resources))]
	InternalTemperature = 0x2000000uL,
	[LocalizableDescription("VBoxChannel_CompactFlashBufferSize", typeof(Resources))]
	CompactFlashBufferSize = 0x4000000uL,
	[LocalizableDescription("VBoxChannel_MemoryUsed", typeof(Resources))]
	MemoryUsed = 0x8000000uL,
	[LocalizableDescription("VBoxChannel_TriggerEventTime", typeof(Resources))]
	TriggerEventTime = 0x10000000uL,
	[LocalizableDescription("VBoxChannel_Event2Time", typeof(Resources))]
	Event2Time = 0x20000000uL,
	[LocalizableDescription("VBoxChannel_Battery1Voltage", typeof(Resources))]
	Battery1Voltage = 0x40000000uL,
	[LocalizableDescription("VBoxChannel_Battery2Voltage", typeof(Resources))]
	Battery2Voltage = 0x80000000uL,
	[LocalizableDescription("VBoxChannel_AviFileIndex", typeof(Resources))]
	AviFileIndex = 0x40000000000000uL,
	[LocalizableDescription("VBoxChannel_AviSyncTime", typeof(Resources))]
	AviSyncTime = 0x80000000000000uL,
	[LocalizableDescription("VBoxChannel_WavFile", typeof(Resources))]
	WavFile = 0x100000000000000uL,
	[LocalizableDescription("VBoxChannel_DualAntenna", typeof(Resources))]
	DualAntenna = 0x200000000000000uL,
	[LocalizableDescription("VBoxChannel_BrakeTrigger", typeof(Resources))]
	BrakeTrigger = 0x400000000000000uL,
	[LocalizableDescription("VBoxChannel_Dgps", typeof(Resources))]
	Dgps = 0x800000000000000uL,
	[LocalizableDescription("VBoxChannel_TimeFromTestStart", typeof(Resources))]
	TimeFromTestStart = 0x1000000000000000uL,
	[LocalizableDescription("VBoxChannel_EndOfAcceleration", typeof(Resources))]
	EndOfAcceleration = 0x2000000000000000uL
}
