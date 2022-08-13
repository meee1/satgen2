using Racelogic.Core;

namespace Racelogic.DataSource;

public enum MultiplexorType
{
	[LocalizableDescription("MultiplexorType_MultiplexorSignal", typeof(Resources))]
	MultiplexorSignal,
	[LocalizableDescription("MultiplexorType_Signal", typeof(Resources))]
	Signal,
	[LocalizableDescription("MultiplexorType_MultiplexedSignal", typeof(Resources))]
	MultiplexedSignal
}
