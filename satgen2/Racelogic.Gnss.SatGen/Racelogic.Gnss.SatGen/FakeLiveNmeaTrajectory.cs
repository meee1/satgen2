using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

public class FakeLiveNmeaTrajectory : Trajectory, ILiveTrajectory
{
	private const int MinInputSampleCount = 11;

	private const int MaxSampleRate = 100;

	protected readonly FixedSizeStack<Pvt> inputBuffer = new FixedSizeStack<Pvt>(200);

	protected readonly FixedSizeStack<Pvt> outputBuffer = new FixedSizeStack<Pvt>(1000);

	private readonly GravitationalModel gravitationalModel;

	private const int lockTimeout = 10000;

	private readonly SyncLock sampleLock = new SyncLock("LiveNmeaSampleLock", 10000);

	private readonly Timer outputTimer;

	private bool isOutputTimerActive;

	private GnssTime outputTime;

	private int outputAdvance;

	private int outputAdvanceCounter;

	private readonly Timer inputTimer;

	public Ecef ecef = Geodetic.FromDegrees(51.9895405, -0.9913955, 111.57).ToEcef(Datum.WGS84, Geoid.Egm96);

	private GnssTime sampleTime;

	private bool isDisposed;

	public FakeLiveNmeaTrajectory(in GnssTime startDate, in int sampleRate, GravitationalModel gravitationalModel = GravitationalModel.Wgs84)
	{
		base.IsExternal = true;
		this.gravitationalModel = gravitationalModel;
		outputTimer = new Timer(new TimerCallback(OnOutputTimerTick));
		base.SampleRate = sampleRate;
		GnssTime gnssTime = GnssTime.FromGps(startDate.GpsWeek, 0);
		GnssTimeSpan gnssTimeSpan = (startDate - gnssTime).Nanoseconds / base.SampleSpan.Nanoseconds * base.SampleSpan;
		sampleTime = gnssTime + gnssTimeSpan;
		if (sampleTime < startDate)
		{
			sampleTime += base.SampleSpan;
		}
		DateTime utcTime = sampleTime.UtcTime;
		DateTime utcDateTime = utcTime - utcTime.TimeOfDay;
		Interval = new Range<GnssTime, GnssTimeSpan>(GnssTime.FromUtc(utcDateTime), GnssTime.MaxValue);
		TimeSpan period = TimeSpan.FromMilliseconds((int)base.SampleSpan.TimeSpan.TotalMilliseconds);
		inputTimer = new Timer(new TimerCallback(OnInputTimerTick), null, TimeSpan.Zero, period);
	}

	private void OnInputTimerTick(object? state)
	{
		using (sampleLock.Lock())
		{
			Console.WriteLine("inside OnInputTimerTick");
			inputBuffer.Push(new Pvt(in sampleTime, in ecef));
			sampleTime += base.SampleSpan;
			ProcessNewSample();
		}
	}

	private void ProcessNewSample()
	{
		if (ProcessInputSamples() && ProduceOutputSamples() && !isOutputTimerActive)
		{
			isOutputTimerActive = true;
			outputTime = outputBuffer[1].Time;
			outputTimer.Change(base.SampleSpan.TimeSpan, base.SampleSpan.TimeSpan);
		}
	}

	private bool ProcessInputSamples()
	{
		if (inputBuffer.Count < 5)
		{
			return false;
		}
		GnssTime time = inputBuffer[4].Time;
		double[] array = new double[5];
		double[] array2 = new double[5];
		double[] array3 = new double[5];
		double[] array4 = new double[5];
		for (int i = 0; i < 5; i++)
		{
			Pvt pvt = inputBuffer[4 - i];
			array[i] = (pvt.Time - time).Seconds;
			Vector3D position = pvt.Ecef.Position;
			array2[i] = position.X;
			array3[i] = position.Y;
			array4[i] = position.Z;
		}
		AkimaSpline akimaSpline = new AkimaSpline(array, array2, isConstantRate: true);
		AkimaSpline akimaSpline2 = new AkimaSpline(array, array3, isConstantRate: true);
		AkimaSpline akimaSpline3 = new AkimaSpline(array, array4, isConstantRate: true);
		for (int num = 2; num >= 0; num--)
		{
			Pvt pvt2 = inputBuffer[num];
			Ecef ecef = pvt2.Ecef;
			GnssTime time2 = pvt2.Time;
			double seconds = (time2 - time).Seconds;
			double x = akimaSpline.Differentiate(seconds);
			double y = akimaSpline2.Differentiate(seconds);
			double z = akimaSpline3.Differentiate(seconds);
			Vector3D velocity = new Vector3D(x, y, z);
			Ecef ecef2 = new Ecef(in ecef.Position, in velocity);
			Pvt newItem = new Pvt(in time2, in ecef2);
			inputBuffer.ReplaceAt(num, newItem);
		}
		return true;
	}

