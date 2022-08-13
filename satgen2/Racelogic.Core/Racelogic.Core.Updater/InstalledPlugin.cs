using System;

namespace Racelogic.Core.Updater;

public class InstalledPlugin : IUpdaterCompatiblePlugin
{
	public string PluginID { get; set; }

	public string GroupID { get; set; }

	public string GroupTitle { get; set; }

	public string Title { get; set; }

	public Version Version { get; set; }
}
