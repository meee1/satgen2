using System.Reflection;

namespace Racelogic.Core;

public class AssemblyInfo
{
	private Assembly assembly;

	public string Title => AssemblyInformation.Title(assembly);

	public string CopyRight => AssemblyInformation.Copyright(assembly);

	public string Version => AssemblyInformation.Version(assembly);

	public string AssemblyVersion => AssemblyInformation.AssemblyVersion(assembly);

	public string Company => AssemblyInformation.Company(assembly);

	public string Description => AssemblyInformation.Description(assembly);

	public AssemblyInfo(Assembly assembly)
	{
		this.assembly = assembly;
	}
}
