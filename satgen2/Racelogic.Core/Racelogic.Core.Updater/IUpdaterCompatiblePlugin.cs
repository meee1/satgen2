using System;

namespace Racelogic.Core.Updater;

public interface IUpdaterCompatiblePlugin
{
	string PluginID { get; }

	string GroupID { get; }

	string GroupTitle { get; }

	string Title { get; }

	Version Version { get; }
}
