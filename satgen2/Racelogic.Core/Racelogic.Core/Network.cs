using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Racelogic.Core;

public class Network
{
	public static bool InternetAvailable()
	{
		if (!NetworkInterface.GetIsNetworkAvailable())
		{
			return false;
		}
		try
		{
			bool flag = string.Equals(new WebClient().DownloadString("http://www.msftncsi.com/ncsi.txt"), "Microsoft NCSI", StringComparison.InvariantCultureIgnoreCase);
		}
		catch (Exception)
		{
			return false;
		}
		try
		{
			IPHostEntry hostEntry = Dns.GetHostEntry("dns.msftncsi.com");
			return hostEntry.AddressList.Any() && string.Equals(hostEntry.AddressList.First().ToString(), "131.107.255.255", StringComparison.InvariantCultureIgnoreCase);
		}
		catch (Exception)
		{
			return false;
		}
	}
}
