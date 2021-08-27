using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
	internal sealed class ModulationBank : IDisposable
	{
		private const int bufferLifetime = 600;

		private const int lockTimeout = 10000;

		private readonly SyncLock bufferLock = new SyncLock("ModulationBank Lock", 10000);

		private readonly Racelogic.DataTypes.ArrayPool<sbyte> bufferPool = new Racelogic.DataTypes.ArrayPool<sbyte>(600);

		private readonly Dictionary<GnssTime, Dictionary<IntPtr, (sbyte[] Buffer, MemoryHandle Handle)>> bufferDictionary = new Dictionary<GnssTime, Dictionary<IntPtr, (sbyte[], MemoryHandle)>>();

		private readonly FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, byte[][]> navigationDataLibraryCA;

		private readonly FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]> modulationSignalLibraryP;

		private readonly FixedSizeDictionary<uint, sbyte[][]> pCodeLibrary;

		private sbyte[]? mCodeModulationSignal;

		private readonly FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]> modulationSignalLibraryOF;

		private readonly FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]> modulationSignalLibraryBI;

		private readonly FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, byte[][]> navigationDataLibrarySps;

		private bool disposedValue;

		public FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, byte[][]> NavigationDataLibraryCA
		{
			[DebuggerStepThrough]
			get
			{
				return navigationDataLibraryCA;
			}
		}

		public FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]> ModulationSignalLibraryOF
		{
			[DebuggerStepThrough]
			get
			{
				return modulationSignalLibraryOF;
			}
		}

		public FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]> ModulationSignalLibraryBI
		{
			[DebuggerStepThrough]
			get
			{
				return modulationSignalLibraryBI;
			}
		}

		public FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, byte[][]> NavigationDataLibrarySPS
		{
			[DebuggerStepThrough]
			get
			{
				return navigationDataLibrarySps;
			}
		}

		public FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]> ModulationSignalLibraryP
		{
			[DebuggerStepThrough]
			get
			{
				return modulationSignalLibraryP;
			}
		}

		public FixedSizeDictionary<uint, sbyte[][]> PCodeX1Library
		{
			[DebuggerStepThrough]
			get
			{
				return pCodeLibrary;
			}
		}

		public sbyte[]? MCode
		{
			[DebuggerStepThrough]
			get
			{
				return mCodeModulationSignal;
			}
			[DebuggerStepThrough]
			set
			{
				mCodeModulationSignal = value;
			}
		}

		public ModulationBank(double sliceLength, int concurrency)
		{
			int capacity = concurrency + 1;
			navigationDataLibraryCA = new FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, byte[][]>(capacity);
			modulationSignalLibraryP = new FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]>(capacity);
			int capacity2 = (int)(sliceLength / 1.5).SafeCeiling() + 1;
			pCodeLibrary = new FixedSizeDictionary<uint, sbyte[][]>(capacity2);
			pCodeLibrary.ItemRecycled += OnPCodeLibraryItemRecycled;
			modulationSignalLibraryOF = new FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]>(capacity);
			modulationSignalLibraryBI = new FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, sbyte[][]>(capacity);
			navigationDataLibrarySps = new FixedSizeDictionary<Range<GnssTime, GnssTimeSpan>, byte[][]>(capacity);
		}

		public sbyte[] GetBuffer(in int size)
		{
			using (bufferLock.Lock())
			{
				return bufferPool.Take(size);
			}
		}

		public unsafe sbyte[] GetPinnedBuffer(in GnssTime timeStamp, in uint size, uint maxSize = 0u)
		{
			if (maxSize == 0)
			{
				maxSize = size;
			}
			using (bufferLock.Lock())
			{
				sbyte[] array = bufferPool.Take(size, maxSize);
				if (!bufferDictionary.TryGetValue(timeStamp, out var value))
				{
					value = new Dictionary<IntPtr, (sbyte[], MemoryHandle)>();
					bufferDictionary[timeStamp] = value;
				}
				MemoryHandle item = array.AsMemory().Pin();
				value[(IntPtr)item.Pointer] = (array, item);
				return array;
			}
		}

		public unsafe sbyte[] FindBuffer(in GnssTime timeStamp, sbyte* bufferPointer)
		{
			using (bufferLock.Lock())
			{
				if (bufferDictionary.TryGetValue(timeStamp, out var value))
				{
					return value.Values.FirstOrDefault<(sbyte[], MemoryHandle)>(((sbyte[] Buffer, MemoryHandle Handle) bufferEntry) => bufferEntry.Handle.Pointer == bufferPointer).Item1;
				}
			}
			return Array.Empty<sbyte>();
		}

		public unsafe sbyte* FindBufferPointer(in GnssTime timeStamp, sbyte[] buffer)
		{
			sbyte[] buffer2 = buffer;
			IntPtr intPtr = IntPtr.Zero;
			using (bufferLock.Lock())
			{
				if (bufferDictionary.TryGetValue(timeStamp, out var value))
				{
					intPtr = (IntPtr)value.Values.FirstOrDefault<(sbyte[], MemoryHandle)>(((sbyte[] Buffer, MemoryHandle Handle) bufferEntry) => bufferEntry.Buffer == buffer2).Item2.Pointer;
				}
				if (intPtr == IntPtr.Zero && bufferDictionary.TryGetValue(GnssTime.MinValue, out value))
				{
					intPtr = (IntPtr)value.Values.FirstOrDefault<(sbyte[], MemoryHandle)>(((sbyte[] Buffer, MemoryHandle Handle) bufferEntry) => bufferEntry.Buffer == buffer2).Item2.Pointer;
				}
			}
			return (sbyte*)(void*)intPtr;
		}

		public void Recycle(sbyte[] buffer)
		{
			using (bufferLock.Lock())
			{
				bufferPool.Recycle(buffer);
			}
		}

		public void Recycle(in GnssTime timeStamp)
		{
			using (bufferLock.Lock())
			{
				if (!bufferDictionary.Remove(timeStamp, out var value))
				{
					return;
				}
				foreach (var value2 in value.Values)
				{
					MemoryHandle item = value2.Item2;
					item.Dispose();
					var (buffer, _) = value2;
					bufferPool.Recycle(buffer);
				}
			}
		}

		private void OnPCodeLibraryItemRecycled(object? sender, ItemRecycledEventArgs<uint, sbyte[][]> args)
		{
			foreach (sbyte[] item in args.Value.Where((sbyte[] v) => v != null))
			{
				Recycle(item);
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposedValue)
			{
				return;
			}
			disposedValue = true;
			if (!disposing)
			{
				return;
			}
			pCodeLibrary.Clear();
			pCodeLibrary.ItemRecycled -= OnPCodeLibraryItemRecycled;
			bufferPool.Recycle(mCodeModulationSignal);
			foreach (GnssTime key in bufferDictionary.Keys)
			{
				GnssTime timeStamp = key;
				Recycle(in timeStamp);
			}
			bufferDictionary.Clear();
			bufferPool.Clear(disposeBuffersInUse: true);
		}
	}
}
