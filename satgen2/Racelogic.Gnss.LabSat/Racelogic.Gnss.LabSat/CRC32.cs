using System.Security.Cryptography;

namespace Racelogic.Gnss.LabSat;

public sealed class CRC32 : HashAlgorithm
{
	public const uint DefaultPolynomial = 3988292384u;

	public const uint DefaultSeed = uint.MaxValue;

	private readonly uint seed;

	private readonly uint[] table;

	private static uint[]? defaultTable;

	private uint hash;

	public override int HashSize => 32;

	public CRC32()
	{
		table = InitializeTable(3988292384u);
		seed = uint.MaxValue;
	}

	public CRC32(in uint polynomial, in uint seed)
	{
		this.seed = seed;
		table = InitializeTable(polynomial);
	}

	public static uint Compute(byte[] buffer)
	{
		uint[] array = InitializeTable(3988292384u);
		uint num = uint.MaxValue;
		int start = 0;
		int size = buffer.Length;
		return ~CalculateHash(array, in num, buffer, in start, in size);
	}

	public static byte[] ComputeBigEndian(byte[] buffer)
	{
		return UInt32ToBigEndianBytes(Compute(buffer));
	}

	public static uint Compute(in uint seed, byte[] buffer)
	{
		uint[] array = InitializeTable(3988292384u);
		int start = 0;
		int size = buffer.Length;
		return ~CalculateHash(array, in seed, buffer, in start, in size);
	}

	public static byte[] ComputeBigEndian(in uint seed, byte[] buffer)
	{
		return UInt32ToBigEndianBytes(Compute(in seed, buffer));
	}

	public static uint Compute(in uint polynomial, in uint seed, byte[] buffer)
	{
		uint[] array = InitializeTable(polynomial);
		int start = 0;
		int size = buffer.Length;
		return ~CalculateHash(array, in seed, buffer, in start, in size);
	}

	public static byte[] ComputeBigEndian(in uint polynomial, in uint seed, byte[] buffer)
	{
		return UInt32ToBigEndianBytes(Compute(in polynomial, in seed, buffer));
	}

	public sealed override void Initialize()
	{
		hash = seed;
	}

	protected override void HashCore(byte[] buffer, int start, int length)
	{
		hash = CalculateHash(table, in hash, buffer, in start, in length);
	}

	protected override byte[] HashFinal()
	{
		return HashValue = UInt32ToBigEndianBytes(~hash);
	}

	private static uint CalculateHash(uint[] table, in uint seed, byte[] buffer, in int start, in int size)
	{
		uint num = seed;
		for (int i = start; i < size; i++)
		{
			num = (num >> 8) ^ table[buffer[i] ^ (num & 0xFF)];
		}
		return num;
	}

	private static uint[] InitializeTable(uint polynomial)
	{
		if (polynomial == 3988292384u && defaultTable != null)
		{
			return defaultTable;
		}
		uint[] array = new uint[256];
		for (int i = 0; i < 256; i++)
		{
			uint num = (uint)i;
			for (int j = 0; j < 8; j++)
			{
				num = (((num & 1) != 1) ? (num >> 1) : ((num >> 1) ^ polynomial));
			}
			array[i] = num;
		}
		if (polynomial == 3988292384u)
		{
			defaultTable = array;
		}
		return array;
	}

	private static byte[] UInt32ToBigEndianBytes(uint x)
	{
		return new byte[4]
		{
			(byte)((x >> 24) & 0xFFu),
			(byte)((x >> 16) & 0xFFu),
			(byte)((x >> 8) & 0xFFu),
			(byte)(x & 0xFFu)
		};
	}
}
