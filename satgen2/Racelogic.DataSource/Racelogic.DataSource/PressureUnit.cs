using Racelogic.Core;

namespace Racelogic.DataSource;

public enum PressureUnit
{
	[LocalizableDescription("PressureUnit_Psi", typeof(Resources))]
	Psi,
	[LocalizableDescription("PressureUnit_Bar", typeof(Resources))]
	Bar,
	[LocalizableDescription("PressureUnit_Atmosphere", typeof(Resources))]
	Atmosphere,
	[LocalizableDescription("PressureUnit_KiloPascal", typeof(Resources))]
	KiloPascal,
	[LocalizableDescription("PressureUnit_inHG", typeof(Resources))]
	inHg
}
