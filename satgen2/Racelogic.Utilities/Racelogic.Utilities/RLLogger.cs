#define TRACE
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Racelogic.DataTypes;

namespace Racelogic.Utilities;

public class RLLogger : IRLLogger
{
	protected static IRLLogger _instance;

	private LogProviderBase _logProviderBase;

	private bool initialized;

	private bool logFileInitialized;

	private string logFileName;

	private int fileSizeLimitKB;

	private DateTime startTime;

	private readonly Stopwatch stopwatch = new Stopwatch();

	private bool logged;

	private readonly string strongSeparator = "============================";

	private const long kiloByte = 1024L;

	private readonly CultureInfo defaultCulture = new CultureInfo("en-GB");

	private readonly BasicLock writeLock = new BasicLock("Logger lock", 5000);

	public string LogFileName
	{
		[DebuggerStepThrough]
		get
		{
			return logFileName;
		}
	}

	public bool AutoFlush
	{
		[DebuggerStepThrough]
		get
		{
			return Trace.AutoFlush;
		}
		[DebuggerStepThrough]
		set
		{
			Trace.AutoFlush = value;
		}
	}

	public static IRLLogger GetLogger()
	{
		return _instance ?? (_instance = new RLLogger());
	}

	protected RLLogger()
	{
	}

	public void Initialize()
	{
		Initialize(string.Empty);
	}

	public void Initialize(int maxFileSize)
	{
		Initialize(string.Empty, useLocalAppDataFolder: false, maxFileSize);
	}

	public void Initialize(string fileName, bool useLocalAppDataFolder = false, int maxFileSize = 100)
	{
		logFileName = GetLogFileName(fileName, useLocalAppDataFolder);
		fileSizeLimitKB = maxFileSize;
		InitializeClock();
		AppDomain.CurrentDomain.ProcessExit -= new EventHandler(OnProcessExit);
		AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
		initialized = true;
	}

	private void UpdateLogFileNameIfOriginalFileIsAlreadyInUse()
	{
		int num = 2;
		string path = logFileName;
		bool flag;
		do
		{
			try
			{
				flag = false;
				if (File.Exists(logFileName))
				{
					File.ReadAllLines(logFileName);
				}
			}
			catch (IOException)
			{
				flag = true;
				logFileName = Path.Combine(Path.GetDirectoryName(path), Path.ChangeExtension(Path.GetFileNameWithoutExtension(path) + num, ".log"));
				num++;
			}
		}
		while (flag);
	}

	internal void InitializeFile()
	{
		if (File.Exists(logFileName))
		{
			UpdateLogFileNameIfOriginalFileIsAlreadyInUse();
			if (File.Exists(logFileName) && fileSizeLimitKB > 0 && new FileInfo(logFileName).Length > (long)fileSizeLimitKB * 1024L)
			{
				string[] array = File.ReadAllLines(logFileName);
				File.WriteAllLines(logFileName, array.Skip(array.Length / 2).ToArray());
			}
		}
		TextWriterTraceListener listener = new TextWriterTraceListener(logFileName);
		Trace.Listeners.Add(listener);
		Trace.AutoFlush = true;
		logFileInitialized = true;
	}

	private void CheckInitialized()
	{
		if (!initialized)
		{
			Initialize();
		}
		if (!logFileInitialized)
		{
			InitializeFile();
		}
	}

