using System;

namespace Racelogic.Core.Updater;

public class PluginSettingInstallationEntity
{
	private SettingsUpdateResult result;

	private string localPath;

	private string parentId;

	private string downloadUrl;

	public string Title => result.Title;

	public string Id => result.Id;

	public string ParentId => parentId;

	public int OrderBy => result.OrderBy;

	public Version LatestVersion => result.LatestVersion;

	public bool DoNotOverwrite => result.DoNotOverwrite;

	public string DownloadUrl => downloadUrl;

	public string LocalPath
	{
		get
		{
			return localPath;
		}
		set
		{
			localPath = value;
		}
	}

	internal PluginSettingInstallationEntity(string pluginid, SettingsUpdateResult result)
	{
		parentId = pluginid;
		this.result = result;
		downloadUrl = SoftwareUpdateChecker.UpdateCatalogueUrl.Substring(0, SoftwareUpdateChecker.UpdateCatalogueUrl.LastIndexOf("/") + 1) + result.DownloadUrl;
	}

	internal PluginSettingInstallationEntity(string pluginId, SettingsUpdateResult result, string downloadUrl)
	{
		parentId = pluginId;
		this.result = result;
		this.downloadUrl = downloadUrl;
	}
}
