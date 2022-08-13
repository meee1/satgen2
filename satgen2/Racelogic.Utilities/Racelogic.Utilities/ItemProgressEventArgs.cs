using System;

namespace Racelogic.Utilities;

public class ItemProgressEventArgs : EventArgs
{
	private readonly int processedItemCount;

	private readonly int totalItemCount;

	private readonly object item;

	public int ProcessedItemCount => processedItemCount;

	public int TotalItemCount => totalItemCount;

	public object Item => item;

	public ItemProgressEventArgs(int processedItemCount, int totalItemCount, object item)
	{
		this.processedItemCount = processedItemCount;
		this.totalItemCount = totalItemCount;
		this.item = item;
	}
}
