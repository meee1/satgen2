namespace Racelogic.DataSource;

public class DataNowAndThen
{
	public readonly double Now;

	public readonly double Old;

	public DataNowAndThen(double Now, double Old)
	{
		this.Now = Now;
		this.Old = Old;
	}
}
