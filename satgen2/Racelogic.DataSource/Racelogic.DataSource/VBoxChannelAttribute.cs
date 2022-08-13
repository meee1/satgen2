using System;

namespace Racelogic.DataSource;

[AttributeUsage(AttributeTargets.Field)]
public class VBoxChannelAttribute : Attribute
{
	private readonly Type unitType;

	public Type UnitType => unitType;

	public VBoxChannelAttribute(Type unitType)
	{
		this.unitType = unitType;
	}
}
