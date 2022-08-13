namespace Racelogic.Comms.Serial;

public struct StrCommandDefinition
{
	public readonly byte Command;

	public readonly byte RetryCount;

	public readonly short? Length;

	public readonly short? ResponseLength;

	public StrCommandDefinition(byte command, byte retryCount, short? length, short? responseLength)
	{
		Command = command;
		RetryCount = retryCount;
		Length = length;
		ResponseLength = responseLength;
	}
}
