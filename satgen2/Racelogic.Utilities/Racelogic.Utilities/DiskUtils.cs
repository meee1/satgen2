using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace Racelogic.Utilities;

public static class DiskUtils
{
	public static IEnumerable<DriveInfo> GetDrives(DriveType driveType, bool containingMedia = false)
	{
		try
		{
			return from d in DriveInfo.GetDrives()
				where d.DriveType == driveType && (!containingMedia || (d.DriveType != DriveType.Removable && d.DriveType != DriveType.Network) || d.IsReady)
				select d;
		}
		catch (UnauthorizedAccessException)
		{
			return Array.Empty<DriveInfo>();
		}
		catch (IOException)
		{
			return Array.Empty<DriveInfo>();
		}
	}

	public static bool CanWriteToFile(string fileName)
	{
		if (!File.Exists(fileName))
		{
			return true;
		}
		try
		{
			using (new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			{
			}
			return true;
		}
		catch (IOException)
		{
		}
		catch (SecurityException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
		return false;
	}

	public static bool CanWriteToFile(string fileName, out string errorMessage)
	{
		if (!File.Exists(fileName))
		{
			errorMessage = null;
			return true;
		}
		try
		{
			using (new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
			{
			}
			errorMessage = null;
			return true;
		}
		catch (IOException ex)
		{
			errorMessage = ex.Message;
		}
		catch (SecurityException ex2)
		{
			errorMessage = ex2.Message;
		}
		catch (UnauthorizedAccessException ex3)
		{
			errorMessage = ex3.Message;
		}
		return false;
	}
}
