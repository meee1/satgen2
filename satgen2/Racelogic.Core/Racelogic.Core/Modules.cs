using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Racelogic.Core;

[Serializable]
public static class Modules
{
	private static List<ModuleDefinition> moduleList;

	public static string ModuleDefinitionXMLFile { get; set; }

	[DataMember]
	public static ReadOnlyCollection<ModuleDefinition> List
	{
		get
		{
			if (moduleList != null)
			{
				return new ReadOnlyCollection<ModuleDefinition>(moduleList);
			}
			if (!string.IsNullOrEmpty(ModuleDefinitionXMLFile) && File.Exists(ModuleDefinitionXMLFile))
			{
				using FileStream fileStream = new FileStream(ModuleDefinitionXMLFile, FileMode.Open);
				XmlDictionaryReader xmlDictionaryReader = XmlDictionaryReader.CreateTextReader(fileStream, new XmlDictionaryReaderQuotas());
				moduleList = (List<ModuleDefinition>)new DataContractSerializer(typeof(List<ModuleDefinition>)).ReadObject(xmlDictionaryReader, verifyObjectName: true);
				xmlDictionaryReader.Close();
				fileStream.Close();
			}
			else
			{
				List<ModuleDefinition> list = new List<ModuleDefinition>();
				list.Add(new ModuleDefinition(-1, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, Resources.ModulesNotFound, "N/A", "N/A"));
				list.Add(new ModuleDefinition(0, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, Resources.InternalVci, "VBCAN02", "N/A"));
				list.Add(new ModuleDefinition(1, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList, 0, Resources.VBoxIIPro4, "VBoxPro", "N/A"));
				list.Add(new ModuleDefinition(2, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList, 4096, Resources.VBoxII, "VB2S/VB2D/VB2DCF", "VBOXII"));
				list.Add(new ModuleDefinition(2, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.IsPhantomUnit | ModuleFunction.HasGpsEngine, 0, Resources.VBIISX, "N/A", "N/A"));
				list.Add(new ModuleDefinition(3, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.Adc01, "VBADC01", "N/A"));
				list.Add(new ModuleDefinition(4, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 1168, Resources.Adc02, "VBADC02", "ADC02-TC8"));
				list.Add(new ModuleDefinition(5, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 1168, Resources.Tc8, "VBTC8", "ADC02-TC8", new SubTypeDefinition[2]
				{
					new SubTypeDefinition(48, "VBTC8"),
					new SubTypeDefinition(50, string.Format("{0} - {1}", "VBTC8", "2"), "TC8v2")
				}));
				list.Add(new ModuleDefinition(6, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.PowerCycleAfterUpdate, 4096, Resources.Dac01, "VBDAC01 OLD", "N/A"));
				list.Add(new ModuleDefinition(7, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.Dsp03, "VBDSP03", "N/A"));
				list.Add(new ModuleDefinition(8, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.SerialNumberFromList, 0, Resources.VBIIMotorSport, "VBII MotorSport", "N/A", new SubTypeDefinition[1]
				{
					new SubTypeDefinition(90)
				}));
				list.Add(new ModuleDefinition(9, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.SerialNumberFromList, 0, Resources.VB2Sps, "VB2SPS/VB2SPSD", "N/A", new SubTypeDefinition[1]
				{
					new SubTypeDefinition(88)
				}));
				list.Add(new ModuleDefinition(10, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, "(No unit)", "N/A", "N/A"));
				list.Add(new ModuleDefinition(11, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.Can01, "VBCAN01", "N/A"));
				list.Add(new ModuleDefinition(12, Processor.ATMega128, SerialBaudRate.br38400, ModuleFunction.None, 0, Resources.TcMkII, "TCMKII", "N/A"));
				list.Add(new ModuleDefinition(13, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, "(No unit)", "N/A", "N/A"));
				list.Add(new ModuleDefinition(14, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.SerialNumberFromList, 4096, Resources.VB2SpsD1, "VB2SPSD1", "N/A"));
				list.Add(new ModuleDefinition(15, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.PowerCycleAfterUpdate, 864, Resources.Adc03, "VBADC03", "N/A"));
				list.Add(new ModuleDefinition(16, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList, 4096, Resources.VB2L, "VB2L", "N/A"));
				list.Add(new ModuleDefinition(17, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.PowerCycleAfterUpdate, 0, Resources.Fim01, "VBFIM01", "N/A"));
				list.Add(new ModuleDefinition(18, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.Dsp03L, "DSP03L", "N/A"));
				list.Add(new ModuleDefinition(19, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.PowerCycleAfterUpdate, 536, Resources.Fim02, "VBFIM02", "FIM"));
				list.Add(new ModuleDefinition(20, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.Dsp03DC, "VBDSP03-DC", "N/A"));
				list.Add(new ModuleDefinition(21, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.Adc03DC, "VBADC03-DC", "N/A"));
				list.Add(new ModuleDefinition(22, Processor.PC104, SerialBaudRate.br115200, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList, 4096, Resources.VBox3, "VB3", "N/A"));
				list.Add(new ModuleDefinition(23, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 4096, Resources.Imu, "VBIMU xx", "IMU-YAW", new SubTypeDefinition[3]
				{
					new SubTypeDefinition(48, Resources.Imu),
					new SubTypeDefinition(51, string.Format("{0} {1}", Resources.Imu, "3")),
					new SubTypeDefinition(52, string.Format("{0} {1}", Resources.Imu, "4"), "IMU04")
				}));
				list.Add(new ModuleDefinition(24, Processor.PC104, SerialBaudRate.br115200, ModuleFunction.SerialNumberFromList, 4096, Resources.VBox3Sps, "VB3SPS", "N/A"));
				list.Add(new ModuleDefinition(25, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 4096, Resources.Yaw02, "VBYAW02/VBYAW03", "IMU-YAW"));
				list.Add(new ModuleDefinition(26, Processor.ATMega128, SerialBaudRate.br38400, ModuleFunction.None, 0, Resources.DriftMeter, "Drift Meter", "N/A"));
				list.Add(new ModuleDefinition(27, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.SlipAngle, "Slip Angle", "N/A"));
				list.Add(new ModuleDefinition(28, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.SlipAngleMkII, "Slip Angle MKII", "N/A"));
				list.Add(new ModuleDefinition(29, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.BaseStation, "VBBS/VBBS2/VBBS3", "N/A"));
				list.Add(new ModuleDefinition(30, Processor.ATMega128, SerialBaudRate.br38400, ModuleFunction.None, 0, Resources.VBoxManager, "VBFMAN", "N/A"));
				list.Add(new ModuleDefinition(31, Processor.ATMega128, SerialBaudRate.br38400, ModuleFunction.None, 0, Resources.Tda, "TCDIA", "N/A"));
				list.Add(new ModuleDefinition(32, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.PowerCycleAfterUpdate, 630, Resources.Fim03, "VBFIM03", "FIM"));
				list.Add(new ModuleDefinition(33, Processor.ATMega128, SerialBaudRate.br38400, ModuleFunction.None, 0, Resources.CanDisplay, "CANDISP", "N/A"));
				list.Add(new ModuleDefinition(34, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.HasGpsEngine | ModuleFunction.UsbSerial, 0, Resources.DriftMeter, "DB01", "N/A"));
				list.Add(new ModuleDefinition(35, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.PowerCycleAfterUpdate, 4096, Resources.Can02, "VBCAN02", "CAN02"));
				list.Add(new ModuleDefinition(36, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SupportsModuleMode | ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, Resources.Vbs20Sl, "VBS20SL", "N/A"));
				list.Add(new ModuleDefinition(37, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, Resources.VBoxMini, "VBM01", "N/A"));
				list.Add(new ModuleDefinition(38, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.PowerCycleAfterUpdate, 4096, Resources.Dac02, "VBDAC02 (VBDAC01 NEW)", "DAC02"));
				list.Add(new ModuleDefinition(39, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, Resources.VBIISX, "VB2SX", "VBIISX-SL"));
				list.Add(new ModuleDefinition(40, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.PowerCycleAfterUpdate, 4096, Resources.MiniInputModule, "VBMIM01", "MIM01"));
				list.Add(new ModuleDefinition(41, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SupportsModuleMode | ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, Resources.VBIISL, "VB20SL", "VBIISX-SL"));
				list.Add(new ModuleDefinition(42, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.BaseStationS, "VBBS2-S", "N/A"));
				list.Add(new ModuleDefinition(43, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, Resources.InternalSlipModule, Resources.SlipAngle, "N/A"));
				list.Add(new ModuleDefinition(44, Processor.ATMega256_1, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.Dsp032, "VBDSP03-2", "N/A"));
				list.Add(new ModuleDefinition(45, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.HasGpsEngine | ModuleFunction.UsbSerial, 0, Resources.PerMeter, "PB01", "N/A", new SubTypeDefinition[2]
				{
					new SubTypeDefinition(0, "Default"),
					new SubTypeDefinition(1, "PBT-Vx")
				}));
				list.Add(new ModuleDefinition(46, Processor.CANBootloader, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.CanBootloader, "CB1/CB1-FORD/CB6R/CB6P1", "N/A"));
				list.Add(new ModuleDefinition(47, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.HasGpsEngine, 0, Resources.VB10Sps, "VB10SPS", "N/A"));
				list.Add(new ModuleDefinition(48, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, Resources.VB2Sx10, "VB2SX10", "VBIISX-SL"));
				list.Add(new ModuleDefinition(49, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBox, "Video VBOX", "N/A"));
				list.Add(new ModuleDefinition(50, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, "VB2SX20SPS", "VB2SX20SPS", "N/A"));
				list.Add(new ModuleDefinition(51, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.HasGpsEngine, 0, "VB10SPSRA", "VB10SPSRA", "N/A"));
				list.Add(new ModuleDefinition(52, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.HasGpsEngine | ModuleFunction.UsbSerial, 0, Resources.DriftBoxSp, "DriftBoxSP", "N/A"));
				list.Add(new ModuleDefinition(53, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.HasGpsEngine | ModuleFunction.UsbSerial, 0, Resources.PerMeterSp, "PerMeterSP", "N/A"));
				list.Add(new ModuleDefinition(54, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.HasGpsEngine, 0, Resources.Vsc, "VSC", "N/A"));
				list.Add(new ModuleDefinition(55, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.UsbSerial, 0, Resources.CanLogger, "VBCANLOG01", "N/A"));
				list.Add(new ModuleDefinition(56, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.HasGpsEngine, 0, Resources.GpsOdometer, "GPS Odometer", "N/A"));
				list.Add(new ModuleDefinition(57, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.HasGpsEngine | ModuleFunction.UsbSerial, 0, Resources.PerMeter, "PB03", "N/A"));
				list.Add(new ModuleDefinition(58, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SupportsModuleMode | ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, "VB20SL3", "VB20SL3", "N/A"));
				list.Add(new ModuleDefinition(59, Processor.CANBootloader, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.CanBootloaderS, "CB1/CB1-FORD/CB6R/CB6P1 - S", "N/A"));
				list.Add(new ModuleDefinition(60, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList, 4096, "VBOX3i", "VB3i", "VBox3i", new SubTypeDefinition[5]
				{
					new SubTypeDefinition(48),
					new SubTypeDefinition(83, "VB3i S", "VB3i S"),
					new SubTypeDefinition(97, "VB3i v3", "VB3i v3"),
					new SubTypeDefinition(98, "VB3i v3", "VB3i v3"),
					new SubTypeDefinition(99, "VB3i v3 Dual antenna", "VB3i v3")
				}));
				list.Add(new ModuleDefinition(61, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, Resources.VBoxMicro, "VBMIC01", "VBoxMicro"));
				list.Add(new ModuleDefinition(62, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SupportsRacelogicCanModule | ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, "VBOXIISX-5", "VB2SX5", "VBIISX-SL"));
				list.Add(new ModuleDefinition(63, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, "ADAS", "ADAS", "N/A"));
				list.Add(new ModuleDefinition(64, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, Resources.VBoxMicroVci, "VBMIC01 C", "VBoxMicro"));
				list.Add(new ModuleDefinition(65, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.PerformanceBoxSport, "PBMIC01", "VBoxMicro"));
				list.Add(new ModuleDefinition(66, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.PerformanceBoxSportVci, "PBMIC01 C", "VBoxMicro"));
				list.Add(new ModuleDefinition(67, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.MiningTruckMfd, "VBDSP03-M", "N/A"));
				list.Add(new ModuleDefinition(68, Processor.STR71x, SerialBaudRate.br1000000, ModuleFunction.UsbSerial, 0, Resources.CanLoggerDp, "CANLOGGER_DP", "N/A"));
				list.Add(new ModuleDefinition(69, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxWatermark, "Video VBOX (watermark)", "N/A"));
				list.Add(new ModuleDefinition(70, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxSingleChannel, "Video VBOX (Non VCI)", "N/A"));
				list.Add(new ModuleDefinition(71, Processor.STR91x, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.Oled, Resources.Oled, "N/A", new SubTypeDefinition[3]
				{
					new SubTypeDefinition(byte.MaxValue, Resources.Oled),
					new SubTypeDefinition(1, Resources.OledLogger),
					new SubTypeDefinition(2, Resources.MiniOled)
				}));
				list.Add(new ModuleDefinition(72, Processor.ATMega128, SerialBaudRate.br38400, ModuleFunction.None, 0, Resources.LabSat, "Lab Sat", "N/A", new SubTypeDefinition[6]
				{
					new SubTypeDefinition(48, Resources.LabSat),
					new SubTypeDefinition(50, string.Format("{0} {1}", Resources.LabSat, "2")),
					new SubTypeDefinition(0, string.Format("{0} {1} : {2}", Resources.LabSat, "3", Resources.Generic)),
					new SubTypeDefinition(16, string.Format("{0} {1} : {2}", Resources.LabSat, "3", Resources.SingleChannel)),
					new SubTypeDefinition(32, string.Format("{0} {1} : {2}", Resources.LabSat, "3", Resources.DualChannel)),
					new SubTypeDefinition(64, string.Format("{0} {1} : {2}", Resources.LabSat, "3", Resources.TriChannel))
				}));
				list.Add(new ModuleDefinition(73, Processor.STR91x, SerialBaudRate.br115200, ModuleFunction.None, 4096, "VB05-100SPS", "VB05-100SPS", "VB05-100SPS", new SubTypeDefinition[3]
				{
					new SubTypeDefinition(byte.MaxValue, Resources.VbSpsOld),
					new SubTypeDefinition(2, Resources.VbSpsMkIIOem),
					new SubTypeDefinition(3, Resources.VbSpsMkIII)
				}));
				list.Add(new ModuleDefinition(74, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.MicroInputModule, "Micro Input Module", "N/A"));
				list.Add(new ModuleDefinition(75, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxLite, Resources.VideoVBoxLite, "N/A"));
				list.Add(new ModuleDefinition(76, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxVciWatermark, Resources.VideoVBoxVciWatermark, "N/A"));
				list.Add(new ModuleDefinition(77, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, Resources.InternalLaneDeparture, Resources.LaneDeparture, "N/A"));
				list.Add(new ModuleDefinition(78, Processor.STR91x, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.SpeedProfiler, "Speed Profiler", "N/A"));
				list.Add(new ModuleDefinition(79, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBox4Camera, Resources.VideoVBox4Camera, "N/A"));
				list.Add(new ModuleDefinition(80, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBox4CameraSingleChannel, Resources.VideoVBox4CameraSingleChannel, "N/A"));
				list.Add(new ModuleDefinition(81, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxLite2Can, Resources.VideoVBoxLite2Can, "N/A"));
				list.Add(new ModuleDefinition(82, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxLite4Can, Resources.VideoVBoxLite4Can, "N/A"));
				list.Add(new ModuleDefinition(83, Processor.STR91x, SerialBaudRate.br115200, ModuleFunction.None, 4096, "VB05-100SPS 256", "VB05-100SPS 256", "VB05-100SPS", new SubTypeDefinition[3]
				{
					new SubTypeDefinition(byte.MaxValue, Resources.VbSpsOld),
					new SubTypeDefinition(2, Resources.VbSpsMkIIOem),
					new SubTypeDefinition(3, Resources.VbSpsMkIII)
				}));
				list.Add(new ModuleDefinition(84, Processor.ATMega128, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.BaseStationPro, "VBBS PRO", "N/A"));
				list.Add(new ModuleDefinition(85, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, Resources.InternalVehicleSeparationPart1, Resources.VehicleSeparation1, "N/A"));
				list.Add(new ModuleDefinition(86, Processor.ATMega128, SerialBaudRate.br38400, ModuleFunction.None, 0, Resources.CanDisplay2, "RLCANDSP02", "N/A"));
				list.Add(new ModuleDefinition(87, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, Resources.InternalVehicleSeparationPart2, Resources.VehicleSeparation2, "N/A"));
				list.Add(new ModuleDefinition(88, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxLite8Can, Resources.VideoVBoxLite8Can, "N/A"));
				list.Add(new ModuleDefinition(89, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBox4Camera8Can, Resources.VideoVBox4Camera8Can, "N/A"));
				list.Add(new ModuleDefinition(90, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxWaterproof8Can, Resources.VideoVBoxWaterproof8Can, "N/A"));
				list.Add(new ModuleDefinition(91, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVBoxWaterproof32Can, Resources.VideoVBoxWaterproof32Can, "N/A"));
				list.Add(new ModuleDefinition(92, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, "(No unit)", "N/A", "N/A"));
				list.Add(new ModuleDefinition(93, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 16384, Resources.Vdms, Resources.Vdms, "VDMS", new SubTypeDefinition[3]
				{
					new SubTypeDefinition(170, Resources.Generic),
					new SubTypeDefinition(0, Resources.NissanVdms),
					new SubTypeDefinition(1, Resources.VdmsJapan)
				}));
				list.Add(new ModuleDefinition(94, Processor.NA, SerialBaudRate.br1000000, ModuleFunction.SerialNumberFromList, 0, Resources.HdCamera, Resources.HdCamera, "N/A", new SubTypeDefinition[2]
				{
					new SubTypeDefinition(1, Resources.Automotive),
					new SubTypeDefinition(2, Resources.Broadcast)
				}));
				list.Add(new ModuleDefinition(95, Processor.LPC, SerialBaudRate.br115200, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 4096, Resources.VBoxSport, Resources.VBoxSport, "N/A", new SubTypeDefinition[2]
				{
					new SubTypeDefinition(0, "Default"),
					new SubTypeDefinition(1, "Matts Test")
				}));
				list.Add(new ModuleDefinition(96, Processor.STR91x, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.LapTimer, Resources.LapTimer, "N/A", new SubTypeDefinition[3]
				{
					new SubTypeDefinition(byte.MaxValue, Resources.LapTimer),
					new SubTypeDefinition(1, $"{Resources.LapTimer} - Ford"),
					new SubTypeDefinition(2, Resources.MiniLapTimer)
				}));
				list.Add(new ModuleDefinition(97, Processor.LPC, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.BluetoothModule, Resources.BluetoothModule, "N/A"));
				list.Add(new ModuleDefinition(98, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, Resources.ImuAttitude, Resources.ImuAttitude, "N/A"));
				list.Add(new ModuleDefinition(99, Processor.NA, SerialBaudRate.br115200, ModuleFunction.SerialNumberFromList | ModuleFunction.UsbSerial, 0, Resources.VideoVboxHd, Resources.VideoVboxHd, "N/A", new SubTypeDefinition[8]
				{
					new SubTypeDefinition(1, "V1"),
					new SubTypeDefinition(2, "V2"),
					new SubTypeDefinition(3, "V3"),
					new SubTypeDefinition(4, "V4"),
					new SubTypeDefinition(5, "V5"),
					new SubTypeDefinition(6, "V6"),
					new SubTypeDefinition(7, "V7"),
					new SubTypeDefinition(8, "V8")
				}));
				list.Add(new ModuleDefinition(100, Processor.LPC, SerialBaudRate.br115200, ModuleFunction.None, 0, Resources.CanHub, Resources.CanHub, "N/A"));
				list.Add(new ModuleDefinition(101, Processor.NA, SerialBaudRate.br115200, ModuleFunction.IsPhantomUnit, 0, Resources.SoundMeter, Resources.SoundMeter, "N/A"));
				list.Add(new ModuleDefinition(102, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.UsbSerial | ModuleFunction.EncryptData, 0, Resources.CanGateway, Resources.CanGateway, "N/A"));
				list.Add(new ModuleDefinition(103, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.UsbSerial, 0, Resources.VboxTouch, Resources.VboxTouch, "N/A", new SubTypeDefinition[4]
				{
					new SubTypeDefinition(byte.MaxValue, Resources.VboxTouch),
					new SubTypeDefinition(1, Resources.MfdTouch),
					new SubTypeDefinition(2, Resources.RtkTouch),
					new SubTypeDefinition(3, Resources.VdmsTouch)
				}));
				list.Add(new ModuleDefinition(104, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.EncryptData | ModuleFunction.SupportsExtendedSerialNumber, 0, Resources.Vips, Resources.Vips, "N/A", new SubTypeDefinition[2]
				{
					new SubTypeDefinition(1, Resources.VipsBeacon),
					new SubTypeDefinition(129, Resources.VipsRover)
				}));
				list.Add(new ModuleDefinition(105, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.EncryptData | ModuleFunction.SupportsExtendedSerialNumber, 0, Resources.HdLite, Resources.HdLite, "N/A", new SubTypeDefinition[1]
				{
					new SubTypeDefinition(1, "Default")
				}));
				list.Add(new ModuleDefinition(106, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.UsbSerial | ModuleFunction.EncryptData, 0, Resources.BatteryPack, Resources.BatteryPack, "N/A", new SubTypeDefinition[1]
				{
					new SubTypeDefinition(1, "Default")
				}));
				list.Add(new ModuleDefinition(108, Processor.STR71x, SerialBaudRate.br115200, ModuleFunction.EncryptData | ModuleFunction.SupportsExtendedSerialNumber, 0, Resources.NtripModem, Resources.NtripModem, "N/A", new SubTypeDefinition[1]
				{
					new SubTypeDefinition(0, "Default")
				}));
				moduleList = list;
			}
			return new ReadOnlyCollection<ModuleDefinition>(moduleList);
		}
	}

	static Modules()
	{
	}
}
