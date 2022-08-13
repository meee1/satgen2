using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace Racelogic.Core.Updater;

public sealed class CheckForUpdates
{
	public enum Product
	{
		Undefined,
		TestSuite,
		FileProcessor,
		CircuitTools,
		CircuitDatabase,
		EcuDatabase,
		VBoxVideo,
		VideoSplit,
		ScreenCapture,
		VBoxHdLite,
		CanGatewayConfig
	}

	internal class ProductInfo
	{
		public string Id { get; }

		public string Catalogue { get; }

		public string CustomUrl { get; }

		public ProductInfo(string identifier, string catalogue, string customUrl)
		{
			Id = identifier;
			Catalogue = catalogue;
			CustomUrl = customUrl;
		}
	}

	public class Result
	{
		public List<SoftwareAndPluginInstallationEntity> AppAndPlugins { get; private set; }

		public List<OfflineHelpContentInstallationEntity> HelpPages { get; private set; }

		public List<PluginSettingInstallationEntity> Settings { get; private set; }

		public Exception Error { get; }

		public bool HasError => Error != null;

		public bool NothingToDo
		{
			get
			{
				if (!AppAndPlugins.Any() && !HelpPages.Any())
				{
					return !Settings.Any();
				}
				return false;
			}
		}

		public Result(List<SoftwareAndPluginInstallationEntity> appsAndPlugins, List<PluginSettingInstallationEntity> settings, List<OfflineHelpContentInstallationEntity> helpPages)
		{
			AppAndPlugins = appsAndPlugins;
			HelpPages = helpPages;
			Settings = settings;
		}

		public Result(Exception exception)
		{
			Error = exception;
		}
	}

	internal const string DefaultCustomUpdateFilename = "CustomUpdateUrl.txt";

	internal const string CircuitDatabaseFilename = "StartFinishDataBase.xml";

	private const string Error_ProductCannotBeUndefined = "Product cannot be \"Undefined\"";

	private const string Error_CircuitDatabaseNotFound = "Circuit Database version file was not found";

	private const string Error_NoPluginFolderNoPluginsSelected = "No Custom Plugin Folder defined and no Plugins selected for Update";

	private const string Error_CatalogueEmpty = "Downloaded catalogue is empty";

	private const string Error_CatalogueNoData = "Downloaded catalogue contains no data";

	private const string Error_ProductNotFound = "Product with identifier of \"{0}\" was not found";

	private static readonly Dictionary<Product, ProductInfo> products = new Dictionary<Product, ProductInfo>();

	private static Dictionary<Product, ProductInfo> Products
	{
		get
		{
			if (!products.Any())
			{
				products.Add(Product.TestSuite, new ProductInfo("VBTS", "", "VBOXTestSuite"));
				products.Add(Product.FileProcessor, new ProductInfo("VBFP", "VBFP/", "VBOXFileProcessor"));
				products.Add(Product.CircuitTools, new ProductInfo("RLCT", "RLCT/", "Circuit Tools"));
				products.Add(Product.CircuitDatabase, new ProductInfo("CircuitDatabase", "Circuit Database/CircuitDatabase", "Start Finish DataBase"));
				products.Add(Product.EcuDatabase, new ProductInfo("VehicleEcuDatabase", "VehicleEcuDatabase/VehicleEcu", "Vehicle Ecu Database"));
				products.Add(Product.VBoxVideo, new ProductInfo("VBOX Video", "VBOX Video/VBoxVideoHd2", "VBOX Video"));
				products.Add(Product.VideoSplit, new ProductInfo("VideoSplit", "VideoSplit/", "VideoSplit"));
				products.Add(Product.ScreenCapture, new ProductInfo("ScreenCapture", "SharedExtensions/ScreenCapture/", "SharedExtensions/ScreenCapture"));
				products.Add(Product.VBoxHdLite, new ProductInfo("VboxHdLite", "VboxHdLite/", "VboxHdLite"));
				products.Add(Product.CanGatewayConfig, new ProductInfo("CGC", "CanGatewayConfig/", "Can Gateway Config"));
			}
			return products;
		}
	}

	private ProductInfo SelectedProduct { get; set; }

