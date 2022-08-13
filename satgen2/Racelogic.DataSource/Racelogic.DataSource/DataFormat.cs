using Racelogic.Core;

namespace Racelogic.DataSource;

public enum DataFormat
{
	[LocalizableDescription("DataFormat_Unsigned", typeof(Resources))]
	Unsigned,
	[LocalizableDescription("DataFormat_Signed", typeof(Resources))]
	Signed,
	[LocalizableDescription("DataFormat_PseudoSigned", typeof(Resources))]
	PseudoSigned,
	[LocalizableDescription("DataFormat_Single", typeof(Resources))]
	Single,
	[LocalizableDescription("DataFormat_Double", typeof(Resources))]
	Double
}
