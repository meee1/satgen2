using System;
using System.Collections.Generic;

namespace Racelogic.Core.Updater;

public class SoftwareAndPluginInstallationEntity
{
	private string id;

	private string downloadUrl;

	private string title;

	private string currentVersion;

	private string availableVersion;

	private bool isPlugin;

	private bool pluginNotInInstaller;

	private string localPath;

	private string pluginTargetFolder;

	private string parentId;

	private Version latestVersion;

	private bool archive;

	private List<PluginSettingInstallationEntity> pluginSettings = new List<PluginSettingInstallationEntity>();

	public string Id => id;

	public string ParentId => parentId;

	public Version LatestVersion => latestVersion;

	public string Title => title;

	public string CurrentVersion => currentVersion;

	public string AvailableVersion => availableVersion;

	public string DownloadUrl => downloadUrl;

	public bool IsPlugin => isPlugin;

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

	public bool PluginNotInInstaller => pluginNotInInstaller;

	public string PluginTargetFolder => pluginTargetFolder;

	public bool Archive => archive;

	internal SoftwareAndPluginInstallationEntity(string id, string parentid, string title, string currentVersion, Version latestVersion, string downloadUrl, bool isPlugin, bool pluginNotInInstaller, string pluginTargetFolder, bool archive = false)
	{
		this.id = id;
		parentId = parentid;
		this.title = title;
		this.currentVersion = currentVersion;
		this.latestVersion = latestVersion;
		availableVersion = latestVersion.ToString();
		this.downloadUrl = SoftwareUpdateChecker.UpdateCatalogueUrl.Substring(0, SoftwareUpdateChecker.UpdateCatalogueUrl.LastIndexOf("/") + 1) + downloadUrl;
		this.isPlugin = isPlugin;
		this.pluginNotInInstaller = pluginNotInInstaller;
		this.pluginTargetFolder = pluginTargetFolder;
		this.archive = archive;
	}

	internal SoftwareAndPluginInstallationEntity(string id, string parentId, string title, string currentVersion, Version latestVersion, string downloadUrl, string installationFolder, bool isPlugin, bool isPrivate, bool isArchive = false)
	{
		this.id = id;
		this.parentId = parentId;
		this.title = title;
		this.currentVersion = currentVersion;
		this.latestVersion = latestVersion;
		availableVersion = latestVersion.ToString();
		this.downloadUrl = downloadUrl;
		pluginTargetFolder = installationFolder;
		this.isPlugin = isPlugin;
		pluginNotInInstaller = isPrivate;
		archive = isArchive;
	}
}
