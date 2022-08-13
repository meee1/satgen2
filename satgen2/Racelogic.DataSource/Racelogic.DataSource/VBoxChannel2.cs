using System;
using Racelogic.Core;

namespace Racelogic.DataSource;

[Flags]
public enum VBoxChannel2 : ulong
{
	[LocalizableDescription("VBoxChannel_None", typeof(Resources))]
	None = 0uL,
	[LocalizableDescription("VBoxChannel2_CentrelineDeviation", typeof(Resources))]
	CentrelineDeviation = 1uL,
	[LocalizableDescription("VBoxChannel2_CorrectedDistance", typeof(Resources))]
	[VBoxChannel(typeof(DistanceUnit))]
	CorrectedDistance = 2uL,
	[LocalizableDescription("VBoxChannel2_BeidouSatellites", typeof(Resources))]
	BeidouSatellites = 4uL,
	[LocalizableDescription("VBoxChannel2_PitchAngleKf", typeof(Resources))]
	PitchAngleKf = 8uL,
	[LocalizableDescription("VBoxChannel2_RollAngleKf", typeof(Resources))]
	RollAngleKf = 0x10uL,
	[LocalizableDescription("VBoxChannel2_HeadingKf", typeof(Resources))]
	HeadingKf = 0x20uL,
	[LocalizableDescription("VBoxChannel2_PitchRateImu", typeof(Resources))]
	PitchRateImu = 0x40uL,
	[LocalizableDescription("VBoxChannel2_RollRateImu", typeof(Resources))]
	RollRateImu = 0x80uL,
	[LocalizableDescription("VBoxChannel2_YawRateImu", typeof(Resources))]
	YawRateImu = 0x100uL,
	[LocalizableDescription("VBoxChannel2_XAccelImu", typeof(Resources))]
	XAccelImu = 0x200uL,
	[LocalizableDescription("VBoxChannel2_YAccelImu", typeof(Resources))]
	YAccelImu = 0x400uL,
	[LocalizableDescription("VBoxChannel2_ZAccelImu", typeof(Resources))]
	ZAccelImu = 0x800uL,
	[LocalizableDescription("VBoxChannel2_Date", typeof(Resources))]
	Date = 0x1000uL,
	[LocalizableDescription("VBoxChannel2_PositionQuality", typeof(Resources))]
	PositionQuality = 0x2000uL,
	[LocalizableDescription("VBoxChannel2_T1", typeof(Resources))]
	T1 = 0x4000uL,
	[LocalizableDescription("VBoxChannel2_WheelSpeed1", typeof(Resources))]
	WheelSpeed1 = 0x8000uL,
	[LocalizableDescription("VBoxChannel2_WheelSpeed2", typeof(Resources))]
	WheelSpeed2 = 0x10000uL,
	[LocalizableDescription("VBoxChannel2_HeadingImu2", typeof(Resources))]
	HeadingImu2 = 0x20000uL
}