	private List<IUpdaterCompatiblePlugin> SelectedPlugins { get; set; }

	private Version CurrentVersion { get; set; }

	private string ProductTitle { get; set; }

	private string AssemblyLocation { get; set; }

	public string CustomPluginFolder { get; set; }

	public string CustomCatalogueUrl { get; set; }

	public bool DownloadNewPlugins { get; set; }

	public CheckForUpdates(Product product, Assembly sourceAssembly, string title)
	{
		if (Network.InternetAvailable())
		{
			if (product == Product.Undefined)
			{
				throw new ArgumentException("Product cannot be \"Undefined\"", "product");
			}
			SelectedProduct = Products[product];
			CurrentVersion = GetAssemblyVersion(sourceAssembly);
			AssemblyLocation = sourceAssembly.Location;
			ProductTitle = title;
			CheckForCustomUpdateUrl(product);
		}
	}

	public CheckForUpdates(Product product, Assembly sourceAssembly, string title, List<IUpdaterCompatiblePlugin> plugins)
	{
		if (Network.InternetAvailable())
		{
			if (product == Product.Undefined)
			{
				throw new ArgumentException("Product cannot be \"Undefined\"", "product");
			}
			SelectedProduct = Products[product];
			CurrentVersion = GetAssemblyVersion(sourceAssembly);
			ProductTitle = title;
			SelectedPlugins = plugins;
			CheckForCustomUpdateUrl(product);
		}
	}

	private CheckForUpdates(Product product, Version version, string title)
	{
		if (Network.InternetAvailable())
		{
			SelectedProduct = Products[product];
			CurrentVersion = version;
			ProductTitle = title;
			CheckForCustomUpdateUrl(product);
		}
	}

	public CheckForUpdates(Product product, string title)
	{
		if (Network.InternetAvailable())
		{
			SelectedProduct = Products[product];
			CurrentVersion = new Version();
			ProductTitle = title;
			CheckForCustomUpdateUrl(product);
		}
	}

	public async Task<Result> GetAsync()
	{
		string catalogue = string.Empty;
		try
		{
			catalogue = await DownloadCatalogueAsync();
		}
		catch (HttpRequestException)
		{
		}
		return ProcessResponse(catalogue);
	}

	private async Task<string> DownloadCatalogueAsync()
	{
		try
		{
			return await new HttpClient().GetStringAsync(new Uri(GetCatalogueUrl(), UriKind.Absolute));
		}
		catch (HttpRequestException)
		{
		}
		return string.Empty;
	}

	public static async Task<Result> CircuitDatabase()
	{
		if (!Network.InternetAvailable())
		{
			return null;
		}
		Version result = new Version();
		string dataPath = GetDataPath(Product.CircuitDatabase, "StartFinishDataBase.xml");
		if (!File.Exists(dataPath))
		{
			return new Result(new FileNotFoundException("Circuit Database version file was not found", dataPath));
		}
		using (XmlReader xmlReader = new XmlTextReader(dataPath))
		{
			while (xmlReader.Read())
			{
				if (xmlReader.NodeType == XmlNodeType.Element && string.Equals(xmlReader.Name, "RacelogicStartFinishDatabase", StringComparison.InvariantCultureIgnoreCase) && xmlReader.HasAttributes)
				{
					Version.TryParse(xmlReader.GetAttribute("version"), out result);
					break;
				}
			}
		}
		return await new CheckForUpdates(Product.CircuitDatabase, result, Resources.UpdaterCaptionCircuitDatabase)
		{
			CustomPluginFolder = Path.GetDirectoryName(dataPath)
		}.GetAsync();
	}

