using Racelogic.Core.MindTouch;

namespace Racelogic.Core.Updater;

public class OfflineHelpContentInstallationEntity
{
	private string pluginId;

	private string contentId;

	private string pdfHelpPageId;

	private bool checkedForDownload;

	public string PluginID => pluginId;

	public string ContentID => contentId;

	public string PdfHelpPageID => pdfHelpPageId;

	public bool CheckedForDownload => checkedForDownload;

	public OfflineHelpContentInstallationEntity(string pluginID, string contentId, string pdfHelpPageId)
	{
		pluginId = pluginID;
		this.contentId = contentId;
		this.pdfHelpPageId = pdfHelpPageId;
	}

	public void DownloadFileAsynch(MindTouchHelper.DownloadFileCallback DownloadedCallBack)
	{
		checkedForDownload = true;
		MindTouchHelper.DownloadDocumentIfRequiredAsync(PdfHelpPageID, ContentID, DownloadedCallBack);
	}
}
