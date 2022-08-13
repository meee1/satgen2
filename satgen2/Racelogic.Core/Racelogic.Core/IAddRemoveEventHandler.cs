using System;

namespace Racelogic.Core;

public interface IAddRemoveEventHandler
{
	void AddHandler(EventHandler value);

	void RemoveHandler(EventHandler value);
}
