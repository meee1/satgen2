using System.Runtime.InteropServices;

namespace Racelogic.DataSource;

[StructLayout(LayoutKind.Explicit)]
public struct doubleUnion
{
	[FieldOffset(0)]
	public double data;

	[FieldOffset(0)]
	public long temp;

	[FieldOffset(0)]
	public ulong utemp;

	[FieldOffset(0)]
	public int i0_LSB;

	[FieldOffset(4)]
	public int i1_MSB;

	[FieldOffset(0)]
	public byte b0_LSB;

	[FieldOffset(1)]
	public byte b1;

	[FieldOffset(2)]
	public byte b2;

	[FieldOffset(3)]
	public byte b3;

	[FieldOffset(4)]
	public byte b4;

	[FieldOffset(5)]
	public byte b5;

	[FieldOffset(6)]
	public byte b6;

	[FieldOffset(7)]
	public byte b7_MSB;
}
