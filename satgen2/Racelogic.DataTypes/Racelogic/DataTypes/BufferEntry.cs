using System.Diagnostics;

namespace Racelogic.DataTypes
{
	internal struct BufferEntry<T> where T : class
	{
		private readonly T buffer;

		private long timestamp;

		public T Buffer
		{
			[DebuggerStepThrough]
			get
			{
				return buffer;
			}
		}

		public long LastUsedTime
		{
			[DebuggerStepThrough]
			get
			{
				return timestamp;
			}
			[DebuggerStepThrough]
			set
			{
				timestamp = value;
			}
		}

		public BufferEntry(T buffer)
		{
			this.buffer = buffer;
			timestamp = 0L;
		}
	}
}
