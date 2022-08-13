using System;
using System.Collections.Generic;

namespace Racelogic.Utilities.Sntp;

public class SntpData
{
	private readonly byte[] data;

	private static readonly DateTime Epoch = new DateTime(1900, 1, 1);

	private const byte LeapIndicatorMask = 192;

	private const byte LeapIndicatorOffset = 6;

	private const byte ModeComplementMask = 248;

	private const byte ModeMask = 7;

	private const int originateIndex = 24;

	private const int receiveIndex = 32;

	private const int referenceIdentifierOffset = 12;

	private const int referenceIndex = 16;

	private const int transmitIndex = 40;

	private const byte VersionNumberComplementMask = 199;

	private const byte VersionNumberMask = 56;

	private const byte VersionNumberOffset = 3;

	public const int MaximumLength = 68;

	public const int MinimumLength = 48;

	public const long TicksPerSecond = 10000000L;

	private static readonly Dictionary<LeapIndicator, string> LeapIndicatorDictionary = new Dictionary<LeapIndicator, string>
	{
		{
			LeapIndicator.NoWarning,
			"No warning"
		},
		{
			LeapIndicator.LastMinute61Seconds,
			"Last minute has 61 seconds"
		},
		{
			LeapIndicator.LastMinute59Seconds,
			"Last minute has 59 seconds"
		},
		{
			LeapIndicator.Alarm,
			"Alarm condition (clock not synchronized)"
		}
	};

	private static readonly Dictionary<SntpVersionNumber, string> VersionNumberDictionary = new Dictionary<SntpVersionNumber, string>
	{
		{
			SntpVersionNumber.Version3,
			"Version 3 (IPv4 only)"
		},
		{
			SntpVersionNumber.Version4,
			"Version 4 (IPv4, IPv6 and OSI)"
		}
	};

	private static readonly Dictionary<SntpMode, string> ModeDictionary = new Dictionary<SntpMode, string>
	{
		{
			SntpMode.Reserved,
			"Reserved"
		},
		{
			SntpMode.SymmetricActive,
			"Symmetric active"
		},
		{
			SntpMode.SymmetricPassive,
			"Symmetric passive"
		},
		{
			SntpMode.Client,
			"Client"
		},
		{
			SntpMode.Server,
			"Server"
		},
		{
			SntpMode.Broadcast,
			"Broadcast"
		},
		{
			SntpMode.ReservedNTPControl,
			"Reserved for NTP control message"
		},
		{
			SntpMode.ReservedPrivate,
			"Reserved for private use"
		}
	};

	private static readonly Dictionary<Stratum, string> StratumDictionary = new Dictionary<Stratum, string>
	{
		{
			Stratum.Primary,
			"1, Primary reference (e.g. radio clock)"
		},
		{
			Stratum.Secondary,
			"2, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary3,
			"3, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary4,
			"4, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary5,
			"5, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary6,
			"6, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary7,
			"7, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary8,
			"8, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary9,
			"9, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary10,
			"10, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary11,
			"11, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary12,
			"12, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary13,
			"13, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary14,
			"14, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Secondary15,
			"15, Secondary reference (via NTP or SNTP)"
		},
		{
			Stratum.Unspecified,
			"Unspecified or unavailable"
		}
	};

