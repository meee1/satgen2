using System;
using System.Threading.Tasks;

namespace Racelogic.Utilities;

public abstract class LogProviderBase
{
	public abstract void LogException(Exception exception);

	public abstract Task Upload();
}
