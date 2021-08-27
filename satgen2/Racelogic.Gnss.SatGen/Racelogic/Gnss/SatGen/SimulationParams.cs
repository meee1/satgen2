using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Geodetics;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen
{
	public sealed class SimulationParams : BasePropertyChanged
	{
		private readonly Trajectory trajectory;

		private Range<GnssTime, GnssTimeSpan> interval;

		private readonly Output output;

		private readonly double? elevationMask;

		private readonly double attenuation;

		private readonly SignalLevelMode signalLevelMode;

		private readonly IReadOnlyList<ConstellationBase> constellations;

		private readonly Signal[] signals;

		private readonly double sliceLength;

		private readonly Dictionary<SignalType, double[]> signalLevels = new Dictionary<SignalType, double[]>();

		private readonly Dictionary<ConstellationType, int> satCountLimits = new Dictionary<ConstellationType, int>();

		private SatCountLimitMode satCountLimitMode = SatCountLimitMode.Automatic;

		private SatCountLimitMode lastSatCountLimitMode = SatCountLimitMode.Automatic;

		private int automaticSatCountLimit = 999;

		public const int DefaultAutomaticSatCountLimit = 999;

		public const int DefaultManualSatCountLimit = 5;

		public IReadOnlyList<Signal> Signals
		{
			[DebuggerStepThrough]
			get
			{
				return signals;
			}
		}

		public IReadOnlyList<ConstellationBase> Constellations
		{
			[DebuggerStepThrough]
			get
			{
				return constellations;
			}
		}

		public Trajectory Trajectory
		{
			[DebuggerStepThrough]
			get
			{
				return trajectory;
			}
		}

		public Range<GnssTime, GnssTimeSpan> Interval
		{
			[DebuggerStepThrough]
			get
			{
				return interval;
			}
			[DebuggerStepThrough]
			internal set
			{
				interval = value;
			}
		}

		public Output Output
		{
			[DebuggerStepThrough]
			get
			{
				return output;
			}
		}

		public ILiveOutput? LiveOutput
		{
			[DebuggerStepThrough]
			get
			{
				return output as ILiveOutput;
			}
		}

		public double SliceLength
		{
			[DebuggerStepThrough]
			get
			{
				return sliceLength;
			}
		}

		public double Attenuation
		{
			[DebuggerStepThrough]
			get
			{
				return attenuation;
			}
		}

		public SignalLevelMode SignalLevelMode
		{
			[DebuggerStepThrough]
			get
			{
				return signalLevelMode;
			}
		}

		public ReadOnlyDictionary<SignalType, double[]> SignalLevels
		{
			[DebuggerStepThrough]
			get
			{
				return new ReadOnlyDictionary<SignalType, double[]>(signalLevels);
			}
		}

		public double? ElevationMask
		{
			[DebuggerStepThrough]
			get
			{
				return elevationMask;
			}
		}

		public SatCountLimitMode SatCountLimitMode
		{
			[DebuggerStepThrough]
			get
			{
				return satCountLimitMode;
			}
			[DebuggerStepThrough]
			set
			{
				satCountLimitMode = value;
				OnPropertyChanged("SatCountLimitMode");
			}
		}

		public SatCountLimitMode LastSatCountLimitMode
		{
			[DebuggerStepThrough]
			get
			{
				return lastSatCountLimitMode;
			}
			[DebuggerStepThrough]
			set
			{
				lastSatCountLimitMode = value;
				OnPropertyChanged("LastSatCountLimitMode");
			}
		}

		public int AutomaticSatCountLimit
		{
			[DebuggerStepThrough]
			get
			{
				return automaticSatCountLimit;
			}
			[DebuggerStepThrough]
			set
			{
				automaticSatCountLimit = value;
				OnPropertyChanged("AutomaticSatCountLimit");
			}
		}

		public Dictionary<ConstellationType, int> SatCountLimits
		{
			[DebuggerStepThrough]
			get
			{
				return satCountLimits;
			}
		}

		public SimulationParams(IReadOnlyList<SignalType> signalTypes, Trajectory trajectory, in Range<GnssTime, GnssTimeSpan> interval, Output output, IReadOnlyList<ConstellationBase> constellations, double? elevationMask, double attenuation = 0.0, SignalLevelMode signalLevelMode = SignalLevelMode.None)
		{
			this.trajectory = trajectory;
			this.interval = interval;
			this.output = output;
			this.constellations = constellations;
			this.elevationMask = elevationMask;
			if (signalTypes == null || !signalTypes.Any())
			{
				signalTypes = new SignalType[1] { SignalType.GpsL1CA };
			}
			signalTypes = Signal.GetIndividualSignalTypes(signalTypes);
			signals = Signal.GetSignals(signalTypes);
			foreach (ConstellationBase constellation in constellations)
			{
				constellation.SignalTypes = signalTypes;
			}
			sliceLength = GetBestSliceLength(signalTypes, output);
			if (attenuation != 0.0 && signals.Select((Signal s) => s.FrequencyBand).Distinct().Count() > 3)
			{
				attenuation = 0.0;
			}
			this.attenuation = attenuation;
			if (signalLevelMode == SignalLevelMode.None)
			{
				signalLevelMode = ((!(output is ILiveOutput)) ? SignalLevelMode.Uniform : SignalLevelMode.Manual);
			}
			this.signalLevelMode = signalLevelMode;
			foreach (SignalType signalType in signalTypes)
			{
				signalLevels.Add(signalType, Enumerable.Repeat(1.0, 50).ToArray(50));
			}
			foreach (ConstellationType item in constellations.Select((ConstellationBase c) => c.ConstellationType))
			{
				satCountLimits.Add(item, 5);
			}
		}

		private static double GetBestSliceLength(IEnumerable<SignalType> signalTypes, Output output)
		{
			ILiveOutput liveOutput = output as ILiveOutput;
			if (liveOutput != null)
			{
				if (liveOutput.IsLowLatency)
				{
					LabSat2Output labSat2Output = output as LabSat2Output;
					if (labSat2Output != null)
					{
						if (labSat2Output.ChannelPlan == null || labSat2Output.ChannelPlan.Channels.Count((Channel ch) => ch != null) == 2 || labSat2Output.ChannelPlan.Quantization == Quantization.TwoBit)
						{
							return 0.1;
						}
						return 0.2;
					}
					return 0.1;
				}
				return 1.0;
			}
			int processorCount = Environment.ProcessorCount;
			int num = signalTypes.Count();
			int num2 = processorCount / num;
			if (num2 >= 12)
			{
				return 6.0;
			}
			if (num2 >= 10)
			{
				return 5.0;
			}
			if (num2 >= 8)
			{
				return 4.0;
			}
			if (num2 >= 6)
			{
				return 3.0;
			}
			if (num2 >= 4)
			{
				return 2.0;
			}
			if (num2 >= 2)
			{
				return 1.0;
			}
			if (num2 >= 1)
			{
				return 0.4;
			}
			return 0.2;
		}
	}
}
