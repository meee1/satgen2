using System;
using System.Management;

namespace Racelogic.Comms.Serial.WMI;

internal class Connection
{
	private ManagementScope connectionScope;

	private ConnectionOptions options;

	public ManagementScope GetConnectionScope => connectionScope;

	public ConnectionOptions GetOptions => options;

	private void EstablishConnection(string userName, string password, string domain, string machineName)
	{
		options = SetConnectionOptions();
		if (domain != null || userName != null)
		{
			options.Username = domain + "\\" + userName;
			options.Password = password;
		}
		connectionScope = SetConnectionScope(machineName, options);
	}

	public static ConnectionOptions SetConnectionOptions()
	{
		ConnectionOptions connectionOptions = new ConnectionOptions();
		connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
		connectionOptions.Authentication = AuthenticationLevel.Default;
		connectionOptions.EnablePrivileges = true;
		return connectionOptions;
	}

	public static ManagementScope SetConnectionScope(string machineName, ConnectionOptions options)
	{
		ManagementScope managementScope = new ManagementScope();
		managementScope.Path = new ManagementPath("\\\\" + machineName + "\\root\\CIMV2");
		managementScope.Options = options;
		try
		{
			managementScope.Connect();
		}
		catch (ManagementException ex)
		{
			Console.WriteLine("An Error Occurred: " + ex.Message.ToString());
		}
		return managementScope;
	}

	public Connection()
	{
		EstablishConnection(null, null, null, Environment.MachineName);
	}

	public Connection(string userName, string password, string domain, string machineName)
	{
		EstablishConnection(userName, password, domain, machineName);
	}
}
