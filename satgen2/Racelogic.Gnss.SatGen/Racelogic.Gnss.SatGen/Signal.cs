using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Racelogic.Gnss.Galileo;
using Racelogic.Gnss.Glonass;
using Racelogic.Maths;

namespace Racelogic.Gnss.SatGen;

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
		long num = 1L;
		int num2 = 1023000;
		int num3 = 1023000;
		int num4 = 3500000;
		int num5 = 24000000;
		double nominalSignalLeveldB = 0.0;
		NavigationDataInfo[] array2 = new NavigationDataInfo[1];
		int dataRate = 50;
		int bitRate = 50;
		array2[0] = new NavigationDataInfo(NavigationDataType.GpsL1CA, in dataRate, in bitRate);
		int centerFrequencyOffset = 0;
		array[0] = new Signal((SignalType)num, ConstellationType.Gps, FrequencyBand.GalileoE1, ModulationType.QuadratureBPSK, in num2, in num3, in num4, in num5, in nominalSignalLeveldB, array2, null, in centerFrequencyOffset);
		long num6 = 4L;
		int num7 = 10230000;
		int num8 = 10230000;
		int num9 = 20000000;
		int num10 = 24000000;
		double nominalSignalLeveldB2 = -3.0;
		NavigationDataInfo[] array3 = new NavigationDataInfo[1];
		int dataRate2 = 50;
		int bitRate2 = 50;
		array3[0] = new NavigationDataInfo(NavigationDataType.GpsL1CA, in dataRate2, in bitRate2);
		int centerFrequencyOffset2 = 0;
		array[1] = new Signal((SignalType)num6, ConstellationType.Gps, FrequencyBand.GalileoE1, ModulationType.InPhaseBPSK, in num7, in num8, in num9, in num10, in nominalSignalLeveldB2, array3, null, in centerFrequencyOffset2);
		long num11 = 8L;
		int num12 = 20460000;
		int num13 = 20460000;
		int num14 = 30000000;
		int num15 = 32000000;
		double nominalSignalLeveldB3 = -1.0;
		int centerFrequencyOffset3 = 0;
		array[2] = new Signal((SignalType)num11, ConstellationType.Gps, FrequencyBand.GalileoE1, ModulationType.SinBOC, in num12, in num13, in num14, in num15, in nominalSignalLeveldB3, null, null, in centerFrequencyOffset3);
		long num16 = 16L;
		int num17 = 1023000;
		int num18 = 1023000;
		int num19 = 3500000;
		int num20 = 24000000;
		double nominalSignalLeveldB4 = -1.5;
		NavigationDataInfo[] array4 = new NavigationDataInfo[1];
		int dataRate3 = 25;
		int bitRate3 = 50;
		array4[0] = new NavigationDataInfo(NavigationDataType.GpsL2C, in dataRate3, in bitRate3);
		int centerFrequencyOffset4 = 0;
		array[3] = new Signal((SignalType)num16, ConstellationType.Gps, FrequencyBand.GpsL2, ModulationType.QuadratureBPSK, in num17, in num18, in num19, in num20, in nominalSignalLeveldB4, array4, null, in centerFrequencyOffset4);
		long num21 = 32L;
		int num22 = 10230000;
		int num23 = 10230000;
		int num24 = 20000000;
		int num25 = 24000000;
		double nominalSignalLeveldB5 = -3.0;
		NavigationDataInfo[] array5 = new NavigationDataInfo[1];
		int dataRate4 = 50;
		int bitRate4 = 50;
		array5[0] = new NavigationDataInfo(NavigationDataType.GpsL1CA, in dataRate4, in bitRate4);
		int centerFrequencyOffset5 = 0;
		array[4] = new Signal((SignalType)num21, ConstellationType.Gps, FrequencyBand.GpsL2, ModulationType.InPhaseBPSK, in num22, in num23, in num24, in num25, in nominalSignalLeveldB5, array5, null, in centerFrequencyOffset5);
		long num26 = 64L;
		int num27 = 20460000;
		int num28 = 20460000;
		int num29 = 30000000;
		int num30 = 32000000;
		double nominalSignalLeveldB6 = -1.0;
		int centerFrequencyOffset6 = 0;
		array[5] = new Signal((SignalType)num26, ConstellationType.Gps, FrequencyBand.GpsL2, ModulationType.SinBOC, in num27, in num28, in num29, in num30, in nominalSignalLeveldB6, null, null, in centerFrequencyOffset6);
		long num31 = 128L;
		int num32 = 10230000;
		int num33 = 10230000;
		int num34 = 20000000;
		int num35 = 30000000;
		double nominalSignalLeveldB7 = 0.6;
		NavigationDataInfo[] array6 = new NavigationDataInfo[1];
		int dataRate5 = 50;
		int bitRate5 = 100;
		array6[0] = new NavigationDataInfo(NavigationDataType.GpsL5, in dataRate5, in bitRate5);
		int centerFrequencyOffset7 = 0;
		array[6] = new Signal((SignalType)num31, ConstellationType.Gps, FrequencyBand.NavicL5, ModulationType.InPhaseBPSK, in num32, in num33, in num34, in num35, in nominalSignalLeveldB7, array6, null, in centerFrequencyOffset7);
		long num36 = 256L;
		int num37 = 10230000;
		int num38 = 10230000;
		int num39 = 20000000;
		int num40 = 30000000;
		double nominalSignalLeveldB8 = 0.6;
		int centerFrequencyOffset8 = 0;
		array[7] = new Signal((SignalType)num36, ConstellationType.Gps, FrequencyBand.NavicL5, ModulationType.QuadratureBPSK, in num37, in num38, in num39, in num40, in nominalSignalLeveldB8, null, null, in centerFrequencyOffset8);
		long num41 = 512L;
		int num42 = 511000;
		int num43 = 511000;
		int num44 = 2500000;
		int num45 = 17000000;
		double nominalSignalLeveldB9 = -2.5;
		NavigationDataInfo[] array7 = new NavigationDataInfo[1];
		int dataRate6 = 50;
		int bitRate6 = 100;
		array7[0] = new NavigationDataInfo(NavigationDataType.GlonassL1OF, in dataRate6, in bitRate6);
		int[] frequencyOffsetsL = FrequencySlot.FrequencyOffsetsL1;
		int centerFrequencyOffset9 = FrequencySlot.CenterFrequencyOffsetL1;
		array[8] = new Signal((SignalType)num41, ConstellationType.Glonass, FrequencyBand.GlonassL1, ModulationType.QuadratureBPSK, in num42, in num43, in num44, in num45, in nominalSignalLeveldB9, array7, frequencyOffsetsL, in centerFrequencyOffset9);
		long num46 = 1024L;
		int num47 = 511000;
		int num48 = 511000;
		int num49 = 2500000;
		int num50 = 17000000;
		double nominalSignalLeveldB10 = -2.5;
		NavigationDataInfo[] array8 = new NavigationDataInfo[1];
		int dataRate7 = 50;
		int bitRate7 = 100;
		array8[0] = new NavigationDataInfo(NavigationDataType.GlonassL1OF, in dataRate7, in bitRate7);
		int[] frequencyOffsetsL2 = FrequencySlot.FrequencyOffsetsL2;
		int centerFrequencyOffset10 = FrequencySlot.CenterFrequencyOffsetL2;
		array[9] = new Signal((SignalType)num46, ConstellationType.Glonass, FrequencyBand.GlonassL2, ModulationType.QuadratureBPSK, in num47, in num48, in num49, in num50, in nominalSignalLeveldB10, array8, frequencyOffsetsL2, in centerFrequencyOffset10);
		long num51 = 2048L;
		int num52 = 2046000;
		int num53 = 2046000;
		int num54 = 5000000;
		int num55 = 24000000;
		double nominalSignalLeveldB11 = 1.5;
		NavigationDataInfo[] array9 = new NavigationDataInfo[2];
		int dataRate8 = 50;
		int bitRate8 = 50;
		array9[0] = new NavigationDataInfo(NavigationDataType.BeiDouD1, in dataRate8, in bitRate8);
		int dataRate9 = 500;
		int bitRate9 = 500;
		array9[1] = new NavigationDataInfo(NavigationDataType.BeiDouD2, in dataRate9, in bitRate9);
		int centerFrequencyOffset11 = 0;
		array[10] = new Signal((SignalType)num51, ConstellationType.BeiDou, FrequencyBand.BeiDouB1, ModulationType.InPhaseBPSK, in num52, in num53, in num54, in num55, in nominalSignalLeveldB11, array9, null, in centerFrequencyOffset11);
		long num56 = 8192L;
		int num57 = 2046000;
		int num58 = 2046000;
		int num59 = 5000000;
		int num60 = 24000000;
		double nominalSignalLeveldB12 = 1.5;
		NavigationDataInfo[] array10 = new NavigationDataInfo[2];
		int dataRate10 = 50;
		int bitRate10 = 50;
		array10[0] = new NavigationDataInfo(NavigationDataType.BeiDouD1, in dataRate10, in bitRate10);
		int dataRate11 = 500;
		int bitRate11 = 500;
		array10[1] = new NavigationDataInfo(NavigationDataType.BeiDouD2, in dataRate11, in bitRate11);
		int centerFrequencyOffset12 = 0;
		array[11] = new Signal((SignalType)num56, ConstellationType.BeiDou, FrequencyBand.BeiDouB2, ModulationType.InPhaseBPSK, in num57, in num58, in num59, in num60, in nominalSignalLeveldB12, array10, null, in centerFrequencyOffset12);
		long num61 = 16384L;
		int num62 = 10230000;
		int num63 = 10230000;
		int num64 = 20000000;
		int num65 = 24000000;
		double nominalSignalLeveldB13 = 1.5;
		NavigationDataInfo[] array11 = new NavigationDataInfo[2];
		int dataRate12 = 50;
		int bitRate12 = 50;
		array11[0] = new NavigationDataInfo(NavigationDataType.BeiDouD1, in dataRate12, in bitRate12);
		int dataRate13 = 500;
		int bitRate13 = 500;
		array11[1] = new NavigationDataInfo(NavigationDataType.BeiDouD2, in dataRate13, in bitRate13);
		int centerFrequencyOffset13 = 0;
		array[12] = new Signal((SignalType)num61, ConstellationType.BeiDou, FrequencyBand.BeiDouB3, ModulationType.InPhaseBPSK, in num62, in num63, in num64, in num65, in nominalSignalLeveldB13, array11, null, in centerFrequencyOffset13);
		long num66 = 32768L;
		int num67 = 1023000;
		int num68 = 12276000;
		int num69 = 10000000;
		int num70 = 25000000;
		double nominalSignalLeveldB14 = 1.5 - CodeE1BC.SignalLeveldB;
		NavigationDataInfo[] array12 = new NavigationDataInfo[1];
		int dataRate14 = 125;
		int bitRate14 = 250;
		array12[0] = new NavigationDataInfo(NavigationDataType.GalileoE1B, in dataRate14, in bitRate14);
		int centerFrequencyOffset14 = 0;
		array[13] = new Signal((SignalType)num66, ConstellationType.Galileo, FrequencyBand.GalileoE1, ModulationType.InPhaseBPSK, in num67, in num68, in num69, in num70, in nominalSignalLeveldB14, array12, null, in centerFrequencyOffset14);
		long num71 = 65536L;
		int num72 = 10230000;
		int num73 = 10230000;
		int num74 = 20000000;
		int num75 = 21000000;
		double nominalSignalLeveldB15 = 3.5;
		NavigationDataInfo[] array13 = new NavigationDataInfo[1];
		int dataRate15 = 25;
		int bitRate15 = 50;
		array13[0] = new NavigationDataInfo(NavigationDataType.GalileoE5a, in dataRate15, in bitRate15);
		int centerFrequencyOffset15 = 0;
		array[14] = new Signal((SignalType)num71, ConstellationType.Galileo, FrequencyBand.NavicL5, ModulationType.InPhaseBPSK, in num72, in num73, in num74, in num75, in nominalSignalLeveldB15, array13, null, in centerFrequencyOffset15);
		long num76 = 131072L;
		int num77 = 10230000;
		int num78 = 10230000;
		int num79 = 20000000;
		int num80 = 21000000;
		double nominalSignalLeveldB16 = 3.5;
		int centerFrequencyOffset16 = 0;
		array[15] = new Signal((SignalType)num76, ConstellationType.Galileo, FrequencyBand.NavicL5, ModulationType.QuadratureBPSK, in num77, in num78, in num79, in num80, in nominalSignalLeveldB16, null, null, in centerFrequencyOffset16);
		long num81 = 262144L;
		int num82 = 10230000;
		int num83 = 10230000;
		int num84 = 20000000;
		int num85 = 21000000;
		double nominalSignalLeveldB17 = 3.5;
		NavigationDataInfo[] array14 = new NavigationDataInfo[1];
		int dataRate16 = 125;
		int bitRate16 = 250;
		array14[0] = new NavigationDataInfo(NavigationDataType.GalileoE5b, in dataRate16, in bitRate16);
		int centerFrequencyOffset17 = 0;
		array[16] = new Signal((SignalType)num81, ConstellationType.Galileo, FrequencyBand.BeiDouB2, ModulationType.InPhaseBPSK, in num82, in num83, in num84, in num85, in nominalSignalLeveldB17, array14, null, in centerFrequencyOffset17);
		long num86 = 524288L;
		int num87 = 10230000;
		int num88 = 10230000;
		int num89 = 20000000;
		int num90 = 21000000;
		double nominalSignalLeveldB18 = 3.5;
		int centerFrequencyOffset18 = 0;
		array[17] = new Signal((SignalType)num86, ConstellationType.Galileo, FrequencyBand.BeiDouB2, ModulationType.QuadratureBPSK, in num87, in num88, in num89, in num90, in nominalSignalLeveldB18, null, null, in centerFrequencyOffset18);
		long num91 = 4194304L;
		int num92 = 5115000;
		int num93 = 5115000;
		int num94 = 12000000;
		int num95 = 41000000;
		double nominalSignalLeveldB19 = 3.5 - CodeE6BC.SignalLeveldB;
		NavigationDataInfo[] array15 = new NavigationDataInfo[1];
		int dataRate17 = 500;
		int bitRate17 = 1000;
		array15[0] = new NavigationDataInfo(NavigationDataType.GalileoE6B, in dataRate17, in bitRate17);
		int centerFrequencyOffset19 = 0;
		array[18] = new Signal((SignalType)num91, ConstellationType.Galileo, FrequencyBand.GalileoE6, ModulationType.InPhaseBPSK, in num92, in num93, in num94, in num95, in nominalSignalLeveldB19, array15, null, in centerFrequencyOffset19);
		long num96 = 8388608L;
		int num97 = 1023000;
		int num98 = 1023000;
		int num99 = 3500000;
		int num100 = 24000000;
		double nominalSignalLeveldB20 = -0.5;
		NavigationDataInfo[] array16 = new NavigationDataInfo[1];
		int dataRate18 = 25;
		int bitRate18 = 50;
		array16[0] = new NavigationDataInfo(NavigationDataType.NavicSPS, in dataRate18, in bitRate18);
		int centerFrequencyOffset20 = 0;
		array[19] = new Signal((SignalType)num96, ConstellationType.Navic, FrequencyBand.NavicL5, ModulationType.InPhaseBPSK, in num97, in num98, in num99, in num100, in nominalSignalLeveldB20, array16, null, in centerFrequencyOffset20);
		long num101 = 16777216L;
		int num102 = 1023000;
		int num103 = 1023000;
		int num104 = 3500000;
		int num105 = 16500000;
		double nominalSignalLeveldB21 = -3.8;
		NavigationDataInfo[] array17 = new NavigationDataInfo[1];
		int dataRate19 = 25;
		int bitRate19 = 50;
		array17[0] = new NavigationDataInfo(NavigationDataType.NavicSPS, in dataRate19, in bitRate19);
		int centerFrequencyOffset21 = 0;
		array[20] = new Signal((SignalType)num101, ConstellationType.Navic, FrequencyBand.NavicS, ModulationType.InPhaseBPSK, in num102, in num103, in num104, in num105, in nominalSignalLeveldB21, array17, null, in centerFrequencyOffset21);
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