	private static readonly Dictionary<ReferenceIdentifier, string> RefererenceIdentifierDictionary = new Dictionary<ReferenceIdentifier, string>
	{
		{
			ReferenceIdentifier.ACTS,
			"NIST dialup modem service"
		},
		{
			ReferenceIdentifier.CHU,
			"Ottawa (Canada) Radio 3330, 7335, 14670 kHz"
		},
		{
			ReferenceIdentifier.DCF,
			"Mainflingen (Germany) Radio 77.5 kHz"
		},
		{
			ReferenceIdentifier.GOES,
			"Geostationary Orbit Environment Satellite"
		},
		{
			ReferenceIdentifier.GPS,
			"Global Positioning Service"
		},
		{
			ReferenceIdentifier.LOCL,
			"Uncalibrated local clock used as a primary reference for a subnet without external means of synchronization"
		},
		{
			ReferenceIdentifier.LORC,
			"LORAN-C radionavigation system"
		},
		{
			ReferenceIdentifier.MSF,
			"Rugby (UK) Radio 60 kHz"
		},
		{
			ReferenceIdentifier.OMEG,
			"OMEGA radionavigation system"
		},
		{
			ReferenceIdentifier.PPS,
			"Atomic clock or other pulse-per-second source individually calibrated to national standards"
		},
		{
			ReferenceIdentifier.PTB,
			"PTB (Germany) modem service"
		},
		{
			ReferenceIdentifier.TDF,
			"Allouis (France) Radio 164 kHz"
		},
		{
			ReferenceIdentifier.USNO,
			"U.S. Naval Observatory modem service"
		},
		{
			ReferenceIdentifier.WWV,
			"Ft. Collins (US) Radio 2.5, 5, 10, 15, 20 MHz"
		},
		{
			ReferenceIdentifier.WWVB,
			"Boulder (US) Radio 60 kHz"
		},
		{
			ReferenceIdentifier.WWVH,
			"Kaui Hawaii (US) Radio 2.5, 5, 10, 15 MHz"
		}
	};

	public DateTime DestinationDateTime { get; internal set; }

	public LeapIndicator LeapIndicator => (LeapIndicator)LeapIndicatorValue;

	public string LeapIndicatorText
	{
		get
		{
			LeapIndicatorDictionary.TryGetValue(LeapIndicator, out var value);
			return value;
		}
	}

	private byte LeapIndicatorValue => (byte)((data[0] & 0xC0) >> 6);

	public int Length => data.Length;

	public double LocalClockOffset => (double)(ReceiveDateTime.Ticks - OriginateDateTime.Ticks + (TransmitDateTime.Ticks - DestinationDateTime.Ticks)) / 2.0 / 10000000.0;

	public SntpMode Mode
	{
		get
		{
			return (SntpMode)ModeValue;
		}
		private set
		{
			ModeValue = (byte)value;
		}
	}

	public string ModeText
	{
		get
		{
			ModeDictionary.TryGetValue(Mode, out var value);
			return value;
		}
	}

	private byte ModeValue
	{
		get
		{
			return (byte)(data[0] & 7u);
		}
		set
		{
			data[0] = (byte)((data[0] & 0xF8u) | value);
		}
	}

	public DateTime OriginateDateTime => TimestampToDateTime(24);

	public double PollInterval => Math.Pow(2.0, (sbyte)data[2]);

	public double Precision => Math.Pow(2.0, (sbyte)data[3]);

	public DateTime ReceiveDateTime => TimestampToDateTime(32);

	public DateTime ReferenceDateTime => TimestampToDateTime(16);

	public string ReferenceId
	{
		get
		{
			string value = null;
			switch (Stratum)
			{
			case Stratum.Unspecified:
			case Stratum.Primary:
			{
				uint num = 0u;
				for (int i = 0; i <= 3; i++)
				{
					num = (num << 8) | data[12 + i];
				}
				if (!RefererenceIdentifierDictionary.TryGetValue((ReferenceIdentifier)num, out value))
				{
					value = $"{(char)data[12]}{(char)data[13]}{(char)data[14]}{(char)data[15]}";
				}
				break;
			}
			case Stratum.Secondary:
			case Stratum.Secondary3:
			case Stratum.Secondary4:
			case Stratum.Secondary5:
			case Stratum.Secondary6:
			case Stratum.Secondary7:
			case Stratum.Secondary8:
			case Stratum.Secondary9:
			case Stratum.Secondary10:
			case Stratum.Secondary11:
			case Stratum.Secondary12:
			case Stratum.Secondary13:
			case Stratum.Secondary14:
			case Stratum.Secondary15:
				switch (VersionNumber)
				{
				case SntpVersionNumber.Version3:
					value = $"{data[12]}.{data[13]}.{data[14]}.{data[15]}";
					break;
				default:
					if (VersionNumber < SntpVersionNumber.Version3)
					{
						value = $"{data[12]}.{data[13]}.{data[14]}.{data[15]}";
					}
					break;
				case SntpVersionNumber.Version4:
					break;
				}
				break;
			}
			return value;
		}
	}

