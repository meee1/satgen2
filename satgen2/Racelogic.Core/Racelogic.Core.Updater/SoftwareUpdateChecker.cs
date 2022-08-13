using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Racelogic.Core.Updater;

public class SoftwareUpdateChecker
{
	public delegate void CheckUpdatesCallback(List<SoftwareAndPluginInstallationEntity> softwareUpdateResult, List<PluginSettingInstallationEntity> settingsUpdateResult, List<OfflineHelpContentInstallationEntity> helpContentUpdateResult);

	public delegate void CheckUpdates2Callback(SoftwareUpdateInfo info);

	private const string HelpContentsVersionFilePath = "\\Versions\\HelpContentVersions.txt";

	private const string SettingsVersionFilePath = "\\Versions\\SettingsFileVersions.txt";

	private const string ReleaseNotesVersionFilePath = "\\Versions\\ReleaseNotesVersions.txt";

	private const string PdfHelpMainPageId = "Main";

	private static string customUpdateUrl = string.Empty;

	private static string pluginTargetFolder;

	private static string softwareID;

	private string mainSoftwareTitle;

	private Version mainSoftwareCurrentVersion;

	private List<IUpdaterCompatiblePlugin> pluginsApplicable;

	private bool downloadNewPlugins;

	private string appStorageFolderName;

	private string updateUrlUsed;

	private static string DefaultCatalogueUrl => "http://www.racelogic.co.uk/" + (string.Equals(softwareID, "VBFP") ? "UpdateVBFP" : "Updates") + "/Catalogue.xml";

	public static string UpdateCatalogueUrl
	{
		get
		{
			if (!string.IsNullOrEmpty(customUpdateUrl))
			{
				return customUpdateUrl;
			}
			return DefaultCatalogueUrl;
		}
		set
		{
			customUpdateUrl = value;
		}
	}

	public static string PluginFolder
	{
		get
		{
			string.IsNullOrEmpty(pluginTargetFolder);
			return pluginTargetFolder;
		}
		set
		{
			pluginTargetFolder = value;
		}
	}

	public SoftwareUpdateChecker(string softwareId, Version mainSoftwareCurrentVersion, string mainSoftwareTitle)
	{
		softwareID = softwareId;
		this.mainSoftwareCurrentVersion = mainSoftwareCurrentVersion;
		this.mainSoftwareTitle = mainSoftwareTitle;
	}

	public SoftwareUpdateChecker(string softwareId, Version mainSoftwareCurrentVersion, string mainSoftwareTitle, List<IUpdaterCompatiblePlugin> pluginsApplicable, bool downloadNewPlugins = false, string AppStorageFolderName = null)
	{
		softwareID = softwareId;
		this.mainSoftwareCurrentVersion = mainSoftwareCurrentVersion;
		this.mainSoftwareTitle = mainSoftwareTitle;
		this.pluginsApplicable = pluginsApplicable;
		this.downloadNewPlugins = downloadNewPlugins;
		appStorageFolderName = AppStorageFolderName;
	}

	public string CheckUpdates(string updateUrl)
	{
		return StartDownloading(updateUrl);
	}

	public void CheckUpdatesAsync(CheckUpdatesCallback callBack, string updateUrl = "")
	{
		new Action<CheckUpdates2Callback, string>(StartDownloadingAsynch).BeginInvoke(delegate(SoftwareUpdateInfo r)
		{
			callBack(r.SoftwareUpdateResult, r.SettingsUpdateResult, r.HelpContentUpdateResult);
		}, updateUrl, null, null);
	}

	public async Task<SoftwareUpdateInfo> CheckUpdatesAsync2(string updateUrl = "")
	{
		SoftwareUpdateInfo result = default(SoftwareUpdateInfo);
		await Task.Run(delegate
		{
			ManualResetEvent evt = new ManualResetEvent(initialState: false);
			new Action<CheckUpdates2Callback, string>(StartDownloadingAsynch).BeginInvoke(delegate(SoftwareUpdateInfo r)
			{
				result = r;
				evt.Set();
			}, updateUrl, null, null);
			evt.WaitOne();
		});
		return result;
	}

