using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;

namespace Racelogic.Utilities.Sntp;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class RemoteSntpServer
{
	private static RemoteSntpServer defaultServer;

	private string _HostNameOrAddress = DefaultHostName;

	private int _Port;

	public static readonly RemoteSntpServer Africa = new RemoteSntpServer("africa.pool.ntp.org");

	public static readonly RemoteSntpServer AppleEurope = new RemoteSntpServer("time1.euro.apple.com");

	public static readonly RemoteSntpServer AppleEurope2 = new RemoteSntpServer("time.euro.apple.com");

	public static readonly RemoteSntpServer Asia = new RemoteSntpServer("asia.pool.ntp.org");

	public static readonly RemoteSntpServer[] AsiaServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.asia.pool.ntp.org"),
		new RemoteSntpServer("1.asia.pool.ntp.org"),
		new RemoteSntpServer("2.asia.pool.ntp.org"),
		new RemoteSntpServer("3.asia.pool.ntp.org")
	};

	public static readonly RemoteSntpServer Australia = new RemoteSntpServer("au.pool.ntp.org");

	public static readonly RemoteSntpServer[] AustraliaServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.au.pool.ntp.org"),
		new RemoteSntpServer("1.au.pool.ntp.org"),
		new RemoteSntpServer("2.au.pool.ntp.org"),
		new RemoteSntpServer("3.au.pool.ntp.org")
	};

	public static readonly RemoteSntpServer BlueYonder = new RemoteSntpServer("ntp.blueyonder.co.uk");

	public static readonly RemoteSntpServer Canada = new RemoteSntpServer("ca.pool.ntp.org");

	public static readonly RemoteSntpServer[] CanadaServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.ca.pool.ntp.org"),
		new RemoteSntpServer("1.ca.pool.ntp.org"),
		new RemoteSntpServer("2.ca.pool.ntp.org"),
		new RemoteSntpServer("3.ca.pool.ntp.org")
	};

	public static string DefaultHostName = "time.nist.gov";

	public const int DefaultPort = 123;

	public static readonly RemoteSntpServer Europe = new RemoteSntpServer("europe.pool.ntp.org");

	public static readonly RemoteSntpServer[] EuropeServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.europe.pool.ntp.org"),
		new RemoteSntpServer("1.europe.pool.ntp.org"),
		new RemoteSntpServer("2.europe.pool.ntp.org"),
		new RemoteSntpServer("3.europe.pool.ntp.org")
	};

	public static readonly RemoteSntpServer Microsoft = new RemoteSntpServer("time-nw.nist.gov");

	public static readonly RemoteSntpServer NorthAmerica = new RemoteSntpServer("north-america.pool.ntp.org");

	public static readonly RemoteSntpServer[] NorthAmericaServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.north-america.pool.ntp.org"),
		new RemoteSntpServer("1.north-america.pool.ntp.org"),
		new RemoteSntpServer("2.north-america.pool.ntp.org"),
		new RemoteSntpServer("3.north-america.pool.ntp.org")
	};

	public static readonly RemoteSntpServer NTL = new RemoteSntpServer("time.cableol.net");

	public static readonly RemoteSntpServer Oceania = new RemoteSntpServer("oceania.pool.ntp.org");

	public static readonly RemoteSntpServer[] OceaniaServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.oceania.pool.ntp.org"),
		new RemoteSntpServer("1.oceania.pool.ntp.org"),
		new RemoteSntpServer("2.oceania.pool.ntp.org"),
		new RemoteSntpServer("3.oceania.pool.ntp.org")
	};

	public static readonly RemoteSntpServer Pool = new RemoteSntpServer("pool.ntp.org");

	public static readonly RemoteSntpServer[] PoolServers = new RemoteSntpServer[3]
	{
		new RemoteSntpServer("0.pool.ntp.org"),
		new RemoteSntpServer("1.pool.ntp.org"),
		new RemoteSntpServer("2.pool.ntp.org")
	};

	public static readonly RemoteSntpServer SouthAmerica = new RemoteSntpServer("south-america.pool.ntp.org");

	public static readonly RemoteSntpServer[] SouthAmericaServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.south-america.pool.ntp.org"),
		new RemoteSntpServer("1.south-america.pool.ntp.org"),
		new RemoteSntpServer("2.south-america.pool.ntp.org"),
		new RemoteSntpServer("3.south-america.pool.ntp.org")
	};

	public static readonly RemoteSntpServer UKJanetGPS = new RemoteSntpServer("ntp1.ja.net");

	public static readonly RemoteSntpServer UnitedKingdom = new RemoteSntpServer("uk.pool.ntp.org");

	public static readonly RemoteSntpServer[] UnitedKingdomServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.uk.pool.ntp.org"),
		new RemoteSntpServer("1.uk.pool.ntp.org"),
		new RemoteSntpServer("2.uk.pool.ntp.org"),
		new RemoteSntpServer("3.uk.pool.ntp.org")
	};

	public static readonly RemoteSntpServer UnitedStates = new RemoteSntpServer("us.pool.ntp.org");

	public static readonly RemoteSntpServer[] UnitedStatesServers = new RemoteSntpServer[4]
	{
		new RemoteSntpServer("0.us.pool.ntp.org"),
		new RemoteSntpServer("1.us.pool.ntp.org"),
		new RemoteSntpServer("2.us.pool.ntp.org"),
		new RemoteSntpServer("3.us.pool.ntp.org")
	};

	public static readonly RemoteSntpServer USNavalObservatory = new RemoteSntpServer("tock.usno.navy.mil");

	public static readonly RemoteSntpServer USNavalObservatory2 = new RemoteSntpServer("tick.usno.navy.mil");

	public static readonly RemoteSntpServer USNavalObservatory3 = new RemoteSntpServer("ntp1.usno.navy.mil");

	public static readonly RemoteSntpServer USXMissionGPS = new RemoteSntpServer("clock.xmission.com");

	public static readonly RemoteSntpServer Windows = new RemoteSntpServer("time.windows.com");

	public static RemoteSntpServer Default
	{
		get
		{
			if (defaultServer == null)
			{
				switch (CountryDatabase.GetContinent(GetCountryName()))
				{
				default:
					defaultServer = Default;
					break;
				case Continent.Africa:
					defaultServer = Africa;
					break;
				case Continent.Asia:
					defaultServer = Asia;
					break;
				case Continent.Europe:
					defaultServer = Europe;
					break;
				case Continent.NorthAmerica:
					defaultServer = NorthAmerica;
					break;
				case Continent.Oceania:
					defaultServer = Oceania;
					break;
				case Continent.SouthAmerica:
					defaultServer = SouthAmerica;
					break;
				}
			}
			return defaultServer;
		}
	}

	[Description("The host name or address of the server.")]
	[NotifyParentProperty(true)]
	public string HostNameOrAddress
	{
		get
		{
			return _HostNameOrAddress;
		}
		set
		{
			value = value.Trim();
			if (string.IsNullOrEmpty(value))
			{
				value = DefaultHostName;
			}
			_HostNameOrAddress = value;
		}
	}

	[Description("The port number that this server uses.")]
	[DefaultValue(123)]
	[NotifyParentProperty(true)]
	public int Port
	{
		get
		{
			return _Port;
		}
		set
		{
			if (value >= 0 && value <= 65535)
			{
				_Port = value;
			}
			else
			{
				_Port = 123;
			}
		}
	}

	public RemoteSntpServer(string hostNameOrAddress, int port)
	{
		HostNameOrAddress = hostNameOrAddress;
		Port = port;
	}

	public RemoteSntpServer(string hostNameOrAddress)
		: this(hostNameOrAddress, 123)
	{
	}

	public RemoteSntpServer()
		: this(DefaultHostName, 123)
	{
	}

	public IPEndPoint GetIPEndPoint()
	{
		return new IPEndPoint(Dns.GetHostAddresses(HostNameOrAddress)[0], Port);
	}

	public override string ToString()
	{
		return $"{HostNameOrAddress}:{Port}";
	}

	private static string GetCountryName()
	{
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://api.hostip.info/get_json.php");
		httpWebRequest.Timeout = 5000;
		httpWebRequest.ReadWriteTimeout = 5000;
		HttpWebResponse httpWebResponse;
		try
		{
			httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
		}
		catch (WebException)
		{
			return string.Empty;
		}
		using StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream());
		return streamReader.ReadLine()!.Split(new char[1] { '"' }).ElementAtOrDefault(3);
	}
}
