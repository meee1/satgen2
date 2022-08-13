using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using GalaSoft.MvvmLight.Messaging;
using Racelogic.Comms.Serial.Properties;
using Racelogic.Core;
using Racelogic.Core.Win;
using Racelogic.DataSource;
using Racelogic.DataSource.Nmea;
using Racelogic.DataTypes.Win;
using Racelogic.FileRoutines;
using Racelogic.Utilities;
using Racelogic.WPF.Utilities;
using ZylSoft.Serial;

namespace Racelogic.Comms.Serial;

public class ComPort : BasePropertyChanged, IDisposable
{
	protected class DelayTimer : System.Timers.Timer
	{
		private bool isRunning;

		public bool IsRunning
		{
			get
			{
				return isRunning;
			}
			private set
			{
				isRunning = value;
			}
		}

		public DelayTimer(int period)
			: base(period)
		{
			base.Elapsed += delegate
			{
				base.Enabled = false;
				IsRunning = false;
			};
			IsRunning = true;
			base.Enabled = true;
		}

		public void Delay(int period)
		{
			DelayTimer delayTimer = new DelayTimer(period);
			while (delayTimer.IsRunning)
			{
				Racelogic.Core.Win.Helper.WaitForPriority();
			}
		}
	}

	private enum CanChannelInformationStatus
	{
		Idle,
		AwaitingResponse,
		Received,
		Requested,
		Updated
	}

	internal struct MessageSent
	{
		internal ushort Id;

		internal byte Command;

		internal byte SubCommand;

		internal byte[] Payload;

		internal MessageSent(ushort id, byte command, byte subCommand, byte[] payload)
		{
			Id = id;
			Command = command;
			SubCommand = subCommand;
			Payload = new byte[payload.Length];
			payload.CopyTo(Payload, 0);
		}
	}

	private enum FileTransferType
	{
		Config,
		ScreenShot
	}

