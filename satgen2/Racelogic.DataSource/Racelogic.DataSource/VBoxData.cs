using System;
using System.Collections.ObjectModel;
using Racelogic.Core;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

public class VBoxData : BasePropertyChanged
{
	private VBoxType vboxType = VBoxType.VBoxII;

	private int serialNumber;

	private double sampleRateHz;

	private ushort crcError;

	private ushort sampleError;

	private int unrecognisedCharacter;

	private DistanceMetres heightMetres;

	private AccelerationG lateralAccelerationG;

	private AccelerationG longitudinalAccelerationG;

	private LongitudeMinutes longitudeXMinutes;

	private LatitudeMinutes latitudeYMinutes;

	private SolutionType solutionType;

	private SpeedKilometresPerHour speedKilometresPerHour;

	private SpeedKilometresPerHour speedQualityKilometersPerHour;

	private TimeSeconds utcTime;

	private SpeedKilometresPerHour verticalVelocityKilometresPerHour;

	private byte satellites;

	private byte gpsSatellites;

	private byte glonassSatellites;

	private bool brakeTrigger;

	private bool dgps;

	private bool dualAntenna;

	private bool wavFile;

	private double heading;

	private TimeSeconds triggerEventTimeSeconds;

	private double memoryUsed;

	private ObservableCollection<CanChannel> internalA2D;

	public VBoxType VBoxType
	{
		get
		{
			return vboxType;
		}
		set
		{
			vboxType = value;
		}
	}

	public double SampleRateHz
	{
		get
		{
			return sampleRateHz;
		}
		set
		{
			sampleRateHz = value;
		}
	}

	public ushort CrcError
	{
		get
		{
			return crcError;
		}
		set
		{
			crcError = value;
		}
	}

	public ushort SampleError
	{
		get
		{
			return sampleError;
		}
		set
		{
			sampleError = value;
		}
	}

	public int UnrecognisedCharacter
	{
		get
		{
			return unrecognisedCharacter;
		}
		set
		{
			unrecognisedCharacter = value;
		}
	}

	public DistanceMetres HeightMetres
	{
		get
		{
			return heightMetres;
		}
		set
		{
			heightMetres = value;
		}
	}

	public AccelerationG LateralAccelerationG
	{
		get
		{
			return lateralAccelerationG;
		}
		set
		{
			lateralAccelerationG = value;
		}
	}

	public AccelerationG LongitudinalAccelerationG
	{
		get
		{
			return longitudinalAccelerationG;
		}
		set
		{
			longitudinalAccelerationG = value;
		}
	}

	public LongitudeMinutes LongitudeXMinutes
	{
		get
		{
			return longitudeXMinutes;
		}
		set
		{
			longitudeXMinutes = value;
		}
	}

	public LatitudeMinutes LatitudeYMinutes
	{
		get
		{
			return latitudeYMinutes;
		}
		set
		{
			latitudeYMinutes = value;
		}
	}

	public SolutionType SolutionType
	{
		get
		{
			return solutionType;
		}
		set
		{
			solutionType = value;
		}
	}

	public SpeedKilometresPerHour SpeedKilometresPerHour
	{
		get
		{
			return speedKilometresPerHour;
		}
		set
		{
			speedKilometresPerHour = value;
		}
	}

	public SpeedKilometresPerHour SpeedQualityKilometersPerHour
	{
		get
		{
			return speedQualityKilometersPerHour;
		}
		set
		{
			speedQualityKilometersPerHour = value;
		}
	}

	public TimeSeconds UtcTime
	{
		get
		{
			return utcTime;
		}
		set
		{
			utcTime = value;
		}
	}

	public SpeedKilometresPerHour VerticalVelocityKilometresPerHour
	{
		get
		{
			return verticalVelocityKilometresPerHour;
		}
		set
		{
			verticalVelocityKilometresPerHour = value;
		}
	}

	public byte Satellites
	{
		get
		{
			return satellites;
		}
		set
		{
			satellites = value;
		}
	}

	public byte GpsSatellites
	{
		get
		{
			return gpsSatellites;
		}
		set
		{
			gpsSatellites = value;
		}
	}

