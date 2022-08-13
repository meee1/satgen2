using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Racelogic.DataTypes;

namespace Racelogic.Utilities;

public static class Extensions
{
	private static readonly double log1024inv = Math.Log(10.0, 1024.0);

	private static readonly string[] fileSizeUnits = new string[9] { "B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

	private static readonly bool fullPropertyCheck = Debugger.IsAttached;

	public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (keySelector == null)
		{
			throw new ArgumentNullException("keySelector");
		}
		HashSet<TKey> keys = new HashSet<TKey>();
		foreach (TSource item in source)
		{
			if (keys.Add(keySelector(item)))
			{
				yield return item;
			}
		}
	}

	public static double HarmonicAverage(this IEnumerable<double> values)
	{
		double result = 0.0;
		int num = values.Count();
		if (num > 0)
		{
			double num2 = values.Sum((double d) => 1.0 / d);
			result = (double)num / num2;
		}
		return result;
	}

	public static int IndexOf<T>(this IEnumerable<T> items, T item)
	{
		try
		{
			return items.IndexOf((T i) => EqualityComparer<T>.Default.Equals(item, i));
		}
		catch (ArgumentNullException)
		{
			throw;
		}
	}

	public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int num = 0;
		foreach (T item in items)
		{
			if (predicate(item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public static IEnumerable<T> Interleave<T>(this IEnumerable<IEnumerable<T>> source)
	{
		IEnumerable<IEnumerator<T>> enumerators = source.Select((IEnumerable<T> s) => s.GetEnumerator()).ToArray();
		while (enumerators.All((IEnumerator<T> en) => en.MoveNext()))
		{
			foreach (IEnumerator<T> item in enumerators)
			{
				yield return item.Current;
			}
		}
		foreach (IEnumerator<T> item2 in enumerators)
		{
			item2.Dispose();
		}
	}

	public static IEnumerable<T> Interleave<T>(this IEnumerable<T> first, IEnumerable<T> second)
	{
		using IEnumerator<T> firstIterator = first.GetEnumerator();
		using IEnumerator<T> secondIterator = second.GetEnumerator();
		while (firstIterator.MoveNext())
		{
			yield return firstIterator.Current;
			if (secondIterator.MoveNext())
			{
				yield return secondIterator.Current;
			}
		}
		while (secondIterator.MoveNext())
		{
			yield return secondIterator.Current;
		}
	}

	public static IEnumerable<T> Interleave<T>(this IEnumerable<T> first, IEnumerable<IEnumerable<T>> others)
	{
		IEnumerable<IEnumerator<T>> enumerators = (from s in new IEnumerable<T>[1] { first }.Concat(others)
			select s.GetEnumerator()).ToArray();
		while (enumerators.All((IEnumerator<T> en) => en.MoveNext()))
		{
			foreach (IEnumerator<T> item in enumerators)
			{
				yield return item.Current;
			}
		}
		foreach (IEnumerator<T> item2 in enumerators)
		{
			item2.Dispose();
		}
	}

	public static int LastIndexOf<T>(this IEnumerable<T> items, T item)
	{
		try
		{
			return items.LastIndexOf((T i) => EqualityComparer<T>.Default.Equals(item, i));
		}
		catch (ArgumentNullException)
		{
			throw;
		}
	}

	public static int LastIndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		int num = 0;
		int result = -1;
		foreach (T item in items)
		{
			if (predicate(item))
			{
				result = num;
			}
			num++;
		}
		return result;
	}

	public static T Median<T>(this IEnumerable<T> source) where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
	{
		if (source == null)
		{
			throw new ArgumentNullException("Source sequence cannot be null");
		}
		if (Nullable.GetUnderlyingType(typeof(T)) != null)
		{
			source = source.Where((T x) => x != null);
		}
		T[] array = source.OrderBy((T n) => n).ToArray();
		int num = array.Length;
		if (num == 0)
		{
			throw new ArgumentException("Source sequence cannot be empty or contain null items only");
		}
		int num2 = num >> 1;
		if ((num & 1) == 0)
		{
			T val = array.ElementAt(num2 - 1);
			T arg = array.ElementAt(num2);
			T arg2 = GenericOperator<T, T, T>.Subtract(arg, val);
			T arg3 = GenericOperator<T, T, T>.Increment(GenericOperator<T, T, T>.Increment(GenericOperator<T, T, T>.Zero));
			T arg4 = GenericOperator<T, T, T>.Divide(arg2, arg3);
			return GenericOperator<T, T, T>.Add(val, arg4);
		}
		return array.ElementAt(num2);
	}

	public static Range<T> MinMax<T>(this IEnumerable<T> source) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
	{
		if (source == null)
		{
			throw new ArgumentNullException("Source sequence cannot be null");
		}
		using IEnumerator<T> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return Range<T>.Empty;
		}
		T val = enumerator.Current;
		T val2 = val;
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (current.CompareTo(val) < 0)
			{
				val = current;
			}
			if (current.CompareTo(val2) > 0)
			{
				val2 = current;
			}
		}
		return new Range<T>(val, val2);
	}

