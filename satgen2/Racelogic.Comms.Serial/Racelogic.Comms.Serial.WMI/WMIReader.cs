using System;
using System.Collections.Generic;
using System.Management;

namespace Racelogic.Comms.Serial.WMI;

internal class WMIReader
{
	public static IList<string> GetPropertyValues(Connection WMIConnection, string SelectQuery, string className)
	{
		List<string> list = new List<string>(4);
		list.Add("DeviceID");
		list.Add("Caption");
		list.Add("Name");
		list.Add("PNPDeviceID");
		ManagementScope getConnectionScope = WMIConnection.GetConnectionScope;
		List<string> list2 = new List<string>();
		SelectQuery query = new SelectQuery(SelectQuery);
		ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(getConnectionScope, query);
		try
		{
			list2.Add(string.Empty);
			foreach (ManagementObject item in managementObjectSearcher.Get())
			{
				foreach (string item2 in list)
				{
					try
					{
						list2.Add(item2 + ": " + item[item2].ToString());
					}
					catch (SystemException)
					{
					}
				}
				list2.Add(string.Empty);
			}
		}
		catch (ManagementException)
		{
		}
		return list2;
	}
}
