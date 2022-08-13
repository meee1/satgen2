using Racelogic.Utilities;

namespace Racelogic.DataSource;

public class CurrentOptions : BasePropertyChanged
{
	private SpeedOptions speed;

	private DistanceOptions distance;

	private LatLongOptions latLong;

	private TimeOptions time;

	private TimeAlwaysInSecondsOptions timeAlwaysInSeconds;

	private AccelerationOptions acceleration;

	private PressureOptions pressure;

	private TemperatureOptions temperature;

	private AngleOptions angle;

	private CanOptions can;

	private MassOptions mass;

	private SoundOptions sound;

	public SpeedOptions Speed
	{
		get
		{
			return speed ?? (speed = new SpeedOptions());
		}
		set
		{
			speed = value;
			OnPropertyChanged("Speed");
		}
	}

	public DistanceOptions Distance
	{
		get
		{
			return distance ?? (distance = new DistanceOptions());
		}
		set
		{
			distance = value;
			OnPropertyChanged("Distance");
		}
	}

	public LatLongOptions LatLong
	{
		get
		{
			return latLong ?? (latLong = new LatLongOptions());
		}
		set
		{
			latLong = value;
			OnPropertyChanged("LatLong");
		}
	}

	public TimeOptions Time
	{
		get
		{
			return time ?? (time = new TimeOptions());
		}
		set
		{
			time = value;
			OnPropertyChanged("Time");
		}
	}

	public TimeAlwaysInSecondsOptions TimeAlwaysInSeconds
	{
		get
		{
			return timeAlwaysInSeconds ?? (timeAlwaysInSeconds = new TimeAlwaysInSecondsOptions());
		}
		set
		{
			timeAlwaysInSeconds = value;
			OnPropertyChanged("TimeAlwaysInSeconds");
		}
	}

	public AccelerationOptions Acceleration
	{
		get
		{
			return acceleration ?? (acceleration = new AccelerationOptions());
		}
		set
		{
			acceleration = value;
			OnPropertyChanged("Acceleration");
		}
	}

	public PressureOptions Pressure
	{
		get
		{
			return pressure ?? (pressure = new PressureOptions());
		}
		set
		{
			pressure = value;
			OnPropertyChanged("Pressure");
		}
	}

	public TemperatureOptions Temperature
	{
		get
		{
			return temperature ?? (temperature = new TemperatureOptions());
		}
		set
		{
			temperature = value;
			OnPropertyChanged("Temperature");
		}
	}

	public AngleOptions Angle
	{
		get
		{
			return angle ?? (angle = new AngleOptions());
		}
		set
		{
			angle = value;
			OnPropertyChanged("Angle");
		}
	}

	public CanOptions Can
	{
		get
		{
			return can ?? (can = new CanOptions());
		}
		set
		{
			can = value;
			OnPropertyChanged("Can");
		}
	}

	public MassOptions Mass
	{
		get
		{
			return mass ?? (mass = new MassOptions());
		}
		set
		{
			mass = value;
			OnPropertyChanged("Mass");
		}
	}

	public SoundOptions Sound
	{
		get
		{
			return sound ?? (sound = new SoundOptions());
		}
		set
		{
			sound = value;
			OnPropertyChanged("Sound");
		}
	}

	internal CurrentOptions()
	{
		Speed.PropertyChanged += delegate
		{
			OnPropertyChanged("Speed");
		};
		Distance.PropertyChanged += delegate
		{
			OnPropertyChanged("Distance");
		};
		LatLong.PropertyChanged += delegate
		{
			OnPropertyChanged("LatLong");
		};
		Time.PropertyChanged += delegate
		{
			OnPropertyChanged("Time");
		};
		Acceleration.PropertyChanged += delegate
		{
			OnPropertyChanged("Acceleration");
		};
		Pressure.PropertyChanged += delegate
		{
			OnPropertyChanged("Pressure");
		};
		Temperature.PropertyChanged += delegate
		{
			OnPropertyChanged("Temperature");
		};
		Mass.PropertyChanged += delegate
		{
			OnPropertyChanged("Mass");
		};
		Sound.PropertyChanged += delegate
		{
			OnPropertyChanged("Sound");
		};
	}
}
