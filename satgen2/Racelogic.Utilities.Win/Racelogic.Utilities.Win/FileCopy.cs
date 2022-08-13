using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace Racelogic.Utilities.Win;

public class FileCopy : DependencyObject
{
	private XCopy xCopy;

	private bool isAbortRequested;

	public bool IsAbortRequested => isAbortRequested;

	public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

	public event EventHandler<ItemProgressEventArgs> ItemProgressChanged;

	public event EventHandler<FileCopyCompletedEventArgs> Completed;

	public void Copy(string source, string destination)
	{
		Copy(source, destination, this.ProgressChanged, this.Completed);
	}

	public void CopyFiles(IEnumerable<string> sources, string destinationFolder)
	{
		IEnumerable<string> destinations = sources.Select((string s) => Path.Combine(destinationFolder, Path.GetFileName(s))).ToArray();
		CopyFiles(sources, destinations);
	}

	public void CopyFiles(IEnumerable<string> sources, IEnumerable<string> destinations)
	{
		CopyFiles(sources, destinations, this.ProgressChanged, this.ItemProgressChanged, this.Completed);
	}

	public void CopyFileGroups(IEnumerable<IEnumerable<string>> sourceGroups, string destinationFolder)
	{
		IEnumerable<IEnumerable<string>> destinationGroups = sourceGroups.Select((IEnumerable<string> s) => s.Select((string ss) => Path.Combine(destinationFolder, Path.GetFileName(ss))).ToArray()).ToArray();
		CopyFileGroups(sourceGroups, destinationGroups, this.ProgressChanged, this.ItemProgressChanged, this.Completed);
	}

	public void CopyFileGroups(IEnumerable<IEnumerable<string>> sourceGroups, IEnumerable<IEnumerable<string>> destinationGroups)
	{
		CopyFileGroups(sourceGroups, destinationGroups, this.ProgressChanged, this.ItemProgressChanged, this.Completed);
	}

	public void Abort()
	{
		if (xCopy != null)
		{
			isAbortRequested = true;
			xCopy.Abort();
		}
	}

	private void Copy(string source, string destination, EventHandler<ProgressChangedEventArgs> progressHandler, EventHandler<FileCopyCompletedEventArgs> completedHandler)
	{
		if (progressHandler != null)
		{
			if (CheckAccess())
			{
				executeProgressHandler();
			}
			else
			{
				base.Dispatcher.Invoke(executeProgressHandler);
			}
		}
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate
		{
			FileCopyAction action = FileCopyAction.Abort;
			xCopy = new XCopy();
			xCopy.ProgressChanged += delegate(object ps, ProgressChangedEventArgs pe)
			{
				if (progressHandler != null)
				{
					if (CheckAccess())
					{
						executeProgressHandler();
					}
					else
					{
						base.Dispatcher.Invoke(executeProgressHandler);
					}
				}
				void executeProgressHandler()
				{
					progressHandler(this, pe);
				}
			};
			xCopy.Completed += delegate(object cs, FileCopyCompletedEventArgs ce)
			{
				if (completedHandler != null)
				{
					if (CheckAccess())
					{
						executedCompletedHandler();
					}
					else
					{
						base.Dispatcher.Invoke(executedCompletedHandler);
					}
				}
				action = ce.UserAction;
				void executedCompletedHandler()
				{
					completedHandler(this, ce);
				}
			};
			while (!xCopy.Copy(source, destination) && action == FileCopyAction.Retry)
			{
			}
		};
		backgroundWorker.RunWorkerAsync();
		void executeProgressHandler()
		{
			progressHandler(null, new ProgressChangedEventArgs(0, source));
		}
	}

