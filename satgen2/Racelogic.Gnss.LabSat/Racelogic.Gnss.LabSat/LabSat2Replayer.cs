using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Racelogic.DataTypes;

namespace Racelogic.Gnss.LabSat;

public class LabSat2Replayer : LabSat2Controller
{
	private const int lockTimeout = 10000;

	private readonly Queue<Memory<byte>> bufferQueue = new Queue<Memory<byte>>(2);

	private readonly Dictionary<Memory<byte>, MemoryHandle> bufferHandleDictionary = new Dictionary<Memory<byte>, MemoryHandle>();

	private readonly SyncLock bufferQueueLock = new SyncLock("BufferQueueLock", 10000);

	private readonly SemaphoreSlim bufferQueueSemaphore = new SemaphoreSlim(2);

	private readonly Thread replayThread;

	private readonly SemaphoreSlim startSemaphore = new SemaphoreSlim(1);

	private readonly SemaphoreSlim cancelSemaphore = new SemaphoreSlim(1);

	private const int replayThreadJoinTimeout = 5000;

	private int readOffset;

	private bool playing;

	private readonly int frameSizeBytes;

	private readonly bool handleBufferUnderrun;

	private readonly SyncLock bufferUnderrunStopwatchLock = new SyncLock("BufferUnderrunStopwatchLock", 10000);

	private readonly Stopwatch bufferUnderrunStopwatch = new Stopwatch();

	private ReadOnlyMemory<byte> emptyBytes;

	private MemoryHandle emptyBytesHandle;

	private IntPtr bufferPointer;

	public const int MaxBufferCount = 2;

	public event EventHandler<TimeSpan>? BufferUnderrun;

	public LabSat2Replayer(ChannelBand[] frequencyBands, in int quantization, in bool useSmallBuffer = false, in bool handleBufferUnderrun = true)
		: base(frequencyBands, in quantization, in useSmallBuffer)
	{
		this.handleBufferUnderrun = handleBufferUnderrun;
		frameSizeBytes = base.FrameSizeWords << 1;
		emptyBytes = new byte[frameSizeBytes];
		emptyBytesHandle = emptyBytes.Pin();
		startSemaphore.Wait();
		cancelSemaphore.Wait();
		replayThread = new Thread(new ThreadStart(DoWork))
		{
			IsBackground = true,
			Priority = ThreadPriority.Highest,
			Name = GetType().Name
		};
		replayThread.Start();
	}

	public void Play()
	{
		if (base.IsConnected)
		{
			playing = true;
			startSemaphore.Release();
		}
	}

	public void Stop()
	{
		playing = false;
		cancelSemaphore.Release();
		Thread thread = replayThread;
		if (thread != null && thread.IsAlive)
		{
			replayThread.Join(5000);
		}
		Thread thread2 = replayThread;
		if (thread2 != null && thread2.IsAlive)
		{
			replayThread.Abort();
		}
		base.Device?.CloseStream();
		if (!isDisposed)
		{
			startSemaphore.Wait();
			cancelSemaphore.Wait();
		}
	}

	public unsafe void AddBuffer(in Memory<byte> buffer)
	{
		if (!base.IsConnected || isDisposed)
		{
			return;
		}
		ThreadPriority priority = Thread.CurrentThread.Priority;
		Thread.CurrentThread.Priority = ThreadPriority.Highest;
		bufferQueueSemaphore.Wait();
		Thread.CurrentThread.Priority = priority;
		using (bufferQueueLock.Lock())
		{
			MemoryHandle value = buffer.Pin();
			bufferHandleDictionary.Add(buffer, value);
			bufferQueue.Enqueue(buffer);
			if (bufferPointer == IntPtr.Zero)
			{
				bufferPointer = (IntPtr)value.Pointer;
			}
		}
		if (!handleBufferUnderrun)
		{
			return;
		}
		TimeSpan? timeSpan = null;
		using (bufferUnderrunStopwatchLock.Lock())
		{
			if (bufferUnderrunStopwatch.IsRunning)
			{
				bufferUnderrunStopwatch.Stop();
				timeSpan = bufferUnderrunStopwatch.Elapsed;
			}
		}
		if (timeSpan.HasValue)
		{
			TimeSpan delay = timeSpan.Value;
			OnBufferUnderrun(in delay);
		}
	}

