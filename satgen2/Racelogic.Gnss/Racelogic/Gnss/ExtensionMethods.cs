using System;

namespace Racelogic.Gnss
{
	public static class ExtensionMethods
	{
		public static string ToShortName(this ConstellationType constellationType)
		{
			return constellationType switch
			{
				ConstellationType.Gps => "GPS", 
				ConstellationType.Glonass => "GLO", 
				ConstellationType.BeiDou => "BDS", 
				ConstellationType.Galileo => "GAL", 
				ConstellationType.Navic => "NAV", 
				_ => throw new ArgumentException("Unsupported constellation type", "constellationType"), 
			};
		}

		public static string ToLongName(this ConstellationType constellationType)
		{
			return constellationType switch
			{
				ConstellationType.Gps => "GPS", 
				ConstellationType.Glonass => "Glonass", 
				ConstellationType.BeiDou => "BeiDou", 
				ConstellationType.Galileo => "Galileo", 
				ConstellationType.Navic => "Navic", 
				_ => throw new ArgumentException("Unsupported constellation type", "constellationType"), 
			};
		}

		public static string ToCodeName(this SignalType signalType)
		{
			switch (signalType)
			{
			case SignalType.None:
			case SignalType.GpsL1CA:
			case SignalType.GpsL1C:
			case SignalType.GpsL1CA | SignalType.GpsL1C:
			case SignalType.GpsL1P:
			case SignalType.GpsL1CA | SignalType.GpsL1P:
			case SignalType.GpsL1C | SignalType.GpsL1P:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1P:
			case SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1M:
			case SignalType.GpsL1C | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1M:
			case SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1C | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL2C:
			{
				SignalType num = signalType - 1;
				if (num <= (SignalType.GpsL1CA | SignalType.GpsL1C))
				{
					switch (num)
					{
					case SignalType.None:
						return "GPS_L1CA";
					case SignalType.GpsL1CA:
						return "GPS_L1C";
					case SignalType.GpsL1CA | SignalType.GpsL1C:
						return "GPS_L1P";
					case SignalType.GpsL1C:
						goto end_IL_0007;
					}
				}
				switch (signalType)
				{
				case SignalType.GpsL1M:
					return "GPS_L1M";
				case SignalType.GpsL2C:
					return "GPS_L2C";
				}
				break;
			}
			case SignalType.GpsL2P:
				return "GPS_L2P";
			case SignalType.GpsL2M:
				return "GPS_L2M";
			case SignalType.GpsL5I:
				return "GPS_L5I";
			case SignalType.GpsL5Q:
				return "GPS_L5Q";
			case SignalType.GpsL5:
				return "GPS_L5";
			case SignalType.GlonassL1OF:
				return "GLO_L1OF";
			case SignalType.GlonassL2OF:
				return "GLO_L2OF";
			case SignalType.BeiDouB1I:
				return "BDS_B1I";
			case SignalType.BeiDouB2I:
				return "BDS_B2I";
			case SignalType.BeiDouB3I:
				return "BDS_B3I";
			case SignalType.GalileoE1BC:
				return "GAL_E1BC";
			case SignalType.GalileoE5aI:
				return "GAL_E5AI";
			case SignalType.GalileoE5aQ:
				return "GAL_E5AQ";
			case SignalType.GalileoE5a:
				return "GAL_E5A";
			case SignalType.GalileoE5bI:
				return "GAL_E5BI";
			case SignalType.GalileoE5bQ:
				return "GAL_E5BQ";
			case SignalType.GalileoE5b:
				return "GAL_E5B";
			case SignalType.GalileoE5AltBoc:
				return "GAL_E5ALTBOC";
			case SignalType.GalileoE6BC:
				return "GAL_E6BC";
			case SignalType.NavicL5SPS:
				return "NAV_L5SPS";
			case SignalType.NavicSSPS:
				{
					return "NAV_SSPS";
				}
				end_IL_0007:
				break;
			}
			return "???";
		}

