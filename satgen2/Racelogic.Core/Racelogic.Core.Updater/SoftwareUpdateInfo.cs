using System.Collections.Generic;

namespace Racelogic.Core.Updater;

public struct SoftwareUpdateInfo
{
	public List<SoftwareAndPluginInstallationEntity> SoftwareUpdateResult { get; set; }

	public List<PluginSettingInstallationEntity> SettingsUpdateResult { get; set; }

	public List<OfflineHelpContentInstallationEntity> HelpContentUpdateResult { get; set; }

	public bool HasNewReleaseNotes { get; set; }
}
