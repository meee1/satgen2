using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 11)]
public struct StringElevenBytes
{
	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
	private byte[] data;

	public string Data
	{
		get
		{
			return StructureFunctions.ByteArrayToString(data);
		}
		set
		{
			data = StructureFunctions.StringToByteArray(value, 11u);
		}
	}
}
