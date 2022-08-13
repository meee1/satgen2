using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

namespace Racelogic.Core;

[DataContract]
public class ResourceMapper
{
	private Type resourceType;

	[DataMember]
	private string resourceKey;

	[DataMember]
	private string fixedText;

	[DataMember]
	private object[] parameters;

	[DataMember]
	private string resourceTypeFullName;

	[DataMember]
	private string assemblyPath;

	[DataMember]
	private List<ResourceMapper> moreThanOneText;

	[DataMember]
	private bool? upperOrLowercase;

	public bool FixedText => !string.IsNullOrEmpty(fixedText);

	public string Text
	{
		get
		{
			if (moreThanOneText != null)
			{
				string text = string.Empty;
				foreach (ResourceMapper item in moreThanOneText)
				{
					text += item.Text;
				}
				if (upperOrLowercase.HasValue)
				{
					if (!upperOrLowercase.Value)
					{
						return text.ToLower();
					}
					return text.ToUpper();
				}
				return text;
			}
			if (!string.IsNullOrEmpty(fixedText))
			{
				return fixedText;
			}
			if (resourceType == null)
			{
				return string.Empty;
			}
			PropertyInfo propertyInfo = ((!string.IsNullOrEmpty(resourceKey)) ? resourceType.GetProperty(resourceKey) : null);
			if (propertyInfo == null)
			{
				propertyInfo = resourceType.GetProperty(resourceKey, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic);
			}
			if (propertyInfo != null)
			{
				string text2 = (string)propertyInfo.GetValue(null);
				if (parameters != null)
				{
					text2 = string.Format(text2, parameters);
				}
				if (upperOrLowercase.HasValue)
				{
					if (!upperOrLowercase.Value)
					{
						return text2.ToLower();
					}
					return text2.ToUpper();
				}
				return text2;
			}
			return string.Empty;
		}
	}

	public ResourceMapper(List<ResourceMapper> moreThanOneText)
	{
		this.moreThanOneText = moreThanOneText;
	}

	public ResourceMapper(Type resourceType, string resourceKey, params object[] parameters)
	{
		resourceTypeFullName = resourceType?.FullName;
		assemblyPath = resourceType?.Assembly.Location;
		this.resourceType = resourceType;
		this.resourceKey = resourceKey;
		if (parameters == null)
		{
			return;
		}
		this.parameters = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			object obj = parameters[i];
			if (obj is ResourceMapper)
			{
				this.parameters[i] = ((ResourceMapper)obj).Text;
			}
			else
			{
				this.parameters[i] = obj;
			}
		}
	}

	public ResourceMapper(bool? upperOrLowercase, Type resourceType, string resourceKey, params object[] parameters)
	{
		resourceTypeFullName = resourceType?.FullName;
		assemblyPath = resourceType?.Assembly.Location;
		this.upperOrLowercase = upperOrLowercase;
		this.resourceType = resourceType;
		this.resourceKey = resourceKey;
		if (parameters == null)
		{
			return;
		}
		this.parameters = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			object obj = parameters[i];
			if (obj is ResourceMapper)
			{
				this.parameters[i] = ((ResourceMapper)obj).Text;
			}
			else
			{
				this.parameters[i] = obj;
			}
		}
	}

	public ResourceMapper(string fixedText)
	{
		this.fixedText = fixedText;
	}

	[OnDeserialized]
	private void OnDeserialised(StreamingContext context)
	{
		if (string.IsNullOrEmpty(assemblyPath) || string.IsNullOrEmpty(resourceTypeFullName))
		{
			return;
		}
		if (File.Exists(assemblyPath))
		{
			try
			{
				Assembly assembly = Assembly.LoadFile(assemblyPath);
				resourceType = assembly.GetType(resourceTypeFullName);
			}
			catch
			{
			}
		}
		if (resourceType == null)
		{
			assemblyPath = null;
			resourceTypeFullName = null;
		}
	}
}
