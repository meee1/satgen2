using System;
using System.Runtime.InteropServices;

namespace Racelogic.Utilities.Win;

internal class ShellNotifications
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct SHChangeNotifyEntry
	{
		public IntPtr pIdl;

		[MarshalAs(UnmanagedType.Bool)]
		public bool Recursively;
	}

	private struct SHFILEINFO
	{
		public IntPtr hIcon;

		public int iIcon;

		public uint dwAttributes;

		[MarshalAs(UnmanagedType.LPStr)]
		public string szDisplayName;

		[MarshalAs(UnmanagedType.LPStr)]
		public string szTypeName;

		public SHFILEINFO(bool b)
		{
			hIcon = IntPtr.Zero;
			iIcon = 0;
			dwAttributes = 0u;
			szDisplayName = "";
			szTypeName = "";
		}
	}

	public struct SHNOTIFYSTRUCT
	{
		public IntPtr dwItem1;

		public IntPtr dwItem2;
	}

	public enum CSIDL
	{
		CSIDL_DESKTOP = 0,
		CSIDL_INTERNET = 1,
		CSIDL_PROGRAMS = 2,
		CSIDL_CONTROLS = 3,
		CSIDL_PRINTERS = 4,
		CSIDL_PERSONAL = 5,
		CSIDL_FAVORITES = 6,
		CSIDL_STARTUP = 7,
		CSIDL_RECENT = 8,
		CSIDL_SENDTO = 9,
		CSIDL_BITBUCKET = 10,
		CSIDL_STARTMENU = 11,
		CSIDL_MYDOCUMENTS = 12,
		CSIDL_MYMUSIC = 13,
		CSIDL_MYVIDEO = 14,
		CSIDL_DESKTOPDIRECTORY = 16,
		CSIDL_DRIVES = 17,
		CSIDL_NETWORK = 18,
		CSIDL_NETHOOD = 19,
		CSIDL_FONTS = 20,
		CSIDL_TEMPLATES = 21,
		CSIDL_COMMON_STARTMENU = 22,
		CSIDL_COMMON_PROGRAMS = 23,
		CSIDL_COMMON_STARTUP = 24,
		CSIDL_COMMON_DESKTOPDIRECTORY = 25,
		CSIDL_APPDATA = 26,
		CSIDL_PRINTHOOD = 27,
		CSIDL_LOCAL_APPDATA = 28,
		CSIDL_ALTSTARTUP = 29,
		CSIDL_COMMON_ALTSTARTUP = 30,
		CSIDL_COMMON_FAVORITES = 31,
		CSIDL_INTERNET_CACHE = 32,
		CSIDL_COOKIES = 33,
		CSIDL_HISTORY = 34,
		CSIDL_COMMON_APPDATA = 35,
		CSIDL_WINDOWS = 36,
		CSIDL_SYSTEM = 37,
		CSIDL_PROGRAM_FILES = 38,
		CSIDL_MYPICTURES = 39,
		CSIDL_PROFILE = 40,
		CSIDL_SYSTEMX86 = 41,
		CSIDL_PROGRAM_FILESX86 = 42,
		CSIDL_PROGRAM_FILES_COMMON = 43,
		CSIDL_PROGRAM_FILES_COMMONX86 = 44,
		CSIDL_COMMON_TEMPLATES = 45,
		CSIDL_COMMON_DOCUMENTS = 46,
		CSIDL_COMMON_ADMINTOOLS = 47,
		CSIDL_ADMINTOOLS = 48,
		CSIDL_CONNECTIONS = 49,
		CSIDL_COMMON_MUSIC = 53,
		CSIDL_COMMON_PICTURES = 54,
		CSIDL_COMMON_VIDEO = 55,
		CSIDL_RESOURCES = 56,
		CSIDL_RESOURCES_LOCALIZED = 57,
		CSIDL_COMMON_OEM_LINKS = 58,
		CSIDL_CDBURN_AREA = 59,
		CSIDL_COMPUTERSNEARME = 61,
		CSIDL_FLAG_CREATE = 32768,
		CSIDL_FLAG_DONT_VERIFY = 16384,
		CSIDL_FLAG_NO_ALIAS = 4096,
		CSIDL_FLAG_PER_USER_INIT = 2048,
		CSIDL_FLAG_MASK = 65280
	}

	public enum SHCNF
	{
		SHCNF_IDLIST = 0,
		SHCNF_PATHA = 1,
		SHCNF_PRINTERA = 2,
		SHCNF_DWORD = 3,
		SHCNF_PATHW = 5,
		SHCNF_PRINTERW = 6,
		SHCNF_TYPE = 255,
		SHCNF_FLUSH = 4096,
		SHCNF_FLUSHNOWAIT = 8192
	}

	public enum SHCNE : uint
	{
		SHCNE_RENAMEITEM = 1u,
		SHCNE_CREATE = 2u,
		SHCNE_DELETE = 4u,
		SHCNE_MKDIR = 8u,
		SHCNE_RMDIR = 16u,
		SHCNE_MEDIAINSERTED = 32u,
		SHCNE_MEDIAREMOVED = 64u,
		SHCNE_DRIVEREMOVED = 128u,
		SHCNE_DRIVEADD = 256u,
		SHCNE_NETSHARE = 512u,
		SHCNE_NETUNSHARE = 1024u,
		SHCNE_ATTRIBUTES = 2048u,
		SHCNE_UPDATEDIR = 4096u,
		SHCNE_UPDATEITEM = 8192u,
		SHCNE_SERVERDISCONNECT = 16384u,
		SHCNE_UPDATEIMAGE = 32768u,
		SHCNE_DRIVEADDGUI = 65536u,
		SHCNE_RENAMEFOLDER = 131072u,
		SHCNE_FREESPACE = 262144u,
		SHCNE_EXTENDED_EVENT = 67108864u,
		SHCNE_ASSOCCHANGED = 134217728u,
		SHCNE_DISKEVENTS = 145439u,
		SHCNE_GLOBALEVENTS = 201687520u,
		SHCNE_ALLEVENTS = 2147483647u,
		SHCNE_INTERRUPT = 2147483648u
	}

	public enum SHGFI : uint
	{
		SHGFI_ICON = 0x100u,
		SHGFI_DISPLAYNAME = 0x200u,
		SHGFI_TYPENAME = 0x400u,
		SHGFI_ATTRIBUTES = 0x800u,
		SHGFI_ICONLOCATION = 0x1000u,
		SHGFI_EXETYPE = 0x2000u,
		SHGFI_SYSICONINDEX = 0x4000u,
		SHGFI_LINKOVERLAY = 0x8000u,
		SHGFI_SELECTED = 0x10000u,
		SHGFI_ATTR_SPECIFIED = 0x20000u,
		SHGFI_LARGEICON = 0u,
		SHGFI_SMALLICON = 1u,
		SHGFI_OPENICON = 2u,
		SHGFI_SHELLICONSIZE = 4u,
		SHGFI_PIDL = 8u,
		SHGFI_USEFILEATTRIBUTES = 0x10u,
		SHGFI_ADDOVERLAYS = 0x20u,
		SHGFI_OVERLAYINDEX = 0x40u
	}

	public enum SHGetFolderLocationReturnValues : uint
	{
		S_OK = 0u,
		S_FALSE = 1u,
		E_INVALIDARG = 2147942487u
	}

	private ulong notifyid;

	public const uint WM_SHNOTIFY = 1025u;

	public const int MAX_PATH = 260;

	public NotifyInfos LastNotificationReceived { get; private set; }

	public ulong NotifyID => notifyid;

	[DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "#2")]
	private static extern uint SHChangeNotifyRegister(IntPtr hWnd, SHCNF fSources, SHCNE fEvents, uint wMsg, int cEntries, ref SHChangeNotifyEntry pFsne);

	[DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = "#4")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SHChangeNotifyUnregister(ulong hNotify);

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	private static extern int SHGetFileInfoA(uint Pidl, uint FileAttributes, out SHFILEINFO Fi, uint FileInfo, SHGFI Flags);

	[DllImport("Shell32.Dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In][Out][MarshalAs(UnmanagedType.LPTStr)] string pszPath);

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	private static extern uint SHGetSpecialFolderLocation(IntPtr hWnd, CSIDL nFolder, out IntPtr Pidl);

	public ulong RegisterChangeNotify(IntPtr hWnd, CSIDL FolderID, bool Recursively)
	{
		if (notifyid != 0L)
		{
			return 0uL;
		}
		SHChangeNotifyEntry pFsne = default(SHChangeNotifyEntry);
		pFsne.pIdl = GetPidlFromFolderID(hWnd, FolderID);
		pFsne.Recursively = Recursively;
		notifyid = SHChangeNotifyRegister(hWnd, SHCNF.SHCNF_TYPE, (SHCNE)4294967295u, 1025u, 1, ref pFsne);
		return notifyid;
	}

	public bool UnregisterChangeNotify()
	{
		if (notifyid == 0L)
		{
			return false;
		}
		if (SHChangeNotifyUnregister(notifyid))
		{
			notifyid = 0uL;
			return true;
		}
		return false;
	}

	public static string GetPathFromPidl(IntPtr Pidl)
	{
		string text = new string('\0', 260);
		if (!SHGetPathFromIDList(Pidl, text))
		{
			return "";
		}
		return text.ToString().TrimEnd(default(char));
	}

	public static string GetDisplayNameFromPidl(IntPtr Pidl)
	{
		SHFILEINFO Fi = new SHFILEINFO(b: true);
		SHGetFileInfoA((uint)(int)Pidl, 0u, out Fi, (uint)Marshal.SizeOf(Fi), (SHGFI)520u);
		return Fi.szDisplayName;
	}

	public static IntPtr GetPidlFromFolderID(IntPtr hWnd, CSIDL Id)
	{
		IntPtr Pidl = IntPtr.Zero;
		SHGetSpecialFolderLocation(hWnd, Id, out Pidl);
		return Pidl;
	}

	public bool NotificationReceipt(IntPtr wParam, IntPtr lParam)
	{
		SHNOTIFYSTRUCT sHNOTIFYSTRUCT = (SHNOTIFYSTRUCT)Marshal.PtrToStructure(wParam, typeof(SHNOTIFYSTRUCT));
		NotifyInfos lastNotificationReceived = new NotifyInfos((SHCNE)(int)lParam);
		if (lastNotificationReceived.Notification == SHCNE.SHCNE_FREESPACE || lastNotificationReceived.Notification == SHCNE.SHCNE_UPDATEIMAGE)
		{
			return false;
		}
		lastNotificationReceived.Item1 = GetPathFromPidl(sHNOTIFYSTRUCT.dwItem1);
		lastNotificationReceived.Item2 = GetPathFromPidl(sHNOTIFYSTRUCT.dwItem2);
		LastNotificationReceived = lastNotificationReceived;
		return true;
	}
}
