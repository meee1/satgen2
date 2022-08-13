using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Racelogic.Core;

namespace Racelogic.Utilities;

public class ExceptionSerialization : LogProviderBase
{
	private string SiteUrl => "https://softwarefunctions.azurewebsites.net/api/processexceptions";

	private string Location { get; }

	private Assembly Assembly { get; }

	private string Product { get; }

	public ExceptionSerialization()
	{
		try
		{
			Assembly = Assembly.GetEntryAssembly();
			if (!(Assembly == null))
			{
				Product = Assembly.GetName().Name;
				Location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Racelogic", "ExceptionSerialization", Product + ".txt");
				EnsureFolderExists(Location);
			}
		}
		catch
		{
		}
	}

	public override void LogException(Exception exception)
	{
		try
		{
			Archive(Serialize(exception));
		}
		catch
		{
		}
	}

	public override async Task Upload()
	{
		if (!Network.InternetAvailable())
		{
			return;
		}
		try
		{
			string[] source = FormatExceptionArchive(GetArchiveContents());
			if (!source.Any())
			{
				return;
			}
			IEnumerable<IEnumerable<string>> enumerable = Chunkify(source, 100);
			foreach (IEnumerable<string> item in enumerable)
			{
				HttpWebRequest httpWebRequest = CreateWebRequest(SiteUrl);
				if (httpWebRequest == null)
				{
					return;
				}
				Stream stream = httpWebRequest.GetRequestStream();
				JsonSerializer.Create().Serialize(new StreamWriter(stream)
				{
					AutoFlush = true
				}, item);
				HttpWebResponse httpWebResponse = await ((AsyncPolicy<HttpWebResponse>)(object)AsyncRetryTResultSyntax.WaitAndRetryAsync<HttpWebResponse>(Policy<HttpWebResponse>.Handle<ProtocolViolationException>().Or<WebException>(), 3, (Func<int, TimeSpan>)((int x) => TimeSpan.FromSeconds(3.0)))).ExecuteAsync((Func<Task<HttpWebResponse>>)(async () => (await httpWebRequest.GetResponseAsync()) as HttpWebResponse));
				if (httpWebResponse == null || httpWebResponse.StatusCode != HttpStatusCode.OK)
				{
					return;
				}
				stream.Close();
				httpWebResponse.Dispose();
			}
			Reset();
		}
		catch (Exception exception)
		{
			LogException(exception);
		}
	}

	private IEnumerable<IEnumerable<string>> Chunkify(IEnumerable<string> source, int chunkSize)
	{
		while (source.Any())
		{
			yield return source.Take(chunkSize);
			source = source.Skip(chunkSize);
		}
	}

