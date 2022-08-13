namespace Racelogic.Comms.Serial;

public class StrErrorDefinition
{
	public readonly string Error;

	public readonly string Description;

	public StrErrorDefinition(string error, string description)
	{
		Error = error;
		Description = description;
	}
}
