using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen
{
	public class JoystickTrajectory : NmeaFileTrajectory
	{
		private const double maxSpeed = 13.888888888888889;

		private const double minSpeed = 5.0 / 9.0;

		private const double maxThrottle = 1.0;

		private const double minThrottle = 0.01;

		private const double maxTurningSpeed = Math.PI / 6.0;

		private const double insensitivity = 0.02;

		private const int bufferLengthSeconds = 15;

		private const int sampleRate = 20;

		private readonly FixedSizeStack<Pvt> sampleBuffer;

		private readonly Joystick joystick;

		private readonly JoystickSlider throttleSlider;

		private GnssTime lastTime;

		private Geodetic lastPosition;

		private double lastHeading;

		private const int lockTimeout = 10000;

		private readonly SyncLock sampleLock = new SyncLock("sampleLock", 10000);

		private Range<GnssTime, GnssTimeSpan> interval;

		private bool isDisposed;

		public sealed override Range<GnssTime, GnssTimeSpan> Interval
		{
			[DebuggerStepThrough]
			get
			{
				using (sampleLock.Lock())
				{
					return interval;
				}
			}
			protected set
			{
				base.Interval = value;
			}
		}

		public JoystickTrajectory(in GnssTime startTime, string nmeaFileName, GravitationalModel gravitationalModel, JoystickSlider throttleSlider = JoystickSlider.Slider1)
			: base(in startTime, nmeaFileName, gravitationalModel)
		{
			base.SampleRate = 20;
			joystick = Joystick.AcquireJoystick(Joystick.FindJoysticks().FirstOrDefault());
			if (joystick == null)
			{
				RLLogger.GetLogger().LogMessage("ERROR: Can't find joystick");
			}
			this.throttleSlider = throttleSlider;
			sampleBuffer = new FixedSizeStack<Pvt>(base.SampleRate * 15);
			Pvt pvt = base.Samples.First();
			base.Samples = null;
			lastPosition = pvt.Ecef.ToGeodetic();
			interval = new Range<GnssTime, GnssTimeSpan>(pvt.Time, GnssTime.MaxValue);
			lastTime = pvt.Time - base.SampleSpan;
		}

		public sealed override IReadOnlyList<Pvt>? GetSamples(in Range<GnssTime, GnssTimeSpan> sectionInterval)
		{
			using (sampleLock.Lock())
			{
				while (lastTime < sectionInterval.End)
				{
					sampleBuffer.Push(GetNextSample());
				}
				Range<GnssTime, GnssTimeSpan> range = new Range<GnssTime, GnssTimeSpan>(Interval.Start, sampleBuffer.Peek().Time);
				using (sampleLock.Lock())
				{
					interval = range;
				}
				GnssTime time = sampleBuffer.ReverseElementAt(0).Time;
				int num = (int)((sectionInterval.Start - time).SecondsDecimal * (decimal)base.SampleRate).SafeCeiling();
				int num4 = (int)((sectionInterval.End - time).SecondsDecimal * (decimal)base.SampleRate).SafeFloor();
				int num5 = ((num4 >= sampleBuffer.Count) ? (sampleBuffer.Count - 1) : num4);
				Pvt[] array = new Pvt[num4 - num + 1];
				int num6;
				int num7;
				if (num >= 0)
				{
					num6 = num;
					num7 = 0;
				}
				else
				{
					num6 = 0;
					num7 = -num;
				}
				for (int i = num6; i <= num5; i++)
				{
					array[num7++] = sampleBuffer.ReverseElementAt(i);
				}
				if (num < 0)
				{
					Pvt firstSample = sampleBuffer.ReverseElementAt(0);
					Pvt secondSample = sampleBuffer.ReverseElementAt(1);
					int num8;
					int num9;
					if (num4 >= 0)
					{
						num8 = -num - 1;
						num9 = -1;
					}
					else
					{
						num8 = -num + num4;
						num9 = num4;
					}
					for (int num10 = num8; num10 >= 0; num10--)
					{
						int num11 = num10;
						int sampleIndex = num9--;
						array[num11] = ExtrapolateLinear(in firstSample, in secondSample, in sampleIndex);
					}
				}
				if (num4 > num5 && num5 > 0)
				{
					Pvt firstSample2 = sampleBuffer.ElementAt(1);
					Pvt secondSample2 = sampleBuffer.ElementAt(0);
					int num12 = num5 - num + 1;
					int num2 = num5 - num + num4 - num5;
					int num3 = 1;
					for (int j = num12; j <= num2; j++)
					{
						int num13 = j;
						int sampleIndex = num3++;
						array[num13] = ExtrapolateLinear(in firstSample2, in secondSample2, in sampleIndex);
					}
				}
				return array;
			}
		}

		private Pvt GetNextSample()
		{
			joystick?.UpdateStatus();
			double num = ReadSlider(throttleSlider);
			double num3 = ((!(num > 0.01)) ? 0.0 : (Math.Exp((num - 0.01) / 0.99 * Math.Log(25.0)) * (5.0 / 9.0)));
			double num4 = ((!(Math.Abs(joystick?.X ?? 0.0) > 0.02)) ? 0.0 : (joystick?.X ?? 0.0)) * (Math.PI / 6.0);
			double num5 = (double)base.SamplePeriod;
			double num6 = FastMath.NormalizeRadiansPi(lastHeading + num4 * num5);
			double num7 = Math.Cos(num6) * num3;
			double north = num7 * num5;
			double num8 = Math.Sin(num6) * num3;
			double east = num8 * num5;
			double num10 = num3 * num5;
			double num9 = lastPosition.ToEcef().Position.Magnitude();
			double num2 = num10 * num10 / (num9 + num9);
			double down = num2 * (double)base.SampleRate;
			Geodetic geodetic = new LocalTangentPlane(north, east, num2).ToEcef(in lastPosition).ToGeodetic().SetAltitude(lastPosition.Altitude);
			Ecef position = geodetic.ToEcef();
			Ecef ecef = new LocalTangentPlane(num7, num8, down).ToRelativeEcef(in position);
			GnssTime time = lastTime + base.SampleSpan;
			Pvt result = new Pvt(in time, in ecef);
			lastHeading = num6;
			lastPosition = geodetic;
			lastTime = time;
			return result;
		}

		private double ReadSlider(JoystickSlider joystickSlider)
		{
			return joystickSlider switch
			{
				JoystickSlider.Slider1 => joystick?.Slider1 ?? 0.0, 
				JoystickSlider.Slider2 => joystick?.Slider2 ?? 0.0, 
				JoystickSlider.ZAxis => joystick?.Z ?? 0.0, 
				_ => joystick?.Slider1 ?? 0.0, 
			};
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!isDisposed)
			{
				isDisposed = true;
				if (disposing)
				{
					joystick?.Dispose();
				}
			}
		}
	}
}