	public void CopyFiles(IEnumerable<string> sources, IEnumerable<string> destinations, EventHandler<ProgressChangedEventArgs> progressHandler, EventHandler<ItemProgressEventArgs> itemProgressHandler, EventHandler<FileCopyCompletedEventArgs> completedHandler)
	{
		int sourceCount = ((sources != null) ? sources.Count() : 0);
		string firstFile = sources.FirstOrDefault();
		if (progressHandler != null || itemProgressHandler != null)
		{
			if (CheckAccess())
			{
				executeMultiProgressHandler();
			}
			else
			{
				base.Dispatcher.Invoke(executeMultiProgressHandler);
			}
		}
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate
		{
			IEnumerable<long> enumerable = null;
			string currentFile = null;
			enumerable = sources.Select(delegate(string ss)
			{
				currentFile = ss;
				FileInfo fileInfo;
				try
				{
					fileInfo = new FileInfo(ss);
				}
				catch
				{
					return -1L;
				}
				return fileInfo.Length;
			}).ToArray();
			CopyFilesBlocking(sources, destinations, enumerable, progressHandler, itemProgressHandler, completedHandler);
		};
		backgroundWorker.RunWorkerAsync();
		void executeMultiProgressHandler()
		{
			progressHandler?.Invoke(null, new ProgressChangedEventArgs(0, sourceCount));
			itemProgressHandler?.Invoke(null, new ItemProgressEventArgs(1, sourceCount, firstFile));
		}
	}

	public void CopyFileGroups(IEnumerable<IEnumerable<string>> sourceGroups, IEnumerable<IEnumerable<string>> destinationGroups, EventHandler<ProgressChangedEventArgs> progressHandler, EventHandler<ItemProgressEventArgs> itemProgressHandler, EventHandler<FileCopyCompletedEventArgs> completedHandler)
	{
		int sourceGroupCount = ((sourceGroups != null) ? sourceGroups.Count() : 0);
		IEnumerable<string> firstGroup = sourceGroups.FirstOrDefault();
		string firstFile = ((firstGroup != null && firstGroup.Any()) ? firstGroup.First() : null);
		if (progressHandler != null || itemProgressHandler != null)
		{
			if (CheckAccess())
			{
				executeMultiProgressHandler();
			}
			else
			{
				base.Dispatcher.Invoke(executeMultiProgressHandler);
			}
		}
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork += delegate
		{
			IEnumerable<IEnumerable<long>> enumerable = null;
			string currentFile = null;
			enumerable = sourceGroups.Select((IEnumerable<string> ss) => ss.Select(delegate(string sss)
			{
				currentFile = sss;
				try
				{
					return new FileInfo(sss).Length;
				}
				catch (FileNotFoundException)
				{
					return -1L;
				}
			}).ToArray()).ToArray();
			long totalLength = enumerable.Select((IEnumerable<long> ss) => ss.Select((long l) => (l < 0) ? 0 : l).Sum()).Sum();
			long totalTransferredBytes = 0L;
			long currentGroupLength = 0L;
			int groupCount = sourceGroups.Count();
			string lastFile = sourceGroups.Last().Last();
			int oldPercentage = 0;
			bool finished = false;
			FileCopyAction action = FileCopyAction.Ignore;
			for (int i = 0; i < groupCount; i++)
			{
				IEnumerable<string> sourceGroup = sourceGroups.ElementAt(i);
				IEnumerable<string> destinations = destinationGroups.ElementAt(i);
				IEnumerable<long> enumerable2 = enumerable.ElementAt(i);
				if (i > 0 && itemProgressHandler != null)
				{
					Action action2 = delegate
					{
						itemProgressHandler(this, new ItemProgressEventArgs(i + 1, groupCount, sourceGroup));
					};
					if (CheckAccess())
					{
						action2();
					}
					else
					{
						base.Dispatcher.Invoke(action2);
					}
				}
				currentGroupLength = enumerable2.Select((long l) => (l < 0) ? 0 : l).Sum();
				CopyFilesBlocking(sourceGroup, destinations, enumerable2, singleGroupProgressHandler, null, singleGroupCompletedHandler);
				if (finished || action == FileCopyAction.Abort)
				{
					break;
				}
				totalTransferredBytes += currentGroupLength;
			}
			void singleGroupCompletedHandler(object cs, FileCopyCompletedEventArgs ce)
			{
				if (ce.Error != null || ce.Cancelled || ce.FileName == lastFile)
				{
					if (ce.Cancelled)
					{
						finished = true;
					}
					if (completedHandler != null)
					{
						if (CheckAccess())
						{
							executeCompletedHandler();
						}
						else
						{
							base.Dispatcher.Invoke(executeCompletedHandler);
						}
					}
					action = ce.UserAction;
				}
				void executeCompletedHandler()
				{
					completedHandler(this, ce);
				}
			}
			void singleGroupProgressHandler(object ss, ProgressChangedEventArgs se)
			{
				long num = totalTransferredBytes + se.ProgressPercentage * currentGroupLength / 100;
				int percentage = (int)(100 * num / totalLength);
				if (percentage > oldPercentage)
				{
					if (progressHandler != null)
					{
						if (CheckAccess())
						{
							executeProgressHandler();
						}
						else
						{
							base.Dispatcher.Invoke(executeProgressHandler);
						}
					}
					oldPercentage = percentage;
				}
				void executeProgressHandler()
				{
					progressHandler(this, new ProgressChangedEventArgs(percentage, se.UserState));
				}
			}
		};
		backgroundWorker.RunWorkerAsync();
		void executeMultiProgressHandler()
		{
			progressHandler?.Invoke(null, new ProgressChangedEventArgs(0, firstFile));
			itemProgressHandler?.Invoke(null, new ItemProgressEventArgs(1, sourceGroupCount, firstGroup));
		}
	}

