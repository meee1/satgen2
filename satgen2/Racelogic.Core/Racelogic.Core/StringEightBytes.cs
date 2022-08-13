using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
public struct StringEightBytes
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	private byte[] data;

	public string Data
	{
		get
		{
			return StructureFunctions.ByteArrayToString(data);
		}
		set
		{
			data = StructureFunctions.StringToByteArray(value, 8u);
		}
	}
}
