namespace Racelogic.Core.Filters;

public interface IRealTimeSmoother
{
	double? GetNextValue(double? val);
}
