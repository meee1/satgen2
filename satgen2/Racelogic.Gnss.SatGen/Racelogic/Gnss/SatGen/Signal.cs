using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Racelogic.Gnss.Galileo;
using Racelogic.Gnss.Glonass;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen
{
	[DebuggerDisplay("{signalType}  Freq={NominalFrequency}")]
	public sealed class Signal
	{
		private readonly SignalType signalType;

		private readonly ModulationType modulationType;

		private readonly ConstellationType constellationType;

		private readonly FrequencyBand frequencyBand;

		private readonly uint nominalFrequency;

		private readonly uint centerFrequency;

		private readonly int minBandwidth;

		private readonly int maxBandwidth;

		private readonly int chippingRate;

		private readonly int modulationRate;

		private readonly double nominalSignalLevel;

		private readonly IEnumerable<NavigationDataInfo> navigationDataInfos;

		private readonly int[]? slotFrequencies;

		private bool isEnabled = true;

		private const int gpsBaseRate = 1023000;

		private const int glonassBaseRate = 511000;

		private static readonly int frequencyBandCount = Enum.GetValues(typeof(FrequencyBand)).Length;

		private static readonly SignalType[] individualSignalTypes = (from SignalType st in Enum.GetValues(typeof(SignalType))
			where st != SignalType.None && (st & (st - 1)) == 0
			select st).ToArray();

		private static readonly int individualSignalCount = individualSignalTypes.Length;

		private static readonly Signal[] allSignals;

		private static readonly ReadOnlyDictionary<ConstellationType, IReadOnlyList<SignalType>> userSignalTypes;

		public static IReadOnlyList<Signal> AllSignals
		{
			[DebuggerStepThrough]
			get
			{
				return allSignals;
			}
		}

		public static ReadOnlyDictionary<ConstellationType, IReadOnlyList<SignalType>> UserSignalTypes
		{
			[DebuggerStepThrough]
			get
			{
				return userSignalTypes;
			}
		}

		public static IReadOnlyList<SignalType> IndividualSignalTypes
		{
			[DebuggerStepThrough]
			get
			{
				return individualSignalTypes;
			}
		}

		public ConstellationType ConstellationType
		{
			[DebuggerStepThrough]
			get
			{
				return constellationType;
			}
		}

		public FrequencyBand FrequencyBand
		{
			[DebuggerStepThrough]
			get
			{
				return frequencyBand;
			}
		}

		public SignalType SignalType
		{
			[DebuggerStepThrough]
			get
			{
				return signalType;
			}
		}

		public ModulationType ModulationType
		{
			[DebuggerStepThrough]
			get
			{
				return modulationType;
			}
		}

		public uint NominalFrequency
		{
			[DebuggerStepThrough]
			get
			{
				return nominalFrequency;
			}
		}

		public uint CenterFrequency
		{
			[DebuggerStepThrough]
			get
			{
				return centerFrequency;
			}
		}

		public int MinBandwidth
		{
			[DebuggerStepThrough]
			get
			{
				return minBandwidth;
			}
		}

		public int MaxBandwidth
		{
			[DebuggerStepThrough]
			get
			{
				return maxBandwidth;
			}
		}

		public int ChippingRate
		{
			[DebuggerStepThrough]
			get
			{
				return chippingRate;
			}
		}

		public int ModulationRate
		{
			[DebuggerStepThrough]
			get
			{
				return modulationRate;
			}
		}

		public double NominalSignalLevel
		{
			[DebuggerStepThrough]
			get
			{
				return nominalSignalLevel;
			}
		}

		public IReadOnlyList<int>? SlotFrequencies
		{
			[DebuggerStepThrough]
			get
			{
				return slotFrequencies;
			}
		}

		public bool IsEnabled
		{
			[DebuggerStepThrough]
			get
			{
				return isEnabled;
			}
			[DebuggerStepThrough]
			set
			{
				isEnabled = value;
			}
		}

		internal IEnumerable<NavigationDataInfo> NavigationDataInfos
		{
			[DebuggerStepThrough]
			get
			{
				return navigationDataInfos;
			}
		}

		internal Signal(SignalType signalType, ConstellationType constellationType, FrequencyBand frequencyBand, ModulationType modulationType, in int chippingRate, in int modulationRate, in int minBandwidth, in int maxBandwidth, in double nominalSignalLeveldB = 0.0, IEnumerable<NavigationDataInfo>? navigationDataInfos = null, int[]? slotFrequencies = null, in int centerFrequencyOffset = 0)
		{
			this.signalType = signalType;
			this.modulationType = modulationType;
			this.constellationType = constellationType;
			this.frequencyBand = frequencyBand;
			nominalFrequency = (uint)frequencyBand;
			this.chippingRate = chippingRate;
			this.modulationRate = modulationRate;
			this.minBandwidth = minBandwidth;
			this.maxBandwidth = maxBandwidth;
			nominalSignalLevel = nominalSignalLeveldB.GainToLevel();
			this.navigationDataInfos = navigationDataInfos ?? Array.Empty<NavigationDataInfo>();
			this.slotFrequencies = slotFrequencies;
			centerFrequency = nominalFrequency.SafeAdd(centerFrequencyOffset);
		}

		internal static Signal[] GetSignals(SignalType signalTypes)
		{
			return GetSignals(new SignalType[1] { signalTypes });
		}

		internal static Signal[] GetSignals(IEnumerable<SignalType> signalTypes)
		{
			IReadOnlyList<SignalType> readOnlyList = GetIndividualSignalTypes(signalTypes);
			Signal[] array = new Signal[readOnlyList.Count];
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				SignalType signalType = readOnlyList[i];
				Signal[] array2 = allSignals;
				foreach (Signal signal in array2)
				{
					if (signal.SignalType == signalType)
					{
						array[i] = signal;
						break;
					}
				}
			}
			return array;
		}

		internal static Signal[] GetSignals(SignalType[] signalTypes)
		{
			IReadOnlyList<SignalType> readOnlyList = GetIndividualSignalTypes(signalTypes);
			Signal[] array = new Signal[readOnlyList.Count];
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				SignalType signalType = readOnlyList[i];
				Signal[] array2 = allSignals;
				foreach (Signal signal in array2)
				{
					if (signal.SignalType == signalType)
					{
						array[i] = signal;
						break;
					}
				}
			}
			return array;
		}

		public static IReadOnlyList<SignalType> GetIndividualSignalTypes(SignalType signalType)
		{
			List<SignalType> list = new List<SignalType>(individualSignalCount);
			SignalType[] array = individualSignalTypes;
			foreach (SignalType signalType2 in array)
			{
				if ((signalType & signalType2) != SignalType.None)
				{
					list.Add(signalType2);
				}
			}
			return list;
		}

		public static IReadOnlyList<SignalType> GetIndividualSignalTypes(IEnumerable<SignalType> signalTypes)
		{
			List<SignalType> list = new List<SignalType>(individualSignalCount);
			foreach (SignalType signalType2 in signalTypes)
			{
				SignalType[] array = individualSignalTypes;
				foreach (SignalType signalType in array)
				{
					if ((signalType2 & signalType) != SignalType.None)
					{
						list.Add(signalType);
					}
				}
			}
			return list;
		}

		internal static IReadOnlyList<SignalType> GetIndividualSignalTypes(SignalType[] signalTypes)
		{
			List<SignalType> list = new List<SignalType>(individualSignalCount);
			for (int i = 0; i < signalTypes.Length; i++)
			{
				ulong num = (ulong)signalTypes[i];
				SignalType[] array = individualSignalTypes;
				foreach (SignalType signalType in array)
				{
					if ((num & (ulong)signalType) != 0L)
					{
						list.Add(signalType);
					}
				}
			}
			return list;
		}

		public static SignalType GetCombinedSignalTypes(IEnumerable<SignalType> signalTypes)
		{
			SignalType signalType = SignalType.None;
			foreach (SignalType signalType2 in signalTypes)
			{
				signalType |= signalType2;
			}
			return signalType;
		}

		internal static FrequencyBand[] GetFrequencyBands(IEnumerable<Signal> signals)
		{
			List<FrequencyBand> list = new List<FrequencyBand>(frequencyBandCount);
			foreach (Signal item in signals.Where((Signal s) => s != null))
			{
				FrequencyBand frequencyBand = item.FrequencyBand;
				bool flag = false;
				foreach (FrequencyBand item2 in list)
				{
					if (item2 == frequencyBand)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(frequencyBand);
				}
			}
			return list.ToArray();
		}

		static Signal()
		{
			Signal[] array = new Signal[21];
			long num94 = 1L;
			int num23 = 1023000;
			int num32 = 1023000;
			int num41 = 3500000;
			int num50 = 24000000;
			double nominalSignalLeveldB = 0.0;
			NavigationDataInfo[] array2 = new NavigationDataInfo[1];
			int dataRate = 50;
			int bitRate = 50;
			array2[0] = new NavigationDataInfo(NavigationDataType.GpsL1CA, in dataRate, in bitRate);
			int centerFrequencyOffset = 0;
			array[0] = new Signal((SignalType)num94, ConstellationType.Gps, FrequencyBand.GalileoE1, ModulationType.QuadratureBPSK, in num23, in num32, in num41, in num50, in nominalSignalLeveldB, array2, null, in centerFrequencyOffset);
			long num95 = 4L;
			int num67 = 10230000;
			int num76 = 10230000;
			int num85 = 20000000;
			int num10 = 24000000;
			double nominalSignalLeveldB12 = -3.0;
			NavigationDataInfo[] array3 = new NavigationDataInfo[1];
			int dataRate12 = 50;
			int bitRate12 = 50;
			array3[0] = new NavigationDataInfo(NavigationDataType.GpsL1CA, in dataRate12, in bitRate12);
			int centerFrequencyOffset12 = 0;
			array[1] = new Signal((SignalType)num95, ConstellationType.Gps, FrequencyBand.GalileoE1, ModulationType.InPhaseBPSK, in num67, in num76, in num85, in num10, in nominalSignalLeveldB12, array3, null, in centerFrequencyOffset12);
			long num96 = 8L;
			int num16 = 20460000;
			int num17 = 20460000;
			int num18 = 30000000;
			int num19 = 32000000;
			double nominalSignalLeveldB15 = -1.0;
			int centerFrequencyOffset15 = 0;
			array[2] = new Signal((SignalType)num96, ConstellationType.Gps, FrequencyBand.GalileoE1, ModulationType.SinBOC, in num16, in num17, in num18, in num19, in nominalSignalLeveldB15, null, null, in centerFrequencyOffset15);
			long num97 = 16L;
			int num20 = 1023000;
			int num21 = 1023000;
			int num22 = 3500000;
			int num24 = 24000000;
			double nominalSignalLeveldB16 = -1.5;
			NavigationDataInfo[] array4 = new NavigationDataInfo[1];
			int dataRate13 = 25;
			int bitRate13 = 50;
			array4[0] = new NavigationDataInfo(NavigationDataType.GpsL2C, in dataRate13, in bitRate13);
			int centerFrequencyOffset16 = 0;
			array[3] = new Signal((SignalType)num97, ConstellationType.Gps, FrequencyBand.GpsL2, ModulationType.QuadratureBPSK, in num20, in num21, in num22, in num24, in nominalSignalLeveldB16, array4, null, in centerFrequencyOffset16);
			long num98 = 32L;
			int num25 = 10230000;
			int num26 = 10230000;
			int num27 = 20000000;
			int num28 = 24000000;
			double nominalSignalLeveldB17 = -3.0;
			NavigationDataInfo[] array5 = new NavigationDataInfo[1];
			int dataRate14 = 50;
			int bitRate14 = 50;
			array5[0] = new NavigationDataInfo(NavigationDataType.GpsL1CA, in dataRate14, in bitRate14);
			int centerFrequencyOffset17 = 0;
			array[4] = new Signal((SignalType)num98, ConstellationType.Gps, FrequencyBand.GpsL2, ModulationType.InPhaseBPSK, in num25, in num26, in num27, in num28, in nominalSignalLeveldB17, array5, null, in centerFrequencyOffset17);
			long num99 = 64L;
			int num29 = 20460000;
			int num30 = 20460000;
			int num31 = 30000000;
			int num33 = 32000000;
			double nominalSignalLeveldB18 = -1.0;
			int centerFrequencyOffset18 = 0;
			array[5] = new Signal((SignalType)num99, ConstellationType.Gps, FrequencyBand.GpsL2, ModulationType.SinBOC, in num29, in num30, in num31, in num33, in nominalSignalLeveldB18, null, null, in centerFrequencyOffset18);
			long num100 = 128L;
			int num34 = 10230000;
			int num35 = 10230000;
			int num36 = 20000000;
			int num37 = 30000000;
			double nominalSignalLeveldB19 = 0.6;
			NavigationDataInfo[] array6 = new NavigationDataInfo[1];
			int dataRate15 = 50;
			int bitRate15 = 100;
			array6[0] = new NavigationDataInfo(NavigationDataType.GpsL5, in dataRate15, in bitRate15);
			int centerFrequencyOffset19 = 0;
			array[6] = new Signal((SignalType)num100, ConstellationType.Gps, FrequencyBand.NavicL5, ModulationType.InPhaseBPSK, in num34, in num35, in num36, in num37, in nominalSignalLeveldB19, array6, null, in centerFrequencyOffset19);
			long num101 = 256L;
			int num38 = 10230000;
			int num39 = 10230000;
			int num40 = 20000000;
			int num42 = 30000000;
			double nominalSignalLeveldB20 = 0.6;
			int centerFrequencyOffset20 = 0;
			array[7] = new Signal((SignalType)num101, ConstellationType.Gps, FrequencyBand.NavicL5, ModulationType.QuadratureBPSK, in num38, in num39, in num40, in num42, in nominalSignalLeveldB20, null, null, in centerFrequencyOffset20);
			long num102 = 512L;
			int num43 = 511000;
			int num44 = 511000;
			int num45 = 2500000;
			int num46 = 17000000;
			double nominalSignalLeveldB21 = -2.5;
			NavigationDataInfo[] array7 = new NavigationDataInfo[1];
			int dataRate16 = 50;
			int bitRate16 = 100;
			array7[0] = new NavigationDataInfo(NavigationDataType.GlonassL1OF, in dataRate16, in bitRate16);
			int[] frequencyOffsetsL = FrequencySlot.FrequencyOffsetsL1;
			int centerFrequencyOffset21 = FrequencySlot.CenterFrequencyOffsetL1;
			array[8] = new Signal((SignalType)num102, ConstellationType.Glonass, FrequencyBand.GlonassL1, ModulationType.QuadratureBPSK, in num43, in num44, in num45, in num46, in nominalSignalLeveldB21, array7, frequencyOffsetsL, in centerFrequencyOffset21);
			long num103 = 1024L;
			int num47 = 511000;
			int num48 = 511000;
			int num49 = 2500000;
			int num51 = 17000000;
			double nominalSignalLeveldB2 = -2.5;
			NavigationDataInfo[] array8 = new NavigationDataInfo[1];
			int dataRate17 = 50;
			int bitRate17 = 100;
			array8[0] = new NavigationDataInfo(NavigationDataType.GlonassL1OF, in dataRate17, in bitRate17);
			int[] frequencyOffsetsL2 = FrequencySlot.FrequencyOffsetsL2;
			int centerFrequencyOffset2 = FrequencySlot.CenterFrequencyOffsetL2;
			array[9] = new Signal((SignalType)num103, ConstellationType.Glonass, FrequencyBand.GlonassL2, ModulationType.QuadratureBPSK, in num47, in num48, in num49, in num51, in nominalSignalLeveldB2, array8, frequencyOffsetsL2, in centerFrequencyOffset2);
			long num104 = 2048L;
			int num52 = 2046000;
			int num53 = 2046000;
			int num54 = 5000000;
			int num55 = 24000000;
			double nominalSignalLeveldB3 = 1.5;
			NavigationDataInfo[] array9 = new NavigationDataInfo[2];
			int dataRate18 = 50;
			int bitRate18 = 50;
			array9[0] = new NavigationDataInfo(NavigationDataType.BeiDouD1, in dataRate18, in bitRate18);
			int dataRate19 = 500;
			int bitRate19 = 500;
			array9[1] = new NavigationDataInfo(NavigationDataType.BeiDouD2, in dataRate19, in bitRate19);
			int centerFrequencyOffset3 = 0;
			array[10] = new Signal((SignalType)num104, ConstellationType.BeiDou, FrequencyBand.BeiDouB1, ModulationType.InPhaseBPSK, in num52, in num53, in num54, in num55, in nominalSignalLeveldB3, array9, null, in centerFrequencyOffset3);
			long num105 = 8192L;
			int num56 = 2046000;
			int num57 = 2046000;
			int num58 = 5000000;
			int num59 = 24000000;
			double nominalSignalLeveldB4 = 1.5;
			NavigationDataInfo[] array10 = new NavigationDataInfo[2];
			int dataRate2 = 50;
			int bitRate2 = 50;
			array10[0] = new NavigationDataInfo(NavigationDataType.BeiDouD1, in dataRate2, in bitRate2);
			int dataRate3 = 500;
			int bitRate3 = 500;
			array10[1] = new NavigationDataInfo(NavigationDataType.BeiDouD2, in dataRate3, in bitRate3);
			int centerFrequencyOffset4 = 0;
			array[11] = new Signal((SignalType)num105, ConstellationType.BeiDou, FrequencyBand.BeiDouB2, ModulationType.InPhaseBPSK, in num56, in num57, in num58, in num59, in nominalSignalLeveldB4, array10, null, in centerFrequencyOffset4);
			long num106 = 16384L;
			int num60 = 10230000;
			int num61 = 10230000;
			int num62 = 20000000;
			int num63 = 24000000;
			double nominalSignalLeveldB5 = 1.5;
			NavigationDataInfo[] array11 = new NavigationDataInfo[2];
			int dataRate4 = 50;
			int bitRate4 = 50;
			array11[0] = new NavigationDataInfo(NavigationDataType.BeiDouD1, in dataRate4, in bitRate4);
			int dataRate5 = 500;
			int bitRate5 = 500;
			array11[1] = new NavigationDataInfo(NavigationDataType.BeiDouD2, in dataRate5, in bitRate5);
			int centerFrequencyOffset5 = 0;
			array[12] = new Signal((SignalType)num106, ConstellationType.BeiDou, FrequencyBand.BeiDouB3, ModulationType.InPhaseBPSK, in num60, in num61, in num62, in num63, in nominalSignalLeveldB5, array11, null, in centerFrequencyOffset5);
			long num107 = 32768L;
			int num64 = 1023000;
			int num65 = 12276000;
			int num66 = 10000000;
			int num68 = 25000000;
			double nominalSignalLeveldB6 = 1.5 - CodeE1BC.SignalLeveldB;
			NavigationDataInfo[] array12 = new NavigationDataInfo[1];
			int dataRate6 = 125;
			int bitRate6 = 250;
			array12[0] = new NavigationDataInfo(NavigationDataType.GalileoE1B, in dataRate6, in bitRate6);
			int centerFrequencyOffset6 = 0;
			array[13] = new Signal((SignalType)num107, ConstellationType.Galileo, FrequencyBand.GalileoE1, ModulationType.InPhaseBPSK, in num64, in num65, in num66, in num68, in nominalSignalLeveldB6, array12, null, in centerFrequencyOffset6);
			long num108 = 65536L;
			int num69 = 10230000;
			int num70 = 10230000;
			int num71 = 20000000;
			int num72 = 21000000;
			double nominalSignalLeveldB7 = 3.5;
			NavigationDataInfo[] array13 = new NavigationDataInfo[1];
			int dataRate7 = 25;
			int bitRate7 = 50;
			array13[0] = new NavigationDataInfo(NavigationDataType.GalileoE5a, in dataRate7, in bitRate7);
			int centerFrequencyOffset7 = 0;
			array[14] = new Signal((SignalType)num108, ConstellationType.Galileo, FrequencyBand.NavicL5, ModulationType.InPhaseBPSK, in num69, in num70, in num71, in num72, in nominalSignalLeveldB7, array13, null, in centerFrequencyOffset7);
			long num109 = 131072L;
			int num73 = 10230000;
			int num74 = 10230000;
			int num75 = 20000000;
			int num77 = 21000000;
			double nominalSignalLeveldB8 = 3.5;
			int centerFrequencyOffset8 = 0;
			array[15] = new Signal((SignalType)num109, ConstellationType.Galileo, FrequencyBand.NavicL5, ModulationType.QuadratureBPSK, in num73, in num74, in num75, in num77, in nominalSignalLeveldB8, null, null, in centerFrequencyOffset8);
			long num110 = 262144L;
			int num78 = 10230000;
			int num79 = 10230000;
			int num80 = 20000000;
			int num81 = 21000000;
			double nominalSignalLeveldB9 = 3.5;
			NavigationDataInfo[] array14 = new NavigationDataInfo[1];
			int dataRate8 = 125;
			int bitRate8 = 250;
			array14[0] = new NavigationDataInfo(NavigationDataType.GalileoE5b, in dataRate8, in bitRate8);
			int centerFrequencyOffset9 = 0;
			array[16] = new Signal((SignalType)num110, ConstellationType.Galileo, FrequencyBand.BeiDouB2, ModulationType.InPhaseBPSK, in num78, in num79, in num80, in num81, in nominalSignalLeveldB9, array14, null, in centerFrequencyOffset9);
			long num111 = 524288L;
			int num82 = 10230000;
			int num83 = 10230000;
			int num84 = 20000000;
			int num86 = 21000000;
			double nominalSignalLeveldB10 = 3.5;
			int centerFrequencyOffset10 = 0;
			array[17] = new Signal((SignalType)num111, ConstellationType.Galileo, FrequencyBand.BeiDouB2, ModulationType.QuadratureBPSK, in num82, in num83, in num84, in num86, in nominalSignalLeveldB10, null, null, in centerFrequencyOffset10);
			long num112 = 4194304L;
			int num87 = 5115000;
			int num88 = 5115000;
			int num89 = 12000000;
			int num90 = 41000000;
			double nominalSignalLeveldB11 = 3.5 - CodeE6BC.SignalLeveldB;
			NavigationDataInfo[] array15 = new NavigationDataInfo[1];
			int dataRate9 = 500;
			int bitRate9 = 1000;
			array15[0] = new NavigationDataInfo(NavigationDataType.GalileoE6B, in dataRate9, in bitRate9);
			int centerFrequencyOffset11 = 0;
			array[18] = new Signal((SignalType)num112, ConstellationType.Galileo, FrequencyBand.GalileoE6, ModulationType.InPhaseBPSK, in num87, in num88, in num89, in num90, in nominalSignalLeveldB11, array15, null, in centerFrequencyOffset11);
			long num113 = 8388608L;
			int num91 = 1023000;
			int num92 = 1023000;
			int num93 = 3500000;
			int num11 = 24000000;
			double nominalSignalLeveldB13 = -0.5;
			NavigationDataInfo[] array16 = new NavigationDataInfo[1];
			int dataRate10 = 25;
			int bitRate10 = 50;
			array16[0] = new NavigationDataInfo(NavigationDataType.NavicSPS, in dataRate10, in bitRate10);
			int centerFrequencyOffset13 = 0;
			array[19] = new Signal((SignalType)num113, ConstellationType.Navic, FrequencyBand.NavicL5, ModulationType.InPhaseBPSK, in num91, in num92, in num93, in num11, in nominalSignalLeveldB13, array16, null, in centerFrequencyOffset13);
			long num114 = 16777216L;
			int num12 = 1023000;
			int num13 = 1023000;
			int num14 = 3500000;
			int num15 = 16500000;
			double nominalSignalLeveldB14 = -3.8;
			NavigationDataInfo[] array17 = new NavigationDataInfo[1];
			int dataRate11 = 25;
			int bitRate11 = 50;
			array17[0] = new NavigationDataInfo(NavigationDataType.NavicSPS, in dataRate11, in bitRate11);
			int centerFrequencyOffset14 = 0;
			array[20] = new Signal((SignalType)num114, ConstellationType.Navic, FrequencyBand.NavicS, ModulationType.InPhaseBPSK, in num12, in num13, in num14, in num15, in nominalSignalLeveldB14, array17, null, in centerFrequencyOffset14);
			allSignals = array;
			userSignalTypes = new ReadOnlyDictionary<ConstellationType, IReadOnlyList<SignalType>>(new Dictionary<ConstellationType, IReadOnlyList<SignalType>>
			{
				{
					ConstellationType.Gps,
					new SignalType[7]
					{
						SignalType.GpsL1CA,
						SignalType.GpsL1P,
						SignalType.GpsL1M,
						SignalType.GpsL2C,
						SignalType.GpsL2P,
						SignalType.GpsL2M,
						SignalType.GpsL5
					}
				},
				{
					ConstellationType.Glonass,
					new SignalType[2]
					{
						SignalType.GlonassL1OF,
						SignalType.GlonassL2OF
					}
				},
				{
					ConstellationType.BeiDou,
					new SignalType[3]
					{
						SignalType.BeiDouB1I,
						SignalType.BeiDouB2I,
						SignalType.BeiDouB3I
					}
				},
				{
					ConstellationType.Galileo,
					new SignalType[4]
					{
						SignalType.GalileoE1BC,
						SignalType.GalileoE5a,
						SignalType.GalileoE5b,
						SignalType.GalileoE6BC
					}
				},
				{
					ConstellationType.Navic,
					new SignalType[2]
					{
						SignalType.NavicL5SPS,
						SignalType.NavicSSPS
					}
				}
			});
		}
	}
}
