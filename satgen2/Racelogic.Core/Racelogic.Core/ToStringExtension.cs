using System.Text.RegularExpressions;

namespace Racelogic.Core;

public static class ToStringExtension
{
	public static string ToString(this bool value, BooleanText text)
	{
		MatchCollection matchCollection = Regex.Matches(text.ToString(), "[A-Z][a-z]+");
		return value.ToString(Resources.ResourceManager.GetString(matchCollection[0].Value), Resources.ResourceManager.GetString(matchCollection[1].Value));
	}

	public static string ToString(this bool value, string trueValue, string falseValue)
	{
		if (!value)
		{
			return falseValue;
		}
		return trueValue;
	}
}
