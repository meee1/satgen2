using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Racelogic.DataSource;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
public class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Racelogic.DataSource.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	public static string AccelerationUnit_FeetPerSecondSquared => ResourceManager.GetString("AccelerationUnit_FeetPerSecondSquared", resourceCulture);

	public static string AccelerationUnit_G => ResourceManager.GetString("AccelerationUnit_G", resourceCulture);

	public static string AccelerationUnit_MetresPerSecondSquared => ResourceManager.GetString("AccelerationUnit_MetresPerSecondSquared", resourceCulture);

	public static string ByteOrder_Intel => ResourceManager.GetString("ByteOrder_Intel", resourceCulture);

	public static string ByteOrder_Motorola => ResourceManager.GetString("ByteOrder_Motorola", resourceCulture);

	public static string CalculatedChannel_Distance => ResourceManager.GetString("CalculatedChannel_Distance", resourceCulture);

	public static string CalculatedChannel_ElapsedTime => ResourceManager.GetString("CalculatedChannel_ElapsedTime", resourceCulture);

	public static string CalculatedChannel_LatAcc => ResourceManager.GetString("CalculatedChannel_LatAcc", resourceCulture);

	public static string CalculatedChannel_LongAcc => ResourceManager.GetString("CalculatedChannel_LongAcc", resourceCulture);

	public static string CANTranChannels_Brake => ResourceManager.GetString("CANTranChannels_Brake", resourceCulture);

	public static string CANTranChannels_IgnitionKey => ResourceManager.GetString("CANTranChannels_IgnitionKey", resourceCulture);

	public static string CANTranChannels_Lights => ResourceManager.GetString("CANTranChannels_Lights", resourceCulture);

	public static string CANTranChannels_Reverse => ResourceManager.GetString("CANTranChannels_Reverse", resourceCulture);

	public static string CANTranChannels_RPM => ResourceManager.GetString("CANTranChannels_RPM", resourceCulture);

	public static string CANTranChannels_Speed => ResourceManager.GetString("CANTranChannels_Speed", resourceCulture);

	public static string CANTranOutputTypes_Frequency => ResourceManager.GetString("CANTranOutputTypes_Frequency", resourceCulture);

	public static string CANTranOutputTypes_Level => ResourceManager.GetString("CANTranOutputTypes_Level", resourceCulture);

	public static string CANTranPriorities_Level0 => ResourceManager.GetString("CANTranPriorities_Level0", resourceCulture);

	public static string CANTranPriorities_Level1 => ResourceManager.GetString("CANTranPriorities_Level1", resourceCulture);

	public static string CANTranPriorities_Level2 => ResourceManager.GetString("CANTranPriorities_Level2", resourceCulture);

	public static string CANTranPriorities_Level3 => ResourceManager.GetString("CANTranPriorities_Level3", resourceCulture);

	public static string CANTranPriorities_Level4 => ResourceManager.GetString("CANTranPriorities_Level4", resourceCulture);

	public static string CANTranPriorities_Level5 => ResourceManager.GetString("CANTranPriorities_Level5", resourceCulture);

	public static string CANTranPriorities_Level6 => ResourceManager.GetString("CANTranPriorities_Level6", resourceCulture);

	public static string CANTranPriorities_Level7 => ResourceManager.GetString("CANTranPriorities_Level7", resourceCulture);

	public static string CodeDiff => ResourceManager.GetString("CodeDiff", resourceCulture);

	public static string CombinedAcceleration => ResourceManager.GetString("CombinedAcceleration", resourceCulture);

	public static string DataFormat_Double => ResourceManager.GetString("DataFormat_Double", resourceCulture);

	public static string DataFormat_PseudoSigned => ResourceManager.GetString("DataFormat_PseudoSigned", resourceCulture);

	public static string DataFormat_Signed => ResourceManager.GetString("DataFormat_Signed", resourceCulture);

	public static string DataFormat_Single => ResourceManager.GetString("DataFormat_Single", resourceCulture);

	public static string DataFormat_Unsigned => ResourceManager.GetString("DataFormat_Unsigned", resourceCulture);

	public static string DaysText => ResourceManager.GetString("DaysText", resourceCulture);

	public static string Degrees => ResourceManager.GetString("Degrees", resourceCulture);

	public static string DegreeUnit_Degree => ResourceManager.GetString("DegreeUnit_Degree", resourceCulture);

	public static string DegreeUnit_Radians => ResourceManager.GetString("DegreeUnit_Radians", resourceCulture);

	public static string Disabled => ResourceManager.GetString("Disabled", resourceCulture);

	public static string Distance => ResourceManager.GetString("Distance", resourceCulture);

	public static string DistanceUnit_Feet => ResourceManager.GetString("DistanceUnit_Feet", resourceCulture);

	public static string DistanceUnit_Kilometres => ResourceManager.GetString("DistanceUnit_Kilometres", resourceCulture);

	public static string DistanceUnit_Metres => ResourceManager.GetString("DistanceUnit_Metres", resourceCulture);

	public static string DistanceUnit_Miles => ResourceManager.GetString("DistanceUnit_Miles", resourceCulture);

	public static string DistanceUnit_NauticalMiles => ResourceManager.GetString("DistanceUnit_NauticalMiles", resourceCulture);

	public static string East => ResourceManager.GetString("East", resourceCulture);

	public static string ElapsedTime => ResourceManager.GetString("ElapsedTime", resourceCulture);

	public static string Enabled => ResourceManager.GetString("Enabled", resourceCulture);

	public static string EngineCoolantTemp => ResourceManager.GetString("EngineCoolantTemp", resourceCulture);

	public static string EngineLoad => ResourceManager.GetString("EngineLoad", resourceCulture);

	public static string EngineOilTemperature => ResourceManager.GetString("EngineOilTemperature", resourceCulture);

	public static string EngineRpm => ResourceManager.GetString("EngineRpm", resourceCulture);

	public static string FixedPos => ResourceManager.GetString("FixedPos", resourceCulture);

	public static string FuelLevel => ResourceManager.GetString("FuelLevel", resourceCulture);

	public static string FuelPressure => ResourceManager.GetString("FuelPressure", resourceCulture);

	public static string GramsPerSec => ResourceManager.GetString("GramsPerSec", resourceCulture);

	public static string Hemisphere_East => ResourceManager.GetString("Hemisphere_East", resourceCulture);

	public static string Hemisphere_NoData => ResourceManager.GetString("Hemisphere_NoData", resourceCulture);

	public static string Hemisphere_North => ResourceManager.GetString("Hemisphere_North", resourceCulture);

	public static string Hemisphere_South => ResourceManager.GetString("Hemisphere_South", resourceCulture);

	public static string Hemisphere_West => ResourceManager.GetString("Hemisphere_West", resourceCulture);

	public static string HoursText => ResourceManager.GetString("HoursText", resourceCulture);

	public static string ImuCoasting => ResourceManager.GetString("ImuCoasting", resourceCulture);

	public static string IntakeAirTemperature => ResourceManager.GetString("IntakeAirTemperature", resourceCulture);

	public static string IntakeManifoldPressure => ResourceManager.GetString("IntakeManifoldPressure", resourceCulture);

	public static string KalmanFilterStatus_Error => ResourceManager.GetString("KalmanFilterStatus_Error", resourceCulture);

	public static string KalmanFilterStatus_Good => ResourceManager.GetString("KalmanFilterStatus_Good", resourceCulture);

	public static string KalmanFilterStatus_NoData => ResourceManager.GetString("KalmanFilterStatus_NoData", resourceCulture);

	public static string KalmanFilterStatus_NotEnabled => ResourceManager.GetString("KalmanFilterStatus_NotEnabled", resourceCulture);

	public static string KalmanFilterStatus_NotMoving => ResourceManager.GetString("KalmanFilterStatus_NotMoving", resourceCulture);

	public static string KalmanFilterStatus_NotReady => ResourceManager.GetString("KalmanFilterStatus_NotReady", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_CoastingTimeout => ResourceManager.GetString("KalmanFilterStatusExtraInformation_CoastingTimeout", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_Enabled => ResourceManager.GetString("KalmanFilterStatusExtraInformation_Enabled", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_GoodLock => ResourceManager.GetString("KalmanFilterStatusExtraInformation_GoodLock", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_IMUCoast => ResourceManager.GetString("KalmanFilterStatusExtraInformation_IMUCoast", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_ImuDataFound => ResourceManager.GetString("KalmanFilterStatusExtraInformation_ImuDataFound", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_ImuDataNotFound => ResourceManager.GetString("KalmanFilterStatusExtraInformation_ImuDataNotFound", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_ImuFound => ResourceManager.GetString("KalmanFilterStatusExtraInformation_ImuFound", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_ImuNotFound => ResourceManager.GetString("KalmanFilterStatusExtraInformation_ImuNotFound", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_Initialise => ResourceManager.GetString("KalmanFilterStatusExtraInformation_Initialise", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_Initialised => ResourceManager.GetString("KalmanFilterStatusExtraInformation_Initialised", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_NewImu => ResourceManager.GetString("KalmanFilterStatusExtraInformation_NewImu", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_NoSats => ResourceManager.GetString("KalmanFilterStatusExtraInformation_NoSats", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_NotEnabled => ResourceManager.GetString("KalmanFilterStatusExtraInformation_NotEnabled", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_NotInitialised => ResourceManager.GetString("KalmanFilterStatusExtraInformation_NotInitialised", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_Reset => ResourceManager.GetString("KalmanFilterStatusExtraInformation_Reset", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_SendOverSerialNotSelected => ResourceManager.GetString("KalmanFilterStatusExtraInformation_SendOverSerialNotSelected", resourceCulture);

	public static string KalmanFilterStatusExtraInformation_TestMode => ResourceManager.GetString("KalmanFilterStatusExtraInformation_TestMode", resourceCulture);

	public static string Kilometres => ResourceManager.GetString("Kilometres", resourceCulture);

	public static string LateralAcceleration => ResourceManager.GetString("LateralAcceleration", resourceCulture);

	public static string LatLongUnit_DegreesDecimalMinutes => ResourceManager.GetString("LatLongUnit_DegreesDecimalMinutes", resourceCulture);

	public static string LatLongUnit_Minutes => ResourceManager.GetString("LatLongUnit_Minutes", resourceCulture);

	public static string LongitudinalAcceleration => ResourceManager.GetString("LongitudinalAcceleration", resourceCulture);

	public static string MassAirFlowRate => ResourceManager.GetString("MassAirFlowRate", resourceCulture);

	public static string MassUnit_Gram => ResourceManager.GetString("MassUnit_Gram", resourceCulture);

	public static string MassUnit_Kilogram => ResourceManager.GetString("MassUnit_Kilogram", resourceCulture);

	public static string MassUnit_Newton => ResourceManager.GetString("MassUnit_Newton", resourceCulture);

	public static string MassUnit_Pound => ResourceManager.GetString("MassUnit_Pound", resourceCulture);

	public static string MassUnit_Stone => ResourceManager.GetString("MassUnit_Stone", resourceCulture);

	public static string MassUnit_Ton => ResourceManager.GetString("MassUnit_Ton", resourceCulture);

	public static string MassUnit_Tonne => ResourceManager.GetString("MassUnit_Tonne", resourceCulture);

	public static string Miles => ResourceManager.GetString("Miles", resourceCulture);

	public static string MinutesText => ResourceManager.GetString("MinutesText", resourceCulture);

	public static string MultiplexorType_MultiplexedSignal => ResourceManager.GetString("MultiplexorType_MultiplexedSignal", resourceCulture);

	public static string MultiplexorType_MultiplexorSignal => ResourceManager.GetString("MultiplexorType_MultiplexorSignal", resourceCulture);

	public static string MultiplexorType_Signal => ResourceManager.GetString("MultiplexorType_Signal", resourceCulture);

	public static string Nmea3DFix_Fix2D => ResourceManager.GetString("Nmea3DFix_Fix2D", resourceCulture);

	public static string Nmea3DFix_Fix3D => ResourceManager.GetString("Nmea3DFix_Fix3D", resourceCulture);

	public static string Nmea3DFix_NA => ResourceManager.GetString("Nmea3DFix_NA", resourceCulture);

	public static string Nmea3DFix_NoData => ResourceManager.GetString("Nmea3DFix_NoData", resourceCulture);

	public static string Nmea3DFix_NoFix => ResourceManager.GetString("Nmea3DFix_NoFix", resourceCulture);

	public static string NmeaActiveIndicator_Active => ResourceManager.GetString("NmeaActiveIndicator_Active", resourceCulture);

	public static string NmeaActiveIndicator_NoData => ResourceManager.GetString("NmeaActiveIndicator_NoData", resourceCulture);

	public static string NmeaActiveIndicator_Void => ResourceManager.GetString("NmeaActiveIndicator_Void", resourceCulture);

	public static string NmeaFixQuality_DgpsFix => ResourceManager.GetString("NmeaFixQuality_DgpsFix", resourceCulture);

	public static string NmeaFixQuality_Estimated => ResourceManager.GetString("NmeaFixQuality_Estimated", resourceCulture);

	public static string NmeaFixQuality_FloatRtk => ResourceManager.GetString("NmeaFixQuality_FloatRtk", resourceCulture);

	public static string NmeaFixQuality_GpsFix => ResourceManager.GetString("NmeaFixQuality_GpsFix", resourceCulture);

	public static string NmeaFixQuality_Invalid => ResourceManager.GetString("NmeaFixQuality_Invalid", resourceCulture);

	public static string NmeaFixQuality_Manual => ResourceManager.GetString("NmeaFixQuality_Manual", resourceCulture);

	public static string NmeaFixQuality_NoData => ResourceManager.GetString("NmeaFixQuality_NoData", resourceCulture);

	public static string NmeaFixQuality_PpsFix => ResourceManager.GetString("NmeaFixQuality_PpsFix", resourceCulture);

	public static string NmeaFixQuality_Rtk => ResourceManager.GetString("NmeaFixQuality_Rtk", resourceCulture);

	public static string NmeaFixQuality_Simulation => ResourceManager.GetString("NmeaFixQuality_Simulation", resourceCulture);

	public static string NmeaFixSelection_Auto => ResourceManager.GetString("NmeaFixSelection_Auto", resourceCulture);

	public static string NmeaFixSelection_Manual => ResourceManager.GetString("NmeaFixSelection_Manual", resourceCulture);

	public static string NmeaFixSelection_NoData => ResourceManager.GetString("NmeaFixSelection_NoData", resourceCulture);

	public static string NmeaMessages_Gga => ResourceManager.GetString("NmeaMessages_Gga", resourceCulture);

	public static string NmeaMessages_Gll => ResourceManager.GetString("NmeaMessages_Gll", resourceCulture);

	public static string NmeaMessages_Gsa => ResourceManager.GetString("NmeaMessages_Gsa", resourceCulture);

	public static string NmeaMessages_Gst => ResourceManager.GetString("NmeaMessages_Gst", resourceCulture);

	public static string NmeaMessages_Gsv => ResourceManager.GetString("NmeaMessages_Gsv", resourceCulture);

	public static string NmeaMessages_None => ResourceManager.GetString("NmeaMessages_None", resourceCulture);

	public static string NmeaMessages_Rmc => ResourceManager.GetString("NmeaMessages_Rmc", resourceCulture);

	public static string NmeaMessages_Vtg => ResourceManager.GetString("NmeaMessages_Vtg", resourceCulture);

	public static string NmeaMessages_Zda => ResourceManager.GetString("NmeaMessages_Zda", resourceCulture);

	public static string NmeaModeIndicator_Autonomous => ResourceManager.GetString("NmeaModeIndicator_Autonomous", resourceCulture);

	public static string NmeaModeIndicator_Differential => ResourceManager.GetString("NmeaModeIndicator_Differential", resourceCulture);

	public static string NmeaModeIndicator_Estimated => ResourceManager.GetString("NmeaModeIndicator_Estimated", resourceCulture);

	public static string NmeaModeIndicator_NoData => ResourceManager.GetString("NmeaModeIndicator_NoData", resourceCulture);

	public static string NmeaModeIndicator_NotValid => ResourceManager.GetString("NmeaModeIndicator_NotValid", resourceCulture);

	public static string NmeaModeIndicator_Simulator => ResourceManager.GetString("NmeaModeIndicator_Simulator", resourceCulture);

	public static string NoData => ResourceManager.GetString("NoData", resourceCulture);

	public static string North => ResourceManager.GetString("North", resourceCulture);

	public static string NoSolution => ResourceManager.GetString("NoSolution", resourceCulture);

	public static string Percent => ResourceManager.GetString("Percent", resourceCulture);

	public static string PressureUnit_Atmosphere => ResourceManager.GetString("PressureUnit_Atmosphere", resourceCulture);

	public static string PressureUnit_Bar => ResourceManager.GetString("PressureUnit_Bar", resourceCulture);

	public static string PressureUnit_inHG => ResourceManager.GetString("PressureUnit_inHG", resourceCulture);

	public static string PressureUnit_KiloPascal => ResourceManager.GetString("PressureUnit_KiloPascal", resourceCulture);

	public static string PressureUnit_mBar => ResourceManager.GetString("PressureUnit_mBar", resourceCulture);

	public static string PressureUnit_Psi => ResourceManager.GetString("PressureUnit_Psi", resourceCulture);

	public static string Radians => ResourceManager.GetString("Radians", resourceCulture);

	public static string Rpm => ResourceManager.GetString("Rpm", resourceCulture);

	public static string RTKFixed => ResourceManager.GetString("RTKFixed", resourceCulture);

	public static string RTKFloat => ResourceManager.GetString("RTKFloat", resourceCulture);

	public static string SecondsText => ResourceManager.GetString("SecondsText", resourceCulture);

	public static string SoundUnit_db => ResourceManager.GetString("SoundUnit_db", resourceCulture);

	public static string South => ResourceManager.GetString("South", resourceCulture);

	public static string SpeedUnit_FeetPerSecond => ResourceManager.GetString("SpeedUnit_FeetPerSecond", resourceCulture);

	public static string SpeedUnit_KilometresPerHour => ResourceManager.GetString("SpeedUnit_KilometresPerHour", resourceCulture);

	public static string SpeedUnit_Knots => ResourceManager.GetString("SpeedUnit_Knots", resourceCulture);

	public static string SpeedUnit_MetresPerSecond => ResourceManager.GetString("SpeedUnit_MetresPerSecond", resourceCulture);

	public static string SpeedUnit_MilesPerHour => ResourceManager.GetString("SpeedUnit_MilesPerHour", resourceCulture);

	public static string SplitType_Finish => ResourceManager.GetString("SplitType_Finish", resourceCulture);

	public static string SplitType_None => ResourceManager.GetString("SplitType_None", resourceCulture);

	public static string SplitType_PitLaneFinish => ResourceManager.GetString("SplitType_PitLaneFinish", resourceCulture);

	public static string SplitType_PitLaneStart => ResourceManager.GetString("SplitType_PitLaneStart", resourceCulture);

	public static string splitType_SectorEnd => ResourceManager.GetString("splitType_SectorEnd", resourceCulture);

	public static string SplitType_SectorStart => ResourceManager.GetString("SplitType_SectorStart", resourceCulture);

	public static string SplitType_Split => ResourceManager.GetString("SplitType_Split", resourceCulture);

	public static string SplitType_StartFinish => ResourceManager.GetString("SplitType_StartFinish", resourceCulture);

	public static string StandAlone => ResourceManager.GetString("StandAlone", resourceCulture);

	public static string TemperatureUnit_DegreeC => ResourceManager.GetString("TemperatureUnit_DegreeC", resourceCulture);

	public static string TemperatureUnit_Fahrenheit => ResourceManager.GetString("TemperatureUnit_Fahrenheit", resourceCulture);

	public static string TemperatureUnit_Kelvin => ResourceManager.GetString("TemperatureUnit_Kelvin", resourceCulture);

	public static string ThrottlePosition => ResourceManager.GetString("ThrottlePosition", resourceCulture);

	public static string TimeUnit_DaysHoursMinutesSeconds => ResourceManager.GetString("TimeUnit_DaysHoursMinutesSeconds", resourceCulture);

	public static string TimeUnit_DaysHoursMinutesWholeSeconds => ResourceManager.GetString("TimeUnit_DaysHoursMinutesWholeSeconds", resourceCulture);

	public static string TimeUnit_HoursMinutes => ResourceManager.GetString("TimeUnit_HoursMinutes", resourceCulture);

	public static string TimeUnit_HoursMinutesSeconds => ResourceManager.GetString("TimeUnit_HoursMinutesSeconds", resourceCulture);

	public static string TimeUnit_HoursMinutesSecondsNoSeparator => ResourceManager.GetString("TimeUnit_HoursMinutesSecondsNoSeparator", resourceCulture);

	public static string TimeUnit_HoursMinutesSecondsPlusNonZeroDays => ResourceManager.GetString("TimeUnit_HoursMinutesSecondsPlusNonZeroDays", resourceCulture);

	public static string TimeUnit_HoursMinutesWholeSeconds => ResourceManager.GetString("TimeUnit_HoursMinutesWholeSeconds", resourceCulture);

	public static string TimeUnit_HoursMinutesWholeSecondsNoSeparator => ResourceManager.GetString("TimeUnit_HoursMinutesWholeSecondsNoSeparator", resourceCulture);

	public static string TimeUnit_Milliseconds => ResourceManager.GetString("TimeUnit_Milliseconds", resourceCulture);

	public static string TimeUnit_SecondsSinceMidnight => ResourceManager.GetString("TimeUnit_SecondsSinceMidnight", resourceCulture);

	public static string Unknown => ResourceManager.GetString("Unknown", resourceCulture);

	public static string VBoxChannel_AviFileIndex => ResourceManager.GetString("VBoxChannel_AviFileIndex", resourceCulture);

	public static string VBoxChannel_AviSyncTime => ResourceManager.GetString("VBoxChannel_AviSyncTime", resourceCulture);

	public static string VBoxChannel_Battery1Voltage => ResourceManager.GetString("VBoxChannel_Battery1Voltage", resourceCulture);

	public static string VBoxChannel_Battery2Voltage => ResourceManager.GetString("VBoxChannel_Battery2Voltage", resourceCulture);

	public static string VBoxChannel_BrakeDistance => ResourceManager.GetString("VBoxChannel_BrakeDistance", resourceCulture);

	public static string VBoxChannel_CompactFlashBufferSize => ResourceManager.GetString("VBoxChannel_CompactFlashBufferSize", resourceCulture);

	public static string VBoxChannel_Distance => ResourceManager.GetString("VBoxChannel_Distance", resourceCulture);

	public static string VBoxChannel_Drift => ResourceManager.GetString("VBoxChannel_Drift", resourceCulture);

	public static string VBoxChannel_DualAntenna => ResourceManager.GetString("VBoxChannel_DualAntenna", resourceCulture);

	public static string VBoxChannel_EndOfAcceleration => ResourceManager.GetString("VBoxChannel_EndOfAcceleration", resourceCulture);

	public static string VBoxChannel_Event2Time => ResourceManager.GetString("VBoxChannel_Event2Time", resourceCulture);

	public static string VBoxChannel_GlonassSatellites => ResourceManager.GetString("VBoxChannel_GlonassSatellites", resourceCulture);

	public static string VBoxChannel_GpsSatellites => ResourceManager.GetString("VBoxChannel_GpsSatellites", resourceCulture);

	public static string VBoxChannel_Heading => ResourceManager.GetString("VBoxChannel_Heading", resourceCulture);

	public static string VBoxChannel_Height => ResourceManager.GetString("VBoxChannel_Height", resourceCulture);

	public static string VBoxChannel_InternalA2D1 => ResourceManager.GetString("VBoxChannel_InternalA2D1", resourceCulture);

	public static string VBoxChannel_InternalA2D2 => ResourceManager.GetString("VBoxChannel_InternalA2D2", resourceCulture);

	public static string VBoxChannel_InternalA2D3 => ResourceManager.GetString("VBoxChannel_InternalA2D3", resourceCulture);

	public static string VBoxChannel_InternalA2D4 => ResourceManager.GetString("VBoxChannel_InternalA2D4", resourceCulture);

	public static string VBoxChannel_InternalTemperature => ResourceManager.GetString("VBoxChannel_InternalTemperature", resourceCulture);

	public static string VBoxChannel_LateralAcceleration => ResourceManager.GetString("VBoxChannel_LateralAcceleration", resourceCulture);

	public static string VBoxChannel_Latitude => ResourceManager.GetString("VBoxChannel_Latitude", resourceCulture);

	public static string VBoxChannel_Longitude => ResourceManager.GetString("VBoxChannel_Longitude", resourceCulture);

	public static string VBoxChannel_LongitudinalAcceleration => ResourceManager.GetString("VBoxChannel_LongitudinalAcceleration", resourceCulture);

	public static string VBoxChannel_MemoryUsed => ResourceManager.GetString("VBoxChannel_MemoryUsed", resourceCulture);

	public static string VBoxChannel_None => ResourceManager.GetString("VBoxChannel_None", resourceCulture);

	public static string VBoxChannel_Satellites => ResourceManager.GetString("VBoxChannel_Satellites", resourceCulture);

	public static string VBoxChannel_SolutionType => ResourceManager.GetString("VBoxChannel_SolutionType", resourceCulture);

	public static string VBoxChannel_Speed => ResourceManager.GetString("VBoxChannel_Speed", resourceCulture);

	public static string VBoxChannel_TimeFromTestStart => ResourceManager.GetString("VBoxChannel_TimeFromTestStart", resourceCulture);

	public static string VBoxChannel_TriggerEventTime => ResourceManager.GetString("VBoxChannel_TriggerEventTime", resourceCulture);

	public static string VBoxChannel_UtcTime => ResourceManager.GetString("VBoxChannel_UtcTime", resourceCulture);

	public static string VBoxChannel_VBox3Rms_VBMiniYaw => ResourceManager.GetString("VBoxChannel_VBox3Rms_VBMiniYaw", resourceCulture);

	public static string VBoxChannel_VelocityQuality => ResourceManager.GetString("VBoxChannel_VelocityQuality", resourceCulture);

	public static string VBoxChannel_VerticalVelocity => ResourceManager.GetString("VBoxChannel_VerticalVelocity", resourceCulture);

	public static string VBoxChannel_WavFile => ResourceManager.GetString("VBoxChannel_WavFile", resourceCulture);

	public static string VBoxChannel_Yaw01LateralAcceleration => ResourceManager.GetString("VBoxChannel_Yaw01LateralAcceleration", resourceCulture);

	public static string VBoxChannel_Yaw01Status => ResourceManager.GetString("VBoxChannel_Yaw01Status", resourceCulture);

	public static string VBoxChannel_Yaw01YawRate => ResourceManager.GetString("VBoxChannel_Yaw01YawRate", resourceCulture);

	public static string VBoxChannel2_BeidouSatellites => ResourceManager.GetString("VBoxChannel2_BeidouSatellites", resourceCulture);

	public static string VBoxChannel2_CentrelineDeviation => ResourceManager.GetString("VBoxChannel2_CentrelineDeviation", resourceCulture);

	public static string VBoxChannel2_CorrectedDistance => ResourceManager.GetString("VBoxChannel2_CorrectedDistance", resourceCulture);

	public static string VBoxChannel2_Date => ResourceManager.GetString("VBoxChannel2_Date", resourceCulture);

	public static string VBoxChannel2_HeadingImu2 => ResourceManager.GetString("VBoxChannel2_HeadingImu2", resourceCulture);

	public static string VBoxChannel2_HeadingKf => ResourceManager.GetString("VBoxChannel2_HeadingKf", resourceCulture);

	public static string VBoxChannel2_PitchAngleKf => ResourceManager.GetString("VBoxChannel2_PitchAngleKf", resourceCulture);

	public static string VBoxChannel2_PitchRateImu => ResourceManager.GetString("VBoxChannel2_PitchRateImu", resourceCulture);

	public static string VBoxChannel2_PositionQuality => ResourceManager.GetString("VBoxChannel2_PositionQuality", resourceCulture);

	public static string VBoxChannel2_RollAngleKf => ResourceManager.GetString("VBoxChannel2_RollAngleKf", resourceCulture);

	public static string VBoxChannel2_RollRateImu => ResourceManager.GetString("VBoxChannel2_RollRateImu", resourceCulture);

	public static string VBoxChannel2_T1 => ResourceManager.GetString("VBoxChannel2_T1", resourceCulture);

	public static string VBoxChannel2_WheelSpeed1 => ResourceManager.GetString("VBoxChannel2_WheelSpeed1", resourceCulture);

	public static string VBoxChannel2_WheelSpeed2 => ResourceManager.GetString("VBoxChannel2_WheelSpeed2", resourceCulture);

	public static string VBoxChannel2_XAccelImu => ResourceManager.GetString("VBoxChannel2_XAccelImu", resourceCulture);

	public static string VBoxChannel2_YAccelImu => ResourceManager.GetString("VBoxChannel2_YAccelImu", resourceCulture);

	public static string VBoxChannel2_YawRateImu => ResourceManager.GetString("VBoxChannel2_YawRateImu", resourceCulture);

	public static string VBoxChannel2_ZAccelImu => ResourceManager.GetString("VBoxChannel2_ZAccelImu", resourceCulture);

	public static string VehicleSpeed => ResourceManager.GetString("VehicleSpeed", resourceCulture);

	public static string VerticalAcceleration => ResourceManager.GetString("VerticalAcceleration", resourceCulture);

	public static string West => ResourceManager.GetString("West", resourceCulture);

	internal Resources()
	{
	}
}
