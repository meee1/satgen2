using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Racelogic.Utilities;

internal static class NativeMethods
{
	[Flags]
	public enum ExecutionStates : uint
	{
		SystemRequired = 1u,
		DisplayRequired = 2u,
		AwayModeRequired = 0x40u,
		Continuous = 0x80000000u
	}

	[Flags]
	public enum EFileAccess : uint
	{
		AccessSystemSecurity = 0x1000000u,
		MaximumAllowed = 0x2000000u,
		Delete = 0x10000u,
		ReadControl = 0x20000u,
		WriteDAC = 0x40000u,
		WriteOwner = 0x80000u,
		Synchronize = 0x100000u,
		StandardRightsRequired = 0xF0000u,
		StandardRightsRead = 0x20000u,
		StandardRightsWrite = 0x20000u,
		StandardRightsExecute = 0x20000u,
		StandardRightsAll = 0x1F0000u,
		SpecificRightsAll = 0xFFFFu,
		FILE_READ_DATA = 1u,
		FILE_LIST_DIRECTORY = 1u,
		FILE_WRITE_DATA = 2u,
		FILE_ADD_FILE = 2u,
		FILE_APPEND_DATA = 4u,
		FILE_ADD_SUBDIRECTORY = 4u,
		FILE_CREATE_PIPE_INSTANCE = 4u,
		FILE_READ_EA = 8u,
		FILE_WRITE_EA = 0x10u,
		FILE_EXECUTE = 0x20u,
		FILE_TRAVERSE = 0x20u,
		FILE_DELETE_CHILD = 0x40u,
		FILE_READ_ATTRIBUTES = 0x80u,
		FILE_WRITE_ATTRIBUTES = 0x100u,
		GenericRead = 0x80000000u,
		GenericWrite = 0x40000000u,
		GenericExecute = 0x20000000u,
		GenericAll = 0x10000000u,
		SPECIFIC_RIGHTS_ALL = 0xFFFFu,
		FILE_ALL_ACCESS = 0x1F01FFu,
		FILE_GENERIC_READ = 0x120089u,
		FILE_GENERIC_WRITE = 0x120116u,
		FILE_GENERIC_EXECUTE = 0x1200A0u
	}

	public enum ECreationDisposition : uint
	{
		New = 1u,
		CreateAlways,
		OpenExisting,
		OpenAlways,
		TruncateExisting
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	public static extern ExecutionStates SetThreadExecutionState(ExecutionStates executionStateFlags);

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	public static extern IntPtr CreateFile(string fileName, uint desiredAccess, uint shareMode, uint securityAttributes, uint creationDisposition, uint flagsAndAttributes, int templateFileHandle);

	[DllImport("kernel32", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	public static extern bool ReadFile(IntPtr fileHandle, IntPtr pBuffer, int numberOfBytesToRead, ref int numberOfBytesRead, int overlapped);

	[DllImport("kernel32", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	public static extern bool WriteFile(IntPtr fileHandle, IntPtr pBuffer, int numberOfBytesToWrite, ref int numberOfBytesWritten, int overlapped);

	[DllImport("kernel32", SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	public static extern bool CloseHandle(IntPtr fileHandle);
}
