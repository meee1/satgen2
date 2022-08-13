using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Racelogic.Utilities;

public class USBDeviceWatcher : IDisposable
{
	private readonly ManagementEventWatcher insertWatcher;

	private readonly ManagementEventWatcher removeWatcher;

	private bool disposedValue;

	public event EventHandler<USBDeviceEventArgs> DeviceInserted;

	public event EventHandler<USBDeviceEventArgs> DeviceRemoved;

	public USBDeviceWatcher()
	{
		WqlEventQuery query = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
		insertWatcher = new ManagementEventWatcher(query);
		insertWatcher.EventArrived += OnDeviceInserted;
		insertWatcher.Start();
		WqlEventQuery query2 = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
		removeWatcher = new ManagementEventWatcher(query2);
		removeWatcher.EventArrived += OnDeviceRemoved;
		removeWatcher.Start();
	}

	protected void OnDeviceInserted(object sender, EventArrivedEventArgs e)
	{
		Dictionary<string, string> deviceProperties = ((ManagementBaseObject)e.NewEvent["TargetInstance"]).Properties.Cast<PropertyData>().ToDictionary((PropertyData pd) => pd.Name, (PropertyData pd) => pd.Value?.ToString() ?? string.Empty);
		this.DeviceInserted(this, new USBDeviceEventArgs(deviceProperties));
	}

	protected void OnDeviceRemoved(object sender, EventArrivedEventArgs e)
	{
		Dictionary<string, string> deviceProperties = ((ManagementBaseObject)e.NewEvent["TargetInstance"]).Properties.Cast<PropertyData>().ToDictionary((PropertyData pd) => pd.Name, (PropertyData pd) => pd.Value?.ToString() ?? string.Empty);
		this.DeviceRemoved(this, new USBDeviceEventArgs(deviceProperties));
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			disposedValue = true;
			if (disposing)
			{
				insertWatcher.Stop();
				insertWatcher.EventArrived -= OnDeviceInserted;
				insertWatcher.Dispose();
				removeWatcher.Stop();
				removeWatcher.EventArrived -= OnDeviceRemoved;
				removeWatcher.Dispose();
			}
		}
	}
}
