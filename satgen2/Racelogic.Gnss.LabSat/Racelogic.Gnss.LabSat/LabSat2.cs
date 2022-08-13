using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Racelogic.Gnss.LabSat;

internal class LabSat2
{
	private readonly uint deviceIndex;

	private static readonly string[] eepromHeaders = new string[2] { "Racelogic LabSatV2", "Racelogic LabSat V2" };

	private static readonly uint maxHeaderLength = eepromHeaders.Select((string h) => (uint)h.Length).Max();

	private readonly double useExternalClocks;

	private int frameSizeWords;

	private const uint outputPipeID = 2u;

	private const int actionRetries = 3;

	private const int fpgaPollDelay = 2;

	private const int defaultMaxFpgaPolls = 150;

	private const int longMaxFpgaPolls = 2500;

	private int maxFpgaPolls = 150;

	private const int shortToByteSizeRatio = 2;

	public const int DefaultFrameSize16BitWords = 81840;

	private bool DataBusInputMode
	{
		set
		{
			ConfigureDIO((byte)(value ? 10 : 11));
		}
	}

	public LabSat2(in uint deviceIndex, in int frameSize16BitWords = 81840)
	{
		this.deviceIndex = deviceIndex;
		frameSizeWords = frameSize16BitWords;
		AIOUSB.DIO_StreamSetClocks(deviceIndex, ref useExternalClocks, ref useExternalClocks);
		ConfigureDIO(11);
	}

	public static LabSat2? GetDevice(in int frameSize16BitWords = 81840)
	{
		uint devices = AIOUSB.GetDevices();
		for (uint num = 0u; num < 32; num++)
		{
			if ((devices & (1 << (int)num)) != 0L && IsLabSat2(in num))
			{
				return new LabSat2(in num, in frameSize16BitWords);
			}
		}
		return null;
	}

	public void EnableChannels(in bool channelAEnabled, in bool channelBEnabled, in bool useSmallBuffer)
	{
		byte b = ReadControlRegister();
		b = ((!channelAEnabled) ? ((byte)(b | 4u)) : ((byte)(b & 0xFBu)));
		b = ((!channelBEnabled) ? ((byte)(b | 2u)) : ((byte)(b & 0xFDu)));
		b = (byte)(b & 0xF7u);
		b = (byte)(b & 0xEFu);
		b = ((!useSmallBuffer) ? ((byte)(b & 0xDFu)) : ((byte)(b | 0x20u)));
		byte mask = 191;
		WriteControlRegister(in b, in mask);
	}

	public void ClearBuffer()
	{
		byte b = ReadControlRegister();
		byte b2 = (byte)(b | 0x80u);
		byte mask = 191;
		WriteControlRegister(in b2, in mask);
		b2 = (byte)(b & 0x7Fu);
		mask = 191;
		WriteControlRegister(in b2, in mask);
		b2 = 191;
		WriteControlRegister(in b, in b2);
	}

	public void OpenStream(in int frameSize16BitWords = 81840)
	{
		frameSizeWords = frameSize16BitWords;
		try
		{
			AIOUSB.DIO_StreamClose(deviceIndex);
		}
		catch (Exception)
		{
		}
		byte bData = 0;
		AIOUSB.DIO_Read1(deviceIndex, 8u, ref bData);
		if (bData != 0)
		{
			uint address = 7u;
			SetAddress(in address);
			byte data = 1;
			TxData(in data);
			SetReplayPin();
			int frameCount = 1;
			StreamEmptyFrames(in frameCount);
			address = 7u;
			SetAddress(in address);
			data = 0;
			TxData(in data);
		}
		SetReplayPin();
		AIOUSB.DIO_SetStreamingBlockSize(deviceIndex, (uint)frameSizeWords);
		AIOUSB.DIO_StreamOpen(deviceIndex, 0u);
	}

	public void StreamFrame(short[] shorts)
	{
		uint bytesWritten = 0u;
		IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(shorts, 0);
		AIOUSB.AWU_GenericBulkOut(deviceIndex, 2u, pData, (uint)(shorts.Length * 2), ref bytesWritten);
	}

	public void StreamFrame(in IntPtr shortsPointer, in int shortsCount = 81840)
	{
		uint bytesWritten = 0u;
		AIOUSB.AWU_GenericBulkOut(deviceIndex, 2u, shortsPointer, (uint)(shortsCount * 2), ref bytesWritten);
	}

