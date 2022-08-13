using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Racelogic.Gnss.LabSat;

internal static class AIOUSB
{
	private const string dllName = "AIOUSB.dll";

	internal const uint PORTA = 0u;

	internal const uint PORTA_7 = 7u;

	internal const uint PORTB_0 = 8u;

	internal const uint PORTB_1 = 9u;

	internal const uint PORTB_2 = 10u;

	internal const uint PORTD_0 = 24u;

	internal const uint PORTD_1 = 25u;

	internal const uint PORTD_2 = 26u;

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBGetDevices")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint GetDevices();

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBCustomEEPROMRead")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint CustomEEPROMRead(uint deviceIndex, uint startAddress, ref uint dataSize, ref byte pData);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBCustomEEPROMWrite")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint CustomEEPROMWrite(uint deviceIndex, uint startAddress, uint dataSize, ref byte pData);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_ConfigureEx")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_ConfigureEx(uint deviceIndex, ref byte pOutMask, ref byte pData, ref byte pTristateMask);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_Read1")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_Read1(uint deviceIndex, uint bitIndex, ref byte bData);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_Read8")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_Read8(uint deviceIndex, uint byteIndex, ref byte Data);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_ReadAll")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_ReadAll(uint deviceIndex, ref byte Data);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_Write1")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_Write1(uint deviceIndex, uint bitIndex, byte bData);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_Write8")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_Write8(uint deviceIndex, uint byteIndex, byte Data);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_StreamSetClocks")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_StreamSetClocks(uint deviceIndex, ref double readClockHz, ref double writeClockHz);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBAIOUSB_SetStreamingBlockSize")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_SetStreamingBlockSize(uint deviceIndex, uint blockSize);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_StreamOpen")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_StreamOpen(uint deviceIndex, uint bIsRead);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl)]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint AWU_GenericBulkOut(uint deviceIndex, uint pipeID, IntPtr pData, uint dataSize, ref uint bytesWritten);

	[DllImport("AIOUSB.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "VBDIO_StreamClose")]
	[SuppressUnmanagedCodeSecurity]
	internal static extern uint DIO_StreamClose(uint deviceIndex);
}