	public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int chunkSize)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (chunkSize < 1)
		{
			throw new ArgumentException(string.Format("{0} is {1} while it should be >= 1", "chunkSize", chunkSize));
		}
		List<T> list = new List<T>(chunkSize);
		foreach (T item in source)
		{
			list.Add(item);
			if (list.Count == chunkSize)
			{
				yield return list;
				list = new List<T>(chunkSize);
			}
		}
		if (list.Any())
		{
			yield return list;
		}
	}

	public static IEnumerable<TResult> SelectPair<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.SelectPairImpl(selector);
	}

	private static IEnumerable<TResult> SelectPairImpl<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> selector)
	{
		using IEnumerator<TSource> iterator = source.GetEnumerator();
		if (iterator.MoveNext())
		{
			TSource current = iterator.Current;
			while (iterator.MoveNext())
			{
				yield return selector(current, iterator.Current);
				current = iterator.Current;
			}
			yield break;
		}
	}

	public static IEnumerable<TResult> SelectManyPair<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, IEnumerable<TResult>> selector)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		return source.SelectManyPairImpl(selector);
	}

	private static IEnumerable<TResult> SelectManyPairImpl<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, IEnumerable<TResult>> selector)
	{
		using IEnumerator<TSource> iterator = source.GetEnumerator();
		if (!iterator.MoveNext())
		{
			yield break;
		}
		TSource current = iterator.Current;
		while (iterator.MoveNext())
		{
			foreach (TResult item in selector(current, iterator.Current))
			{
				yield return item;
			}
			current = iterator.Current;
		}
	}

	public static double StdDev(this IEnumerable<double> values)
	{
		double result = 0.0;
		int num = values.Count();
		if (num > 1)
		{
			double avg = values.Average();
			result = Math.Sqrt(values.Sum((double d) => (d - avg) * (d - avg)) / (double)(num - 1));
		}
		return result;
	}

	public static T[] ToArray<T>(this IEnumerable<T> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count cannot be lower than zero");
		}
		T[] array = new T[count];
		int num = 0;
		try
		{
			foreach (T item in source)
			{
				array[num++] = item;
			}
		}
		catch (IndexOutOfRangeException)
		{
			throw new ArgumentException(string.Format("Source has {0} elements while {1} is {2}", source.Count(), "count", count));
		}
		if (num < count)
		{
			throw new ArgumentException(string.Format("Source has {0} elements while {1} is {2}", num, "count", count));
		}
		return array;
	}

	public static List<T> ToList<T>(this IEnumerable<T> source, int count)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count cannot be lower than zero");
		}
		List<T> list = new List<T>(count);
		foreach (T item in source)
		{
			list.Add(item);
		}
		int count2 = list.Count;
		if (count2 < count)
		{
			throw new ArgumentException(string.Format("Source has {0} elements while {1} is {2}", source.Count(), "count", count));
		}
		if (count2 > count)
		{
			throw new ArgumentException(string.Format("Source has {0} elements while {1} is {2}", count2, "count", count));
		}
		return list;
	}

	public static IEnumerable<T> WherePair<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		return source.WherePairImpl(predicate);
	}

	private static IEnumerable<T> WherePairImpl<T>(this IEnumerable<T> source, Func<T, T, bool> predicate)
	{
		using IEnumerator<T> iterator = source.GetEnumerator();
		if (!iterator.MoveNext())
		{
			yield break;
		}
		T current = iterator.Current;
		if (iterator.MoveNext() && predicate(current, iterator.Current))
		{
			yield return current;
			yield return iterator.Current;
			current = iterator.Current;
		}
		while (iterator.MoveNext())
		{
			if (predicate(current, iterator.Current))
			{
				yield return iterator.Current;
			}
			current = iterator.Current;
		}
	}

	public static IEnumerable<T> Zip<T>(this IEnumerable<IEnumerable<T>> source, Func<IEnumerable<T>, T> resultSelector)
	{
		IEnumerator<T>[] enumerators = source.Select((IEnumerable<T> s) => s.GetEnumerator()).ToArray();
		while (enumerators.All((IEnumerator<T> en) => en.MoveNext()))
		{
			yield return resultSelector(enumerators.Select((IEnumerator<T> en) => en.Current));
		}
		IEnumerator<T>[] array = enumerators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Dispose();
		}
	}

	public static IEnumerable<T> Zip<T>(this IEnumerable<T> first, IEnumerable<IEnumerable<T>> others, Func<IEnumerable<T>, T> resultSelector)
	{
		IEnumerator<T>[] enumerators = (from s in new IEnumerable<T>[1] { first }.Concat(others)
			select s.GetEnumerator()).ToArray();
		while (enumerators.All((IEnumerator<T> en) => en.MoveNext()))
		{
			yield return resultSelector(enumerators.Select((IEnumerator<T> en) => en.Current));
		}
		IEnumerator<T>[] array = enumerators;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Dispose();
		}
	}

	public static int IndexOf<T>(this IList<T> items, Func<T, bool> predicate)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		for (int i = 0; i < items.Count; i++)
		{
			if (predicate(items[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static int LastIndexOf<T>(this IList<T> items, T item)
	{
		try
		{
			return items.LastIndexOf((T i) => EqualityComparer<T>.Default.Equals(item, i));
		}
		catch (ArgumentNullException)
		{
			throw;
		}
	}

	public static int LastIndexOf<T>(this IList<T> items, Func<T, bool> predicate)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		if (predicate == null)
		{
			throw new ArgumentNullException("predicate");
		}
		for (int num = items.Count - 1; num >= 0; num--)
		{
			if (predicate(items[num]))
			{
				return num;
			}
		}
		return -1;
	}

	public static T Median<T>(this IList<T> source) where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
	{
		if (source == null)
		{
			throw new ArgumentNullException("Source sequence cannot be null");
		}
		T[] array = ((!(Nullable.GetUnderlyingType(typeof(T)) != null)) ? source.OrderBy((T n) => n).ToArray(source.Count) : (from x in source
			where x != null
			select x into n
			orderby n
			select n).ToArray());
		int num = array.Length;
		if (num == 0)
		{
			throw new ArgumentException("Source sequence cannot be empty or contain null items only");
		}
		int num2 = num >> 1;
		if ((num & 1) == 0)
		{
			T val = array[num2 - 1];
			T arg = array[num2];
			T arg2 = GenericOperator<T, T, T>.Subtract(arg, val);
			T arg3 = GenericOperator<T, T, T>.Increment(GenericOperator<T, T, T>.Increment(GenericOperator<T, T, T>.Zero));
			T arg4 = GenericOperator<T, T, T>.Divide(arg2, arg3);
			return GenericOperator<T, T, T>.Add(val, arg4);
		}
		return array[num2];
	}

	public static Range<T> MinMax<T>(this IList<T> source) where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
	{
		if (source == null)
		{
			throw new ArgumentNullException("Source sequence cannot be null");
		}
		if (source.Count == 0)
		{
			return Range<T>.Empty;
		}
		T val = source[0];
		T val2 = val;
		for (int i = 1; i < source.Count; i++)
		{
			T val3 = source[i];
			if (val3.CompareTo(val) < 0)
			{
				val = val3;
			}
			if (val3.CompareTo(val2) > 0)
			{
				val2 = val3;
			}
		}
		return new Range<T>(val, val2);
	}

	public static string ToFileSizeString(this long fileSize)
	{
		double num = fileSize;
		double val = Math.Floor(((num > 0.0) ? Math.Log10(num) : 0.0) * log1024inv);
		val = Math.Min(val, fileSizeUnits.Length - 1);
		double num2 = num * Math.Pow(1024.0, 0.0 - val);
		int num3 = (int)val;
		int num4 = ((!(num2 >= 10.0)) ? ((num2 >= 2.0) ? 1 : 2) : 0);
		string text;
		if (num3 == 0 || num4 == 0)
		{
			text = num2.ToString("F0");
		}
		else
		{
			char[] trimChars = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToCharArray();
			text = num2.ToString($"F{num4}").TrimEnd(new char[1] { '0' }).TrimEnd(trimChars);
		}
		return text + " " + fileSizeUnits[num3];
	}

	public static string[] FastSplit(this string myString, char separator, StringSplitOptions options = StringSplitOptions.None)
	{
		if (myString.Length == 0)
		{
			return new string[0];
		}
		int[] sepList = new int[myString.Length];
		int numReplaces = MakeSeparatorList(myString, separator, ref sepList);
		if (options == StringSplitOptions.RemoveEmptyEntries)
		{
			return InternalSplitOmitEmptyEntries(myString, sepList, null, numReplaces, 10000);
		}
		return InternalSplitKeepEmptyEntries(myString, sepList, null, numReplaces, 10000);
	}

	public static int? ToInt32Suffix(this string myString)
	{
		if (!int.TryParse(new string(myString.Reverse().ToArray().TakeWhile((char c) => char.IsDigit(c))
			.Reverse()
			.ToArray()), out var result))
		{
			return null;
		}
		return result;
	}

	public static string[] FastSplit(this string myString, string separator, StringSplitOptions options = StringSplitOptions.None)
	{
		int[] sepList = new int[myString.Length];
		int[] lengthList = new int[myString.Length];
		int numReplaces = MakeSeparatorList(myString, separator, ref sepList, ref lengthList);
		if (options == StringSplitOptions.RemoveEmptyEntries)
		{
			return InternalSplitOmitEmptyEntries(myString, sepList, lengthList, numReplaces, 10000);
		}
		return InternalSplitKeepEmptyEntries(myString, sepList, lengthList, numReplaces, 10000);
	}

	public static string RemoveInvalidFileNameCharacters(this string myString)
	{
		string text = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
		for (int i = 0; i < text.Length; i++)
		{
			myString = myString.Replace(text[i].ToString(), "");
		}
		myString = myString.Replace(".", "");
		return myString;
	}

	private unsafe static int MakeSeparatorList(string myString, char separator, ref int[] sepList)
	{
		int num = 0;
		int num2 = sepList.Length;
		int length = myString.Length;
		fixed (char* ptr2 = myString)
		{
			for (int i = 0; i < length; i++)
			{
				if (num >= num2)
				{
					break;
				}
				char* ptr = &separator;
				if (ptr2[i] == *ptr)
				{
					sepList[num++] = i;
				}
			}
		}
		return num;
	}

	private unsafe static int MakeSeparatorList(string myString, string separator, ref int[] sepList, ref int[] lengthList)
	{
		int num = 0;
		int num2 = sepList.Length;
		int length = myString.Length;
		fixed (char* ptr = myString)
		{
			for (int i = 0; i < length; i++)
			{
				if (num >= num2)
				{
					break;
				}
				if (!string.IsNullOrEmpty(separator))
				{
					int length2 = separator.Length;
					if (ptr[i] == separator[0] && length2 <= myString.Length - i && (length2 == 1 || string.CompareOrdinal(myString, i, separator, 0, length2) == 0))
					{
						sepList[num] = i;
						lengthList[num] = length2;
						num++;
						i += length2 - 1;
						break;
					}
				}
			}
		}
		return num;
	}

	private static string[] InternalSplitKeepEmptyEntries(string myString, int[] sepList, int[] lengthList, int numReplaces, int count)
	{
		int num = 0;
		int num2 = 0;
		count--;
		int num3 = ((numReplaces < count) ? numReplaces : count);
		string[] array = new string[num3 + 1];
		for (int i = 0; i < num3; i++)
		{
			if (num >= myString.Length)
			{
				break;
			}
			array[num2++] = myString.Substring(num, sepList[i] - num);
			num = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
		}
		if (num < myString.Length && num3 >= 0)
		{
			array[num2] = myString.Substring(num);
		}
		else if (num2 == num3)
		{
			array[num2] = string.Empty;
		}
		return array;
	}

	private static string[] InternalSplitOmitEmptyEntries(string myString, int[] sepList, int[] lengthList, int numReplaces, int count)
	{
		int num = ((numReplaces < count) ? (numReplaces + 1) : count);
		string[] array = new string[num];
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < numReplaces; i++)
		{
			if (num2 >= myString.Length)
			{
				break;
			}
			if (sepList[i] - num2 > 0)
			{
				array[num3++] = myString.Substring(num2, sepList[i] - num2);
			}
			num2 = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
			if (num3 == count - 1)
			{
				while (i < numReplaces - 1 && num2 == sepList[++i])
				{
					num2 += ((lengthList == null) ? 1 : lengthList[i]);
				}
				break;
			}
		}
		if (num2 < myString.Length)
		{
			array[num3++] = myString.Substring(num2);
		}
		string[] array2 = array;
		if (num3 != num)
		{
			array2 = new string[num3];
			for (int j = 0; j < num3; j++)
			{
				array2[j] = array[j];
			}
		}
		return array2;
	}

	public static string GetTextAsDefaultCulture(this string text)
	{
		if (!string.Equals(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."))
		{
			text = text.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, ".");
		}
		if (!string.Equals(CultureInfo.CurrentCulture.TextInfo.ListSeparator, ","))
		{
			text = text.Replace(CultureInfo.CurrentCulture.TextInfo.ListSeparator, ",");
		}
		return text;
	}

	public static string FirstLetterToUpperCase(this string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
		{
			return char.ToUpper(text[0]) + text.Substring(1);
		}
		return text;
	}

	public static int CompareTo(this Version version, Version otherVersion, int significantParts)
	{
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		if (otherVersion == null)
		{
			return 1;
		}
		if (version.Major != otherVersion.Major && significantParts >= 1)
		{
			if (version.Major > otherVersion.Major)
			{
				return 1;
			}
			return -1;
		}
		if (version.Minor != otherVersion.Minor && significantParts >= 2)
		{
			if (version.Minor > otherVersion.Minor)
			{
				return 1;
			}
			return -1;
		}
		if (version.Build != otherVersion.Build && significantParts >= 3)
		{
			if (version.Build > otherVersion.Build)
			{
				return 1;
			}
			return -1;
		}
		if (version.Revision != otherVersion.Revision && significantParts >= 4)
		{
			if (version.Revision > otherVersion.Revision)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}

	public static void Raise(this PropertyChangedEventHandler handler, object sender, [CallerMemberName] string propertyName = null, bool beginInvoke = false)
	{
		if (beginInvoke)
		{
			handler?.BeginInvoke(sender, new PropertyChangedEventArgs(propertyName), null, null);
		}
		else
		{
			handler?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
		}
	}

	[Obsolete("Use nameof instead")]
	public static bool IsPropertyEqual<T>(this PropertyChangedEventArgs args, T propertyOwner, Expression<Func<T, object>> propertyExpression)
	{
		return args.PropertyName == GetPropertyName(propertyExpression);
	}

	private static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
	{
		if (propertyExpression == null)
		{
			throw new ArgumentNullException("propertyExpression should not be null", "propertyExpression");
		}
		MemberExpression memberExpression = propertyExpression.Body as MemberExpression;
		if (memberExpression == null)
		{
			memberExpression = ((propertyExpression.Body as UnaryExpression) ?? throw new ArgumentException("The expression should be a member or unary expression", "propertyExpression")).Operand as MemberExpression;
		}
		if (fullPropertyCheck)
		{
			PropertyInfo obj = memberExpression.Member as PropertyInfo;
			if (obj == null)
			{
				throw new ArgumentException("The expression does not access a property.", "propertyExpresssion");
			}
			if (obj.GetGetMethod(nonPublic: true)!.IsStatic)
			{
				throw new ArgumentException("The referenced property is a static property.", "propertyExpresssion");
			}
		}
		return memberExpression.Member.Name;
	}
}