	public static async Task<SoftwareAndPluginInstallationEntity> VehicleEcuDatabase(ushort currentVersion, string packageFolder)
	{
		CheckForUpdates checkForUpdates = new CheckForUpdates(Product.EcuDatabase, new Version(currentVersion, 0), "");
		string text = await checkForUpdates.DownloadCatalogueAsync();
		SoftwareUpdateResult softwareUpdateResult = null;
		if (!string.IsNullOrWhiteSpace(text))
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(text);
			foreach (XmlNode childNode in xmlDocument.ChildNodes)
			{
				if (childNode.IsSoftwareNode() && childNode.HasIdAttribute(Products[Product.EcuDatabase]))
				{
					softwareUpdateResult = new SoftwareUpdateResult(childNode, checkForUpdates.CurrentVersion, checkForUpdates.SelectedPlugins);
					break;
				}
			}
		}
		if (softwareUpdateResult == null)
		{
			softwareUpdateResult = checkForUpdates.ProductNotFound();
		}
		if (softwareUpdateResult.IsNewer)
		{
			return new SoftwareAndPluginInstallationEntity(softwareUpdateResult.SoftwareID, string.Empty, checkForUpdates.ProductTitle, checkForUpdates.CurrentVersion.ToString(), softwareUpdateResult.LatestVersion, CombineUrls(GetCatalogueUrl(Product.EcuDatabase), softwareUpdateResult.DownloadUrl), packageFolder, isPlugin: true, isPrivate: false, isArchive: true);
		}
		return null;
	}

	private string GetPluginFolder()
	{
		if (string.IsNullOrEmpty(CustomPluginFolder) && (SelectedPlugins == null || !SelectedPlugins.Any()) && string.IsNullOrEmpty(AssemblyLocation))
		{
			throw new ArgumentException("No Custom Plugin Folder defined and no Plugins selected for Update");
		}
		if (string.IsNullOrEmpty(CustomPluginFolder))
		{
			if (SelectedPlugins == null || !SelectedPlugins.Any())
			{
				return Path.GetDirectoryName(AssemblyLocation);
			}
			return Path.GetDirectoryName(SelectedPlugins.First().GetType().Assembly.Location);
		}
		return CustomPluginFolder;
	}

	internal static string GetDataPath(Product product, string filename)
	{
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic", Products[product].CustomUrl, filename);
	}

	private void CheckForCustomUpdateUrl(Product product)
	{
		if (product == Product.Undefined)
		{
			throw new ArgumentException("Product cannot be \"Undefined\"", "product");
		}
		if (!string.IsNullOrEmpty(CustomCatalogueUrl))
		{
			return;
		}
		string dataPath = GetDataPath(product, "CustomUpdateUrl.txt");
		if (File.Exists(dataPath))
		{
			string text = File.ReadAllText(dataPath).Trim();
			if (Uri.TryCreate(text, UriKind.Absolute, out var result) && result.Scheme == Uri.UriSchemeHttp)
			{
				CustomCatalogueUrl = text;
			}
		}
	}

	private static Version GetAssemblyVersion(Assembly assembly)
	{
		Version result = null;
		string location = assembly.Location;
		if (!string.IsNullOrEmpty(location))
		{
			result = new Version(FileVersionInfo.GetVersionInfo(location).ProductVersion.Replace("*", "0"));
		}
		return result;
	}

	private static string GetProductUrl(ProductInfo productInfo)
	{
		return "http://www.racelogic.co.uk/Updates/" + productInfo.Catalogue + "Catalogue.xml";
	}

	internal static string GetCatalogueUrl(Product product)
	{
		return GetProductUrl(Products[product]);
	}

	private string GetCatalogueUrl()
	{
		if (string.IsNullOrEmpty(CustomCatalogueUrl))
		{
			return GetProductUrl(SelectedProduct);
		}
		return CustomCatalogueUrl;
	}

	private SoftwareUpdateResult ProductNotFound()
	{
		return new SoftwareUpdateResult(new ArgumentException($"Product with identifier of \"{SelectedProduct.Id}\" was not found"));
	}

	private Result ProcessResponse(string downloadedCatalogue)
	{
		if (string.IsNullOrEmpty(downloadedCatalogue))
		{
			return new Result(new ArgumentException("Downloaded catalogue is empty"));
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(downloadedCatalogue);
		if (xmlDocument.ChildNodes.Count == 0)
		{
			return new Result(new ArgumentException("Downloaded catalogue contains no data"));
		}
		XmlNode xmlNode = xmlDocument.ChildNodes.Cast<XmlNode>().FirstOrDefault((XmlNode x) => x.IsSoftwareNode() && x.HasIdAttribute(SelectedProduct));
		if (xmlNode == null)
		{
			return new Result(new ArgumentNullException("xmlNode", $"Product with identifier of \"{SelectedProduct.Id}\" was not found"));
		}
		SoftwareUpdateResult softwareUpdateResult = new SoftwareUpdateResult(xmlNode, CurrentVersion, SelectedPlugins);
		string catalogueUrl = GetCatalogueUrl();
		List<SoftwareAndPluginInstallationEntity> appAndPlugins = new List<SoftwareAndPluginInstallationEntity>();
		if (softwareUpdateResult.IsNewer)
		{
			appAndPlugins.Add(new SoftwareAndPluginInstallationEntity(softwareUpdateResult.SoftwareID, string.Empty, ProductTitle, CurrentVersion.ToString(), softwareUpdateResult.LatestVersion, CombineUrls(catalogueUrl, softwareUpdateResult.DownloadUrl), null, isPlugin: false, isPrivate: false));
		}
		List<OfflineHelpContentInstallationEntity> helpPages = new List<OfflineHelpContentInstallationEntity> { { softwareUpdateResult.PdfHelpPageId, softwareUpdateResult.SoftwareID } };
		List<PluginSettingInstallationEntity> settings = new List<PluginSettingInstallationEntity>();
		if (softwareUpdateResult.PluginUpdates.Count != 0)
		{
			string pluginFolder = GetPluginFolder();
			softwareUpdateResult.PluginUpdates.ForEach(delegate(PluginUpdateResult u)
			{
				IUpdaterCompatiblePlugin updaterCompatiblePlugin = SelectedPlugins?.Find((IUpdaterCompatiblePlugin p) => string.Equals(p.PluginID, u.ID) || string.Equals(p.GroupID, u.ID));
				if (updaterCompatiblePlugin != null)
				{
					if (new Version(updaterCompatiblePlugin.Version.ToString(3)) < u.LatestVersionWithoutRivision)
					{
						appAndPlugins.Add(new SoftwareAndPluginInstallationEntity(u.ID, SelectedProduct.Id, string.Equals(updaterCompatiblePlugin.GroupID, u.ID, StringComparison.InvariantCultureIgnoreCase) ? updaterCompatiblePlugin.GroupTitle : (string.IsNullOrEmpty(updaterCompatiblePlugin.Title) ? u.Title : updaterCompatiblePlugin.Title), updaterCompatiblePlugin.Version.ToString(), u.LatestVersion, CombineUrls(catalogueUrl, u.DownloadUrl), pluginFolder, isPlugin: true, u.PluginNotInInstaller, u.Archive));
					}
					u.SettingsUpdates.ForEach(delegate(SettingsUpdateResult s)
					{
						settings.Add(new PluginSettingInstallationEntity(u.ID, s, CombineUrls(catalogueUrl, softwareUpdateResult.DownloadUrl)));
					});
					helpPages.Add(u.PdfHelpPageId, u.ID);
				}
				else if (DownloadNewPlugins)
				{
					appAndPlugins.Add(new SoftwareAndPluginInstallationEntity(u.ID, SelectedProduct.Id, u.Title, null, u.LatestVersion, CombineUrls(catalogueUrl, u.DownloadUrl), isPlugin: true, u.PluginNotInInstaller, pluginFolder, u.Archive));
				}
			});
		}
		return new Result(appAndPlugins, settings, helpPages);
	}

	private static string CombineUrls(string leftHandSide, string rightHandSide)
	{
		int num = leftHandSide.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase);
		leftHandSide = leftHandSide.Substring(0, ++num);
		bool num2 = leftHandSide.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase);
		if (num2)
		{
			leftHandSide = leftHandSide.Replace("http://", string.Empty);
		}
		return (num2 ? "http://" : string.Empty) + string.Join("/", (leftHandSide.Replace('\\', '/').TrimEnd(new char[1] { '/' }) + "/" + rightHandSide.Replace('\\', '/').TrimStart(new char[1] { '/' })).Split(new string[1] { "/" }, StringSplitOptions.RemoveEmptyEntries).Distinct());
	}
}
