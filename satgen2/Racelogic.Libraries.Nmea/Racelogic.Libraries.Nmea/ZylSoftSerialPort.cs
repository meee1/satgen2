using System;
using System.IO.Ports;
using System.Threading;
//using ZylSoft.Serial;

namespace Racelogic.Libraries.Nmea;
/*
public class ZylSoftSerialPort : ISerialPort
{
	private readonly SerialPort serialPort;

	private string buffer;

	private bool isOpen;

	public int BaudRate
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Convert.ToInt32(serialPort.get_BaudRate());
		}
		set
		{
			serialPort.set_BaudRate((SerialBaudRate)value);
		}
	}

	public bool IsOpen => isOpen;

	public string PortName
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			SerialCommPort port = serialPort.get_Port();
			return ((object)(SerialCommPort)(ref port)).ToString();
		}
		set
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			serialPort.set_Port(SerialPort.StringToSerialCommPort(value));
		}
	}

	public event SerialDataReceived DataReceived;

	public event EventHandler PortOpened;

	public event EventHandler PortClosed;

	[CLSCompliant(false)]
	public ZylSoftSerialPort(SerialPort serialPort)
	{
		this.serialPort = serialPort;
		this.serialPort.add_Received((EventHandler<DataEventArgs>)serialPort_Received);
		this.serialPort.add_Connected((EventHandler<ConnectionEventArgs>)serialPort_Connected);
		this.serialPort.add_Disconnected((EventHandler<ConnectionEventArgs>)serialPort_Disconnected);
	}

	private void serialPort_Connected(object sender, ConnectionEventArgs e)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		isOpen = true;
		if ((int)e.get_Port() != 0)
		{
			OnPortOpened();
		}
	}

	private void serialPort_Disconnected(object sender, ConnectionEventArgs e)
	{
		isOpen = false;
		OnPortClosed();
	}

	private void serialPort_Received(object sender, DataEventArgs e)
	{
		buffer = SerialPort.AsciiByteArrayToString(e.get_Buffer());
		this.DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(SerialData.Chars));
	}

	private void OnPortOpened()
	{
		this.PortOpened?.Invoke(this, EventArgs.Empty);
	}

	private void OnPortClosed()
	{
		this.PortClosed?.Invoke(this, EventArgs.Empty);
	}

	public void Close()
	{
		serialPort.Close();
	}

	public void Open()
	{
		if (isOpen)
		{
			serialPort.Close();
		}
		int num = 20;
		bool flag;
		do
		{
			flag = serialPort.Open();
			if (!flag)
			{
				Thread.Sleep(50);
			}
		}
		while (!flag && num > 0);
		if (!flag)
		{
			throw new InvalidOperationException();
		}
	}

	public string ReadExisting()
	{
		return buffer;
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		try
		{
			if ((int)serialPort.get_ConnectedTo() != 0)
			{
				serialPort.ClearInputBuffer();
				do
				{
					num2++;
					num = serialPort.SendByteArray(buffer);
					Thread.Sleep(20);
				}
				while (num == 0 && num2 < 20);
			}
		}
		finally
		{
		}
	}
}
*/