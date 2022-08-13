using System;

namespace Racelogic.Utilities.Win;

public class FileCopyCompletedEventArgs : EventArgs
{
	private readonly Exception error;

	private readonly bool cancelled;

	private readonly string fileName;

	private FileCopyAction userAction = FileCopyAction.Ignore;

	public Exception Error => error;

	public bool Cancelled => cancelled;

	public string FileName => fileName;

	public FileCopyAction UserAction
	{
		get
		{
			return userAction;
		}
		set
		{
			userAction = value;
		}
	}

	public FileCopyCompletedEventArgs(Exception error, bool cancelled, string fileName)
	{
		this.error = error;
		this.cancelled = cancelled;
		this.fileName = fileName;
	}
}
