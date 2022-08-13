using System;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;

namespace Racelogic.Utilities;

public class ProgressEstimator : BasePropertyChanged
{
	private int startIndex = 3;

	private int estimateIndex = 8;

	private int windowSize = 16;

	private readonly Stopwatch stopwatch = new Stopwatch();

	private FixedSizeStack<(double Time, double Progress, double WholeTimeEstimate)> history;

	private int currentIndex;

	private double progress;

	private TimeSpan elapsedTime;

	private TimeSpan timeLeft;

	public int StartIndex
	{
		[DebuggerStepThrough]
		get
		{
			return startIndex;
		}
		[DebuggerStepThrough]
		set
		{
			startIndex = value;
			OnPropertyChanged("StartIndex");
		}
	}

	public int EstimateIndex
	{
		[DebuggerStepThrough]
		get
		{
			return estimateIndex;
		}
		[DebuggerStepThrough]
		set
		{
			estimateIndex = value;
			OnPropertyChanged("EstimateIndex");
		}
	}

	public int WindowSize
	{
		[DebuggerStepThrough]
		get
		{
			return windowSize;
		}
		[DebuggerStepThrough]
		set
		{
			windowSize = value;
			history = new FixedSizeStack<(double, double, double)>(WindowSize);
			OnPropertyChanged("WindowSize");
		}
	}

	public double Progress
	{
		[DebuggerStepThrough]
		get
		{
			return progress;
		}
		set
		{
			if (value <= progress)
			{
				return;
			}
			if (progress <= 0.0)
			{
				CurrentIndex = 0;
				stopwatch.Restart();
			}
			else
			{
				CurrentIndex++;
			}
			if (progress >= 1.0)
			{
				stopwatch.Stop();
			}
			progress = value;
			ElapsedTime = stopwatch.Elapsed;
			if (CurrentIndex == StartIndex)
			{
				history.Push((ElapsedTime.TotalSeconds, progress, double.NaN));
			}
			else if (CurrentIndex > StartIndex)
			{
				var (num, num2, _) = history.ReverseElementAt(0);
				if (CurrentIndex == StartIndex + 1)
				{
					history.Clear();
				}
				double totalSeconds = ElapsedTime.TotalSeconds;
				double item = (totalSeconds - num) / (progress - num2);
				history.Push((totalSeconds, progress, item));
			}
			if (CurrentIndex >= EstimateIndex)
			{
				TimeLeft = TimeSpan.FromSeconds(history.Average(((double Time, double Progress, double WholeTimeEstimate) h) => h.WholeTimeEstimate) * (1.0 - progress));
			}
			OnPropertyChanged("Progress");
			OnProgressChanged();
		}
	}

	public int CurrentIndex
	{
		[DebuggerStepThrough]
		get
		{
			return currentIndex;
		}
		[DebuggerStepThrough]
		private set
		{
			if (value != currentIndex)
			{
				currentIndex = value;
				OnPropertyChanged("CurrentIndex");
			}
		}
	}

	public TimeSpan ElapsedTime
	{
		[DebuggerStepThrough]
		get
		{
			return elapsedTime;
		}
		[DebuggerStepThrough]
		private set
		{
			if (!(value == elapsedTime))
			{
				elapsedTime = value;
				OnPropertyChanged("ElapsedTime");
			}
		}
	}

	public TimeSpan TimeLeft
	{
		[DebuggerStepThrough]
		get
		{
			return timeLeft;
		}
		[DebuggerStepThrough]
		private set
		{
			if (!(value == timeLeft))
			{
				timeLeft = value;
				OnPropertyChanged("TimeLeft");
			}
		}
	}

	public event EventHandler<EventArgs<double>> ProgressChanged;

	public ProgressEstimator()
	{
		history = new FixedSizeStack<(double, double, double)>(WindowSize);
	}

	public void Start()
	{
		stopwatch.Restart();
	}

	public void Reset()
	{
		stopwatch.Stop();
		history.Clear();
		elapsedTime = TimeSpan.Zero;
		progress = 0.0;
		timeLeft = TimeSpan.Zero;
		currentIndex = 0;
		OnPropertyChanged("ElapsedTime");
		OnPropertyChanged("TimeLeft");
		OnPropertyChanged("CurrentIndex");
		OnPropertyChanged("Progress");
		OnProgressChanged();
	}

	protected void OnProgressChanged()
	{
		EventArgs<double> e = new EventArgs<double>(Progress);
		this.ProgressChanged?.Invoke(this, e);
	}
}