	public enum EncryptionType
	{
		None,
		TripleDes
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 20)]
	internal struct FlashInfoDefinition
	{
		internal uint FlashBase;

		internal uint FlashSize;

		internal uint MaxFlashBanks;

		internal uint MaxFlashSectors;

		internal uint FlashSectorSize;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 192)]
	private struct ModulesFound
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		internal CanModulesDefinition[] CanModules;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 832)]
	private struct ChannelsFound
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
		internal CanModuleConfigurationDefinition[] CanModuleConfiguration;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 43)]
	private struct AnalogueDefinition
	{
		private double CalibrationScale;

		private double CalibrationOffset;

		public float Scale;

		public float Offset;

		public StringTenBytes Name;

		public StringNineBytes Units;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 172)]
	private struct InternalA2DChannels
	{
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		internal AnalogueDefinition[] InternalAnalogue;
	}

	public enum BlockSizes
	{
		TwoHundredFiftySixB = 0x100,
		FourKB = 0x1000,
		SixteenKB = 0x4000,
		ThirtyTwoKB = 0x8000,
		SixtyFourKB = 0x10000
	}

	[Flags]
	private enum OldProtocolState
	{
		Idle = 0,
		MessageSent = 1,
		ReplyReceived = 2,
		StopGps = 4
	}

	private bool isReceivingComms = false;

	private ManagementEventWatcher _watcher;

	private System.Timers.Timer _watcherTimer;

	private double percentComplete;

	private string progressText;

	private bool isReceivingVBoxComms = false;

	private Action<List<CanChannel>, List<CanChannel>> UpdateCanDataChannelsAction;

	private Action<string, string, double?> DisplaySlidingMessageAction;

	private bool PortWasOpen = false;

	private const int NoCommsTimeout = 5000;

	private const int UnitDisconnectedTimeout = 5000;

	private const int IsReceivingCommsTimeout = 1000;

	private int VboLoggerBytesWritten = 0;

	private bool isloggingRequired = false;

	private uint replayCount = 0u;

	public PortSettings Settings;

	private volatile bool ParsingRequired = false;

	private ObservableCollection<AvailablePort> availablePorts = new ObservableCollection<AvailablePort>();

	private bool isRefreshingPortsList = false;

	private bool isRefreshingPortDetails = false;

	private ICommand getSerialPorts;

	private readonly SyncLock GetPortsLock = new SyncLock("GetPortsLock", 11000);

	private readonly SyncLock ReturnedDataLock = new SyncLock("ReturnedDataLock", 11000);

	private object CanDataChannelsLock = new object();

	private Header HeaderFound = Header.None;

	private string CurrentHeader = string.Empty;

	private VBoxType vBoxType;

	private VBoxType DetectedUnit = VBoxType.Unknown;

	private double gpsLatency;

	private static string[] HeaderList = new string[38]
	{
		"$NEWCAN", "$VBOX3i", "$VBOXII", "$VBOX3$", "$VB2SX$", "$VB2100", "$VBOXM$", "$VB2SX2", "$VB2SL$", "$VBSX10",
		"$VB3TR2", "$VB3TR3", "$VBOXV$", "$VBMICR", "$VBBTST", "$VBSPT$", "$VB3is$", "$NEWPOS", "$RLCMD$", "$RLRSP$",
		"$GPGGA", "$GPVTG", "$GPZDA", "$GPGLL", "$GPGSA", "$GPRMC", "$GPGSV", "$GPGST", "$GPTXT", "$GNGGA",
		"$GNVTG", "$GNZDA", "$GNGLL", "$GNGSA", "$GNRMC", "$GNGSV", "$GNGST", "$GNTXT"
	};

	private bool requestingCanChannelInformation = false;

	private List<MessageSent> MessagesSent = new List<MessageSent>();

	private MessageSent ReceivedMessage;

	private object ReadIndexLock = new object();

	private object WriteIndexLock = new object();

	private object SetupReadIndexLock = new object();

	private object SetupWriteIndexLock = new object();

	private object UnrecognisedReadIndexLock = new object();

	private object UnrecognisedWriteIndexLock = new object();

	private object OnDataReceivedLock = new object();

	private object SetQuietLock = new object();

	private bool setQuietRequested = false;

	private volatile bool InSetup = false;

	private const int rxBufferSize = 131072;

	private byte[] receivedDataBuffer = new byte[131072];

	public byte[] setupReceivedDataBuffer = new byte[131072];

	public byte[] UnrecognisedReceivedDataBuffer = new byte[131072];

	private int unrecognisedReadIndex = 0;

	private int unrecognisedWriteIndex = 0;

	private int readIndex = 0;

	private int writeIndex = 0;

	private int setupReadIndex = 0;

	private int setupWriteIndex = 0;

	private List<byte> CrcString = new List<byte>();

	private double OldVBoxTime;

	private int maxRxCount = 0;

	private int crcError = 0;

	private AutoResetEvent ParserResetEvent = new AutoResetEvent(initialState: false);

	private ICommand closePort;

	private ICommand openPort;

	private string portNameTemp = string.Empty;

	private static bool CanChannelOpen = false;

	private byte[] CypherBlock = new byte[128]
	{
		7, 23, 28, 27, 115, 124, 54, 8, 51, 52,
		81, 83, 106, 95, 41, 50, 11, 31, 79, 9,
		107, 37, 53, 57, 65, 0, 98, 76, 45, 48,
		86, 15, 120, 22, 105, 58, 112, 66, 14, 91,
		116, 114, 97, 82, 46, 103, 33, 61, 59, 88,
		24, 110, 62, 94, 39, 111, 70, 32, 25, 85,
		1, 77, 125, 38, 40, 56, 118, 99, 78, 104,
		93, 2, 90, 55, 5, 96, 74, 89, 47, 21,
		30, 18, 4, 20, 35, 121, 113, 71, 19, 44,
		69, 16, 101, 108, 127, 29, 109, 123, 126, 122,
		49, 6, 68, 42, 60, 34, 63, 3, 80, 26,
		84, 102, 75, 36, 17, 72, 64, 13, 67, 119,
		117, 12, 92, 43, 87, 10, 73, 100
	};

	private byte[] _eeprom;

	private byte[] _flash;

	private ushort? _seed = null;

	private bool _locked = true;

	private ushort _protocolVersion = 0;

	private doubleUnion _responseResult = default(doubleUnion);

	private ushort _messageId = 0;

	private bool _responseOk = false;

	private bool _waitingForResponse = false;

	public const string CommandHeader = "$RLCMD$";

	private byte[] _responseData;

	private VideoVBoxFile TransferFile = null;

	private uint oldDotCount = 0u;

	private uint dotCount = 0u;

	private FileTransferType _fileTransferType;

	private bool _configurationOk = false;

	private uint _maxPayloadLength = 0u;

	private volatile bool _awaitingStmResponse = false;

	private List<StmCommands> AvailableStmCommands = new List<StmCommands>();

	private StrErrorDefinition _error = null;

	private volatile bool _awaitingStrResponse = false;

	private string _sentStrCommand = string.Empty;

	private bool _responseError = false;

	private uint _lastErrorCode = 0u;

	private TextWriter CommsDebugFile = null;

	private ICommand getCanChannelInformation;

	private ICommand getGpsLatency;

	private bool autoRetrieveInformation = false;

	private const byte _maxPayloadOldProtocol = 220;

	private const uint FlashBaseAddress = 4293394432u;

	private const uint FlashSize = 524288u;

	private bool _awaitingBootloaderResponse = false;

	private static readonly string VboxLiteStartGpsReply = new UTF7Encoding().GetString(new byte[10] { 181, 98, 5, 1, 2, 0, 6, 1, 15, 56 });

	public static readonly string VboxLiteEnableDisableDgpsReply = new UTF7Encoding().GetString(new byte[10] { 181, 98, 5, 1, 2, 0, 6, 22, 36, 77 });

	private static readonly byte[] _standardAck = new byte[4] { 255, 1, 19, 222 };

	private List<byte> _returnedDatafield = new List<byte>();

	private const int GpsCommand = 6;

	private const int VboxCommand = 7;

	private const int Gps2Command = 8;

	private const int TestCommand = 32;

	private OldProtocolState _OldProtocolState = OldProtocolState.Idle;

	private int? _requestedCommand;

	private int? _requestedVBOXSubCommand;

	private int _oldProtocolResponseLength = 0;

	private bool _crcError = false;

	private long txTime;

	private long rxTime;

	private bool _downloadingEEPROM = false;

	private bool _requestingVariableLengthResponse = false;

	private System.Timers.Timer NoCommsTimer { get; set; }

	private System.Timers.Timer IsReceivingCommsTimer { get; set; }

	private System.Timers.Timer UnitDisconnectedTimer { get; set; }

	private bool IsWPFApp => Application.Current != null;

	public VBoxData VBoxData { get; private set; }

	public CanData CanData { get; private set; }

	public NmeaData NmeaData { get; private set; }

	public string Name => Settings.PortName;

	public bool IsLoggingRequired
	{
		get
		{
			return isloggingRequired;
		}
		set
		{
			isloggingRequired = value;
			RaisePropertyChangedOnUi("IsLoggingRequired");
			if (value)
			{
				CreateLogger();
			}
			else if (VBoxCommsLogger != null)
			{
				VBoxCommsLogger.Flush();
				VBoxCommsLogger.Close();
				VBoxCommsLogger = null;
			}
		}
	}

	public SerialBaudRate BaudRate
	{
		get
		{
			return Settings.BaudRate;
		}
		set
		{
			Settings.BaudRate = value;
		}
	}

	public bool IsOpen => Settings.IsOpen;

	public bool IsReceivingVBoxComms
	{
		get
		{
			return isReceivingVBoxComms;
		}
		private set
		{
			if (isReceivingVBoxComms != value && (!value || (value && !InSetup)))
			{
				isReceivingVBoxComms = value;
				RaisePropertyChangedOnUi("IsReceivingVBoxComms");
			}
		}
	}

	public bool IsReceivingComms
	{
		get
		{
			return isReceivingComms;
		}
		private set
		{
			if (isReceivingComms != value)
			{
				isReceivingComms = value;
				RaisePropertyChangedOnUi("IsReceivingComms");
			}
		}
	}

	public double PercentComplete
	{
		get
		{
			return percentComplete;
		}
		set
		{
			if (percentComplete != value)
			{
				percentComplete = value;
				RaisePropertyChanged("PercentComplete");
			}
		}
	}

	public string ProgressText
	{
		get
		{
			return progressText;
		}
		set
		{
			if (progressText != value)
			{
				progressText = value;
				RaisePropertyChanged("ProgressText");
			}
		}
	}

	public bool HasFrameError { get; private set; }

	public UserSettings UserSettings { get; private set; }

	public bool IsReplayingLoggedData => replayCount != 0;

	private uint ReplayCount
	{
		get
		{
			lock (ReplayDataLock)
			{
				return replayCount;
			}
		}
		set
		{
			lock (ReplayDataLock)
			{
				replayCount = value;
			}
			RaisePropertyChangedOnUi("IsReplayingLoggedData");
			CommandManagerInvalidateOnUi();
		}
	}

	private Thread parserThread { get; set; }

	public bool IsInSimulatorMode { get; private set; }

	private bool _IsReceivingComms
	{
		set
		{
			if (!InSetup)
			{
				if (value)
				{
					IsReceivingComms = value;
					HasFrameError = false;
				}
				else if (IsReceivingCommsTimer != null && !IsReceivingCommsTimer.Enabled)
				{
					IsReceivingCommsTimer.Interval = 1000.0;
					IsReceivingCommsTimer.Start();
				}
			}
		}
	}

	private BinaryWriter VBoxCommsLogger { get; set; }

	private string VBoxCommsLogFileName { get; set; }

	public ObservableCollection<AvailablePort> ComPorts => new ObservableCollection<AvailablePort>(availablePorts);

	public bool IsRefreshingPortsList
	{
		get
		{
			return isRefreshingPortsList;
		}
		private set
		{
			isRefreshingPortsList = value;
			RaisePropertyChangedOnUi("IsRefreshingPortsList");
			if (value)
			{
				IsRefreshingPortDetails = value;
			}
			CommandManagerInvalidateOnUi();
		}
	}

	public bool IsRefreshingPortDetails
	{
		get
		{
			return isRefreshingPortDetails;
		}
		private set
		{
			isRefreshingPortDetails = value;
			RaisePropertyChangedOnUi("IsRefreshingPortDetails");
			CommandManagerInvalidateOnUi();
		}
	}

	public ICommand GetSerialPorts => getSerialPorts ?? (getSerialPorts = new Racelogic.Core.Win.RacelogicCommand(ExecuteGetSerialPorts, CanExecuteGetSerialPorts));

	private object ClosePortLock { get; set; }

	private object ReplayDataLock { get; set; }

	private bool RefreshPortsRequired { get; set; }

	public VBoxType VBoxType
	{
		get
		{
			return vBoxType;
		}
		internal set
		{
			if (value == vBoxType)
			{
				return;
			}
			vBoxType = value;
			if (autoRetrieveInformation)
			{
				Task.Factory.StartNew(delegate
				{
					GpsLatency = RetrieveGpsLatency(vBoxType, VBoxData.DualAntenna);
				});
			}
		}
	}

	public double GpsLatency
	{
		get
		{
			return gpsLatency;
		}
		private set
		{
			gpsLatency = value;
			RaisePropertyChangedOnUi("GpsLatency");
		}
	}

	public bool RequestingCanChannelInformation
	{
		get
		{
			return requestingCanChannelInformation;
		}
		private set
		{
			requestingCanChannelInformation = value;
			RaisePropertyChangedOnUi("RequestingCanChannelInformation");
		}
	}

	private CanChannelInformationStatus CanChannelInformationState { get; set; }

	public VBoxChannel AvailableVBoxData { get; private set; } = VBoxChannel.None;


	public VBoxChannel2 AvailableVBoxData2 { get; private set; } = VBoxChannel2.None;


	private int LengthOfMessage { get; set; }

	private bool oldDualAntenna { get; set; }

	public int RxCount
	{
		get
		{
			int num = ((writeIndex >= readIndex) ? (writeIndex - readIndex) : (writeIndex + 131072 - readIndex));
			if (num > maxRxCount)
			{
				MaxRxCount = num;
			}
			return num;
		}
	}

	public int MaxRxCount
	{
		get
		{
			return maxRxCount;
		}
		set
		{
			maxRxCount = value;
			RaisePropertyChangedOnUi("MaxRxCount");
		}
	}

	public int CrcError
	{
		get
		{
			return crcError;
		}
		set
		{
			crcError = value;
			RaisePropertyChangedOnUi("CrcError");
		}
	}

	private bool SetQuietRequested
	{
		get
		{
			lock (SetQuietLock)
			{
				return setQuietRequested;
			}
		}
		set
		{
			lock (SetQuietLock)
			{
				setQuietRequested = value;
			}
		}
	}

	private int ReadIndex
	{
		get
		{
			return readIndex;
		}
		set
		{
			lock (ReadIndexLock)
			{
				readIndex = ((value < 131072) ? value : 0);
			}
		}
	}

	private int WriteIndex
	{
		get
		{
			return writeIndex;
		}
		set
		{
			lock (WriteIndexLock)
			{
				writeIndex = ((value < 131072) ? value : 0);
			}
		}
	}

	private int SetupReadIndex
	{
		get
		{
			return setupReadIndex;
		}
		set
		{
			lock (SetupWriteIndexLock)
			{
				setupReadIndex = ((value < 131072) ? value : 0);
			}
		}
	}

	private int SetupWriteIndex
	{
		get
		{
			return setupWriteIndex;
		}
		set
		{
			lock (SetupWriteIndexLock)
			{
				setupWriteIndex = ((value < 131072) ? value : 0);
			}
		}
	}

	public int SetupRxCount => (setupWriteIndex >= setupReadIndex) ? (setupWriteIndex - setupReadIndex) : (setupWriteIndex + 131072 - setupReadIndex);

	public object Ascii { get; private set; }

	public int UnrecognisedWriteIndex
	{
		get
		{
			return unrecognisedWriteIndex;
		}
		private set
		{
			lock (UnrecognisedWriteIndexLock)
			{
				unrecognisedWriteIndex = ((value < 131072) ? value : 0);
			}
		}
	}

	public int UnrecognisedReadIndex
	{
		get
		{
			return unrecognisedReadIndex;
		}
		private set
		{
			lock (UnrecognisedReadIndexLock)
			{
				unrecognisedReadIndex = ((value < 131072) ? value : 0);
			}
		}
	}

	public int UnrecognisedRxCount => (UnrecognisedWriteIndex >= UnrecognisedReadIndex) ? (UnrecognisedWriteIndex - UnrecognisedReadIndex) : (UnrecognisedWriteIndex + 131072 - UnrecognisedReadIndex);

	public ICommand ClosePort
	{
		get
		{
			return closePort ?? (closePort = new Racelogic.Core.Win.RacelogicCommand(ExecuteClosePort, CanExecuteClosePort));
		}
		set
		{
			closePort = value;
		}
	}

	public ICommand OpenPort
	{
		get
		{
			return openPort ?? (openPort = new Racelogic.Core.Win.RacelogicCommand(ExecuteOpenPort, CanExecuteOpenPort));
		}
		set
		{
			openPort = value;
		}
	}

	public bool AutoRetrieveInformation
	{
		get
		{
			return autoRetrieveInformation;
		}
		set
		{
			autoRetrieveInformation = value;
			RaisePropertyChangedOnUi("AutoRetrieveInformation");
			if (value)
			{
				Task.Factory.StartNew(delegate
				{
					NoCommsTimer.Stop();
					GpsLatency = RetrieveGpsLatency(vBoxType, VBoxData.DualAntenna);
					CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, new List<CanChannel>(), null);
					NoCommsTimer.Start();
				});
			}
		}
	}

	public ICommand GetCanChannelInformation => getCanChannelInformation ?? (getCanChannelInformation = new Racelogic.Core.Win.RacelogicCommand(ExecuteGetCanChannelInformation, CanExecuteGetCanChannelInformation));

	public ICommand GetGpsLatency => getGpsLatency ?? (getGpsLatency = new Racelogic.Core.Win.RacelogicCommand(ExecuteGetGpsLatency, CanExecuteGetGpsLatency));

	private List<byte> _returnedData
	{
		get
		{
			return _returnedDatafield;
		}
		set
		{
			using (ReturnedDataLock.Lock())
			{
				_returnedDatafield = value;
			}
		}
	}

	public event EventHandler<SerialCommsInformationEventArgs> SerialCommsInformation;

	private event EventHandler<MessagesEventArgs> NewMessageSent;

	public ComPort(string portName = "")
	{
		ClosePortLock = new object();
		ReplayDataLock = new object();
		VBoxType = VBoxType.Unknown;
		DetectedUnit = VBoxType.Unknown;
		GpsLatency = double.NaN;
		VBoxData = new VBoxData();
		CanData = new CanData();
		LengthOfMessage = 0;
		CanChannelInformationState = CanChannelInformationStatus.Idle;
		NmeaData = new NmeaData();
		UpdateCanDataChannelsAction = UpdateCanDataChannels;
		DisplaySlidingMessageAction = DisplaySlidingMessage;
		Settings = default(PortSettings);
		Settings.Initialise(portName);
		ref EventHandler flushPort = ref Settings.FlushPort;
		flushPort = (EventHandler)Delegate.Combine(flushPort, (EventHandler)delegate
		{
			FlushRxTx();
		});
		AddDefaultErrorHandler();
		Settings.PropertyChanged += delegate(object s, PropertyChangedEventArgs e)
		{
			if (string.Equals("PortName", e.PropertyName))
			{
				RaisePropertyChangedOnUi("Name");
			}
			else if (string.Equals("BaudRate", e.PropertyName))
			{
				RaisePropertyChangedOnUi("BaudRate");
				UserSettings.BaudRate = Settings.BaudRateAsInt;
				HasFrameError = false;
				FlushRxTx();
			}
		};
		if (IsWPFApp)
		{
			Settings.NewSlidingMessage += delegate(object s, SlidingMessageEventArgs e)
			{
				Application.Current.Dispatcher.BeginInvoke(DisplaySlidingMessageAction, e.Text, e.Title, null);
			};
		}
		base.PropertyChanged += delegate(object s, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsOpen")
			{
				Reset();
				CanChannelInformationState = CanChannelInformationStatus.Idle;
				PortWasOpen = IsOpen;
				HasFrameError = false;
			}
			else if (e.PropertyName == "ComPorts" && PortWasOpen)
			{
				bool flag = false;
				foreach (AvailablePort availablePort in availablePorts)
				{
					if (string.Equals(availablePort.Name, Name))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Reset();
					CanChannelInformationState = CanChannelInformationStatus.Idle;
					RaisePropertyChangedOnUi("IsOpen");
				}
			}
		};
		RefreshComPortsList();
		if (IsWPFApp)
		{
			Initialize();
		}
		else
		{
			parserThread = new Thread(ParseData)
			{
				Name = "ParserThread"
			};
			parserThread.Start();
		}
		NoCommsTimer = new System.Timers.Timer(5000.0)
		{
			AutoReset = false
		};
		NoCommsTimer.Elapsed += delegate
		{
			if (_OldProtocolState == OldProtocolState.Idle)
			{
				FlushRxTx();
				IsReceivingVBoxComms = false;
				IsReceivingComms = false;
				VBoxData.Clear(clearCrcCount: true, InSetup);
				CanData.Clear(clearCrcCount: true);
				NmeaData.Clear();
				ProgressText = string.Empty;
				RaisePropertyChangedOnUi("VBoxData");
				RaisePropertyChangedOnUi("CanData");
				RaisePropertyChangedOnUi("NmeaData");
				if (VBoxType != 0)
				{
					UnitDisconnectedTimer.Enabled = true;
				}
			}
		};
		IsReceivingCommsTimer = new System.Timers.Timer(1000.0)
		{
			AutoReset = false
		};
		IsReceivingCommsTimer.Elapsed += delegate
		{
			IsReceivingComms = false;
		};
		UnitDisconnectedTimer = new System.Timers.Timer(5000.0)
		{
			AutoReset = false,
			Enabled = false
		};
		UnitDisconnectedTimer.Elapsed += delegate
		{
			if (!RequestingCanChannelInformation)
			{
				VBoxType = VBoxType.Unknown;
				DetectedUnit = VBoxType.Unknown;
			}
		};
		UserSettings = new UserSettings();
		if (File.Exists(Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "GPS simultator CAN channels.xml")))
		{
			IsInSimulatorMode = true;
			CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, ReadSimulatorChannels(), null);
		}
	}

	public override string ToString()
	{
		return Settings.PortName;
	}

	public void FlushRxTx()
	{
		NoCommsTimer.Stop();
		if (Settings.IsOpen)
		{
			Settings.FlushRxTx();
		}
		IsReceivingVBoxComms = false;
		IsReceivingComms = false;
		ReadIndex = 0;
		WriteIndex = 0;
		SetupReadIndex = 0;
		SetupWriteIndex = 0;
	}

	public void FlushUnrecognisedBuffer()
	{
		UnrecognisedReadIndex = 0;
		UnrecognisedWriteIndex = 0;
	}

	public bool TxData(byte[] data)
	{
		bool result = true;
		try
		{
			if (IsOpen)
			{
				Write(data);
			}
			else
			{
				result = false;
				DispatchSlidingMessage("Racelogic.Comms.Serial.TxData(): " + Racelogic.Comms.Serial.Properties.Resources.PortNotOpen, Racelogic.Comms.Serial.Properties.Resources.Information, null);
			}
		}
		catch (Exception ex)
		{
			result = false;
			StringBuilder stringBuilder = new StringBuilder(ex.Message);
			for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
			{
				stringBuilder.AppendLine(innerException.Message);
			}
			DispatchSlidingMessage("Racelogic.Comms.Serial.TxData(): " + Environment.NewLine + stringBuilder.ToString(), Racelogic.Comms.Serial.Properties.Resources.Information, null);
		}
		return result;
	}

	public bool TxData(Queue<byte> data)
	{
		return TxData(data.ToArray());
	}

	public bool TxData(IEnumerable<byte> data)
	{
		return TxData(data.ToArray());
	}

	public void Reset()
	{
		if (CanData != null)
		{
			CanData.Clear(clearCrcCount: true);
			if (!IsInSimulatorMode)
			{
				CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, new List<CanChannel>(), null);
			}
			else
			{
				CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, ReadSimulatorChannels(), null);
			}
		}
		RaisePropertyChangedOnUi("CanData");
		VBoxData.Clear(clearCrcCount: true, InSetup);
		VBoxData.SampleError = 0;
		RaisePropertyChangedOnUi("VBoxData");
		FlushRxTx();
		IsReceivingVBoxComms = false;
		IsReceivingComms = false;
		HeaderFound = Header.None;
		CrcString.Clear();
		CrcError = 0;
		MaxRxCount = 0;
		InSetup = false;
	}

	public void ReplayLoggedDataFile(Action<Collection<Sample>> processSamples, Action<int> percentRead)
	{
		ReplayLoggedData(null, null, processSamples, percentRead);
	}

	public void ReplayLoggedDataFile(double startTime, double endTime, Action<Collection<Sample>> processSamples, Action<int> percentRead)
	{
		ReplayLoggedData(startTime, endTime, processSamples, percentRead);
	}

	private void DisplaySlidingMessage(string text, string title, double? timeoutSeconds)
	{
		InformationMessage informationMessage = new InformationMessage(text, title)
		{
			Timeout = timeoutSeconds
		};
		Messenger.get_Default().Send<InformationMessage>(informationMessage, (object)"SlidingInformationMessage");
	}

	private void Current_Activated(object sender, EventArgs e)
	{
		Application.Current.Activated -= Current_Activated;
		Initialize();
	}

	private void Initialize()
	{
		if (IsWPFApp)
		{
			try
			{
				WqlEventQuery query = new WqlEventQuery
				{
					EventClassName = "__InstanceOperationEvent",
					WithinInterval = new TimeSpan(0, 0, 1),
					Condition = "TargetInstance ISA 'Win32_USBControllerdevice' "
				};
				_watcher = new ManagementEventWatcher(new ManagementScope
				{
					Options = 
					{
						EnablePrivileges = true
					}
				}, query);
				_watcherTimer = new System.Timers.Timer(200.0)
				{
					AutoReset = false,
					Enabled = false
				};
				_watcherTimer.Elapsed += delegate
				{
					RefreshComPortsList(isUsbEvent: true);
				};
				_watcher.EventArrived += delegate
				{
					_watcherTimer.Stop();
					_watcherTimer.Interval = 200.0;
					_watcherTimer.Start();
				};
				_watcher.Start();
			}
			catch
			{
			}
		}
		parserThread = new Thread(ParseData)
		{
			Name = "ParserThread"
		};
		ParsingRequired = true;
		parserThread.Start();
		if (IsLoggingRequired)
		{
			CreateLogger();
		}
	}

	private void CreateLogger()
	{
		if (!string.IsNullOrEmpty(VBoxCommsLogFileName) && File.Exists(VBoxCommsLogFileName))
		{
			File.Delete(VBoxCommsLogFileName);
		}
		VBoxCommsLogFileName = Path.GetTempFileName();
		VBoxCommsLogger = new BinaryWriter(new FileStream(VBoxCommsLogFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite), Encoding.GetEncoding(1252));
		CommandManagerInvalidateOnUi();
	}

	private void ReplayLoggedData(double? startTime, double? endTime, Action<Collection<Sample>> processSamples, Action<int> percentRead)
	{
		Task.Factory.StartNew(delegate
		{
			Sample oldSample = null;
			Collection<Sample> collection = new Collection<Sample>();
			if (!string.IsNullOrEmpty(VBoxCommsLogFileName) && File.Exists(VBoxCommsLogFileName))
			{
				if (!startTime.HasValue)
				{
					startTime = 0.0;
				}
				if (!endTime.HasValue)
				{
					endTime = double.MaxValue;
				}
				if (startTime.Value > endTime)
				{
					throw new ArgumentException("Racelogic.FileRoutines.ReplayLoggedDataFile - startTime must be less than endTime");
				}
				uint num = ReplayCount;
				ReplayCount = num + 1;
				using (BinaryReader binaryReader = new BinaryReader(new FileStream(VBoxCommsLogFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.GetEncoding(1252)))
				{
					double num2 = 0.0;
					int num3 = 0;
					bool flag = false;
					VBoxData vBoxData = new VBoxData();
					CanData canData = new CanData();
					while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
					{
						num2 = binaryReader.BaseStream.Position * 100 / binaryReader.BaseStream.Length;
						if ((int)num2 > num3)
						{
							num3 = (int)num2;
							percentRead?.Invoke(num3);
						}
						List<byte> list = new List<byte>(binaryReader.ReadBytes(7));
						string @string = Encoding.ASCII.GetString(list.ToArray());
						if (string.Equals(HeaderList[0], @string))
						{
							list.AddRange(binaryReader.ReadBytes(6));
							uint num4 = 0u;
							for (int i = 0; i < 4; i++)
							{
								num4 <<= 8;
								num4 |= list[8 + i];
							}
							uint num5 = 0u;
							for (int j = 0; j < 32; j++)
							{
								if ((int)(num4 & (1 << j)) == 1 << j)
								{
									num5++;
								}
							}
							if (num5 != CanData.Channels.Count)
							{
								break;
							}
							list.AddRange(binaryReader.ReadBytes((int)(4 * num5 + 2)));
							if (VerifyCheckSum(list))
							{
								canData.Channels = new ObservableCollection<CanChannel>(CanData.Channels);
								UpdateNewCanData(list, canData);
								if (flag && (double)vBoxData.UtcTime >= startTime.Value && (double)vBoxData.UtcTime <= endTime.Value)
								{
									Sample sample = new Sample(vBoxData, canData.Channels);
									sample.AddCalculatedChannels(oldSample, RacelogicDataFile.RequiredCalculatedChannel.Distance | RacelogicDataFile.RequiredCalculatedChannel.LateralAcceleration | RacelogicDataFile.RequiredCalculatedChannel.LongitudinalAcceleration | RacelogicDataFile.RequiredCalculatedChannel.Elapsedtime | RacelogicDataFile.RequiredCalculatedChannel.CombinedAcceleration);
									collection.Add(sample);
									oldSample = sample.Clone();
								}
							}
							else
							{
								canData.CrcError++;
							}
						}
						else
						{
							VBoxType vBoxType = (VBoxType)Array.IndexOf(HeaderList, @string);
							list.AddRange(binaryReader.ReadBytes(10));
							uint num6 = 0u;
							uint num7 = 0u;
							for (int k = 0; k < 4; k++)
							{
								num6 <<= 8;
								num6 |= list[8 + k];
								num7 <<= 8;
								num7 |= list[12 + k];
							}
							VBoxChannel availableChannels = (VBoxChannel)num6;
							list.AddRange(binaryReader.ReadBytes(CalculateMessageLength(availableChannels, num7)));
							if (VerifyCheckSum(list))
							{
								flag = true;
								UpdateVBoxData(list, vBoxData, availableChannels);
								if ((double)vBoxData.UtcTime > endTime.Value)
								{
									break;
								}
							}
							else
							{
								vBoxData.CrcError++;
							}
						}
					}
				}
				num = ReplayCount;
				ReplayCount = num - 1;
			}
			processSamples?.Invoke(collection);
		});
	}

	public void RaisePropertyChangedOnUi(string propertyName)
	{
		if (IsWPFApp)
		{
			if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
			{
				RaisePropertyChanged(propertyName);
				return;
			}
			Application.Current.Dispatcher.BeginInvoke(new Action<string>(RaisePropertyChangedOnUi), propertyName);
		}
		else
		{
			RaisePropertyChanged(propertyName);
		}
	}

	private void CommandManagerInvalidateOnUi()
	{
		if (IsWPFApp)
		{
			if (Thread.CurrentThread == Application.Current.Dispatcher.Thread)
			{
				CommandManager.InvalidateRequerySuggested();
				return;
			}
			Application.Current.Dispatcher.BeginInvoke((Action)delegate
			{
				CommandManager.InvalidateRequerySuggested();
			}, DispatcherPriority.Render);
		}
		else
		{
			RaisePropertyChanged("Invalidate");
		}
	}

	private void DispatchSlidingMessage(params string[] args)
	{
		if (IsWPFApp)
		{
			Application.Current.Dispatcher.BeginInvoke(DisplaySlidingMessageAction, args);
		}
	}

	public void ClosePortAndCleanup()
	{
		if (VBoxCommsLogger != null)
		{
			VBoxCommsLogger.Flush();
			VBoxCommsLogger.Close();
		}
		if (!string.IsNullOrEmpty(VBoxCommsLogFileName) && File.Exists(VBoxCommsLogFileName))
		{
			File.Delete(VBoxCommsLogFileName);
			VBoxCommsLogFileName = string.Empty;
		}
		if (_watcherTimer != null)
		{
			_watcherTimer.Stop();
		}
		if (_watcher != null)
		{
			_watcher.Stop();
		}
		if (parserThread != null)
		{
			ParsingRequired = false;
			ParserResetEvent.Set();
		}
		CloseAPort();
	}

	~ComPort()
	{
		ClosePortAndCleanup();
	}

	void IDisposable.Dispose()
	{
		ClosePortAndCleanup();
	}

	private void NotifyCollectionChanged()
	{
	}

	private void DebugLoggerCreate()
	{
	}

	private void DebugLoggerFunctionName(string value)
	{
	}

	private void DebugLogerData(byte[] data)
	{
	}

	public void RefreshComPortsList(bool isUsbEvent = false)
	{
		if (!IsRefreshingPortsList)
		{
			IsRefreshingPortsList = true;
			Task.Factory.StartNew(delegate
			{
				GetAvailablePortsList(isUsbEvent);
			});
		}
		else
		{
			RefreshPortsRequired = true;
		}
	}

	public List<RacelogicPort> GetAllRacelogicUsbPorts()
	{
		List<RacelogicPort> usbPorts = GetUsbPorts(6017, 0);
		foreach (RacelogicPort item in GetUsbPort(1155, 13313))
		{
			usbPorts.Add(item);
		}
		return usbPorts;
	}

	public List<RacelogicPort> GetRacelogicUsbPort(int pid)
	{
		return GetUsbPort(6017, pid);
	}

	public List<RacelogicPort> GetUsbPort(int vid, int pid)
	{
		return GetUsbPorts(vid, pid);
	}

	private bool CanExecuteGetSerialPorts(object param)
	{
		return !IsRefreshingPortsList && !IsRefreshingPortDetails;
	}

	private void ExecuteGetSerialPorts(object param)
	{
		RefreshComPortsList();
	}

	private void GetPortAvailability(Func<AvailablePort, bool> predicate, bool isUsbEvent, List<AvailablePort> removedDevices)
	{
		bool flag = true;
		foreach (AvailablePort item in availablePorts.Where(predicate))
		{
			if (RefreshPortsRequired)
			{
				break;
			}
			if (flag)
			{
				DateTime now = DateTime.Now;
				GetPortAvailability(item, isUsbEvent, removedDevices);
			}
			else
			{
				item.IsBusy = false;
			}
			RaisePropertyChangedOnUi("ComPorts");
		}
	}

	private void GetAvailablePortsList(bool isUsbEvent)
	{
		try
		{
			using (GetPortsLock.Lock())
			{
				availablePorts = GetSortedPortsList(isUsbEvent);
				foreach (AvailablePort availablePort in availablePorts)
				{
					try
					{
						GetPortDescription(availablePort);
					}
					catch
					{
					}
				}
			}
			IsRefreshingPortsList = false;
			RaisePropertyChangedOnUi("ComPorts");
			using (GetPortsLock.Lock())
			{
				if (!RefreshPortsRequired)
				{
					List<AvailablePort> list = new List<AvailablePort>();
					GetPortAvailability((AvailablePort p) => !p.Description.ToLower().Contains("bluetooth") && !p.Description.ToLower().Contains("usb"), isUsbEvent, list);
					GetPortAvailability((AvailablePort p) => p.Description.ToLower().Contains("usb"), isUsbEvent, list);
					GetPortAvailability((AvailablePort p) => p.Description.ToLower().Contains("bluetooth"), isUsbEvent, list);
					foreach (AvailablePort item in list)
					{
						availablePorts.Remove(item);
					}
				}
				RaisePropertyChangedOnUi("ComPorts");
			}
		}
		catch (Exception)
		{
			DisplaySlidingMessage(Racelogic.Comms.Serial.Properties.Resources.GetPortsError, Racelogic.Comms.Serial.Properties.Resources.Information, null);
		}
		if (RefreshPortsRequired)
		{
			RefreshPortsRequired = false;
			GetAvailablePortsList(isUsbEvent: false);
		}
		else
		{
			IsRefreshingPortsList = false;
			IsRefreshingPortDetails = false;
			RaisePropertyChangedOnUi("ComPorts");
		}
	}

	private ObservableCollection<AvailablePort> GetSortedPortsList(bool isUsbEvent)
	{
		string[] portNames = SerialPort.GetPortNames();
		List<int> list = new List<int>();
		string[] array = portNames;
		foreach (string text in array)
		{
			if (int.TryParse(text.Substring(3), out var result))
			{
				list.Add(result);
			}
		}
		list.Sort();
		ObservableCollection<AvailablePort> observableCollection = new ObservableCollection<AvailablePort>();
		foreach (int item in list)
		{
			observableCollection.Add(new AvailablePort("COM" + item));
		}
		return observableCollection;
	}

	private void _GetPortDescription(AvailablePort port)
	{
		ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
		foreach (ManagementObject item in managementObjectSearcher.Get())
		{
			string text = item["Name"].ToString();
			if (text.Contains("(COM") && string.Equals(text.Substring(text.IndexOf("(COM")), "(" + port.Name + ")"))
			{
				port.Description = text;
				break;
			}
		}
	}

	private void GetPortDescription(AvailablePort port)
	{
		using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Caption like '%(COM%'");
		string[] portNames = SerialPort.GetPortNames();
		IEnumerable<string> ports = from p in managementObjectSearcher.Get().Cast<ManagementBaseObject>().ToList()
			select p["Caption"].ToString();
		port.Description = ports.FirstOrDefault((string s) => s.Contains("(" + port.Name + ")"));
		List<string> list = portNames.Select((string n) => ports.FirstOrDefault((string s) => s.Contains(n))).ToList();
	}

	private void GetPortAvailability(AvailablePort port, bool isUsbEvent, List<AvailablePort> removedDevices)
	{
		string portName = Settings.PortName;
		bool isOpen = Settings.IsOpen;
		bool isAvailable = false;
		if (!removedDevices.Contains(port))
		{
			if (string.Equals(port.Name, portName) && isOpen)
			{
				isAvailable = true;
			}
			else
			{
				SerialPort serialPort = new SerialPort(port.Name);
				try
				{
					serialPort.Open();
					isAvailable = true;
				}
				catch (Exception)
				{
					if (string.Equals(port.Name, portName) && PortWasOpen)
					{
						removedDevices.Add(port);
					}
				}
				Thread.Sleep(25);
				serialPort.Close();
				serialPort = null;
			}
		}
		port.IsAvailable = isAvailable;
		port.IsBusy = false;
	}

	private List<RacelogicPort> GetUsbPorts(int vid, int pid)
	{
		List<RacelogicPort> list = new List<RacelogicPort>();
		ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity");
		foreach (ManagementObject item in managementObjectSearcher.Get())
		{
			try
			{
				string text = item["DeviceID"].ToString();
				string text2 = item["Name"].ToString();
				if (text.Contains("PID_") && text.Contains("VID_") && text2.Contains("(COM"))
				{
					text2 = text2.Substring(text2.IndexOf("(COM"));
					text2 = text2.Trim('(', ')');
					int num = int.Parse(text.Substring(text.IndexOf("VID_") + 4, 4), NumberStyles.HexNumber);
					int num2 = int.Parse(text.Substring(text.IndexOf("PID_") + 4, 4), NumberStyles.HexNumber);
					if (num == vid && (num2 == pid || pid == 0 || (vid == 6017 && num2 >= 2630 && num2 <= 2649)))
					{
						list.Add(new RacelogicPort(text2, num, num2));
					}
				}
			}
			catch
			{
			}
		}
		return list;
	}

	private void UpdateCanDataChannels(List<CanChannel> newChannels, List<CanChannel> internalA2D)
	{
		lock (CanDataChannelsLock)
		{
			CanData.Channels = new ObservableCollection<CanChannel>(newChannels);
			for (int i = 0; i < VBoxData.InternalA2D.Count; i++)
			{
				VBoxData.InternalA2D[i].Name = ((internalA2D == null) ? string.Empty : internalA2D[i].Name);
				VBoxData.InternalA2D[i].Units = ((internalA2D == null) ? string.Empty : internalA2D[i].Units);
			}
			if (CanChannelInformationState == CanChannelInformationStatus.Received)
			{
				CanChannelInformationState = CanChannelInformationStatus.Updated;
			}
		}
	}

	private Header DetectHeader()
	{
		Header header = Header.None;
		CrcString.Clear();
		bool flag = false;
		while (header == Header.None && RxCount > 0)
		{
			if (InSetup)
			{
				setupReceivedDataBuffer[SetupWriteIndex++] = receivedDataBuffer[ReadIndex++];
				VBoxData.UnrecognisedCharacter++;
				continue;
			}
			flag = false;
			string currentHeader = CurrentHeader;
			char c = (char)receivedDataBuffer[ReadIndex++];
			CurrentHeader = currentHeader + c;
			using (IEnumerator<string> enumerator = HeaderList.Where((string h) => h.StartsWith(CurrentHeader)).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					flag = true;
					int num = Array.IndexOf(HeaderList, CurrentHeader);
					if (num != -1)
					{
						string currentHeader2 = CurrentHeader;
						foreach (char c2 in currentHeader2)
						{
							CrcString.Add((byte)c2);
						}
						CurrentHeader = string.Empty;
						string text = current;
						string text2 = text;
						if (text2 != null)
						{
							switch (text2)
							{
							case "$RLCMD$":
								header = Header.RacelogicCommand;
								break;
							case "$RLRSP$":
								header = Header.RacelogicResponse;
								break;
							case "$VBOXII":
							case "$VBOX3$":
							case "$VB2SX$":
							case "$VB2100":
							case "$VBOXM$":
							case "$VB2SX2":
							case "$VB2SL$":
							case "$VBSX10":
							case "$VBOX3i":
							case "$VB3TR2":
							case "$VB3TR3":
							case "$VBOXV$":
							case "$VBMICR":
							case "$VBBTST":
							case "$VBSPT$":
							case "$VB3is$":
								DetectedUnit = (VBoxType)Array.IndexOf(HeaderList, current);
								VBoxType = ((DetectedUnit == VBoxType.VB3is) ? VBoxType.VBox3i : DetectedUnit);
								header = Header.VBox;
								break;
							case "$NEWCAN":
								header = Header.NewCan;
								break;
							case "$NEWPOS":
								header = Header.NewPosition;
								break;
							case "$GPGGA":
							case "$GPVTG":
							case "$GPZDA":
							case "$GPGLL":
							case "$GPGSA":
							case "$GPRMC":
							case "$GPGSV":
							case "$GPGST":
							case "$GPTXT":
							case "$GNGGA":
							case "$GNVTG":
							case "$GNZDA":
							case "$GNGLL":
							case "$GNGSA":
							case "$GNRMC":
							case "$GNGSV":
							case "$GNGST":
							case "$GNTXT":
								header = Header.Nmea;
								break;
							}
						}
					}
				}
			}
			if (!flag)
			{
				VBoxData.UnrecognisedCharacter++;
				setupReceivedDataBuffer[SetupWriteIndex++] = (byte)CurrentHeader[0];
				UnrecognisedReceivedDataBuffer[UnrecognisedWriteIndex++] = (byte)CurrentHeader[0];
				CurrentHeader = CurrentHeader.Remove(0, 1);
				while (CurrentHeader.Length > 0 && CurrentHeader[0] != '$')
				{
					setupReceivedDataBuffer[SetupWriteIndex++] = (byte)CurrentHeader[0];
					UnrecognisedReceivedDataBuffer[UnrecognisedWriteIndex++] = (byte)CurrentHeader[0];
					CurrentHeader = CurrentHeader.Remove(0, 1);
				}
			}
		}
		return header;
	}

	private void ExtractBrakeTestSpeedSensorData()
	{
		if (RxCount < 29)
		{
			return;
		}
		if (VerifyCheckSum(29))
		{
			Union union = default(Union);
			doubleUnion doubleUnion = default(doubleUnion);
			int startIndex = 7;
			VBoxData.Satellites = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			uint num = (uint)ReadFromMessage(CrcString, ref startIndex, 3);
			double num2 = (double)num - OldVBoxTime;
			if (num2 > 0.0)
			{
				VBoxData.SampleRateHz = Math.Round(100.0 / num2, MidpointRounding.AwayFromZero);
			}
			else
			{
				VBoxData.SampleRateHz = 20.0;
			}
			OldVBoxTime = num;
			VBoxData.UtcTime = (double)num / 100.0;
			union.b0_LSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b1 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b2 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b3_MSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			VBoxData.SpeedKilometresPerHour = (double)union.data / SpeedConstants.KilometresPerHourToMetresPerSecond;
			num = (uint)ReadFromMessage(CrcString, ref startIndex, 2);
			VBoxData.Heading = (double)num / 100.0;
			union.b0_LSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b1 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b2 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b3_MSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b0_LSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b1 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b2 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b3 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b4 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b5 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b6 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b7_MSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b0_LSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b1 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b2 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			union.b3_MSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			VBoxData.TriggerEventTimeSeconds = union.data;
			num = (uint)ReadFromMessage(CrcString, ref startIndex, 1);
			VBoxData.BrakeTrigger = (num & 1) == 1;
			AvailableVBoxData = VBoxChannel.Satellites | VBoxChannel.UtcTime | VBoxChannel.Speed | VBoxChannel.Heading | VBoxChannel.TriggerEventTime | VBoxChannel.BrakeTrigger;
			AvailableVBoxData2 = VBoxChannel2.None;
		}
		else
		{
			AvailableVBoxData = VBoxChannel.None;
			AvailableVBoxData2 = VBoxChannel2.None;
			VBoxData.Clear(clearCrcCount: false);
			VBoxData.CrcError++;
		}
		HeaderFound = Header.None;
		RaisePropertyChanged("VBoxData");
	}

	private void ExtractNewCanData()
	{
		if (CrcString.Count < 13)
		{
			if (RxCount < 6)
			{
				return;
			}
			for (int i = 0; i < 6; i++)
			{
				CrcString.Add(receivedDataBuffer[ReadIndex++]);
			}
			uint num = 0u;
			for (int j = 0; j < 4; j++)
			{
				num <<= 8;
				num |= CrcString[8 + j];
			}
			uint num2 = 0u;
			for (int k = 0; k < 32; k++)
			{
				if ((int)(num & (1 << k)) == 1 << k)
				{
					num2++;
				}
			}
			LengthOfMessage = (int)(4 * num2 + 2);
			if (CanChannelInformationState == CanChannelInformationStatus.Updated)
			{
				VBoxData.UnrecognisedCharacter = 0;
				CanChannelInformationState = CanChannelInformationStatus.Idle;
			}
			if (CanData.ChannelsBeingSentOverSerial != num2 && !IsInSimulatorMode)
			{
				HeaderFound = Header.None;
				if (!SetQuietRequested)
				{
					if (num2 == 0)
					{
						CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, new List<CanChannel>(), null);
					}
					else if (autoRetrieveInformation)
					{
						if (GetCanChannelInformation.CanExecute(null))
						{
							SetQuietNoWaitForResponse_StandardProtocol(MakeQuiet: true);
							GetCanChannelInformation.Execute(null);
						}
					}
					else
					{
						List<CanChannel> list = new List<CanChannel>();
						for (int l = 0; l < num2; l++)
						{
							CanChannel canChannel = new CanChannel();
							canChannel.Name = $"CAN_{l + 1}";
							canChannel.IsBeingSentOverSerial = true;
							list.Add(canChannel);
						}
						CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, list, null);
					}
				}
			}
			else
			{
				CanChannelInformationState = CanChannelInformationStatus.Idle;
			}
			WaitForNewCanData();
		}
		else
		{
			WaitForNewCanData();
		}
	}

	private void WaitForNewCanData()
	{
		if (RxCount < LengthOfMessage)
		{
			return;
		}
		if (VerifyCheckSum(LengthOfMessage))
		{
			if (VBoxCommsLogger != null)
			{
				LogData(CrcString.ToArray());
			}
			if (!IsReplayingLoggedData)
			{
				UpdateNewCanData(CrcString, CanData);
			}
		}
		else
		{
			CanData.CrcError++;
		}
		HeaderFound = Header.None;
		RaisePropertyChanged("CanData");
	}

	internal void UpdateNewCanData(List<byte> data, CanData canData)
	{
		int num = 0;
		Union union = default(Union);
		foreach (CanChannel channel in canData.Channels)
		{
			channel.Value = 0.0;
			if (!channel.IsBeingSentOverSerial)
			{
				continue;
			}
			if (num * 4 + 16 >= data.Count)
			{
				channel.Value = 0.0;
				continue;
			}
			union.temp = 0;
			union.data = 0f;
			union.b3_MSB = data[num * 4 + 13];
			union.b2 = data[num * 4 + 14];
			union.b1 = data[num * 4 + 15];
			union.b0_LSB = data[num * 4 + 16];
			if (VBoxType == VBoxType.VBoxII)
			{
				channel.Value = double.Parse(CanSignal.ConvertRacelogicUnits(union.temp), CultureInfo.InvariantCulture);
			}
			else
			{
				channel.Value = union.data;
			}
			num++;
		}
	}

	internal void UpdateNewPositionData(List<byte> data, VBoxData vBoxData)
	{
		if (data[8] != 2)
		{
			return;
		}
		doubleUnion doubleUnion = default(doubleUnion);
		for (int i = 0; i < 2; i++)
		{
			doubleUnion.b0_LSB = data[9 + i * 8];
			doubleUnion.b1 = data[10 + i * 8];
			doubleUnion.b2 = data[11 + i * 8];
			doubleUnion.b3 = data[12 + i * 8];
			doubleUnion.b4 = data[13 + i * 8];
			doubleUnion.b5 = data[14 + i * 8];
			doubleUnion.b6 = data[15 + i * 8];
			doubleUnion.b7_MSB = data[16 + i * 8];
			if (i == 0)
			{
				vBoxData.LongitudeXMinutes = doubleUnion.data;
			}
			else
			{
				vBoxData.LatitudeYMinutes = doubleUnion.data;
			}
		}
	}

	private void ExtractNewPositionData()
	{
		if (RxCount >= 20)
		{
			if (VerifyCheckSum(20))
			{
				UpdateNewPositionData(CrcString, VBoxData);
			}
			else
			{
				VBoxData.LongitudeXMinutes = 0.0;
				VBoxData.LatitudeYMinutes = 0.0;
				VBoxData.CrcError++;
			}
			HeaderFound = Header.None;
			RaisePropertyChanged("VBoxData");
		}
	}

	private void ExtractNmeaData()
	{
		if (RxCount <= 0)
		{
			return;
		}
		CrcString.Add(receivedDataBuffer[ReadIndex++]);
		if (CrcString.Count < 11 || CrcString[CrcString.Count - 2] != 13 || CrcString[CrcString.Count - 1] != 10)
		{
			return;
		}
		if (CheckNmeaChecksum(CrcString))
		{
			try
			{
				ExtractNmeaData(CrcString);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			CrcString.Clear();
			RaisePropertyChanged("NmeaData");
		}
		else
		{
			CrcError++;
			CrcString.Clear();
			NmeaData.Clear();
			RaisePropertyChanged("NmeaData");
		}
		HeaderFound = Header.None;
	}

	private void _ExtractNmeaData()
	{
		try
		{
			if (CrcString.Count + RxCount < 11)
			{
				return;
			}
			while (RxCount > 0 && HeaderFound != 0)
			{
				CrcString.Add(receivedDataBuffer[ReadIndex++]);
				if (CrcString.Count >= 11 && CrcString[CrcString.Count - 2] == 13 && CrcString[CrcString.Count - 1] == 10)
				{
					if (CheckNmeaChecksum(CrcString))
					{
						ExtractNmeaData(CrcString);
						CrcString.Clear();
						RaisePropertyChanged("NmeaData");
					}
					else
					{
						CrcError++;
						CrcString.Clear();
						NmeaData.Clear();
						RaisePropertyChanged("NmeaData");
					}
					HeaderFound = Header.None;
				}
			}
		}
		catch (Exception)
		{
			CrcError++;
			CrcString.Clear();
		}
	}

	private bool CheckNmeaChecksum(List<byte> nmeaData)
	{
		bool result = false;
		byte b = 0;
		int num = nmeaData.IndexOf(42);
		if (num != -1 && num < nmeaData.Count - 4)
		{
			for (int i = 1; i < num; i++)
			{
				b = (byte)(b ^ nmeaData[i]);
			}
			string empty = string.Empty;
			empty += (char)nmeaData[num + 1];
			empty += (char)nmeaData[num + 2];
			int result2 = 0;
			if (int.TryParse(empty, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result2))
			{
				result = b == result2;
			}
		}
		return result;
	}

	private void ExtractNmeaData(List<byte> nmeaData)
	{
		string[] array = NmeaDataAsString(nmeaData).Split(',');
		if (array[0] == "GGA")
		{
			ExtractGGA(array);
		}
		else if (array[0] == "VTG")
		{
			ExtractVTG(array);
		}
		else if (array[0] == "ZDA")
		{
			ExtractZDA(array);
		}
		else if (array[0] == "GLL")
		{
			ExtractGLL(array);
		}
		else if (array[0] == "GSA")
		{
			ExtractGSA(array);
		}
		else if (array[0] == "RMC")
		{
			ExtractRMC(array);
		}
		else if (array[0] == "GSV")
		{
			ExtractGSV(array);
		}
		else if (array[0] == "GST")
		{
			ExtractGST(array);
		}
		else if (array[0] == "TXT")
		{
			ExtractTXT(array);
		}
	}

	private string NmeaDataAsString(List<byte> nmeaData)
	{
		StringBuilder stringBuilder = new StringBuilder(nmeaData.Count - 5);
		for (int i = 3; i < nmeaData.Count - 2; i++)
		{
			stringBuilder.Append((char)nmeaData[i]);
		}
		return stringBuilder.ToString();
	}

	private void ExtractGST(string[] nmeaData)
	{
		if (nmeaData.Length >= 9)
		{
			NmeaData.GST.UtcFixTime = (string.IsNullOrEmpty(nmeaData[1]) ? 0.0 : (double.Parse(nmeaData[1].Substring(0, 2), CultureInfo.InvariantCulture) * 3600.0 + double.Parse(nmeaData[1].Substring(2, 2), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[1].Substring(4), CultureInfo.InvariantCulture)));
			NmeaData.GST.Rms = (string.IsNullOrEmpty(nmeaData[2]) ? 0.0 : double.Parse(nmeaData[2], CultureInfo.InvariantCulture));
			NmeaData.GST.StandardDeviationOfSemiMajorAxisOfErrorEllipseMetres = (string.IsNullOrEmpty(nmeaData[3]) ? 0.0 : double.Parse(nmeaData[3], CultureInfo.InvariantCulture));
			NmeaData.GST.StandardDeviationOfSemiMinorAxisOfErrorEllipseMetres = (string.IsNullOrEmpty(nmeaData[4]) ? 0.0 : double.Parse(nmeaData[4], CultureInfo.InvariantCulture));
			NmeaData.GST.OrientationOfSemiMajorAxisOfErrorEllipseDegrees = (string.IsNullOrEmpty(nmeaData[5]) ? 0.0 : double.Parse(nmeaData[5], CultureInfo.InvariantCulture));
			NmeaData.GST.StandardDeviationOfLatitudeErrorMetres = (string.IsNullOrEmpty(nmeaData[6]) ? 0.0 : double.Parse(nmeaData[6], CultureInfo.InvariantCulture));
			NmeaData.GST.StandardDeviationOfLongitudeErrorMetres = (string.IsNullOrEmpty(nmeaData[7]) ? 0.0 : double.Parse(nmeaData[7], CultureInfo.InvariantCulture));
			NmeaData.GST.StandardDeviationOfAltitudeErrorMetres = ((nmeaData[8].IndexOf('*') > 0) ? double.Parse(nmeaData[8].Substring(0, nmeaData[8].IndexOf('*')), CultureInfo.InvariantCulture) : 0.0);
		}
	}

	private void ExtractGSV(string[] nmeaData)
	{
		if (nmeaData.Length < 4 || string.IsNullOrEmpty(nmeaData[2]))
		{
			return;
		}
		int num = int.Parse(nmeaData[2]);
		if (num == 1)
		{
			NmeaData.GSV.SatellitesInView = (nmeaData[3].Contains('*') ? byte.Parse(nmeaData[3].Substring(0, nmeaData[3].IndexOf('*'))) : byte.Parse(nmeaData[3]));
			NmeaData.GSV.SatelliteInformation = new SatelliteInfo[NmeaData.GSV.SatellitesInView];
		}
		num = 4 * --num;
		int num2 = 4;
		int num3 = -1;
		while (num2 + 4 <= nmeaData.Length && ++num3 < 4)
		{
			if (NmeaData.GSV.SatelliteInformation.Length > num + num3)
			{
				NmeaData.GSV.SatelliteInformation[num + num3].PrnNumber = (byte)((!string.IsNullOrEmpty(nmeaData[num2])) ? byte.Parse(nmeaData[num2]) : 0);
				num2++;
				NmeaData.GSV.SatelliteInformation[num + num3].Elevation = (byte)((!string.IsNullOrEmpty(nmeaData[num2])) ? byte.Parse(nmeaData[num2]) : 0);
				num2++;
				NmeaData.GSV.SatelliteInformation[num + num3].Azimuth = ((!string.IsNullOrEmpty(nmeaData[num2])) ? int.Parse(nmeaData[num2]) : 0);
				num2++;
				NmeaData.GSV.SatelliteInformation[num + num3].Snr = (byte)((!string.IsNullOrEmpty(nmeaData[num2]) && nmeaData[num2].Length >= 2 && !nmeaData[num2].StartsWith("*")) ? byte.Parse(nmeaData[num2].Substring(0, 2)) : 0);
				num2++;
			}
		}
	}

	private void ExtractRMC(string[] nmeaData)
	{
		if (nmeaData.Length >= 12)
		{
			NmeaData.RMC.UtcFixTime = (string.IsNullOrEmpty(nmeaData[1]) ? 0.0 : (double.Parse(nmeaData[1].Substring(0, 2), CultureInfo.InvariantCulture) * 3600.0 + double.Parse(nmeaData[1].Substring(2, 2), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[1].Substring(4), CultureInfo.InvariantCulture)));
			NmeaData.RMC.Status = (NmeaActiveIndicator)((!string.IsNullOrEmpty(nmeaData[2])) ? nmeaData[2][0] : '\0');
			NmeaData.RMC.LatitudeMinutes = (string.IsNullOrEmpty(nmeaData[3]) ? 0.0 : ((double.Parse(nmeaData[3].Substring(0, 2), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[3].Substring(2), CultureInfo.InvariantCulture)) * (double)((!(nmeaData[4] == "S")) ? 1 : (-1))));
			NmeaData.RMC.LongitudeMinutes = (string.IsNullOrEmpty(nmeaData[5]) ? 0.0 : ((double.Parse(nmeaData[5].Substring(0, 3), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[5].Substring(3), CultureInfo.InvariantCulture)) * (double)((!(nmeaData[6] == "W")) ? 1 : (-1))));
			NmeaData.RMC.SpeedOverGroundKnots = (string.IsNullOrEmpty(nmeaData[7]) ? 0.0 : double.Parse(nmeaData[7], CultureInfo.InvariantCulture));
			NmeaData.RMC.TrackAngle = (string.IsNullOrEmpty(nmeaData[8]) ? 0.0 : double.Parse(nmeaData[8], CultureInfo.InvariantCulture));
			NmeaData.RMC.Date = (string.IsNullOrEmpty(nmeaData[9]) ? default(DateTime) : new DateTime(2000 + int.Parse(nmeaData[9].Substring(4, 2)), int.Parse(nmeaData[9].Substring(2, 2)), int.Parse(nmeaData[9].Substring(0, 2))));
			NmeaData.RMC.MagneticVariation = (string.IsNullOrEmpty(nmeaData[10]) ? 0.0 : double.Parse(nmeaData[10], CultureInfo.InvariantCulture));
			NmeaData.RMC.MagneticVariationHemisphere = (Hemisphere)((!string.IsNullOrEmpty(nmeaData[11])) ? nmeaData[11][0] : '\0');
			if (nmeaData.Length < 13)
			{
				NmeaData.RMC.ModeIndicator = NmeaModeIndicator.NoData;
			}
			else
			{
				NmeaData.RMC.ModeIndicator = (NmeaModeIndicator)((!string.IsNullOrEmpty(nmeaData[12])) ? nmeaData[12][0] : '\0');
			}
		}
	}

	private void ExtractGSA(string[] nmeaData)
	{
		if (nmeaData.Length < 18)
		{
			return;
		}
		NmeaData.GSA.FixSelection = (NmeaFixSelection)((!string.IsNullOrEmpty(nmeaData[1])) ? nmeaData[1][0] : '\0');
		NmeaData.GSA.Fix = (Nmea3DFix)((!string.IsNullOrEmpty(nmeaData[2])) ? nmeaData[2][0] : '\0');
		NmeaData.GSA.SatellitePrn.Clear();
		for (int i = 3; i < 15; i++)
		{
			if (!string.IsNullOrEmpty(nmeaData[i]))
			{
				NmeaData.GSA.SatellitePrn.Add(byte.Parse(nmeaData[i]));
			}
		}
		NmeaData.GSA.Pdop = (string.IsNullOrEmpty(nmeaData[15]) ? 0.0 : double.Parse(nmeaData[15], CultureInfo.InvariantCulture));
		NmeaData.GSA.Hdop = (string.IsNullOrEmpty(nmeaData[16]) ? 0.0 : double.Parse(nmeaData[16], CultureInfo.InvariantCulture));
		if (string.IsNullOrEmpty(nmeaData[17]))
		{
			NmeaData.GSA.Vdop = 0.0;
		}
		else if (nmeaData[17].Contains("*"))
		{
			if (nmeaData[17].IndexOf('*') != 0)
			{
				NmeaData.GSA.Vdop = double.Parse(nmeaData[17].Substring(0, nmeaData[17].IndexOf('*')), CultureInfo.InvariantCulture);
			}
			else
			{
				NmeaData.GSA.Vdop = 0.0;
			}
		}
		else
		{
			NmeaData.GSA.Vdop = double.Parse(nmeaData[17], CultureInfo.InvariantCulture);
		}
	}

	private void ExtractGLL(string[] nmeaData)
	{
		if (nmeaData.Length >= 7)
		{
			NmeaData.GLL.LatitudeMinutes = (string.IsNullOrEmpty(nmeaData[1]) ? 0.0 : ((double.Parse(nmeaData[1].Substring(0, 2), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[1].Substring(2), CultureInfo.InvariantCulture)) * (double)((!(nmeaData[2] == "S")) ? 1 : (-1))));
			NmeaData.GLL.LongitudeMinutes = (string.IsNullOrEmpty(nmeaData[3]) ? 0.0 : ((double.Parse(nmeaData[3].Substring(0, 3), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[3].Substring(3), CultureInfo.InvariantCulture)) * (double)((!(nmeaData[4] == "W")) ? 1 : (-1))));
			NmeaData.GLL.UtcFixTime = (string.IsNullOrEmpty(nmeaData[5]) ? 0.0 : (double.Parse(nmeaData[5].Substring(0, 2), CultureInfo.InvariantCulture) * 3600.0 + double.Parse(nmeaData[5].Substring(2, 2), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[5].Substring(4), CultureInfo.InvariantCulture)));
			NmeaData.GLL.Status = (NmeaActiveIndicator)((!string.IsNullOrEmpty(nmeaData[6])) ? nmeaData[6][0] : '\0');
			if (nmeaData.Length < 8)
			{
				NmeaData.GLL.ModeIndicator = NmeaModeIndicator.NoData;
			}
			else
			{
				NmeaData.GLL.ModeIndicator = (NmeaModeIndicator)((!string.IsNullOrEmpty(nmeaData[7])) ? nmeaData[7][0] : '\0');
			}
		}
	}

	private void ExtractZDA(string[] nmeaData)
	{
		if (nmeaData.Length >= 7)
		{
			NmeaData.ZDA.Utctime = (string.IsNullOrEmpty(nmeaData[1]) ? 0.0 : (double.Parse(nmeaData[1].Substring(0, 2), CultureInfo.InvariantCulture) * 3600.0 + double.Parse(nmeaData[1].Substring(2, 2), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[1].Substring(4), CultureInfo.InvariantCulture)));
			NmeaData.ZDA.Date = (string.IsNullOrEmpty(nmeaData[2]) ? default(DateTime) : new DateTime(int.Parse(nmeaData[4]), int.Parse(nmeaData[3]), int.Parse(nmeaData[2])));
			NmeaData.ZDA.LocalZoneHours = (byte)((!string.IsNullOrEmpty(nmeaData[5])) ? byte.Parse(nmeaData[5]) : 0);
			NmeaData.ZDA.LocalZoneMinutes = (byte)((!string.IsNullOrEmpty(nmeaData[6]) && nmeaData[6].Length >= 5) ? byte.Parse(nmeaData[6].Substring(0, 2)) : 0);
		}
	}

	private void ExtractVTG(string[] nmeaData)
	{
		if (nmeaData.Length >= 9)
		{
			NmeaData.VTG.TrueTrackMadeGood = (string.IsNullOrEmpty(nmeaData[1]) ? 0.0 : double.Parse(nmeaData[1], CultureInfo.InvariantCulture));
			NmeaData.VTG.MagneticTrackMadeGood = (string.IsNullOrEmpty(nmeaData[3]) ? 0.0 : double.Parse(nmeaData[3], CultureInfo.InvariantCulture));
			NmeaData.VTG.GroundSpeedKnots = (string.IsNullOrEmpty(nmeaData[5]) ? 0.0 : double.Parse(nmeaData[5], CultureInfo.InvariantCulture));
			NmeaData.VTG.GroundSpeedKilometresPerHour = (string.IsNullOrEmpty(nmeaData[7]) ? 0.0 : double.Parse(nmeaData[7], CultureInfo.InvariantCulture));
			if (nmeaData.Length < 10)
			{
				NmeaData.VTG.ModeIndicator = NmeaModeIndicator.NoData;
			}
			else
			{
				NmeaData.VTG.ModeIndicator = (NmeaModeIndicator)((!string.IsNullOrEmpty(nmeaData[9])) ? nmeaData[9][0] : '\0');
			}
		}
	}

	private void ExtractGGA(string[] nmeaData)
	{
		if (nmeaData.Length >= 15)
		{
			NmeaData.GGA.UtcFixTime = (string.IsNullOrEmpty(nmeaData[1]) ? 0.0 : (double.Parse(nmeaData[1].Substring(0, 2), CultureInfo.InvariantCulture) * 3600.0 + double.Parse(nmeaData[1].Substring(2, 2), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[1].Substring(4), CultureInfo.InvariantCulture)));
			NmeaData.GGA.LatitudeMinutes = (string.IsNullOrEmpty(nmeaData[2]) ? 0.0 : ((double.Parse(nmeaData[2].Substring(0, 2), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[2].Substring(2), CultureInfo.InvariantCulture)) * (double)((!(nmeaData[3] == "S")) ? 1 : (-1))));
			NmeaData.GGA.LongitudeMinutes = (string.IsNullOrEmpty(nmeaData[4]) ? 0.0 : ((double.Parse(nmeaData[4].Substring(0, 3), CultureInfo.InvariantCulture) * 60.0 + double.Parse(nmeaData[4].Substring(3), CultureInfo.InvariantCulture)) * (double)((!(nmeaData[5] == "W")) ? 1 : (-1))));
			NmeaData.GGA.FixQuality = (NmeaFixQuality)((!string.IsNullOrEmpty(nmeaData[6])) ? nmeaData[6][0] : '\0');
			NmeaData.GGA.Satellites = (byte)((!string.IsNullOrEmpty(nmeaData[7])) ? byte.Parse(nmeaData[7]) : 0);
			NmeaData.GGA.HorizontalDilutionOfPosition = (string.IsNullOrEmpty(nmeaData[8]) ? 0.0 : double.Parse(nmeaData[8], CultureInfo.InvariantCulture));
			NmeaData.GGA.AltitudeAboveMeanSeaLevel = (string.IsNullOrEmpty(nmeaData[9]) ? 0.0 : double.Parse(nmeaData[9], CultureInfo.InvariantCulture));
			NmeaData.GGA.HeightOfGeoidAboveWGS84Eellipsoid = (string.IsNullOrEmpty(nmeaData[10]) ? 0.0 : double.Parse(nmeaData[10], CultureInfo.InvariantCulture));
		}
	}

	private void ExtractTXT(string[] nmeaData)
	{
		NmeaData.TXT.Data = new string[nmeaData.Length - 1];
		for (int i = 0; i < NmeaData.TXT.Data.Length; i++)
		{
			NmeaData.TXT.Data[i] = nmeaData[i + 1];
		}
	}

	private void ExtractRacelogicResponse()
	{
		if (CrcString.Count < 14)
		{
			if (RxCount >= 7)
			{
				for (int i = 0; i < 7; i++)
				{
					CrcString.Add(receivedDataBuffer[ReadIndex++]);
				}
				LengthOfMessage = 0;
				for (int j = 0; j < 4; j++)
				{
					LengthOfMessage <<= 8;
					LengthOfMessage |= CrcString[j + 10];
				}
				ReceivedMessage = new MessageSent((ushort)((ushort)(CrcString[7] << 8) | CrcString[8]), CrcString[9], 0, new byte[LengthOfMessage]);
				LengthOfMessage += 2;
				WaitForRacelogicResponse();
			}
		}
		else
		{
			WaitForRacelogicResponse();
		}
	}

	private void WaitForRacelogicResponse()
	{
		if (RxCount < LengthOfMessage)
		{
			return;
		}
		for (int i = 0; i < LengthOfMessage; i++)
		{
			CrcString.Add(receivedDataBuffer[ReadIndex++]);
		}
		SerialMessageStatus status;
		MessageSent message;
		byte[] data = CheckMessageOK(out status, out message);
		if (status == SerialMessageStatus.Ok)
		{
			switch (message.Command)
			{
			case 1:
				ProcessVBOXCommand(message, data);
				break;
			}
			_responseOk = true;
		}
		_waitingForResponse = false;
		HeaderFound = Header.None;
	}

	private byte[] CheckMessageOK(out SerialMessageStatus status, out MessageSent message)
	{
		byte[] array = null;
		status = SerialMessageStatus.UnexpectedResponseReceived;
		message = default(MessageSent);
		for (int i = 0; i < MessagesSent.Count; i++)
		{
			if (MessagesSent[i].Id == ReceivedMessage.Id)
			{
				message = new MessageSent(MessagesSent[i].Id, MessagesSent[i].Command, MessagesSent[i].SubCommand, MessagesSent[i].Payload);
				if (ReceivedMessage.Command != 128)
				{
					MessagesSent.Remove(MessagesSent[i]);
				}
				status = SerialMessageStatus.Ok;
				break;
			}
		}
		if (status == SerialMessageStatus.Ok)
		{
			try
			{
				Checksum.Check(CrcString, (uint)CrcString.Count, PolynomialUnitType.VBox);
				array = CrcString.ToArray();
				switch (ReceivedMessage.Command)
				{
				case 0:
					status = SerialMessageStatus.CommandNotRecognised;
					array = null;
					break;
				case 1:
					status = SerialMessageStatus.ErrorInCommand;
					try
					{
						string text2 = array.ToString();
						if (!text2.Contains("Checksum Error"))
						{
						}
					}
					catch
					{
					}
					array = null;
					break;
				case 2:
					status = SerialMessageStatus.CommandNotSupported;
					array = null;
					break;
				case 3:
				{
					status = SerialMessageStatus.ErrorInCommand;
					uint num = 0u;
					for (int j = 0; j < 4; j++)
					{
						num <<= 8;
						num |= array[j + 10];
					}
					uint num2 = 0u;
					for (int k = 0; k < num; k++)
					{
						num2 <<= 8;
						num2 |= array[14 + k];
					}
					string text3 = GlobalisedEnumConverter.ConvertToString((ErrorCode)num2);
					break;
				}
				case 254:
					status = SerialMessageStatus.ErrorInCommand;
					try
					{
						string text = array.ToString();
						if (!text.Contains("Checksum Error"))
						{
						}
					}
					catch
					{
					}
					array = null;
					break;
				case 128:
				case byte.MaxValue:
					array = RemoveResponseCode(array);
					break;
				}
			}
			catch (RacelogicCheckSumException)
			{
				status = SerialMessageStatus.CRCError;
			}
		}
		return array;
	}

	private byte[] RemoveResponseCode(byte[] data)
	{
		byte[] array = new byte[data.Length - 16];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = data[i + 14];
		}
		return array;
	}

	private void ExtractSetupResponse()
	{
		if (CommsDebugFile != null)
		{
			CommsDebugFile.WriteLine("ExtractSetupResponse()");
		}
		if (_awaitingStrResponse)
		{
			ExtractStrResponse();
		}
		else if (_awaitingStmResponse)
		{
			ExtractStmResponse();
		}
		else
		{
			if (_awaitingBootloaderResponse || _OldProtocolState == OldProtocolState.StopGps || SetupRxCount < _oldProtocolResponseLength || (_OldProtocolState & OldProtocolState.ReplyReceived) == OldProtocolState.ReplyReceived)
			{
				return;
			}
			using (ReturnedDataLock.Lock())
			{
				List<byte> list = new List<byte>(_oldProtocolResponseLength);
				for (int i = 0; i < _oldProtocolResponseLength; i++)
				{
					list.Add(setupReceivedDataBuffer[SetupReadIndex++]);
					VBoxData.UnrecognisedCharacter--;
				}
				if (_downloadingEEPROM)
				{
					_downloadingEEPROM = false;
					rxTime = DateTime.Now.Ticks;
				}
				if (_requestingVariableLengthResponse)
				{
					_requestedCommand = null;
					_requestedVBOXSubCommand = null;
					_oldProtocolResponseLength = int.MaxValue;
					_returnedData.Clear();
					foreach (byte item in list)
					{
						_returnedData.Add(item);
					}
				}
				else if (_oldProtocolResponseLength > 0)
				{
					uint num = Checksum.Calculate(list, (uint)(_oldProtocolResponseLength - 2), PolynomialUnitType.VBox);
					uint num2 = list[list.Count - 2];
					num2 <<= 8;
					num2 |= list[list.Count - 1];
					_returnedData.Clear();
					if (num2 == num)
					{
						if (_requestedCommand.HasValue)
						{
							switch (_requestedCommand.Value)
							{
							case 7:
								switch (_requestedVBOXSubCommand.Value)
								{
								case 4:
									if (list[0] == byte.MaxValue && list[1] == 6)
									{
										for (int m = 2; m < 8; m++)
										{
											_returnedData.Add(list[m]);
										}
									}
									break;
								case 6:
								{
									for (int n = 2; n < 10; n++)
									{
										_returnedData.Add(list[n]);
									}
									break;
								}
								case 7:
								case 10:
								case 21:
								case 22:
								case 30:
								case 32:
								case 51:
								case 52:
								case 54:
								case 55:
								case 58:
								case 60:
								case 64:
								case 123:
									if (list[0] == byte.MaxValue)
									{
										_returnedData.Add(list[1]);
									}
									break;
								case 8:
								case 56:
								case 59:
								case 124:
								case 125:
									if (list[0] == byte.MaxValue)
									{
										for (int k = 1; k < list.Count - 2; k++)
										{
											_returnedData.Add(list[k]);
										}
									}
									break;
								case 9:
								case 11:
								case 12:
								case 13:
								case 17:
								case 23:
								case 24:
								case 25:
								case 28:
								case 29:
								case 31:
								case 33:
								case 35:
								case 36:
								case 49:
								case 53:
								case 57:
									if (list[0] == byte.MaxValue && list[1] == 1)
									{
										for (int l = 2; l < list.Count - 2; l++)
										{
											_returnedData.Add(list[l]);
										}
									}
									break;
								case 66:
								case 67:
								{
									byte b = (byte)((_requestedVBOXSubCommand.Value == 67) ? 13 : 6);
									if (list[0] == byte.MaxValue && list[1] == 1 && list[2] == b)
									{
										for (int j = 3; j < list.Count - 2; j++)
										{
											_returnedData.Add(list[j]);
										}
									}
									break;
								}
								}
								break;
							}
						}
						else
						{
							switch (_requestedVBOXSubCommand.Value)
							{
							case 3:
							case 5:
							case 8:
							case 16:
							case 19:
							case 20:
							case 21:
							case 44:
							case 46:
								if (list[0] == byte.MaxValue)
								{
									_returnedData.Add(list[1]);
								}
								break;
							case 4:
								if (list[0] == byte.MaxValue && list[1] == 1)
								{
									int num4 = ((list[2] == 0) ? 256 : list[2]);
									for (int num5 = 0; num5 < num4; num5++)
									{
										_returnedData.Add(list[num5 + 3]);
									}
								}
								break;
							case 18:
								if (list[0] == byte.MaxValue && list[1] == 1)
								{
									_returnedData.Add(list[2]);
									_returnedData.Add(list[3]);
								}
								break;
							case 45:
								if (list[0] == byte.MaxValue && list[1] == 1)
								{
									for (int num3 = 0; num3 < 4096; num3++)
									{
										_returnedData.Add(list[num3 + 2]);
									}
								}
								break;
							}
						}
					}
					else
					{
						_crcError = true;
					}
				}
				_OldProtocolState |= OldProtocolState.ReplyReceived;
			}
		}
	}

	private void ExtractSpeedSensorData()
	{
		if (RxCount < 32)
		{
			return;
		}
		if (VerifyCheckSum(32))
		{
			doubleUnion doubleUnion = default(doubleUnion);
			int startIndex = 7;
			VBoxData.Satellites = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			uint num = (uint)ReadFromMessage(CrcString, ref startIndex, 3);
			double num2 = (double)num - OldVBoxTime;
			if (num2 > 0.0)
			{
				VBoxData.SampleRateHz = Math.Round(100.0 / num2, MidpointRounding.AwayFromZero);
			}
			else
			{
				VBoxData.SampleRateHz = 20.0;
			}
			OldVBoxTime = num;
			VBoxData.UtcTime = (double)num / 100.0;
			doubleUnion.b7_MSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b6 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b5 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b4 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b3 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b2 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b1 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b0_LSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.data = Maths.RadianToDegree(doubleUnion.data) * 60.0;
			VBoxData.LatitudeYMinutes = doubleUnion.data;
			doubleUnion.b7_MSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b6 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b5 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b4 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b3 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b2 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b1 = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.b0_LSB = (byte)ReadFromMessage(CrcString, ref startIndex, 1);
			doubleUnion.data = Maths.RadianToDegree(doubleUnion.data) * 60.0;
			VBoxData.LongitudeXMinutes = doubleUnion.data;
			num = (uint)ReadFromMessage(CrcString, ref startIndex, 2);
			VBoxData.SpeedKilometresPerHour = (double)num / (100.0 * SpeedConstants.KilometresPerHourToKnots);
			num = (uint)ReadFromMessage(CrcString, ref startIndex, 2);
			VBoxData.Heading = (double)num / 100.0;
			num = (uint)ReadFromMessage(CrcString, ref startIndex, 2);
			if ((num & 0x8000) == 32768)
			{
				VBoxData.VerticalVelocityKilometresPerHour = 65536 - num;
				VBoxData vBoxData = VBoxData;
				vBoxData.VerticalVelocityKilometresPerHour = (double)vBoxData.VerticalVelocityKilometresPerHour * -0.01;
			}
			else
			{
				VBoxData.VerticalVelocityKilometresPerHour = (double)num / 100.0;
			}
			VBoxData vBoxData2 = VBoxData;
			vBoxData2.VerticalVelocityKilometresPerHour = (double)vBoxData2.VerticalVelocityKilometresPerHour / SpeedConstants.KilometresPerHourToMetresPerSecond;
			short num3 = (short)ReadFromMessage(CrcString, ref startIndex, 2);
			VBoxData.LateralAccelerationG = (double)num3 / 100.0;
			num3 = (short)ReadFromMessage(CrcString, ref startIndex, 2);
			VBoxData.LongitudinalAccelerationG = (double)num3 / 100.0;
			AvailableVBoxData = VBoxChannel.Satellites | VBoxChannel.UtcTime | VBoxChannel.Latitude | VBoxChannel.Longitude | VBoxChannel.Speed | VBoxChannel.Heading | VBoxChannel.VerticalVelocity | VBoxChannel.LongitudinalAcceleration | VBoxChannel.LateralAcceleration;
			AvailableVBoxData2 = VBoxChannel2.None;
		}
		else
		{
			AvailableVBoxData = VBoxChannel.None;
			AvailableVBoxData2 = VBoxChannel2.None;
			VBoxData.Clear(clearCrcCount: false);
			VBoxData.CrcError++;
		}
		HeaderFound = Header.None;
		RaisePropertyChanged("VBoxData");
	}

	private void ExtractVB3isData()
	{
		AvailableVBoxData = VBoxChannel.Satellites | VBoxChannel.UtcTime | VBoxChannel.Latitude | VBoxChannel.Longitude | VBoxChannel.Speed | VBoxChannel.Heading | VBoxChannel.Height | VBoxChannel.VerticalVelocity | VBoxChannel.GlonassSatellites | VBoxChannel.GpsSatellites | VBoxChannel.VBox3Rms_VBMiniYaw | VBoxChannel.SolutionType | VBoxChannel.VelocityQuality | VBoxChannel.TriggerEventTime;
		AvailableVBoxData2 = VBoxChannel2.BeidouSatellites | VBoxChannel2.PitchAngleKf | VBoxChannel2.RollAngleKf | VBoxChannel2.HeadingKf | VBoxChannel2.PitchRateImu | VBoxChannel2.RollRateImu | VBoxChannel2.YawRateImu | VBoxChannel2.XAccelImu | VBoxChannel2.YAccelImu | VBoxChannel2.ZAccelImu | VBoxChannel2.Date | VBoxChannel2.PositionQuality | VBoxChannel2.T1 | VBoxChannel2.WheelSpeed1 | VBoxChannel2.WheelSpeed2 | VBoxChannel2.HeadingImu2;
		if (RxCount < 66)
		{
			return;
		}
		lock (ReplayDataLock)
		{
			if (!IsReplayingLoggedData)
			{
				VBoxData.Clear(clearCrcCount: false);
			}
		}
		if (VerifyCheckSum(66))
		{
			if (VBoxCommsLogger != null)
			{
				LogData(CrcString.ToArray());
			}
			if (!IsReplayingLoggedData)
			{
				UpdateVB3isData(CrcString, VBoxData);
			}
		}
		else
		{
			VBoxData.CrcError++;
		}
		HeaderFound = Header.None;
		RaisePropertyChanged("VBoxData");
	}

	internal void UpdateVB3isData(List<byte> data, VBoxData vBoxData)
	{
		int startIndex = 7;
		vBoxData.GpsSatellites = (byte)ReadFromMessage(data, ref startIndex, 1);
		vBoxData.GlonassSatellites = (byte)ReadFromMessage(data, ref startIndex, 1);
		vBoxData.BeidouSatellites = (byte)ReadFromMessage(data, ref startIndex, 1);
		vBoxData.Satellites = (byte)(vBoxData.GpsSatellites + vBoxData.GlonassSatellites + vBoxData.BeidouSatellites);
		uint u32 = (uint)ReadFromMessage(data, ref startIndex, 3, logRawData: true);
		double sampleRateHz = vBoxData.SampleRateHz;
		double num = (double)u32 - OldVBoxTime;
		if (num > 0.0)
		{
			vBoxData.SampleRateHz = Math.Round(100.0 / num, MidpointRounding.AwayFromZero);
		}
		else
		{
			vBoxData.SampleRateHz = 100.0;
		}
		if (sampleRateHz != 0.0 && Maths.Compare(vBoxData.SampleRateHz, sampleRateHz) != 0)
		{
			vBoxData.SampleError++;
		}
		OldVBoxTime = u32;
		vBoxData.UtcTime = (double)u32 / 100.0;
		int num2 = ReadFromMessage(data, ref startIndex, 4);
		vBoxData.LatitudeYMinutes = (double)num2 * 60.0 / 10000000.0;
		num2 = ReadFromMessage(data, ref startIndex, 4);
		vBoxData.LongitudeXMinutes = (double)num2 * 60.0 / 10000000.0;
		u32 = (uint)ReadFromMessage(data, ref startIndex, 3);
		vBoxData.SpeedKilometresPerHour = (double)u32 / 1000.0;
		u32 = (uint)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.Heading = (double)u32 / 100.0;
		vBoxData.HeightMetres = GetThreeByteSignedValue(100.0, logRawData: false);
		vBoxData.VerticalVelocityKilometresPerHour = GetThreeByteSignedValue(1000.0, logRawData: true) / SpeedConstants.KilometresPerHourToMetresPerSecond;
		vBoxData.SolutionType = (byte)ReadFromMessage(data, ref startIndex, 1);
		short num3 = (short)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.PitchAngleKf = (double)num3 / 100.0;
		num3 = (short)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.RollAngleKf = (double)num3 / 100.0;
		u32 = (uint)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.HeadingKf = (double)u32 / 100.0;
		num3 = (short)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.PitchRateImu = (double)num3 / 100.0;
		num3 = (short)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.RollRateImu = (double)num3 / 100.0;
		num3 = (short)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.YawRateImu = (double)num3 / 100.0;
		num3 = (short)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.XAccelImu = (double)num3 / 100.0 / AccelerationConstants.GToMetresPerSecondSquared;
		num3 = (short)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.YAccelImu = (double)num3 / 100.0 / AccelerationConstants.GToMetresPerSecondSquared;
		num3 = (short)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.ZAccelImu = (double)num3 / 100.0 / AccelerationConstants.GToMetresPerSecondSquared;
		ushort num4 = (ushort)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.Date = new DateTime(1980 + ((num4 & 0xFE00) >> 9), (num4 & 0x1E0) >> 5, num4 & 0x1F);
		u32 = (uint)ReadFromMessage(data, ref startIndex, 3);
		vBoxData.TriggerEventTimeSeconds = (double)u32 / 1000000.0;
		ExtractKalmanFilterStatus(vBoxData, data, ref startIndex);
		vBoxData.PositionQuality = (byte)ReadFromMessage(data, ref startIndex, 1);
		num4 = (ushort)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.SpeedQualityKilometersPerHour = (double)(int)num4 / 100.0 / SpeedConstants.KilometresPerHourToMetresPerSecond;
		num4 = (ushort)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.T1 = (double)(int)num4 / 10000000.0;
		u32 = (uint)ReadFromMessage(data, ref startIndex, 3);
		vBoxData.WheelSpeed1 = (double)u32 / 1000.0 / SpeedConstants.KilometresPerHourToMetresPerSecond;
		u32 = (uint)ReadFromMessage(data, ref startIndex, 3);
		vBoxData.WheelSpeed2 = (double)u32 / 1000.0 / SpeedConstants.KilometresPerHourToMetresPerSecond;
		u32 = (uint)ReadFromMessage(data, ref startIndex, 2);
		vBoxData.HeadingImu2 = (double)u32 / 100.0;
		double GetThreeByteSignedValue(double scale, bool logRawData)
		{
			double num5 = 0.0;
			u32 = (uint)ReadFromMessage(data, ref startIndex, 3, logRawData);
			if ((u32 & 0x800000) == 8388608)
			{
				num5 = 16777216 - u32;
				return num5 * (-1.0 / scale);
			}
			return (double)u32 / scale;
		}
	}

	public static int CalculateStandardChannelsMessageLength(VBoxType vboxType, VBoxChannel availableChannels)
	{
		int num = 0;
		if ((availableChannels & VBoxChannel.Satellites) == VBoxChannel.Satellites)
		{
			num++;
		}
		if ((availableChannels & VBoxChannel.UtcTime) == VBoxChannel.UtcTime)
		{
			num += 3;
		}
		if ((availableChannels & VBoxChannel.Latitude) == VBoxChannel.Latitude)
		{
			num += 4;
		}
		if ((availableChannels & VBoxChannel.Longitude) == VBoxChannel.Longitude)
		{
			num += 4;
		}
		if ((availableChannels & VBoxChannel.Speed) == VBoxChannel.Speed)
		{
			num += 2;
		}
		if ((availableChannels & VBoxChannel.Heading) == VBoxChannel.Heading)
		{
			num += 2;
		}
		if ((availableChannels & VBoxChannel.Height) == VBoxChannel.Height)
		{
			num += 3;
		}
		if ((availableChannels & VBoxChannel.VerticalVelocity) == VBoxChannel.VerticalVelocity)
		{
			num += 2;
		}
		if ((availableChannels & VBoxChannel.LongitudinalAcceleration) == VBoxChannel.LongitudinalAcceleration)
		{
			num += 2;
		}
		if ((availableChannels & VBoxChannel.LateralAcceleration) == VBoxChannel.LateralAcceleration)
		{
			num += 2;
		}
		if (vboxType != VBoxType.VBoxSport)
		{
			if ((availableChannels & VBoxChannel.BrakeDistance) == VBoxChannel.BrakeDistance)
			{
				num += 4;
			}
			if ((availableChannels & VBoxChannel.Distance) == VBoxChannel.Distance)
			{
				num += 4;
			}
			int num2 = 4;
			if (vboxType == VBoxType.VBoxII || vboxType == VBoxType.VB2Sx || vboxType == VBoxType.VB2100 || vboxType == VBoxType.VB2Sx2 || vboxType == VBoxType.VB2Sl || vboxType == VBoxType.VBSx10)
			{
				num2 = 2;
			}
			if ((availableChannels & VBoxChannel.InternalA2D1) == VBoxChannel.InternalA2D1)
			{
				num += num2;
			}
			if ((availableChannels & VBoxChannel.InternalA2D2) == VBoxChannel.InternalA2D2)
			{
				num += num2;
			}
			if ((availableChannels & VBoxChannel.InternalA2D3) == VBoxChannel.InternalA2D3)
			{
				num += num2;
			}
			if ((availableChannels & VBoxChannel.InternalA2D4) == VBoxChannel.InternalA2D4)
			{
				num += num2;
			}
			if ((availableChannels & VBoxChannel.GlonassSatellites) == VBoxChannel.GlonassSatellites)
			{
				num++;
			}
			if ((availableChannels & VBoxChannel.GpsSatellites) == VBoxChannel.GpsSatellites)
			{
				num++;
			}
			if ((availableChannels & VBoxChannel.Yaw01YawRate) == VBoxChannel.Yaw01YawRate)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.Yaw01LateralAcceleration) == VBoxChannel.Yaw01LateralAcceleration)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.Yaw01Status) == VBoxChannel.Yaw01Status)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.Drift) == VBoxChannel.Drift)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.VBox3Rms_VBMiniYaw) == VBoxChannel.VBox3Rms_VBMiniYaw)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.SolutionType) == VBoxChannel.SolutionType)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.VelocityQuality) == VBoxChannel.VelocityQuality)
			{
				num += 4;
			}
			if ((availableChannels & VBoxChannel.InternalTemperature) == VBoxChannel.InternalTemperature)
			{
				num += 4;
			}
			if ((availableChannels & VBoxChannel.CompactFlashBufferSize) == VBoxChannel.CompactFlashBufferSize)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.MemoryUsed) == VBoxChannel.MemoryUsed)
			{
				num += 3;
			}
			if ((availableChannels & VBoxChannel.TriggerEventTime) == VBoxChannel.TriggerEventTime)
			{
				num = ((vboxType != VBoxType.VBoxII && vboxType != VBoxType.VB2Sx) ? (num + 4) : (num + 2));
			}
			if ((availableChannels & VBoxChannel.Event2Time) == VBoxChannel.Event2Time)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.Battery1Voltage) == VBoxChannel.Battery1Voltage)
			{
				num += 2;
			}
			if ((availableChannels & VBoxChannel.Battery2Voltage) == VBoxChannel.Battery2Voltage)
			{
				num += 2;
			}
		}
		return num;
	}

	private void ExtractVBoxData()
	{
		if (CrcString.Count < 17)
		{
			if (RxCount >= 10)
			{
				for (int i = 0; i < 10; i++)
				{
					CrcString.Add(receivedDataBuffer[ReadIndex++]);
				}
				uint num = 0u;
				uint num2 = 0u;
				for (int j = 0; j < 4; j++)
				{
					num <<= 8;
					num |= CrcString[8 + j];
					num2 <<= 8;
					num2 |= CrcString[12 + j];
				}
				AvailableVBoxData = (VBoxChannel)num;
				if ((AvailableVBoxData & VBoxChannel.Satellites) == VBoxChannel.Satellites)
				{
					AvailableVBoxData |= VBoxChannel.Dgps;
					AvailableVBoxData |= VBoxChannel.DualAntenna;
					AvailableVBoxData |= VBoxChannel.BrakeTrigger;
				}
				AvailableVBoxData2 = VBoxChannel2.None;
				LengthOfMessage = CalculateMessageLength(AvailableVBoxData, num2);
				WaitForVBoxData();
			}
		}
		else
		{
			WaitForVBoxData();
		}
	}

	private int CalculateMessageLength(VBoxChannel availableChannels, uint oldStyleAvailableCanChannels)
	{
		int num = 2;
		num += CalculateStandardChannelsMessageLength(VBoxType, availableChannels);
		if (VBoxType == VBoxType.VBoxSport)
		{
			if ((oldStyleAvailableCanChannels & 1) == 1)
			{
				num += 2;
			}
			if ((oldStyleAvailableCanChannels & 0x10) == 16)
			{
				num += 4;
			}
			if ((oldStyleAvailableCanChannels & 0x20) == 32)
			{
				num += 4;
			}
			if ((oldStyleAvailableCanChannels & 0x40) == 64)
			{
				num += 2;
			}
		}
		else if (VBoxType != VBoxType.VideoVBox)
		{
			for (int i = 0; i < 32; i++)
			{
				if ((oldStyleAvailableCanChannels & (1 << i)) == 1 << i)
				{
					num = ((i >= 8) ? ((i >= 24) ? (num + 4) : (num + 2)) : (num + 1));
				}
			}
		}
		return num;
	}

	private void WaitForVBoxData()
	{
		if (RxCount < LengthOfMessage)
		{
			return;
		}
		lock (ReplayDataLock)
		{
			if (!IsReplayingLoggedData)
			{
				VBoxData.Clear(clearCrcCount: false);
			}
		}
		if (VerifyCheckSum(LengthOfMessage))
		{
			if (VBoxCommsLogger != null)
			{
				LogData(CrcString.ToArray());
			}
			if (!IsReplayingLoggedData)
			{
				UpdateVBoxData(CrcString, VBoxData, AvailableVBoxData);
			}
		}
		else
		{
			VBoxData.CrcError++;
		}
		HeaderFound = Header.None;
		RaisePropertyChanged("VBoxData");
	}

	internal void UpdateVBoxData(List<byte> data, VBoxData vboxData, VBoxChannel availableChannels)
	{
		double sampleRateHz = vboxData.SampleRateHz;
		int startIndex = 17;
		if ((availableChannels & VBoxChannel.Satellites) == VBoxChannel.Satellites)
		{
			byte b = (byte)ReadFromMessage(data, ref startIndex, 1);
			vboxData.Dgps = (b & 0x80) == 128;
			b = (byte)(b & 0x7Fu);
			vboxData.BrakeTrigger = (b & 0x40) == 64;
			b = (byte)(b & 0xBFu);
			vboxData.DualAntenna = (b & 0x20) == 32;
			vboxData.Satellites = (byte)(b & 0xDFu);
		}
		if ((availableChannels & VBoxChannel.UtcTime) == VBoxChannel.UtcTime)
		{
			uint num = (uint)ReadFromMessage(data, ref startIndex, 3);
			double num2 = (double)num - OldVBoxTime;
			if (num2 > 0.0)
			{
				vboxData.SampleRateHz = Math.Round(100.0 / num2, MidpointRounding.AwayFromZero);
			}
			else
			{
				vboxData.SampleRateHz = 20.0;
			}
			if (sampleRateHz != 0.0 && Maths.Compare(vboxData.SampleRateHz, sampleRateHz) != 0)
			{
				vboxData.SampleError++;
			}
			OldVBoxTime = num;
			vboxData.UtcTime = (double)num / 100.0;
		}
		int num3 = 0;
		if ((availableChannels & VBoxChannel.Latitude) == VBoxChannel.Latitude)
		{
			num3 = ReadFromMessage(data, ref startIndex, 4);
		}
		int num4 = 0;
		if ((availableChannels & VBoxChannel.Longitude) == VBoxChannel.Longitude)
		{
			num4 = ReadFromMessage(data, ref startIndex, 4);
		}
		if (vboxData.VBoxType == VBoxType.VBox3 || vboxData.VBoxType == VBoxType.VBox3i || vboxData.VBoxType == VBoxType.VB3Tr2 || vboxData.VBoxType == VBoxType.VB3Tr3 || vboxData.VBoxType == VBoxType.VideoVBox || VBoxData.VBoxType == VBoxType.VBoxSport)
		{
			vboxData.LatitudeYMinutes = (double)num3 / 100000.0;
			vboxData.LongitudeXMinutes = (double)num4 / 100000.0;
		}
		else
		{
			vboxData.LatitudeYMinutes = VBOXIILatLong(num3);
			vboxData.LongitudeXMinutes = VBOXIILatLong(num4);
		}
		vboxData.LongitudeXMinutes = (double)vboxData.LongitudeXMinutes * -1.0;
		if ((availableChannels & VBoxChannel.Speed) == VBoxChannel.Speed)
		{
			uint num = (uint)ReadFromMessage(data, ref startIndex, 2);
			vboxData.SpeedKilometresPerHour = (double)num / 100.0 / SpeedConstants.KilometresPerHourToKnots;
		}
		if ((availableChannels & VBoxChannel.Heading) == VBoxChannel.Heading)
		{
			uint num = (uint)ReadFromMessage(data, ref startIndex, 2);
			vboxData.Heading = (double)num / 100.0;
		}
		if ((availableChannels & VBoxChannel.Height) == VBoxChannel.Height)
		{
			uint num = (uint)ReadFromMessage(data, ref startIndex, 3);
			if ((num & 0x800000) == 8388608)
			{
				vboxData.HeightMetres = 16777216 - num;
				vboxData.HeightMetres = (double)vboxData.HeightMetres * 0.0;
			}
			else
			{
				vboxData.HeightMetres = (float)num / 100f;
			}
		}
		if ((availableChannels & VBoxChannel.VerticalVelocity) == VBoxChannel.VerticalVelocity)
		{
			short num5 = (short)ReadFromMessage(data, ref startIndex, 2, logRawData: true);
			vboxData.VerticalVelocityKilometresPerHour = (double)num5 / 100.0;
			VBoxData vBoxData = VBoxData;
			vBoxData.VerticalVelocityKilometresPerHour = (double)vBoxData.VerticalVelocityKilometresPerHour / SpeedConstants.KilometresPerHourToMetresPerSecond;
		}
		if ((availableChannels & VBoxChannel.LongitudinalAcceleration) == VBoxChannel.LongitudinalAcceleration)
		{
			short num5 = (short)ReadFromMessage(data, ref startIndex, 2);
			vboxData.LongitudinalAccelerationG = (double)num5 / 100.0;
		}
		if ((availableChannels & VBoxChannel.LateralAcceleration) == VBoxChannel.LateralAcceleration)
		{
			short num5 = (short)ReadFromMessage(data, ref startIndex, 2);
			vboxData.LateralAccelerationG = (double)num5 / 100.0;
		}
		if (vboxData.VBoxType != VBoxType.VBoxSport)
		{
			if ((availableChannels & VBoxChannel.BrakeDistance) == VBoxChannel.BrakeDistance)
			{
				ReadFromMessage(data, ref startIndex, 4);
			}
			if ((availableChannels & VBoxChannel.Distance) == VBoxChannel.Distance)
			{
				ReadFromMessage(data, ref startIndex, 4);
			}
			int count = 4;
			if (VBoxType == VBoxType.VBoxII || VBoxType == VBoxType.VB2Sx || VBoxType == VBoxType.VB2100 || VBoxType == VBoxType.VB2Sx2 || VBoxType == VBoxType.VB2Sl || VBoxType == VBoxType.VBSx10)
			{
				count = 2;
			}
			if ((availableChannels & VBoxChannel.InternalA2D1) == VBoxChannel.InternalA2D1)
			{
				uint num = (uint)ReadFromMessage(data, ref startIndex, count);
				Union union = default(Union);
				union.temp = (int)num;
				Union union2 = union;
				vboxData.InternalA2D[0].Value = union2.data;
				vboxData.InternalA2D[0].IsBeingSentOverSerial = true;
			}
			if ((availableChannels & VBoxChannel.InternalA2D2) == VBoxChannel.InternalA2D2)
			{
				uint num = (uint)ReadFromMessage(data, ref startIndex, count);
				Union union = default(Union);
				union.temp = (int)num;
				Union union2 = union;
				vboxData.InternalA2D[1].Value = union2.data;
				vboxData.InternalA2D[1].IsBeingSentOverSerial = true;
			}
			if ((availableChannels & VBoxChannel.InternalA2D3) == VBoxChannel.InternalA2D3)
			{
				uint num = (uint)ReadFromMessage(data, ref startIndex, count);
				Union union = default(Union);
				union.temp = (int)num;
				Union union2 = union;
				vboxData.InternalA2D[2].Value = union2.data;
				vboxData.InternalA2D[2].IsBeingSentOverSerial = true;
			}
			if ((availableChannels & VBoxChannel.InternalA2D4) == VBoxChannel.InternalA2D4)
			{
				uint num = (uint)ReadFromMessage(data, ref startIndex, count);
				Union union = default(Union);
				union.temp = (int)num;
				Union union2 = union;
				vboxData.InternalA2D[3].Value = union2.data;
				vboxData.InternalA2D[3].IsBeingSentOverSerial = true;
			}
			if ((availableChannels & VBoxChannel.GlonassSatellites) == VBoxChannel.GlonassSatellites)
			{
				vboxData.GlonassSatellites = (byte)ReadFromMessage(data, ref startIndex, 1);
			}
			if ((availableChannels & VBoxChannel.GpsSatellites) == VBoxChannel.GpsSatellites)
			{
				vboxData.GpsSatellites = (byte)ReadFromMessage(data, ref startIndex, 1);
			}
			if ((availableChannels & VBoxChannel.Yaw01YawRate) == VBoxChannel.Yaw01YawRate)
			{
				ReadFromMessage(data, ref startIndex, 2);
			}
			if ((availableChannels & VBoxChannel.Yaw01LateralAcceleration) == VBoxChannel.Yaw01LateralAcceleration)
			{
				ReadFromMessage(data, ref startIndex, 2);
			}
			if ((availableChannels & VBoxChannel.Yaw01Status) == VBoxChannel.Yaw01Status)
			{
				ReadFromMessage(data, ref startIndex, 2);
			}
			if ((availableChannels & VBoxChannel.Drift) == VBoxChannel.Drift)
			{
				short num5 = (short)ReadFromMessage(data, ref startIndex, 2);
				if (VBoxType == VBoxType.VBoxMini)
				{
					vboxData.Drift = (double)num5 / 100.0;
				}
				else
				{
					vboxData.SerialNumber = num5;
				}
			}
			string status;
			string extraInformation;
			if ((availableChannels & VBoxChannel.VBox3Rms_VBMiniYaw) == VBoxChannel.VBox3Rms_VBMiniYaw)
			{
				vboxData.KalmanFilterCode = -1;
				if (VBoxType == VBoxType.VBox3i)
				{
					ExtractKalmanFilterStatus(vboxData, data, ref startIndex);
				}
				else
				{
					VBoxData.GetKalmanFilterStatusAsString(-1, out status, out extraInformation);
					if (VBoxType == VBoxType.VBoxMini)
					{
						ReadFromMessage(data, ref startIndex, 2);
					}
				}
			}
			else if (VBoxType == VBoxType.VBox3i)
			{
				vboxData.KalmanFilterStatus = Racelogic.DataSource.Resources.KalmanFilterStatus_NoData;
				vboxData.KalmanFilterStatusExtraInformation = Racelogic.DataSource.Resources.KalmanFilterStatusExtraInformation_SendOverSerialNotSelected;
			}
			else
			{
				VBoxData.GetKalmanFilterStatusAsString(-1, out status, out extraInformation);
			}
			if ((availableChannels & VBoxChannel.SolutionType) == VBoxChannel.SolutionType)
			{
				vboxData.SolutionType = (short)ReadFromMessage(data, ref startIndex, 2);
			}
			if ((availableChannels & VBoxChannel.VelocityQuality) == VBoxChannel.VelocityQuality)
			{
				uint num = (uint)ReadFromMessage(data, ref startIndex, 4);
				vboxData.SpeedQualityKilometersPerHour = (double)num / 100.0 / SpeedConstants.KilometresPerHourToKnots;
			}
			if ((availableChannels & VBoxChannel.InternalTemperature) == VBoxChannel.InternalTemperature)
			{
				ReadFromMessage(data, ref startIndex, 4);
			}
			if ((availableChannels & VBoxChannel.CompactFlashBufferSize) == VBoxChannel.CompactFlashBufferSize)
			{
				ReadFromMessage(data, ref startIndex, 2);
			}
			if ((availableChannels & VBoxChannel.MemoryUsed) == VBoxChannel.MemoryUsed)
			{
				uint num = (uint)ReadFromMessage(data, ref startIndex, 3);
				double num2 = 100.0 * (double)num / 980991.0;
				if (num2 > 100.0)
				{
					num2 = 100.0;
				}
				vboxData.MemoryUsed = num2;
			}
			if ((availableChannels & VBoxChannel.TriggerEventTime) == VBoxChannel.TriggerEventTime)
			{
				uint num = (uint)((VBoxType != VBoxType.VBoxII && VBoxType != VBoxType.VB2Sx) ? ReadFromMessage(data, ref startIndex, 4) : ReadFromMessage(data, ref startIndex, 2));
				if (VBoxType == VBoxType.VBox3 || VBoxType == VBoxType.VBox3i || VBoxType == VBoxType.VB3Tr2 || VBoxType == VBoxType.VB3Tr3 || VBoxType == VBoxType.VideoVBox || VBoxType == VBoxType.VBoxMini)
				{
					Union union = default(Union);
					union.temp = (int)num;
					Union union2 = union;
					vboxData.TriggerEventTimeSeconds = union2.data;
				}
				else
				{
					vboxData.TriggerEventTimeSeconds = num;
					if (VBoxType != VBoxType.VBoxII)
					{
						vboxData.TriggerEventTimeSeconds = (double)vboxData.TriggerEventTimeSeconds / 100000.0;
					}
				}
			}
			if ((availableChannels & VBoxChannel.Event2Time) == VBoxChannel.Event2Time)
			{
				ReadFromMessage(data, ref startIndex, 2);
			}
			if ((availableChannels & VBoxChannel.Battery1Voltage) == VBoxChannel.Battery1Voltage)
			{
				ReadFromMessage(data, ref startIndex, 2);
			}
			if ((availableChannels & VBoxChannel.Battery2Voltage) == VBoxChannel.Battery2Voltage)
			{
				ReadFromMessage(data, ref startIndex, 2);
			}
			if (vBoxType == VBoxType.VB2Sl && vboxData.DualAntenna != oldDualAntenna)
			{
				GpsLatency = RetrieveGpsLatency(vBoxType, vboxData.DualAntenna);
			}
			oldDualAntenna = vboxData.DualAntenna;
		}
		else
		{
			ReadFromMessage(data, ref startIndex, 2);
			uint num = (uint)ReadFromMessage(data, ref startIndex, 4);
			double num2 = Convert.ToDouble(num);
			num = (uint)ReadFromMessage(data, ref startIndex, 4);
			VBoxData.MemoryUsed = 100.0 - (double)num * 100.0 / num2;
			ReadFromMessage(data, ref startIndex, 2);
			VBoxData.MemoryUsed = 100.0;
		}
	}

	private void ExtractKalmanFilterStatus(VBoxData vboxData, List<byte> data, ref int startIndex)
	{
		vboxData.KalmanFilterCode = (short)((short)ReadFromMessage(data, ref startIndex, 2) & 0xFFF);
		VBoxData.GetKalmanFilterStatusAsString(vboxData.KalmanFilterCode, out var status, out var extraInformation);
		vboxData.KalmanFilterStatus = status;
		vboxData.KalmanFilterStatusExtraInformation = extraInformation;
	}

	private double RetrieveGpsLatency(VBoxType vBoxType, bool rtkLock)
	{
		byte revision = 0;
		GpsEngineType gpsEngine = GpsEngineType.Unknown;
		if (vBoxType == VBoxType.VB2Sx || vBoxType == VBoxType.VB2Sx2)
		{
			SetQuiet_StandardProtocol(MakeQuiet: true);
			if (!GetRevision(VBoxSubCommand.ReportRevision, out revision))
			{
				vBoxType = VBoxType.Unknown;
			}
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		else if (vBoxType == VBoxType.VBSx10)
		{
			SetQuiet_StandardProtocol(MakeQuiet: true);
			gpsEngine = GetSxSlEngineType();
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		return VBoxData.GetGpsLatency(vBoxType, gpsEngine, revision, rtkLock);
	}

	private double VBOXIILatLong(int value)
	{
		if ((value & 0x80000000u) == 2147483648u)
		{
			value &= 0x7FFFFFFF;
			value *= -1;
		}
		double num = (double)value / 100000.0;
		value = (int)num;
		value /= 100;
		num -= (double)(value * 100);
		value *= 60;
		num += (double)value;
		if (VBoxType == VBoxType.VBoxMini)
		{
			num *= -1.0;
		}
		return num;
	}

	internal void AddDefaultErrorHandler()
	{
	}

	internal void SubscribeToDataReceived()
	{
		Settings.Port.add_Received((EventHandler<DataEventArgs>)OnDataReceived);
	}

	internal void SubscribeToErrorReceived()
	{
	}

	internal void UnSubscribeToDataReceived()
	{
		Settings.Port.remove_Received((EventHandler<DataEventArgs>)OnDataReceived);
	}

	internal void UnSubscribeToErrorReceived()
	{
	}

	internal byte[] Read(int count)
	{
		byte[] array = new byte[count];
		return Encoding.ASCII.GetBytes(Settings.Port.ReadBytes(count, 60));
	}

	internal void Write(byte[] data)
	{
		Settings.Port.SendByteArray(data);
	}

	private void OnDataReceived(object sender, DataEventArgs e)
	{
		NoCommsTimer.Stop();
		UnitDisconnectedTimer.Enabled = false;
		lock (ClosePortLock)
		{
			byte[] array = Settings.Port.ReadBuffer();
			int num = ((array != null) ? array.Length : 0);
			while (Settings.IsOpen && num > 0)
			{
				if (num > 131072)
				{
					Settings.DiscardInBuffer();
				}
				else if (writeIndex + num > 131072)
				{
					int num2 = writeIndex + num - 131072;
					Buffer.BlockCopy(array, 0, receivedDataBuffer, writeIndex, num - num2);
					Buffer.BlockCopy(array, num - num2, receivedDataBuffer, 0, num2);
					WriteIndex = num2;
				}
				else
				{
					Buffer.BlockCopy(array, 0, receivedDataBuffer, writeIndex, num);
					WriteIndex += num;
				}
				array = Settings.Port.ReadBuffer();
				num = ((array != null) ? array.Length : 0);
			}
		}
		if (CanChannelInformationState == CanChannelInformationStatus.Requested)
		{
			CanChannelInformationState = CanChannelInformationStatus.AwaitingResponse;
			Task.Factory.StartNew(delegate
			{
				RequestCanChannelInformation();
			});
		}
		if (CommsDebugFile != null)
		{
			CommsDebugFile.WriteLine($"RxCount : {RxCount}");
		}
		ParserResetEvent.Set();
		NoCommsTimer.Interval = 5000.0;
		NoCommsTimer.Start();
	}

	private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
	{
		Console.WriteLine("Error");
	}

	private void LogData(byte[] dataToLog)
	{
		VBoxCommsLogger.Write(dataToLog);
		VboLoggerBytesWritten += dataToLog.Length;
		if (VboLoggerBytesWritten >= 512)
		{
			VBoxCommsLogger.Flush();
			VboLoggerBytesWritten = 0;
		}
	}

	private void ParseData()
	{
		while (ParsingRequired)
		{
			_IsReceivingComms = false;
			try
			{
				ParserResetEvent.WaitOne();
			}
			catch (ThreadAbortException)
			{
			}
			_IsReceivingComms = true;
			try
			{
				int num = -1;
				int num2 = -1;
				while (RxCount != num || SetupRxCount != num2)
				{
					num = RxCount;
					if (HeaderFound == Header.None)
					{
						HeaderFound = DetectHeader();
					}
					num2 = SetupRxCount;
					if (HeaderFound != 0)
					{
						IsReceivingVBoxComms = true;
						switch (HeaderFound)
						{
						case Header.VBox:
							switch (DetectedUnit)
							{
							case VBoxType.Unknown:
							case VBoxType.VB2100:
								ExtractSpeedSensorData();
								break;
							case VBoxType.BrakeTestSpeedSensor:
								ExtractBrakeTestSpeedSensorData();
								break;
							case VBoxType.VB3is:
								ExtractVB3isData();
								break;
							default:
								ExtractVBoxData();
								break;
							}
							break;
						case Header.Nmea:
							ExtractNmeaData();
							break;
						case Header.NewCan:
							ExtractNewCanData();
							break;
						case Header.NewPosition:
							ExtractNewPositionData();
							break;
						case Header.RacelogicResponse:
							ExtractRacelogicResponse();
							break;
						}
					}
					else if (SetupRxCount > 0)
					{
						ExtractSetupResponse();
					}
				}
			}
			catch (Exception ex2)
			{
				Console.WriteLine(ex2.Message);
			}
		}
	}

	private int ReadFromMessage(List<byte> data, ref int startIndex, int count, bool logRawData = false)
	{
		int num = 0;
		while (count-- > 0)
		{
			num <<= 8;
			num |= data[startIndex++];
		}
		return num;
	}

	private bool VerifyCheckSum(int messageLength)
	{
		bool flag = true;
		VBoxData.VBoxType = VBoxType;
		for (int i = 0; i < messageLength; i++)
		{
			CrcString.Add(receivedDataBuffer[ReadIndex++]);
		}
		if (VBoxType == VBoxType.VBoxSport)
		{
			flag = receivedDataBuffer[ReadIndex++] == 13 && receivedDataBuffer[ReadIndex++] == 10;
		}
		return flag & VerifyCheckSum(CrcString);
	}

	private bool VerifyCheckSum(List<byte> data)
	{
		bool result = false;
		if (data.Count > 2)
		{
			uint num = Checksum.Calculate(data, (uint)(data.Count - 2), PolynomialUnitType.VBox);
			uint num2 = 0u;
			num2 = data[data.Count - 2];
			num2 <<= 8;
			num2 |= data[data.Count - 1];
			result = num == num2;
		}
		return result;
	}

	public bool Close()
	{
		return CloseAPort();
	}

	public bool Open()
	{
		return OpenAPort(checkPortOnly: false);
	}

	public bool Open(string portName)
	{
		if (Settings.IsOpen)
		{
			CloseAPort();
		}
		Settings.PortName = portName;
		return OpenAPort(checkPortOnly: false);
	}

	public bool Open(string portName, bool checkPortOnly)
	{
		if (checkPortOnly)
		{
			portNameTemp = portName;
		}
		else
		{
			Settings.PortName = portName;
		}
		return OpenAPort(checkPortOnly);
	}

	private bool CanExecuteClosePort(object param)
	{
		return IsOpen;
	}

	private bool CanExecuteOpenPort(object param)
	{
		bool result = false;
		if (param is string)
		{
			List<AvailablePort> list = new List<AvailablePort>(availablePorts);
			foreach (AvailablePort item in list)
			{
				if (string.Equals(item.Name, (string)param))
				{
					result = item.IsAvailable;
					break;
				}
			}
		}
		return result;
	}

	private bool CloseAPort()
	{
		bool result = true;
		VBoxType = VBoxType.Unknown;
		if (IsOpen)
		{
			try
			{
				UnSubscribeToDataReceived();
				UnSubscribeToErrorReceived();
				while (RxCount > 0)
				{
					FlushRxTx();
				}
				lock (ClosePortLock)
				{
					Settings.Port.Close();
				}
				if (!IsRefreshingPortsList)
				{
					RaisePropertyChangedOnUi("IsOpen");
				}
			}
			catch (Exception)
			{
				result = false;
			}
		}
		return result;
	}

	private void ExecuteClosePort(object param)
	{
		CloseAPort();
	}

	private void ExecuteOpenPort(object param)
	{
		Open((string)param);
	}

	private bool OpenAPort(bool checkPortOnly)
	{
		bool flag = true;
		try
		{
			if (IsOpen)
			{
				CloseAPort();
			}
			string text = (checkPortOnly ? portNameTemp : Settings.PortName);
			if (string.IsNullOrEmpty(text))
			{
				if (!checkPortOnly && Application.Current != null)
				{
					Application.Current.Dispatcher.BeginInvoke(DisplaySlidingMessageAction, "Racelogic.Comms.LowLevel.OpenPort(): " + Racelogic.Comms.Serial.Properties.Resources.PortNameIsNull, Racelogic.Comms.Serial.Properties.Resources.Information, null);
				}
				flag = false;
			}
			else
			{
				bool flag2 = false;
				string[] portNames = SerialPort.GetPortNames();
				string[] array = portNames;
				foreach (string text2 in array)
				{
					if (text2 == text)
					{
						flag2 = true;
						Settings.CheckPortAndSetFields(text);
						break;
					}
				}
				if (!flag2)
				{
					flag = false;
				}
				else
				{
					int num = 0;
					flag = false;
					while (num++ < 5 && !flag)
					{
						Settings.Port.Open();
						if (!checkPortOnly)
						{
							flag = IsOpen;
							if (flag)
							{
								SubscribeToDataReceived();
								SubscribeToErrorReceived();
								RaisePropertyChangedOnUi("IsOpen");
							}
							else
							{
								Thread.Sleep(200);
							}
						}
						else
						{
							flag = true;
						}
					}
					if (!flag)
					{
						DisplayUnableToOpenPortMessage(checkPortOnly, "");
					}
				}
			}
		}
		catch (Exception ex)
		{
			flag = false;
			DisplayUnableToOpenPortMessage(checkPortOnly, ex.Message);
		}
		if (flag)
		{
			FlushRxTx();
		}
		return flag;
	}

	private void DisplayUnableToOpenPortMessage(bool checkPortOnly, string msg)
	{
		if (!checkPortOnly && Application.Current != null)
		{
			Application.Current.Dispatcher.BeginInvoke(DisplaySlidingMessageAction, "Racelogic.Comms.LowLevel.OpenPort(): " + Racelogic.Comms.Serial.Properties.Resources.UnableToOpenPort + (string.IsNullOrEmpty(msg) ? "" : (Environment.NewLine + msg)), Racelogic.Comms.Serial.Properties.Resources.Error, null);
		}
	}

	private void SetupModuleIdentifier(int serialNumber, int channelNumber, ReadWriteConfig readWrite, CanModuleConfigBits configBit, out int replyId, out int replyIdTemp)
	{
		replyId = serialNumber << 11;
		if (configBit == CanModuleConfigBits.SendData)
		{
			channelNumber &= 0xFFFE;
		}
		replyId |= channelNumber;
		replyIdTemp = replyId | (int)(((uint)configBit | (uint)readWrite) << 4);
	}

	private bool SendCanData(SendCanCommand command, int identifier, bool isExtended, int responseLength, byte[] data, int retryCount)
	{
		bool flag = false;
		while (!flag && retryCount-- > 0)
		{
			flag = SendCanData(command, identifier, isExtended, responseLength, data);
		}
		return flag;
	}

	private bool SendCanData(SendCanCommand command, int identifier, bool isExtended, int responseLength, byte[] data)
	{
		bool flag = false;
		Queue<byte> queue = new Queue<byte>();
		if (command == SendCanCommand.PlaceDataOntoCanBus)
		{
			if (isExtended)
			{
				queue.Enqueue((byte)((uint)data.Length | 0x80u));
			}
			else
			{
				queue.Enqueue((byte)data.Length);
			}
		}
		for (int i = 0; i < 4; i++)
		{
			queue.Enqueue((byte)(identifier >> 8 * i));
		}
		if (command == SendCanCommand.PlaceDataOntoCanBus)
		{
			foreach (byte item in data)
			{
				queue.Enqueue(item);
			}
		}
		int rxTimeout = ((responseLength == 29) ? 5000 : 3000);
		if (RequestVBOXData(7, (int)command, queue, responseLength, rxTimeout, ShowError: false, "SendCanData"))
		{
			using (ReturnedDataLock.Lock())
			{
				if (command == SendCanCommand.RequestDataFromCanBus)
				{
					flag = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
				else
				{
					switch (responseLength)
					{
					case 29:
						if (_returnedData.Count == 25)
						{
							_returnedData.RemoveRange(0, 3);
							byte[] array = new byte[12];
							_returnedData.CopyTo(0, array, 0, 12);
							string a = Racelogic.Core.Helper.ByteArrayToString(array);
							array = null;
							if (string.Equals(a, "$VBOXII,CAN,", StringComparison.OrdinalIgnoreCase))
							{
								IsReceivingVBoxComms = false;
								_returnedData.RemoveRange(0, 12);
								int num = _returnedData[0] & -129;
								_returnedData.RemoveRange(0, 2);
								_returnedData.RemoveRange(num, _returnedData.Count - num);
								flag = true;
							}
						}
						break;
					case 5:
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_returnedData.Clear();
				}
			}
		}
		return flag;
	}

	public bool RollCall()
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = new Queue<byte>();
			queue.Enqueue(129);
			queue.Enqueue(byte.MaxValue);
			queue.Enqueue(7);
			queue.Enqueue(0);
			queue.Enqueue(0);
			queue.Enqueue(1);
			RequestVBOXData(7, 33, queue, 5, 500, ShowError: false, "RollCall");
		}
		if (flag)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		return result;
	}

	public bool CanModuleReadWriteChannelData(int serialNumber, int channelNumber, ReadWriteConfig readWrite, CanModuleConfigBits configBit, List<byte> data)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			SetupModuleIdentifier(serialNumber, channelNumber, readWrite, configBit, out var _, out var replyIdTemp);
			if (SendCanData(SendCanCommand.RequestDataFromCanBus, replyIdTemp, isExtended: false, 4, data.ToArray(), 2))
			{
				if (SendCanData(SendCanCommand.PlaceDataOntoCanBus, replyIdTemp | 0x400, isExtended: true, 29, data.ToArray(), 3))
				{
					data.Clear();
					using (ReturnedDataLock.Lock())
					{
						foreach (byte returnedDatum in _returnedData)
						{
							data.Add(returnedDatum);
						}
					}
					result = true;
				}
				else
				{
					SystemSounds.Beep.Play();
				}
			}
			else
			{
				SystemSounds.Beep.Play();
			}
			if (!SendCanData(SendCanCommand.RequestDataFromCanBus, 0, isExtended: false, 4, data.ToArray(), 2))
			{
				SystemSounds.Beep.Play();
			}
		}
		if (flag)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		return result;
	}

	public bool CanChannelSetupLiveDataRequest(int serialNumber, int channelNumber)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			SetupModuleIdentifier(serialNumber, channelNumber, ReadWriteConfig.Read, CanModuleConfigBits.SendData, out var _, out var replyIdTemp);
			result = SendCanData(SendCanCommand.RequestDataFromCanBus, replyIdTemp, isExtended: false, 4, new byte[0], 2);
		}
		if (flag)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		return result;
	}

	public bool CanChannelClearLiveDataRequest()
	{
		return CanChannelSetupLiveDataRequest(0, 0);
	}

	public bool CanChannelRequestLiveData(int serialNumber, int channelNumber, bool isRacelogicFloat, out float data)
	{
		bool result = false;
		data = 0f;
		bool flag = isReceivingVBoxComms;
		if ((!flag || SetQuiet_StandardProtocol(MakeQuiet: true)) && SendCanData(SendCanCommand.PlaceDataOntoCanBus, 1575681, isExtended: false, 5, new byte[0], 2))
		{
			SetupModuleIdentifier(serialNumber, channelNumber, ReadWriteConfig.Read, CanModuleConfigBits.SendData, out var _, out var replyIdTemp);
			if (SendCanData(SendCanCommand.PlaceDataOntoCanBus, replyIdTemp | 0x400, isExtended: true, 29, new byte[0], 3))
			{
				List<byte> list = new List<byte>();
				using (ReturnedDataLock.Lock())
				{
					foreach (byte returnedDatum in _returnedData)
					{
						list.Add(returnedDatum);
					}
					Union union = default(Union);
					int num = ((channelNumber % 2 != 0) ? 4 : 0);
					union.b3_MSB = _returnedData[num];
					union.b2 = _returnedData[1 + num];
					union.b1 = _returnedData[2 + num];
					union.b0_LSB = _returnedData[3 + num];
					if (isRacelogicFloat)
					{
						data = float.Parse(CanSignal.ConvertRacelogicUnits(union.temp));
					}
					else
					{
						data = union.data;
					}
				}
				result = true;
			}
		}
		if (flag)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		return result;
	}

	private bool DataTransfer(List<byte> data)
	{
		bool flag = true;
		foreach (KeyValuePair<string, CanBootloaderCommandDefinition> item in CanBootloaderCommands.List.Where((KeyValuePair<string, CanBootloaderCommandDefinition> cmd) => string.Equals(cmd.Key, "DataTransfer")))
		{
			if (!flag)
			{
				break;
			}
			for (int i = 0; i < 32; i++)
			{
				item.Value.Data[0] = data[i * 8];
				item.Value.Data[1] = data[i * 8 + 1];
				item.Value.Data[2] = data[i * 8 + 2];
				item.Value.Data[3] = data[i * 8 + 3];
				item.Value.Data[4] = data[i * 8 + 4];
				item.Value.Data[5] = data[i * 8 + 5];
				item.Value.Data[6] = data[i * 8 + 6];
				item.Value.Data[7] = data[i * 8 + 7];
				flag = SendCanBootloaderCommand("DataTransfer");
				Thread.Sleep(10);
			}
		}
		return flag;
	}

	private bool SetFlashAddress(int address)
	{
		bool result = false;
		foreach (KeyValuePair<string, CanBootloaderCommandDefinition> item in CanBootloaderCommands.List.Where((KeyValuePair<string, CanBootloaderCommandDefinition> cmd) => string.Equals(cmd.Key, "SetFlashAddress")))
		{
			item.Value.Data[0] = (byte)(address / 65536);
			item.Value.Data[1] = (byte)(address / 256);
			item.Value.Data[2] = (byte)(address % 256);
			result = SendCanBootloaderCommand("SetFlashAddress");
		}
		return result;
	}

	private bool SetFlashAddress2(int address, List<byte> random)
	{
		KeyValuePair<string, CanBootloaderCommandDefinition>? keyValuePair = null;
		foreach (KeyValuePair<string, CanBootloaderCommandDefinition> item in CanBootloaderCommands.List.Where((KeyValuePair<string, CanBootloaderCommandDefinition> cmd) => string.Equals(cmd.Key, "SetFlashAddress2")))
		{
			keyValuePair = item;
			item.Value.Data[0] = (byte)((address / 65536) ^ random[0]);
			item.Value.Data[1] = random[0];
			item.Value.Data[2] = (byte)((address / 256) ^ random[1]);
			item.Value.Data[3] = random[1];
			item.Value.Data[4] = (byte)((address % 256) ^ random[2]);
			item.Value.Data[5] = random[2];
			item.Value.Data[6] = random[3];
			item.Value.Data[7] = 0;
		}
		return keyValuePair.HasValue && SendCanBootloaderCommand("SetFlashAddress2");
	}

	private bool DataTransfer2(List<byte> data, List<byte> random)
	{
		bool flag = false;
		foreach (KeyValuePair<string, CanBootloaderCommandDefinition> item in CanBootloaderCommands.List.Where((KeyValuePair<string, CanBootloaderCommandDefinition> cmd) => string.Equals(cmd.Key, "DataTransfer2")))
		{
			flag = true;
			int num = 0;
			byte[] array = new byte[8];
			for (int i = 0; i < 32; i++)
			{
				if (!flag)
				{
					break;
				}
				num = i * 4;
				array[0] = data[CypherBlock[num] * 2];
				array[1] = data[CypherBlock[num] * 2 + 1];
				array[2] = data[CypherBlock[num + 1] * 2];
				array[3] = data[CypherBlock[num + 1] * 2 + 1];
				array[4] = data[CypherBlock[num + 2] * 2];
				array[5] = data[CypherBlock[num + 2] * 2 + 1];
				array[6] = data[CypherBlock[num + 3] * 2];
				array[7] = data[CypherBlock[num + 3] * 2 + 1];
				item.Value.Data[0] = (byte)(array[0] ^ random[0]);
				item.Value.Data[1] = (byte)(array[1] ^ random[3]);
				item.Value.Data[2] = (byte)(array[2] ^ random[1]);
				item.Value.Data[3] = (byte)(array[3] ^ random[2]);
				item.Value.Data[4] = (byte)(array[4] ^ random[2]);
				item.Value.Data[5] = (byte)(array[5] ^ random[1]);
				item.Value.Data[6] = (byte)(array[6] ^ random[3]);
				item.Value.Data[7] = (byte)(array[7] ^ random[0]);
				flag = SendCanBootloaderCommand("DataTransfer2");
				Thread.Sleep(10);
			}
		}
		return flag;
	}

	private bool SendCanBootloaderCommand(string command)
	{
		InSetup = true;
		bool result = false;
		bool flag = false;
		foreach (KeyValuePair<string, CanBootloaderCommandDefinition> item in CanBootloaderCommands.List.Where((KeyValuePair<string, CanBootloaderCommandDefinition> cmd) => string.Equals(cmd.Key, command)))
		{
			flag = true;
			if (CanBootloaderOpenChannel())
			{
				string text = "T";
				int num = item.Value.Identifier & 0x1FFFFFFF;
				for (int num2 = 7; num2 >= 0; num2--)
				{
					text += ItoA((num >> num2 * 4) & 0xF);
				}
				text += ItoA(8);
				for (int i = 0; i < 8; i++)
				{
					text = text + ItoA((item.Value.Data[i] >> 4) & 0xF) + ItoA(item.Value.Data[i] & 0xF);
				}
				result = CanBootloaderSendCommand(text, 'Z');
			}
		}
		if (!flag)
		{
			Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.SendCanBootloaderCommand(): " + Racelogic.Comms.Serial.Properties.Resources.UnrecognisedCommand + Environment.NewLine + command, Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
		}
		InSetup = false;
		return result;
	}

	private char ItoA(int value)
	{
		value = ((value > 9) ? (value + 55) : (value + 48));
		return (char)value;
	}

	public bool CanBootloaderOpenChannel(bool forceIt = false)
	{
		bool flag = true;
		if (forceIt || !CanChannelOpen)
		{
			flag = CanBootloaderSendCommand("O");
			if (flag)
			{
				CanChannelOpen = true;
			}
		}
		return flag;
	}

	public bool CanBootloaderCloseChannel(bool forceIt = false)
	{
		bool flag = true;
		if (forceIt || CanChannelOpen)
		{
			flag = CanBootloaderSendCommand("C");
			if (flag)
			{
				CanChannelOpen = false;
			}
		}
		return flag;
	}

	public bool CanBootloaderSetBaudRate()
	{
		bool flag = false;
		flag = CanBootloaderCloseChannel();
		if (flag)
		{
			flag = CanBootloaderSendCommand("S8");
		}
		return flag;
	}

	public bool CanBootloaderSendCommand(string command, char? response = null)
	{
		bool result = false;
		_awaitingBootloaderResponse = true;
		InSetup = true;
		byte[] array = StructureFunctions.StringToByteArray(command, (uint)command.Length, nullTerminated: false);
		Queue<byte> queue = new Queue<byte>(array.Length + 1);
		byte[] array2 = array;
		foreach (byte item in array2)
		{
			queue.Enqueue(item);
		}
		queue.Enqueue(13);
		FlushRxTx();
		int num = ((!response.HasValue) ? 1 : 2);
		DelayTimer delayTimer = new DelayTimer(2500);
		TxData(queue);
		while (delayTimer.IsRunning && SetupRxCount < num)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (SetupRxCount == num)
		{
			array = new byte[num];
			for (int j = 0; j < num; j++)
			{
				array[j] = setupReceivedDataBuffer[SetupReadIndex++];
			}
			if (array[array.Length - 1] == 13)
			{
				result = true;
			}
			if (num == 2 && array[0] != response.Value)
			{
				result = false;
			}
		}
		InSetup = false;
		_awaitingBootloaderResponse = false;
		return result;
	}

	public bool CanBootloaderInitializeProgramming()
	{
		return SendCanBootloaderCommand("InitializeProgramming");
	}

	public bool CanBootloaderEraseFlash()
	{
		return SendCanBootloaderCommand("EraseChip");
	}

	public bool CanBootloaderRun()
	{
		return SendCanBootloaderCommand("Run");
	}

	public bool CanBootloaderProgramData(int address, List<byte> data)
	{
		bool result = false;
		if (SetFlashAddress(address))
		{
			Thread.Sleep(10);
			result = DataTransfer(data);
		}
		return result;
	}

	public bool CanBootloaderProgramData2(int address, List<byte> data)
	{
		bool result = false;
		Random random = new Random(DateTime.Now.Millisecond);
		List<byte> list = new List<byte>(4);
		for (int i = 0; i < 4; i++)
		{
			list.Add((byte)random.Next(255));
		}
		if (SetFlashAddress2(address, list))
		{
			Thread.Sleep(10);
			result = DataTransfer2(data, list);
		}
		return result;
	}

	public bool MfdReadData(int serialNumber, List<byte> data, List<string> parameters, int numberOfBytes, int startAddress, out byte eepromVersion)
	{
		bool result = false;
		eepromVersion = 0;
		data.Clear();
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingDataFromMfd;
			PercentComplete = 0.0;
			if (MfdVboxManagerUnlock(serialNumber) && MfdGetEepromVersion(serialNumber, out eepromVersion) && GetAvailableParameters(serialNumber, parameters))
			{
				result = MfdVboxManagerReadData(serialNumber, numberOfBytes, data, startAddress);
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
			ProgressText = string.Empty;
			PercentComplete = 0.0;
		}
		return result;
	}

	public bool MfdWriteData(int serialNumber, byte[] data, int startAddress)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.WritingDataToMfd;
			PercentComplete = 0.0;
			if (MfdVboxManagerUnlock(serialNumber) && MfdVBoxManagerWriteData(serialNumber, data, startAddress))
			{
				result = MfdVBoxManagerReloadEeprom(serialNumber);
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
			ProgressText = string.Empty;
			PercentComplete = 0.0;
		}
		return result;
	}

	public bool VBoxManagerReadData(int serialNumber, List<byte> data)
	{
		bool result = false;
		data.Clear();
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingDataFromVBoxManager;
			PercentComplete = 0.0;
			if (MfdVboxManagerUnlock(serialNumber))
			{
				result = MfdVboxManagerReadData(serialNumber, 5, data, 256);
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
			ProgressText = string.Empty;
			PercentComplete = 0.0;
		}
		return result;
	}

	public bool VBoxManagerWriteData(int serialNumber, byte[] data)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.WritingDataToVBoxManager;
			PercentComplete = 0.0;
			if (MfdVboxManagerUnlock(serialNumber) && MfdVBoxManagerWriteData(serialNumber, data, 256))
			{
				result = MfdVBoxManagerReloadEeprom(serialNumber);
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
			ProgressText = string.Empty;
			PercentComplete = 0.0;
		}
		return result;
	}

	private bool GetAvailableParameters(int serialNumber, List<string> parameters)
	{
		bool flag = false;
		parameters.Clear();
		List<byte> list = new List<byte> { 22, 0 };
		if (MfdVBoxManagerCommunication(serialNumber, list) && list[0] == byte.MaxValue && list[1] == 1)
		{
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingAvailableParametersFromMfd;
			PercentComplete = 0.0;
			int num = list[2];
			StringBuilder stringBuilder = new StringBuilder();
			flag = true;
			for (int i = 1; i <= num; i++)
			{
				if (!flag)
				{
					break;
				}
				stringBuilder.Clear();
				list = new List<byte>
				{
					22,
					(byte)i,
					0,
					7
				};
				if (MfdVBoxManagerCommunication(serialNumber, list))
				{
					for (int j = 0; j < 7; j++)
					{
						stringBuilder.Append((char)list[j]);
					}
					list = new List<byte>
					{
						22,
						(byte)i,
						7,
						3
					};
					if (MfdVBoxManagerCommunication(serialNumber, list))
					{
						for (int k = 0; k < 3; k++)
						{
							stringBuilder.Append((char)list[k]);
						}
						parameters.Add(stringBuilder.ToString().TrimEnd('\0', ' '));
						PercentComplete = (double)(100 * i) / (double)num;
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					flag = false;
				}
			}
		}
		percentComplete = 0.0;
		return flag;
	}

	private void AddAddressToPayload(int address, List<byte> data)
	{
		for (int num = 2; num >= 0; num--)
		{
			data.Add((byte)(address >> num * 8));
		}
	}

	private bool MfdGetEepromVersion(int serialNumber, out byte eepromVersion)
	{
		bool result = false;
		eepromVersion = 0;
		List<byte> list = new List<byte>();
		if (MfdVboxManagerReadData(serialNumber, 1, list, 1))
		{
			result = list[0] >= 18;
			eepromVersion = list[0];
		}
		return result;
	}

	private bool MfdVboxManagerUnlock(int serialNumber)
	{
		bool result = false;
		List<byte> list = new List<byte>();
		list.Add(18);
		if (MfdVBoxManagerCommunication(serialNumber, list) && list[0] == byte.MaxValue && list[1] == 1)
		{
			list.RemoveRange(0, 2);
			uint num = Checksum.Calculate(list, 2u, PolynomialUnitType.VideoVBoxAndMfd);
			list.Clear();
			list.Add(19);
			list.Add((byte)(num >> 8));
			list.Add((byte)num);
			if (MfdVBoxManagerCommunication(serialNumber, list) && list[0] == byte.MaxValue && list[1] == 1)
			{
				result = true;
			}
		}
		return result;
	}

	private bool MfdVBoxManagerCommunication(int serialNumber, List<byte> data)
	{
		bool result = false;
		List<byte> list = new List<byte>(data);
		SetupModuleIdentifier(serialNumber, 0, ReadWriteConfig.Read, CanModuleConfigBits.MfdComms, out var _, out var replyIdTemp);
		if (SendCanData(SendCanCommand.RequestDataFromCanBus, replyIdTemp, isExtended: false, 4, list.ToArray(), 2))
		{
			replyIdTemp |= 0x400;
			if (SendCanData(SendCanCommand.PlaceDataOntoCanBus, replyIdTemp, isExtended: true, 29, data.ToArray(), 2))
			{
				result = true;
				data.Clear();
				using (ReturnedDataLock.Lock())
				{
					foreach (byte returnedDatum in _returnedData)
					{
						data.Add(returnedDatum);
					}
				}
			}
		}
		SendCanData(SendCanCommand.RequestDataFromCanBus, 0, isExtended: false, 4, list.ToArray(), 2);
		return result;
	}

	private bool MfdVBoxManagerReloadEeprom(int serialNumber)
	{
		bool result = false;
		List<byte> list = new List<byte>();
		list.Add(21);
		if (MfdVBoxManagerCommunication(serialNumber, list) && list[0] == byte.MaxValue && list[1] == 1)
		{
			result = true;
		}
		return result;
	}

	private bool MfdVboxManagerSetupReadWriteAddress(int serialNumber, int address, ReadWriteConfig readWrite)
	{
		bool result = false;
		List<byte> list = new List<byte>();
		list.Add((byte)((readWrite == ReadWriteConfig.Read) ? 4 : 3));
		AddAddressToPayload(address, list);
		if (MfdVBoxManagerCommunication(serialNumber, list))
		{
			result = list[0] == byte.MaxValue && list[1] == 1;
		}
		return result;
	}

	private bool MfdVboxManagerReadData(int serialNumber, int numberOfBytes, List<byte> data, int address)
	{
		bool result = false;
		if (MfdVboxManagerSetupReadWriteAddress(serialNumber, address, ReadWriteConfig.Read))
		{
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingDataFromMfd;
			result = MfdVboxManagerReadData(serialNumber, numberOfBytes, data);
		}
		return result;
	}

	private bool MfdVboxManagerReadData(int serialNumber, int numberOfBytes, List<byte> data)
	{
		bool flag = true;
		data.Clear();
		List<byte> list = new List<byte>();
		int num = 0;
		while (flag && num < numberOfBytes)
		{
			list.Clear();
			list.Add(5);
			byte b = (byte)((numberOfBytes - num > 7) ? 7 : ((byte)(numberOfBytes - num)));
			list.Add(b);
			flag = MfdVBoxManagerCommunication(serialNumber, list);
			if (!flag)
			{
				continue;
			}
			num += b;
			if (list[0] == b)
			{
				flag = true;
				for (int i = 1; i < b + 1; i++)
				{
					data.Add(list[i]);
				}
				PercentComplete = (double)(100 * num) / (double)numberOfBytes;
			}
		}
		PercentComplete = 0.0;
		return flag;
	}

	private bool MfdVBoxManagerWriteData(int serialNumber, byte[] data, int address)
	{
		bool result = false;
		if (MfdVboxManagerSetupReadWriteAddress(serialNumber, address, ReadWriteConfig.Write))
		{
			result = MfdVBoxManagerWriteData(serialNumber, data);
		}
		return result;
	}

	private bool MfdVBoxManagerWriteData(int serialNumber, byte[] data)
	{
		bool flag = true;
		int num = 0;
		int num2 = data.Length;
		List<byte> list = new List<byte>();
		while (flag && num < data.Length)
		{
			list.Clear();
			list.Add(5);
			byte b = (byte)((data.Length - num > 6) ? 6 : ((byte)(data.Length - num)));
			list.Add(b);
			for (int i = 0; i < b; i++)
			{
				list.Add(data[i + num]);
			}
			if (!MfdVBoxManagerCommunication(serialNumber, list))
			{
				continue;
			}
			for (int j = 0; j < b; j++)
			{
				if (data[num + j] != list[j + 1])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				num += b;
				PercentComplete = (double)(100 * num) / (double)num2;
			}
		}
		return flag;
	}

	private void GetSeed()
	{
		_seed = null;
		SendCommand(CommandCode.MemoryAccess, MemoryAccessSubCommand.GetSeed, null, "GetSeed():", string.Empty);
		DelayTimer delayTimer = new DelayTimer(1500);
		while (!_seed.HasValue && delayTimer.IsRunning)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			delayTimer = null;
			throw new RacelogicSerialPortException("Racelogic.Comms.Serial.GetSeed():\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout);
		}
		delayTimer = null;
	}

	private void Unlock(ushort seed)
	{
		Queue<byte> queue = new Queue<byte>(2);
		queue.Enqueue((byte)(seed >> 8));
		queue.Enqueue((byte)seed);
		uint num = Checksum.Calculate(queue, 2u, PolynomialUnitType.VideoVBoxAndMfd);
		byte[] payload = new byte[2]
		{
			(byte)(num >> 8),
			(byte)num
		};
		_locked = true;
		SendCommand(CommandCode.MemoryAccess, MemoryAccessSubCommand.Unlock, payload, "Unlock():", Racelogic.Comms.Serial.Properties.Resources.Unlock);
		DelayTimer delayTimer = new DelayTimer(1500);
		while (_locked && delayTimer.IsRunning)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			delayTimer = null;
			throw new RacelogicSerialPortException("Racelogic.Comms.Serial.Unlock():\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout);
		}
		delayTimer = null;
	}

	private void Lock()
	{
		SendCommand(CommandCode.MemoryAccess, MemoryAccessSubCommand.Lock, null, "Lock():", string.Empty);
	}

	private MemoryInfo[] GetSystemMemory()
	{
		throw new NotImplementedException();
	}

	private void Upload(MemoryType memoryType, uint address, byte[] data)
	{
		byte[] array = new byte[data.Length + 1 + 4];
		array[0] = (byte)memoryType;
		int num = 1;
		int num2 = 3;
		while (num2 >= 0)
		{
			array[num] = (byte)(address >> num2 * 8);
			num2--;
			num++;
		}
		data.CopyTo(array, 5);
		SendCommand(CommandCode.MemoryAccess, MemoryAccessSubCommand.UploadDataToUnit, array, "UploadDataToUnit():", Racelogic.Comms.Serial.Properties.Resources.UploadDataToUnit);
	}

	private void UploadDataToUnit(MemoryType memoryType, uint address, byte[] data)
	{
		if (data == null || data.Length == 0)
		{
			this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.UploadDataToUnit():\r\n" + Racelogic.Comms.Serial.Properties.Resources.DataLengthError));
			return;
		}
		Queue<byte> queue = new Queue<byte>(data.Length);
		byte[] array = data;
		foreach (byte item in array)
		{
			queue.Enqueue(item);
		}
		while (queue.Count > _maxPayloadLength)
		{
			data = new byte[_maxPayloadLength];
			for (int j = 0; j < _maxPayloadLength; j++)
			{
				data[j] = queue.Dequeue();
			}
			Upload(memoryType, address, data);
			address += _maxPayloadLength;
		}
		if (queue.Count > 0)
		{
			data = queue.ToArray();
			Upload(memoryType, address, data);
		}
	}

	private void Download(MemoryType memoryType, uint address, uint numberOfBytes)
	{
		byte[] array = new byte[9]
		{
			(byte)memoryType,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};
		int num = 1;
		int num2 = 3;
		while (num2 >= 0)
		{
			array[num] = (byte)(address >> num2 * 8);
			array[num + 4] = (byte)(numberOfBytes >> num2 * 8);
			num2--;
			num++;
		}
		SendCommand(CommandCode.MemoryAccess, MemoryAccessSubCommand.DownloadDataFromUnit, array, "DownloadDataFromUnit():", Racelogic.Comms.Serial.Properties.Resources.DownloadDataFromUnit);
	}

	private void DownloadDataFromUnit(MemoryType memoryType, uint address, uint numberOfBytes)
	{
		while (numberOfBytes > _maxPayloadLength)
		{
			Download(MemoryType.Eeprom, address, _maxPayloadLength);
			address += _maxPayloadLength;
			numberOfBytes -= _maxPayloadLength;
		}
		if (numberOfBytes != 0)
		{
			Download(MemoryType.Eeprom, address, numberOfBytes);
		}
	}

	private void DownloadUnitEEPROM()
	{
		DownloadDataFromUnit(MemoryType.Eeprom, 0u, (uint)_eeprom.Length);
		DelayTimer delayTimer = new DelayTimer(5000);
		while (MessagesSent.Count != 0 && delayTimer.IsRunning)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.DownloadUnitEEPROM():\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout));
		}
		delayTimer = null;
	}

	private void GetEEPROMSize()
	{
		_eeprom = null;
		SendCommand(CommandCode.MemoryAccess, MemoryAccessSubCommand.GetMemorySize, new byte[1] { 0 }, "GetMemorySize(EEPROM)():", Racelogic.Comms.Serial.Properties.Resources.GetEepromSize);
		DelayTimer delayTimer = new DelayTimer(1500);
		while (_eeprom == null && delayTimer.IsRunning)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.GetMemorySize():\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout));
		}
		delayTimer = null;
	}

	private void ProcessMemoryAccessCommand(MessageSent message, byte[] data)
	{
		switch (message.SubCommand)
		{
		case 1:
		{
			ushort value = (ushort)((data[0] << 8) | data[1]);
			_protocolVersion = (ushort)((data[2] << 8) | data[3]);
			_maxPayloadLength = (uint)((data[4] << 24) | (data[5] << 16) | (data[6] << 8) | data[7]);
			_seed = value;
			break;
		}
		case 2:
			_locked = false;
			break;
		case 3:
			_locked = true;
			break;
		case 4:
		{
			uint num2 = (uint)((data[1] << 24) | (data[2] << 16) | (data[3] << 8) | data[4]);
			switch (data[0])
			{
			case 0:
				_eeprom = new byte[num2];
				break;
			case 1:
				_flash = new byte[num2];
				break;
			default:
				this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.ProcessMemoryAccessCommand():\r\n" + Racelogic.Comms.Serial.Properties.Resources.InvalidMemoryType));
				break;
			}
			break;
		}
		case 5:
			break;
		case 6:
		{
			uint num = (uint)((message.Payload[1] << 24) | (message.Payload[2] << 16) | (message.Payload[3] << 8) | message.Payload[4]);
			switch (message.Payload[0])
			{
			case 0:
				foreach (byte b2 in data)
				{
					_eeprom[num] = b2;
					num++;
				}
				break;
			case 1:
				foreach (byte b in data)
				{
					_flash[num] = b;
					num++;
				}
				break;
			}
			break;
		}
		case 10:
		{
			_responseData = new byte[data.Length];
			if (data.Length % 9 != 0)
			{
				throw new RacelogicSerialPortException("Racelogic.Comms.Serial.GetSystemMemory() - " + Racelogic.Comms.Serial.Properties.Resources.InvalidNumberOfBytesInResponse);
			}
			for (int i = 0; i < data.Length; i++)
			{
				_responseData[i] = data[i];
			}
			break;
		}
		case 7:
			break;
		case 8:
			break;
		case 9:
			break;
		}
	}

	public void ReadEEPROM()
	{
		try
		{
			GetSeed();
			if (_seed.HasValue)
			{
				Unlock(_seed.Value);
				UnitInformation unitInfo = new UnitInformation();
				GetSerialNumber(unitInfo);
				GetEEPROMSize();
				DownloadUnitEEPROM();
			}
		}
		catch (RacelogicSerialPortException ex)
		{
			ShowErrorMessage(ex);
		}
	}

	public void ReadFLASH()
	{
		SendCommand(CommandCode.MemoryAccess, MemoryAccessSubCommand.GetMemorySize, new byte[1] { 1 }, "GetMemorySize(EEPROM)():", Racelogic.Comms.Serial.Properties.Resources.GetEepromSize);
	}

	public void WriteEEPROM()
	{
		if (_eeprom == null || _eeprom.Length == 0)
		{
			this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.WriteEEPROM():\r\n" + Racelogic.Comms.Serial.Properties.Resources.MemoryNotRead));
			return;
		}
		try
		{
			GetSeed();
		}
		catch (RacelogicSerialPortException ex)
		{
			ShowErrorMessage(ex);
		}
	}

	public MemoryInfo[] GetMemoryInformation()
	{
		MemoryInfo[] result = new MemoryInfo[0];
		try
		{
			GetSeed();
			if (_seed.HasValue)
			{
				Unlock(_seed.Value);
				result = GetSystemMemory();
			}
		}
		catch (RacelogicSerialPortException ex)
		{
			ShowErrorMessage(ex);
		}
		return result;
	}

	public bool ProgramFlash(byte[] data)
	{
		return false;
	}

	private void ShowErrorMessage(Exception ex)
	{
		if (this.SerialCommsInformation != null)
		{
			StringBuilder stringBuilder = new StringBuilder(ex.Message);
			Exception innerException;
			while ((innerException = ex.InnerException) != null)
			{
				stringBuilder.Append("\r\n");
				stringBuilder.Append(innerException.Message);
				ex = ex.InnerException;
			}
			this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, stringBuilder.ToString()));
		}
	}

	private bool SendCommand(byte command, byte subCommand, byte[] payload, string errorMessage, string statusMessage, double percentComplete)
	{
		bool result = false;
		if (payload == null)
		{
			payload = new byte[0];
		}
		Queue<byte> queue = new Queue<byte>(17 + payload.Length);
		string text = "$RLCMD$";
		foreach (char c in text)
		{
			queue.Enqueue((byte)c);
		}
		queue.Enqueue((byte)(_messageId >> 8));
		queue.Enqueue((byte)_messageId);
		queue.Enqueue(command);
		queue.Enqueue(subCommand);
		queue.Enqueue((byte)(payload.Length >> 24));
		queue.Enqueue((byte)(payload.Length >> 16));
		queue.Enqueue((byte)(payload.Length >> 8));
		queue.Enqueue((byte)payload.Length);
		byte[] array = payload;
		foreach (byte item in array)
		{
			queue.Enqueue(item);
		}
		Checksum.Append(queue);
		FlushRxTx();
		MessagesSent.Add(new MessageSent(_messageId++, command, subCommand, payload));
		if (TxData(queue))
		{
			_waitingForResponse = true;
			_responseOk = false;
			if (this.NewMessageSent != null)
			{
				this.NewMessageSent(this, new MessagesEventArgs((uint)MessagesSent.Count, statusMessage, percentComplete));
			}
			result = true;
		}
		else
		{
			MessagesSent.Remove(MessagesSent[MessagesSent.Count - 1]);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				errorMessage = errorMessage + "\r\n" + Racelogic.Comms.Serial.Properties.Resources.SendCommandFail;
				if (this.SerialCommsInformation != null)
				{
					this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.SendCommand(): " + errorMessage));
				}
			}
		}
		return result;
	}

	public bool SendCommand(CommandCode command, VBoxSubCommand subCommand, byte[] payload, string errorMessage, string statusMessage)
	{
		bool result = false;
		if (command != CommandCode.VBox && this.SerialCommsInformation != null)
		{
			this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Raclogic.Comms.Serial.SendCommand(): Incorrect CommandCode type for VBOXSubCommand"));
		}
		else
		{
			result = SendCommand((byte)command, (byte)subCommand, payload, errorMessage, statusMessage, 0.0);
		}
		return result;
	}

	public bool SendCommand(CommandCode command, GpsSubCommand subCommand, byte[] payload, string errorMessage, string statusMessage)
	{
		bool result = false;
		if (command != CommandCode.Gps && this.SerialCommsInformation != null)
		{
			this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Raclogic.Comms.Serial.SendCommand(): Incorrect CommandCode type for GPSSubCommand"));
		}
		else
		{
			result = SendCommand((byte)command, (byte)subCommand, payload, errorMessage, statusMessage, 0.0);
		}
		return result;
	}

	public bool SendCommand(CommandCode command, MemoryAccessSubCommand subCommand, byte[] payload, string errorMessage, string statusMessage)
	{
		bool result = false;
		if (command != CommandCode.MemoryAccess && this.SerialCommsInformation != null)
		{
			this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Raclogic.Comms.Serial.SendCommand(): Incorrect CommandCode type for MemoryAccessSubCommand"));
		}
		else
		{
			result = SendCommand((byte)command, (byte)subCommand, payload, errorMessage, statusMessage, 0.0);
		}
		return result;
	}

	public bool SendCommand(CommandCode command, VideoVBoxSubCommand subCommand, byte[] payload, string errorMessage, string statusMessage, double percentageComplete)
	{
		bool result = false;
		if (command != CommandCode.VideoVBox && this.SerialCommsInformation != null)
		{
			this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Raclogic.Comms.Serial.SendCommand(): Incorrect CommandCode type for VideoVBOXSubCommand"));
		}
		else
		{
			result = SendCommand((byte)command, (byte)subCommand, payload, errorMessage, statusMessage, percentageComplete);
		}
		return result;
	}

	public bool SendCommand(CommandCode command, VideoVBoxSubCommand subCommand, byte[] payload, string errorMessage, string statusMessage)
	{
		return SendCommand(command, subCommand, payload, errorMessage, statusMessage, 0.0);
	}

	public bool GetSerialNumber(UnitInformation UnitInfo)
	{
		return GetSerialNumber(UnitInfo, ShowError: true);
	}

	public bool GetSerialNumber(UnitInformation UnitInfo, bool ShowError)
	{
		bool result = false;
		string errorMessage = (ShowError ? "GetSerialNumber():" : string.Empty);
		if (UnitInfo == null)
		{
			UnitInfo = new UnitInformation();
		}
		SendCommand(CommandCode.VBox, VBoxSubCommand.GetSerialNumber, null, errorMessage, Racelogic.Comms.Serial.Properties.Resources.GetSerialNumber);
		DelayTimer delayTimer = new DelayTimer(1500);
		while (delayTimer.IsRunning && _waitingForResponse)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			if (ShowError && this.SerialCommsInformation != null)
			{
				this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.GetSerialNumber():\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout));
			}
		}
		else if (_responseOk)
		{
			result = true;
			UnitInfo.UnitType = _responseData[0];
			UnitInfo.SerialNumber = 0;
			for (int i = 1; i < 5; i++)
			{
				UnitInfo.SerialNumber <<= 8;
				UnitInfo.SerialNumber |= _responseData[i];
			}
		}
		delayTimer = null;
		return result;
	}

	public bool GetFirmwareVersion(UnitInformation UnitInfo)
	{
		bool result = false;
		UnitInfo.FirmwareVersion = -1;
		SendCommand(CommandCode.VBox, VBoxSubCommand.GetFirmwareVersion, null, "GetFirmwareVersion():", Racelogic.Comms.Serial.Properties.Resources.GetFirmwareVersion);
		DelayTimer delayTimer = new DelayTimer(1500);
		while (delayTimer.IsRunning && _waitingForResponse)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			result = false;
			this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.GetFirmwareVersion():\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout));
		}
		else if (_responseOk)
		{
			result = true;
			UnitInfo.FirmwareVersion = 0;
			for (int i = 0; i < 4; i++)
			{
				UnitInfo.FirmwareVersion <<= 8;
				UnitInfo.FirmwareVersion |= _responseData[i];
			}
		}
		delayTimer = null;
		return result;
	}

	public bool GetFirmwareString(UnitInformation UnitInfo)
	{
		bool result = false;
		UnitInfo.FirmwareString = string.Empty;
		SendCommand(CommandCode.VBox, VBoxSubCommand.GetFirmwareString, null, "GetFirmwareString():", Racelogic.Comms.Serial.Properties.Resources.GetFirmwareString);
		DelayTimer delayTimer = new DelayTimer(1500);
		while (delayTimer.IsRunning && _waitingForResponse)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			result = false;
			this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.Serial.GetFirmwareString():\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout));
		}
		else if (_responseOk)
		{
			result = true;
			UnitInfo.FirmwareString = Racelogic.Core.Helper.ByteArrayToString(_responseData);
		}
		delayTimer = null;
		return result;
	}

	private void ProcessVBOXCommand(MessageSent message, byte[] data)
	{
		int num = -1;
		switch (message.SubCommand)
		{
		case 6:
			if (data.Length == 5)
			{
				num = 5;
			}
			break;
		case 38:
			if (data.Length == 4)
			{
				num = 4;
			}
			break;
		case 39:
			num = data.Length;
			break;
		}
		if (num != -1)
		{
			_responseData = new byte[num];
			for (int i = 0; i < num; i++)
			{
				_responseData[i] = data[i];
			}
		}
	}

	private bool GetFileFromVideoVBox(VideoVBoxSubCommand subCommand, string saveFileAs)
	{
		bool result = false;
		try
		{
			if (TransferFile == null)
			{
				GetSeed();
				if (_seed.HasValue)
				{
					Unlock(_seed.Value);
					GetFileSize(subCommand);
					if (TransferFile == null)
					{
						throw new RacelogicSerialPortException(Racelogic.Comms.Serial.Properties.Resources.GetConfigurationFileSizeError);
					}
					GetFile(saveFileAs);
					result = true;
					TransferFile = null;
				}
			}
			else
			{
				VideoVBoxConfigurationFileTransferError(Racelogic.Comms.Serial.Properties.Resources.VideoVBoxConfigFileInUse);
			}
		}
		catch (RacelogicSerialPortException ex)
		{
			ShowErrorMessage(ex);
			TransferFile = null;
		}
		return result;
	}

	private bool GetConfigurationOkConfirmation()
	{
		bool flag = true;
		_configurationOk = false;
		SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.RequestConfigurationOkConfirmation, null, "GetConfigurationOkConfirmation():", Racelogic.Comms.Serial.Properties.Resources.WaitingUploadConfirmation);
		DelayTimer delayTimer = new DelayTimer(15000);
		while (!_configurationOk && delayTimer.IsRunning)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
			if (oldDotCount != dotCount)
			{
				oldDotCount = dotCount;
				delayTimer.Interval = 15000.0;
			}
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.ModuleLowLevel.GetConfigurationOKConfirmation():\r\n" + Racelogic.Comms.Serial.Properties.Resources.UploadConfirmationFail));
			delayTimer = null;
			flag = false;
		}
		delayTimer = null;
		byte[] payload = ((!flag) ? Racelogic.Core.Helper.StringToByteArray("Timeout") : Racelogic.Core.Helper.StringToByteArray("OK"));
		SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.ReceivedFinishConfirmation, payload, string.Empty, string.Empty);
		return flag;
	}

	private void CancelFileProgress()
	{
		SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.CancelFileProgress, null, "CancelFileProgress():", Racelogic.Comms.Serial.Properties.Resources.CancelFileProgress);
	}

	private void GetFile(string saveFileAs)
	{
		uint num = 2u;
		double num2 = 0.0;
		dotCount = 0u;
		oldDotCount = dotCount;
		switch (_fileTransferType)
		{
		case FileTransferType.Config:
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.DownloadingScene;
			PercentComplete = 0.0;
			break;
		case FileTransferType.ScreenShot:
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.DownloadingScreentshot;
			PercentComplete = 0.0;
			break;
		}
		while (TransferFile.Size != 0)
		{
			uint size = TransferFile.Size;
			double num3 = TransferFile.BytesTransferred / TransferFile.TotalSize * 100.0;
			if (num3 - num2 > 1.0)
			{
				num2 = num3;
				PercentComplete = num3;
			}
			switch (_fileTransferType)
			{
			case FileTransferType.Config:
				SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.BlockRequest, null, "GetFile():", Racelogic.Comms.Serial.Properties.Resources.GetConfigurationFile, num3);
				break;
			case FileTransferType.ScreenShot:
				SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.BlockRequest, null, "GetFile():", Racelogic.Comms.Serial.Properties.Resources.GetScreenShot, num3);
				break;
			}
			DelayTimer delayTimer = new DelayTimer(5000);
			while (delayTimer.IsRunning && TransferFile.Size == size)
			{
				Racelogic.Core.Win.Helper.WaitForPriority();
			}
			delayTimer.Enabled = false;
			if (!delayTimer.IsRunning && _waitingForResponse)
			{
				if (num == 0)
				{
					delayTimer = null;
					VideoVBoxConfigurationFileTransferError(Racelogic.Comms.Serial.Properties.Resources.BlockRequestFail);
				}
				num--;
			}
			else if (size != TransferFile.Size)
			{
				num = 2u;
			}
			delayTimer = null;
		}
		TransferFile.Stream.Seek(0L, SeekOrigin.Current);
		using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(saveFileAs, FileMode.Create)))
		{
			binaryWriter.Write(TransferFile.Stream.ToArray());
		}
		TransferFile.Stream.Close();
		TransferFile = null;
	}

	private void GetFileSize(VideoVBoxSubCommand subCommand)
	{
		PercentComplete = 0.0;
		switch (_fileTransferType)
		{
		case FileTransferType.Config:
			SendCommand(CommandCode.VideoVBox, subCommand, null, "GetConfigurationFileSize():", Racelogic.Comms.Serial.Properties.Resources.GetConfigurationFile);
			TransferFile = null;
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.GetConfigurationFile;
			break;
		case FileTransferType.ScreenShot:
			SendCommand(CommandCode.VideoVBox, subCommand, null, "GetConfigurationFileSize():", Racelogic.Comms.Serial.Properties.Resources.GetScreenShot);
			TransferFile = null;
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.GetScreenShot;
			break;
		}
		DelayTimer delayTimer = new DelayTimer(6500);
		while (delayTimer.IsRunning && TransferFile == null)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
			if (oldDotCount != dotCount)
			{
				oldDotCount = dotCount;
				delayTimer.Interval = 6500.0;
			}
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			delayTimer = null;
			VideoVBoxConfigurationFileTransferError(Racelogic.Comms.Serial.Properties.Resources.GetConfigurationFileSizeError);
		}
		delayTimer = null;
	}

	private void SendConfigurationFileSize()
	{
		byte[] array = new byte[4];
		for (int num = 3; num >= 0; num--)
		{
			array[3 - num] = (byte)(TransferFile.Size >> num * 8);
		}
		SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.SetConfiguration, array, "SendConfigurationFileSize():", Racelogic.Comms.Serial.Properties.Resources.SetConfigurationFile);
		PercentComplete = 0.0;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.SetConfigurationFile;
		DelayTimer delayTimer = new DelayTimer(1500);
		while (delayTimer.IsRunning && MessagesSent.Count != 0)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
			if (oldDotCount != dotCount)
			{
				oldDotCount = dotCount;
				delayTimer.Interval = 1500.0;
			}
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			delayTimer = null;
			VideoVBoxConfigurationFileTransferError(Racelogic.Comms.Serial.Properties.Resources.SetConfigurationFileSizeError);
		}
		delayTimer = null;
	}

	private bool SendConfigurationFile()
	{
		bool result = false;
		uint num = 0u;
		double num2 = 0.0;
		uint num3 = 2u;
		byte[] array = new byte[_maxPayloadLength];
		PercentComplete = 0.0;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.UploadingScene;
		while (TransferFile.BytesTransferred < (double)TransferFile.Size && TransferFile.Status == SerialMessageStatus.Ok)
		{
			if (num3 == 2)
			{
				if ((double)TransferFile.Size - TransferFile.BytesTransferred >= (double)_maxPayloadLength)
				{
					TransferFile.Stream.Read(array, 0, (int)_maxPayloadLength);
				}
				else
				{
					array = new byte[TransferFile.Size - (int)TransferFile.BytesTransferred];
					TransferFile.Stream.Read(array, 0, array.Length);
				}
			}
			double num4 = TransferFile.BytesTransferred / TransferFile.TotalSize * 100.0;
			if (num4 - num2 > 1.0)
			{
				num2 = num4;
				PercentComplete = num4;
			}
			num = (uint)TransferFile.BytesTransferred;
			SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.WriteFileBlock, array, "SendConfigurationFile():", Racelogic.Comms.Serial.Properties.Resources.SetConfigurationFile, num4);
			DelayTimer delayTimer = new DelayTimer(3000);
			while (delayTimer.IsRunning && TransferFile.BytesTransferred == (double)num)
			{
				Racelogic.Core.Win.Helper.WaitForPriority();
			}
			delayTimer.Enabled = false;
			if (!delayTimer.IsRunning && _waitingForResponse)
			{
				if (num3 == 0)
				{
					VideoVBoxConfigurationFileTransferError(Racelogic.Comms.Serial.Properties.Resources.WriteFileBlockFail);
				}
				else
				{
					UnitInformation unitInfo = new UnitInformation();
					num3--;
					GetSerialNumber(unitInfo, ShowError: false);
					delayTimer = new DelayTimer(3000);
					while (delayTimer.IsRunning && TransferFile.BytesTransferred == (double)num)
					{
						Racelogic.Core.Win.Helper.WaitForPriority();
					}
					delayTimer.Enabled = false;
					if ((double)num != TransferFile.BytesTransferred)
					{
						num3 = 2u;
					}
					delayTimer = null;
				}
			}
			else if ((double)num != TransferFile.BytesTransferred)
			{
				num3 = 2u;
			}
			delayTimer = null;
		}
		if (TransferFile.BytesTransferred == (double)TransferFile.Size)
		{
			result = true;
		}
		return result;
	}

	private void VideoVBoxConfigurationFileTransferError(string errorMessage)
	{
		if (TransferFile != null && TransferFile.Stream != null)
		{
			try
			{
				TransferFile.Stream.Close();
			}
			catch
			{
			}
		}
		TransferFile = null;
		SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.CancelFileProgress, null, "VideoVBOXConfigurationFileTransferError():", Racelogic.Comms.Serial.Properties.Resources.CancelFileProgress);
		throw new RacelogicCheckSumException("Racelogic.Comms.Serial.ReadCurrentVideoVBOXConfiguration():\r\n" + errorMessage);
	}

	private void ProcessVideoVBOXCommand(MessageSent message, byte[] data, SerialMessageStatus status)
	{
		if (status == SerialMessageStatus.Ok)
		{
			switch (message.SubCommand)
			{
			case 3:
				TransferFile.BytesTransferred += (uint)message.Payload.Length;
				break;
			case 16:
			case 20:
				if (data.Length == 0)
				{
					string text3 = string.Empty;
					switch (_fileTransferType)
					{
					case FileTransferType.Config:
						text3 = Racelogic.Comms.Serial.Properties.Resources.GetConfigurationFile;
						break;
					case FileTransferType.ScreenShot:
						text3 = Racelogic.Comms.Serial.Properties.Resources.GetScreenShot;
						break;
					}
					for (byte b3 = 0; b3 < dotCount; b3 = (byte)(b3 + 1))
					{
						text3 += ".";
					}
					dotCount = ((dotCount < 6) ? (dotCount + 1) : 0u);
					PercentComplete = 0.0;
					ProgressText = text3;
				}
				else if (data.Length == 4)
				{
					if (TransferFile == null)
					{
						TransferFile = new VideoVBoxFile((uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]));
					}
					else
					{
						VideoVBoxConfigurationFileTransferError(Racelogic.Comms.Serial.Properties.Resources.VideoVBoxConfigFileInUse);
					}
				}
				else
				{
					VideoVBoxConfigurationFileTransferError(Racelogic.Comms.Serial.Properties.Resources.GetConfigurationFileSizeErrorIncorrectResponse);
				}
				break;
			case 17:
				dotCount++;
				break;
			case 18:
				TransferFile.Stream.Write(data, 0, data.Length);
				TransferFile.Size -= (uint)data.Length;
				TransferFile.BytesTransferred += (uint)data.Length;
				break;
			case 22:
				if (data.Length == 0)
				{
					string text2 = Racelogic.Comms.Serial.Properties.Resources.WaitingUploadConfirmation;
					for (byte b2 = 0; b2 < dotCount; b2 = (byte)(b2 + 1))
					{
						text2 += ".";
					}
					dotCount = ((dotCount < 6) ? (dotCount + 1) : 0u);
					dotCount = ((dotCount < 6) ? (dotCount + 1) : 0u);
					PercentComplete = 0.0;
					ProgressText = text2;
				}
				if (data.Length == 4)
				{
					_configurationOk = true;
				}
				break;
			case 23:
			case 24:
			{
				_responseResult.data = 0.0;
				_responseResult.i0_LSB = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
				_responseData = new byte[data.Length];
				for (int i = 0; i < data.Length; i++)
				{
					_responseData[i] = data[i];
				}
				break;
			}
			case 25:
				if (data.Length == 0)
				{
					string text = Racelogic.Comms.Serial.Properties.Resources.WaitingUploadConfirmation;
					for (byte b = 0; b < dotCount; b = (byte)(b + 1))
					{
						text += ".";
					}
					dotCount = ((dotCount < 6) ? (dotCount + 1) : 0u);
					dotCount = ((dotCount < 6) ? (dotCount + 1) : 0u);
					PercentComplete = 0.0;
					ProgressText = text;
				}
				if (data.Length == 4)
				{
					_configurationOk = true;
				}
				break;
			}
		}
		if (TransferFile != null)
		{
			TransferFile.Status = status;
		}
	}

	public bool GetVideoVBOXScreenshot(string saveFileAs)
	{
		_fileTransferType = FileTransferType.ScreenShot;
		return GetFileFromVideoVBox(VideoVBoxSubCommand.GetScreenshot, saveFileAs);
	}

	public bool ReadCurrentVideoVBoxConfiguration(string saveFileAs)
	{
		_fileTransferType = FileTransferType.Config;
		return GetFileFromVideoVBox(VideoVBoxSubCommand.GetConfiguration, saveFileAs);
	}

	public bool SetVideoVBoxOnScreenDisplay(OnScreenDisplayState newState)
	{
		bool result = true;
		try
		{
			SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.SetOnScreenDisplayState, new byte[1] { (byte)newState }, "SetVideoVBOXOnScreenDisplay():", Racelogic.Comms.Serial.Properties.Resources.SetOnScreenDisplay);
			DelayTimer delayTimer = new DelayTimer(1500);
			while (!delayTimer.IsRunning && _waitingForResponse)
			{
				Racelogic.Core.Win.Helper.WaitForPriority();
			}
			delayTimer.Enabled = false;
			if (!delayTimer.IsRunning && _waitingForResponse)
			{
				delayTimer = null;
				this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.ModuleLowLevel.GetConfigurationOKConfirmation():" + Environment.NewLine + Racelogic.Comms.Serial.Properties.Resources.UploadConfirmationFail));
			}
			delayTimer = null;
		}
		catch (Exception ex)
		{
			ShowErrorMessage(ex);
			result = false;
		}
		return result;
	}

	public bool WriteConfigurationFileToVideoVBOX(string ZippedConfigurationFile)
	{
		bool flag = false;
		if (!File.Exists(ZippedConfigurationFile))
		{
			this.SerialCommsInformation?.Invoke(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.ModuleLowLevel.WriteEEPROM():\r\n" + Racelogic.Comms.Serial.Properties.Resources.ZippedConfigFileDoesNotExist));
		}
		else
		{
			try
			{
				if (TransferFile == null)
				{
					GetSeed();
					if (_seed.HasValue)
					{
						Unlock(_seed.Value);
						FileInfo fileInfo = new FileInfo(ZippedConfigurationFile);
						byte[] array = File.ReadAllBytes(ZippedConfigurationFile);
						TransferFile = new VideoVBoxFile((uint)array.Length)
						{
							Stream = new MemoryStream(array),
							Name = ZippedConfigurationFile
						};
						SendConfigurationFileSize();
						flag = SendConfigurationFile();
						TransferFile = null;
						if (flag)
						{
							flag = GetConfigurationOkConfirmation();
						}
					}
				}
				else
				{
					VideoVBoxConfigurationFileTransferError(Racelogic.Comms.Serial.Properties.Resources.VideoVBoxConfigFileInUse);
				}
			}
			catch (RacelogicSerialPortException ex)
			{
				ShowErrorMessage(ex);
				TransferFile = null;
			}
		}
		return flag;
	}

	public bool RequestSceneList(int MaxNameLength, out int NumberOfScenes)
	{
		bool result = true;
		NumberOfScenes = -1;
		byte[] array = new byte[4];
		for (int num = 3; num >= 0; num--)
		{
			array[3 - num] = (byte)(MaxNameLength >> num * 8);
		}
		SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.RequestSceneList, array, "RequestSceneList():", Racelogic.Comms.Serial.Properties.Resources.RequestSceneList);
		PercentComplete = 0.0;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.RequestSceneList;
		DelayTimer delayTimer = new DelayTimer(1500);
		while (delayTimer.IsRunning && _waitingForResponse)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			result = false;
		}
		else
		{
			NumberOfScenes = _responseResult.i0_LSB;
		}
		delayTimer = null;
		return result;
	}

	public bool RequestSceneName(int SceneIndex, out string SceneName)
	{
		bool result = true;
		SceneName = string.Empty;
		byte[] array = new byte[4];
		for (int num = 3; num >= 0; num--)
		{
			array[3 - num] = (byte)(SceneIndex >> num * 8);
		}
		SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.RequestSceneName, array, "RequestSceneName():", Racelogic.Comms.Serial.Properties.Resources.RequestSceneName);
		PercentComplete = 0.0;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.RequestSceneName;
		DelayTimer delayTimer = new DelayTimer(1500);
		while (delayTimer.IsRunning && _waitingForResponse)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			result = false;
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder(_responseData.Length);
			byte[] responseData = _responseData;
			foreach (byte value in responseData)
			{
				stringBuilder.Append((char)value);
			}
			SceneName = stringBuilder.ToString();
		}
		delayTimer = null;
		return result;
	}

	public bool SetSceneIndex(int SceneIndex)
	{
		bool result = true;
		byte[] array = new byte[4];
		dotCount = 0u;
		oldDotCount = dotCount;
		for (int num = 3; num >= 0; num--)
		{
			array[3 - num] = (byte)(SceneIndex >> num * 8);
		}
		SendCommand(CommandCode.VideoVBox, VideoVBoxSubCommand.RequestSelectScene, array, "SetSceneIndex():", Racelogic.Comms.Serial.Properties.Resources.SetSceneIndex);
		PercentComplete = 0.0;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.SetSceneIndex;
		DelayTimer delayTimer = new DelayTimer(1500);
		while (!_configurationOk && delayTimer.IsRunning)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
			if (oldDotCount != dotCount)
			{
				oldDotCount = dotCount;
				delayTimer.Interval = 6500.0;
			}
		}
		delayTimer.Enabled = false;
		if (!delayTimer.IsRunning && _waitingForResponse)
		{
			result = false;
		}
		delayTimer = null;
		return result;
	}

	private void ExtractFirmwareVersion(UnitInformation UnitInfo, ref double v)
	{
		uint num = (uint)v;
		v -= num;
		num <<= 8;
		if (UnitInfo.UnitType == 16)
		{
			v = Math.Round(v * 10.0, 0, MidpointRounding.AwayFromZero);
		}
		else
		{
			v = Math.Round(v * 100.0, 0, MidpointRounding.AwayFromZero);
		}
		num |= (uint)v;
		num = (uint)(UnitInfo.FirmwareVersion = (int)(num << 16));
	}

	public ProtocolTypes GetProtocolType(UnitInformation UnitInfo)
	{
		ProtocolTypes result = ProtocolTypes.Unknown;
		if (GetSerialNumber_StandardProtocol(UnitInfo, ShowError: false))
		{
			result = ProtocolTypes.VBox;
		}
		else if (GetSerialNumber(UnitInfo, ShowError: false))
		{
			result = ProtocolTypes.VideoVBox;
		}
		return result;
	}

	public bool GetSerialNumber(UnitInformation unitInfo, ProtocolTypes protocolType)
	{
		return GetSerialNumber(unitInfo, protocolType, showError: true);
	}

	public bool GetSerialNumber(UnitInformation unitInfo, ProtocolTypes protocolType, bool showError)
	{
		bool result = false;
		switch (protocolType)
		{
		case ProtocolTypes.VBox:
			result = GetSerialNumber_StandardProtocol(unitInfo, showError);
			break;
		case ProtocolTypes.VideoVBox:
			result = GetSerialNumber(unitInfo, showError);
			break;
		}
		return result;
	}

	public bool GetFirmwareVersion(UnitInformation unitInfo, ProtocolTypes protocolType)
	{
		bool result = false;
		switch (protocolType)
		{
		case ProtocolTypes.VBox:
		{
			List<byte> list = DownloadFromUnit_StandardProtocol(1, 24);
			if (list.Count != 24)
			{
				break;
			}
			int num = 8;
			string text = Racelogic.Core.Helper.ByteArrayToString(list.ToArray());
			int num2 = text.LastIndexOf("version ", StringComparison.OrdinalIgnoreCase);
			if (num2 < 0)
			{
				num2 = text.LastIndexOf("version", StringComparison.OrdinalIgnoreCase);
				num = 7;
				if (num2 < 0)
				{
					num2 = text.LastIndexOf("v", StringComparison.OrdinalIgnoreCase);
					num = 1;
				}
			}
			if (num2 < 0)
			{
				break;
			}
			if (double.TryParse(text.Substring(num2 + num), out var result2))
			{
				ExtractFirmwareVersion(unitInfo, ref result2);
				result = true;
				break;
			}
			unitInfo.FirmwareString = text;
			if (unitInfo.UnitType != 16)
			{
				break;
			}
			text = text.Substring(num2 + num);
			string text2 = string.Empty;
			string text3 = text;
			for (int i = 0; i < text3.Length; i++)
			{
				char c = text3[i];
				if ((c >= '0' && c <= '9') || c.ToString() == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
				{
					text2 += c;
				}
			}
			if (double.TryParse(text2, out result2))
			{
				ExtractFirmwareVersion(unitInfo, ref result2);
				result = true;
			}
			break;
		}
		case ProtocolTypes.VideoVBox:
			if (GetFirmwareVersion(unitInfo))
			{
				result = GetFirmwareString(unitInfo);
			}
			break;
		}
		return result;
	}

	public List<byte> DownloadEEPROM(ProtocolTypes protocolType, int address, int numberOfBytes)
	{
		List<byte> result = null;
		switch (protocolType)
		{
		case ProtocolTypes.VBox:
			result = DownloadFromUnit_StandardProtocol(address, numberOfBytes);
			break;
		}
		return result;
	}

	public bool UploadEEPROM(ProtocolTypes protocolType, int address, List<byte> data)
	{
		bool result = false;
		if (protocolType == ProtocolTypes.VBox)
		{
			result = UploadToUnit_StandardProtocol(address, data);
		}
		return result;
	}

	public bool UploadEEPROM(ProtocolTypes protocolType, int address, List<byte> newData, List<byte> oldData, bool unlockRequired)
	{
		bool result = false;
		if (protocolType == ProtocolTypes.VBox)
		{
			result = UploadToUnit_StandardProtocol(address, newData, oldData, unlockRequired);
		}
		return result;
	}

	public bool ReScanCan(ProtocolTypes protocolType)
	{
		bool result = false;
		if (protocolType == ProtocolTypes.VBox)
		{
			result = ReScanCan_StandardProtocol();
			Thread.Sleep(500);
		}
		return result;
	}

	public bool Stm_GetVersionAndAllowedCommands()
	{
		bool result = false;
		AvailableStmCommands.Clear();
		if (SendStmCommand(new List<byte>().AddStmCommand(StmCommands.GetVersionAndAllowedCommands)) == 121)
		{
			if (Stm_WaitForResponse(endsWithAck: true, null))
			{
				result = true;
				byte b = _returnedData[1];
				byte b2 = _returnedData[2];
				if (_returnedData[b + 3] == 121)
				{
					List<byte> list = Enum.GetValues(typeof(StmCommands)).Cast<byte>().ToList();
					for (int i = 0; i < b; i++)
					{
						if (list.Contains(_returnedData[i + 3]))
						{
							AvailableStmCommands.Add((StmCommands)_returnedData[i + 3]);
						}
					}
				}
				else
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	private bool Stm_WaitForResponse(bool endsWithAck, int? expectedBytes, long? endTime = null)
	{
		bool flag = true;
		if (endTime.HasValue)
		{
			if (DateTime.Now.Ticks > endTime)
			{
				flag = false;
			}
		}
		else
		{
			endTime = DateTime.Now.Ticks + TimeSpan.FromMilliseconds(1000.0).Ticks;
		}
		if (flag)
		{
			if (expectedBytes.HasValue)
			{
				if (_returnedData.Count + SetupRxCount < expectedBytes.Value)
				{
					Thread.Sleep(50);
					flag = Stm_WaitForResponse(endsWithAck, expectedBytes, endTime);
				}
				else
				{
					while (_returnedData.Count < expectedBytes.Value)
					{
						_returnedData.Add(setupReceivedDataBuffer[SetupReadIndex++]);
					}
					flag = true;
				}
			}
			else
			{
				if (_returnedData.Count < 2)
				{
					while (SetupRxCount > 0)
					{
						_returnedData.Add(setupReceivedDataBuffer[SetupReadIndex++]);
					}
				}
				if (_returnedData.Count >= 2)
				{
					int num = _returnedData[1] + 1 + (endsWithAck ? 1 : 0);
					if (_returnedData.Count + SetupRxCount < num)
					{
						Thread.Sleep(50);
						flag = Stm_WaitForResponse(endsWithAck, num, endTime);
					}
					else
					{
						while (_returnedData.Count < num)
						{
							_returnedData.Add(setupReceivedDataBuffer[SetupReadIndex++]);
						}
						flag = true;
					}
				}
				else
				{
					Thread.Sleep(50);
					flag = Stm_WaitForResponse(endsWithAck, null, endTime);
				}
			}
		}
		return flag;
	}

	public IEnumerable<byte> Stm_ReadMemory(uint address, int numberOfBytesToRead)
	{
		bool flag = true;
		List<byte> list = new List<byte>();
		while (flag && numberOfBytesToRead >= 256)
		{
			flag = Stm_MemoryRead(address, 0, out var data);
			if (flag)
			{
				list = list.Append(data);
				address += 256;
				numberOfBytesToRead -= 256;
			}
		}
		if (flag && numberOfBytesToRead > 0)
		{
			flag = Stm_MemoryRead(address, (byte)numberOfBytesToRead, out var data2);
			list = list.Append(data2);
		}
		return flag ? list : new List<byte>();
	}

	private bool Stm_MemoryRead(uint address, byte numberOfBytesToRead, out List<byte> data)
	{
		bool result = false;
		data = new List<byte>();
		if (SendStmCommand(new List<byte>().AddStmCommand(StmCommands.ReadMemory)) == 121 && SendStmCommand(new List<byte>().Add(address).AddStmChecksum()) == 121)
		{
			int num = 1 + ((numberOfBytesToRead == 0) ? 256 : numberOfBytesToRead);
			if (SendStmCommand(new List<byte> { (byte)(numberOfBytesToRead - 1) }.AddStmChecksum(), 4000, 3, num) == 121 && Stm_WaitForResponse(endsWithAck: false, num) && _returnedData.Count == num)
			{
				data = new List<byte>(_returnedData);
				data.RemoveAt(0);
				result = true;
			}
		}
		return result;
	}

	public bool Stm_Go(uint address)
	{
		bool result = false;
		if (SendStmCommand(new List<byte>().AddStmCommand(StmCommands.Go)) == 121 && SendStmCommand(new List<byte>().Add(address).AddStmChecksum()) == 121)
		{
			result = true;
		}
		return result;
	}

	public bool Stm_WriteMemory(uint address, IEnumerable<byte> data)
	{
		bool flag = true;
		int num = data.Count();
		int num2 = 0;
		while (flag && num >= 256)
		{
			flag = Stm_MemoryWrite(address, data.Skip(num2).Take(256));
			num2 += 256;
			num -= 256;
		}
		if (flag && num > 0)
		{
			flag = Stm_MemoryWrite(address, data.Skip(num2).Take(num));
		}
		return flag;
	}

	private bool Stm_MemoryWrite(uint address, IEnumerable<byte> data)
	{
		bool result = false;
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int num = data.Count();
		if (num > 256)
		{
			throw new ArgumentException("data length cannot exceed 256 bytes");
		}
		if (num == 0)
		{
			throw new ArgumentException("data length cannot equal 0 bytes");
		}
		if (SendStmCommand(new List<byte>().AddStmCommand(StmCommands.WriteMemory)) == 121 && SendStmCommand(new List<byte>().Add(address).AddStmChecksum()) == 121 && SendStmCommand(new List<byte> { (byte)(num - 1) }.Add(data).AddStmChecksum(), 4000) == 121)
		{
			result = true;
		}
		return result;
	}

	public bool Stm_Erase(List<ushort> pagesToBeErased)
	{
		bool result = false;
		Stm_GetVersionAndAllowedCommands();
		if (AvailableStmCommands.Contains(StmCommands.Erase))
		{
			result = Stm_StandardErase(pagesToBeErased);
		}
		else if (AvailableStmCommands.Contains(StmCommands.EraseExtended))
		{
			result = Stm_ExtendedErase(pagesToBeErased);
		}
		return result;
	}

	private void ExtractStmResponse()
	{
		using (ReturnedDataLock.Lock())
		{
			if (SetupRxCount >= Math.Max(1, _oldProtocolResponseLength))
			{
				_returnedData.Clear();
				while (SetupRxCount > 0)
				{
					_returnedData.Add(setupReceivedDataBuffer[SetupReadIndex++]);
				}
				_OldProtocolState |= OldProtocolState.ReplyReceived;
			}
		}
	}

	private byte SendStmCommand(IEnumerable<byte> data, int timeout_mS = 3000, int retryCount = 3, int responseLength = 0)
	{
		byte b = 121;
		_oldProtocolResponseLength = responseLength;
		while (retryCount-- > 0)
		{
			InSetup = true;
			FlushRxTx();
			Racelogic.Core.Win.Helper.WaitForPriority();
			_awaitingStmResponse = true;
			_OldProtocolState = OldProtocolState.MessageSent;
			DelayTimer delayTimer = new DelayTimer(timeout_mS);
			if (TxData(data))
			{
				while ((_OldProtocolState & OldProtocolState.ReplyReceived) != OldProtocolState.ReplyReceived && delayTimer.IsRunning)
				{
					Racelogic.Core.Win.Helper.WaitForPriority();
				}
				delayTimer.Enabled = false;
				if (!delayTimer.IsRunning)
				{
					b = 0;
				}
			}
			else
			{
				delayTimer.Enabled = false;
				b = byte.MaxValue;
				StringBuilder stringBuilder = new StringBuilder("TxData Error- Stm ( ");
				foreach (byte datum in data)
				{
					stringBuilder.Append("0x" + datum.ToString("X2") + " ");
				}
				stringBuilder.Append(")");
				if (!IsOpen)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Port not open.");
					retryCount = 0;
				}
				Messenger.get_Default().Send<InformationMessage>(new InformationMessage(stringBuilder.ToString(), Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
			}
			delayTimer = null;
			_awaitingStmResponse = false;
			if (b == 121)
			{
				b = _returnedData[0];
				retryCount = 0;
			}
			if (retryCount > 0)
			{
				Thread.Sleep(25);
			}
		}
		InSetup = false;
		return b;
	}

	private bool Stm_StandardErase(List<ushort> pagesToBeErased)
	{
		bool result = false;
		if (pagesToBeErased == null)
		{
			throw new ArgumentNullException("pagesToBeErased");
		}
		if (pagesToBeErased.Count == 0)
		{
			throw new ArgumentException("pagesToErased must contain at least one value", "pagesToBeErased");
		}
		if (pagesToBeErased.Count > 254)
		{
			throw new ArgumentException("number of pages to be erased exceeds maximum value of 254", "pagesToBeErased");
		}
		bool flag = pagesToBeErased.Count == 1 && pagesToBeErased[0] == 255;
		if (!flag)
		{
			List<ushort> list = new List<ushort>(pagesToBeErased.Count);
			foreach (ushort item in pagesToBeErased)
			{
				if (item > 254)
				{
					throw new ArgumentException("Page number cannot exceed 254.", "pagesToBeErased");
				}
				if (list.Contains(item))
				{
					throw new ArgumentException($"Duplicate page to be erased found, page {item}");
				}
				list.Add(item);
			}
		}
		if (SendStmCommand(new List<byte>().AddStmCommand(StmCommands.Erase)) == 121)
		{
			if (flag)
			{
				if (SendStmCommand(new List<byte>
				{
					byte.MaxValue,
					0
				}) == 121)
				{
					result = true;
				}
			}
			else if (SendStmCommand(new List<byte>().Add(pagesToBeErased.ConvertAll((ushort page) => (byte)page)).AddStmChecksum()) == 121)
			{
				result = true;
			}
		}
		return result;
	}

	private bool Stm_ExtendedErase(List<ushort> pagesToBeErased)
	{
		bool result = false;
		bool flag = false;
		if (pagesToBeErased == null)
		{
			throw new ArgumentNullException("pagesToBeErased");
		}
		if (pagesToBeErased.Count == 0)
		{
			throw new ArgumentException("pagesToErased must contain at least one value", "pagesToBeErased");
		}
		for (ushort num = 65520; num < 65533; num = (ushort)(num + 1))
		{
			if (pagesToBeErased.Contains(num))
			{
				throw new ArgumentException("0x" + num.ToString("X4") + " is reserved", "pagesToBeErased");
			}
		}
		for (uint num2 = 65533u; num2 <= 65535; num2++)
		{
			if (pagesToBeErased.Contains((ushort)num2))
			{
				flag = true;
				if (pagesToBeErased.Count > 1)
				{
					throw new ArgumentException("0x" + num2.ToString("X4") + " special erase command must be the only page.", "pagesToBeErased");
				}
			}
		}
		if (SendStmCommand(new List<byte>().AddStmCommand(StmCommands.EraseExtended)) == 121)
		{
			if (flag)
			{
				if (SendStmCommand(new List<byte>().Add(pagesToBeErased[0]).AddStmChecksum(), 32000) == 121)
				{
					result = true;
				}
			}
			else if (SendStmCommand(new List<byte>().Add(pagesToBeErased).AddStmChecksum(), pagesToBeErased.Count * 3000) == 121)
			{
				result = true;
			}
		}
		return result;
	}

	public string GetLastErrorCode()
	{
		return _lastErrorCode switch
		{
			0u => $"{_lastErrorCode} - No error.", 
			2u => $"{_lastErrorCode} - Checksum fail.", 
			3u => $"{_lastErrorCode} - Unknown command.", 
			4u => $"{_lastErrorCode} - Unlock fail.", 
			5u => $"{_lastErrorCode} - Memory locked.", 
			6u => $"{_lastErrorCode} - Invalid address.", 
			7u => $"{_lastErrorCode} - Invalid length.", 
			8u => $"{_lastErrorCode} - Invalid sectors.", 
			9u => $"{_lastErrorCode} - Serial number.", 
			10u => $"{_lastErrorCode} - Security state.", 
			11u => $"{_lastErrorCode} - Message length.", 
			12u => $"{_lastErrorCode} - Message checksum.", 
			128u => $"{_lastErrorCode} - Memory.", 
			129u => $"{_lastErrorCode} - Memory write.", 
			130u => $"{_lastErrorCode} - Memory erase.", 
			131u => $"{_lastErrorCode} - Memory read.", 
			132u => $"{_lastErrorCode} - Memory program.", 
			136u => $"{_lastErrorCode} - Memory_10VER0.", 
			144u => $"{_lastErrorCode} - Memory sequence.", 
			160u => $"{_lastErrorCode} - Memory resume.", 
			192u => $"{_lastErrorCode} - Test running.", 
			2147483648u => ".", 
			_ => $"{_lastErrorCode} - Unknown error.", 
		};
	}

	public bool Str_Awake()
	{
		return SendStrCommand("Hello");
	}

	public bool Str_Reset()
	{
		return SendStrCommand("Reset");
	}

	private bool Str_GetSeed(bool ShowError = false)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			result = SendStrCommand("Get seed");
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	public bool Str_GetChecksum(int startAddress, int endAddress, out int checksum, bool showError = false)
	{
		bool result = false;
		checksum = 0;
		List<byte> list = new List<byte>();
		for (int num = 3; num >= 0; num--)
		{
			list.Add((byte)(startAddress >> 8 * num));
		}
		for (int num2 = 3; num2 >= 0; num2--)
		{
			list.Add((byte)(endAddress >> 8 * num2));
		}
		if (SendStrCommand("Checksum", list))
		{
			result = true;
			for (int num3 = 3; num3 >= 0; num3--)
			{
				checksum <<= 8;
				checksum |= _returnedData[num3];
			}
		}
		return result;
	}

	public bool Str_Unlock(bool ShowError = false)
	{
		bool result = false;
		if (SendStrCommand("Get seed"))
		{
			List<byte> list = new List<byte>();
			using (ReturnedDataLock.Lock())
			{
				list.Add(_returnedData[0]);
				list.Add(_returnedData[1]);
			}
			uint num = Checksum.Calculate(list, (uint)list.Count, PolynomialUnitType.VideoVBoxAndMfd);
			list.Clear();
			list.Add((byte)(num >> 8));
			list.Add((byte)num);
			result = SendStrCommand("Unlock", list, ShowError);
		}
		return result;
	}

	public bool Str_Upload(List<byte> data, out List<byte> uploadedData, bool showError = false, TextWriter debugFile = null, EncryptionType encryption = EncryptionType.None)
	{
		bool result = false;
		uploadedData = new List<byte>();
		if (SendStrCommand("Upload", data, showError, encryption, debugFile))
		{
			result = true;
			using (ReturnedDataLock.Lock())
			{
				foreach (byte returnedDatum in _returnedData)
				{
					uploadedData.Add(returnedDatum);
				}
			}
		}
		return result;
	}

	public bool Str_Download(List<byte> data, bool showError = false, EncryptionType encryption = EncryptionType.None)
	{
		return SendStrCommand("Download", data, showError, encryption);
	}

	public bool Str_DownloadEnd(bool showError = false, EncryptionType encryption = EncryptionType.None)
	{
		return SendStrCommand("Download end", encryption, showError);
	}

	public bool Str_Erase(short sectorBitField, bool showError = false)
	{
		return Str_Erase("Erase flash", sectorBitField, showError);
	}

	public bool Str_EraseSectorNumber(short sectorNumber, bool showError = false)
	{
		return Str_Erase("Erase flash MkII", sectorNumber, showError);
	}

	public bool Str_MassErase()
	{
		return SendStrCommand("Mass Erase");
	}

	private bool Str_Erase(string command, short data, bool showError = false)
	{
		List<byte> data2 = new List<byte>
		{
			(byte)(data >> 8),
			(byte)data
		};
		return SendStrCommand(command, data2, showError);
	}

	public bool Str_EraseApplication(bool showError = false, EncryptionType encryption = EncryptionType.None)
	{
		return SendStrCommand("Erase App", encryption, showError);
	}

	public bool Str_Initialise(int imageAddress, int imageSize, byte[] ivValue, bool showError = false)
	{
		if (ivValue.Length != 8)
		{
			throw new ArgumentException("ivValue - invalid length");
		}
		List<byte> list = new List<byte>(16);
		for (int num = 3; num >= 0; num--)
		{
			list.Add((byte)(imageAddress >> 8 * num));
		}
		for (int num2 = 3; num2 >= 0; num2--)
		{
			list.Add((byte)(imageSize >> 8 * num2));
		}
		foreach (byte item in ivValue)
		{
			list.Add(item);
		}
		return SendStrCommand("Initialise", list, showError);
	}

	public bool Str_InitialiseTransfer(int dataLength, bool showError = false)
	{
		List<byte> list = new List<byte>(4);
		for (int num = 3; num >= 0; num--)
		{
			list.Add((byte)(dataLength >> 8 * num));
		}
		return SendStrCommand("Initialise progressbar", list, showError);
	}

	public bool Str_GetSerial(UnitInformation unitInfo, bool newBootloader, out bool allowUnitSwapping, bool showError = false)
	{
		bool flag = false;
		allowUnitSwapping = false;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingSerialNumber;
		if (unitInfo == null)
		{
			unitInfo = new UnitInformation();
		}
		if (SendStrCommand("Get serialnumber"))
		{
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 16)
				{
					foreach (byte item in _returnedData.Where((byte b) => b != byte.MaxValue))
					{
						flag = true;
					}
					if (flag)
					{
						if (newBootloader)
						{
							allowUnitSwapping = (_returnedData[8] & 1) != 1;
							unitInfo.UnitType = _returnedData[11];
							unitInfo.SubType = (char)_returnedData[10];
							int num = 0;
							for (int num2 = 15; num2 > 11; num2--)
							{
								num <<= 8;
								num |= _returnedData[num2];
							}
							unitInfo.SerialNumber = num;
						}
						else
						{
							unitInfo.UnitType = _returnedData[8];
							foreach (ModuleDefinition item2 in Modules.List.Where((ModuleDefinition m) => m.UnitType == unitInfo.UnitType))
							{
								unitInfo.SubType = (char)item2.DefaultSubType.Value;
							}
							StringBuilder stringBuilder = new StringBuilder();
							int num3 = 0;
							for (num3 = 9; num3 < 16; num3++)
							{
								stringBuilder.Append((char)_returnedData[num3]);
							}
							if (int.TryParse(stringBuilder.ToString(), out num3))
							{
								unitInfo.SerialNumber = num3;
							}
						}
					}
					else if (showError)
					{
						Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.Str_GetSerial(): serial number is blank.", "SlidingInformationMessage"));
					}
				}
			}
		}
		ProgressText = string.Empty;
		return flag;
	}

	public bool Str_SetSerial(byte[] identity, bool showError = false)
	{
		if (identity.Length != 16)
		{
			Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.Str_SetSerial(): identity invalid length MUST be 16 bytes", "SlidingInformationMessage"));
			return false;
		}
		bool flag = SendStrCommand("Set serialnumber", new List<byte>(identity), showError: true);
		if (!flag && _error != null)
		{
			MessageBox.Show(_error.Error + Environment.NewLine + _error.Description);
		}
		return flag;
	}

	public uint Str_GetHardwareVersion(bool showError = false, EncryptionType encryption = EncryptionType.None)
	{
		uint num = uint.MaxValue;
		if (SendStrCommand("Get hardware", encryption, showError))
		{
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 4)
				{
					for (int i = 0; i < 4; i++)
					{
						num <<= 8;
						num |= _returnedData[i];
					}
				}
			}
		}
		return num;
	}

	public bool Str_SetHardwareVersion(int newHardwareVersion, bool showError = false)
	{
		List<byte> list = new List<byte>(4);
		for (int num = 3; num >= 0; num--)
		{
			list.Add((byte)(newHardwareVersion >> 8 * num));
		}
		return SendStrCommand("Set hardware", list, showError);
	}

	public byte[] Str_GetPcbInformation(UnitInformation unitInfo, bool showError = false)
	{
		byte[] array = new byte[0];
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingPcbInformation;
		if (unitInfo == null)
		{
			unitInfo = new UnitInformation();
		}
		if (SendStrCommand("Get PCB information"))
		{
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 16)
				{
					bool flag = false;
					array = new byte[16];
					using (IEnumerator<byte> enumerator = _returnedData.Where((byte b) => b != byte.MaxValue).GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							byte current = enumerator.Current;
							flag = true;
							for (int i = 0; i < 16; i++)
							{
								array[i] = _returnedData[i];
							}
						}
					}
					if (!flag && showError)
					{
						Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.Str_GetPcbInformation(): PCB informatiom is blank.", "SlidingInformationMessage"));
					}
				}
			}
		}
		ProgressText = string.Empty;
		return array;
	}

	public bool Str_SetPcbInformation(byte[] pcbInformation, bool showError = false)
	{
		if (pcbInformation.Length != 16)
		{
			Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.Str_SetPcbInformation(): PCB information invalid length MUST be 16 bytes", "SlidingInformationMessage"));
			return false;
		}
		bool flag = SendStrCommand("Set PCB information", new List<byte>(pcbInformation), showError);
		if (!flag && _error != null)
		{
			MessageBox.Show(_error.Error + Environment.NewLine + _error.Description);
		}
		return flag;
	}

	public bool Str_GetFlashLayout(out byte[] flashLayout, EncryptionType encryption = EncryptionType.None, bool showError = false)
	{
		string command = "Get Flash layout";
		short value = GetMessageResponseLengthMinusHeaderAndChecksum(command, encryption).Value;
		bool result = false;
		flashLayout = null;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingPcbInformation;
		if (SendStrCommand(command, encryption, showError))
		{
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == value)
				{
					result = true;
					flashLayout = new byte[value];
					for (int i = 0; i < value; i++)
					{
						flashLayout[i] = _returnedData[i];
					}
				}
			}
		}
		return result;
	}

	public bool Str_SetFlashLayout(byte[] flashLayout, EncryptionType encryption = EncryptionType.None, bool showError = false)
	{
		if (flashLayout.Length % 16 != 0)
		{
			Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.Str_SetFlashLayout(): Flash layout information invalid length MUST be divisible by 16 bytes", "SlidingInformationMessage"));
			return false;
		}
		List<byte> list = new List<byte>(flashLayout);
		while (list.Count < 256)
		{
			list.Add(byte.MaxValue);
		}
		bool flag = SendStrCommand("Set Flash layout", list, showError, encryption);
		if (!flag && _error != null)
		{
			MessageBox.Show(_error.Error + Environment.NewLine + _error.Description);
		}
		return flag;
	}

	public bool Str_GetUnitInformation(out byte[] unitInformation, EncryptionType encryption = EncryptionType.None, bool showError = false)
	{
		bool result = false;
		unitInformation = null;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingUnitInformation;
		string command = "Get unit information";
		short value = GetMessageResponseLengthMinusHeaderAndChecksum(command, encryption).Value;
		if (SendStrCommand(command, encryption, showError))
		{
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == value)
				{
					result = true;
					unitInformation = new byte[value];
					for (int i = 0; i < value; i++)
					{
						unitInformation[i] = _returnedData[i];
					}
				}
			}
		}
		return result;
	}

	public bool Str_SetUnitInformation(byte[] unitInformation, EncryptionType encryption = EncryptionType.None, bool showError = false)
	{
		if (unitInformation.Length != 256)
		{
			Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.Str_SetUnitInformation(): Unit information invalid length MUST be 0x100 bytes", "SlidingInformationMessage"));
			return false;
		}
		bool flag = SendStrCommand("Set unit information", new List<byte>(unitInformation), showError, encryption);
		if (!flag && _error != null)
		{
			MessageBox.Show(_error.Error + Environment.NewLine + _error.Description);
		}
		return flag;
	}

	public bool Str_SetSecurity()
	{
		return SendStrCommand("Set security", new List<byte> { 1 });
	}

	public string Str_ClearSecurity()
	{
		return SendStrCommand("Set security", new List<byte> { 0 }) ? Racelogic.Comms.Serial.Properties.Resources.SecurityCleared : Racelogic.Comms.Serial.Properties.Resources.FailedtoClearSecurity;
	}

	public string Str_GetSecurity(bool showError = false)
	{
		string result = Racelogic.Comms.Serial.Properties.Resources.FailedToGetSecurity;
		if (SendStrCommand("Get security"))
		{
			using (ReturnedDataLock.Lock())
			{
				short num = _returnedData[1];
				num = (short)(num << 8);
				num = (short)(num | _returnedData[0]);
				short num2 = _returnedData[3];
				num2 = (short)(num2 << 8);
				num2 = (short)(num2 | _returnedData[2]);
				string text = ((num != num2 || _returnedData[4] != 0) ? Racelogic.Comms.Serial.Properties.Resources.SecurityDisabled : Racelogic.Comms.Serial.Properties.Resources.SecurityEnabled);
				result = string.Format("{1}{0}0x{2}  0x{3}{0}{4}", Environment.NewLine, Racelogic.Comms.Serial.Properties.Resources.SecurityLevelRetrieved, num.ToString("X4"), num2.ToString("X4"), text);
			}
		}
		return result;
	}

	public bool Str_GetBootloaderVersion(int unitType, out bool useNewBootloader, bool showError = false)
	{
		useNewBootloader = false;
		bool flag = false;
		byte[] array = new byte[6];
		if (unitType != 34 && unitType != 36)
		{
			flag = true;
			useNewBootloader = true;
		}
		else
		{
			foreach (ModuleDefinition item in Modules.List.Where((ModuleDefinition module) => module.UnitType == unitType))
			{
				int num = 420;
				array[0] = (byte)((item.Processor == Processor.STR71x) ? 64 : 0);
				for (int num2 = 3; num2 >= 1; num2--)
				{
					array[num2] = (byte)num;
					num >>= 8;
				}
				array[4] = 0;
				array[5] = 4;
				if ((item.ModuleFunctions & ModuleFunction.UsbSerial) != ModuleFunction.UsbSerial)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag && Str_Unlock(showError) && SendStrCommand("Upload", new List<byte>(array)))
		{
			flag = true;
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 4)
				{
					Version version = new Version(_returnedData[0], _returnedData[1], (_returnedData[3] << 8) | _returnedData[2]);
					switch (unitType)
					{
					case 34:
						useNewBootloader = version.CompareTo(new Version(1, 0, 5)) == 1;
						break;
					case 36:
						useNewBootloader = version.CompareTo(new Version(1, 0, 6)) == 1;
						break;
					default:
						useNewBootloader = true;
						break;
					}
				}
			}
		}
		return flag;
	}

	private bool SendStrCommand(string command, EncryptionType encryption = EncryptionType.None, bool showError = false)
	{
		return SendStrCommand(command, null, showError, encryption);
	}

	private bool SendStrCommand(string command, List<byte> data, bool showError = false, EncryptionType encryption = EncryptionType.None, TextWriter debugFile = null)
	{
		InSetup = true;
		bool flag = true;
		bool flag2 = false;
		if (data == null)
		{
			data = new List<byte>();
		}
		foreach (KeyValuePair<string, StrCommandDefinition> item in StrCommands.List.Where((KeyValuePair<string, StrCommandDefinition> cmd) => string.Equals(cmd.Key, command)))
		{
			flag2 = true;
			Queue<byte> queue = new Queue<byte>();
			queue.Enqueue(byte.MaxValue);
			queue.Enqueue(item.Value.Command);
			int count = data.Count;
			int num = ((string.Equals(item.Key, "Upload") && data.Count > 5) ? ((data[4] << 8) + data[5]) : 0);
			switch (encryption)
			{
			case EncryptionType.TripleDes:
				if (data.Count > 0)
				{
					data = new TripleDESEncryption().Encrypt(data.ToArray()).ToList();
				}
				break;
			default:
				throw new ArgumentException($"Encryption mode not supported : {encryption}");
			case EncryptionType.None:
				break;
			}
			if (item.Value.Length.HasValue && encryption == EncryptionType.None)
			{
				queue.Enqueue((byte)(item.Value.Length >> 8).Value);
				queue.Enqueue((byte)item.Value.Length.Value);
			}
			else
			{
				queue.Enqueue((byte)(data.Count + 6 >> 8));
				queue.Enqueue((byte)(data.Count + 6));
			}
			foreach (byte datum in data)
			{
				queue.Enqueue(datum);
			}
			Checksum.Append(queue);
			if (string.Equals(item.Key, "Upload"))
			{
				if (count < 6)
				{
					flag = false;
					Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.SendStrCommand(): " + Racelogic.Comms.Serial.Properties.Resources.InvalidUploadDataCount, Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
					debugFile?.WriteLine("Racelogic.Comms.SendStrCommand(): Invalid data length for STR upload.");
				}
				else
				{
					if (encryption == EncryptionType.TripleDes)
					{
						num += (short)(8 - num % 8);
					}
					_oldProtocolResponseLength = num + 6;
				}
			}
			else if (item.Value.ResponseLength.HasValue)
			{
				_oldProtocolResponseLength = ((item.Value.ResponseLength.Value != 0) ? GetMessageResponseLengthMinusHeaderAndChecksum(command, encryption, includeHeaderAndChecksum: true).Value : 0);
			}
			else
			{
				flag = false;
				Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.SendStrCommand(): " + Racelogic.Comms.Serial.Properties.Resources.UnknownResponseLength, Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
				debugFile?.WriteLine("Racelogic.Comms.SendStrCommand(): Unknown response length for STR upload.");
			}
			if (!flag)
			{
				continue;
			}
			FlushRxTx();
			Racelogic.Core.Win.Helper.WaitForPriority();
			_awaitingStrResponse = true;
			_sentStrCommand = item.Key;
			_crcError = false;
			_responseError = false;
			_error = null;
			if (debugFile != null)
			{
				string text = $"Racelogic.Comms.SendStrCommand(): {command} :";
				foreach (byte item2 in queue)
				{
					text += string.Format("0x{0} ", item2.ToString("X2"));
				}
				debugFile.WriteLine();
				debugFile.WriteLine(text);
				CommsDebugFile = debugFile;
				debugFile.Write("Response : ");
			}
			_OldProtocolState = OldProtocolState.MessageSent;
			DelayTimer delayTimer = new DelayTimer(string.Equals(item.Key, "Erase App") ? 10000 : 5000);
			if (TxData(queue))
			{
				if (_oldProtocolResponseLength != 0)
				{
					while ((_OldProtocolState & OldProtocolState.ReplyReceived) != OldProtocolState.ReplyReceived && delayTimer.IsRunning)
					{
						Racelogic.Core.Win.Helper.WaitForPriority();
					}
				}
				delayTimer.Enabled = false;
				if (!delayTimer.IsRunning)
				{
					flag = false;
					if (showError && this.SerialCommsInformation != null)
					{
						this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.ModuleLowLevel Str " + item.Key + "() :\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout));
					}
					if (showError)
					{
						Messenger.get_Default().Send<InformationMessage>(new InformationMessage($"{Racelogic.Comms.Serial.Properties.Resources.CommunicationError}{Environment.NewLine}Timeout - Str {item.Key}", Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
					}
				}
			}
			else
			{
				delayTimer.Enabled = false;
				flag = false;
				StringBuilder stringBuilder = new StringBuilder("TxData Error- Str " + item.Key);
				if (!IsOpen)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("Port not open.");
				}
				Messenger.get_Default().Send<InformationMessage>(new InformationMessage(stringBuilder.ToString(), Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
			}
			delayTimer = null;
			_awaitingStrResponse = false;
			if (!flag)
			{
				continue;
			}
			if (_crcError)
			{
				flag = false;
				if (showError && this.SerialCommsInformation != null)
				{
					this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.ModuleLowLevel - Str " + item.Key + "() :\r\n" + Racelogic.Comms.Serial.Properties.Resources.CrcError));
				}
				if (showError)
				{
					Messenger.get_Default().Send<InformationMessage>(new InformationMessage($"CRC error - Str {item.Key}", Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
				}
				debugFile?.WriteLine($"CRC error - Str {item.Key}");
			}
			if (!_responseError)
			{
				continue;
			}
			flag = false;
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count != 4)
				{
					continue;
				}
				int num2 = 0;
				for (int i = 0; i < 4; i++)
				{
					num2 <<= 8;
					num2 |= _returnedData[i];
				}
				if (num2 < StrErrors.List.Count)
				{
					_error = StrErrors.List[num2];
					if (showError)
					{
						Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.SendStrCommand(): " + command + Environment.NewLine + Environment.NewLine + StrErrors.List[num2].Error + Environment.NewLine + StrErrors.List[num2].Description, Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
					}
					debugFile?.WriteLine("Racelogic.Comms.SendStrCommand(): " + command + Environment.NewLine + Environment.NewLine + StrErrors.List[num2].Error + Environment.NewLine + StrErrors.List[num2].Description);
				}
			}
		}
		if (!flag2)
		{
			flag = false;
			Messenger.get_Default().Send<InformationMessage>(new InformationMessage("Racelogic.Comms.SendStrCommand(): " + Racelogic.Comms.Serial.Properties.Resources.UnrecognisedCommand + Environment.NewLine + command, Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
			debugFile?.WriteLine("Racelogic.Comms.SendStrCommand(): Unrecognised command : " + command);
		}
		InSetup = false;
		return flag;
	}

	private void ExtractStrResponse()
	{
		if (SetupRxCount < _oldProtocolResponseLength)
		{
			return;
		}
		byte[] array = new byte[_oldProtocolResponseLength];
		for (int i = 0; i < _oldProtocolResponseLength; i++)
		{
			array[i] = setupReceivedDataBuffer[SetupReadIndex++];
		}
		if (CommsDebugFile != null)
		{
			CommsDebugFile.WriteLine("ExtractStrResponse()");
			byte[] array2 = array;
			foreach (byte b in array2)
			{
				CommsDebugFile.Write(string.Format("0x{0} ", b.ToString("X2")));
			}
		}
		List<byte> list = new List<byte>(array);
		using (ReturnedDataLock.Lock())
		{
			_returnedData.Clear();
			if (list[0] == byte.MaxValue)
			{
				if (list[0] == byte.MaxValue && list[1] == 1)
				{
					uint num = Checksum.Calculate(list, (uint)(_oldProtocolResponseLength - 2), PolynomialUnitType.VBox);
					uint num2 = list[list.Count - 2];
					num2 <<= 8;
					num2 |= list[list.Count - 1];
					if (num2 == num)
					{
						int num3 = (list[2] << 8) + list[3];
						if (list.Count == num3 && num3 != 6)
						{
							for (int k = 4; k < list.Count - 2; k++)
							{
								_returnedData.Add(list[k]);
							}
						}
					}
					else
					{
						_crcError = true;
					}
				}
				else if (list[0] == byte.MaxValue && list[1] == 0)
				{
					_responseError = true;
					DelayTimer delayTimer = new DelayTimer(5000);
					while (RxCount < 10 - _oldProtocolResponseLength && SetupRxCount < 10 - _oldProtocolResponseLength && delayTimer.IsRunning)
					{
						Racelogic.Core.Win.Helper.WaitForPriority();
					}
					delayTimer.Enabled = false;
					delayTimer = null;
					if (RxCount >= 10 - _oldProtocolResponseLength)
					{
						array = Read(10 - _oldProtocolResponseLength);
						for (int l = 0; l < array.Length; l++)
						{
							list.Add(array[l]);
						}
						SetLastError(list);
					}
					else if (SetupRxCount >= 10 - _oldProtocolResponseLength)
					{
						while (SetupRxCount > 0)
						{
							list.Add(setupReceivedDataBuffer[SetupReadIndex++]);
						}
						SetLastError(list);
					}
				}
			}
			else
			{
				_responseError = true;
			}
		}
		_OldProtocolState |= OldProtocolState.ReplyReceived;
	}

	private void SetLastError(List<byte> data)
	{
		if (data.Count >= 8)
		{
			uint num = 0u;
			for (int i = 0; i < 4; i++)
			{
				num <<= 8;
				num |= data[i + 4];
			}
			_lastErrorCode = num;
		}
	}

	private short? GetMessageResponseLengthMinusHeaderAndChecksum(string command, EncryptionType encryption, bool includeHeaderAndChecksum = false)
	{
		short? num = StrCommands.List.FirstOrDefault((KeyValuePair<string, StrCommandDefinition> cmd) => string.Equals(cmd.Key, command)).Value.ResponseLength;
		if (num.HasValue)
		{
			num = (short?)(num - 6);
			if (num > 0 && encryption == EncryptionType.TripleDes)
			{
				num = (short?)(num + (short)(8 - num.Value % 8));
			}
			if (includeHeaderAndChecksum)
			{
				num = (short?)(num + 6);
			}
		}
		return num;
	}

	public bool ClearLaneDataInFlash(int laneNumber, Action<string> updateStatus, byte[] flashInfoData = null)
	{
		if (laneNumber < 1 || laneNumber > 3)
		{
			throw new ArgumentException("ClearLaneDataInFlash - laneNumber must be between 1 and 3");
		}
		FlashInfoDefinition flashInfoFromLaneNumber = GetFlashInfoFromLaneNumber(laneNumber, 0u, flashInfoData);
		uint flashLaneAddress = GetFlashLaneAddress(flashInfoFromLaneNumber, laneNumber);
		updateStatus?.Invoke(string.Format("Clearing data for lane {0}, Address : 0x{1}", laneNumber, flashLaneAddress.ToString("X8")));
		Unlock_StandardProtocol(ShowError: false);
		bool result = ErasePage_StandardProtocol(flashLaneAddress, flashInfoFromLaneNumber);
		Lock_StandardProtocol();
		return result;
	}

	public List<byte> DownloadLaneDataFromFlash(int laneNumber, Action<string> updateStatus, byte[] flashInfoData = null)
	{
		if (laneNumber < 1 || laneNumber > 3)
		{
			throw new ArgumentException("DownloadLaneDataFromFlash - laneNumber must be between 1 and 3");
		}
		List<byte> list = null;
		List<byte> list2 = new List<byte>(4096);
		FlashInfoDefinition flashInfoFromLaneNumber = GetFlashInfoFromLaneNumber(laneNumber, 0u, flashInfoData);
		uint num = GetFlashLaneAddress(flashInfoFromLaneNumber, laneNumber);
		updateStatus?.Invoke(string.Format("Downloading data for lane {0}, Address : 0x{1}", laneNumber, num.ToString("X8")));
		try
		{
			if (flashInfoData == null)
			{
				Unlock_StandardProtocol(ShowError: false);
			}
			list2 = DownloadFromUnit4KBlocks_StandardProtocol(flashInfoFromLaneNumber, num, unlockRequired: false);
		}
		catch (Exception ex)
		{
			updateStatus?.Invoke($"0) lane {laneNumber} {ex.Message}");
		}
		finally
		{
			if (flashInfoData == null)
			{
				Lock_StandardProtocol();
			}
		}
		if (list2 != null && list2.Count == 4096)
		{
			int num2 = 0;
			for (int i = 64; i < 68; i++)
			{
				if (list2[i] != byte.MaxValue)
				{
					for (int j = 0; j < 4; j++)
					{
						num2 <<= 8;
						num2 |= list2[j + 64];
					}
					break;
				}
			}
			int num3 = 68 + num2 * 16;
			int num4 = num3 / 4096;
			if (num3 % 4096 != 0)
			{
				num4++;
			}
			list = new List<byte>(num4 * 4096);
			list.AddRange(list2);
			for (int k = 1; k < num4; k++)
			{
				num += 4096;
				updateStatus?.Invoke(string.Format("Downloading data for lane {0}, Address : 0x{1}", laneNumber, num.ToString("X8")));
				try
				{
					list2 = null;
					list2 = DownloadFromUnit4KBlocks_StandardProtocol(flashInfoFromLaneNumber, num, unlockRequired: false);
				}
				catch (Exception ex2)
				{
					updateStatus?.Invoke($"{k}) lane {laneNumber} {ex2.Message}");
				}
				if (list2 != null && list2.Count == 4096)
				{
					list.AddRange(list2);
					continue;
				}
				list.Clear();
				break;
			}
		}
		if (list == null || list.Count == 0)
		{
			updateStatus?.Invoke(string.Format(Racelogic.Comms.Serial.Properties.Resources.DownloadLaneError, laneNumber));
		}
		return list;
	}

	public bool GetFlashInfo(out byte[] flashInfoData)
	{
		bool flag = false;
		int num = 3;
		int num2 = Marshal.SizeOf(typeof(FlashInfoDefinition));
		while (!flag && num-- > 0)
		{
			if (!RequestVBOXData(7, 59, null, num2 + 5, 1500, ShowError: false, "GetFlashInfo"))
			{
				continue;
			}
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 22 && _returnedData[0] == 1)
				{
					flag = true;
				}
			}
		}
		flashInfoData = (flag ? _returnedData.Skip(2).Take(num2).ToArray() : null);
		return flag;
	}

	public bool UploadLaneDataToFlash(int laneNumber, List<byte> data, Action<string> updateStatus, byte[] flashInfoData = null)
	{
		if (laneNumber < 1 || laneNumber > 3)
		{
			throw new ArgumentException("UploadLaneDataToFlash - laneNumber must be between 1 and 3");
		}
		bool flag = true;
		int num = data.Count % 4096;
		if (num > 0)
		{
			num = 4096 - num;
		}
		while (num-- > 0)
		{
			data.Add(0);
		}
		FlashInfoDefinition flashInfoFromLaneNumber = GetFlashInfoFromLaneNumber(laneNumber, (uint)data.Count, flashInfoData);
		uint num2 = GetFlashLaneAddress(flashInfoFromLaneNumber, laneNumber);
		if (data.Count % 4096 != 0)
		{
			throw new ArgumentException("Racelogic.Comms.Serial.UploadLaneDataToFlash(int laneNumber, List<byte> data, bool showError) - invalid length of data");
		}
		int count = data.Count;
		int num3 = 0;
		updateStatus?.Invoke(string.Format("Erasing data for lane {0}, Address : 0x{1}", laneNumber, num2.ToString("X8")));
		if (flashInfoData == null)
		{
			Unlock_StandardProtocol(ShowError: false);
		}
		if (ErasePage_StandardProtocol(num2, flashInfoFromLaneNumber))
		{
			while (flag && num3 < count)
			{
				if (Upload4KToUnit(num2, data.Skip(num3).Take(4096).ToList()))
				{
					num2 += 4096;
					num3 += 4096;
					PercentComplete = num3 * 100 / count;
				}
				else
				{
					flag = false;
					updateStatus?.Invoke(string.Format("Failed to upload data for lane {0}, Failed to write to address 0x{1}", laneNumber, num2.ToString("X8")));
				}
			}
		}
		else
		{
			flag = false;
			updateStatus?.Invoke($"Failed to erase data for lane {laneNumber}.");
		}
		if (flashInfoData == null)
		{
			Lock_StandardProtocol();
		}
		return flag;
	}

	private List<byte> DownloadFromUnit4KBlocks_StandardProtocol(FlashInfoDefinition flashInfo, uint address, bool unlockRequired, uint numberOfBlocks = 1u)
	{
		List<byte> list = new List<byte>((int)(4096 * numberOfBlocks));
		List<byte> list2 = new List<byte>(4096);
		Queue<byte> queue = new Queue<byte>();
		int num = 2;
		bool flag = false;
		for (int i = 0; i < numberOfBlocks; i++)
		{
			queue.Clear();
			num = 2;
			flag = false;
			queue.Enqueue((byte)(address >> 24));
			queue.Enqueue((byte)(address >> 16));
			queue.Enqueue((byte)(address >> 8));
			queue.Enqueue((byte)address);
			while (!flag && num-- > 0)
			{
				if (RequestVBOXData(null, 45, queue, 4100, 2500, ShowError: false, "DownloadFromUnit4K_StandardProtocol"))
				{
					using (ReturnedDataLock.Lock())
					{
						if (_returnedData.Count == 4096)
						{
							list.AddRange(_returnedData);
							_returnedData.Clear();
							flag = true;
						}
					}
					address += 4096;
				}
				else
				{
					Thread.Sleep(10);
				}
			}
			if (!flag)
			{
				break;
			}
		}
		if (!flag)
		{
			list.Clear();
		}
		return list;
	}

	private bool ErasePage_StandardProtocol(uint address, FlashInfoDefinition? flashInfo = null)
	{
		bool flag = false;
		if (!flashInfo.HasValue)
		{
			flashInfo = GetFlashInfoFromAddress(address);
		}
		address = (address - flashInfo.Value.FlashBase) / flashInfo.Value.FlashSectorSize * flashInfo.Value.FlashSectorSize + flashInfo.Value.FlashBase;
		Queue<byte> queue = new Queue<byte>(4);
		queue.Enqueue((byte)(address >> 24));
		queue.Enqueue((byte)(address >> 16));
		queue.Enqueue((byte)(address >> 8));
		queue.Enqueue((byte)address);
		int num = 3;
		while (!flag && num-- > 0)
		{
			if (!RequestVBOXData(null, 46, queue, 4, 3500, num == 0, "ErasePage_StandardProtocol"))
			{
				continue;
			}
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 1 && _returnedData[0] == 1)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	private bool GetFlashInfo(out FlashInfoDefinition flashInfo)
	{
		byte[] flashInfoData;
		bool flashInfo2 = GetFlashInfo(out flashInfoData);
		flashInfo = (flashInfo2 ? ((FlashInfoDefinition)StructureFunctions.Create(flashInfoData, typeof(FlashInfoDefinition))) : default(FlashInfoDefinition));
		return flashInfo2;
	}

	private FlashInfoDefinition GetFlashInfoFromAddress(uint address, uint length = 0u)
	{
		if (GetFlashInfo(out FlashInfoDefinition flashInfo))
		{
			if (address < flashInfo.FlashBase || address + length >= flashInfo.FlashBase + flashInfo.FlashSize)
			{
				throw new ArgumentException("Flash Address is invalid 0x" + address.ToString("X8"));
			}
			return flashInfo;
		}
		throw new Exception("Unable to read flash infor from unit.");
	}

	private FlashInfoDefinition GetFlashInfoFromLaneNumber(int laneNumber, uint length = 0u, byte[] flashInfoData = null)
	{
		if (laneNumber < 1 || laneNumber > 3)
		{
			throw new ArgumentException("laneNumber must be between 1 and 3");
		}
		bool flag = false;
		FlashInfoDefinition flashInfo;
		if (flashInfoData != null)
		{
			flashInfo = (FlashInfoDefinition)StructureFunctions.Create(flashInfoData, typeof(FlashInfoDefinition));
			flag = true;
		}
		else
		{
			flag = GetFlashInfo(out flashInfo);
		}
		if (flag)
		{
			if (length > flashInfo.FlashSectorSize)
			{
				throw new ArgumentException("CheckFlashAddressIsValid - data length exceeds sector size.");
			}
			return flashInfo;
		}
		throw new Exception("Unable to read flash info from unit.");
	}

	private uint GetFlashLaneAddress(FlashInfoDefinition flashInfo, int laneNumber)
	{
		if (laneNumber < 1 || laneNumber > 3)
		{
			throw new ArgumentException("laneNumber must be between 1 and 3");
		}
		return (uint)(flashInfo.FlashBase + flashInfo.FlashSectorSize * (laneNumber - 1));
	}

	private bool Upload4KToUnit(uint address, List<byte> data)
	{
		bool flag = false;
		Queue<byte> queue = new Queue<byte>(4106);
		queue.Enqueue((byte)(address >> 24));
		queue.Enqueue((byte)(address >> 16));
		queue.Enqueue((byte)(address >> 8));
		queue.Enqueue((byte)address);
		for (int i = 0; i < 3; i++)
		{
			queue.Enqueue(0);
		}
		foreach (byte datum in data)
		{
			queue.Enqueue(datum);
		}
		int num = 3;
		while (!flag && num-- > 0)
		{
			if (!RequestVBOXData(null, 44, queue, 4, 3500, num == 0, "UploadFromUnit4K_StandardProtocol"))
			{
				continue;
			}
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 1 && _returnedData[0] == 1)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	private List<CanChannel> GetInternalA2DChannels()
	{
		List<CanChannel> list = new List<CanChannel>(4);
		for (int i = 0; i < 4; i++)
		{
			list.Add(new CanChannel());
		}
		List<byte> list2 = DownloadFromUnit_StandardProtocol(3924, 172);
		int num = 3;
		while (list2.Count == 0 && num-- > 0)
		{
			Thread.Sleep(100);
			list2 = DownloadFromUnit_StandardProtocol(3924, 172);
		}
		InternalA2DChannels internalA2DChannels = (InternalA2DChannels)StructureFunctions.Create(list2.ToArray(), typeof(InternalA2DChannels));
		list2 = DownloadFromUnit_StandardProtocol(74, 4, ShowError: false);
		if (list2.Count == 4)
		{
			uint num2 = 0u;
			for (int num3 = 3; num3 >= 0; num3--)
			{
				num2 <<= 8;
				num2 |= list2[num3];
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j].ChannelNumber = (byte)j;
				list[j].Name = internalA2DChannels.InternalAnalogue[j].Name.Data;
				list[j].SerialNumber = (uint)VBoxData.SerialNumber;
				list[j].Units = internalA2DChannels.InternalAnalogue[j].Units.Data;
				list[j].IsBeingSentOverSerial = (num2 & (1 << j + 12)) == 1 << j + 12;
			}
		}
		return list;
	}

	public bool Get_AvailableCANChannels_StandardProtocol(bool ShowError)
	{
		bool result = false;
		List<CanChannel> list = new List<CanChannel>();
		List<CanChannel> list2 = new List<CanChannel>(4);
		for (int i = 0; i < 4; i++)
		{
			list2.Add(new CanChannel());
		}
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			list2 = GetInternalA2DChannels();
			List<byte> list3 = DownloadFromUnit_StandardProtocol(256, 192);
			if (list3.Count == 192)
			{
				ModulesFound modulesFound = (ModulesFound)StructureFunctions.Create(list3.ToArray(), typeof(ModulesFound));
				list3 = DownloadFromUnit_StandardProtocol(78, 4, ShowError);
				if (list3.Count == 4)
				{
					uint num = 0u;
					for (int num2 = 3; num2 >= 0; num2--)
					{
						num <<= 8;
						num |= list3[num2];
					}
					list3 = DownloadFromUnit_StandardProtocol(1536, 832, ShowError);
					if (list3.Count == 832)
					{
						ChannelsFound channelsFound = (ChannelsFound)StructureFunctions.Create(list3.ToArray(), typeof(ChannelsFound));
						result = true;
						bool flag2 = false;
						for (int j = 0; j < channelsFound.CanModuleConfiguration.Length; j++)
						{
							if (channelsFound.CanModuleConfiguration[j].SerialNumber == 0)
							{
								continue;
							}
							flag2 = false;
							CanModulesDefinition[] canModules = modulesFound.CanModules;
							for (int k = 0; k < canModules.Length; k++)
							{
								CanModulesDefinition canModulesDefinition = canModules[k];
								if (canModulesDefinition.SerialNumber == channelsFound.CanModuleConfiguration[j].SerialNumber)
								{
									flag2 = true;
									list.Add(CreateCanTelemetryChannel(canModulesDefinition.UnitType, channelsFound.CanModuleConfiguration[j], num, j));
									break;
								}
							}
							if (!flag2)
							{
								list.Add(CreateCanTelemetryChannel(0, channelsFound.CanModuleConfiguration[j], num, j));
							}
						}
					}
				}
			}
		}
		if (CanChannelInformationState == CanChannelInformationStatus.AwaitingResponse)
		{
			CanChannelInformationState = CanChannelInformationStatus.Received;
		}
		CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, list, list2);
		list = null;
		list2 = null;
		if (flag)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		return result;
	}

	private CanChannel CreateCanTelemetryChannel(byte unitType, CanModuleConfigurationDefinition channel, uint telemetryStatus, int index)
	{
		return new CanChannel
		{
			UnitType = unitType,
			SerialNumber = (uint)channel.SerialNumber,
			ChannelNumber = channel.ChannelNumber,
			IsBeingSentOverSerial = ((int)(telemetryStatus & (1 << index)) == 1 << index),
			Name = channel.Name.Data,
			Units = channel.Units.Data
		};
	}

	private bool CanExecuteGetCanChannelInformation(object param)
	{
		return !InSetup && !RequestingCanChannelInformation && _OldProtocolState == OldProtocolState.Idle && !_requestedCommand.HasValue && !_requestedVBOXSubCommand.HasValue;
	}

	private void ExecuteGetCanChannelInformation(object param)
	{
		Task.Factory.StartNew(delegate
		{
			RequestCanChannelInformation();
		});
	}

	private bool CanExecuteGetGpsLatency(object param)
	{
		return !InSetup && !RequestingCanChannelInformation && _OldProtocolState == OldProtocolState.Idle && !_requestedCommand.HasValue && !_requestedVBOXSubCommand.HasValue && vBoxType != VBoxType.Unknown;
	}

	private void ExecuteGetGpsLatency(object param)
	{
		Task.Factory.StartNew(() => GpsLatency = RetrieveGpsLatency(vBoxType, VBoxData.DualAntenna));
	}

	private void RequestCanChannelInformation()
	{
		if (IsInSimulatorMode)
		{
			RequestingCanChannelInformation = true;
			NoCommsTimer.Stop();
			CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, ReadSimulatorChannels(), null);
			RequestingCanChannelInformation = false;
			NoCommsTimer.Start();
			return;
		}
		int num = 0;
		RequestingCanChannelInformation = true;
		NoCommsTimer.Stop();
		if (!SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			num |= 1;
		}
		if (!CloseLogFile(closeFile: true))
		{
			num |= 2;
		}
		if (!ReScanCan_StandardProtocol())
		{
			num |= 4;
		}
		if (!Get_AvailableCANChannels_StandardProtocol(ShowError: false))
		{
			num |= 8;
		}
		RequestingCanChannelInformation = false;
		if (!CloseLogFile(closeFile: false))
		{
			num |= 0x20;
		}
		if (!SetQuiet_StandardProtocol(MakeQuiet: false))
		{
			num |= 0x10;
		}
		HeaderFound = Header.None;
		MaxRxCount = 0;
		if (num != 0)
		{
			Console.WriteLine(string.Format("Error   0x{0}", num.ToString("X2")));
		}
		NoCommsTimer.Start();
	}

	private List<CanChannel> ReadSimulatorChannels()
	{
		List<CanChannel> list = new List<CanChannel>();
		string text = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "GPS simultator CAN channels.xml");
		if (File.Exists(text))
		{
			XElement xElement = XElement.Load(text);
			if (string.Equals(xElement.Name.LocalName, "GPSSimulator"))
			{
				XElement xElement2 = xElement.Elements().FirstOrDefault((XElement can) => string.Equals(can.Name.LocalName, "CANChannels"));
				if (xElement2 != null)
				{
					foreach (XElement item2 in from c in xElement2.Elements()
						where string.Equals(c.Name.LocalName, "Channel")
						select c)
					{
						string text2 = string.Empty;
						string units = string.Empty;
						byte b = 0;
						uint serialNumber = 0u;
						foreach (XAttribute item3 in item2.Attributes())
						{
							switch (item3.Name.LocalName)
							{
							case "Name":
								text2 = item3.Value;
								break;
							case "SerialNumber":
								serialNumber = Convert.ToUInt32(item3.Value);
								break;
							case "Units":
								units = item3.Value;
								break;
							case "UnitType":
								b = Convert.ToByte(item3.Value);
								break;
							}
						}
						if (!string.IsNullOrEmpty(text2) && b != 0)
						{
							CanChannel item = new CanChannel
							{
								Name = text2,
								Units = units,
								SerialNumber = serialNumber,
								UnitType = b,
								IsBeingSentOverSerial = true
							};
							list.Add(item);
							continue;
						}
						throw new Exception("Error in GPS simulator file.  All channels must have a name and unit type");
					}
				}
			}
		}
		return list;
	}

	private List<byte> Download_StandardProtocol(int Address, byte NumberOfBytes)
	{
		return Download_StandardProtocol(Address, NumberOfBytes, ShowError: true);
	}

	private List<byte> Download_StandardProtocol(int Address, byte NumberOfBytes, bool ShowError)
	{
		List<byte> list = new List<byte>();
		int num = ((NumberOfBytes == 0) ? 256 : NumberOfBytes);
		Queue<byte> queue = new Queue<byte>();
		queue.Enqueue((byte)(Address >> 16));
		queue.Enqueue((byte)(Address >> 8));
		queue.Enqueue((byte)Address);
		queue.Enqueue(NumberOfBytes);
		int num2 = 2;
		bool flag = false;
		while (!flag && num2-- > 0)
		{
			if (RequestVBOXData(null, 4, queue, num + 5, 1500, num2 == 0 && ShowError, "Download_StandardProtocol"))
			{
				using (ReturnedDataLock.Lock())
				{
					if (_returnedData.Count != num)
					{
						continue;
					}
					foreach (byte returnedDatum in _returnedData)
					{
						list.Add(returnedDatum);
					}
					_returnedData.Clear();
					flag = true;
				}
			}
			else
			{
				Thread.Sleep(10);
			}
		}
		if (!flag && ShowError)
		{
			Messenger.get_Default().Send<InformationMessage>(new InformationMessage(string.Format(Racelogic.Comms.Serial.Properties.Resources.DownloadEepromError, Address), Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
		}
		return list;
	}

	public List<byte> DownloadFromUnit_StandardProtocol(int Address, int NumberOfBytes)
	{
		return DownloadFromUnit_StandardProtocol(Address, NumberOfBytes, ShowError: true, unlockRequired: true, 220);
	}

	public List<byte> DownloadFromUnit_StandardProtocol(int Address, int NumberOfBytes, bool ShowError)
	{
		return DownloadFromUnit_StandardProtocol(Address, NumberOfBytes, ShowError, unlockRequired: true, 220);
	}

	public List<byte> DownloadFromUnit_StandardProtocol(int Address, int NumberOfBytes, bool ShowError, bool unlockRequired)
	{
		return DownloadFromUnit_StandardProtocol(Address, NumberOfBytes, ShowError, unlockRequired, 220);
	}

	public List<byte> DownloadFromUnit_StandardProtocol(int Address, int NumberOfBytes, bool ShowError, bool unlockRequired, byte maximumPayload)
	{
		int num = 0;
		double num2 = 0.0;
		long ticks = DateTime.Now.Ticks;
		List<byte> list = new List<byte>();
		int num3 = NumberOfBytes;
		uint num4 = 0u;
		bool flag = isReceivingVBoxComms;
		bool flag2 = true;
		ushort num5 = (ushort)((maximumPayload == 0) ? 256 : maximumPayload);
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (!unlockRequired || Unlock_StandardProtocol(ShowError: false))
			{
				while (flag2 && NumberOfBytes > num5)
				{
					num2 = 100.0 - (double)(num3 - num4) / (double)num3 * 100.0;
					if (Math.Round(num2) > (double)num && TimeSpan.FromTicks(DateTime.Now.Ticks - ticks) > TimeSpan.FromSeconds(2.0))
					{
						ticks = DateTime.Now.Ticks;
						num = (int)Math.Round(num2);
						PercentComplete = num2;
						ProgressText = Racelogic.Comms.Serial.Properties.Resources.DownloadDataFromUnit;
					}
					int num6 = 2;
					flag2 = false;
					while (!flag2 && num6-- > 0)
					{
						List<byte> list2 = Download_StandardProtocol(Address, maximumPayload, ShowError = num6 == 0);
						if (list2.Count == num5)
						{
							flag2 = true;
							foreach (byte item in list2)
							{
								list.Add(item);
							}
							Address += num5;
							num4 += num5;
							NumberOfBytes -= num5;
							Thread.Sleep((Address >= 135168) ? 5 : 10);
						}
						else
						{
							Thread.Sleep(50);
						}
					}
				}
				if (flag2 && NumberOfBytes > 0)
				{
					List<byte> list2 = Download_StandardProtocol(Address, (byte)NumberOfBytes, ShowError);
					if (list2.Count == NumberOfBytes)
					{
						foreach (byte item2 in list2)
						{
							list.Add(item2);
						}
						PercentComplete = 100.0;
						ProgressText = Racelogic.Comms.Serial.Properties.Resources.DownloadDataFromUnit;
					}
					else
					{
						flag2 = false;
					}
				}
				if (unlockRequired)
				{
					Lock_StandardProtocol();
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		PercentComplete = 0.0;
		ProgressText = string.Empty;
		if (!flag2)
		{
			list.Clear();
		}
		return list;
	}

	private bool Erase_Standard(int Address, byte NumberOfBytes)
	{
		bool result = false;
		Queue<byte> queue = new Queue<byte>(3);
		queue.Enqueue((byte)(Address >> 16));
		queue.Enqueue((byte)(Address >> 8));
		queue.Enqueue((byte)Address);
		if (RequestVBOXData(null, 16, queue, 4, 7500, ShowError: true, "Erase_Standard"))
		{
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 1 && _returnedData[0] == 1)
				{
					result = true;
				}
			}
		}
		return result;
	}

	private bool EraseBlock_Standard(int Address, BlockSizes BlockSize)
	{
		bool flag = false;
		Queue<byte> queue = new Queue<byte>(4);
		queue.Enqueue((byte)(Address >> 16));
		queue.Enqueue((byte)(Address >> 8));
		queue.Enqueue((byte)Address);
		queue.Enqueue((byte)BlockSize);
		int num = 3;
		while (!flag && num-- > 0)
		{
			if (!RequestVBOXData(null, 20, queue, 4, 1500, num == 0, "EraseBlock_Standard"))
			{
				continue;
			}
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 1 && _returnedData[0] == 1)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	public bool ProgramFlash_StandardProtocol(int address, List<byte> data)
	{
		string error;
		return ProgramFlash_StandardProtocol(address, data, out error);
	}

	public bool ProgramFlash_StandardProtocol(int address, List<byte> data, out string error)
	{
		bool flag = false;
		error = string.Empty;
		if (data.Count <= 256)
		{
			byte numberOfBytes = (byte)((data.Count < 256) ? ((byte)data.Count) : 0);
			if (Erase_Standard(address, numberOfBytes))
			{
				List<byte> list = Download_StandardProtocol(address, numberOfBytes);
				if (list.Count == data.Count)
				{
					flag = true;
					foreach (byte item in list)
					{
						if (item != byte.MaxValue)
						{
							flag = false;
							error = Racelogic.Comms.Serial.Properties.Resources.EraseDataFail + Environment.NewLine + Racelogic.Comms.Serial.Properties.Resources.EraseDataVerifyFail;
							break;
						}
					}
					if (flag)
					{
						flag = false;
						if (Upload_StandardProtocol(address, data, 3))
						{
							list = new List<byte> { 1 };
							if (Upload_StandardProtocol(address, list, 8))
							{
								Thread.Sleep(20);
								list = Download_StandardProtocol(address, numberOfBytes);
								if (list.Count == data.Count)
								{
									flag = true;
									for (int i = 0; i < data.Count; i++)
									{
										if (list[i] != data[i])
										{
											flag = false;
											error = Racelogic.Comms.Serial.Properties.Resources.ProgramFlashFail + Environment.NewLine + Racelogic.Comms.Serial.Properties.Resources.UploadDataVerifyFail;
											break;
										}
									}
								}
							}
							else
							{
								error = Racelogic.Comms.Serial.Properties.Resources.ProgramFlashFail + Environment.NewLine + Racelogic.Comms.Serial.Properties.Resources.UploadConfirmationFail;
							}
						}
						else
						{
							error = Racelogic.Comms.Serial.Properties.Resources.UploadConfirmationFail;
						}
					}
				}
			}
			else
			{
				error = Racelogic.Comms.Serial.Properties.Resources.EraseDataFail;
			}
		}
		return flag;
	}

	public bool Erase_StandardProtocol(int Address, int NumberOfBytes)
	{
		return Erase_StandardProtocol(Address, NumberOfBytes, ShowError: true);
	}

	public bool Erase_StandardProtocol(int Address, int NumberOfBytes, bool ShowError)
	{
		bool flag = false;
		int oldPercent = 0;
		double num = 0.0;
		long TimeStamp = DateTime.Now.Ticks;
		int num2 = NumberOfBytes;
		bool flag2 = isReceivingVBoxComms;
		if (!flag2 || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (Unlock_StandardProtocol(ShowError))
			{
				flag = true;
				int num3 = 3;
				while (NumberOfBytes > 220 && flag)
				{
					num = (double)(num2 - NumberOfBytes) / (double)num2 * 100.0;
					UpdateProgress(ref oldPercent, num, ref TimeStamp);
					flag = Erase_Standard(Address, 220);
					if (flag)
					{
						Address += 220;
						NumberOfBytes -= 220;
					}
					else if (--num3 > 0)
					{
						flag = true;
					}
				}
				while (NumberOfBytes > 0)
				{
					num = (PercentComplete = (double)(num2 - NumberOfBytes) / (double)num2 * 100.0);
					ProgressText = Racelogic.Comms.Serial.Properties.Resources.UploadDataToUnit;
					flag = Erase_Standard(Address, (byte)NumberOfBytes);
					if (flag)
					{
						NumberOfBytes = 0;
					}
					else if (--num3 > 0)
					{
						flag = true;
					}
					PercentComplete = 100.0;
					ProgressText = Racelogic.Comms.Serial.Properties.Resources.EraseData;
				}
				Lock_StandardProtocol();
			}
			if (flag2)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return flag;
	}

	public bool EraseBlock_StandardProtocol(int Address, int NumberOfBytes, BlockSizes BlockSize)
	{
		return EraseBlock_StandardProtocol(Address, NumberOfBytes, BlockSize, ShowError: true);
	}

	public bool EraseBlock_StandardProtocol(int Address, int NumberOfBytes, BlockSizes BlockSize, bool ShowError)
	{
		bool flag = false;
		int oldPercent = 0;
		double num = 0.0;
		long TimeStamp = DateTime.Now.Ticks;
		bool flag2 = isReceivingVBoxComms;
		int num2 = Address + NumberOfBytes;
		int num3 = num2 % (int)BlockSize;
		num2 = (int)(num2 + (BlockSize - num3));
		num3 = Address % (int)BlockSize;
		int num4 = Address - num3;
		int num5 = (num2 - num4) / (int)BlockSize;
		Address = num4;
		if (!flag2 || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (Unlock_StandardProtocol(ShowError))
			{
				flag = true;
				int num6 = 0;
				while (flag && num6++ < num5)
				{
					num = (double)(num5 - num6) / (double)num5 * 100.0;
					UpdateProgress(ref oldPercent, num, ref TimeStamp);
					flag = EraseBlock_Standard(Address, BlockSize);
					Address = (int)(Address + BlockSize);
				}
				Lock_StandardProtocol();
			}
			if (flag2)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return flag;
	}

	public static int GetTimeout(int rxTimeout, SerialBaudRate baudRate)
	{
		int num = 1;
		if (baudRate < SerialBaudRate.br115200)
		{
			num = 2;
			if (baudRate < SerialBaudRate.br28800)
			{
				num = 3;
				if (baudRate < SerialBaudRate.br9600)
				{
					num = 4;
				}
			}
		}
		return rxTimeout * num;
	}

	private bool RequestVBOXData(int? Command, int SubCommand, Queue<byte> data, int? ResponseLength, int rxTimeout, bool ShowError, string FunctionName, short? unitType = null, SerialBaudRate? replyBaudRate = null)
	{
		while (_requestedCommand.HasValue && _requestedVBOXSubCommand.HasValue)
		{
			Racelogic.Core.Win.Helper.WaitForPriority();
		}
		bool result = true;
		int num = ((!Command.HasValue) ? 4 : 5);
		int num2 = ((data == null) ? num : (num + data.Count));
		_requestedCommand = Command;
		_requestedVBOXSubCommand = SubCommand;
		_oldProtocolResponseLength = ResponseLength ?? int.MaxValue;
		Queue<byte> queue = ((data != null) ? new Queue<byte>(num + data.Count) : new Queue<byte>(num));
		if (Command.HasValue)
		{
			queue.Enqueue((byte)Command.Value);
			queue.Enqueue((byte)num2);
			queue.Enqueue((byte)SubCommand);
			if (string.Equals(FunctionName, "GetSerialNumber_StandardProtocolExtended"))
			{
				_requestingVariableLengthResponse = true;
			}
		}
		else
		{
			queue.Enqueue((byte)SubCommand);
			if (string.Equals(FunctionName, "SendGpsEngineMessage_StandardProtocol"))
			{
				_requestingVariableLengthResponse = true;
				queue.Enqueue((byte)data.Count);
			}
			else if ((byte)SubCommand != 3 && (byte)SubCommand != 21 && (byte)SubCommand != 44)
			{
				queue.Enqueue((byte)num2);
			}
			else if (SubCommand != 44)
			{
				queue.Enqueue((byte)(data.Count - 3));
			}
		}
		if (data != null)
		{
			foreach (byte datum in data)
			{
				queue.Enqueue(datum);
			}
		}
		_crcError = false;
		Checksum.Append(queue);
		FlushRxTx();
		using (ReturnedDataLock.Lock())
		{
			_returnedData.Clear();
		}
		_OldProtocolState = OldProtocolState.MessageSent;
		if (string.Equals("Download_StandardProtocol", FunctionName))
		{
			_downloadingEEPROM = true;
			txTime = DateTime.Now.Ticks;
		}
		InSetup = true;
		if (!ResponseLength.HasValue || ResponseLength.Value != 0)
		{
			rxTimeout = ((Command.HasValue && Command.Value == 7 && SubCommand == 2) ? rxTimeout : GetTimeout(rxTimeout, Settings.BaudRate));
			if (TxData(queue))
			{
				if (replyBaudRate.HasValue && BaudRate != replyBaudRate)
				{
					BaudRate = replyBaudRate.Value;
				}
				long ticks = DateTime.Now.Ticks;
				bool flag = true;
				while ((_OldProtocolState & OldProtocolState.ReplyReceived) != OldProtocolState.ReplyReceived && flag)
				{
					Thread.Sleep(1);
					if (TimeSpan.FromTicks(DateTime.Now.Ticks - ticks) > TimeSpan.FromMilliseconds(rxTimeout))
					{
						flag = false;
						break;
					}
				}
				if (ResponseLength.HasValue && !flag)
				{
					result = false;
					if (ShowError && this.SerialCommsInformation != null)
					{
						this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.ModuleLowLevel " + FunctionName + "() :\r\n" + Racelogic.Comms.Serial.Properties.Resources.ResponseTimeout));
					}
					if (ShowError)
					{
						Messenger.get_Default().Send<InformationMessage>(new InformationMessage($"{Racelogic.Comms.Serial.Properties.Resources.CommunicationError}{Environment.NewLine}Timeout - {FunctionName}", Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
					}
				}
				if (!ResponseLength.HasValue)
				{
					if (!Command.HasValue || Command != 7 || SubCommand != 2)
					{
						byte[] array = new byte[SetupRxCount];
						using (ReturnedDataLock.Lock())
						{
							while (SetupRxCount > 0)
							{
								_returnedData.Add(setupReceivedDataBuffer[SetupReadIndex++]);
								VBoxData.UnrecognisedCharacter--;
							}
						}
					}
				}
				else if (_requestingVariableLengthResponse)
				{
					DateTime now = DateTime.Now;
					while (SetupRxCount < ResponseLength.Value)
					{
						Thread.Sleep(1);
						if (DateTime.Now - now > TimeSpan.FromSeconds(1.5))
						{
							break;
						}
					}
					if (SetupRxCount >= ResponseLength.Value)
					{
						now = DateTime.Now;
						int num3 = 0;
						using (ReturnedDataLock.Lock())
						{
							num3 = (string.Equals(FunctionName, "GetSerialNumber_StandardProtocolExtended") ? (_returnedData[2] + 2) : (_returnedData[2] - 3));
						}
						while (SetupRxCount < num3)
						{
							Thread.Sleep(1);
							if (DateTime.Now - now > TimeSpan.FromSeconds(1.5))
							{
								break;
							}
						}
						if (SetupRxCount >= num3)
						{
							using (ReturnedDataLock.Lock())
							{
								for (int i = 0; i < num3; i++)
								{
									_returnedData.Add(setupReceivedDataBuffer[SetupReadIndex++]);
								}
							}
							result = true;
						}
						else
						{
							result = false;
						}
					}
				}
			}
			else
			{
				result = false;
			}
		}
		else
		{
			TxData(queue);
		}
		if (_crcError)
		{
			result = false;
			if (ShowError && this.SerialCommsInformation != null)
			{
				this.SerialCommsInformation(this, new SerialCommsInformationEventArgs(InformationType.Information, "Racelogic.Comms.ModuleLowLevel " + FunctionName + "() :\r\n" + Racelogic.Comms.Serial.Properties.Resources.CrcError));
			}
			if (ShowError)
			{
				Messenger.get_Default().Send<InformationMessage>(new InformationMessage($"{Racelogic.Comms.Serial.Properties.Resources.CommunicationError}{Environment.NewLine}CRC error - {FunctionName}", Racelogic.Comms.Serial.Properties.Resources.Information), (object)"SlidingInformationMessage");
			}
		}
		_requestedCommand = null;
		_requestedVBOXSubCommand = null;
		_requestingVariableLengthResponse = false;
		_OldProtocolState = OldProtocolState.Idle;
		InSetup = false;
		return result;
	}

	private void UpdateProgress(ref int oldPercent, double percent, ref long TimeStamp)
	{
		if (Math.Round(percent) > (double)oldPercent && (percent - (double)oldPercent > 10.0 || TimeSpan.FromTicks(DateTime.Now.Ticks - TimeStamp) > TimeSpan.FromSeconds(2.0)))
		{
			TimeStamp = DateTime.Now.Ticks;
			oldPercent = (int)Math.Round(percent);
			PercentComplete = percent;
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.UploadDataToUnit;
		}
	}

	private void WaitForTimer(int delayMs)
	{
		Thread.Sleep(delayMs);
	}

	private bool GetSeed_StandardProtocol(bool ShowError)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			result = RequestVBOXData(null, 18, null, 6, 3000, ShowError, "GetSeed_StandardProtocol");
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	private bool SatLabEnterBootloader(Modes Mode)
	{
		bool result = false;
		bool flag = true;
		while (flag)
		{
			if (MessageBox.Show(Racelogic.Comms.Serial.Properties.Resources.PowerCycleLabSat + Environment.NewLine + Racelogic.Comms.Serial.Properties.Resources.CancelToAbort, string.Empty, MessageBoxButton.OKCancel, MessageBoxImage.None) == MessageBoxResult.OK)
			{
				_awaitingBootloaderResponse = true;
				InSetup = true;
				FlushRxTx();
				string text = string.Empty;
				long ticks = DateTime.Now.Ticks;
				TimeSpan timeSpan = TimeSpan.FromMilliseconds(7500.0);
				while (!text.Contains("Bootloader") && TimeSpan.FromTicks(DateTime.Now.Ticks - ticks) < timeSpan)
				{
					while (RxCount > 0)
					{
						string text2 = text;
						char c = (char)receivedDataBuffer[ReadIndex++];
						text = text2 + c;
					}
				}
				if (!text.Contains("Bootloader"))
				{
					continue;
				}
				byte[] data = new byte[1] { 85 };
				for (int i = 0; i <= 30; i++)
				{
					TxData(data);
					WaitForTimer(10);
				}
				FlushRxTx();
				text = string.Empty;
				ticks = DateTime.Now.Ticks;
				while (!text.Contains("Here we go") && TimeSpan.FromTicks(DateTime.Now.Ticks - ticks) < timeSpan)
				{
					while (RxCount > 0)
					{
						string text3 = text;
						char c = (char)receivedDataBuffer[ReadIndex++];
						text = text3 + c;
					}
				}
				if (text.Contains("Here we go"))
				{
					flag = false;
					result = true;
				}
			}
			else
			{
				flag = false;
			}
		}
		WaitForTimer(1000);
		InSetup = false;
		_awaitingBootloaderResponse = false;
		return result;
	}

	private bool EnterBootloader_StandardProtocol(ModuleDefinition Module, Modes Mode)
	{
		bool flag = false;
		if (Module.UnitType == 72)
		{
			flag = SatLabEnterBootloader(Mode);
		}
		else
		{
			Queue<byte> queue = new Queue<byte>(1);
			queue.Enqueue((byte)Mode);
			if (RequestVBOXData(null, 5, queue, 4, 5000, ShowError: false, "EnterBootloader_StandardProtocol"))
			{
				using (ReturnedDataLock.Lock())
				{
					if (_returnedData.Count == 1 && _returnedData[0] == (byte)Mode)
					{
						flag = true;
					}
				}
				if (!flag && Mode == Modes.Bootloader && (Module.Processor == Processor.STR71x || Module.Processor == Processor.STR91x))
				{
					flag = Str_Awake();
				}
				if (flag)
				{
					WaitForTimer(2000);
					if (Mode == Modes.Bootloader)
					{
						WaitForTimer(3000);
						if (Module.UnitType == 31 || Module.UnitType == 30 || Module.UnitType == 33 || Module.UnitType == 44)
						{
							WaitForTimer(3000);
						}
					}
					else if (Module.UnitType == 31 || Module.UnitType == 30 || Module.UnitType == 33 || Module.UnitType == 44)
					{
						WaitForTimer(3000);
					}
				}
			}
		}
		return flag;
	}

	private string SendGpsEngineMessage_StandardProtocol(string message, short unitType, GpsEngineNumber gpsEngine, GpsEngineType engineType, byte firmwareRevision)
	{
		if (gpsEngine == GpsEngineNumber.Two)
		{
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.SendingMessageToGpsEngine2;
		}
		else
		{
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.SendingMessageToGpsEngine;
		}
		Queue<byte> queue = new Queue<byte>();
		if ((engineType == GpsEngineType.Javad || engineType == GpsEngineType.JGG100_2 || engineType == GpsEngineType.JGG100_4 || engineType == GpsEngineType.Oem1 || engineType == GpsEngineType.Oem1Dual || engineType == GpsEngineType.TrG2 || engineType == GpsEngineType.TrG3 || engineType == GpsEngineType.B210) && !message.StartsWith("%"))
		{
			message = "%" + message + "%" + message;
		}
		message = PrepareMessage(message, unitType);
		for (int i = 0; i < message.Length; i++)
		{
			if (message[i] == '\\')
			{
				switch (message[i + 1])
				{
				case 'A':
				case 'a':
					queue.Enqueue(7);
					i++;
					break;
				case 'B':
				case 'b':
					queue.Enqueue(8);
					i++;
					break;
				case 'F':
				case 'f':
					queue.Enqueue(12);
					i++;
					break;
				case 'N':
				case 'n':
					queue.Enqueue(10);
					i++;
					break;
				case 'R':
				case 'r':
					queue.Enqueue(13);
					i++;
					break;
				case 'T':
				case 't':
					queue.Enqueue(9);
					i++;
					break;
				case 'V':
				case 'v':
					queue.Enqueue(11);
					i++;
					break;
				case '"':
				case '\'':
				case '?':
				case '\\':
					queue.Enqueue((byte)message[i + 1]);
					i++;
					break;
				default:
					queue.Enqueue((byte)message[i]);
					break;
				}
			}
			else
			{
				queue.Enqueue((byte)message[i]);
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		short? num = (((unitType == 14 || unitType == 22 || unitType == 24 || unitType == 58 || unitType == 36 || unitType == 39 || unitType == 48 || unitType == 62 || unitType == 73 || unitType == 83 || unitType == 16) && firmwareRevision <= 34) ? null : new short?(3));
		bool flag = !num.HasValue && Environment.UserDomainName == "RL" && (Environment.UserName.ToLower() == "stuart jones" || Environment.UserName.ToLower() == "toby lievesley");
		if (RequestVBOXData(null, (int)gpsEngine, queue, num, 1500, ShowError: false, "SendGpsEngineMessage_StandardProtocol", unitType))
		{
			if ((unitType == 58 || unitType == 36 || unitType == 16) && firmwareRevision <= 33)
			{
				num = (short)3;
			}
			using (ReturnedDataLock.Lock())
			{
				for (int j = num.GetValueOrDefault(); j < _returnedData.Count - 2; j++)
				{
					stringBuilder.Append((char)_returnedData[j]);
				}
			}
		}
		if (flag)
		{
			MessageBox.Show("Crappy GPS response message");
		}
		ProgressText = string.Empty;
		return stringBuilder.ToString();
	}

	private string PrepareMessage(string message, short unitType)
	{
		switch (unitType)
		{
		case 36:
		case 39:
		case 41:
		case 48:
		case 50:
		case 58:
		case 62:
		case 73:
		case 83:
			if (!message.EndsWith("\r"))
			{
				message += "\r";
			}
			break;
		}
		return message;
	}

	public bool Unlock_StandardProtocol(bool ShowError)
	{
		bool result = false;
		if (GetSeed_StandardProtocol(ShowError))
		{
			List<byte> list = new List<byte>();
			using (ReturnedDataLock.Lock())
			{
				foreach (byte returnedDatum in _returnedData)
				{
					list.Add(returnedDatum);
				}
			}
			if (list.Count == 2)
			{
				Queue<byte> queue = new Queue<byte>(2);
				uint num = list[0];
				num <<= 8;
				num |= list[1];
				StringBuilder stringBuilder = new StringBuilder();
				byte value = (byte)(num >> 8);
				stringBuilder.Append((char)value);
				value = (byte)num;
				stringBuilder.Append((char)value);
				num = Checksum.Calculate(stringBuilder.ToString(), 2u, PolynomialUnitType.VideoVBoxAndMfd);
				queue.Enqueue((byte)(num >> 8));
				queue.Enqueue((byte)num);
				Thread.Sleep(100);
				if (RequestVBOXData(null, 19, queue, 4, 3000, ShowError, "Unlock_StandardProtocol"))
				{
					lock (ReturnedDataLock)
					{
						if (_returnedData.Count == 1 && _returnedData[0] == 1)
						{
							result = true;
						}
						_returnedData.Clear();
					}
				}
			}
		}
		return result;
	}

	public bool Lock_StandardProtocol()
	{
		bool result = false;
		Queue<byte> queue = new Queue<byte>(2);
		queue.Enqueue(0);
		queue.Enqueue(1);
		if (RequestVBOXData(null, 19, queue, 4, 3000, ShowError: false, "Lock_StandardProtocol"))
		{
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 1 && _returnedData[0] == 0)
				{
					result = true;
				}
				_returnedData.Clear();
			}
		}
		return result;
	}

	public string SendGpsEngineMessage_StandardProtocol(string message, short unitType, GpsEngineType gpsEngineType, byte firmwareRevision)
	{
		return SendGpsEngineMessage_StandardProtocol(message, unitType, GpsEngineNumber.One, gpsEngineType, firmwareRevision);
	}

	public string SendGpsEngine2Message_StandardProtocol(string message, short unitType, GpsEngineType gpsEngineType, byte firmwareRevision)
	{
		return SendGpsEngineMessage_StandardProtocol(message, unitType, GpsEngineNumber.Two, gpsEngineType, firmwareRevision);
	}

	public bool StopGPS(bool StopGPS, bool waitForResponse = true)
	{
		bool flag = false;
		bool flag2 = isReceivingVBoxComms;
		if (!flag2 || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			byte[] array = (StopGPS ? Racelogic.Core.Helper.StringToByteArray("$PASHS,NME,POS,A,OFF") : Racelogic.Core.Helper.StringToByteArray("$PASHS,NME,POS,A,ON"));
			ProgressText = (StopGPS ? Racelogic.Comms.Serial.Properties.Resources.StopGpsEngine : Racelogic.Comms.Serial.Properties.Resources.StartGpsEngine);
			Queue<byte> queue = new Queue<byte>();
			queue.Enqueue(6);
			queue.Enqueue((byte)array.Length);
			byte[] array2 = array;
			foreach (byte item in array2)
			{
				queue.Enqueue(item);
			}
			Checksum.Append(queue);
			string text = string.Empty;
			InSetup = true;
			List<byte> list = new List<byte>();
			_OldProtocolState = OldProtocolState.StopGps;
			if (TxData(queue))
			{
				if (waitForResponse)
				{
					long ticks = DateTime.Now.Ticks;
					TimeSpan timeSpan = TimeSpan.FromSeconds(10.0);
					while (!flag && TimeSpan.FromTicks(DateTime.Now.Ticks - ticks) < timeSpan)
					{
						Thread.Sleep(1);
						while (SetupRxCount > 0)
						{
							list.Add(setupReceivedDataBuffer[SetupReadIndex++]);
							text += (char)list[list.Count - 1];
							VBoxData.UnrecognisedCharacter--;
							if (text.Contains(",ACK"))
							{
								flag = true;
								continue;
							}
							if (text.Contains("$>"))
							{
								flag = true;
							}
							else if (text.Contains("RE004") && text.Contains("\r\n"))
							{
								flag = true;
							}
							else if (text.Contains(VboxLiteStartGpsReply))
							{
								flag = true;
							}
							else if (list.Count == _standardAck.Length)
							{
								flag = true;
								for (int j = 0; j < _standardAck.Length; j++)
								{
									if (list[j] != _standardAck[j])
									{
										flag = false;
										break;
									}
								}
							}
							if (flag)
							{
								continue;
							}
							if (StopGPS)
							{
								if (text.Contains("STOP"))
								{
									flag = true;
								}
							}
							else if (text.Contains("GO"))
							{
								flag = true;
							}
							else if (text.Contains("$GPGGA") || text.Contains("$PSAT") || text.Contains("$BIN"))
							{
								flag = true;
							}
						}
					}
				}
				else
				{
					flag = true;
				}
			}
			InSetup = false;
			_OldProtocolState = OldProtocolState.Idle;
			ProgressText = string.Empty;
		}
		if (flag2)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false, forceQuiet: true);
		}
		return flag;
	}

	public bool EnterBootloader_StandardProtocol(ModuleDefinition Module)
	{
		return EnterBootloader_StandardProtocol(Module, Modes.Bootloader);
	}

	public bool EnterNormalMode_StandardProtocol(ModuleDefinition Module)
	{
		return EnterBootloader_StandardProtocol(Module, Modes.NormalRunning);
	}

	public bool SetGatePosition(SplitType splitType)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = new Queue<byte>();
			switch (splitType)
			{
			case SplitType.StartFinish:
				queue.Enqueue(0);
				break;
			case SplitType.Finish:
				queue.Enqueue(1);
				break;
			default:
				throw new ArgumentOutOfRangeException("splitType", "Invalid value, only StartFinish or Finish is allowed");
			}
			if (RequestVBOXData(32, 8, queue, 4, 1500, ShowError: false, "SetGatePosition"))
			{
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	private bool Upload_StandardProtocol(int Address, List<byte> Data, byte SubCommand)
	{
		bool flag = false;
		Queue<byte> queue = new Queue<byte>();
		queue.Enqueue((byte)(Address >> 16));
		queue.Enqueue((byte)(Address >> 8));
		queue.Enqueue((byte)Address);
		foreach (byte Datum in Data)
		{
			queue.Enqueue(Datum);
		}
		int num = 3;
		while (!flag && num-- > 0)
		{
			if (!RequestVBOXData(null, SubCommand, queue, 4, 6500, num == 0, "Upload_StandardProtocol"))
			{
				continue;
			}
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 1 && _returnedData[0] == 1)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	public bool UploadToUnit_StandardProtocol(int Address, List<byte> Data, bool unlockRequired = true)
	{
		return UploadToUnit_StandardProtocol(Address, Data, null, unlockRequired);
	}

	public bool UploadToUnit_StandardProtocol(int Address, List<byte> newData, List<byte> oldData, bool unlockRequired = true)
	{
		return UploadToUnit_StandardProtocol(Address, newData, oldData, ShowError: true, 220, unlockRequired);
	}

	public bool UploadToUnit_StandardProtocol(int Address, List<byte> Data, int maxPayloadLength)
	{
		return UploadToUnit_StandardProtocol(Address, Data, ShowError: true, maxPayloadLength);
	}

	public bool UploadToUnit_StandardProtocol(int Address, List<byte> Data, bool ShowError, int maxPayloadLength = 220, bool unlockRequired = true)
	{
		return UploadToUnit_StandardProtocol(Address, Data, new List<byte>(), ShowError: true, maxPayloadLength, unlockRequired);
	}

	public bool UploadToUnit_StandardProtocol(int Address, List<byte> newData, List<byte> oldData, bool ShowError, int maxPayloadLength = 220, bool unlockRequired = true)
	{
		bool flag = false;
		bool flag2 = isReceivingVBoxComms;
		List<Tuple<int, List<byte>>> list = new List<Tuple<int, List<byte>>>();
		int num = Address;
		bool flag3 = true;
		if (newData != null)
		{
			if (oldData != null && oldData.Count == newData.Count)
			{
				int num2 = 0;
				while (num2 < newData.Count)
				{
					if (newData[num2] != oldData[num2])
					{
						if (flag3)
						{
							flag3 = false;
							list.Add(new Tuple<int, List<byte>>(Address + num2, new List<byte>()));
						}
						list[list.Count - 1].Item2.Add(newData[num2]);
					}
					else
					{
						flag3 = true;
					}
					num2++;
					num++;
				}
			}
			else
			{
				list.Add(new Tuple<int, List<byte>>(Address, newData));
			}
		}
		if (list.Count > 0)
		{
			if (!flag2 || SetQuiet_StandardProtocol(MakeQuiet: true))
			{
				if (!unlockRequired || Unlock_StandardProtocol(ShowError))
				{
					int oldPercent = 0;
					double num3 = 0.0;
					long TimeStamp = DateTime.Now.Ticks;
					int num4 = 0;
					int num5 = 0;
					foreach (Tuple<int, List<byte>> item in list)
					{
						num4 += item.Item2.Count;
					}
					for (int i = 0; i < list.Count; i++)
					{
						Queue<byte> queue = new Queue<byte>(list[i].Item2.Count);
						foreach (byte item2 in list[i].Item2)
						{
							queue.Enqueue(item2);
						}
						Address = list[i].Item1;
						flag = true;
						while (flag && queue.Count > maxPayloadLength)
						{
							num3 = 100.0 - (double)(num4 - num5) / (double)num4 * 100.0;
							UpdateProgress(ref oldPercent, num3, ref TimeStamp);
							newData = new List<byte>(maxPayloadLength);
							for (int j = 0; j < maxPayloadLength; j++)
							{
								newData.Add(queue.Dequeue());
							}
							flag = Upload_StandardProtocol(Address, newData, 3);
							Address += maxPayloadLength;
							num5 += maxPayloadLength;
						}
						if (flag && queue.Count > 0)
						{
							num3 = (PercentComplete = 100.0 - (double)(num4 - num5) / (double)num4 * 100.0);
							ProgressText = Racelogic.Comms.Serial.Properties.Resources.UploadDataToUnit;
							newData = new List<byte>(queue.Count);
							while (queue.Count > 0)
							{
								newData.Add(queue.Dequeue());
							}
							flag = Upload_StandardProtocol(Address, newData, 3);
							num5 += newData.Count;
							if (i >= list.Count)
							{
								PercentComplete = 100.0;
							}
							ProgressText = Racelogic.Comms.Serial.Properties.Resources.UploadDataToUnit;
						}
					}
					if (unlockRequired)
					{
						Lock_StandardProtocol();
					}
				}
				if (flag2)
				{
					SetQuiet_StandardProtocol(MakeQuiet: false);
				}
			}
		}
		else
		{
			flag = true;
		}
		PercentComplete = 0.0;
		ProgressText = string.Empty;
		return flag;
	}

	public bool UploadBlockToUnit_StandardProtocol(int Address, List<byte> Data, BlockSizes BlockSize)
	{
		return UploadBlockToUnit_StandardProtocol(Address, Data, BlockSize, ShowError: true);
	}

	public bool UploadBlockToUnit_StandardProtocol(int Address, List<byte> Data, BlockSizes BlockSize, bool ShowError)
	{
		bool flag = false;
		bool flag2 = isReceivingVBoxComms;
		if (!flag2 || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (Unlock_StandardProtocol(ShowError))
			{
				int oldPercent = 0;
				double num = 0.0;
				long TimeStamp = DateTime.Now.Ticks;
				int count = Data.Count;
				int num2 = 0;
				Queue<byte> queue = new Queue<byte>(Data.Count);
				foreach (byte Datum in Data)
				{
					queue.Enqueue(Datum);
				}
				flag = true;
				while (flag && queue.Count > (int)BlockSize)
				{
					num = 100.0 - (double)(count - num2) / (double)count * 100.0;
					UpdateProgress(ref oldPercent, num, ref TimeStamp);
					Data = new List<byte>((int)BlockSize);
					for (int i = 0; i < (int)BlockSize; i++)
					{
						Data.Add(queue.Dequeue());
					}
					flag = Upload_StandardProtocol(Address, Data, 21);
					Address = (int)(Address + BlockSize);
					num2 = (int)(num2 + BlockSize);
				}
				if (flag && queue.Count > 0)
				{
					num = (PercentComplete = 100.0 - (double)(count - num2) / (double)count * 100.0);
					ProgressText = Racelogic.Comms.Serial.Properties.Resources.UploadDataToUnit;
					Data = new List<byte>(queue.Count);
					while (queue.Count > 0)
					{
						Data.Add(queue.Dequeue());
					}
					flag = Upload_StandardProtocol(Address, Data, 21);
					PercentComplete = 100.0;
					ProgressText = Racelogic.Comms.Serial.Properties.Resources.UploadDataToUnit;
				}
			}
			Lock_StandardProtocol();
		}
		if (flag2)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		return flag;
	}

	private bool SwitchRacelogicBus(bool GetBus, ref byte newValue)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = new Queue<byte>();
			queue.Enqueue((byte)((!GetBus) ? byte.MaxValue : 0));
			queue.Enqueue(newValue);
			if (RequestVBOXData(7, 36, queue, 6, 1500, ShowError: false, "GetRacelogicBus"))
			{
				using (ReturnedDataLock.Lock())
				{
					newValue = _returnedData[1];
				}
				result = true;
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	private bool GetRevision(VBoxSubCommand vBoxSubCommand, out byte revision)
	{
		bool result = false;
		revision = 0;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, (int)vBoxSubCommand, null, 5, 1000, ShowError: false, (vBoxSubCommand == VBoxSubCommand.ReportRevision) ? "GetFirmwareRevision" : "GetHardwareRevision"))
			{
				result = true;
				using (ReturnedDataLock.Lock())
				{
					revision = _returnedData[0];
				}
			}
			else
			{
				SystemSounds.Asterisk.Play();
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	private string GetVoltages(VBoxSubCommand subCommand)
	{
		string arg = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		string arg2 = string.Empty;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, (int)subCommand, null, 6, 1000, ShowError: false, (subCommand == VBoxSubCommand.ReportInternalBattery) ? "GetInternalVoltage" : "GetExternalVoltage"))
			{
				uint num = 0u;
				using (ReturnedDataLock.Lock())
				{
					num = (uint)((_returnedData[0] << 8) | _returnedData[1]);
				}
				double num2 = num * 5;
				num2 /= 1024.0;
				if (subCommand == VBoxSubCommand.ReportExternalBattery)
				{
					num2 *= 4.3;
				}
				arg = num2.ToString("0.00");
				arg2 = Racelogic.Comms.Serial.Properties.Resources.Volts;
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return $"{((subCommand == VBoxSubCommand.ReportInternalBattery) ? Racelogic.Comms.Serial.Properties.Resources.InternalBatteryVoltage : Racelogic.Comms.Serial.Properties.Resources.PowerSupply)} : {arg} {arg2}";
	}

	private List<string> GetVB3iRevisions()
	{
		List<string> list = new List<string>();
		string arg = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		string arg2 = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		string arg3 = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		string arg4 = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		string arg5 = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		string arg6 = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		string arg7 = string.Empty;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingHardwareRevisions;
		if (RequestVBOXData(7, 49, null, 34, 1000, ShowError: false, "GetRevisions"))
		{
			using (ReturnedDataLock.Lock())
			{
				Version version = new Version((_returnedData[1] << 8) | _returnedData[2], (_returnedData[3] << 8) | _returnedData[4], (_returnedData[5] << 8) | _returnedData[6]);
				arg = $"V{version.Major}.{version.Minor} b{version.Build}";
				version = new Version((_returnedData[7] << 8) | _returnedData[8], (_returnedData[9] << 8) | _returnedData[10], (_returnedData[11] << 8) | _returnedData[12]);
				arg2 = $"V{version.Major}.{version.Minor} b{version.Build}";
				arg5 = _returnedData[13] switch
				{
					85 => Racelogic.Comms.Serial.Properties.Resources.Bootloader, 
					170 => Racelogic.Comms.Serial.Properties.Resources.MainApplication, 
					_ => Racelogic.Comms.Serial.Properties.Resources.Error, 
				};
				arg3 = ((_returnedData[14] << 24) | (_returnedData[15] << 16) | (_returnedData[16] << 8) | _returnedData[17]).ToString();
				arg4 = new Version((_returnedData[18] << 8) | _returnedData[19], _returnedData[20]).ToString() + (char)_returnedData[21];
				arg6 = ((_returnedData[22] << 8) | _returnedData[23]) switch
				{
					65535 => Racelogic.Comms.Serial.Properties.Resources.Bootloader, 
					0 => Racelogic.Comms.Serial.Properties.Resources.Bootstrap, 
					_ => Racelogic.Comms.Serial.Properties.Resources.Error, 
				};
				version = new Version((_returnedData[24] << 8) | _returnedData[25], (_returnedData[26] << 8) | _returnedData[27], (_returnedData[28] << 8) | _returnedData[29]);
				arg7 = $" V{version.Major}.{version.Minor} b{version.Build}";
			}
		}
		list.Add($"{Racelogic.Comms.Serial.Properties.Resources.FirmwareVersion} : {arg}");
		list.Add($"{Racelogic.Comms.Serial.Properties.Resources.FrontPanelVersion} : {arg2}");
		list.Add($"{Racelogic.Comms.Serial.Properties.Resources.XilinxCode} : {arg3}");
		list.Add($"{Racelogic.Comms.Serial.Properties.Resources.HardwareCode} : {arg4}");
		list.Add($"{Racelogic.Comms.Serial.Properties.Resources.FrontApplication} : {arg5}");
		list.Add($"{Racelogic.Comms.Serial.Properties.Resources.LastUpdatedBy} : {arg6}{arg7}");
		return list;
	}

	private bool WiFiCommands(VBoxSubCommand subCommand, string functionName, int? responseLength, string text)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			PercentComplete = 0.0;
			ProgressText = text;
			int rxTimeout = 12500;
			switch (subCommand)
			{
			case VBoxSubCommand.WiFiVersion:
				rxTimeout = 5000;
				break;
			case VBoxSubCommand.WiFiUpgrade:
				rxTimeout = 45000;
				break;
			}
			if (RequestVBOXData(7, (int)subCommand, null, responseLength, rxTimeout, ShowError: false, functionName))
			{
				using (ReturnedDataLock.Lock())
				{
					result = subCommand switch
					{
						VBoxSubCommand.WiFiStatusCheck => _returnedData.Count == 2 && _returnedData[1] == 1, 
						VBoxSubCommand.WiFiVersion => _returnedData.Count == _returnedData[2] && _returnedData[0] == byte.MaxValue && _returnedData[1] == 1, 
						_ => _returnedData.Count == 1 && _returnedData[0] == 1, 
					};
				}
			}
			ProgressText = string.Empty;
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false, forceQuiet: true);
			}
		}
		return result;
	}

	public void SetQuietNoWaitForResponse_StandardProtocol(bool MakeQuiet)
	{
		SetQuietRequested = true;
		Queue<byte> queue = new Queue<byte>();
		queue.Enqueue((byte)(MakeQuiet ? byte.MaxValue : 0));
		ProgressText = (MakeQuiet ? Racelogic.Comms.Serial.Properties.Resources.MakeQuiet : Racelogic.Comms.Serial.Properties.Resources.MakeNoise);
		RequestVBOXData(7, 2, queue, null, 500, ShowError: false, "SetQuiet_StandardProtocol");
		ProgressText = string.Empty;
		SetQuietRequested = false;
	}

	public bool SetQuiet_StandardProtocol(bool MakeQuiet, bool forceQuiet = false, bool waitForResponse = true)
	{
		SetQuietRequested = true;
		FlushRxTx();
		if (!forceQuiet && requestingCanChannelInformation)
		{
			DateTime now = DateTime.Now;
			while (requestingCanChannelInformation)
			{
				Thread.Sleep(1);
				if (DateTime.Now - now > TimeSpan.FromSeconds(5.0))
				{
					RequestingCanChannelInformation = false;
				}
			}
		}
		bool flag = false;
		Queue<byte> queue = new Queue<byte>();
		queue.Enqueue((byte)(MakeQuiet ? byte.MaxValue : 0));
		ProgressText = (MakeQuiet ? Racelogic.Comms.Serial.Properties.Resources.MakeQuiet : Racelogic.Comms.Serial.Properties.Resources.MakeNoise);
		if (RequestVBOXData(7, 2, queue, null, waitForResponse ? 1500 : 250, ShowError: false, "SetQuiet_StandardProtocol"))
		{
			if (waitForResponse)
			{
				Thread.Sleep(500);
				FlushRxTx();
				Thread.Sleep(500);
				if (MakeQuiet)
				{
					if (SetupRxCount == 4)
					{
						flag = setupReceivedDataBuffer[setupReadIndex] == byte.MaxValue && setupReceivedDataBuffer[setupReadIndex + 1] == 1 && setupReceivedDataBuffer[setupReadIndex + 2] == 19 && setupReceivedDataBuffer[setupReadIndex + 3] == 222;
						SetupReadIndex += 4;
						VBoxData.UnrecognisedCharacter -= 4;
					}
					else
					{
						flag = RxCount == 0 && !IsReceivingVBoxComms;
					}
				}
				else
				{
					flag = IsReceivingVBoxComms || RxCount > 0 || SetupRxCount > 0;
				}
			}
			else
			{
				flag = true;
			}
		}
		else if (MakeQuiet)
		{
			flag = RxCount == 0 && !IsReceivingVBoxComms;
		}
		if (MakeQuiet && flag)
		{
			Reset();
		}
		SetQuietRequested = false;
		ProgressText = string.Empty;
		return flag;
	}

	public void Reboot_StandardProtocol()
	{
		RequestVBOXData(7, 127, null, 0, 3000, ShowError: false, "Reboot_StandardProtocol");
	}

	public bool GetRacelogicBus(ref byte value)
	{
		return SwitchRacelogicBus(GetBus: true, ref value);
	}

	public bool SetRacelogicBus(byte newValue, out byte actualValue)
	{
		byte b = newValue;
		SwitchRacelogicBus(GetBus: false, ref newValue);
		actualValue = 0;
		SwitchRacelogicBus(GetBus: true, ref actualValue);
		return b == actualValue;
	}

	public bool GetFirmwareRevision(out byte revision)
	{
		return GetRevision(VBoxSubCommand.ReportRevision, out revision);
	}

	public bool GetHardwareRevision(out byte revision)
	{
		return GetRevision(VBoxSubCommand.ReportHardware, out revision);
	}

	public string GetInternalVoltage()
	{
		return GetVoltages(VBoxSubCommand.ReportInternalBattery);
	}

	public string GetExternalVoltage()
	{
		return GetVoltages(VBoxSubCommand.ReportExternalBattery);
	}

	public bool SetDebugMode(bool enableDebug)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = new Queue<byte>();
			queue.Enqueue((byte)(enableDebug ? byte.MaxValue : 0));
			if (RequestVBOXData(7, 25, queue, 4, 1000, ShowError: false, "SetDebugMode"))
			{
				result = true;
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	public bool SetDebugVelocity(ushort value)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = new Queue<byte>();
			for (int num = 1; num >= 0; num--)
			{
				queue.Enqueue((byte)(value >> num * 8));
			}
			if (RequestVBOXData(7, 24, queue, 4, 1000, ShowError: false, "SetDebugValue"))
			{
				result = true;
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	public bool GetRealTimeClock(out DateTime realTimeClock, bool supportsSeconds = false)
	{
		bool result = false;
		realTimeClock = default(DateTime);
		bool flag = isReceivingVBoxComms;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingRealTimeClock;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 4, null, 10, 1500, ShowError: false, "GetRealTimeClock"))
			{
				result = true;
				try
				{
					using (ReturnedDataLock.Lock())
					{
						realTimeClock = new DateTime(2000 + _returnedData[5], _returnedData[4], _returnedData[3], _returnedData[0], _returnedData[1], supportsSeconds ? _returnedData[2] : 0);
					}
				}
				catch (Exception)
				{
					realTimeClock = default(DateTime);
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		return result;
	}

	public bool SetRealTimeClock(DateTime newDateTime)
	{
		Queue<byte> queue = new Queue<byte>(5);
		queue.Enqueue((byte)newDateTime.Day);
		queue.Enqueue((byte)newDateTime.Month);
		queue.Enqueue((byte)(newDateTime.Year - 2000));
		queue.Enqueue((byte)newDateTime.Hour);
		queue.Enqueue((byte)newDateTime.Minute);
		return RequestVBOXData(7, 3, queue, null, 1500, ShowError: false, "SetRealTimeClock");
	}

	public List<string> GetRevisions(short unitType)
	{
		bool flag = isReceivingVBoxComms;
		List<string> list = new List<string>();
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (unitType == 60)
			{
				list = GetVB3iRevisions();
			}
			else
			{
				string arg = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
				string arg2 = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
				if (RequestVBOXData(7, 11, null, 5, 1000, ShowError: false, "GetRevisions"))
				{
					using (ReturnedDataLock.Lock())
					{
						arg = _returnedData[0].ToString();
					}
				}
				if (RequestVBOXData(7, 23, null, 5, 1000, ShowError: false, "GetRevisions"))
				{
					using (ReturnedDataLock.Lock())
					{
						switch (unitType)
						{
						case 22:
						case 24:
						case 36:
						case 39:
						case 41:
						case 48:
						case 50:
						case 58:
						case 62:
							arg2 = _returnedData[0].ToString();
							break;
						default:
							arg2 = (_returnedData[0] & 1).ToString();
							break;
						}
					}
				}
				list.Add($"{Racelogic.Comms.Serial.Properties.Resources.FirmwareRevision} : {arg}");
				list.Add($"{Racelogic.Comms.Serial.Properties.Resources.HardwareRevision} : {arg2}");
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		return list;
	}

	public string GetGpsRevision()
	{
		bool flag = isReceivingVBoxComms;
		string result = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		bool flag2 = false;
		StringBuilder stringBuilder = new StringBuilder();
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingGpsRevision;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 17, null, 8, 1000, ShowError: false, "GetGpsRevision"))
			{
				flag2 = true;
			}
			else if (RequestVBOXData(7, 27, null, 0, 1000, ShowError: false, "ForceGpsFirmware"))
			{
				flag2 = RequestVBOXData(7, 17, null, 8, 1000, ShowError: false, "GetGpsRevision");
			}
			using (ReturnedDataLock.Lock())
			{
				if (flag2 && _returnedData[1] != 0)
				{
					foreach (byte returnedDatum in _returnedData)
					{
						stringBuilder.Append((char)returnedDatum);
					}
				}
			}
			result = $"{Racelogic.Comms.Serial.Properties.Resources.Revision} : {stringBuilder.ToString()}";
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		return result;
	}

	public string GetBluetoothFirmware()
	{
		string arg = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		bool flag = isReceivingVBoxComms;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingBluetoothFirmware;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 50, null, null, 1000, ShowError: false, "GetBluetoothFirmware"))
			{
				StringBuilder stringBuilder = new StringBuilder();
				using (ReturnedDataLock.Lock())
				{
					if (_returnedData.Count >= 3 && _returnedData[0] == byte.MaxValue && _returnedData[1] == 1 && _returnedData.Count == _returnedData[2])
					{
						for (int i = 3; i < _returnedData.Count - 3; i++)
						{
							stringBuilder.Append((char)_returnedData[i]);
						}
					}
				}
				arg = stringBuilder.ToString().Trim();
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		return $"{Racelogic.Comms.Serial.Properties.Resources.BluetoothFirmware} : {arg}";
	}

	public string GetFrontPanelHardware()
	{
		string arg = Racelogic.Comms.Serial.Properties.Resources.Unavailable;
		bool flag = isReceivingVBoxComms;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingFrontPanelHardware;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 51, null, 6, 1000, ShowError: false, "GetFrontPanelHardware"))
			{
				using (ReturnedDataLock.Lock())
				{
					if (_returnedData[0] != byte.MaxValue)
					{
						arg = _returnedData[0].ToString();
					}
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		return $"{Racelogic.Comms.Serial.Properties.Resources.FrontPanelHardwareRevision} : {arg}";
	}

	public bool GetSerialNumber_StandardProtocol(UnitInformation UnitInfo)
	{
		return GetSerialNumber_StandardProtocol(UnitInfo, ShowError: true);
	}

	private bool GetExtendedSerialNumber(UnitInformation UnitInfo, bool ShowError)
	{
		bool result = false;
		StringBuilder stringBuilder = new StringBuilder();
		if (RequestVBOXData(7, 126, null, 3, 3000, ShowError, "GetSerialNumber_StandardProtocolExtended"))
		{
			using (ReturnedDataLock.Lock())
			{
				if (Checksum.Check(_returnedData, (uint)_returnedData.Count, PolynomialUnitType.VBox))
				{
					result = true;
					byte b = _returnedData[3];
					byte b2 = b;
					if ((uint)(b2 - 1) > 1u)
					{
						throw new NotImplementedException("GetSerialNumber_StandardProtocolExtended");
					}
					UnitInfo.UnitType = _returnedData[4];
					UnitInfo.SubType = (char)_returnedData[5];
					stringBuilder.Clear();
					int num = ((_returnedData[3] == 1) ? 13 : 14);
					for (int i = 6; i < num; i++)
					{
						stringBuilder.Append((char)_returnedData[i]);
					}
					UnitInfo.SerialNumber = int.Parse(stringBuilder.ToString());
				}
			}
		}
		return result;
	}

	public bool GetSerialNumber_StandardProtocol(UnitInformation UnitInfo, bool ShowError, bool getExtended = false)
	{
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReadingSerialNumber;
		if (UnitInfo == null)
		{
			UnitInfo = new UnitInformation();
		}
		bool flag = false;
		bool flag2 = isReceivingVBoxComms;
		if (flag2)
		{
			SetQuiet_StandardProtocol(MakeQuiet: true);
		}
		if (getExtended)
		{
			flag = GetExtendedSerialNumber(UnitInfo, ShowError);
		}
		else if (RequestVBOXData(7, 6, null, 12, 3000, ShowError, "GetSerialNumber_StandardProtocol"))
		{
			List<byte> list = new List<byte>();
			bool flag3 = false;
			using (ReturnedDataLock.Lock())
			{
				foreach (byte returnedDatum in _returnedData)
				{
					list.Add(returnedDatum);
					if (returnedDatum != 0)
					{
						flag3 = true;
					}
				}
			}
			if (list.Count == 8 && flag3)
			{
				flag = true;
				UnitInfo.UnitType = list[0];
				UnitInfo.SubType = (char)list[1];
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ModuleDefinition item in Modules.List.Where((ModuleDefinition m) => m.UnitType == UnitInfo.UnitType))
				{
					if (item.Processor == Processor.LPC)
					{
						flag = GetExtendedSerialNumber(UnitInfo, ShowError);
						continue;
					}
					stringBuilder.Clear();
					for (int i = 2; i < 8; i++)
					{
						stringBuilder.Append((char)list[i]);
					}
					UnitInfo.SerialNumber = (int.TryParse(stringBuilder.ToString(), out var result) ? result : (-1));
				}
			}
		}
		if (flag && flag2)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false);
		}
		ProgressText = string.Empty;
		return flag;
	}

	public bool CloseLogFile(bool closeFile, bool showError = false, bool waitForResponse = true)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = new Queue<byte>();
			queue.Enqueue((byte)(closeFile ? byte.MaxValue : 0));
			if (RequestVBOXData(7, 8, queue, 8, waitForResponse ? 7500 : 500, ShowError: false, "SetQuiet_CloseLogFile"))
			{
				using (ReturnedDataLock.Lock())
				{
					if (_returnedData.Count == 5)
					{
						switch (_returnedData[4])
						{
						case 1:
							result = !closeFile;
							break;
						case 2:
							result = true;
							break;
						case 3:
						case 4:
							result = true;
							break;
						default:
							result = false;
							break;
						}
					}
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	public void ColdStart_StandardProtocol()
	{
		RequestVBOXData(7, 1, null, null, 5000, ShowError: false, "ColdStart_StandardProtocol");
	}

	public bool IsImuSynchronising_StandardProtocol()
	{
		bool result = false;
		if ((!isReceivingVBoxComms || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true)) && Unlock_StandardProtocol(ShowError: false) && RequestVBOXData(7, 60, null, 4, 500, ShowError: false, "IsImuSynchronising_StandardProtocol"))
		{
			using (ReturnedDataLock.Lock())
			{
				result = _returnedData.Count == 1 && _returnedData[0] == 1;
			}
		}
		return result;
	}

	public bool ReScanCan_StandardProtocol()
	{
		bool flag = false;
		bool flag2 = isReceivingVBoxComms;
		if (!flag2 || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			PercentComplete = 0.0;
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.ScanningCanBus;
			if (RequestVBOXData(7, 10, null, 4, 7500, ShowError: false, "RescanCan_StandardProtocol"))
			{
				using (ReturnedDataLock.Lock())
				{
					flag = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
				if (flag)
				{
					Thread.Sleep(50);
				}
			}
			ProgressText = string.Empty;
			if (flag2)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false, forceQuiet: true);
			}
		}
		return flag;
	}

	public bool ReloadEEPROM_StandardProtocol()
	{
		return ReloadEEPROM_StandardProtocol(BaudRate);
	}

	public bool ReloadEEPROM_StandardProtocol(SerialBaudRate replyBaudRate)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReloadingEeprom;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 21, null, 4, 7500, ShowError: false, "ReloadEEPROM_StandardProtocol", null, replyBaudRate))
			{
				using (ReturnedDataLock.Lock())
				{
					result = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
			}
			if (!requestingCanChannelInformation && CanData != null && !IsInSimulatorMode)
			{
				CanData.Clear(clearCrcCount: true);
				CanData.Dispatcher.Invoke(UpdateCanDataChannelsAction, new List<CanChannel>(), null);
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		return result;
	}

	public bool ReinitialiseCAN_StandardProtocol()
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.ReinitialisingCan;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 22, null, 4, 2000, ShowError: false, "ReinitialiseCAN_StandardProtocol"))
			{
				using (ReturnedDataLock.Lock())
				{
					result = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		return result;
	}

	public bool IsInternalAnalogueToDigitalCalibrationEnabled()
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.RequestingCalibration;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 30, null, 4, 2000, ShowError: false, "RequestCalibration"))
			{
				using (ReturnedDataLock.Lock())
				{
					result = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		return result;
	}

	public bool SetCalibrationMode(bool enterCalibration)
	{
		return SetCalibrationMode(enterCalibration);
	}

	public bool StartVBoxInternalAnalogueToDigitalCalibration()
	{
		return SetCalibrationMode(null);
	}

	private bool SetCalibrationMode(bool? enterCalibration)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		ProgressText = "Enter calibration mode";
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = (enterCalibration.HasValue ? new Queue<byte>() : null);
			if (enterCalibration.HasValue)
			{
				queue.Enqueue((byte)(enterCalibration.Value ? byte.MaxValue : 0));
			}
			result = RequestVBOXData(7, 28, queue, 4, 10000, ShowError: false, "SetCalibrationMode");
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		PercentComplete = 0.0;
		return result;
	}

	public double[] RequestInternalA2DCalibrationChannelData()
	{
		double[] array = null;
		bool flag = isReceivingVBoxComms;
		ProgressText = "Requesting calibration data";
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 29, null, 36, 2000, ShowError: false, "RequestInternalA2DCalibrationChannelData"))
			{
				using (ReturnedDataLock.Lock())
				{
					if (_returnedData.Count == 32)
					{
						array = new double[4];
						doubleUnion doubleUnion = default(doubleUnion);
						for (int i = 0; i < 4; i++)
						{
							doubleUnion.b7_MSB = _returnedData[7 + i * 8];
							doubleUnion.b6 = _returnedData[6 + i * 8];
							doubleUnion.b5 = _returnedData[5 + i * 8];
							doubleUnion.b4 = _returnedData[4 + i * 8];
							doubleUnion.b3 = _returnedData[3 + i * 8];
							doubleUnion.b2 = _returnedData[2 + i * 8];
							doubleUnion.b1 = _returnedData[1 + i * 8];
							doubleUnion.b0_LSB = _returnedData[i * 8];
							array[i] = doubleUnion.data;
						}
					}
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		ProgressText = string.Empty;
		PercentComplete = 0.0;
		return array;
	}

	public float[] WatchInternalADC_StandardProtocol()
	{
		float[] array = null;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			if (RequestVBOXData(7, 31, null, 20, 2000, ShowError: false, "WatchInternalADC_StandardProtocol"))
			{
				using (ReturnedDataLock.Lock())
				{
					if (_returnedData.Count == 16)
					{
						array = new float[4];
						Union union = default(Union);
						for (int i = 0; i < 4; i++)
						{
							union.b3_MSB = _returnedData[3 + i * 4];
							union.b2 = _returnedData[2 + i * 4];
							union.b1 = _returnedData[1 + i * 4];
							union.b0_LSB = _returnedData[i * 4];
							array[i] = union.data;
						}
					}
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return array;
	}

	public bool ConfigureWiFi_StandardProtocol(string text)
	{
		bool flag = WiFiCommands(VBoxSubCommand.WiFiConfigure, "ConfigureWiFi_StandardProtocol", 4, text);
		if (flag)
		{
			ReloadEEPROM_StandardProtocol();
		}
		return flag;
	}

	public bool GetGpsMessageSpeed(out string messageSpeed)
	{
		return GpsMessageSpeed(setSpeed: false, "0.00", out messageSpeed);
	}

	public bool SetGpsMessageSpeed(string newSpeed, out string messageSpeed)
	{
		return GpsMessageSpeed(setSpeed: true, newSpeed, out messageSpeed);
	}

	private bool GpsMessageSpeed(bool setSpeed, string newSpeed, out string messageSpeed)
	{
		bool result = false;
		messageSpeed = "0.00";
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = new Queue<byte>(5);
			queue.Enqueue((byte)(setSpeed ? byte.MaxValue : 0));
			byte[] bytes = Encoding.ASCII.GetBytes(newSpeed);
			foreach (byte item in bytes)
			{
				queue.Enqueue(item);
			}
			if (RequestVBOXData(7, 35, queue, 9, 2000, ShowError: false, "GpsMessageSpeed"))
			{
				using (ReturnedDataLock.Lock())
				{
					if (_returnedData.Count == 5)
					{
						result = true;
						messageSpeed = Encoding.ASCII.GetString(_returnedData.ToArray(), 1, 4);
					}
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	public bool UpdateGPSFirmware_StandardProtocol()
	{
		bool result = false;
		if ((!isReceivingVBoxComms || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true)) && Unlock_StandardProtocol(ShowError: false) && RequestVBOXData(7, 64, null, 4, 3500, ShowError: false, "UpdateGPSFirmware_StandardProtocol"))
		{
			using (ReturnedDataLock.Lock())
			{
				result = _returnedData.Count == 1 && _returnedData[0] == 1;
			}
		}
		return result;
	}

	public bool WiFiStatusCheck_StandardProtocol(string text)
	{
		return WiFiCommands(VBoxSubCommand.WiFiStatusCheck, "WiFiStatusCheck_StandardProtocol", 6, text);
	}

	public bool WiFiTxTest_StandardProtocol(string text)
	{
		return WiFiCommands(VBoxSubCommand.WiFiTxTest, "WiFiTxTest_StandardProtocol", 4, text);
	}

	public bool WiFiUpgrade_StandardProtocol(string text)
	{
		return WiFiCommands(VBoxSubCommand.WiFiUpgrade, "WiFiUpgrade_StandardProtocol", 4, text);
	}

	public bool WiFiVersion_StandardProtocol(string text, out string version)
	{
		version = string.Empty;
		bool flag = WiFiCommands(VBoxSubCommand.WiFiVersion, "WiFiVersion_StandardProtocol", null, text);
		if (flag)
		{
			using (ReturnedDataLock.Lock())
			{
				version = new UTF7Encoding(allowOptionals: true).GetString(_returnedData.ToArray(), 3, _returnedData[2] - 5);
			}
		}
		return flag;
	}

	public bool CalibrateDac(byte data)
	{
		Queue<byte> queue = new Queue<byte>();
		queue.Enqueue(data);
		return RequestVBOXData(7, 9, queue, 4, 1500, ShowError: false, "CalibrateDac");
	}

	public bool CalibrateDac(float max1, float max2, float max3, float max4, float min1, float min2, float min3, float min4)
	{
		bool result = false;
		Queue<byte> data = new Queue<byte>();
		data.Enqueue(2);
		AddScaleOffsetData(ref data, min1);
		AddScaleOffsetData(ref data, min2);
		AddScaleOffsetData(ref data, min3);
		AddScaleOffsetData(ref data, min4);
		if (RequestVBOXData(7, 9, data, 4, 1500, ShowError: false, ""))
		{
			data.Clear();
			data.Enqueue(3);
			int num = 3136;
			max1 -= min1;
			if (max1 == 0f)
			{
				max1 = 0.1f;
			}
			AddScaleOffsetData(ref data, (float)num / max1);
			max2 -= min2;
			if (max2 == 0f)
			{
				max2 = 0.1f;
			}
			AddScaleOffsetData(ref data, (float)num / max2);
			max3 -= min3;
			if (max3 == 0f)
			{
				max3 = 0.1f;
			}
			AddScaleOffsetData(ref data, (float)num / max3);
			max4 -= min4;
			if (max4 == 0f)
			{
				max4 = 0.1f;
			}
			AddScaleOffsetData(ref data, (float)num / max4);
			if (RequestVBOXData(7, 9, data, 4, 1500, ShowError: false, ""))
			{
				data.Clear();
				data.Enqueue(1);
				AddScaleOffsetData(ref data, 5f);
				result = RequestVBOXData(7, 9, data, 4, 1500, ShowError: false, "");
			}
		}
		return result;
	}

	private void AddScaleOffsetData(ref Queue<byte> data, float value)
	{
		Union union = default(Union);
		union.data = value;
		data.Enqueue(union.b0_LSB);
		data.Enqueue(union.b1);
		data.Enqueue(union.b2);
		data.Enqueue(union.b3_MSB);
	}

	public bool FakeSerialTx_StandardProtocol(bool sendData)
	{
		bool result = false;
		if (SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			PercentComplete = 0.0;
			ProgressText = "FakeSerialTx_StandardProtocol";
			Queue<byte> queue = new Queue<byte>(1);
			queue.Enqueue((byte)(sendData ? byte.MaxValue : 0));
			if (RequestVBOXData(7, 55, queue, 4, 1500, ShowError: false, "FakeSerialTx_StandardProtocol"))
			{
				using (ReturnedDataLock.Lock())
				{
					result = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
			}
			ProgressText = string.Empty;
			SetQuiet_StandardProtocol(MakeQuiet: false, forceQuiet: true);
		}
		return result;
	}

	public void RequestMaximumUpdateRates(out int maximumGpsUpdateRate, out int maximumSerialUpdateRate)
	{
		maximumGpsUpdateRate = 1;
		maximumSerialUpdateRate = 1;
		bool flag = isReceivingVBoxComms;
		if (flag && !SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			return;
		}
		if (RequestVBOXData(7, 67, null, 13, 1500, ShowError: false, "RequestMaximumUpdateRates"))
		{
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 8)
				{
					for (int i = 0; i < 4; i++)
					{
						maximumGpsUpdateRate <<= 8;
						maximumGpsUpdateRate |= _returnedData[i];
						maximumSerialUpdateRate <<= 8;
						maximumSerialUpdateRate |= _returnedData[i + 4];
					}
				}
			}
		}
		if (flag)
		{
			SetQuiet_StandardProtocol(MakeQuiet: false, forceQuiet: true);
		}
	}

	public bool ResetTotalDistance()
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			if (RequestVBOXData(7, 56, null, 4, 1500, ShowError: false, "ResetTotalDistance"))
			{
				using (ReturnedDataLock.Lock())
				{
					result = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false, forceQuiet: true);
			}
		}
		return result;
	}

	public byte RequestDgpsModes()
	{
		byte result = 0;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			if (RequestVBOXData(7, 66, null, 6, 1500, ShowError: false, "RequestDgpsModes"))
			{
				using (ReturnedDataLock.Lock())
				{
					result = (byte)((_returnedData.Count == 1) ? _returnedData[0] : 0);
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false, forceQuiet: true);
			}
		}
		return result;
	}

	public bool ClearRam()
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true, forceQuiet: true))
		{
			PercentComplete = 0.0;
			ProgressText = Racelogic.Comms.Serial.Properties.Resources.ClearingRam;
			if (RequestVBOXData(7, 7, null, 4, 30000, ShowError: false, "ClearRam"))
			{
				using (ReturnedDataLock.Lock())
				{
					result = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
			}
			ProgressText = string.Empty;
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false, forceQuiet: true);
			}
		}
		return result;
	}

	public async Task<bool> SendAdasSetupToTargetAsync(AdasSyncTargetNumber target)
	{
		bool result = false;
		bool NoiseRequired = isReceivingVBoxComms;
		if (!NoiseRequired || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> data = new Queue<byte>();
			data.Enqueue(Convert.ToByte(target));
			if (RequestVBOXData(7, 123, data, 4, 1000, ShowError: false, "SetRlCanOuput"))
			{
				using (ReturnedDataLock.Lock())
				{
					result = _returnedData.Count == 1 && _returnedData[0] == 1;
				}
			}
			if (result)
			{
				result = await WaitForAdasSetupToBeSentToTarget();
			}
			if (NoiseRequired)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	public bool SetRlCanOuput(bool enableOutput)
	{
		bool result = false;
		bool flag = isReceivingVBoxComms;
		if (!flag || SetQuiet_StandardProtocol(MakeQuiet: true))
		{
			Queue<byte> queue = new Queue<byte>();
			queue.Enqueue((byte)(enableOutput ? byte.MaxValue : 0));
			if (RequestVBOXData(7, 124, queue, 5, 1000, ShowError: false, "SetRlCanOuput"))
			{
				using (ReturnedDataLock.Lock())
				{
					result = _returnedData.Count == 2 && _returnedData[0] == 1 && _returnedData[1] == 1;
				}
			}
			if (flag)
			{
				SetQuiet_StandardProtocol(MakeQuiet: false);
			}
		}
		return result;
	}

	private async Task<bool> WaitForAdasSetupToBeSentToTarget()
	{
		return await Task.Run(() => GetAdasSyncProgesss());
	}

	private bool GetAdasSyncProgesss()
	{
		bool flag = true;
		PercentComplete = 0.0;
		ProgressText = Racelogic.Comms.Serial.Properties.Resources.SendingAdasSetupToTarget;
		while (flag && PercentComplete < 100.0)
		{
			flag = RequestVBOXData(7, 125, null, 7, 1000, ShowError: false, "GetAdasSyncProgesss");
			if (!flag)
			{
				continue;
			}
			using (ReturnedDataLock.Lock())
			{
				if (_returnedData.Count == 4 && _returnedData[1] == 1)
				{
					flag = true;
					PercentComplete = (double)(100 * _returnedData[3]) / (double)(int)_returnedData[2];
					if (PercentComplete < 100.0)
					{
						DateTime now = DateTime.Now;
						DateTime dateTime = now + TimeSpan.FromSeconds(2.0);
						while (now < dateTime)
						{
							Thread.Sleep(dateTime - now);
							now = DateTime.Now;
						}
					}
				}
				else
				{
					flag = false;
				}
			}
		}
		ProgressText = string.Empty;
		PercentComplete = 0.0;
		return flag;
	}

	public GpsEngineType GetSxSlEngineType()
	{
		GpsEngineType gpsEngineType = GpsEngineType.Unknown;
		byte revision = 0;
		GetRevision(VBoxSubCommand.ReportRevision, out revision);
		string text = SendGpsEngineMessage_StandardProtocol("%Firmware%$JT", 39, GpsEngineType.P102_20Hz, revision).Trim();
		if (text.Contains("$>JT,"))
		{
			text = text.Remove(0, text.IndexOf("$>JT,") + 5);
			if (text.ToUpper().Contains("SX2A"))
			{
				gpsEngineType = GpsEngineType.Sx2a;
			}
			else if (text.ToUpper().Contains("SX2G"))
			{
				gpsEngineType = GpsEngineType.P102_20Hz;
			}
			else if (text.ToUpper().Contains("SX2I"))
			{
				gpsEngineType = GpsEngineType.Sx2i;
			}
			else if (text.ToUpper().Contains("DF3"))
			{
				gpsEngineType = GpsEngineType.P202;
			}
			else if (text.ToUpper().Contains("DM4"))
			{
				gpsEngineType = GpsEngineType.P302;
			}
			else if (text.ToUpper().Contains("S"))
			{
				gpsEngineType = GpsEngineType.Sl;
			}
			string text2 = SendGpsEngineMessage_StandardProtocol("%Revision%$JI", 39, GpsEngineType.P102_20Hz, revision).Trim();
			if (text2.Contains("$>JI,"))
			{
				text2 = text2.Remove(0, text2.IndexOf("$>JI,") + 5);
				string[] array = text2.Split(',');
				if (array.Length >= 7)
				{
					text2 = $"{text} {array[6]} ({array[0]})";
					if (gpsEngineType == GpsEngineType.P102_20Hz)
					{
						gpsEngineType = GpsEngineType.Unknown;
						int result;
						if (array[0].Length == 6)
						{
							if (array[0][0] >= '2' && array[0][0] <= '9')
							{
								gpsEngineType = GpsEngineType.Sx2i;
							}
						}
						else if (array[0].Length == 7 && int.TryParse(array[0], out result) && result > 8000000 && result < 8150000)
						{
							string[] array2 = array[5].Split('/');
							if (array2.Length == 3 && int.TryParse(array2[2], out result))
							{
								result -= 3000;
								gpsEngineType = (((result & 1) != 1) ? GpsEngineType.P102_10Hz : GpsEngineType.P102_20Hz);
							}
						}
					}
				}
			}
		}
		return gpsEngineType;
	}
}