	public byte BeidouSatellites { get; set; }

	public byte GlonassSatellites
	{
		get
		{
			return glonassSatellites;
		}
		set
		{
			glonassSatellites = value;
		}
	}

	public bool BrakeTrigger
	{
		get
		{
			return brakeTrigger;
		}
		set
		{
			brakeTrigger = value;
		}
	}

	public bool Dgps
	{
		get
		{
			return dgps;
		}
		set
		{
			dgps = value;
		}
	}

	public bool DualAntenna
	{
		get
		{
			return dualAntenna;
		}
		set
		{
			dualAntenna = value;
		}
	}

	public bool WavFile
	{
		get
		{
			return wavFile;
		}
		set
		{
			wavFile = value;
		}
	}

	public double Heading
	{
		get
		{
			return heading;
		}
		set
		{
			heading = value;
		}
	}

	public TimeSeconds TriggerEventTimeSeconds
	{
		get
		{
			return triggerEventTimeSeconds;
		}
		set
		{
			triggerEventTimeSeconds = value;
		}
	}

	public double MemoryUsed
	{
		get
		{
			return memoryUsed;
		}
		set
		{
			memoryUsed = value;
		}
	}

	public int SerialNumber
	{
		get
		{
			return serialNumber;
		}
		set
		{
			serialNumber = value;
		}
	}

	public DistanceMetres Drift { get; set; }

	public short KalmanFilterCode { get; set; }

	public string KalmanFilterStatus { get; set; }

	public string KalmanFilterStatusExtraInformation { get; set; }

	public ObservableCollection<CanChannel> InternalA2D
	{
		get
		{
			return internalA2D;
		}
		set
		{
			internalA2D = value;
			OnPropertyChanged("InternalA2D");
		}
	}

	public double PitchAngleKf { get; set; }

	public double RollAngleKf { get; set; }

	public double HeadingKf { get; set; }

	public double PitchRateImu { get; set; }

	public double RollRateImu { get; set; }

	public double YawRateImu { get; set; }

	public double XAccelImu { get; set; }

	public double YAccelImu { get; set; }

	public double ZAccelImu { get; set; }

	public DateTime Date { get; set; }

	public byte PositionQuality { get; set; }

	public double T1 { get; set; }

	public double WheelSpeed1 { get; set; }

	public double WheelSpeed2 { get; set; }

	public double HeadingImu2 { get; set; }

	public VBoxData()
	{
		internalA2D = new ObservableCollection<CanChannel>();
		for (int i = 0; i < 4; i++)
		{
			internalA2D.Add(new CanChannel());
		}
		Clear(clearCrcCount: true);
	}

	public void Clear(bool clearCrcCount, bool inSetup = false)
	{
		if (!inSetup)
		{
			VBoxType = VBoxType.Unknown;
		}
		Satellites = 0;
		BeidouSatellites = 0;
		GlonassSatellites = 0;
		GpsSatellites = 0;
		SolutionType = 0;
		BrakeTrigger = false;
		DualAntenna = false;
		Dgps = false;
		WavFile = false;
		UtcTime = 0.0;
		LatitudeYMinutes = 0.0;
		LongitudeXMinutes = 0.0;
		SpeedKilometresPerHour = 0.0;
		Heading = 0.0;
		HeightMetres = 0.0;
		VerticalVelocityKilometresPerHour = 0.0;
		SolutionType = -1;
		SpeedQualityKilometersPerHour = 0.0;
		LateralAccelerationG = 0.0;
		LongitudinalAccelerationG = 0.0;
		SampleRateHz = 0.0;
		MemoryUsed = 0.0;
		TriggerEventTimeSeconds = 0.0;
		SerialNumber = 0;
		Drift = 0.0;
		for (int i = 0; i < InternalA2D.Count; i++)
		{
			InternalA2D[i].Value = 0.0;
			InternalA2D[i].IsBeingSentOverSerial = false;
		}
		KalmanFilterCode = 0;
		KalmanFilterStatus = string.Empty;
		KalmanFilterStatusExtraInformation = string.Empty;
		PitchAngleKf = 0.0;
		RollAngleKf = 0.0;
		HeadingKf = 0.0;
		PitchRateImu = 0.0;
		RollRateImu = 0.0;
		YawRateImu = 0.0;
		XAccelImu = 0.0;
		YAccelImu = 0.0;
		ZAccelImu = 0.0;
		Date = default(DateTime);
		PositionQuality = 0;
		T1 = 0.0;
		WheelSpeed1 = 0.0;
		WheelSpeed2 = 0.0;
		HeadingImu2 = 0.0;
		if (clearCrcCount)
		{
			CrcError = 0;
			UnrecognisedCharacter = 0;
		}
	}

