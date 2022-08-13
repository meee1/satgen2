using System;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Racelogic.Core.Updater;

public class SettingsUpdateResult
{
	private readonly string id;

	private readonly Version latestversion;

	private readonly string downloadUrl;

	private readonly string title;

	private readonly bool doNotOverwrite;

	private readonly int orderBy;

	public string Id => id;

	public Version LatestVersion => latestversion;

	public string DownloadUrl => downloadUrl;

	public string Title => title;

	public bool DoNotOverwrite => doNotOverwrite;

	public int OrderBy => orderBy;

	public SettingsUpdateResult(XmlNode node)
	{
		id = node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "id").Value;
		latestversion = new Version(node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "latestversion").Value);
		title = node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "title").Value;
		downloadUrl = node.Attributes.Cast<XmlAttribute>().First((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "downloadurl").Value;
		if (node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "donotoverwrite") != null)
		{
			try
			{
				doNotOverwrite = Convert.ToBoolean(node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "donotoverwrite").Value);
			}
			catch
			{
				doNotOverwrite = false;
			}
		}
		if (node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "orderby") != null)
		{
			try
			{
				orderBy = Convert.ToInt32(node.Attributes.Cast<XmlAttribute>().FirstOrDefault((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "orderby").Value);
				return;
			}
			catch
			{
				orderBy = int.MaxValue;
				return;
			}
		}
		orderBy = int.MaxValue;
	}
}
