using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Racelogic.Utilities;

namespace Racelogic.Libraries.Nmea;

public class GpsReceiver : IDisposable
{
	private readonly INmeaParser nmeaParser = new NmeaParser();

	private ISerialPort serialPort;

	private bool receivingBadData;

	private List<GpsSample> previoussamples = new List<GpsSample>();

	private List<string> previousnmea = new List<string>();

	public int BaudRate { get; set; }

	public virtual byte[] ColdstartCommand { get; set; }

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

	public string PortName { get; set; }

	protected ISerialPort SerialPort => serialPort;

	public event ExceptionThrownEvent ExceptionThrown;

	public event GpsSampleReceived GpsSampleReceived;

	public event SerialDataReceived SerialDataReceived;

	public event EventHandler PortOpened;

	public event EventHandler PortClosed;

	public void OnExceptionThrown(ExceptionThrownEventArgs args)
	{
		this.ExceptionThrown?.Invoke(this, args);
	}

	public GpsReceiver(ISerialPort serialPort)
	{
		this.serialPort = serialPort;
	}

	public virtual void Close()
	{
		try
		{
			serialPort.Close();
		}
		catch (IOException ex)
		{
			RLLogger.GetLogger().LogException(ex);
		}
		serialPort.DataReceived -= SerialPortOnDataReceived;
		serialPort.PortOpened -= SerialPortOnOpened;
		serialPort.PortClosed -= SerialPortOnClosed;
	}

	public virtual void Coldstart()
	{
		if (ColdstartCommand != null)
		{
			try
			{
				SendCommand(ColdstartCommand);
			}
			catch (Exception ex)
			{
				RLLogger.GetLogger().LogMessage("Writing command to serial port failed.  Exception:");
				RLLogger.GetLogger().LogException(ex);
				OnExceptionThrown(new ExceptionThrownEventArgs(ex));
			}
		}
	}

	public virtual void Dispose()
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			Close();
		});
	}

	public virtual void SwitchToSingleConstellation(Constellation constellation)
	{
	}

	public void OnGpsSampleReceived(GpsSampleReceivedEventArgs eventargs)
	{
		this.GpsSampleReceived?.Invoke(this, eventargs);
	}

	public void OnSerialDataReceived(SerialDataReceivedEventArgs eventargs)
	{
		this.SerialDataReceived?.Invoke(this, eventargs);
	}

	private void OnPortOpened()
	{
		this.PortOpened?.Invoke(this, EventArgs.Empty);
	}

	private void OnPortClosed()
	{
		this.PortClosed?.Invoke(this, EventArgs.Empty);
	}

	public virtual void Open()
	{
		if (IsOpen)
		{
			return;
		}
		try
		{
			serialPort.PortName = PortName;
			serialPort.DataReceived += SerialPortOnDataReceived;
			serialPort.PortOpened += SerialPortOnOpened;
			serialPort.PortClosed += SerialPortOnClosed;
			serialPort.BaudRate = BaudRate;
			serialPort.Open();
		}
		catch (UnauthorizedAccessException ex)
		{
			RLLogger.GetLogger().LogException(ex);
			throw;
		}
		catch (IOException ex2)
		{
			RLLogger.GetLogger().LogException(ex2);
			throw;
		}
		catch (Exception ex3)
		{
			RLLogger.GetLogger().LogException(ex3);
			throw;
		}
	}

	public virtual void SendCommand(byte[] command)
	{
		serialPort.Write(command, 0, command.Length);
	}

	private void SerialPortOnOpened(object sender, EventArgs e)
	{
		ISerialPort serialPort = (ISerialPort)sender;
		if (serialPort != null && serialPort.IsOpen)
		{
			OnPortOpened();
		}
	}

	private void SerialPortOnClosed(object sender, EventArgs e)
	{
		ISerialPort serialPort = (ISerialPort)sender;
		if (serialPort != null && !serialPort.IsOpen)
		{
			OnPortClosed();
		}
	}

	private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs eventArgs)
	{
		ISerialPort serialPort = (ISerialPort)sender;
		if (serialPort == null || !serialPort.IsOpen)
		{
			return;
		}
		string text = string.Empty;
		try
		{
			text = serialPort.ReadExisting();
		}
		catch (Exception ex)
		{
			RLLogger.GetLogger().LogMessage("ReadExisting after checking for port IsOpen.  Exception:");
			RLLogger.GetLogger().LogException(ex);
		}
		OnSerialDataReceived(new SerialDataReceivedEventArgs(SerialData.Chars, text));
		if (text.StartsWith("\0") && GetType() != typeof(GpsReceiver) && !receivingBadData)
		{
			receivingBadData = true;
			SerialPortReconnect();
		}
		foreach (GpsSample item in nmeaParser.ParseIncomingData(text))
		{
			receivingBadData = false;
			OnGpsSampleReceived(new GpsSampleReceivedEventArgs
			{
				Sample = item
			});
		}
	}

	private void SerialPortReconnect()
	{
		if (IsOpen)
		{
			Close();
		}
		System.IO.Ports.SerialPort serialPort = new System.IO.Ports.SerialPort(this.serialPort.PortName, BaudRate);
		this.serialPort = new SerialPort(serialPort);
		Open();
	}
}
