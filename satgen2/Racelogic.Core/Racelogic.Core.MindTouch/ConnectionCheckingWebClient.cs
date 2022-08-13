using System;
using System.Net;

namespace Racelogic.Core.MindTouch;

public class ConnectionCheckingWebClient : WebClient
{
	protected override WebRequest GetWebRequest(Uri address)
	{
		WebRequest webRequest = base.GetWebRequest(address);
		webRequest.Timeout = 5000;
		return webRequest;
	}
}
