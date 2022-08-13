using System.Management;

namespace Racelogic.Utilities.Win;

public static class SystemUtils
{
	public static int GetPhysicalCoreCount()
	{
		using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
		int num = 0;
		foreach (ManagementObject item in managementObjectSearcher.Get())
		{
			num += int.Parse(item["NumberOfCores"].ToString());
		}
		return num;
	}

	public static int GetPhysicalProcessorCount()
	{
		using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
		int result = 0;
		foreach (ManagementObject item in managementObjectSearcher.Get())
		{
			result = int.Parse(item["NumberOfProcessors"].ToString());
		}
		return result;
	}
}