	public void CloseStream()
	{
		AIOUSB.DIO_StreamClose(deviceIndex);
		byte bData = 0;
		AIOUSB.DIO_Read1(deviceIndex, 8u, ref bData);
		int frameCount;
		if (bData == 0)
		{
			frameCount = 2;
			StreamEmptyFrames(in frameCount);
			return;
		}
		byte b = ReadControlRegister();
		DataBusInputMode = false;
		uint address = 7u;
		SetAddress(in address);
		byte data = 1;
		TxData(in data);
		frameCount = 2;
		StreamEmptyFrames(in frameCount);
		data = 254;
		WriteControlRegister(in b, in data);
	}

	private static bool IsLabSat2(in uint deviceIndex)
	{
		uint startAddress = 0u;
		byte[] eeprom = CustomEepromRead(in deviceIndex, in startAddress, maxHeaderLength);
		return eepromHeaders.Any((string eepromHeader) => ArrayBeginsWithText(eeprom, eepromHeader) || ArrayBeginsWithText(eeprom, eepromHeader.ToUpperInvariant()));
	}

	private static byte[]? CustomEepromRead(in uint deviceIndex, in uint startAddress, uint length)
	{
		byte[] array = new byte[length];
		if (AIOUSB.CustomEEPROMRead(deviceIndex, startAddress, ref length, ref array[0]) != 0)
		{
			return null;
		}
		return array;
	}

	private static bool ArrayBeginsWithText(byte[]? array, string text)
	{
		int length = text.Length;
		if (array == null || array!.Length < length)
		{
			return false;
		}
		byte[] array2;
		if (array!.Length == length)
		{
			array2 = array;
		}
		else
		{
			array2 = new byte[length];
			Array.Copy(array, array2, length);
		}
		return Encoding.ASCII.GetBytes(text).SequenceEqual(array2);
	}

	private void ConfigureDIO(byte outputMask)
	{
		byte[] array = new byte[4];
		AIOUSB.DIO_ReadAll(deviceIndex, ref array[0]);
		byte[] array2 = new byte[4];
		AIOUSB.DIO_ConfigureEx(deviceIndex, ref outputMask, ref array[0], ref array2[0]);
	}

	private byte ReadControlRegister()
	{
		DataBusInputMode = true;
		uint address = 7u;
		SetAddress(in address);
		return RxData();
	}

	private void WriteControlRegister(in byte b, in byte mask = 191)
	{
		DataBusInputMode = false;
		uint address = 7u;
		SetAddress(in address);
		byte data = (byte)(b & mask);
		TxData(in data);
	}

	private void SetReplayPin()
	{
		AIOUSB.DIO_Write1(deviceIndex, 9u, 1);
		AIOUSB.DIO_Write1(deviceIndex, 10u, 1);
		AIOUSB.DIO_Write1(deviceIndex, 8u, 0);
	}

	private void SetAddress(in uint address)
	{
		AIOUSB.DIO_Write1(deviceIndex, 24u, (byte)(address & 1u));
		AIOUSB.DIO_Write1(deviceIndex, 25u, (byte)((address >> 1) & 1u));
		AIOUSB.DIO_Write1(deviceIndex, 26u, (byte)((address >> 2) & 1u));
	}

	private byte RxData()
	{
		DataBusInputMode = true;
		byte Data = 0;
		AIOUSB.DIO_Write1(deviceIndex, 10u, 0);
		AIOUSB.DIO_Read8(deviceIndex, 0u, ref Data);
		AIOUSB.DIO_Write1(deviceIndex, 10u, 1);
		return Data;
	}

	private void TxData(in byte data)
	{
		DataBusInputMode = false;
		AIOUSB.DIO_Write8(deviceIndex, 0u, data);
		AIOUSB.DIO_Write1(deviceIndex, 9u, 0);
		AIOUSB.DIO_Write1(deviceIndex, 9u, 1);
	}

	internal byte[] ReadDioEepromBytes(in int startAddress, in int length)
	{
		uint dataSize = (uint)length;
		byte[] array = new byte[length];
		AIOUSB.CustomEEPROMRead(deviceIndex, (uint)startAddress, ref dataSize, ref array[0]);
		return array;
	}

	internal void WriteDioEepromBytes(in int startAddress, byte[] data)
	{
		AIOUSB.CustomEEPROMWrite(deviceIndex, (uint)startAddress, (uint)data.Length, ref data[0]);
	}

