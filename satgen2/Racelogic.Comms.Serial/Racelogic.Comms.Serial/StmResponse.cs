namespace Racelogic.Comms.Serial;

internal static class StmResponse
{
	internal const byte TxFail = byte.MaxValue;

	internal const byte NoResponse = 0;

	internal const byte Ack = 121;

	internal const byte Nack = 31;
}
