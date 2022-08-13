using System;

namespace Racelogic.Core;

[AttributeUsage(AttributeTargets.Class)]
public class CanResizeHostApplication : Attribute
{
	public bool CanResize { get; private set; }

	public double MaximumWidth { get; private set; } = double.PositiveInfinity;


	public double MaximumHeight { get; private set; } = double.PositiveInfinity;


	public double MinimumWidth { get; private set; } = 800.0;


	public double MinimumHeight { get; private set; } = 552.0;


	public CanResizeHostApplication(bool canResize)
	{
		CanResize = canResize;
	}

	public CanResizeHostApplication(bool canResize, double minimumWidth, double minimumHeight)
		: this(canResize)
	{
		MinimumWidth = minimumWidth;
		MinimumHeight = minimumHeight;
	}

	public CanResizeHostApplication(bool canResize, double minimumWidth, double minimumHeight, double maximumWidth, double maximumHeight)
		: this(canResize, minimumWidth, minimumHeight)
	{
		MaximumWidth = maximumWidth;
		MaximumHeight = maximumHeight;
	}
}
