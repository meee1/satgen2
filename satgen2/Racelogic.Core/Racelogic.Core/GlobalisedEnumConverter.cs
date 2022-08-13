using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Racelogic.Core;

public class GlobalisedEnumConverter : EnumConverter
{
	private class LookupTable : Dictionary<string, object>
	{
	}

	private Dictionary<CultureInfo, LookupTable> _lookupTables = new Dictionary<CultureInfo, LookupTable>();

	private ResourceManager _resourceManager;

	private Dictionary<string, string> _dictionary;

	public GlobalisedEnumConverter(Type type, ResourceManager resourceManager)
		: base(type)
	{
		_resourceManager = resourceManager;
	}

	public GlobalisedEnumConverter(Type type, Dictionary<string, string> dictionary)
		: base(type)
	{
		_dictionary = dictionary;
	}

	public GlobalisedEnumConverter(Type type)
		: base(type)
	{
	}

	public static string ConvertToString(Enum value, ResourceManager _resourceManager)
	{
		return GetValueText(value, _resourceManager);
	}

	public static string ConvertToString(Enum value)
	{
		return TypeDescriptor.GetConverter(value.GetType()).ConvertToString(value);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		string text = value as string;
		if (!string.IsNullOrEmpty(text))
		{
			object obj = GetValue(culture, text);
			if (obj == null)
			{
				obj = base.ConvertFrom(context, culture, value);
			}
			return obj;
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value != null && destinationType == typeof(string))
		{
			return GetValueText(culture, value);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private static string GetValueText(Enum value, ResourceManager _resourceManager)
	{
		string text = null;
		string text2 = string.Format(CultureInfo.CurrentUICulture, "{0}_{1}", value.GetType().Name, value.ToString());
		if (_resourceManager != null)
		{
			text = _resourceManager.GetString(text2, CultureInfo.CurrentUICulture);
			if (text == null)
			{
				text = text2;
			}
		}
		return text;
	}

	private LookupTable GetLookupTable(CultureInfo culture)
	{
		LookupTable value = null;
		if (culture == null)
		{
			culture = CultureInfo.CurrentCulture;
		}
		if (!_lookupTables.TryGetValue(culture, out value))
		{
			value = new LookupTable();
			foreach (object? standardValue in GetStandardValues())
			{
				string valueText = GetValueText(culture, standardValue);
				if (valueText != null)
				{
					value.Add(valueText, standardValue);
				}
			}
			_lookupTables.Add(culture, value);
		}
		return value;
	}

	private string GetValueText(CultureInfo culture, object value)
	{
		string text = null;
		Type type = value.GetType();
		string text2 = string.Format(culture, "{0}_{1}", type.Name, value.ToString());
		if (_resourceManager != null)
		{
			text = _resourceManager.GetString(text2, culture);
			if (text == null)
			{
				text = text2;
			}
		}
		else if (_dictionary != null)
		{
			text = ((!_dictionary.TryGetValue(text2, out var value2)) ? text2 : value2);
		}
		else if (text == null)
		{
			text = text2;
		}
		return text;
	}

	private object GetValue(CultureInfo culture, string text)
	{
		LookupTable lookupTable = GetLookupTable(culture);
		object value = null;
		lookupTable.TryGetValue(text, out value);
		return value;
	}
}
