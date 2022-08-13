using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Racelogic.Core;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class LocalizableDescriptionAttribute : DescriptionAttribute
{
	private string initialDescription;

	private readonly Type _resourcesType;

	private bool _isLocalized;

	public override string Description
	{
		get
		{
			if (!_isLocalized)
			{
				if (string.IsNullOrEmpty(initialDescription))
				{
					initialDescription = base.DescriptionValue;
				}
				ResourceManager resourceManager = _resourcesType.InvokeMember("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty, null, null, new object[0]) as ResourceManager;
				CultureInfo culture = _resourcesType.InvokeMember("Culture", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty, null, null, new object[0]) as CultureInfo;
				_isLocalized = true;
				if (resourceManager != null)
				{
					base.DescriptionValue = resourceManager.GetString(initialDescription, culture);
				}
			}
			return base.DescriptionValue;
		}
	}

	public LocalizableDescriptionAttribute(string description, Type resourcesType)
		: base(description)
	{
		_resourcesType = resourcesType;
	}

	public void ClearCache()
	{
		_isLocalized = false;
	}
}
