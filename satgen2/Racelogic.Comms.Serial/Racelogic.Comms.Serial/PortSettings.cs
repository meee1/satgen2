using System;
using System.ComponentModel;
using Racelogic.Comms.Serial.Properties;
using Racelogic.Core;
//using ZylSoft.Serial;

namespace Racelogic.Comms.Serial;

public struct PortSettings : INotifyPropertyChanged
{/*
	public EventHandler FlushPort;

	public SerialBaudRate BaudRate
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected I4, but got Unknown
			return (SerialBaudRate)Port.get_BaudRate();
		}
		set
		{
			CheckPortAndSetFields(PortName, value);
			OnNotify("BaudRate");
		}
	}

	public int BaudRateAsInt => (int)Port.get_BaudRate();

	internal bool IsOpen => (int)Port.get_ConnectedTo() > 0;

	public SerialPort Port { get; private set; }

	public string PortName
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			object result;
			if ((int)Port.get_Port() != 0)
			{
				SerialCommPort port = Port.get_Port();
				result = ((object)(SerialCommPort)(ref port)).ToString();
			}
			else
			{
				result = "";
			}
			return (string)result;
		}
		set
		{
			CheckPortAndSetFields(value, (SerialBaudRate)BaudRateAsInt);
			OnNotify("PortName");
		}
	}
	*/
	public event PropertyChangedEventHandler PropertyChanged;
	/*
	internal event EventHandler<SlidingMessageEventArgs> NewSlidingMessage;

	internal void Initialise(string portName)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Port = new SerialPort();
		Port.set_UnlockKey("FA3450FEA2344897EFC34325BA391072");
		Port.set_Port((SerialCommPort)((!string.IsNullOrEmpty(portName)) ? ((int)SerialPort.StringToSerialCommPort(portName)) : 0));
		Port.set_BaudRate((SerialBaudRate)115200);
		Port.set_StopBits((SerialStopBits)0);
		Port.set_DataWidth((SerialDataWidth)8);
		Port.set_HardwareFlowControl((SerialHardwareFlowControl)0);
		Port.set_ParityCheck(false);
		Port.set_ParityBits((SerialParityBits)0);
		Port.set_EnableDtrOnOpen(true);
		Port.set_AutoReceive(false);
		Port.set_Interval(2);
	}

	internal void CheckPortAndSetFields(string tempName)
	{
		CheckPortAndSetFields(PortName, (SerialBaudRate)BaudRateAsInt, tempName);
	}

	internal void DiscardInBuffer()
	{
		Port.ClearInputBuffer();
	}

	internal void DiscardOutBuffer()
	{
		Port.ClearOutputBuffer();
	}

	internal void FlushRxTx()
	{
		DiscardOutBuffer();
		DiscardInBuffer();
	}

	private void DisplaySlidingMessage(string text, string title)
	{
		if (this.NewSlidingMessage != null)
		{
			this.NewSlidingMessage(this, new SlidingMessageEventArgs(title, text));
		}
	}

	private void CheckPortAndSetFields(string newName, SerialBaudRate newBaudRate, string tempName = null)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		bool flag2 = true;
		string text = (string.IsNullOrEmpty(tempName) ? newName : tempName);
		if (IsOpen)
		{
			if (string.Equals(text, PortName))
			{
				flag = true;
				try
				{
					Port.Close();
				}
				catch (Exception ex)
				{
					flag2 = false;
					DisplaySlidingMessage("Racelogic.Comms.LowLevel.ClosePort(): " + ex.Message, Racelogic.Comms.Serial.Properties.Resources.Error);
				}
			}
			else
			{
				flag2 = false;
			}
		}
		if (!flag2)
		{
			return;
		}
		if (!string.IsNullOrEmpty(text))
		{
			Port.set_Port(SerialPort.StringToSerialCommPort(text));
		}
		Port.set_BaudRate((SerialBaudRate)newBaudRate);
		if (!flag)
		{
			return;
		}
		try
		{
			Port.Open();
			if (FlushPort != null)
			{
				FlushPort(this, EventArgs.Empty);
			}
		}
		catch (Exception)
		{
		}
	}

	private void OnNotify(string PropertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
		}
	}*/
}
