using Racelogic.Core;

namespace Racelogic.DataSource;

public enum MassUnit
{
	[LocalizableDescription("MassUnit_Kilogram", typeof(Resources))]
	Kilogram,
	[LocalizableDescription("MassUnit_Gram", typeof(Resources))]
	Gram,
	[LocalizableDescription("MassUnit_Pound", typeof(Resources))]
	Pound,
	[LocalizableDescription("MassUnit_Stone", typeof(Resources))]
	Stone,
	[LocalizableDescription("MassUnit_Tonne", typeof(Resources))]
	Tonne,
	[LocalizableDescription("MassUnit_Ton", typeof(Resources))]
	Ton,
	[LocalizableDescription("MassUnit_Newton", typeof(Resources))]
	Newton
}
