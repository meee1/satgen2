using System;

namespace Racelogic.Comms.Serial;

internal class SlidingMessageEventArgs : EventArgs
{
	internal readonly string Title;

	internal readonly string Text;

	internal SlidingMessageEventArgs(string title, string text)
	{
		Title = title;
		Text = text;
	}
}
