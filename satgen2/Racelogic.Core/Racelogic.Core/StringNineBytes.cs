using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 9)]
public struct StringNineBytes
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
	private byte[] data;

	public string Data
	{
		get
		{
			return StructureFunctions.ByteArrayToString(data);
		}
		set
		{
			data = StructureFunctions.StringToByteArray(value, 9u, nullTerminated: false);
		}
	}
}
