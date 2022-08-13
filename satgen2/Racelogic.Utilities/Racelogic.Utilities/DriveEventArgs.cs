using System;
using System.Collections.Generic;
using System.IO;

namespace Racelogic.Utilities;

public class DriveEventArgs : EventArgs
{
	private readonly IEnumerable<DriveInfo> drives;

	public IEnumerable<DriveInfo> Drives => drives;

	public DriveEventArgs(IEnumerable<DriveInfo> drives)
	{
		this.drives = drives;
	}
}
