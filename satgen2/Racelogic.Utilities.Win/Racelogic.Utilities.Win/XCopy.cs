using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace Racelogic.Utilities.Win;

internal class XCopy
{
	private delegate CopyProgressResult CopyProgressRoutine(long TotalFileSize, long TotalBytesTransferred, long StreamSize, long StreamBytesTransferred, uint dwStreamNumber, CopyProgressCallbackReason dwCallbackReason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData);

	private enum CopyProgressResult : uint
	{
		PROGRESS_CONTINUE,
		PROGRESS_CANCEL,
		PROGRESS_STOP,
		PROGRESS_QUIET
	}

	private enum CopyProgressCallbackReason : uint
	{
		CALLBACK_CHUNK_FINISHED,
		CALLBACK_STREAM_SWITCH
	}

	[Flags]
	private enum CopyFileFlags : uint
	{
		COPY_FILE_FAIL_IF_EXISTS = 1u,
		COPY_FILE_NO_BUFFERING = 0x1000u,
		COPY_FILE_RESTARTABLE = 2u,
		COPY_FILE_OPEN_SOURCE_FOR_WRITE = 4u,
		COPY_FILE_ALLOW_DECRYPTED_DESTINATION = 8u
	}

	private int isAbortRequested;

	private string fileSource;

	private string fileDestination;

	public bool IsAbortRequested => isAbortRequested != 0;

	public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

	public event EventHandler<FileCopyCompletedEventArgs> Completed;

	internal XCopy()
	{
	}

	public bool Copy(string source, string destination, bool overwrite = true, bool buffering = true)
	{
		isAbortRequested = 0;
		CopyFileFlags copyFileFlags = CopyFileFlags.COPY_FILE_RESTARTABLE;
		if (!overwrite)
		{
			copyFileFlags |= CopyFileFlags.COPY_FILE_FAIL_IF_EXISTS;
		}
		if (!buffering)
		{
			copyFileFlags |= CopyFileFlags.COPY_FILE_NO_BUFFERING;
		}
		fileSource = source;
		fileDestination = destination;
		try
		{
			if (!CopyFileEx(fileSource, fileDestination, CopyProgressHandler, IntPtr.Zero, ref isAbortRequested, copyFileFlags))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}
		catch (Exception error)
		{
			this.Completed?.Invoke(this, new FileCopyCompletedEventArgs(error, isAbortRequested > 0, fileSource));
			return false;
		}
		return true;
	}

	public void Abort()
	{
		isAbortRequested = 1;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CopyFileEx(string lpExistingFileName, string lpNewFileName, CopyProgressRoutine lpProgressRoutine, IntPtr lpData, ref int pbCancel, CopyFileFlags dwCopyFlags);

	private CopyProgressResult CopyProgressHandler(long total, long transferred, long streamSize, long streamByteTrans, uint dwStreamNumber, CopyProgressCallbackReason reason, IntPtr hSourceFile, IntPtr hDestinationFile, IntPtr lpData)
	{
		if (reason == CopyProgressCallbackReason.CALLBACK_CHUNK_FINISHED && this.ProgressChanged != null)
		{
			this.ProgressChanged(this, new ProgressChangedEventArgs((int)(100 * transferred / total), fileSource));
		}
		if (transferred >= total && this.Completed != null)
		{
			this.Completed(this, new FileCopyCompletedEventArgs(null, cancelled: false, fileSource));
		}
		return CopyProgressResult.PROGRESS_CONTINUE;
	}
}