	public static void CheckCircuitDatabase(CheckUpdatesCallback checkCircuitDatabaseCallback)
	{
		Version result = new Version();
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic", "Start Finish DataBase", "StartFinishDataBase.xml");
		if (File.Exists(path))
		{
			using TextReader textReader = new StreamReader(path);
			string text = null;
			while ((text = textReader.ReadLine()) != null)
			{
				if (text.StartsWith("<RacelogicStartFinishDatabase version="))
				{
					text = text.Remove(0, text.IndexOf("\"") + 1);
					text = text.Substring(0, text.IndexOf("\""));
					Version.TryParse(text, out result);
					break;
				}
			}
		}
		new SoftwareUpdateChecker("CircuitDatabase", result, "Circuit Database").CheckUpdatesAsync(checkCircuitDatabaseCallback, "http://www.racelogic.co.uk/Updates/Circuit Database/CircuitDatabaseCatalogue.xml");
	}

	public static List<SoftwareAndPluginInstallationEntity> CheckVehicleEcuDatabase(ushort currentVersion, string packageFolder, int packageVersion = 1)
	{
		SoftwareUpdateChecker softwareUpdateChecker = null;
		string text;
		switch (packageVersion)
		{
		case 1:
			softwareUpdateChecker = new SoftwareUpdateChecker("VehicleEcuDatabase", new Version(currentVersion, 0), "");
			text = softwareUpdateChecker.CheckUpdates("http://www.racelogic.co.uk/Updates/VehicleEcuDatabase/VehicleEcuDatabase.xml");
			break;
		case 2:
			softwareUpdateChecker = new SoftwareUpdateChecker("VehicleEcuDatabase2", new Version(currentVersion, 0), "");
			text = softwareUpdateChecker.CheckUpdates("http://www.racelogic.co.uk/Updates/VehicleEcuDatabase2/VehicleEcuDatabase2.xml");
			break;
		default:
			text = null;
			break;
		}
		List<SoftwareAndPluginInstallationEntity> list = new List<SoftwareAndPluginInstallationEntity>();
		SoftwareUpdateResult softwareUpdateResult = null;
		if (!string.IsNullOrWhiteSpace(text) && softwareUpdateChecker != null)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(text);
			foreach (XmlNode childNode in xmlDocument.ChildNodes)
			{
				if (childNode.Name.ToLower(CultureInfo.InvariantCulture) == "software" && childNode.Attributes.Cast<XmlAttribute>().Any((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "id" && c.Value.ToLower(CultureInfo.InvariantCulture) == softwareID.ToLower()))
				{
					softwareUpdateResult = new SoftwareUpdateResult(childNode, softwareUpdateChecker.mainSoftwareCurrentVersion, softwareUpdateChecker.pluginsApplicable);
					break;
				}
			}
		}
		if (softwareUpdateResult == null)
		{
			softwareUpdateResult = new SoftwareUpdateResult(new Exception("Software of ID '" + softwareID + "' not found"));
		}
		if (softwareUpdateResult.IsNewer)
		{
			list.Add(new SoftwareAndPluginInstallationEntity(softwareUpdateResult.SoftwareID, string.Empty, softwareUpdateChecker.mainSoftwareTitle, softwareUpdateChecker.mainSoftwareCurrentVersion.ToString(), softwareUpdateResult.LatestVersion, softwareUpdateResult.DownloadUrl, isPlugin: true, pluginNotInInstaller: false, packageFolder, archive: true));
		}
		return list;
	}

	private string StartDownloading(string updateUrl)
	{
		WebClient webClient = new WebClient();
		string text = "";
		try
		{
			return webClient.DownloadString(new Uri(string.IsNullOrWhiteSpace(updateUrl) ? UpdateCatalogueUrl : updateUrl, UriKind.Absolute));
		}
		catch
		{
			return "";
		}
	}

	private void StartDownloadingAsynch(CheckUpdates2Callback callBack, string updateUrl)
	{
		WebClient webClient = new WebClient();
		webClient.DownloadStringCompleted += client_DownloadStringCompleted;
		webClient.DownloadStringAsync(new Uri(updateUrlUsed = (string.IsNullOrWhiteSpace(updateUrl) ? UpdateCatalogueUrl : updateUrl), UriKind.Absolute), callBack);
	}

	private bool HasReleaseNotesForCurrentSoftwareVersion(out string existingOnlineUrl, out string existingOfflineFile)
	{
		existingOnlineUrl = null;
		existingOfflineFile = null;
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic\\" + (appStorageFolderName ?? mainSoftwareTitle) + "\\Versions\\ReleaseNotesVersions.txt");
		if (File.Exists(path))
		{
			string text = File.ReadAllText(path);
			if (!string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(new char[1] { ',' });
				string input = array[0];
				if (array.Length > 1 && !string.IsNullOrEmpty(array[1]))
				{
					existingOnlineUrl = array[1];
				}
				if (array.Length > 2 && !string.IsNullOrEmpty(array[2]))
				{
					existingOfflineFile = array[2];
				}
				Version.TryParse(input, out var result);
				return result >= mainSoftwareCurrentVersion;
			}
		}
		return false;
	}

