using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Racelogic.Utilities;

public class WinFileIO : IDisposable
{
	private const int blockSize = 1048576;

	private GCHandle gchBuf;

	private IntPtr fileHandle = IntPtr.Zero;

	private IntPtr pBuffer;

	private bool disposedValue;

	public WinFileIO()
	{
	}

	public WinFileIO(Array Buffer)
		: this()
	{
		PinBuffer(Buffer);
	}

	public WinFileIO(IntPtr bufferPointer)
		: this()
	{
		SetBuffer(bufferPointer);
	}

	public void OpenForReading(string fileName)
	{
		Close();
		fileHandle = NativeMethods.CreateFile(fileName, 2147483648u, 0u, 0u, 3u, 0u, 0);
		if (fileHandle == IntPtr.Zero)
		{
			throw new IOException(new Win32Exception().Message);
		}
	}

	public int Read(int numBytesToRead)
	{
		return Read(pBuffer, numBytesToRead);
	}

	public int Read(IntPtr bufferPointer, int numBytesToRead)
	{
		int numberOfBytesRead = 0;
		if (!NativeMethods.ReadFile(fileHandle, bufferPointer, numBytesToRead, ref numberOfBytesRead, 0))
		{
			throw new IOException(new Win32Exception().Message);
		}
		return numberOfBytesRead;
	}

	public int ReadUntilEOF()
	{
		return ReadUntilEOF(pBuffer);
	}

	public int ReadUntilEOF(IntPtr bufferPointer)
	{
		int numberOfBytesRead = 0;
		int num = 0;
		IntPtr intPtr = bufferPointer;
		while (true)
		{
			if (!NativeMethods.ReadFile(fileHandle, intPtr, 1048576, ref numberOfBytesRead, 0))
			{
				throw new IOException(new Win32Exception().Message);
			}
			if (numberOfBytesRead == 0)
			{
				break;
			}
			num += numberOfBytesRead;
			intPtr += numberOfBytesRead;
		}
		return num;
	}

	public int ReadBlocks(int numBytesToRead)
	{
		return ReadBlocks(pBuffer, numBytesToRead);
	}

	public int ReadBlocks(IntPtr bufferPointer, int numBytesToRead)
	{
		int numberOfBytesRead = 0;
		int num = 0;
		IntPtr intPtr = bufferPointer;
		do
		{
			int numberOfBytesToRead = Math.Min(1048576, numBytesToRead - num);
			if (!NativeMethods.ReadFile(fileHandle, intPtr, numberOfBytesToRead, ref numberOfBytesRead, 0))
			{
				throw new IOException(new Win32Exception().Message);
			}
			if (numberOfBytesRead == 0)
			{
				break;
			}
			num += numberOfBytesRead;
			intPtr += numberOfBytesRead;
		}
		while (num < numBytesToRead);
		return num;
	}

	public void OpenForWriting(string fileName)
	{
		Close();
		fileHandle = NativeMethods.CreateFile(fileName, 1073741824u, 0u, 0u, 2u, 0u, 0);
		if (fileHandle == IntPtr.Zero)
		{
			throw new IOException(new Win32Exception().Message);
		}
	}

	public int Write(int numBytesToWrite)
	{
		return Write(pBuffer, numBytesToWrite);
	}

	public int Write(IntPtr bufferPointer, int numBytesToWrite)
	{
		int numberOfBytesWritten = 0;
		if (!NativeMethods.WriteFile(fileHandle, bufferPointer, numBytesToWrite, ref numberOfBytesWritten, 0))
		{
			throw new IOException(new Win32Exception().Message);
		}
		return numberOfBytesWritten;
	}

	public int WriteBlocks(int numBytesToWrite)
	{
		return WriteBlocks(pBuffer, numBytesToWrite);
	}

	public int WriteBlocks(IntPtr bufferPointer, int numBytesToWrite)
	{
		int numberOfBytesWritten = 0;
		int num = numBytesToWrite;
		int num2 = 0;
		IntPtr intPtr = bufferPointer;
		do
		{
			int num3 = Math.Min(num, 1048576);
			if (fileHandle == IntPtr.Zero)
			{
				break;
			}
			if (!NativeMethods.WriteFile(fileHandle, intPtr, num3, ref numberOfBytesWritten, 0))
			{
				throw new IOException(new Win32Exception().Message);
			}
			intPtr += num3;
			num2 += num3;
			num -= num3;
		}
		while (num > 0);
		return num2;
	}

	public void SetBuffer(IntPtr bufferPointer)
	{
		UnpinBuffer();
		pBuffer = bufferPointer;
	}

	public void PinBuffer(Array buffer)
	{
		UnpinBuffer();
		if (buffer != null)
		{
			gchBuf = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			IntPtr buffer2 = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
			SetBuffer(buffer2);
		}
	}

	public void UnpinBuffer()
	{
		_ = gchBuf;
		if (gchBuf.IsAllocated)
		{
			gchBuf.Free();
		}
	}

	public bool Close()
	{
		bool result = true;
		if (fileHandle != IntPtr.Zero)
		{
			result = NativeMethods.CloseHandle(fileHandle);
			fileHandle = IntPtr.Zero;
		}
		return result;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			disposedValue = true;
			if (disposing)
			{
				Close();
				UnpinBuffer();
			}
		}
	}

	~WinFileIO()
	{
		Dispose(disposing: false);
	}
}