	private void DoWork()
	{
		if (!WaitForSemaphore(startSemaphore))
		{
			return;
		}
		while (playing)
		{
			IntPtr shortsPointer = ReadDataFromBuffer(in frameSizeBytes);
			LabSat2? labSat = base.Device;
			if (labSat != null)
			{
				int shortsCount = base.FrameSizeWords;
				labSat!.StreamFrame(in shortsPointer, in shortsCount);
			}
		}
		startSemaphore.Release();
	}

	private unsafe IntPtr ReadDataFromBuffer(in int byteCount)
	{
		LockToken lockToken = bufferQueueLock.Lock();
		if (bufferQueue.Count == 0)
		{
			using (bufferUnderrunStopwatchLock.Lock())
			{
				if (!bufferUnderrunStopwatch.IsRunning)
				{
					bufferUnderrunStopwatch.Start();
				}
			}
			if (handleBufferUnderrun)
			{
				readOffset += byteCount;
				MemoryHandle emptyArray = GetEmptyArray(in byteCount);
				lockToken.Dispose();
				return (IntPtr)emptyArray.Pointer;
			}
			lockToken.Dispose();
			while (bufferQueue.Count == 0)
			{
				Thread.Sleep(1);
			}
			lockToken = bufferQueueLock.Lock();
			LockToken lockToken3 = bufferUnderrunStopwatchLock.Lock();
			bufferUnderrunStopwatch.Stop();
			TimeSpan delay = bufferUnderrunStopwatch.Elapsed;
			lockToken3.Dispose();
			OnBufferUnderrun(in delay);
		}
		Memory<byte> key = bufferQueue.Peek();
		bufferPointer = (IntPtr)bufferHandleDictionary[key].Pointer;
		while (readOffset >= key.Length)
		{
			bufferQueue.Dequeue();
			bufferHandleDictionary.Remove(key, out var value);
			value.Dispose();
			bufferQueueSemaphore.Release();
			readOffset -= key.Length;
			if (bufferQueue.Count == 0)
			{
				using (bufferUnderrunStopwatchLock.Lock())
				{
					if (!bufferUnderrunStopwatch.IsRunning)
					{
						bufferUnderrunStopwatch.Start();
					}
				}
				if (handleBufferUnderrun)
				{
					readOffset += byteCount;
					MemoryHandle emptyArray2 = GetEmptyArray(in byteCount);
					lockToken.Dispose();
					return (IntPtr)emptyArray2.Pointer;
				}
				lockToken.Dispose();
				while (bufferQueue.Count == 0)
				{
					Thread.Sleep(1);
				}
				lockToken = bufferQueueLock.Lock();
				LockToken lockToken4 = bufferUnderrunStopwatchLock.Lock();
				bufferUnderrunStopwatch.Stop();
				TimeSpan delay2 = bufferUnderrunStopwatch.Elapsed;
				lockToken4.Dispose();
				OnBufferUnderrun(in delay2);
			}
			key = bufferQueue.Peek();
			bufferPointer = (IntPtr)bufferHandleDictionary[key].Pointer;
		}
		IntPtr result = bufferPointer + readOffset;
		readOffset += byteCount;
		lockToken.Dispose();
		return result;
	}

	private void OnBufferUnderrun(in TimeSpan delay)
	{
		this.BufferUnderrun?.Invoke(this, delay);
	}

	private MemoryHandle GetEmptyArray(in int byteCount)
	{
		if (byteCount != emptyBytes.Length)
		{
			if (!emptyBytes.IsEmpty)
			{
				emptyBytesHandle.Dispose();
			}
			emptyBytes = new byte[byteCount];
			emptyBytesHandle = emptyBytes.Pin();
		}
		return emptyBytesHandle;
	}

	private bool WaitForSemaphore(SemaphoreSlim semaphore)
	{
		if (WaitHandle.WaitAny(new WaitHandle[2] { semaphore.AvailableWaitHandle, cancelSemaphore.AvailableWaitHandle }) > 0)
		{
			startSemaphore.Release();
			cancelSemaphore.Release();
			return false;
		}
		return true;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (isDisposed)
		{
			return;
		}
		isDisposed = true;
		if (!disposing)
		{
			return;
		}
		Stop();
		emptyBytesHandle.Dispose();
		foreach (MemoryHandle value in bufferHandleDictionary.Values)
		{
			value.Dispose();
		}
		bufferHandleDictionary.Clear();
		bufferQueueSemaphore.Dispose();
		startSemaphore.Dispose();
		cancelSemaphore.Dispose();
	}
}
