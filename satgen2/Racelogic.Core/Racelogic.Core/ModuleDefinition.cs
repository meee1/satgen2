using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Racelogic.Core;

[DataContract(Name = "ModuleDefinition", Namespace = "")]
public class ModuleDefinition
{
	[DataMember]
	public int UnitType { get; private set; }

	[DataMember]
	public Processor Processor { get; private set; }

	[DataMember]
	public int SizeOfEeprom { get; private set; }

	[DataMember]
	public string UserFriendlyName { get; private set; }

	[DataMember]
	public string RacelogicName { get; private set; }

	[DataMember]
	public string PluginGroup { get; private set; }

	[DataMember]
	public ModuleFunction ModuleFunctions { get; private set; }

	[DataMember(Name = "SubTypes")]
	public List<SubTypeDefinition> SubTypes { get; private set; }

	[DataMember]
	public SubTypeDefinition DefaultSubType { get; private set; }

	[DataMember]
	public SerialBaudRate DefaultBaudRate { get; private set; }

	public ModuleDefinition()
	{
	}

	public ModuleDefinition(int unitType, Processor processor, SerialBaudRate baudRate, ModuleFunction moduleFunctions, int sizeOfEeprom, string userFriendlyName, string racelogicName, string pluginGroup, SubTypeDefinition[] subTypes = null)
	{
		UnitType = unitType;
		Processor = processor;
		ModuleFunctions = moduleFunctions;
		if ((ModuleFunctions & ModuleFunction.SerialNumberFromList) == ModuleFunction.SerialNumberFromList)
		{
			ModuleFunctions |= ModuleFunction.HasGpsEngine;
		}
		SizeOfEeprom = sizeOfEeprom;
		UserFriendlyName = userFriendlyName;
		RacelogicName = racelogicName;
		PluginGroup = (string.Equals(pluginGroup, "N/A") ? racelogicName : pluginGroup);
		DefaultBaudRate = baudRate;
		switch (Processor)
		{
		case Processor.STR71x:
		case Processor.STR91x:
		case Processor.NA:
			DefaultSubType = new SubTypeDefinition(byte.MaxValue);
			break;
		case Processor.LPC:
			DefaultSubType = new SubTypeDefinition(0);
			break;
		default:
			DefaultSubType = new SubTypeDefinition(48);
			break;
		}
		if (subTypes != null)
		{
			SubTypes = new List<SubTypeDefinition>();
			foreach (SubTypeDefinition item in subTypes)
			{
				if (SubTypes.IndexOf(item) == -1)
				{
					SubTypes.Add(item);
				}
			}
		}
		else
		{
			SubTypes = null;
		}
	}
}
