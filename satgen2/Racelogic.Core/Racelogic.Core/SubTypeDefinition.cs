using System;

namespace Racelogic.Core;

[Serializable]
public struct SubTypeDefinition
{
	public string Description;

	public string PluginGroup;

	public byte Value;

	public SubTypeDefinition(byte value, string description = "", string pluginGroup = "")
	{
		Value = value;
		Description = (string.IsNullOrEmpty(description) ? Resources.Default : description);
		PluginGroup = pluginGroup;
	}

	public override string ToString()
	{
		return Description;
	}
}