	private void CopyFilesBlocking(IEnumerable<string> sources, IEnumerable<string> destinations, IEnumerable<long> fileLengths, EventHandler<ProgressChangedEventArgs> progressHandler, EventHandler<ItemProgressEventArgs> itemProgressHandler, EventHandler<FileCopyCompletedEventArgs> completedHandler)
	{
		long totalLength = fileLengths.Sum();
		long totalTransferredBytes = 0L;
		long currentFileLength = 0L;
		int fileCount = sources.Count();
		string lastFile = sources.Last();
		int oldPercentage = 0;
		FileCopyAction action = FileCopyAction.Ignore;
		bool finished = false;
		xCopy = new XCopy();
		xCopy.ProgressChanged += delegate(object ss, ProgressChangedEventArgs se)
		{
			long num = totalTransferredBytes + se.ProgressPercentage * currentFileLength / 100;
			int percentage = (int)(100 * num / totalLength);
			if (percentage > oldPercentage)
			{
				if (progressHandler != null)
				{
					if (CheckAccess())
					{
						executeProgressHandler();
					}
					else
					{
						base.Dispatcher.Invoke(executeProgressHandler);
					}
				}
				oldPercentage = percentage;
			}
			void executeProgressHandler()
			{
				progressHandler(this, new ProgressChangedEventArgs(percentage, se.UserState));
			}
		};
		xCopy.Completed += delegate(object cs, FileCopyCompletedEventArgs ce)
		{
			if (ce.Error != null || ce.Cancelled || ce.FileName == lastFile)
			{
				if (ce.Cancelled)
				{
					finished = true;
				}
				if (completedHandler != null)
				{
					if (CheckAccess())
					{
						executeCompletedHandler();
					}
					else
					{
						base.Dispatcher.Invoke(executeCompletedHandler);
					}
				}
				action = ce.UserAction;
			}
			void executeCompletedHandler()
			{
				completedHandler(this, ce);
			}
		};
		for (int i = 0; i < fileCount; i++)
		{
			string source = sources.ElementAt(i);
			string destination = destinations.ElementAt(i);
			currentFileLength = fileLengths.ElementAt(i);
			if (currentFileLength < 0)
			{
				currentFileLength = 0L;
			}
			if (i > 0 && itemProgressHandler != null)
			{
				if (CheckAccess())
				{
					executeItemProgressHandler();
				}
				else
				{
					base.Dispatcher.Invoke(executeItemProgressHandler);
				}
			}
			bool flag;
			do
			{
				flag = xCopy.Copy(source, destination);
			}
			while (!flag && action == FileCopyAction.Retry);
			if (flag || action == FileCopyAction.Ignore)
			{
				totalTransferredBytes += currentFileLength;
			}
			if (finished || action == FileCopyAction.Abort)
			{
				break;
			}
			void executeItemProgressHandler()
			{
				itemProgressHandler(this, new ItemProgressEventArgs(i + 1, fileCount, source));
			}
		}
	}
}
