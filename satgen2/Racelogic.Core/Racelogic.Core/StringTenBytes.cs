using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 10)]
public struct StringTenBytes
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
	private byte[] data;

	public string Data
	{
		get
		{
			return StructureFunctions.ByteArrayToString(data);
		}
		set
		{
			data = StructureFunctions.StringToByteArray(value, 10u, nullTerminated: false);
		}
	}
}
