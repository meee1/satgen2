using System.Runtime.InteropServices;

namespace Racelogic.Core;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 26)]
public struct CanModuleConfigurationDefinition
{
	public StringTenBytes Name;

	public StringNineBytes Units;

	public int SerialNumber;

	public byte ChannelNumber;

	public byte Status;

	public byte DataType;
}
