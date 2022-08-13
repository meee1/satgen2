using System.Configuration;

namespace Racelogic.Comms.Serial;

public class UserSettings : ApplicationSettingsBase
{
	[UserScopedSetting]
	[DefaultSettingValue("true")]
	public bool CallUpgrade
	{
		get
		{
			return (bool)this["CallUpgrade"];
		}
		private set
		{
			this["CallUpgrade"] = value;
		}
	}

	[UserScopedSetting]
	[DefaultSettingValue("115200")]
	public int BaudRate
	{
		get
		{
			return (int)this["BaudRate"];
		}
		set
		{
			this["BaudRate"] = value;
		}
	}

	internal UserSettings()
	{
		if (CallUpgrade)
		{
			Upgrade();
			CallUpgrade = false;
		}
	}

	~UserSettings()
	{
		Save();
	}
}
