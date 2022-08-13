using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Racelogic.Core;

public static class Extensions
{
	public static T GetNextEnumValue<T>(this T currentValue, bool cycle = false) where T : struct, IComparable, IFormattable, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("This extension method is designed only for enums");
		}
		IEnumerable<T> source = Enum.GetValues(typeof(T)).Cast<T>();
		if (cycle)
		{
			return source.SkipWhile((T v) => !v.Equals(currentValue)).Skip(1).DefaultIfEmpty(source.First())
				.First();
		}
		return source.SkipWhile((T v) => !v.Equals(currentValue)).Skip(1).DefaultIfEmpty(source.Last())
			.First();
	}

	public static T GetPreviousEnumValue<T>(this T currentValue, bool cycle = false) where T : struct, IComparable, IFormattable, IConvertible
	{
		if (!typeof(T).IsEnum)
		{
			throw new ArgumentException("This extension method is designed only for enums");
		}
		IEnumerable<T> source = Enum.GetValues(typeof(T)).Cast<T>();
		if (cycle)
		{
			return source.Reverse().SkipWhile((T v) => !v.Equals(currentValue)).Skip(1)
				.DefaultIfEmpty(source.Last())
				.First();
		}
		return source.Reverse().SkipWhile((T v) => !v.Equals(currentValue)).Skip(1)
			.DefaultIfEmpty(source.First())
			.First();
	}

	public static string LocalisedString(this Enum value)
	{
		Type type = value.GetType();
		if (type.GetCustomAttributes(typeof(FlagsAttribute), inherit: true).Length != 0)
		{
			object obj = null;
			List<string> list = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (object? value2 in Enum.GetValues(type))
			{
				if (value.HasFlag((Enum)value2))
				{
					list.Add(((Enum)value2).GetLocalisedString());
				}
			}
			if (list.Count > 1)
			{
				int num = 0;
				if (Enum.GetUnderlyingType(type) == typeof(int))
				{
					num = (Enum.IsDefined(type, 0) ? 1 : 0);
				}
				else if (Enum.GetUnderlyingType(type) == typeof(uint))
				{
					num = (Enum.IsDefined(type, 0u) ? 1 : 0);
				}
				else if (Enum.GetUnderlyingType(type) == typeof(short))
				{
					num = (Enum.IsDefined(type, (short)0) ? 1 : 0);
				}
				else if (Enum.GetUnderlyingType(type) == typeof(ushort))
				{
					num = (Enum.IsDefined(type, (ushort)0) ? 1 : 0);
				}
				else if (Enum.GetUnderlyingType(type) == typeof(byte))
				{
					num = (Enum.IsDefined(type, (byte)0) ? 1 : 0);
				}
				else if (Enum.GetUnderlyingType(type) == typeof(sbyte))
				{
					num = (Enum.IsDefined(type, (sbyte)0) ? 1 : 0);
				}
				else if (Enum.GetUnderlyingType(type) == typeof(long))
				{
					num = (Enum.IsDefined(type, 0L) ? 1 : 0);
				}
				else if (Enum.GetUnderlyingType(type) == typeof(ulong))
				{
					num = (Enum.IsDefined(type, 0uL) ? 1 : 0);
				}
				for (int i = num; i < list.Count - 1; i++)
				{
					stringBuilder.Append(list[i]);
					stringBuilder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
					stringBuilder.Append(" ");
				}
			}
			if (list.Count > 0)
			{
				stringBuilder.Append(list[list.Count - 1]);
				return stringBuilder.ToString();
			}
			if (obj != null)
			{
				return ((Enum)obj).GetLocalisedString();
			}
		}
		FieldInfo field = type.GetField(value.ToString());
		if (field != null)
		{
			LocalizableDescriptionAttribute[] array = (LocalizableDescriptionAttribute[])field.GetCustomAttributes(typeof(LocalizableDescriptionAttribute), inherit: false);
			if (array.Length == 0 || string.IsNullOrEmpty(array[0].Description))
			{
				return value.ToString();
			}
			return array[0].Description;
		}
		return value.ToString();
	}

	private static string GetLocalisedString(this Enum value)
	{
		FieldInfo field = value.GetType().GetField(value.ToString());
		if (field != null)
		{
			LocalizableDescriptionAttribute[] array = (LocalizableDescriptionAttribute[])field.GetCustomAttributes(typeof(LocalizableDescriptionAttribute), inherit: false);
			if (array.Length == 0 || string.IsNullOrEmpty(array[0].Description))
			{
				return value.ToString();
			}
			return array[0].Description;
		}
		return value.ToString();
	}

	[Obsolete("Raise method is deprecacted and will soon be removed from Racelgic.Core.  Please use the Raise(..) extension (defined in Racelogic.Utilities), execute it for your PropertyChanged event handler.")]
	public static void Raise<T>(this PropertyChangedEventHandler handler, Expression<Func<T>> propertyExpression)
	{
		if (handler != null)
		{
			object callingObject;
			string propertyName = PropertyHelper.GetPropertyName(propertyExpression, out callingObject);
			handler(callingObject, new PropertyChangedEventArgs(propertyName));
		}
	}
}
