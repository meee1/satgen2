using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Racelogic.Core.Updater;

internal class SoftwareUpdateResult
{
	private string softwareID;

	private Version latestVersion;

	private string downloadurl;

	private string onlineReleaseNotesUrl;

	private string offlineReleaseNotesFileName;

	private byte[] offlineReleaseNotesContent;

	private Exception error;

	private bool cancelled;

	private bool isNewer;

	private string pdfHelpPageId;

	private List<PluginUpdateResult> pluginUpdates = new List<PluginUpdateResult>();

	internal string OnlineReleaseNotesUrl => onlineReleaseNotesUrl;

	internal string OfflineReleaseNotesFileName => offlineReleaseNotesFileName;

	public string SoftwareID => softwareID;

	public string PdfHelpPageId => pdfHelpPageId;

	public bool IsNewer => isNewer;

	public string DownloadUrl => downloadurl;

	public Version LatestVersion => latestVersion;

	public bool Cancelled => cancelled;

	public List<PluginUpdateResult> PluginUpdates => pluginUpdates;

	public Exception Error => error;

	internal SoftwareUpdateResult(Exception ex)
	{
		error = ex;
	}

	internal SoftwareUpdateResult(bool cancelled)
	{
		this.cancelled = cancelled;
	}

	internal SoftwareUpdateResult(XmlNode node, Version currentVersion, List<IUpdaterCompatiblePlugin> pluginsApplicable)
	{
		softwareID = node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "id").Value;
		latestVersion = new Version(node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "latestversion").Value);
		downloadurl = node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "downloadurl").Value;
		Version version = new Version(latestVersion.Major, Math.Max(latestVersion.Minor, 0), Math.Max(latestVersion.Build, 0));
		Version version2 = new Version(currentVersion.Major, Math.Max(currentVersion.Minor, 0), Math.Max(currentVersion.Build, 0));
		onlineReleaseNotesUrl = node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "onlinereleasenotesurl")?.Value;
		offlineReleaseNotesFileName = node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "offlinereleasenotesfilename")?.Value;
		isNewer = version > version2;
		XmlAttribute xmlAttribute = node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "pdfhelppageid");
		if (xmlAttribute != null)
		{
			pdfHelpPageId = xmlAttribute.Value;
		}
		if (pluginsApplicable == null)
		{
			return;
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name.ToLower(CultureInfo.InvariantCulture) == "plugin")
			{
				string id = childNode.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "id").Value;
				IUpdaterCompatiblePlugin correspondingAvailablePlugin = pluginsApplicable.FirstOrDefault((IUpdaterCompatiblePlugin c) => c.PluginID == id || c.GroupID == id);
				pluginUpdates.Add(new PluginUpdateResult(childNode, correspondingAvailablePlugin));
			}
		}
	}

	public void PopulateInstallationEntities()
	{
	}
}