	private string[] FormatExceptionArchive(string[] archiveContents)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<string> list = new List<string>();
		for (int i = 0; i < archiveContents.Length; i++)
		{
			if (archiveContents[i].StartsWith("{") && archiveContents[i].EndsWith("}"))
			{
				list.Add(archiveContents[i]);
			}
			else if (archiveContents[i].StartsWith("{"))
			{
				stringBuilder.Clear();
				stringBuilder.Append(archiveContents[i]);
			}
			else if (archiveContents[i].EndsWith("}"))
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(archiveContents[i]);
					list.Add(stringBuilder.ToString());
				}
				stringBuilder.Clear();
			}
			else if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(archiveContents[i]);
			}
		}
		return list.ToArray();
	}

	private HttpWebRequest CreateWebRequest(string url)
	{
		try
		{
			HttpWebRequest httpWebRequest = WebRequest.CreateHttp(url);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";
			return httpWebRequest;
		}
		catch (Exception exception)
		{
			LogException(exception);
			return null;
		}
	}

	private string[] GetArchiveContents()
	{
		string[] array = null;
		try
		{
			if (File.Exists(Location))
			{
				array = File.ReadAllLines(Location);
			}
		}
		finally
		{
			if (array == null)
			{
				array = new string[0];
			}
		}
		return array;
	}

	private string Serialize(Exception exception)
	{
		if (exception == null)
		{
			return string.Empty;
		}
		string text = string.Empty;
		try
		{
			text = JsonConvert.SerializeObject(ExtractExceptionDetails(exception));
			text = text.Replace(Environment.NewLine, string.Empty).Replace('\n'.ToString(), string.Empty);
			return text;
		}
		catch
		{
			return text;
		}
	}

	private void Archive(string text, string additionalInformation = "")
	{
		try
		{
			text = ExceptionDepersonalization(text);
			string value = JsonConvert.SerializeObject(new
			{
				DateTime = DateTime.UtcNow.ToString("yyyy-MMM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
				Product = Product,
				VersionString = ((!string.IsNullOrEmpty(Assembly.Location)) ? new Version(FileVersionInfo.GetVersionInfo(Assembly.Location).ProductVersion.Replace("*", "0")) : new Version(0, 0, 0, 0)).ToString(),
				Language = Thread.CurrentThread.CurrentUICulture,
				OsVersion = Environment.OSVersion.VersionString,
				OsName = GetOperatingSystemName(),
				Architecture = (Environment.Is64BitOperatingSystem ? "64" : "32"),
				Additional = additionalInformation,
				Exception = text
			});
			StreamWriter streamWriter = File.AppendText(Location);
			streamWriter.WriteLine(value);
			streamWriter.Close();
		}
		catch
		{
		}
	}

	public string ExceptionDepersonalization(string text)
	{
		while (text.Contains("\\\\"))
		{
			text = text.Replace("\\\\", "\\");
		}
		text = text.ToLower();
		text = text.Replace(Environment.UserName.ToLower(), "username");
		if (text.Contains(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToLower()))
		{
			return text.Replace(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToLower(), "username");
		}
		if (text.Contains("users"))
		{
			int i = 0;
			string text2 = "users\\";
			string[] array;
			for (array = text.Split(new string[1] { "\\" }, StringSplitOptions.RemoveEmptyEntries); i < array.Length; i++)
			{
				if (i != array.Length - 1)
				{
					array[i] += "\\";
				}
				if (text2 == array[i])
				{
					i++;
					array[i] = "username\\";
				}
			}
			return string.Concat(array);
		}
		return text;
	}

	private void Reset()
	{
		try
		{
			File.WriteAllText(Location, string.Empty);
		}
		catch
		{
		}
	}

	private void EnsureFolderExists(string path)
	{
		try
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
		}
		catch
		{
		}
	}

	private string GetOperatingSystemName()
	{
		string result = "Unknown";
		try
		{
			result = (from ManagementObject x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get()
				select x.GetPropertyValue("Caption")).FirstOrDefault()?.ToString();
			return result;
		}
		catch
		{
			return result;
		}
	}

	private dynamic ExtractExceptionDetails(Exception e)
	{
		if (e.InnerException != null)
		{
			return ExtractExceptionDetails(e.InnerException);
		}
		(string, string)? classAndLineNumber = GetClassAndLineNumber(e.StackTrace);
		return new
		{
			ExceptionType = e.GetType().Name,
			ExceptionMessage = e.Message,
			ClassName = (classAndLineNumber.HasValue ? classAndLineNumber.Value.Item2 : "null"),
			LineNumber = (classAndLineNumber.HasValue ? classAndLineNumber.Value.Item1 : "0"),
			Exception = e.ToString()
		};
	}

	private static (string lineNumber, string className)? GetClassAndLineNumber(string trace)
	{
		if (string.IsNullOrEmpty(trace))
		{
			return null;
		}
		string[] array = trace.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string input in array)
		{
			Match match = Regex.Match(input, "\\:line\\s(\\d+)$");
			Match match2 = Regex.Match(input, "^\\s+at\\s(.*) in");
			if (match.Success && match2.Success)
			{
				string item = string.Join(".", match2.Groups[match2.Groups.Count - 1].Value.Split(new string[1] { "." }, StringSplitOptions.RemoveEmptyEntries).TakeWhile((string s) => !s.EndsWith(")")));
				return new(string, string)?((match.Groups[match.Groups.Count - 1].Value, item));
			}
		}
		return null;
	}
}
