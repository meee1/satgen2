namespace Racelogic.Maths;

public struct Point
{
	private double x;

	private double y;

	public double X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public double Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public double Z { get; set; }

	public Point(double x, double y)
	{
		this.x = x;
		this.y = y;
		Z = 0.0;
	}

	public Point(double x, double y, double z)
	{
		this.x = x;
		this.y = y;
		Z = z;
	}
}
