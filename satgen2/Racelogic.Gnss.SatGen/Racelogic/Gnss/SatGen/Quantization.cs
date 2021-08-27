namespace Racelogic.Gnss.SatGen
{
	public enum Quantization
	{
		None = 0,
		OneBit = 1,
		TwoBit = 2,
		ThreeBit = 3,
		EightBit = 8,
		TwelveBit = 12,
		SixteenBit = 0x10,
		Float = 0x20,
		Double = 0x40,
		Max = int.MaxValue
	}
}