	public void Clone(VBoxData data)
	{
		data.VBoxType = VBoxType;
		data.Satellites = Satellites;
		data.BeidouSatellites = BeidouSatellites;
		data.BrakeTrigger = BrakeTrigger;
		data.DualAntenna = DualAntenna;
		data.Dgps = Dgps;
		data.WavFile = WavFile;
		data.UtcTime = UtcTime;
		data.LatitudeYMinutes = LatitudeYMinutes;
		data.LongitudeXMinutes = LongitudeXMinutes;
		data.SpeedKilometresPerHour = SpeedKilometresPerHour;
		data.Heading = Heading;
		data.HeightMetres = HeightMetres;
		data.VerticalVelocityKilometresPerHour = VerticalVelocityKilometresPerHour;
		data.SolutionType = SolutionType;
		data.SpeedQualityKilometersPerHour = SpeedQualityKilometersPerHour;
		data.TriggerEventTimeSeconds = TriggerEventTimeSeconds;
		data.LateralAccelerationG = LateralAccelerationG;
		data.LongitudinalAccelerationG = LongitudinalAccelerationG;
		data.MemoryUsed = MemoryUsed;
		data.SampleRateHz = SampleRateHz;
		data.KalmanFilterCode = KalmanFilterCode;
		data.KalmanFilterStatus = KalmanFilterStatus;
		data.KalmanFilterStatusExtraInformation = KalmanFilterStatusExtraInformation;
		data.CrcError = CrcError;
		for (int i = 0; i < internalA2D.Count; i++)
		{
			data.internalA2D[i] = (CanChannel)internalA2D[i].Clone();
		}
		data.PitchAngleKf = PitchAngleKf;
		data.RollAngleKf = RollAngleKf;
		data.HeadingKf = HeadingKf;
		data.PitchRateImu = PitchRateImu;
		data.RollRateImu = RollRateImu;
		data.YawRateImu = YawRateImu;
		data.XAccelImu = XAccelImu;
		data.YAccelImu = YAccelImu;
		data.ZAccelImu = ZAccelImu;
		data.Date = Date;
		data.PositionQuality = PositionQuality;
		data.T1 = T1;
		data.WheelSpeed1 = WheelSpeed1;
		data.WheelSpeed2 = WheelSpeed2;
		data.HeadingImu2 = HeadingImu2;
	}

	public VBoxData Clone()
	{
		VBoxData vBoxData = new VBoxData();
		Clone(vBoxData);
		return vBoxData;
	}

	public static double GetGpsLatency(VBoxType vboxType, GpsEngineType gpsEngine, byte firmwareRevision, bool rtkLock)
	{
		double result = double.NaN;
		switch (vboxType)
		{
		case VBoxType.VBox3i:
		case VBoxType.VB2100:
		case VBoxType.VBoxMini:
		case VBoxType.VideoVBox:
		case VBoxType.VBoxMicro:
		case VBoxType.BrakeTestSpeedSensor:
		case VBoxType.VB3is:
			result = 0.0;
			break;
		case VBoxType.VBox3:
			result = (((uint)(gpsEngine - 2) > 1u) ? 0.0125 : 0.0068);
			break;
		case VBoxType.VB2Sl:
			result = (rtkLock ? 0.031516 : 0.030516);
			break;
		case VBoxType.VB2Sx:
		case VBoxType.VB2Sx2:
			result = ((firmwareRevision >= 33) ? 0.042216 : 0.0475);
			break;
		case VBoxType.VB3Tr2:
		case VBoxType.VB3Tr3:
			result = 0.0068;
			break;
		case VBoxType.VBSx10:
			result = 0.041502;
			switch (gpsEngine)
			{
			case GpsEngineType.P102_20Hz:
				result = 0.047886;
				break;
			case GpsEngineType.P102_10Hz:
				result = 0.048052;
				break;
			case GpsEngineType.Unknown:
				result = double.NaN;
				break;
			}
			break;
		case VBoxType.VBoxII:
			result = ((gpsEngine != GpsEngineType.Sx2a) ? 0.025 : 0.054);
			break;
		}
		return result;
	}

