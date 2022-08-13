using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Racelogic.Gnss.LabSat;

[Serializable]
public class LabSatException : Exception
{
	private int code;

	public int Code
	{
		[DebuggerStepThrough]
		get
		{
			return code;
		}
		[DebuggerStepThrough]
		protected set
		{
			code = value;
		}
	}

	public LabSatException()
	{
	}

	public LabSatException(string message)
		: base(message)
	{
	}

	internal LabSatException(string message, int code)
		: base(string.Format(message, code))
	{
		this.code = code;
	}

	internal LabSatException(string message, uint code)
		: base(string.Format(message, code))
	{
		this.code = (int)code;
	}

	internal LabSatException(string message, object param)
		: base(string.Format(message, param))
	{
	}

	protected LabSatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public LabSatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("ErrorCode", code);
	}
}
