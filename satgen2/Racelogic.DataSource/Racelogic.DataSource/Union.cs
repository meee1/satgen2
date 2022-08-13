using System.Runtime.InteropServices;

namespace Racelogic.DataSource;

[StructLayout(LayoutKind.Explicit)]
public struct Union
{
	[FieldOffset(0)]
	public float data;

	[FieldOffset(0)]
	public int temp;

	[FieldOffset(0)]
	public uint utemp;

	[FieldOffset(0)]
	public byte b0_LSB;

	[FieldOffset(1)]
	public byte b1;

	[FieldOffset(2)]
	public byte b2;

	[FieldOffset(3)]
	public byte b3_MSB;
}
