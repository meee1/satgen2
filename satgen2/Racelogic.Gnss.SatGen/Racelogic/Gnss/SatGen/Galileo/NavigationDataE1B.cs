using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen.Galileo
{
	internal sealed class NavigationDataE1B : NavigationDataINav
	{
		private static readonly NavigationDataInfo dataInfo = Signal.AllSignals.SelectMany((Signal s) => s.NavigationDataInfos).First((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GalileoE1B);

		private static readonly SignalType signalType = Signal.AllSignals.First((Signal s) => s.NavigationDataInfos.Any((NavigationDataInfo ndi) => ndi.NavigationDataType == NavigationDataType.GalileoE1B)).SignalType;

		private static readonly WordType[] pageFormats = new WordType[15]
		{
			WordType.Word2,
			WordType.Word4,
			WordType.Word6,
			WordType.Word79,
			WordType.Word810,
			WordType.Spare,
			WordType.Spare,
			WordType.Spare,
			WordType.Spare,
			WordType.Spare,
			WordType.Word1,
			WordType.Word3,
			WordType.Word5,
			WordType.Spare,
			WordType.Spare
		};

		private const int SarMessagePartLength = 20;

		private static readonly byte[] emptySarBits;

		private static readonly byte[] acknowledgementServiceType1ParametersFirst2Bits;

		public static NavigationDataInfo Info
		{
			[DebuggerStepThrough]
			get
			{
				return dataInfo;
			}
		}

		protected override SignalType SignalType
		{
			[DebuggerStepThrough]
			get
			{
				return signalType;
			}
		}

		public override int BitRate
		{
			[DebuggerStepThrough]
			get
			{
				return dataInfo.BitRate;
			}
		}

		protected override WordType[] PageFormats
		{
			[DebuggerStepThrough]
			get
			{
				return pageFormats;
			}
		}

		public NavigationDataE1B(in int satIndex, Almanac almanac, in Range<GnssTime, GnssTimeSpan> interval, IEnumerable<Signal> simulatedSignals)
			: base(in satIndex, almanac, in interval, simulatedSignals)
		{
		}

		protected override PagePartParams GetFirstPagePartParameters(int pageIndex, int subframeIndex, in GnssTime pageSlotTime)
		{
			if (--pageIndex < 0)
			{
				pageIndex += 15;
				if (--subframeIndex < 0)
				{
					subframeIndex += 24;
				}
			}
			GnssTime pageTime = pageSlotTime - GnssTimeSpan.FromSeconds(1);
			int pagePartIndex = 1;
			return new PagePartParams(in pagePartIndex, in pageIndex, in subframeIndex, in pageTime);
		}

		protected override PagePartParams GetSecondPagePartParameters(in int pageIndex, in int subframeIndex, in GnssTime pageSlotTime)
		{
			GnssTime pageTime = pageSlotTime + GnssTimeSpan.FromSeconds(1);
			int pagePartIndex = 0;
			return new PagePartParams(in pagePartIndex, in pageIndex, in subframeIndex, in pageTime);
		}

		protected override int GetFirstAlmanacIndex(in int subframeIndex)
		{
			int num = 18 + (subframeIndex >> 1) * 3;
			if (num > 33)
			{
				num -= 36;
			}
			return num;
		}

		protected override IEnumerable<byte> GetReservedBits1(PagePartParams pageParams)
		{
			IEnumerable<byte> second = emptySarBits;
			return NavigationData.ZeroBits.Take(40).Concat(second).Concat(NavigationData.ZeroBits.Take(2));
		}

		private IEnumerable<byte> GetSarBitsForPage(PagePartParams pageParams)
		{
			int num = (pageParams.SubframeIndex * 15 + pageParams.PageIndex) % 4;
			int count = num * 20;
			byte b = (byte)((num == 0) ? 1 : 0);
			byte b2 = 0;
			byte[] beaconID = Code.HexStringToBits("9c02be2963e2963", 60);
			SarMessageCode messageCode = SarMessageCode.ShortRlmTestService;
			IEnumerable<byte> second = GetSarMessage(beaconID, messageCode).Skip(count).Take(20);
			return new byte[2] { b, b2 }.Concat(second);
		}

		private IEnumerable<byte> GetSarMessage(IEnumerable<byte> beaconID, SarMessageCode messageCode, IEnumerable<byte>? parameterBits = null)
		{
			IEnumerable<byte> sarMessageCodeBits = GetSarMessageCodeBits(messageCode);
			if (parameterBits == null)
			{
				IEnumerable<byte> first;
				if (messageCode != SarMessageCode.ShortRlmAcknowledgementService)
				{
					first = NavigationData.ZeroBits.Take(2);
				}
				else
				{
					IEnumerable<byte> enumerable = acknowledgementServiceType1ParametersFirst2Bits;
					first = enumerable;
				}
				parameterBits = first.Concat(NavigationData.ZeroBits.Take(13));
			}
			byte[] source = beaconID.Concat(sarMessageCodeBits).Concat(parameterBits).ToArray(79);
			byte element = (byte)((uint)((IEnumerable<byte>)source).Select((Func<byte, int>)((byte b) => b)).Sum() & 1u);
			return source.Append(element);
		}

		private IEnumerable<byte> GetSarMessageCodeBits(SarMessageCode messageCode)
		{
			for (int i = 3; i >= 0; i--)
			{
				yield return (byte)((uint)((int)messageCode >> i) & 1u);
			}
		}

		static NavigationDataE1B()
		{
			byte[] array = new byte[22];
			array[0] = 1;
			emptySarBits = array;
			acknowledgementServiceType1ParametersFirst2Bits = new byte[2] { 1, 0 };
		}
	}
}
