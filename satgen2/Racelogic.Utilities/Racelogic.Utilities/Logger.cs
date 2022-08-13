using System;
using System.Diagnostics;

namespace Racelogic.Utilities;

[Obsolete("I know what you're thinking: you want a testable version of Logger for your unit tests? Use the RLLogger class and call the GetLogger method instead.")]
public static class Logger
{
	private static readonly IRLLogger _instance = RLLogger.GetLogger();

	public static string LogFileName => _instance.LogFileName;

	public static bool AutoFlush
	{
		[DebuggerStepThrough]
		get
		{
			return _instance.AutoFlush;
		}
		[DebuggerStepThrough]
		set
		{
			_instance.AutoFlush = value;
		}
	}

	public static void Initialize()
	{
		_instance.Initialize(string.Empty);
	}

	public static void Initialize(int maxFileSize)
	{
		_instance.Initialize(string.Empty, useLocalAppDataFolder: false, maxFileSize);
	}

	public static void Initialize(string fileName, bool useLocalAppDataFolder = false, int maxFileSize = 100)
	{
		_instance.Initialize(fileName, useLocalAppDataFolder, maxFileSize);
	}

	public static void LogMessage(string message)
	{
		_instance.LogMessage(message);
	}

	public static void LogShortMessage(string message)
	{
		_instance.LogShortMessage(message);
	}

	public static void LogUnhandledExceptions()
	{
		_instance.LogUnhandledExceptions();
	}

	public static void LogException(Exception ex)
	{
		_instance.LogException(ex);
	}
}