	private void RegisterLatestReleaseNotesVersion(string onlineUrl, string offlineFilename)
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic", (appStorageFolderName ?? mainSoftwareTitle) + "\\Versions\\ReleaseNotesVersions.txt");
		Directory.CreateDirectory(Path.GetDirectoryName(path) ?? string.Empty);
		File.WriteAllText(path, $"{mainSoftwareCurrentVersion},{onlineUrl},{offlineFilename}");
	}

	private bool DownOfflineLoadReleaseNotes(string filename)
	{
		string text = updateUrlUsed.Substring(0, updateUrlUsed.LastIndexOf("/"));
		WebClient webClient = new WebClient();
		byte[] array = null;
		try
		{
			string uriString = text + "/" + filename;
			array = webClient.DownloadDataTaskAsync(new Uri(uriString, UriKind.Absolute)).Result;
		}
		catch
		{
		}
		if (array != null)
		{
			File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic\\" + (appStorageFolderName ?? mainSoftwareTitle) + "\\" + filename), array);
			return true;
		}
		return false;
	}

	private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
	{
		CheckUpdates2Callback checkUpdates2Callback = (CheckUpdates2Callback)e.UserState;
		SoftwareUpdateResult softwareUpdateResult = null;
		if (!e.Cancelled && e.Error == null)
		{
			XmlDocument xmlDocument = new XmlDocument();
			string text = e.Result.Replace("&amp;", "&");
			text = text.Replace("&", "&amp;");
			try
			{
				xmlDocument.LoadXml(text);
			}
			catch (XmlException)
			{
			}
			foreach (XmlNode childNode in xmlDocument.ChildNodes)
			{
				if (childNode.Name.ToLower(CultureInfo.InvariantCulture) == "software" && childNode.Attributes.Cast<XmlAttribute>().Any((XmlAttribute c) => c.Name.ToLower(CultureInfo.InvariantCulture) == "id" && c.Value.ToLower(CultureInfo.InvariantCulture) == softwareID.ToLower()))
				{
					softwareUpdateResult = new SoftwareUpdateResult(childNode, mainSoftwareCurrentVersion, pluginsApplicable);
					break;
				}
			}
			if (softwareUpdateResult == null)
			{
				softwareUpdateResult = new SoftwareUpdateResult(new Exception("Software of ID '" + softwareID + "' not found"));
			}
		}
		List<SoftwareAndPluginInstallationEntity> list = new List<SoftwareAndPluginInstallationEntity>();
		List<PluginSettingInstallationEntity> list2 = new List<PluginSettingInstallationEntity>();
		List<OfflineHelpContentInstallationEntity> list3 = new List<OfflineHelpContentInstallationEntity>();
		string text2 = null;
		string text3 = null;
		Version version = null;
		if (softwareUpdateResult != null)
		{
			text2 = softwareUpdateResult.OnlineReleaseNotesUrl;
			text3 = softwareUpdateResult.OfflineReleaseNotesFileName;
			version = softwareUpdateResult.LatestVersion;
			if (pluginsApplicable != null && pluginsApplicable.Any() && string.IsNullOrEmpty(pluginTargetFolder))
			{
				pluginTargetFolder = Path.GetDirectoryName(pluginsApplicable.First().GetType().Assembly.Location);
			}
			if (softwareUpdateResult.IsNewer)
			{
				list.Add(new SoftwareAndPluginInstallationEntity(softwareUpdateResult.SoftwareID, string.Empty, mainSoftwareTitle, mainSoftwareCurrentVersion.ToString(), softwareUpdateResult.LatestVersion, softwareUpdateResult.DownloadUrl, isPlugin: false, pluginNotInInstaller: false, pluginTargetFolder));
			}
			string path = null;
			string text4 = string.Empty;
			string text5 = string.Empty;
			if (!string.IsNullOrEmpty(softwareUpdateResult.PdfHelpPageId))
			{
				if (string.IsNullOrEmpty(text4))
				{
					path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic\\" + (appStorageFolderName ?? mainSoftwareTitle) + "\\Versions\\HelpContentVersions.txt");
					if (File.Exists(path))
					{
						text4 = File.ReadAllText(path);
					}
				}
				string text6 = softwareUpdateResult.SoftwareID + ":" + softwareUpdateResult.LatestVersion.ToString();
				if (!text4.Contains(text6))
				{
					string[] array = softwareUpdateResult.PdfHelpPageId.Split(new char[1] { ',' });
					for (int i = 0; i < array.Length; i++)
					{
						string[] array2 = array[i].Split(new char[1] { '-' });
						if (array2.Length == 1)
						{
							list3.Add(new OfflineHelpContentInstallationEntity(softwareUpdateResult.SoftwareID, "Main", array2[0]));
						}
						else
						{
							list3.Add(new OfflineHelpContentInstallationEntity(softwareUpdateResult.SoftwareID, array2[0], array2[1]));
						}
					}
				}
				if (!text5.Contains(text6))
				{
					text5 = text5 + text6 + "\r\n";
				}
			}
			string path2 = null;
			string text7 = string.Empty;
			string text8 = string.Empty;
			foreach (PluginUpdateResult plugin in softwareUpdateResult.PluginUpdates)
			{
				IUpdaterCompatiblePlugin updaterCompatiblePlugin = pluginsApplicable.FirstOrDefault((IUpdaterCompatiblePlugin c) => c.PluginID == plugin.ID || c.GroupID == plugin.ID);
				if (updaterCompatiblePlugin != null)
				{
					if (new Version(updaterCompatiblePlugin.Version.Major, updaterCompatiblePlugin.Version.Minor, updaterCompatiblePlugin.Version.Build) < plugin.LatestVersionWithoutRivision)
					{
						list.Add(new SoftwareAndPluginInstallationEntity(plugin.ID, softwareID, (updaterCompatiblePlugin.GroupID == plugin.ID) ? updaterCompatiblePlugin.GroupTitle : ((!string.IsNullOrEmpty(updaterCompatiblePlugin.Title)) ? updaterCompatiblePlugin.Title : plugin.Title), updaterCompatiblePlugin.Version.ToString(), plugin.LatestVersion, plugin.DownloadUrl, isPlugin: true, plugin.PluginNotInInstaller, pluginTargetFolder, plugin.Archive));
					}
					if (plugin.SettingsUpdates != null && plugin.SettingsUpdates.Count != 0 && string.IsNullOrEmpty(text7))
					{
						path2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic\\" + (appStorageFolderName ?? mainSoftwareTitle) + "\\Versions\\SettingsFileVersions.txt");
						if (File.Exists(path2))
						{
							text7 = File.ReadAllText(path2);
						}
					}
					foreach (SettingsUpdateResult settingsUpdate in plugin.SettingsUpdates)
					{
						string text9 = plugin.ID + ":" + settingsUpdate.Id + ":" + settingsUpdate.LatestVersion;
						if (!text7.Contains(text9))
						{
							list2.Add(new PluginSettingInstallationEntity(plugin.ID, settingsUpdate));
						}
						if (!text8.Contains(text9))
						{
							text8 = text8 + text9 + "\r\n";
						}
					}
					if (string.IsNullOrEmpty(plugin.PdfHelpPageId))
					{
						continue;
					}
					if (string.IsNullOrEmpty(text4))
					{
						path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic\\" + (appStorageFolderName ?? mainSoftwareTitle) + "\\Versions\\HelpContentVersions.txt");
						if (File.Exists(path))
						{
							text4 = File.ReadAllText(path);
						}
					}
					string text10 = plugin.ID + ":" + plugin.LatestVersion.ToString();
					if (!text4.Contains(text10))
					{
						string[] array = plugin.PdfHelpPageId.Split(new char[1] { ',' });
						for (int i = 0; i < array.Length; i++)
						{
							string[] array3 = array[i].Split(new char[1] { '-' });
							if (array3.Length == 1)
							{
								list3.Add(new OfflineHelpContentInstallationEntity(plugin.ID, "Main", array3[0]));
							}
							else
							{
								list3.Add(new OfflineHelpContentInstallationEntity(plugin.ID, array3[0], array3[1]));
							}
						}
					}
					if (!text5.Contains(text10))
					{
						text5 = text5 + text10 + "\r\n";
					}
				}
				else if (downloadNewPlugins)
				{
					list.Add(new SoftwareAndPluginInstallationEntity(plugin.ID, softwareID, plugin.Title, null, plugin.LatestVersion, plugin.DownloadUrl, isPlugin: true, plugin.PluginNotInInstaller, pluginTargetFolder, plugin.Archive));
				}
			}
			if (!string.IsNullOrEmpty(text8))
			{
				text8 = text8.Substring(0, text8.Length - 2);
			}
			if (text8 != text7)
			{
				try
				{
					string directoryName = Path.GetDirectoryName(path2);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					File.WriteAllText(path2, text8, Encoding.UTF8);
				}
				catch
				{
				}
			}
			if (!string.IsNullOrEmpty(text5))
			{
				text5 = text5.Substring(0, text5.Length - 2);
			}
			if (text5 != text4)
			{
				try
				{
					string directoryName2 = Path.GetDirectoryName(path);
					if (!Directory.Exists(directoryName2))
					{
						Directory.CreateDirectory(directoryName2);
					}
					File.WriteAllText(path, text5, Encoding.UTF8);
				}
				catch
				{
				}
			}
		}
		SoftwareUpdateInfo softwareUpdateInfo = default(SoftwareUpdateInfo);
		softwareUpdateInfo.SoftwareUpdateResult = list;
		softwareUpdateInfo.SettingsUpdateResult = list2;
		softwareUpdateInfo.HelpContentUpdateResult = list3;
		SoftwareUpdateInfo info = softwareUpdateInfo;
		if (e.Error == null && version == mainSoftwareCurrentVersion)
		{
			string existingOnlineUrl;
			string existingOfflineFile;
			bool flag = !HasReleaseNotesForCurrentSoftwareVersion(out existingOnlineUrl, out existingOfflineFile);
			if (list.Count == 0 && (flag || existingOnlineUrl != text2 || existingOfflineFile != text3))
			{
				if (!string.IsNullOrEmpty(text2) && text2 != existingOnlineUrl)
				{
					try
					{
						WebRequest webRequest = WebRequest.Create(text2);
						webRequest.Method = "HEAD";
						using (webRequest.GetResponse())
						{
						}
					}
					catch
					{
						text2 = null;
					}
				}
				if (!string.IsNullOrEmpty(text3) && text3 != existingOfflineFile && !DownOfflineLoadReleaseNotes(text3))
				{
					text3 = null;
				}
				RegisterLatestReleaseNotesVersion(text2, text3);
				info.HasNewReleaseNotes = (flag && (!string.IsNullOrEmpty(text2) || !string.IsNullOrEmpty(text3))) || (!string.IsNullOrEmpty(text2) && existingOnlineUrl != text2) || (string.IsNullOrEmpty(text2) && !string.IsNullOrEmpty(text3) && existingOfflineFile != text3);
			}
		}
		checkUpdates2Callback.BeginInvoke(info, null, null);
	}

	public string GetReleaseNotesUrl(string updateUrl = "")
	{
		string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic\\" + appStorageFolderName + "\\Versions\\ReleaseNotesVersions.txt");
		ParseReleaseVersionInfoFile(filename, out var releaseVersion, out var onlineurl, out var offlineurl);
		bool flag = releaseVersion == mainSoftwareCurrentVersion;
		if (!flag && Task.Run(() => CheckUpdatesAsync2(updateUrl)).Result.HasNewReleaseNotes)
		{
			ParseReleaseVersionInfoFile(filename, out releaseVersion, out onlineurl, out offlineurl);
			flag = releaseVersion == mainSoftwareCurrentVersion;
		}
		if (flag)
		{
			if (!string.IsNullOrEmpty(onlineurl) && Helper.CheckForInternetConnection())
			{
				return onlineurl;
			}
			if (!string.IsNullOrEmpty(offlineurl))
			{
				string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Racelogic\\" + appStorageFolderName + "\\" + offlineurl);
				if (File.Exists(text))
				{
					return "file:///" + text.Replace("\\", "/");
				}
			}
		}
		return null;
	}

	private void ParseReleaseVersionInfoFile(string filename, out Version releaseVersion, out string onlineurl, out string offlineurl)
	{
		releaseVersion = null;
		onlineurl = null;
		offlineurl = null;
		if (File.Exists(filename))
		{
			string[] array = File.ReadAllText(filename).Split(new char[1] { ',' }, StringSplitOptions.None);
			if (!string.IsNullOrEmpty(array[0]) && Version.TryParse(array[0], out var result))
			{
				releaseVersion = result;
			}
			if (array.Length > 1)
			{
				onlineurl = array[1];
			}
			if (array.Length > 2)
			{
				offlineurl = array[2];
			}
		}
	}
}
