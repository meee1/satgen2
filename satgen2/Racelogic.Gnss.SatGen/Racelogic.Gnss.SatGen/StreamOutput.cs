using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

public abstract class StreamOutput : Output
{
	private WinFileIO? outputFile;

	private string? filePath;

	private long currentFileLength;

	private bool isDisposed;

	protected string? FilePath
	{
		[DebuggerStepThrough]
		get
		{
			return filePath;
		}
		[DebuggerStepThrough]
		[param: DisallowNull]
		set
		{
			filePath = value;
		}
	}

	protected WinFileIO? OutputFile
	{
		[DebuggerStepThrough]
		get
		{
			return outputFile;
		}
		[DebuggerStepThrough]
		[param: DisallowNull]
		set
		{
			outputFile = value;
		}
	}

	protected long CurrentFileLength
	{
		[DebuggerStepThrough]
		get
		{
			return currentFileLength;
		}
		[DebuggerStepThrough]
		set
		{
			currentFileLength = value;
		}
	}

	protected StreamOutput(string? filePath)
	{
		this.filePath = filePath;
	}

	protected bool CreateFile(string filePath)
	{
		Close();
		FilePath = filePath;
		string directoryName = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		OutputFile = new WinFileIO();
		try
		{
			OutputFile!.OpenForWriting(filePath);
		}
		catch (IOException ex)
		{
			OnError(ex);
			return false;
		}
		return true;
	}

	protected unsafe virtual bool WriteBuffer(in Memory<byte> buffer)
	{
		using MemoryHandle memoryHandle = buffer.Pin();
		IntPtr bufferPointer = (IntPtr)memoryHandle.Pointer;
		int byteCount = buffer.Length;
		return WriteBuffer(in bufferPointer, in byteCount);
	}

	protected unsafe virtual bool WriteBuffer(in Memory<byte> buffer, in int byteCount)
	{
		using MemoryHandle memoryHandle = buffer.Pin();
		IntPtr bufferPointer = (IntPtr)memoryHandle.Pointer;
		return WriteBuffer(in bufferPointer, in byteCount);
	}

	[MethodImpl(MethodImplOptions.Synchronized)]
	protected bool WriteBuffer(in IntPtr bufferPointer, in int byteCount)
	{
		if (OutputFile == null || byteCount <= 0)
		{
			return false;
		}
		try
		{
			OutputFile!.WriteBlocks(bufferPointer, byteCount);
		}
		catch (IOException ex)
		{
			OnError(ex);
			return false;
		}
		CurrentFileLength += byteCount;
		return true;
	}

	protected unsafe bool WriteBuffer(byte[] buffer)
	{
		if (OutputFile == null || buffer == null || buffer.Length == 0)
		{
			return false;
		}
		using MemoryHandle memoryHandle = buffer.AsMemory().Pin();
		try
		{
			OutputFile!.WriteBlocks((IntPtr)memoryHandle.Pointer, buffer.Length);
		}
		catch (IOException ex)
		{
			OnError(ex);
			return false;
		}
		CurrentFileLength += buffer.Length;
		return true;
	}

	public override void Close()
	{
		outputFile?.Close();
		CurrentFileLength = 0L;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing)
			{
				outputFile?.Dispose();
				outputFile = null;
			}
		}
	}
}
