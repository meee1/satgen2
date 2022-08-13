using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Racelogic.DataTypes;
using Racelogic.Utilities.Sntp;

namespace Racelogic.Utilities;

[DefaultEvent("QueryServerCompleted")]
[DefaultProperty("RemoteSNTPServer")]
public class SntpClient : Component
{
	private delegate void WorkerThreadStartDelegate();

	private int _Timeout;

	private AsyncOperation asyncOperation;

	private readonly SendOrPostCallback operationCompleted;

	private readonly WorkerThreadStartDelegate threadStart;

	public static readonly RemoteSntpServer DefaultServer = RemoteSntpServer.Default;

	public const int DefaultTimeout = 5000;

	public const SntpVersionNumber DefaultVersionNumber = SntpVersionNumber.Version3;

	private const int lockTimeout = 10000;

	private readonly BasicLock queryServerLock = new BasicLock("SNTPQueryServerLock", 10000);

	[Browsable(false)]
	public bool IsBusy { get; private set; }

	public static DateTime Now => GetNow();

	[Description("The server to use.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Category("Connection")]
	public RemoteSntpServer RemoteSntpServer { get; set; }

	[Description("The timeout in milliseconds used for sending and receiving.")]
	[DefaultValue(5000)]
	[Category("Connection")]
	public int Timeout
	{
		get
		{
			return _Timeout;
		}
		set
		{
			if (value < -1)
			{
				value = 5000;
			}
			_Timeout = value;
		}
	}

	[Description("Whether to update the local date and time to the date and time calculated by querying the server.")]
	[DefaultValue(true)]
	[Category("Actions")]
	public bool UpdateLocalDateTime { get; set; }

	[Description("The NTP/SNTP version to use.")]
	[DefaultValue(SntpVersionNumber.Version3)]
	[Category("Connection")]
	public SntpVersionNumber VersionNumber { get; set; }

	[Description("Raised when a query to the server completes successfully.")]
	[Category("Success")]
	public event EventHandler<QueryServerCompletedEventArgs> QueryServerCompleted;

	public SntpClient()
	{
		Initialize();
		threadStart = WorkerThreadStart;
		operationCompleted = AsyncOperationCompleted;
		Timeout = 5000;
		VersionNumber = SntpVersionNumber.Version3;
		UpdateLocalDateTime = true;
	}

	public static TimeSpan GetCurrentLocalTimeZoneOffset()
	{
		return TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow);
	}

	public static DateTime GetNow()
	{
		return GetNow(RemoteSntpServer.Default, 500);
	}

	public static DateTime GetNow(RemoteSntpServer remoteSntpServer)
	{
		return GetNow(remoteSntpServer, 500);
	}

	public static DateTime GetNow(int timeout)
	{
		return GetNow(RemoteSntpServer.Default, timeout);
	}

	public static DateTime GetNow(RemoteSntpServer remoteSNTPServer, int timeout)
	{
		QueryServerCompletedEventArgs queryServerCompletedEventArgs = new SntpClient
		{
			UpdateLocalDateTime = false,
			RemoteSntpServer = remoteSNTPServer,
			Timeout = timeout
		}.QueryServer();
		if (queryServerCompletedEventArgs.Succeeded)
		{
			return DateTime.Now.AddSeconds(queryServerCompletedEventArgs.Data.LocalClockOffset);
		}
		return DateTime.MinValue;
	}

	public bool QueryServerAsync()
	{
		bool result = false;
		if (!IsBusy)
		{
			IsBusy = true;
			asyncOperation = AsyncOperationManager.CreateOperation(null);
			threadStart.BeginInvoke(null, null);
			result = true;
		}
		return result;
	}

	protected virtual void OnQueryServerCompleted(QueryServerCompletedEventArgs e)
	{
		this.QueryServerCompleted?.Invoke(this, e);
	}

	private void AsyncOperationCompleted(object arg)
	{
		IsBusy = false;
		OnQueryServerCompleted((QueryServerCompletedEventArgs)arg);
	}

	private void Initialize()
	{
		if (RemoteSntpServer == null)
		{
			RemoteSntpServer = DefaultServer;
		}
	}

	public QueryServerCompletedEventArgs QueryServer()
	{
		QueryServerCompletedEventArgs queryServerCompletedEventArgs = new QueryServerCompletedEventArgs();
		Initialize();
		UdpClient udpClient = null;
		try
		{
			udpClient = new UdpClient();
			IPEndPoint remoteEP = RemoteSntpServer.GetIPEndPoint();
			udpClient.Client.SendTimeout = Timeout;
			udpClient.Client.ReceiveTimeout = Timeout;
			udpClient.Connect(remoteEP);
			SntpData clientRequestPacket = SntpData.GetClientRequestPacket(VersionNumber);
			udpClient.Send(clientRequestPacket, clientRequestPacket.Length);
			queryServerCompletedEventArgs.Data = udpClient.Receive(ref remoteEP);
			queryServerCompletedEventArgs.Data.DestinationDateTime = DateTime.UtcNow;
			if (queryServerCompletedEventArgs.Data.Mode == SntpMode.Server)
			{
				queryServerCompletedEventArgs.Succeeded = true;
				if (UpdateLocalDateTime)
				{
					UpdateTime(queryServerCompletedEventArgs.Data.LocalClockOffset);
					queryServerCompletedEventArgs.LocalDateTimeUpdated = true;
				}
			}
			else
			{
				queryServerCompletedEventArgs.ErrorData = new ErrorData("The response from the server was invalid.");
			}
			return queryServerCompletedEventArgs;
		}
		catch (Exception exception)
		{
			queryServerCompletedEventArgs.ErrorData = new ErrorData(exception);
			return queryServerCompletedEventArgs;
		}
		finally
		{
			udpClient?.Close();
		}
	}

	private void UpdateTime(double localClockOffset)
	{
		DateTime dateTime = DateTime.Now.AddSeconds(localClockOffset);
		SYSTEMTIME lpSystemTime = default(SYSTEMTIME);
		lpSystemTime.wYear = (ushort)dateTime.Year;
		lpSystemTime.wMonth = (ushort)dateTime.Month;
		lpSystemTime.wDayOfWeek = (ushort)dateTime.DayOfWeek;
		lpSystemTime.wDay = (ushort)dateTime.Day;
		lpSystemTime.wHour = (ushort)dateTime.Hour;
		lpSystemTime.wMinute = (ushort)dateTime.Minute;
		lpSystemTime.wSecond = (ushort)dateTime.Second;
		lpSystemTime.wMilliseconds = (ushort)dateTime.Millisecond;
		if (!NativeMethods.SetLocalTime(ref lpSystemTime))
		{
			throw new Win32Exception("SetLocalTime(..) failure");
		}
	}

	private void WorkerThreadStart()
	{
		using (queryServerLock.Lock())
		{
			QueryServerCompletedEventArgs queryServerCompletedEventArgs = null;
			try
			{
				queryServerCompletedEventArgs = QueryServer();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			asyncOperation.PostOperationCompleted(operationCompleted, queryServerCompletedEventArgs);
		}
	}
}
