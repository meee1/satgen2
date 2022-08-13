using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Racelogic.Libraries.Nmea;

public class SerialPort : ISerialPort
{
	private readonly System.IO.Ports.SerialPort serialPort;

	public int BaudRate
	{
		get
		{
			return serialPort.BaudRate;
		}
		set
		{
			serialPort.BaudRate = value;
		}
	}

	public bool IsOpen
	{
		get
		{
			if (serialPort != null)
			{
				return serialPort.IsOpen;
			}
			return false;
		}
	}

	public string PortName
	{
		get
		{
			return serialPort.PortName;
		}
		set
		{
			serialPort.PortName = value;
		}
	}

	public event SerialDataReceived DataReceived;

	public event EventHandler PortOpened;

	public event EventHandler PortClosed;

	public SerialPort(System.IO.Ports.SerialPort serialPort)
	{
		this.serialPort = serialPort;
		this.serialPort.DataReceived += SerialPortDataReceived;
	}

	public void Close()
	{
		if (serialPort.IsOpen)
		{
			Thread.Sleep(250);
			Task.Factory.StartNew(delegate
			{
				serialPort.Close();
			});
			OnPortClosed();
		}
	}

	public void Open()
	{
		if (!serialPort.IsOpen)
		{
			serialPort.Open();
			OnPortOpened();
		}
	}

	public string ReadExisting()
	{
		return serialPort.ReadExisting();
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		serialPort.Write(buffer, offset, count);
	}

	private void InvokeDataReceived(SerialDataReceivedEventArgs eventArgs)
	{
		this.DataReceived?.Invoke(this, eventArgs);
	}

	private void SerialPortDataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
	{
		InvokeDataReceived(new SerialDataReceivedEventArgs(e.EventType));
	}

	private void OnPortOpened()
	{
		this.PortOpened?.Invoke(this, EventArgs.Empty);
	}

	private void OnPortClosed()
	{
		this.PortClosed?.Invoke(this, EventArgs.Empty);
	}
}
