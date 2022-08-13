using System.Globalization;

namespace Racelogic.DataSource;

public class UnitsGlobal
{
	private static CultureInfo currentCulture;

	public static CultureInfo CurrentCulture
	{
		get
		{
			if (currentCulture == null)
			{
				currentCulture = CultureInfo.CurrentCulture;
			}
			return currentCulture;
		}
	}

	public static void RefreshCulture()
	{
		currentCulture = null;
	}
}
