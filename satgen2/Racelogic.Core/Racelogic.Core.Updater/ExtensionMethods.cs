using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Racelogic.Core.Updater;

internal static class ExtensionMethods
{
	private const string MainHelpPage = "Main";

	private const string CatalogueId = "id";

	private const string CatalogueSoftware = "software";

	public static void Add(this List<OfflineHelpContentInstallationEntity> list, string source, string id)
	{
		if (!string.IsNullOrEmpty(source))
		{
			new List<string>(source.Split(new char[1] { ',' })).ForEach(delegate(string x)
			{
				string[] array = x.Split(new char[1] { '-' });
				list.Add(new OfflineHelpContentInstallationEntity(id, (array.Length == 1) ? "Main" : array[0], (array.Length == 1) ? array[0] : array[1]));
			});
		}
	}

	public static bool HasIdAttribute(this XmlNode xmlNode, CheckForUpdates.ProductInfo product)
	{
		if (xmlNode?.Attributes == null)
		{
			return false;
		}
		return xmlNode.Attributes.Cast<XmlAttribute>().Any((XmlAttribute xmlAttribute) => string.Equals(xmlAttribute.Name, "id", StringComparison.InvariantCultureIgnoreCase) && string.Equals(xmlAttribute.Value, product.Id, StringComparison.InvariantCultureIgnoreCase));
	}

	public static bool IsSoftwareNode(this XmlNode xmlNode)
	{
		return string.Equals(xmlNode.Name, "software", StringComparison.InvariantCultureIgnoreCase);
	}
}
