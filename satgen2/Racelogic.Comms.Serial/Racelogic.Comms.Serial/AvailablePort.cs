using Racelogic.Utilities;

namespace Racelogic.Comms.Serial;

public class AvailablePort : BasePropertyChanged
{
	private string name;

	private bool isAvailable;

	private string description;

	private bool isBusy;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (!string.Equals(name, value))
			{
				name = value;
			}
		}
	}

	public bool IsAvailable
	{
		get
		{
			return isAvailable;
		}
		set
		{
			isAvailable = value;
		}
	}

	public string Description
	{
		get
		{
			return description;
		}
		set
		{
			if (!string.Equals(description, value))
			{
				description = value;
			}
		}
	}

	public string NameAndDescription => ToString();

	public bool IsBusy
	{
		get
		{
			return isBusy;
		}
		internal set
		{
			isBusy = value;
		}
	}

	public AvailablePort(string portName, string description = "")
	{
		name = portName;
		this.description = description;
		isAvailable = false;
		isBusy = true;
	}

	public override string ToString()
	{
		return string.IsNullOrEmpty(description) ? name : description;
	}
}
