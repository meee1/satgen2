using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using SlimDX.DirectInput;

namespace Racelogic.Utilities;

public class Joystick : IDisposable
{
	private static readonly DirectInput directInput = new DirectInput();

	private readonly SlimDX.DirectInput.Joystick joystick;

	private JoystickState joystickState;

	private const double MaxValue = 1000.0;

	private double x;

	private double y;

	private double z;

	private double rotationX;

	private double rotationY;

	private double rotationZ;

	private double slider1;

	private double slider2;

	private bool[] buttons;

	private bool disposedValue;

	public double X => x;

	public double Y => y;

	public double Z => z;

	public double RotationX => rotationX;

	public double RotationY => rotationY;

	public double RotationZ => rotationZ;

	public double Slider1 => slider1;

	public double Slider2 => slider2;

	public bool[] Buttons => buttons;

	private Joystick(DeviceInstance deviceInstance)
	{
		IntPtr handle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
		joystick = new SlimDX.DirectInput.Joystick(directInput, deviceInstance.InstanceGuid);
		joystick.SetCooperativeLevel(handle, CooperativeLevel.Background | CooperativeLevel.Nonexclusive);
		joystick.Acquire();
		foreach (DeviceObjectInstance item in from d in joystick.GetObjects()
			where d.ObjectType.HasFlag(ObjectDeviceType.Axis) || d.ObjectType.HasFlag(ObjectDeviceType.AbsoluteAxis) || d.ObjectType.HasFlag(ObjectDeviceType.RelativeAxis)
			select d)
		{
			if (item.Name.ToUpperInvariant().Contains("SLIDER"))
			{
				joystick.GetObjectPropertiesById((int)item.ObjectType).SetRange(0, 1000);
			}
			else
			{
				joystick.GetObjectPropertiesById((int)item.ObjectType).SetRange(-1000, 1000);
			}
		}
		UpdateStatus();
	}

	public static string[] FindJoysticks()
	{
		List<string> list = new List<string>();
		try
		{
			foreach (DeviceInstance device in directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly))
			{
				SlimDX.DirectInput.Joystick joystick = new SlimDX.DirectInput.Joystick(directInput, device.InstanceGuid);
				list.Add(joystick.Information.InstanceName);
				joystick.Dispose();
			}
		}
		catch
		{
			return null;
		}
		return list.ToArray();
	}

	public static Joystick AcquireJoystick(string name)
	{
		try
		{
			using IEnumerator<DeviceInstance> enumerator = (from d in directInput.GetDevices(DeviceClass.GameController, DeviceEnumerationFlags.AttachedOnly)
				where d.InstanceName == name
				select d).GetEnumerator();
			if (enumerator.MoveNext())
			{
				return new Joystick(enumerator.Current);
			}
		}
		catch (Exception)
		{
		}
		return null;
	}

	public void UpdateStatus()
	{
		Poll();
		x = (double)joystickState.X / 1000.0;
		y = (double)joystickState.Y / 1000.0;
		z = (double)joystickState.Z / 1000.0;
		rotationX = (double)joystickState.RotationX / 1000.0;
		rotationY = (double)joystickState.RotationY / 1000.0;
		rotationZ = (double)joystickState.RotationZ / 1000.0;
		int[] sliders = joystickState.GetSliders();
		slider1 = (1000.0 - (double)sliders.ElementAtOrDefault(0)) / 1000.0;
		slider2 = (1000.0 - (double)sliders.ElementAtOrDefault(1)) / 1000.0;
		buttons = joystickState.GetButtons();
	}

	private void Poll()
	{
		try
		{
			joystick.Poll();
			joystickState = joystick.GetCurrentState();
		}
		catch
		{
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			disposedValue = true;
			if (disposing)
			{
				joystick.Unacquire();
				joystick.Dispose();
				directInput.Dispose();
			}
		}
	}
}
