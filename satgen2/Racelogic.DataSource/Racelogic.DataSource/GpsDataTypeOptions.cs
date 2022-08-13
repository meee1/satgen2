using System;
using Racelogic.Utilities;

namespace Racelogic.DataSource;

public abstract class GpsDataTypeOptions : BasePropertyChanged
{
	private ToStringOptions options;

	private byte decimalPlaces = 2;

	public string Format = "0.000";

	public ToStringOptions Options
	{
		get
		{
			return options;
		}
		set
		{
			options = value;
			OnPropertyChanged("Options");
		}
	}

	public byte DecimalPlaces
	{
		get
		{
			return decimalPlaces;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", "Decimal places cannot be less than zero.");
			}
			decimalPlaces = value;
			if (value == 0)
			{
				Format = "0";
			}
			else
			{
				string text = "0.0";
				while (value-- > 1)
				{
					text += "0";
				}
				Format = text;
			}
			OnPropertyChanged("DecimalPlaces");
		}
	}

	internal GpsDataTypeOptions(byte decimalPlaces, ToStringOptions options)
	{
		DecimalPlaces = decimalPlaces;
		Options = options;
	}
}
