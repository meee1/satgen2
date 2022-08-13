using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Racelogic.Core.MindTouch;

public class MindTouchHelper
{
	private delegate void GetHelpFileDetailCallback(MindTouchPdfDocumentDetail helpFileDetail, object token);

	public delegate void DownloadFileCallback();

	private const string DefaultMindTouchUrl = "https://racelogic.support?cid={0}";

	private const string DefaultFileDetailsCheckingUrl = "https://racelogic.support/@api/deki/pages/{0}";

	private const string DefaultDownloadFileUrl = "https://racelogic.support/@api/deki/pages/{0}/pdf";

	private static string offlinePdfFileStoragePath;

	private static string mindTouchUrl;

	private static string fileDetailsCheckingUrl;

	private static string downloadFileUrl;

	private static RacelogicCommand mindTouchLaunchCommand;

	public static string MindTouchLinkUrl
	{
		get
		{
			if (string.IsNullOrEmpty(mindTouchUrl))
			{
				return "https://racelogic.support?cid={0}";
			}
			return mindTouchUrl;
		}
		set
		{
			mindTouchUrl = value;
		}
	}

	public static string MindTouchPageUrl
	{
		get
		{
			if (string.IsNullOrEmpty(fileDetailsCheckingUrl))
			{
				return "https://racelogic.support/@api/deki/pages/{0}";
			}
			return fileDetailsCheckingUrl;
		}
		set
		{
			fileDetailsCheckingUrl = value;
		}
	}

	public static string MindTouchDocumentDownloadUrl
	{
		get
		{
			if (string.IsNullOrEmpty(downloadFileUrl))
			{
				return "https://racelogic.support/@api/deki/pages/{0}/pdf";
			}
			return downloadFileUrl;
		}
		set
		{
			downloadFileUrl = value;
		}
	}

	public static RacelogicCommand MindTouchLaunchCommand
	{
		get
		{
			if (mindTouchLaunchCommand == null)
			{
				mindTouchLaunchCommand = new RacelogicCommand(OnMindTouchLaunch, CanMindTouchLaunch, null);
			}
			return mindTouchLaunchCommand;
		}
	}

	static MindTouchHelper()
	{
		offlinePdfFileStoragePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Racelogic\\OfflineDocumentCache";
		if (!Directory.Exists(offlinePdfFileStoragePath))
		{
			Directory.CreateDirectory(offlinePdfFileStoragePath);
		}
	}

	private static bool CanMindTouchLaunch(object parameter)
	{
		return true;
	}

	private static void OnMindTouchLaunch(object parameter)
	{
		if (string.IsNullOrEmpty((string)parameter))
		{
			return;
		}
		string[] array = ((string)parameter).Split(new char[1] { ',' });
		if (HasInternetConnection())
		{
			Process.Start(string.Format(MindTouchLinkUrl, array[0]));
			if (array.Length > 1)
			{
				DownloadDocumentIfRequiredAsync(array[1], array[0], null);
			}
		}
		else
		{
			string text = offlinePdfFileStoragePath + "\\" + array[0] + ".pdf";
			if (File.Exists(text))
			{
				Process.Start(text);
			}
		}
	}

	private static void GetFileDetailAsynch(string PdfDocumentID, GetHelpFileDetailCallback callback, object token)
	{
		string uriString = string.Format(MindTouchPageUrl, PdfDocumentID);
		WebClient webClient = new WebClient();
		webClient.DownloadStringCompleted += client_DownloadStringCompleted;
		webClient.DownloadStringAsync(new Uri(uriString, UriKind.Absolute), new List<object> { callback, token });
	}

	private static void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
	{
		GetHelpFileDetailCallback obj = (GetHelpFileDetailCallback)((List<object>)e.UserState)[0];
		object token = ((List<object>)e.UserState)[1];
		MindTouchPdfDocumentDetail helpFileDetail = null;
		if (e.Error == null)
		{
			helpFileDetail = new MindTouchPdfDocumentDetail(e.Result);
		}
		obj?.Invoke(helpFileDetail, token);
	}

	public static void DownloadDocumentIfRequiredAsync(string PdfDocumentID, string ContentID, DownloadFileCallback callback)
	{
		if (!HasInternetConnection())
		{
			callback?.Invoke();
			return;
		}
		if (File.Exists(offlinePdfFileStoragePath + "\\" + ContentID + ".pdf"))
		{
			GetFileDetailAsynch(PdfDocumentID, DownloadFileCheckLatestCompleted, new List<object> { callback, ContentID });
			return;
		}
		string uriString = string.Format(MindTouchDocumentDownloadUrl, PdfDocumentID);
		WebClient webClient = new WebClient();
		webClient.DownloadDataCompleted += client_DownloadDataCompleted;
		webClient.DownloadDataAsync(new Uri(uriString, UriKind.Absolute), new List<object> { callback, ContentID });
	}

	private static void DownloadFileCheckLatestCompleted(MindTouchPdfDocumentDetail helpFileDetail, object token)
	{
		DownloadFileCallback downloadFileCallback = (DownloadFileCallback)((List<object>)token)[0];
		string text = (string)((List<object>)token)[1];
		DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(offlinePdfFileStoragePath + "\\" + text + ".pdf");
		if (helpFileDetail != null && lastWriteTimeUtc < helpFileDetail.DateModified)
		{
			string uriString = string.Format(MindTouchDocumentDownloadUrl, helpFileDetail.PdfDocumentID);
			WebClient webClient = new WebClient();
			webClient.DownloadDataCompleted += client_DownloadDataCompleted;
			webClient.DownloadDataAsync(new Uri(uriString, UriKind.Absolute), new List<object> { downloadFileCallback, text });
		}
		else
		{
			downloadFileCallback?.Invoke();
		}
	}

	private static void client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
	{
		DownloadFileCallback downloadFileCallback = (DownloadFileCallback)((List<object>)e.UserState)[0];
		string text = (string)((List<object>)e.UserState)[1];
		try
		{
			if (!string.IsNullOrEmpty(text))
			{
				File.WriteAllBytes(offlinePdfFileStoragePath + "\\" + text + ".pdf", e.Result);
			}
		}
		catch
		{
		}
		downloadFileCallback?.Invoke();
	}

	private static bool HasInternetConnection()
	{
		try
		{
			using ConnectionCheckingWebClient connectionCheckingWebClient = new ConnectionCheckingWebClient();
			using (connectionCheckingWebClient.OpenRead("http://www.google.com"))
			{
				return true;
			}
		}
		catch
		{
			return false;
		}
	}
}
