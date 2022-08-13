using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Racelogic.Core;

public static class StructureFunctions
{
	public static object Create(byte[] data, Type type)
	{
		GCHandle gCHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
		object? result = Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), type);
		gCHandle.Free();
		return result;
	}

	public static byte[] StructureToByteArray(object o)
	{
		int num = Marshal.SizeOf(o);
		byte[] array = new byte[num];
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		Marshal.StructureToPtr(o, intPtr, fDeleteOld: true);
		Marshal.Copy(intPtr, array, 0, num);
		Marshal.FreeHGlobal(intPtr);
		return array;
	}

	public static byte[] StringToByteArray(string value, uint length, bool nullTerminated = true)
	{
		byte[] array = new byte[length];
		byte[] array2 = new byte[0];
		if (!string.IsNullOrEmpty(value))
		{
			array2 = new UnicodeEncoding().GetBytes(value);
		}
		for (int i = 0; i < length; i++)
		{
			if (i * 2 < array2.Length)
			{
				array[i] = array2[i * 2];
			}
			else
			{
				array[i] = 0;
			}
		}
		if (nullTerminated)
		{
			array[length - 1] = 0;
		}
		return array;
	}

	public static string ByteArrayToString(byte[] value)
	{
		string @string = new UTF7Encoding(allowOptionals: true).GetString(value);
		StringBuilder stringBuilder = new StringBuilder();
		string text = @string;
		foreach (char c in text)
		{
			if (c == '\0')
			{
				break;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}
}
