namespace Racelogic.Utilities.Win;

internal struct NotifyInfos
{
	internal ShellNotifications.SHCNE Notification;

	internal string Item1;

	internal string Item2;

	internal NotifyInfos(ShellNotifications.SHCNE notification)
	{
		Notification = notification;
		Item1 = "";
		Item2 = "";
	}
}
