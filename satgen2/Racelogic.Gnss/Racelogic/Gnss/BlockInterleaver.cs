using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Racelogic.Gnss
{
	public class BlockInterleaver<T>
	{
		private readonly int columnCount;

		private readonly int rowCount;

		public int RowCount
		{
			[DebuggerStepThrough]
			get
			{
				return rowCount;
			}
		}

		public int ColumnCount
		{
			[DebuggerStepThrough]
			get
			{
				return columnCount;
			}
		}

		public BlockInterleaver(in int columns, in int rows)
		{
			columnCount = columns;
			rowCount = rows;
		}

		public IEnumerable<T> Interleave(IReadOnlyList<T> data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			if (data.Count != RowCount * ColumnCount)
			{
				throw new ArgumentException("Data length does not match the block size", "data");
			}
			for (int row = 0; row < RowCount; row++)
			{
				for (int column = 0; column < ColumnCount; column++)
				{
					yield return data[column * RowCount + row];
				}
			}
		}
	}
}