	public double RootDelay => SecondsStampToSeconds(4);

	public double RootDispersion => SecondsStampToSeconds(8);

	public double RoundTripDelay => (double)(DestinationDateTime.Ticks - OriginateDateTime.Ticks - (ReceiveDateTime.Ticks - TransmitDateTime.Ticks)) / 10000000.0;

	public Stratum Stratum => (Stratum)StratumValue;

	public string StratumText
	{
		get
		{
			if (!StratumDictionary.TryGetValue(Stratum, out var value))
			{
				return "Reserved";
			}
			return value;
		}
	}

	private byte StratumValue => data[1];

	public DateTime TransmitDateTime
	{
		get
		{
			return TimestampToDateTime(40);
		}
		private set
		{
			DateTimeToTimestamp(value, 40);
		}
	}

	public SntpVersionNumber VersionNumber
	{
		get
		{
			return (SntpVersionNumber)VersionNumberValue;
		}
		private set
		{
			VersionNumberValue = (byte)value;
		}
	}

	public string VersionNumberText
	{
		get
		{
			if (!VersionNumberDictionary.TryGetValue(VersionNumber, out var value))
			{
				return "Unknown";
			}
			return value;
		}
	}

	private byte VersionNumberValue
	{
		get
		{
			return (byte)((data[0] & 0x38) >> 3);
		}
		set
		{
			data[0] = (byte)((data[0] & 0xC7u) | (uint)(value << 3));
		}
	}

	internal SntpData(byte[] bytearray)
	{
		if (bytearray.Length >= 48 && bytearray.Length <= 68)
		{
			data = bytearray;
			return;
		}
		throw new ArgumentOutOfRangeException("Byte Array", $"Byte array must have a length between {48} and {68}.");
	}

	internal SntpData()
		: this(new byte[48])
	{
	}

	private void DateTimeToTimestamp(DateTime dateTime, int startIndex)
	{
		long ticks = (dateTime - Epoch).Ticks;
		ulong num = (ulong)ticks / 10000000uL;
		ulong num2 = (ulong)ticks % 10000000uL * 4294967296L / 10000000uL;
		for (int num3 = 3; num3 >= 0; num3--)
		{
			data[startIndex + num3] = (byte)num;
			num >>= 8;
		}
		for (int num4 = 7; num4 >= 4; num4--)
		{
			data[startIndex + num4] = (byte)num2;
			num2 >>= 8;
		}
	}

	private double SecondsStampToSeconds(int startIndex)
	{
		ulong num = 0uL;
		for (int i = 0; i <= 1; i++)
		{
			num = (num << 8) | data[startIndex + i];
		}
		ulong num2 = 0uL;
		for (int j = 2; j <= 3; j++)
		{
			num2 = (num2 << 8) | data[startIndex + j];
		}
		return (double)(num * 10000000 + num2 * 10000000 / 65536uL) / 10000000.0;
	}

	private DateTime TimestampToDateTime(int startIndex)
	{
		ulong num = 0uL;
		for (int i = 0; i <= 3; i++)
		{
			num = (num << 8) | data[startIndex + i];
		}
		ulong num2 = 0uL;
		for (int j = 4; j <= 7; j++)
		{
			num2 = (num2 << 8) | data[startIndex + j];
		}
		ulong value = num * 10000000 + num2 * 10000000 / 4294967296uL;
		return Epoch + TimeSpan.FromTicks((long)value);
	}

	internal static SntpData GetClientRequestPacket(SntpVersionNumber versionNumber)
	{
		return new SntpData
		{
			Mode = SntpMode.Client,
			VersionNumber = versionNumber,
			TransmitDateTime = DateTime.UtcNow
		};
	}

	public static implicit operator SntpData(byte[] byteArray)
	{
		return new SntpData(byteArray);
	}

	public static implicit operator byte[](SntpData sntpPacket)
	{
		return sntpPacket.data;
	}
}
