using System;

namespace Racelogic.Core.MindTouch;

public class MindTouchPdfDocumentDetail
{
	private const string DateModifiedTagName = "date.modified";

	private const string SecurityTagName = "security";

	private string pdfDocumentID;

	private DateTime datemodified;

	public string PdfDocumentID => pdfDocumentID;

	public DateTime DateModified => datemodified;

	public MindTouchPdfDocumentDetail(string xml)
	{
		int num = xml.IndexOf("id=\"");
		int num2;
		int num3;
		if (num != -1)
		{
			num2 = num + 4;
			num3 = xml.IndexOf("\"", num2);
			pdfDocumentID = xml.Substring(num2, num3 - num2);
		}
		int num4 = xml.IndexOf("</security>");
		num2 = ((num4 == -1) ? xml.IndexOf("<date.modified>") : xml.IndexOf("<date.modified>", num4)) + "<date.modified>".Length;
		num3 = xml.IndexOf("</date.modified>", num2);
		string value = xml.Substring(num2, num3 - num2);
		datemodified = Convert.ToDateTime(value);
	}
}
