using System;
using System.ComponentModel;
using Infralution.Localization.Wpf;
using Racelogic.Gnss.SatGen.BlackBox.Properties;

namespace Racelogic.Gnss.SatGen.BlackBox;

internal class LocalizedEnumConverter : ResourceEnumConverter
{
	public LocalizedEnumConverter(Type enumType)
		: base(enumType, Resources.ResourceManager)
	{
	}

	public static void Add(Type enumType)
	{
		TypeDescriptor.AddAttributes(enumType, new TypeConverterAttribute(typeof(LocalizedEnumConverter)));
	}
}
