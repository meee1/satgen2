using System;
using System.Diagnostics;
using System.Linq;

namespace Racelogic.Gnss.LabSat;

public class LabSat2Controller : IDisposable
{
	private readonly LabSat2? device;

	private readonly ChannelBand bandA;

	private readonly ChannelBand bandB;

	private readonly int quantization;

	private readonly bool useSmallBuffer;

	private readonly int frameSizeWords;

	protected bool isDisposed;

	public bool IsConnected
	{
		[DebuggerStepThrough]
		get
		{
			return device != null;
		}
	}

	private protected LabSat2? Device
	{
		[DebuggerStepThrough]
		get
		{
			return device;
		}
	}

	private protected int FrameSizeWords
	{
		[DebuggerStepThrough]
		get
		{
			return frameSizeWords;
		}
	}

	public LabSat2Controller(ChannelBand[] frequencyBands, in int quantization, in bool useSmallBuffer = false)
	{
		this.quantization = quantization;
		this.useSmallBuffer = useSmallBuffer;
		GetChannelOrder(frequencyBands, out bandA, out bandB);
		frameSizeWords = 81840;
		device = LabSat2.GetDevice(in frameSizeWords);
		if (device != null)
		{
			WriteConfiguration();
			device!.OpenStream(in frameSizeWords);
			device!.ClearBuffer();
		}
	}

	public virtual void WriteBuffer(short[] shorts)
	{
		Device?.StreamFrame(shorts);
	}

	public virtual void WriteBuffer(in IntPtr shortsPointer, in int shortsCount)
	{
		Device?.StreamFrame(in shortsPointer, in shortsCount);
	}

	public int GetChannelIndex(ChannelBand band)
	{
		if (band == bandA)
		{
			return 0;
		}
		if (band == bandB)
		{
			return 1;
		}
		return -1;
	}

	private void WriteConfiguration()
	{
		if (Device != null)
		{
			LabSat2? labSat = Device;
			bool channelAEnabled = bandA != ChannelBand.Unset;
			bool channelBEnabled = bandB != ChannelBand.Unset;
			labSat!.EnableChannels(in channelAEnabled, in channelBEnabled, in useSmallBuffer);
			UserConfiguration userConfiguration = new UserConfiguration(Device);
			userConfiguration.ChannelMode = GetChannelMode(bandA, bandB, in quantization);
			userConfiguration.ChannelABand = bandA;
			userConfiguration.ChannelBBand = bandB;
			userConfiguration.ChannelADio = false;
			userConfiguration.ChannelBDio = false;
			userConfiguration.WriteToLabSat();
		}
	}

	private static ChannelMode GetChannelMode(ChannelBand bandA, ChannelBand bandB, in int quantization)
	{
		if (bandA != ChannelBand.Unset && bandB != ChannelBand.Unset)
		{
			return ChannelMode.AB1;
		}
		if (bandA != ChannelBand.Unset)
		{
			if (quantization == 2)
			{
				return ChannelMode.A2;
			}
			return ChannelMode.A1;
		}
		if (quantization == 2)
		{
			return ChannelMode.B2;
		}
		return ChannelMode.B1;
	}

	private static void GetChannelOrder(ChannelBand[] bands, out ChannelBand bandA, out ChannelBand bandB)
	{
		bandA = ChannelBand.Unset;
		bandB = ChannelBand.Unset;
		if (bands.Contains(ChannelBand.GpsL1))
		{
			bandA = ChannelBand.GpsL1;
		}
		if (bands.Contains(ChannelBand.GlonassL1))
		{
			bandB = ChannelBand.GlonassL1;
			if (bands.Contains(ChannelBand.BeiDouB1))
			{
				bandA = ChannelBand.BeiDouB1;
			}
		}
		else if (bands.Contains(ChannelBand.BeiDouB1))
		{
			bandB = ChannelBand.BeiDouB1;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing)
			{
				device?.CloseStream();
			}
		}
	}
}
