using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Racelogic.Core.Updater;

internal class PluginUpdateResult
{
	private string id;

	private string title;

	private bool archive;

	private Version latestVersion;

	private string downloadurl;

	private bool pluginNotInInstaller;

	private bool isNewer;

	private string pdfHelpPageId;

	private List<SettingsUpdateResult> settingsUpdates = new List<SettingsUpdateResult>();

	public string ID => id;

	public string Title => title;

	public bool Archive => archive;

	public string PdfHelpPageId => pdfHelpPageId;

	public bool IsNewer => isNewer;

	public Version LatestVersion => latestVersion;

	public Version LatestVersionWithoutRivision
	{
		get
		{
			if (!(latestVersion != null))
			{
				return null;
			}
			return new Version(latestVersion.Major, latestVersion.Minor, latestVersion.Build);
		}
	}

	public string DownloadUrl => downloadurl;

	public bool PluginNotInInstaller => pluginNotInInstaller;

	public List<SettingsUpdateResult> SettingsUpdates => settingsUpdates;

	internal PluginUpdateResult(XmlNode node, IUpdaterCompatiblePlugin correspondingAvailablePlugin)
	{
		id = node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "id").Value;
		title = Convert.ToString(GetAttributeValue(node, "title"));
		archive = Convert.ToBoolean(GetAttributeValue(node, "archive"));
		latestVersion = new Version(node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "latestversion").Value);
		isNewer = correspondingAvailablePlugin == null || latestVersion > correspondingAvailablePlugin.Version;
		XmlAttribute xmlAttribute = node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "pdfhelppageid");
		if (xmlAttribute != null)
		{
			pdfHelpPageId = xmlAttribute.Value;
		}
		downloadurl = node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "downloadurl").Value;
		if (node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "pluginnotininstaller") != null)
		{
			try
			{
				pluginNotInInstaller = Convert.ToBoolean(node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "pluginnotininstaller").Value);
			}
			catch
			{
				pluginNotInInstaller = true;
			}
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name.ToLower(CultureInfo.InvariantCulture) == "settings")
			{
				settingsUpdates.Add(new SettingsUpdateResult(childNode));
			}
		}
	}

	private object GetAttributeValue(XmlNode node, string attributeToFind)
	{
		string result = null;
		XmlAttribute xmlAttribute = node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == attributeToFind);
		if (xmlAttribute != null)
		{
			result = xmlAttribute.Value;
		}
		return result;
	}
}
