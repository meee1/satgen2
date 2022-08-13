using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Racelogic.Comms.Serial;
using Racelogic.Core;
using Racelogic.DataSource.Nmea;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Maths;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen
{
	public class LiveNmeaTrajectory : Trajectory, ILiveTrajectory
	{
		private enum VelocitySource
		{
			None,
			RMC,
			VTG
		}

		private const int MinInputSampleCount = 11;

		private const double MinutesToDegrees = 0.016666666666666666;

		private readonly ComPort port;

		private const int MaxSampleRate = 100;

		private readonly bool useSourceSampleRate = true;

		protected readonly FixedSizeStack<Pvt> inputBuffer = new FixedSizeStack<Pvt>(200);

		private readonly FixedSizeDictionary<GnssTime, double> velocityBuffer = new FixedSizeDictionary<GnssTime, double>(5);

		protected readonly FixedSizeStack<Pvt> outputBuffer = new FixedSizeStack<Pvt>(1000);

		private VelocitySource velocitySource;

		private GnssTime ggaMidnight;

		private GnssTime rmcMidnight;

		private GnssTime vtgMidnight;

		private double lastGgaSecondOfDay;

		private double lastRmcSecondOfDay;

		private double lastVtgSecondOfDay;

		private readonly Vector3D[] previousInputVelocityVectors = new Vector3D[3];

		private readonly GravitationalModel gravitationalModel;

		private const int lockTimeout = 10000;

		private readonly SyncLock sampleLock = new SyncLock("LiveNmeaSampleLock", 10000);

		private readonly Timer outputTimer;

		private bool isOutputTimerActive;

		private GnssTime outputTime;

		private int outputAdvance;

		private int outputAdvanceCounter;

		private bool isDisposed;

		public LiveNmeaTrajectory(in GnssTime startDate, string portName, SerialBaudRate baudRate, in int sampleRate = 0, GravitationalModel gravitationalModel = GravitationalModel.Wgs84)
		{
			base.IsExternal = true;
			outputTimer = new Timer(new TimerCallback(OnOutputTimerTick));
			DateTime utcTime = startDate.UtcTime;
			DateTime utcDateTime = utcTime - utcTime.TimeOfDay;
			ggaMidnight = GnssTime.FromUtc(utcDateTime);
			rmcMidnight = ggaMidnight;
			if (sampleRate > 0)
			{
				base.SampleRate = sampleRate;
				useSourceSampleRate = false;
			}
			this.gravitationalModel = gravitationalModel;
			Interval = new Range<GnssTime, GnssTimeSpan>(ggaMidnight, GnssTime.MaxValue);
			port = new ComPort(portName)
			{
				//BaudRate = baudRate
			};
			//if (port.Open())
			{
				//port.PropertyChanged += OnPortPropertyChanged;
				//return;
			}
			base.ErrorMessage = $"ERROR: Can't open serial port {portName} at {(int)baudRate} bps";
			RLLogger.GetLogger().LogMessage(base.ErrorMessage);
		}

		private void OnPortPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "NmeaData")
			{
				//ProcessNmeaSample(port.NmeaData);
			}
		}

		private void ProcessNmeaSample(NmeaData nmea)
		{
			(GnssTime, double)? tuple = null;
			if (velocitySource == VelocitySource.RMC || velocitySource == VelocitySource.None)
			{
				tuple = ReadRmcVelocity(nmea);
				if (velocitySource == VelocitySource.None && tuple.HasValue)
				{
					velocitySource = VelocitySource.RMC;
				}
			}
			else if (!tuple.HasValue && (velocitySource == VelocitySource.VTG || velocitySource == VelocitySource.None))
			{
				tuple = ReadVtgVelocity(nmea);
				if (velocitySource == VelocitySource.None && tuple.HasValue)
				{
					velocitySource = VelocitySource.VTG;
				}
			}
			if (tuple.HasValue)
			{
				using (sampleLock.Lock())
				{
					velocityBuffer.Add(tuple.Value.Item1, tuple.Value.Item2);
					if (tuple.Value.Item1 == inputBuffer.FirstOrDefault().Time)
					{
						ProcessNewSample();
					}
				}
				return;
			}
			Pvt? pvt = ReadGgaSample(nmea);
			if (!pvt.HasValue)
			{
				return;
			}
			using (sampleLock.Lock())
			{
				inputBuffer.Push(pvt.Value);
				if (velocitySource == VelocitySource.None || velocityBuffer.ContainsKey(pvt.Value.Time))
				{
					ProcessNewSample();
				}
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

		private Pvt? ReadGgaSample(NmeaData nmea)
		{
			GgaData gGA = nmea.GGA;
			if (gGA.FixQuality == NmeaFixQuality.NoData || gGA.FixQuality == NmeaFixQuality.Invalid)
			{
				return null;
			}
			double num = gGA.UtcFixTime;
			if (num == lastGgaSecondOfDay)
			{
				return null;
			}
			if (num <= 2.0 && lastGgaSecondOfDay >= 86397.0)
			{
				ggaMidnight += GnssTimeSpan.FromDays(1);
			}
			lastGgaSecondOfDay = num;
			GnssTime time = ggaMidnight + GnssTimeSpan.FromSeconds(num);
			double latitude = (double)gGA.LatitudeMinutes * 0.016666666666666666;
			double longitude = (double)gGA.LongitudeMinutes * 0.016666666666666666;
			double height = gGA.AltitudeAboveMeanSeaLevel;
			double geoidHeight = gGA.HeightOfGeoidAboveWGS84Eellipsoid;
			Ecef ecef = Ecef.FromCoordinates(latitude, longitude, height, geoidHeight, gravitationalModel);
			return new Pvt(in time, in ecef);
		}

		private (GnssTime Time, double Velocity)? ReadRmcVelocity(NmeaData nmea)
		{
			RmcData rMC = nmea.RMC;
			if (rMC.Status != NmeaActiveIndicator.Active)
			{
				return null;
			}
			double num = rMC.UtcFixTime;
			if (num == lastRmcSecondOfDay)
			{
				return null;
			}
			if (num <= 2.0 && lastRmcSecondOfDay >= 86397.0)
			{
				rmcMidnight += GnssTimeSpan.FromDays(1);
			}
			lastRmcSecondOfDay = num;
			GnssTime item3 = rmcMidnight + GnssTimeSpan.FromSeconds(num);
			double item2 = rMC.SpeedOverGroundKnots * (463.0 / 900.0);
			return new(GnssTime, double)?((item3, item2));
		}

		private (GnssTime Time, double Velocity)? ReadVtgVelocity(NmeaData nmea)
		{
			VtgData vTG = nmea.VTG;
			if (vTG.ModeIndicator == NmeaModeIndicator.NoData || vTG.ModeIndicator == NmeaModeIndicator.NotValid)
			{
				return null;
			}
			double num = lastGgaSecondOfDay;
			if (num == lastVtgSecondOfDay)
			{
				return null;
			}
			if (num <= 2.0 && lastVtgSecondOfDay >= 86397.0)
			{
				vtgMidnight += GnssTimeSpan.FromDays(1);
			}
			lastVtgSecondOfDay = num;
			GnssTime item3 = vtgMidnight + GnssTimeSpan.FromSeconds(num);
			double item2 = vTG.GroundSpeedKnots * (463.0 / 900.0);
			return new(GnssTime, double)?((item3, item2));
		}

		private static Vector3D AdjustVelocity(Vector3D velocityVector, in Vector3D lastVelocityVector, in Ecef ecefPosition, in double groundVelocity)
		{
			if (groundVelocity < 0.5)
			{
				return velocityVector;
			}
			bool flag = false;
			if (velocityVector.Magnitude() < 0.1 && lastVelocityVector.Magnitude() > 0.1)
			{
				velocityVector = lastVelocityVector;
				flag = true;
			}
			Geodetic referenceLocation = ecefPosition.ToGeodetic();
			double num = new Ecef(in velocityVector, isAbsolute: false).ToNed(in referenceLocation).FlatMagnitude();
			double num2 = groundVelocity / num;
			if (double.IsNaN(num2))
			{
				return velocityVector;
			}
			num2 = ((!flag) ? Math.Clamp(num2, 0.5, 2.0) : Math.Clamp(num2, 0.1, 10.0));
			velocityVector *= num2;
			return velocityVector;
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
				Ecef ecefPosition = pvt2.Ecef;
				GnssTime time2 = pvt2.Time;
				double seconds = (time2 - time).Seconds;
				double x = akimaSpline.Differentiate(seconds);
				double y = akimaSpline2.Differentiate(seconds);
				double z = akimaSpline3.Differentiate(seconds);
				Vector3D velocity = new Vector3D(x, y, z);
				if (velocityBuffer.TryGetValue(time2, out var value))
				{
					velocity = AdjustVelocity(velocity, in previousInputVelocityVectors[num], in ecefPosition, in value);
					previousInputVelocityVectors[num] = velocity;
				}
				Ecef ecef = new Ecef(in ecefPosition.Position, in velocity);
				Pvt newItem = new Pvt(in time2, in ecef);
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
			if (inputBuffer.Count == 11 && useSourceSampleRate && !DetermineSampleRate())
			{
				return false;
			}
			if (useSourceSampleRate)
			{
				GnssTime lastOutputTime2 = (outputBuffer.Any() ? outputBuffer.Peek().Time : Interval.Start);
				foreach (Pvt item2 in inputBuffer.Skip(2).TakeWhile((Pvt s) => s.Time > lastOutputTime2).Reverse())
				{
					outputBuffer.Push(item2);
				}
			}
			else
			{
				GnssTime lastOutputTime;
				if (outputBuffer.Any())
				{
					lastOutputTime = outputBuffer.Peek().Time;
				}
				else
				{
					GnssTime time = inputBuffer.Reverse().Skip(2).First()
						.Time;
					GnssTime gnssTime = GnssTime.FromGps(time.GpsWeek, 0);
					GnssTimeSpan gnssTimeSpan = (time - gnssTime).Nanoseconds / base.SampleSpan.Nanoseconds * base.SampleSpan;
					lastOutputTime = gnssTime + gnssTimeSpan;
					if (lastOutputTime < time)
					{
						lastOutputTime += base.SampleSpan;
					}
				}
				GnssTime time2 = inputBuffer.Skip(2).First().Time;
				int num = inputBuffer.TakeWhile((Pvt s) => s.Time > lastOutputTime).Count() + 2 + 1;
				double[] array = new double[num];
				double[] array2 = new double[num];
				double[] array3 = new double[num];
				double[] array4 = new double[num];
				double[] array5 = new double[num];
				double[] array6 = new double[num];
				double[] array7 = new double[num];
				for (int i = 0; i < num; i++)
				{
					Pvt pvt = inputBuffer[num - 1 - i];
					array[i] = (pvt.Time - lastOutputTime).Seconds;
					Vector3D position = pvt.Ecef.Position;
					array2[i] = position.X;
					array3[i] = position.Y;
					array4[i] = position.Z;
					Vector3D velocity = pvt.Ecef.Velocity;
					array5[i] = velocity.X;
					array6[i] = velocity.Y;
					array7[i] = velocity.Z;
				}
				AkimaSpline akimaSpline = new AkimaSpline(array, array2, isConstantRate: true);
				AkimaSpline akimaSpline2 = new AkimaSpline(array, array3, isConstantRate: true);
				AkimaSpline akimaSpline3 = new AkimaSpline(array, array4, isConstantRate: true);
				AkimaSpline akimaSpline4 = new AkimaSpline(array, array5, isConstantRate: true);
				AkimaSpline akimaSpline5 = new AkimaSpline(array, array6, isConstantRate: true);
				AkimaSpline akimaSpline6 = new AkimaSpline(array, array7, isConstantRate: true);
				for (GnssTime time3 = lastOutputTime + base.SampleSpan; time3 <= time2; time3 += base.SampleSpan)
				{
					double seconds = (time3 - lastOutputTime).Seconds;
					double x = akimaSpline.Interpolate(seconds);
					double y = akimaSpline2.Interpolate(seconds);
					double z = akimaSpline3.Interpolate(seconds);
					Vector3D position2 = new Vector3D(x, y, z);
					double x2 = akimaSpline4.Interpolate(seconds);
					double y2 = akimaSpline5.Interpolate(seconds);
					double z2 = akimaSpline6.Interpolate(seconds);
					Vector3D velocity2 = new Vector3D(x2, y2, z2);
					Ecef ecef = new Ecef(in position2, in velocity2);
					Pvt item = new Pvt(in time3, in ecef);
					outputBuffer.Push(item);
				}
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
			}
			GnssTime start = Interval.Start;
			GnssTime time = outputBuffer.Peek().Time;
			Interval = new Range<GnssTime, GnssTimeSpan>(start, time);
			if (time < outputTime)
			{
				RLLogger.GetLogger().LogMessage($"Extrapolate {(outputTime - time).Seconds} s");
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
					outputAdvanceCounter = 3;
				}
			}
		}

		private bool DetermineSampleRate()
		{
			double num = Math.Abs(inputBuffer.SelectPair((Pvt prev, Pvt cur) => (cur.Time - prev.Time).Seconds).Median());
			if (double.IsNaN(num) || num == 0.0 || num > 1.0)
			{
				base.ErrorMessage = "Sampling rate of the NMEA stream cannot be determined.";
				return false;
			}
			int num2 = (int)Math.Round(1.0 / num);
			if (num2 == 0 || !num.AlmostEquals(1.0 / (double)num2))
			{
				base.ErrorMessage = "Sampling rate of the NMEA stream is not a whole number.";
				return false;
			}
			base.SampleRate = num2;
			return true;
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
			if (isDisposed)
			{
				return;
			}
			isDisposed = true;
			if (disposing)
			{
				outputTimer.Dispose();
				//if (port != null)
				{
				//	port.PropertyChanged -= OnPortPropertyChanged;
				//	port.ClosePortAndCleanup();
				}
			}
		}
	}
}
