using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Racelogic.Gnss.SatGen.BlackBox.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Racelogic.Gnss.SatGen.BlackBox.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string SatCountFormat => ResourceManager.GetString("SatCountFormat", resourceCulture);

	internal static string SatCountLimitMode_Automatic => ResourceManager.GetString("SatCountLimitMode_Automatic", resourceCulture);

	internal static string SatCountLimitMode_Constellation => ResourceManager.GetString("SatCountLimitMode_Constellation", resourceCulture);

	internal static string SatCountLimitMode_Manual => ResourceManager.GetString("SatCountLimitMode_Manual", resourceCulture);

	internal static string SatCountLimitMode_None => ResourceManager.GetString("SatCountLimitMode_None", resourceCulture);

	internal Resources()
	{
	}
}