	protected bool ProduceOutputSamples()
	{
		if (inputBuffer.Count < 11)
		{
			return false;
		}
		GnssTime lastOutputTime = (outputBuffer.Any() ? outputBuffer.Peek().Time : Interval.Start);
		foreach (Pvt item in inputBuffer.Skip(2).TakeWhile((Pvt s) => s.Time > lastOutputTime).Reverse())
		{
			outputBuffer.Push(item);
		}
		return true;
	}

	private void OnOutputTimerTick(object? state)
	{
		if (outputTime.IsEmpty)
		{
			outputTime = outputBuffer.Peek().Time - base.SampleSpan;
		}
		outputTime += base.SampleSpan;
		Interlocked.Decrement(ref outputAdvanceCounter);
		int num = Interlocked.Exchange(ref outputAdvance, 0);
		if (num > 0)
		{
			TimeSpan timeSpan = base.SampleSpan.TimeSpan - TimeSpan.FromMilliseconds(num);
			TimeSpan timeSpan2 = TimeSpan.FromMilliseconds(2.0);
			if (timeSpan < timeSpan2)
			{
				timeSpan = timeSpan2;
			}
			outputTimer.Change(timeSpan, base.SampleSpan.TimeSpan);
			RLLogger.GetLogger().LogMessage($"Advance {num}ms");
		}
		GnssTime start = Interval.Start;
		GnssTime time = outputBuffer.Peek().Time;
		Interval = new Range<GnssTime, GnssTimeSpan>(start, time);
		if (time < outputTime)
		{
			RLLogger.GetLogger().LogMessage($"Extrapolate {(outputTime - time).Seconds}s");
		}
		OnNewSample(this, new EventArgs<GnssTime>(outputTime));
	}

	void ILiveTrajectory.AdvanceSampleClock(TimeSpan advance)
	{
		if (!(advance.TotalMilliseconds < 1.0))
		{
			outputAdvance = (int)Math.Round(advance.TotalMilliseconds);
			if (outputAdvanceCounter <= 0)
			{
				outputAdvanceCounter = 5;
			}
		}
	}

	public override IReadOnlyList<Pvt>? GetSamples(in Range<GnssTime, GnssTimeSpan> interval)
	{
		using (sampleLock.Lock())
		{
            if (!outputBuffer.Any())
			{
				return null;
			}
			GnssTime time = outputBuffer.ReverseElementAt(0).Time;
			int num = (int)((interval.End - time).Seconds * (double)base.SampleRate).SafeFloor();
			if (num >= outputBuffer.Count)
			{
				return null;
			}
			int num2 = (int)((interval.Start - time).Seconds * (double)base.SampleRate).SafeCeiling();
			Pvt[] array = new Pvt[num - num2 + 1];
			int num3;
			int num4;
			if (num2 >= 0)
			{
				num3 = num2;
				num4 = 0;
			}
			else
			{
				num3 = 0;
				num4 = -num2;
			}
			for (int i = num3; i <= num; i++)
			{
				array[num4++] = outputBuffer.ReverseElementAt(i);
			}
			if (num2 < 0)
			{
				Pvt firstSample = outputBuffer.ReverseElementAt(0);
				Pvt secondSample = outputBuffer.ReverseElementAt(1);
				int num5;
				int num6;
				if (num >= 0)
				{
					num5 = -num2 - 1;
					num6 = -1;
				}
				else
				{
					num5 = -num2 + num;
					num6 = num;
				}
				for (int num7 = num5; num7 >= 0; num7--)
				{
					int num8 = num7;
					int sampleIndex = num6--;
					array[num8] = ExtrapolateLinear(in firstSample, in secondSample, in sampleIndex);
				}
			}
			return array;
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!isDisposed)
		{
			isDisposed = true;
			if (disposing)
			{
				inputTimer.Dispose();
				outputTimer.Dispose();
			}
		}
	}
}
