using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Racelogic.Comms.Serial.WMI;

internal class Win32_SerialPort : IWMI
{
	private Connection WMIConnection;

	public Win32_SerialPort(Connection wmiConnection)
	{
		WMIConnection = wmiConnection;
	}

	public IList<string> GetPropertyValues()
	{
		string value = Regex.Match(GetType().ToString(), "Win32_.*").Value;
		return WMIReader.GetPropertyValues(WMIConnection, "SELECT * FROM " + value, value);
	}
}
