namespace Racelogic.DataSource;

public interface ISmoothing
{
	uint SmoothBy { get; }

	double SmoothedValue { get; }

	double GetSmoothedValue(double rawValue);

	void Reset();
}
