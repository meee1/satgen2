using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Racelogic.Gnss.SatGen
{
	internal readonly struct AlignedBuffer<T> : IDisposable where T : struct
	{
		private const int CacheLineSize = 64;

		private const ulong CacheLineSizeMask = 63uL;

		private readonly Memory<byte> buffer;

		private readonly MemoryHandle handle;

		private static readonly AlignedBuffer<T> empty;

		public Memory<byte> Memory
		{
			[DebuggerStepThrough]
			get
			{
				return buffer;
			}
		}

		public Span<T> Span
		{
			[DebuggerStepThrough]
			get
			{
				return MemoryMarshal.Cast<byte, T>(buffer.Span);
			}
		}

		internal unsafe IntPtr Pointer
		{
			[DebuggerStepThrough]
			get
			{
				return (IntPtr)handle.Pointer;
			}
		}

		public bool IsEmpty
		{
			[DebuggerStepThrough]
			get
			{
				return buffer.IsEmpty;
			}
		}

		public static AlignedBuffer<T> Empty
		{
			[DebuggerStepThrough]
			get
			{
				return empty;
			}
		}

		public unsafe AlignedBuffer(in int size)
		{
			if (size == 0)
			{
				this = default(AlignedBuffer<T>);
				return;
			}
			int num = size * Marshal.SizeOf(default(T));
			Memory<byte> memory = new byte[num + 63];
			MemoryHandle memoryHandle = memory.Pin();
			int num2 = (int)((long)memoryHandle.Pointer & 0x3FL);
			int start = ((num2 != 0) ? (64 - num2) : 0);
			buffer = memory.Slice(start, num);
			handle = buffer.Pin();
			memoryHandle.Dispose();
		}

		public Span<U> ToSpan<U>() where U : struct
		{
			return MemoryMarshal.Cast<byte, U>(buffer.Span);
		}

		public void Dispose()
		{
			if (!buffer.IsEmpty)
			{
				handle.Dispose();
			}
			GC.SuppressFinalize(this);
		}
	}
}
