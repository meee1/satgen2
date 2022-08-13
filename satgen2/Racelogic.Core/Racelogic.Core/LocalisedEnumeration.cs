using System;

namespace Racelogic.Core;

internal class LocalisedEnumeration : GlobalisedEnumConverter
{
	public LocalisedEnumeration(Type type)
		: base(type, Resources.ResourceManager)
	{
	}
}
