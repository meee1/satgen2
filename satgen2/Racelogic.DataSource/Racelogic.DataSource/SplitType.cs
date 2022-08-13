using System.Runtime.InteropServices;
using Racelogic.Core;

namespace Racelogic.DataSource;

[ComVisible(true)]
public enum SplitType
{
	[LocalizableDescription("SplitType_None", typeof(Resources))]
	None,
	[LocalizableDescription("SplitType_StartFinish", typeof(Resources))]
	StartFinish,
	[LocalizableDescription("SplitType_Finish", typeof(Resources))]
	Finish,
	[LocalizableDescription("SplitType_Split", typeof(Resources))]
	Split,
	[LocalizableDescription("SplitType_SectorStart", typeof(Resources))]
	SectorStart,
	[LocalizableDescription("SplitType_SectorEnd", typeof(Resources))]
	SectorEnd,
	[LocalizableDescription("SplitType_Start", typeof(Resources))]
	Start,
	[LocalizableDescription("SplitType_PitLaneStart", typeof(Resources))]
	PitLaneStart,
	[LocalizableDescription("SplitType_PitLaneFinish", typeof(Resources))]
	PitLaneFinish
}
