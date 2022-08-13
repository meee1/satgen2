using System.Runtime.InteropServices;
using System.Security;

namespace Racelogic.Utilities.Sntp;

internal static class NativeMethods
{
	[DllImport("kernel32.dll")]
	[SuppressUnmanagedCodeSecurity]
	public static extern bool SetLocalTime(ref SYSTEMTIME lpSystemTime);
}