	public static (double minimumSpeedKmh, int samplesToAverage, uint smoothLevel) GetMinimumSpeedOverride(UnitInformation unitInfo, GpsEngineType gpsEngine, byte firmwareRevision)
	{
		switch (unitInfo?.UnitType)
		{
		case 103:
			return (2.5, 7, 2u);
		case 45:
			if (unitInfo.SubType == '\u0001')
			{
				return (2.5, 7, 2u);
			}
			break;
		}
		return (0.0, 0, 0u);
	}

	public static void GetKalmanFilterStatusAsString(short code, out string status, out string extraInformation)
	{
		switch (code)
		{
		case -1:
			status = Resources.KalmanFilterStatus_NoData;
			extraInformation = "";
			return;
		case 61:
		case 317:
			status = Resources.KalmanFilterStatus_Good;
			break;
		case 53:
		case 309:
			status = Resources.KalmanFilterStatus_NotReady;
			break;
		case 565:
		case 821:
			status = Resources.KalmanFilterStatus_NotMoving;
			break;
		case 29:
		case 285:
			status = Resources.KalmanFilterStatusExtraInformation_IMUCoast;
			break;
		default:
			status = (((code & 4) == 4) ? Resources.KalmanFilterStatus_Error : Resources.KalmanFilterStatus_NotEnabled);
			break;
		}
		string text = "";
		text = (((code & 4) != 4) ? Resources.KalmanFilterStatusExtraInformation_NotEnabled : Resources.KalmanFilterStatusExtraInformation_Enabled);
		text += Environment.NewLine;
		text = (((code & 0x100) != 256) ? (text + Resources.KalmanFilterStatusExtraInformation_ImuNotFound) : (text + Resources.KalmanFilterStatusExtraInformation_ImuFound));
		text += Environment.NewLine;
		if ((code & 1) == 1)
		{
			text += Resources.KalmanFilterStatusExtraInformation_NewImu;
			text += Environment.NewLine;
		}
		if ((code & 2) == 2)
		{
			text += Resources.KalmanFilterStatusExtraInformation_TestMode;
			text += Environment.NewLine;
		}
		text = (((code & 0x10) != 16) ? (text + Resources.KalmanFilterStatusExtraInformation_ImuDataNotFound) : (text + Resources.KalmanFilterStatusExtraInformation_ImuDataFound));
		text += Environment.NewLine;
		text = (((code & 8) != 8) ? (text + Resources.KalmanFilterStatusExtraInformation_NotInitialised) : (text + Resources.KalmanFilterStatusExtraInformation_Initialised));
		text += Environment.NewLine;
		text = (((code & 0x20) != 32) ? (text + Resources.KalmanFilterStatusExtraInformation_NoSats) : (text + Resources.KalmanFilterStatusExtraInformation_GoodLock));
		text += Environment.NewLine;
		if ((code & 0x40) == 64)
		{
			text += Resources.KalmanFilterStatusExtraInformation_Reset;
			text += Environment.NewLine;
		}
		if ((code & 0x80) == 128)
		{
			text += Resources.KalmanFilterStatusExtraInformation_CoastingTimeout;
			text += Environment.NewLine;
		}
		if ((code & 0x200) == 512)
		{
			text += Resources.KalmanFilterStatusExtraInformation_Initialise;
			text += Environment.NewLine;
		}
		extraInformation = text;
	}
}
