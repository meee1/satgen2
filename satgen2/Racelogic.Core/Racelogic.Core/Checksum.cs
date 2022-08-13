using System;
using System.Collections.Generic;

namespace Racelogic.Core;

public static class Checksum
{
	public static uint Calculate(Queue<byte> data, uint length, PolynomialUnitType unitType)
	{
		int crc = 0;
		Queue<byte> queue = new Queue<byte>(data);
		for (int i = 0; i < length; i++)
		{
			Calculate(ref crc, queue.Dequeue(), unitType);
		}
		return (uint)crc;
	}

	public static uint Calculate(List<byte> data, uint length, PolynomialUnitType unitType)
	{
		int crc = 0;
		for (int i = 0; i < length; i++)
		{
			Calculate(ref crc, data[i], unitType);
		}
		return (uint)crc;
	}

	public static uint Calculate(List<byte> data, uint length, uint polynomial)
	{
		int crc = 0;
		for (int i = 0; i < length; i++)
		{
			Calculate(ref crc, data[i], polynomial);
		}
		return (uint)crc;
	}

	public static uint Calculate(byte[] data, uint length, uint polynomial)
	{
		int crc = 0;
		for (int i = 0; i < length; i++)
		{
			Calculate(ref crc, data[i], polynomial);
		}
		return (uint)crc;
	}

	public static uint Calculate(string data, uint length, PolynomialUnitType unitType)
	{
		int crc = 0;
		for (int i = 0; i < length; i++)
		{
			Calculate(ref crc, data[i], unitType);
		}
		return (uint)crc;
	}

	public static bool Check(Queue<byte> data, uint length, PolynomialUnitType unitType)
	{
		Queue<byte> queue = new Queue<byte>(data);
		uint num = Calculate(queue, length - 2, unitType);
		while (queue.Count > 2)
		{
			queue.Dequeue();
		}
		uint num2 = queue.Dequeue();
		num2 <<= 8;
		num2 += queue.Dequeue();
		bool num3 = num == num2;
		if (!num3)
		{
			throw new RacelogicCheckSumException("Checksum.Calculate(): Incorrect checksum");
		}
		return num3;
	}

	public static bool Check(List<byte> data, uint length, PolynomialUnitType unitType)
	{
		uint num = Calculate(data, length - 2, unitType);
		uint num2 = data[data.Count - 2];
		num2 <<= 8;
		num2 += data[data.Count - 1];
		bool num3 = num == num2;
		if (!num3)
		{
			throw new RacelogicCheckSumException("Checksum.Calculate(): Incorrect checksum");
		}
		return num3;
	}

	public static void Append(Queue<byte> data)
	{
		uint num = Calculate(data, (uint)data.Count, PolynomialUnitType.VBox);
		data.Enqueue((byte)(num >> 8));
		data.Enqueue((byte)num);
	}

	private static void Calculate(ref int crc, int temp, PolynomialUnitType unitType)
	{
		Calculate(ref crc, temp, unitType switch
		{
			PolynomialUnitType.VBox => 4129u, 
			PolynomialUnitType.DriftBox => 4132u, 
			PolynomialUnitType.VideoVBoxAndMfd => 17884u, 
			PolynomialUnitType.LabSat3 => 3988292384u, 
			_ => throw new ArgumentException($"Invalid PolynomialUnitType in Racelogic.Comms.Serial.Checksum.Calculate - {unitType}"), 
		});
	}

	private static void Calculate(ref int crc, int temp, uint polynomial)
	{
		crc ^= temp * 256;
		crc %= 65536;
		for (int num = 8; num > 0; num--)
		{
			if ((crc & 0x8000) == 32768)
			{
				crc *= 2;
				crc = (int)(crc ^ polynomial);
			}
			else
			{
				crc *= 2;
			}
			crc %= 65536;
		}
	}
}