	public void AddProvider(LogProviderBase logProviderBase)
	{
		_logProviderBase = logProviderBase;
		System.Timers.Timer timer = new System.Timers.Timer(327680.0);
		timer.AutoReset = false;
		timer.Enabled = true;
		timer.Elapsed += UploadTimerElapsed;
		timer.Start();
		void UploadTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (sender is System.Timers.Timer timer2)
			{
				timer2.Stop();
				timer2.Elapsed -= UploadTimerElapsed;
				timer2.Dispose();
				_logProviderBase?.Upload();
			}
		}
	}

	private string GetLogFileName(string fileName, bool useLocalAppDataFolder)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		string extension = Path.GetExtension(fileName);
		string path;
		string text;
		if (!string.IsNullOrWhiteSpace(fileNameWithoutExtension))
		{
			if (string.IsNullOrWhiteSpace(extension) || (!(extension.ToUpperInvariant() == ".EXE") && !(extension.ToUpperInvariant() == ".DLL")))
			{
				path = ((!string.IsNullOrWhiteSpace(extension)) ? Path.GetFileName(fileName) : Path.ChangeExtension(fileNameWithoutExtension, ".log"));
				text = ((Path.GetFileName(fileName) != fileName) ? Path.GetDirectoryName(Path.GetFullPath(fileName)) : ((!useLocalAppDataFolder) ? Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Racelogic", fileNameWithoutExtension)));
			}
			else
			{
				path = Path.ChangeExtension(fileNameWithoutExtension, ".log");
				text = ((!useLocalAppDataFolder) ? Path.GetDirectoryName(Path.GetFullPath(fileName)) : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Racelogic", fileNameWithoutExtension));
			}
		}
		else
		{
			fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location);
			path = Path.ChangeExtension(fileNameWithoutExtension, ".log");
			text = ((!useLocalAppDataFolder) ? Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Racelogic", fileNameWithoutExtension));
		}
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return Path.Combine(text, path);
	}

	public virtual void LogMessage(string message)
	{
		CheckInitialized();
		using (writeLock.Lock())
		{
			if (!logged)
			{
				WriteApplicationHeader();
			}
			WriteTimestamp();
			Trace.WriteLine(message);
		}
	}

	public virtual void LogShortMessage(string message)
	{
		CheckInitialized();
		using (writeLock.Lock())
		{
			if (!logged)
			{
				WriteApplicationHeader();
			}
			Trace.WriteLine(message);
		}
	}

	public void LogUnhandledExceptions()
	{
		AppDomain.CurrentDomain.UnhandledException -= new UnhandledExceptionEventHandler(OnCurrentDomainUnhandledException);
		AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnCurrentDomainUnhandledException);
		TaskScheduler.UnobservedTaskException -= new EventHandler<UnobservedTaskExceptionEventArgs>(OnUnobservedTaskException);
		TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(OnUnobservedTaskException);
	}

	private void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
	{
		LogException(e.Exception);
	}

	protected virtual void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		CheckInitialized();
		using (writeLock.Lock())
		{
			if (!logged)
			{
				WriteApplicationHeader();
			}
			WriteTimestamp();
			Trace.WriteLine("Unhandled exception:");
			LogException(e.ExceptionObject as Exception);
			Trace.WriteLine(null);
		}
	}

	private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
	{
		CheckInitialized();
		using (writeLock.Lock())
		{
			Trace.WriteLine("Unobserved task exception:");
			LogException(e.Exception!.Flatten());
			Trace.WriteLine(null);
		}
	}

	public virtual void LogException(Exception ex, bool handled = false)
	{
		if (ex == null)
		{
			return;
		}
		_logProviderBase?.LogException(ex);
		CheckInitialized();
		using (writeLock.Lock())
		{
			if (!logged)
			{
				WriteApplicationHeader();
			}
			WriteTimestamp();
			Trace.WriteLine(handled ? ("Handled " + ex.GetType().FullName) : ex.GetType().FullName);
			Trace.Indent();
			Trace.WriteLine(ex.Message);
			Trace.WriteLine(string.Empty);
			if (!string.IsNullOrWhiteSpace(ex.StackTrace))
			{
				Trace.WriteLine("Stack trace:");
				Trace.WriteLine(ex.StackTrace);
				Trace.WriteLine(string.Empty);
			}
			if (ex.InnerException != null)
			{
				Trace.WriteLine("Inner exception:");
				Trace.Indent();
				LogException(ex.InnerException);
				Trace.Unindent();
			}
			if (ex is ReflectionTypeLoadException ex2)
			{
				Trace.WriteLine(string.Empty);
				Trace.WriteLine("Loader exceptions:");
				Trace.Indent();
				Exception[] loaderExceptions = ex2.LoaderExceptions;
				foreach (Exception ex3 in loaderExceptions)
				{
					LogException(ex3);
				}
				Trace.Unindent();
			}
			Trace.Unindent();
		}
	}

	private void WriteApplicationHeader()
	{
		using (writeLock.Lock())
		{
			Trace.WriteLine(null);
			Trace.WriteLine(strongSeparator);
			Trace.WriteLine("Application started");
			Trace.WriteLine(startTime.ToString("d", defaultCulture) + "  " + startTime.ToString("HH:mm:ss.fff", defaultCulture) + " UTC");
			Trace.WriteLine(strongSeparator);
			logged = true;
		}
	}

	private void WriteTimestamp()
	{
		using (writeLock.Lock())
		{
			DateTime currentTime = GetCurrentTime();
			string text = currentTime.ToString("HH:mm:ss.ffffff", defaultCulture) + " UTC";
			string text2 = (currentTime - startTime).TotalSeconds.ToString("F6", defaultCulture);
			string text3 = Thread.CurrentThread.Name;
			if (!string.IsNullOrWhiteSpace(text3))
			{
				text3 = "  [" + text3 + "]";
			}
			Trace.WriteLine(null);
			Trace.WriteLine(text + "  (+" + text2 + ")" + text3);
		}
	}

	private void WriteApplicationFooter()
	{
		using (writeLock.Lock())
		{
			if (logged)
			{
				DateTime currentTime = GetCurrentTime();
				string text = currentTime.ToString("d", defaultCulture) + "  " + currentTime.ToString("HH:mm:ss.fff", defaultCulture) + " UTC";
				string text2 = (currentTime - startTime).TotalSeconds.ToString("F6", defaultCulture);
				Trace.WriteLine(null);
				Trace.WriteLine(strongSeparator);
				Trace.WriteLine("Application closed");
				Trace.WriteLine(text + "  (+" + text2 + ")");
				Trace.WriteLine(strongSeparator);
				Trace.WriteLine(null);
				Trace.Flush();
			}
		}
	}

	private void OnProcessExit(object sender, EventArgs e)
	{
		CheckInitialized();
		WriteApplicationFooter();
	}

	private void InitializeClock()
	{
		DateTime utcNow = DateTime.UtcNow;
		DateTime utcNow2;
		do
		{
			utcNow2 = DateTime.UtcNow;
		}
		while (utcNow2 == utcNow);
		stopwatch.Restart();
		startTime = utcNow2;
	}

	private DateTime GetCurrentTime()
	{
		return startTime + stopwatch.Elapsed;
	}
}
