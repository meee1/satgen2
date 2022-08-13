using System.IO;

namespace Racelogic.Comms.Serial;

internal class VideoVBoxFile
{
	private string name = string.Empty;

	private uint size = 0u;

	private MemoryStream stream = null;

	private double totalSize;

	private double bytesTransferred;

	private SerialMessageStatus status;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public uint Size
	{
		get
		{
			return size;
		}
		set
		{
			size = value;
		}
	}

	public MemoryStream Stream
	{
		get
		{
			return stream;
		}
		set
		{
			stream = value;
		}
	}

	public double BytesTransferred
	{
		get
		{
			return bytesTransferred;
		}
		set
		{
			bytesTransferred = value;
		}
	}

	public double TotalSize
	{
		get
		{
			return totalSize;
		}
		set
		{
			totalSize = value;
		}
	}

	public SerialMessageStatus Status
	{
		get
		{
			return status;
		}
		set
		{
			status = value;
		}
	}

	public VideoVBoxFile(uint size)
	{
		Size = size;
		Stream = new MemoryStream((int)Size);
		TotalSize = Size;
		BytesTransferred = 0.0;
		Status = SerialMessageStatus.Ok;
	}
}
