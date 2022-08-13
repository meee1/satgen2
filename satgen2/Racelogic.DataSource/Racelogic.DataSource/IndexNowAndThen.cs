namespace Racelogic.DataSource;

public class IndexNowAndThen
{
	public readonly int Now;

	public readonly int Old;

	public IndexNowAndThen(int Now, int Old)
	{
		this.Now = Now;
		this.Old = Old;
	}
}
