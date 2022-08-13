using System;

namespace Racelogic.Utilities;

public interface IRLLogger
{
	bool AutoFlush { get; set; }

	string LogFileName { get; }

	void Initialize();

	void Initialize(int maxFileSize);

	void Initialize(string fileName, bool useLocalAppDataFolder = false, int maxFileSize = 100);

	void LogException(Exception ex, bool handled = false);

	void LogMessage(string message);

	void LogShortMessage(string message);

	void LogUnhandledExceptions();

	void AddProvider(LogProviderBase logProviderBase);
}
