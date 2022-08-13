using System;

namespace Racelogic.DataSource;

public interface IRacelogicData : IEquatable<IRacelogicData>, IComparable, IComparable<IRacelogicData>, IConvertible
{
	string ToString(string format);
}
