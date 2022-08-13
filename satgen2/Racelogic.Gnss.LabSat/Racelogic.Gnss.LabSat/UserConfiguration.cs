using System;

namespace Racelogic.Gnss.LabSat;

internal sealed class UserConfiguration : MemoryBlock
{
	internal ChannelMode ChannelMode
	{
		get
		{
			return (ChannelMode)Enum.ToObject(typeof(ChannelMode), base.Memory[66 - base.BaseAddress]);
		}
		set
		{
			if (value == ChannelMode.A1 || value == ChannelMode.B1)
			{
				base.Memory[65 - base.BaseAddress] = 2;
			}
			else
			{
				base.Memory[65 - base.BaseAddress] = 4;
			}
			base.Memory[66 - base.BaseAddress] = (byte)value;
			if (value == ChannelMode.AB1 || value == ChannelMode.A1 || value == ChannelMode.B1 || value == ChannelMode.A1DIO || value == ChannelMode.B1DIO)
			{
				base.Memory[67 - base.BaseAddress] = 0;
			}
			else
			{
				base.Memory[67 - base.BaseAddress] = 1;
			}
		}
	}

	internal ChannelBand ChannelABand
	{
		get
		{
			return (ChannelBand)Enum.ToObject(typeof(ChannelBand), base.Memory[68 - base.BaseAddress]);
		}
		set
		{
			base.Memory[68 - base.BaseAddress] = (byte)value;
		}
	}

	internal ChannelBand ChannelBBand
	{
		get
		{
			return (ChannelBand)Enum.ToObject(typeof(ChannelBand), base.Memory[69 - base.BaseAddress]);
		}
		set
		{
			base.Memory[69 - base.BaseAddress] = (byte)value;
		}
	}

	internal bool ChannelADio
	{
		get
		{
			return base.Memory[90 - base.BaseAddress] == 1;
		}
		set
		{
			base.Memory[90 - base.BaseAddress] = (byte)(value ? 1u : 5u);
		}
	}

	internal bool ChannelBDio
	{
		get
		{
			return base.Memory[91 - base.BaseAddress] == 1;
		}
		set
		{
			base.Memory[91 - base.BaseAddress] = (byte)(value ? 1u : 5u);
		}
	}

	internal UserConfiguration(LabSat2 labSat): base(labSat, 64,32)
	{
    }
}
