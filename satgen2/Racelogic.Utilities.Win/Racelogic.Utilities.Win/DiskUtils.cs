using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;

namespace Racelogic.Utilities.Win;

public static class DiskUtils
{
	private static ManagementEventWatcher usbDriveInsertedEventWatcher;

	private static ManagementEventWatcher usbDriveRemovedEventWatcher;

	private static readonly object usbDriveWatcherLock = new object();

	private static bool WatchForRemovableMedia = false;

	private static bool NotificationsRegistered = false;

	private static Action OnMediaInserted;

	private static Action OnMediaRemoved;

	private static Action OnAbout;

	private static readonly ShellNotifications Notifications = new ShellNotifications();

	private const int MF_STRING = 0;

	private const int SYSMENU_ABOUT_ID = 1;

	private const int WM_SYSCOMMAND = 274;

	private const int WM_SHNOTIFY = 1025;

	private const int MF_SEPARATOR = 2048;

	public static event EventHandler<DriveEventArgs> USBDriveInserted;

	public static event EventHandler USBDriveRemoved;

	public static void AddAboutItemToSystemMenu(Window window, string text, Action onAbout, bool addSeparatorBeforeItem)
	{
		IntPtr systemMenu = GetSystemMenu(new WindowInteropHelper(window).Handle, bRevert: false);
		if (addSeparatorBeforeItem)
		{
			AppendMenu(systemMenu, 2048, 0, string.Empty);
		}
		AppendMenu(systemMenu, 0, 1, text);
		OnAbout = onAbout;
		((HwndSource)PresentationSource.FromVisual(window)).AddHook(WndProc_About);
		RegisterForNotifications(window);
	}

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

	[DllImport("user32.dll", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

	private static IntPtr WndProc_About(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == 274 && (int)wParam == 1)
		{
			OnAbout?.Invoke();
		}
		return IntPtr.Zero;
	}

	public static void RemovableMediaRegisterForChangeNotificationsAndStartMonitoring(Window window, Action onMediaInserted, Action onMediaRemoved = null)
	{
		((HwndSource)PresentationSource.FromVisual(window)).AddHook(WndProc_WatchForRemovableMedia);
		RegisterForNotifications(window);
		WatchForRemovableMedia = true;
		OnMediaInserted = onMediaInserted;
		OnMediaRemoved = onMediaRemoved ?? onMediaInserted;
	}

	public static void RemovableMediaStartMonitoring(Action onMediaInserted, Action onMediaRemoved = null)
	{
		if (NotificationsRegistered)
		{
			WatchForRemovableMedia = true;
			OnMediaInserted = onMediaInserted;
			OnMediaRemoved = onMediaRemoved ?? onMediaInserted;
		}
	}

	public static void RemovableMediaStopMonitoring()
	{
		WatchForRemovableMedia = false;
		OnMediaInserted = null;
		OnMediaRemoved = null;
	}

	private static void RegisterForNotifications(Window window)
	{
		if (!NotificationsRegistered)
		{
			Notifications.RegisterChangeNotify(new WindowInteropHelper(window).Handle, ShellNotifications.CSIDL.CSIDL_DESKTOP, Recursively: true);
			NotificationsRegistered = true;
		}
	}

	private static IntPtr WndProc_WatchForRemovableMedia(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == 1025)
		{
			switch ((int)lParam)
			{
			case 32:
			case 256:
				if (WatchForRemovableMedia && Notifications.NotificationReceipt(wParam, lParam))
				{
					OnMediaInserted?.Invoke();
				}
				break;
			case 64:
			case 128:
				if (WatchForRemovableMedia && Notifications.NotificationReceipt(wParam, lParam))
				{
					OnMediaRemoved?.Invoke();
				}
				break;
			}
		}
		return IntPtr.Zero;
	}

	public static void StartWatchingForUSBDrives(double pollingInterval = 3.0)
	{
		lock (usbDriveWatcherLock)
		{
			StopWatchingForUSBDrives();
			AddUSBDriveInsertedHandler(pollingInterval);
			AddUSBDriveRemovedHandler(pollingInterval);
		}
	}

	public static void StopWatchingForUSBDrives()
	{
		lock (usbDriveWatcherLock)
		{
			usbDriveInsertedEventWatcher.Stop();
			usbDriveInsertedEventWatcher.Dispose();
			usbDriveInsertedEventWatcher = null;
			usbDriveRemovedEventWatcher.Stop();
			usbDriveRemovedEventWatcher.Dispose();
			usbDriveRemovedEventWatcher = null;
		}
	}

	private static void AddUSBDriveInsertedHandler(double pollingInterval)
	{
		ManagementScope managementScope = new ManagementScope("root\\CIMV2");
		managementScope.Options.EnablePrivileges = true;
		try
		{
			WqlEventQuery query = new WqlEventQuery
			{
				EventClassName = "__InstanceCreationEvent",
				WithinInterval = TimeSpan.FromSeconds(pollingInterval),
				Condition = "TargetInstance ISA 'Win32_DiskDrive'"
			};
			usbDriveRemovedEventWatcher = new ManagementEventWatcher(managementScope, query);
			usbDriveRemovedEventWatcher.EventArrived += OnUSBDriveInserted;
			usbDriveRemovedEventWatcher.Start();
		}
		catch
		{
			if (usbDriveInsertedEventWatcher != null)
			{
				usbDriveInsertedEventWatcher.Stop();
				usbDriveInsertedEventWatcher = null;
			}
		}
	}

	private static void AddUSBDriveRemovedHandler(double pollingInterval)
	{
		ManagementScope managementScope = new ManagementScope("root\\CIMV2");
		managementScope.Options.EnablePrivileges = true;
		try
		{
			WqlEventQuery query = new WqlEventQuery
			{
				EventClassName = "__InstanceDeletionEvent",
				WithinInterval = TimeSpan.FromSeconds(pollingInterval),
				Condition = "TargetInstance ISA 'Win32_DiskDrive'"
			};
			usbDriveInsertedEventWatcher = new ManagementEventWatcher(managementScope, query);
			usbDriveInsertedEventWatcher.EventArrived += OnUSBDriveRemoved;
			usbDriveInsertedEventWatcher.Start();
		}
		catch
		{
			if (usbDriveRemovedEventWatcher != null)
			{
				usbDriveRemovedEventWatcher.Stop();
				usbDriveRemovedEventWatcher = null;
			}
		}
	}

	private static void OnUSBDriveInserted(object sender, EventArrivedEventArgs e)
	{
		List<DriveInfo> list = new List<DriveInfo>();
		foreach (PropertyData property in e.NewEvent.Properties)
		{
			if (!(property.Value is ManagementBaseObject managementBaseObject))
			{
				continue;
			}
			foreach (ManagementObject item in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + managementBaseObject.Properties["DeviceID"].Value?.ToString() + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
			{
				using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator2 = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + item["DeviceID"]?.ToString() + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get().GetEnumerator();
				if (managementObjectEnumerator2.MoveNext())
				{
					string driveName = ((ManagementObject)managementObjectEnumerator2.Current)["Name"].ToString();
					try
					{
						list.Add(new DriveInfo(driveName));
						return;
					}
					catch (ArgumentNullException)
					{
						return;
					}
					catch (ArgumentException)
					{
						return;
					}
				}
			}
		}
		DiskUtils.USBDriveInserted?.Invoke(sender, new DriveEventArgs(list));
	}

	private static void OnUSBDriveRemoved(object sender, EventArrivedEventArgs e)
	{
		DiskUtils.USBDriveRemoved?.Invoke(sender, EventArgs.Empty);
	}
}
