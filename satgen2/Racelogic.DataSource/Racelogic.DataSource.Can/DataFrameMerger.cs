using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Racelogic.DataSource.Can;

internal class DataFrameMerger
{
	public List<DataFrame> GenerateMergedCollection(List<DataFrame> frames, DataFrame newFrame)
	{
		DataFrame matchingFrame = frames.FirstOrDefault((DataFrame x) => x.Equals(newFrame));
		if (matchingFrame == null)
		{
			return new List<DataFrame>(frames) { newFrame };
		}
		return new List<DataFrame>(frames.Where((DataFrame f) => f != matchingFrame)) { MergeFrame(matchingFrame, newFrame) };
	}

	public DataFrame MergeFrame(DataFrame baseFrame, DataFrame modifiedFrame, bool preserveOriginalName = true)
	{
		DataFrame outputFrame = new DataFrame(modifiedFrame.ContainedSignals.ToArray());
		outputFrame.ContainedSignals = new ObservableCollection<IDataSignal>(modifiedFrame.ContainedSignals.Concat(baseFrame.ContainedSignals.Where((IDataSignal x) => !outputFrame.ContainedSignals.Contains(x))));
		if (outputFrame.Name == string.Empty || !preserveOriginalName || !(baseFrame.Name != string.Empty))
		{
			outputFrame.Name = baseFrame.Name;
		}
		return outputFrame;
	}
}
