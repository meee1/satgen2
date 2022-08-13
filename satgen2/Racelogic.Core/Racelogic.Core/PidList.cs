using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Racelogic.Core;

public static class PidList
{
	public class PidDefinition
	{
		public int[] UnitType { get; private set; }

		public int Pid { get; private set; }

		public UsbDeviceType DeviceType { get; private set; }

		internal PidDefinition(int pid, UsbDeviceType deviceType, int[] unitType)
		{
			UnitType = unitType;
			Pid = pid;
			DeviceType = deviceType;
		}
	}

	public enum UsbDeviceType
	{
		CDC,
		MassStorage,
		Combo
	}

	public static ReadOnlyCollection<PidDefinition> List;

	static PidList()
	{
		List = new ReadOnlyCollection<PidDefinition>(new List<PidDefinition>
		{
			new PidDefinition(2631, UsbDeviceType.MassStorage, new int[0]),
			new PidDefinition(2632, UsbDeviceType.MassStorage, new int[0]),
			new PidDefinition(2633, UsbDeviceType.CDC, new int[0]),
			new PidDefinition(2634, UsbDeviceType.CDC, new int[0]),
			new PidDefinition(2635, UsbDeviceType.CDC, new int[0]),
			new PidDefinition(2636, UsbDeviceType.MassStorage, new int[0]),
			new PidDefinition(2637, UsbDeviceType.CDC, new int[0]),
			new PidDefinition(2638, UsbDeviceType.MassStorage, new int[0]),
			new PidDefinition(2639, UsbDeviceType.MassStorage, new int[0]),
			new PidDefinition(2640, UsbDeviceType.CDC, new int[0]),
			new PidDefinition(2641, UsbDeviceType.Combo, new int[0]),
			new PidDefinition(2642, UsbDeviceType.Combo, new int[0]),
			new PidDefinition(2643, UsbDeviceType.Combo, new int[0]),
			new PidDefinition(2644, UsbDeviceType.Combo, new int[0]),
			new PidDefinition(2645, UsbDeviceType.CDC, new int[0]),
			new PidDefinition(2646, UsbDeviceType.CDC, new int[0]),
			new PidDefinition(2647, UsbDeviceType.CDC, new int[0])
		});
	}
}