	internal byte[] ReadEepromBytes(in int startAddress, in int length)
	{
		byte[] array = new byte[length];
		array[0] = SetEepromAddressAndRead(in startAddress);
		for (int i = 1; i < length; i++)
		{
			TryAndThenPoll(delegate
			{
				uint address2 = 6u;
				SetAddress(in address2);
				byte data = 16;
				TxData(in data);
			});
			uint address = 5u;
			SetAddress(in address);
			array[i] = RxData();
		}
		return array;
	}

	internal void WriteEepromBytes(in int startAddress, byte[] data)
	{
		byte[] data2 = data;
		if (startAddress >= 64 && startAddress <= 95)
		{
			maxFpgaPolls = 2500;
		}
		if (data2.Length != 0)
		{
			SetEepromAddressAndWrite(in startAddress, data2[0]);
			int i;
			for (i = 1; i < data2.Length; i++)
			{
				TryAndThenPoll(delegate
				{
					LabSat2 labSat = this;
					uint address = 5u;
					labSat.SetAddress(in address);
					TxData(in data2[i]);
					LabSat2 labSat2 = this;
					address = 6u;
					labSat2.SetAddress(in address);
					LabSat2 labSat3 = this;
					byte data3 = 8;
					labSat3.TxData(in data3);
				});
			}
		}
		maxFpgaPolls = 150;
	}

	private byte SetEepromAddressAndRead(in int address)
	{
		byte highAddress = (byte)(address >> 8);
		byte lowAddress = (byte)address;
		TryAndThenPoll(delegate
		{
			LabSat2 labSat = this;
			uint address3 = 3u;
			labSat.SetAddress(in address3);
			TxData(in highAddress);
			LabSat2 labSat2 = this;
			address3 = 4u;
			labSat2.SetAddress(in address3);
			TxData(in lowAddress);
			LabSat2 labSat3 = this;
			address3 = 6u;
			labSat3.SetAddress(in address3);
			LabSat2 labSat4 = this;
			byte data = 64;
			labSat4.TxData(in data);
		});
		uint address2 = 5u;
		SetAddress(in address2);
		return RxData();
	}

	private void SetEepromAddressAndWrite(in int address, byte data)
	{
		byte highAddress = (byte)(address >> 8);
		byte lowAddress = (byte)address;
		TryAndThenPoll(delegate
		{
			LabSat2 labSat = this;
			uint address2 = 3u;
			labSat.SetAddress(in address2);
			TxData(in highAddress);
			LabSat2 labSat2 = this;
			address2 = 4u;
			labSat2.SetAddress(in address2);
			TxData(in lowAddress);
			LabSat2 labSat3 = this;
			address2 = 5u;
			labSat3.SetAddress(in address2);
			TxData(in data);
			LabSat2 labSat4 = this;
			address2 = 6u;
			labSat4.SetAddress(in address2);
			LabSat2 labSat5 = this;
			byte data2 = 32;
			labSat5.TxData(in data2);
		});
	}

	private void TryAndThenPoll(Action action)
	{
		for (int i = 0; i < 3; i++)
		{
			action();
			if (PollFpga())
			{
				break;
			}
		}
	}

	private bool PollFpga()
	{
		DataBusInputMode = true;
		uint address = 6u;
		SetAddress(in address);
		AIOUSB.DIO_Write1(deviceIndex, 10u, 0);
		int num = 0;
		while (true)
		{
			byte bData = 0;
			AIOUSB.DIO_Read1(deviceIndex, 7u, ref bData);
			if (bData != 0)
			{
				break;
			}
			Thread.Sleep(2);
			if (num++ > maxFpgaPolls)
			{
				return false;
			}
			num++;
		}
		AIOUSB.DIO_Write1(deviceIndex, 10u, 1);
		return true;
	}

	private void StreamEmptyFrames(in int frameCount)
	{
		AIOUSB.DIO_SetStreamingBlockSize(deviceIndex, 4096u);
		AIOUSB.DIO_StreamOpen(deviceIndex, 0u);
		short[] array = new short[4096];
		for (int i = 0; i < 4096; i++)
		{
			array[i] = -1;
		}
		IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
		uint bytesWritten = 0u;
		for (int j = 0; j < frameCount; j++)
		{
			AIOUSB.AWU_GenericBulkOut(deviceIndex, 2u, pData, 4096u, ref bytesWritten);
		}
		AIOUSB.DIO_StreamClose(deviceIndex);
	}
}
