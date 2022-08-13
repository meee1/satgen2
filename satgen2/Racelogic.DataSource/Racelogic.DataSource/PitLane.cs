using System.Runtime.InteropServices;
using System.Text;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

[ClassInterface(ClassInterfaceType.None)]
public class PitLane : BasePropertyChanged
{
	private SplitPoint start;

	private SplitPoint finish;

	public string Description
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(start.Description);
			if (finish != null)
			{
				stringBuilder.AppendLine(finish.Description);
			}
			return stringBuilder.ToString();
		}
	}

	public SplitPoint Finish
	{
		get
		{
			return finish;
		}
		set
		{
			finish = new SplitPoint(value);
			OnPropertyChanged("Finish");
		}
	}

	public bool IsValid => start != null;

	public SplitPoint Start
	{
		get
		{
			return start;
		}
		set
		{
			start = new SplitPoint(value);
			OnPropertyChanged("Start");
		}
	}

	public PitLane(SplitPoint start, SplitPoint finish)
	{
		if (start != null)
		{
			start.Type = SplitType.PitLaneStart;
			this.start = new SplitPoint(start);
		}
		if (finish != null)
		{
			finish.Type = SplitType.PitLaneFinish;
			this.finish = new SplitPoint(finish);
		}
	}

	public override string ToString()
	{
		return Description;
	}
}