		public static string ToShortName(this SignalType signalType)
		{
			switch (signalType)
			{
			case SignalType.None:
			case SignalType.GpsL1CA:
			case SignalType.GpsL1C:
			case SignalType.GpsL1CA | SignalType.GpsL1C:
			case SignalType.GpsL1P:
			case SignalType.GpsL1CA | SignalType.GpsL1P:
			case SignalType.GpsL1C | SignalType.GpsL1P:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1P:
			case SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1M:
			case SignalType.GpsL1C | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1M:
			case SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1C | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL2C:
			{
				SignalType num = signalType - 1;
				if (num <= (SignalType.GpsL1CA | SignalType.GpsL1C))
				{
					switch (num)
					{
					case SignalType.None:
						return "GPS L1CA";
					case SignalType.GpsL1CA:
						return "GPS L1C";
					case SignalType.GpsL1CA | SignalType.GpsL1C:
						return "GPS L1P";
					case SignalType.GpsL1C:
						goto end_IL_0007;
					}
				}
				switch (signalType)
				{
				case SignalType.GpsL1M:
					return "GPS L1M";
				case SignalType.GpsL2C:
					return "GPS L2C";
				}
				break;
			}
			case SignalType.GpsL2P:
				return "GPS L2P";
			case SignalType.GpsL2M:
				return "GPS L2M";
			case SignalType.GpsL5I:
				return "GPS L5I";
			case SignalType.GpsL5Q:
				return "GPS L5Q";
			case SignalType.GpsL5:
				return "GPS L5";
			case SignalType.GlonassL1OF:
				return "GLO L1OF";
			case SignalType.GlonassL2OF:
				return "GLO L2OF";
			case SignalType.BeiDouB1I:
				return "BDS B1I";
			case SignalType.BeiDouB2I:
				return "BDS B2I";
			case SignalType.BeiDouB3I:
				return "BDS B3I";
			case SignalType.GalileoE1BC:
				return "GAL E1BC";
			case SignalType.GalileoE5aI:
				return "GAL E5AI";
			case SignalType.GalileoE5aQ:
				return "GAL E5AQ";
			case SignalType.GalileoE5a:
				return "GAL E5A";
			case SignalType.GalileoE5bI:
				return "GAL E5BI";
			case SignalType.GalileoE5bQ:
				return "GAL E5BQ";
			case SignalType.GalileoE5b:
				return "GAL E5B";
			case SignalType.GalileoE5AltBoc:
				return "GAL E5AltBOC";
			case SignalType.GalileoE6BC:
				return "GAL E6BC";
			case SignalType.NavicL5SPS:
				return "NAV L5sps";
			case SignalType.NavicSSPS:
				{
					return "NAV Ssps";
				}
				end_IL_0007:
				break;
			}
			return "???";
		}

		public static string ToLongName(this SignalType signalType)
		{
			switch (signalType)
			{
			case SignalType.None:
			case SignalType.GpsL1CA:
			case SignalType.GpsL1C:
			case SignalType.GpsL1CA | SignalType.GpsL1C:
			case SignalType.GpsL1P:
			case SignalType.GpsL1CA | SignalType.GpsL1P:
			case SignalType.GpsL1C | SignalType.GpsL1P:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1P:
			case SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1M:
			case SignalType.GpsL1C | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1M:
			case SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1C | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL1CA | SignalType.GpsL1C | SignalType.GpsL1P | SignalType.GpsL1M:
			case SignalType.GpsL2C:
			{
				SignalType num = signalType - 1;
				if (num <= (SignalType.GpsL1CA | SignalType.GpsL1C))
				{
					switch (num)
					{
					case SignalType.None:
						return "GPS L1 C/A";
					case SignalType.GpsL1CA:
						return "GPS L1 C";
					case SignalType.GpsL1CA | SignalType.GpsL1C:
						return "GPS L1 P";
					case SignalType.GpsL1C:
						goto end_IL_0007;
					}
				}
				switch (signalType)
				{
				case SignalType.GpsL1M:
					return "GPS L1 M";
				case SignalType.GpsL2C:
					return "GPS L2 C";
				}
				break;
			}
			case SignalType.GpsL2P:
				return "GPS L2 P";
			case SignalType.GpsL2M:
				return "GPS L2 M";
			case SignalType.GpsL5I:
				return "GPS L5 I";
			case SignalType.GpsL5Q:
				return "GPS L5 Q";
			case SignalType.GpsL5:
				return "GPS L5";
			case SignalType.GlonassL1OF:
				return "Glonass L1 OF";
			case SignalType.GlonassL2OF:
				return "Glonass L2 OF";
			case SignalType.BeiDouB1I:
				return "BeiDou B1 I";
			case SignalType.BeiDouB2I:
				return "BeiDou B2 I";
			case SignalType.BeiDouB3I:
				return "BeiDou B3 I";
			case SignalType.GalileoE1BC:
				return "Galileo E1 BC";
			case SignalType.GalileoE5aI:
				return "Galileo E5A I";
			case SignalType.GalileoE5aQ:
				return "Galileo E5A Q";
			case SignalType.GalileoE5a:
				return "Galileo E5A";
			case SignalType.GalileoE5bI:
				return "Galileo E5B I";
			case SignalType.GalileoE5bQ:
				return "Galileo E5B Q";
			case SignalType.GalileoE5b:
				return "Galileo E5B";
			case SignalType.GalileoE5AltBoc:
				return "Galileo E5 AltBOC";
			case SignalType.GalileoE6BC:
				return "Galileo E6 BC";
			case SignalType.NavicL5SPS:
				return "Navic L5 SPS";
			case SignalType.NavicSSPS:
				{
					return "Navic S SPS";
				}
				end_IL_0007:
				break;
			}
			return "???";
		}
	}
}
