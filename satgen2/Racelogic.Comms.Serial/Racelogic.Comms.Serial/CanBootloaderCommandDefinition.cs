namespace Racelogic.Comms.Serial;

internal class CanBootloaderCommandDefinition
{
	internal int Identifier { get; private set; }

	internal byte[] Data { get; set; }

	internal CanBootloaderCommandDefinition(int identifier, byte[] data)
	{
		Identifier = identifier;
		Data = new byte[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			Data[i] = data[i];
		}
	}
}
