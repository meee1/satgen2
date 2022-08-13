using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.Utilities;

public class USBDeviceEventArgs : EventArgs
{
	private readonly Dictionary<string, string> deviceProperties;

	public Dictionary<string, string> DeviceProperties
	{
		[DebuggerStepThrough]
		get
		{
			return deviceProperties;
		}
	}

	public USBDeviceEventArgs(Dictionary<string, string> deviceProperties)
	{
		this.deviceProperties = deviceProperties;
	}
}
