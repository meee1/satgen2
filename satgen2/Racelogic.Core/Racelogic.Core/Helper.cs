using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Racelogic.Core;

public static class Helper
{
	public enum RemovingTrailingNull
	{
		Yes,
		No
	}

	public static string ByteArrayToString(byte[] data)
	{
		return ByteArrayToString(data, RemovingTrailingNull.No);
	}

	public static string ByteArrayToString(byte[] data, RemovingTrailingNull removeTrailingNull)
	{
		if (removeTrailingNull == RemovingTrailingNull.Yes)
		{
			return new UTF7Encoding(allowOptionals: true).GetString(data).TrimEnd(new char[1]);
		}
		return new UTF7Encoding(allowOptionals: true).GetString(data);
	}

	public static byte[] StringToByteArray(string data)
	{
		byte[] array = new byte[0];
		if (data != null)
		{
			array = new UnicodeEncoding().GetBytes(data);
		}
		List<byte> list = new List<byte>();
		for (int i = 0; i < array.Length; i++)
		{
			if (i % 2 == 0)
			{
				list.Add(array[i]);
			}
		}
		return list.ToArray();
	}

	public static bool CheckForInternetConnection()
	{
		bool flag = false;
		try
		{
			using WebClient webClient = new WebClient();
			using (webClient.OpenRead("http://www.racelogic.co.uk/Updates/Catalogue.xml"))
			{
				flag = true;
			}
		}
		catch
		{
			flag = false;
		}
		if (!flag)
		{
			try
			{
				using WebClient webClient2 = new WebClient();
				using (webClient2.OpenRead("http://www.google.com"))
				{
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
		return flag;
	}

	public static double GetGrahicsScore()
	{
		try
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Performance\\WinSAT\\DataStore";
			if (Directory.Exists(path))
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(path);
				DateTime dateTime = default(DateTime);
				string url = string.Empty;
				FileInfo[] files = directoryInfo.GetFiles();
				foreach (FileInfo fileInfo in files)
				{
					if (fileInfo.LastAccessTime >= dateTime)
					{
						dateTime = fileInfo.LastAccessTime;
						url = fileInfo.FullName;
					}
				}
				XmlTextReader xmlTextReader = new XmlTextReader(url);
				try
				{
					xmlTextReader.WhitespaceHandling = WhitespaceHandling.None;
					while (xmlTextReader.Read())
					{
						try
						{
							if (xmlTextReader.Name == "GraphicsScore")
							{
								return Convert.ToDouble(xmlTextReader.ReadElementContentAsString());
							}
						}
						catch
						{
						}
					}
				}
				finally
				{
					xmlTextReader.Close();
				}
				return -1.0;
			}
		}
		catch
		{
		}
		return -1.0;
	}

	public static bool IsSerialNumberValid(uint serialNumber, out string message)
	{
		bool result = true;
		message = string.Empty;
		if (serialNumber >= 990 && serialNumber <= 999)
		{
			result = false;
			message = "Serial numbers 990 - 999 are reserved." + Environment.NewLine + "Please change value.";
		}
		if (serialNumber >= 30000 && serialNumber <= 30010)
		{
			result = false;
			message = "Serial numbers 30000 - 30010 are reserved." + Environment.NewLine + "Please change value.";
		}
		if (serialNumber >= 99990 && serialNumber <= 99999)
		{
			result = false;
			message = "Serial numbers 99990 - 99999 are reserved." + Environment.NewLine + "Please change value.";
		}
		if (serialNumber == 99990)
		{
			result = false;
			message = "Serial number 99990 is reserved." + Environment.NewLine + "Please change value.";
		}
		if (serialNumber >= 262100 && serialNumber <= 262117)
		{
			result = false;
			message = "Serial numbers 262100 - 262117 are reserved." + Environment.NewLine + "Please change value.";
		}
		return result;
	}

	public static bool IsValidEmail(string strIn)
	{
		if (string.IsNullOrEmpty(strIn))
		{
			return false;
		}
		try
		{
			return Regex.IsMatch(strIn, "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$", RegexOptions.IgnoreCase);
		}
		catch
		{
			return false;
		}
	}

	public static ExcelVersion GetExcelVersion()
	{
		ExcelVersion excelVersion = ExcelVersion.None;
		try
		{
			foreach (ExcelVersion item in from ExcelVersion i in Enum.GetValues(typeof(ExcelVersion))
				where i != 0 && i != ExcelVersion.Unknown
				select i)
			{
				if (Type.GetTypeFromProgID($"Excel.Application.{(int)item}") != null)
				{
					excelVersion = item;
					break;
				}
			}
			if (excelVersion == ExcelVersion.None)
			{
				if (Type.GetTypeFromProgID("Excel.Application") != null)
				{
					excelVersion = ExcelVersion.Unknown;
					return excelVersion;
				}
				return excelVersion;
			}
			return excelVersion;
		}
		catch
		{
			return excelVersion;
		}
	}

	public static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
	{
		while (toCheck != null && toCheck != typeof(object))
		{
			Type type = (toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck);
			if (generic == type)
			{
				return true;
			}
			toCheck = toCheck.BaseType;
		}
		return false;
	}

	public static Version GetRacelogicModuleFirmwareVersion(sbyte[] eepromVersionString)
	{
		ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
		byte[] array = new byte[eepromVersionString.Length];
		for (int i = 0; i < eepromVersionString.Length; i++)
		{
			array[i] = (byte)eepromVersionString[i];
			if (array[i] == 0)
			{
				break;
			}
		}
		string[] array2 = aSCIIEncoding.GetString(array).Split(new char[1] { ' ' });
		string text = string.Empty;
		string text2 = string.Empty;
		for (int j = 0; j < array2.Length; j++)
		{
			double result5;
			if (array2[j].StartsWith("v", StringComparison.OrdinalIgnoreCase))
			{
				if (string.Compare(array2[j], "version", ignoreCase: true) == 0)
				{
					string[] array3 = array2[j + 1].Split(new char[1] { '.' });
					int result = 0;
					int result2 = 0;
					int num = 0;
					if (array3.Length == 2 && int.TryParse(array3[0], out result))
					{
						array3[1] = array3[1].TrimEnd(new char[1]);
						if (array3[1].Length == 2 && int.TryParse(array3[1].Substring(0, 1), out result2))
						{
							array = aSCIIEncoding.GetBytes(new char[1] { array3[1][1] });
							num = ((array[0] > 96) ? (array[0] - 96) : (array[0] - 64));
							text = $"{result}.{result2}.{num}";
						}
					}
					continue;
				}
				if (array2[j].StartsWith("Ver.", StringComparison.OrdinalIgnoreCase))
				{
					string[] array4 = array2[j].Remove(0, 4).Split(new char[1] { '.' });
					if (array4.Length == 2)
					{
						int result3 = 0;
						int result4 = 0;
						if (int.TryParse(array4[0], out result3) && int.TryParse(array4[1], out result4))
						{
							text = $"{result3}.{result4}";
						}
					}
					continue;
				}
				array2[j] = array2[j].Remove(0, 1);
				if (double.TryParse(array2[j], out result5))
				{
					text = array2[j];
				}
				else
				{
					if (!array2[j].Contains("b."))
					{
						continue;
					}
					array2 = array2[j].Split(new char[1] { 'b' });
					if (double.TryParse(array2[0], out result5))
					{
						text = array2[0];
						if (double.TryParse(array2[1].Substring(1), out result5))
						{
							text2 = array2[1].Substring(1);
						}
					}
				}
			}
			else if ((string.Equals("build", array2[j], StringComparison.OrdinalIgnoreCase) || string.Equals("bld.", array2[j], StringComparison.OrdinalIgnoreCase) || string.Equals("bld", array2[j], StringComparison.OrdinalIgnoreCase)) && j < array2.Length - 1 && double.TryParse(array2[j + 1], out result5))
			{
				text2 = array2[j + 1];
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "0";
			}
			text += ".";
			text += text2;
			return new Version(text);
		}
		return new Version(0, 0, 0);
	}
}
