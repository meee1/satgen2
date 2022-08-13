using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace Racelogic.DataSource;

[ClassInterface(ClassInterfaceType.None)]
public class GlobalComObject : IGlobalStoresCOM
{
	public SplitPoint[] Splits
	{
		get
		{
			return Global.Instance.Splits.ToArray();
		}
		set
		{
			Global.Instance.Splits = new ObservableCollection<SplitPoint>(value);
		}
	}

	public void ClearSplits()
	{
		Global.Instance.Splits.Clear();
	}

	public void AddSplit(ISplitDefinition split)
	{
		Global.Instance.Splits.Add(new SplitPoint(split));
	}
}
