namespace Racelogic.DataSource;

public static class FormatOptions
{
	private static CurrentOptions instance;

	public static CurrentOptions Instance
	{
		get
		{
			return instance ?? (instance = new CurrentOptions());
		}
		set
		{
			instance = value;
		}
	}
}
