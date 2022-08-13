using System.Reflection;

namespace Racelogic.Core;

public static class AssemblyInformation
{
	public static string Title(Assembly assembly)
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((AssemblyTitleAttribute)customAttributes[0]).Title;
		}
		return string.Empty;
	}

	public static string Copyright(Assembly assembly)
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
		}
		return string.Empty;
	}

	public static string AssemblyVersion(Assembly assembly)
	{
		return assembly.GetName().Version!.ToString();
	}

	public static string Company(Assembly assembly)
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((AssemblyCompanyAttribute)customAttributes[0]).Company;
		}
		return string.Empty;
	}

	public static string Description(Assembly assembly)
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((AssemblyDescriptionAttribute)customAttributes[0]).Description;
		}
		return string.Empty;
	}

	public static string Version(Assembly assembly)
	{
		object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), inherit: false);
		if (customAttributes.Length != 0)
		{
			return ((AssemblyFileVersionAttribute)customAttributes[0]).Version.Replace('*', '0');
		}
		return string.Empty;
	}
}
