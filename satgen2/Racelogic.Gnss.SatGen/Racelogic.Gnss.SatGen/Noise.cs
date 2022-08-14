using System;
using System.Collections.Generic;
using System.Linq;
using Racelogic.DataTypes;
using Racelogic.Utilities;

namespace Racelogic.Gnss.SatGen;

internal static class Noise
{
	private static readonly double[] gaussianNoise = (from i in Enumerable.Range(0, 862)
		select 10.0 * RandomProvider.GaussianNoise()).ToArray(862);

	private static readonly Dictionary<FrequencyBand, int> noiseIndexOffsets = new Dictionary<FrequencyBand, int>();

	private const int noiseIndexOffsetIncrement = 158;

	private static int newNoiseIndexOffset;

	private static readonly Dictionary<FrequencyBand, FixedSizeDictionary<double, AlignedBuffer<double>>> generalNoiseBuffer = new Dictionary<FrequencyBand, FixedSizeDictionary<double, AlignedBuffer<double>>>();

	private static readonly Dictionary<FrequencyBand, FixedSizeDictionary<double, AlignedBuffer<double>>> galileoNoiseBuffer = new Dictionary<FrequencyBand, FixedSizeDictionary<double, AlignedBuffer<double>>>();

	private static readonly Dictionary<FrequencyBand, FixedSizeDictionary<double, AlignedBuffer<double>>> navicNoiseBuffer = new Dictionary<FrequencyBand, FixedSizeDictionary<double, AlignedBuffer<double>>>();

	private static readonly Dictionary<FrequencyBand, (double Level, double Attenuation)[]> generalNoisePoints = new Dictionary<FrequencyBand, (double, double)[]>
	{
		[FrequencyBand.GalileoE1] = new(double, double)[29]
		{
			(6.0, -15.4),
			(5.5, -15.0),
			(5.0, -14.6),
			(4.5, -14.2),
			(4.0, -13.4),
			(3.6, -13.0),
			(3.3, -12.2),
			(3.0, -12.0),
			(2.8, -10.9),
			(2.7, -9.4),
			(2.4, -9.0),
			(2.2, -8.0),
			(2.0, -7.8),
			(1.8, -7.6),
			(1.6, -6.5),
			(1.4, -6.3),
			(1.2, -6.0),
			(1.1, -5.0),
			(1.0, -4.2),
			(0.9, -3.7),
			(0.8, -3.2),
			(0.7, -2.9),
			(0.6, -2.2),
			(0.5, -2.0),
			(0.4, -1.5),
			(0.3, -1.2),
			(0.2, -1.0),
			(0.1, -0.2),
			(0.0, 0.0)
		},
		[FrequencyBand.GpsL2] = new(double, double)[36]
		{
			(9.0, -26.2),
			(8.0, -26.1),
			(7.5, -26.0),
			(7.0, -25.9),
			(6.5, -25.5),
			(6.0, -24.3),
			(5.5, -22.9),
			(5.0, -21.5),
			(4.5, -20.6),
			(4.0, -19.7),
			(3.6, -19.0),
			(3.3, -18.5),
			(3.0, -17.8),
			(2.8, -17.3),
			(2.6, -16.8),
			(2.4, -16.2),
			(2.2, -15.4),
			(2.0, -14.6),
			(1.8, -13.4),
			(1.6, -12.5),
			(1.5, -12.1),
			(1.4, -11.7),
			(1.3, -11.4),
			(1.2, -11.0),
			(1.1, -10.6),
			(1.0, -9.9),
			(0.9, -9.4),
			(0.8, -8.8),
			(0.7, -8.1),
			(0.6, -7.5),
			(0.5, -6.7),
			(0.4, -5.6),
			(0.3, -4.1),
			(0.2, -2.2),
			(0.1, -0.6),
			(0.0, 0.0)
		},
		[FrequencyBand.NavicL5] = new(double, double)[35]
		{
			(9.0, -27.0),
			(7.5, -25.5),
			(7.0, -25.1),
			(6.5, -24.9),
			(6.0, -24.3),
			(5.5, -23.1),
			(5.0, -21.8),
			(4.5, -20.8),
			(4.0, -19.7),
			(3.6, -19.0),
			(3.3, -18.4),
			(3.0, -17.9),
			(2.8, -17.4),
			(2.6, -16.9),
			(2.4, -16.3),
			(2.2, -15.5),
			(2.0, -14.6),
			(1.8, -13.4),
			(1.6, -12.5),
			(1.5, -12.1),
			(1.4, -11.7),
			(1.3, -11.2),
			(1.2, -10.7),
			(1.1, -10.1),
			(1.0, -9.5),
			(0.9, -8.8),
			(0.8, -8.0),
			(0.7, -7.1),
			(0.6, -6.2),
			(0.5, -5.1),
			(0.4, -3.8),
			(0.3, -2.5),
			(0.2, -1.3),
			(0.1, -0.3),
			(0.0, 0.0)
		},
		[FrequencyBand.GalileoE6] = new(double, double)[20]
		{
			(5.0, -23.7),
			(4.0, -22.2),
			(3.5, -21.1),
			(3.0, -19.9),
			(2.5, -18.4),
			(2.0, -17.0),
			(1.7, -15.5),
			(1.4, -13.9),
			(1.2, -12.5),
			(1.0, -11.3),
			(0.9, -10.3),
			(0.8, -9.3),
			(0.7, -8.2),
			(0.6, -7.2),
			(0.5, -5.9),
			(0.4, -4.5),
			(0.3, -3.1),
			(0.2, -2.0),
			(0.1, -0.9),
			(0.0, 0.0)
		},
		[FrequencyBand.GlonassL1] = new(double, double)[29]
		{
			(8.0, -25.0),
			(7.0, -23.8),
			(6.0, -22.7),
			(5.0, -21.2),
			(4.5, -20.2),
			(4.0, -19.3),
			(3.6, -19.0),
			(3.3, -18.9),
			(3.0, -18.2),
			(2.7, -16.6),
			(2.4, -14.7),
			(2.2, -13.9),
			(2.0, -13.6),
			(1.8, -12.9),
			(1.6, -12.3),
			(1.4, -10.5),
			(1.2, -9.6),
			(1.1, -9.1),
			(1.0, -8.1),
			(0.9, -7.6),
			(0.8, -7.1),
			(0.7, -6.2),
			(0.6, -5.2),
			(0.5, -4.2),
			(0.4, -3.2),
			(0.3, -2.2),
			(0.2, -1.0),
			(0.1, -0.3),
			(0.0, 0.0)
		},
		[FrequencyBand.GlonassL2] = new(double, double)[29]
		{
			(8.0, -28.5),
			(7.0, -26.8),
			(6.0, -25.3),
			(5.0, -24.0),
			(4.5, -23.2),
			(4.0, -22.1),
			(3.6, -21.2),
			(3.3, -20.4),
			(3.0, -19.3),
			(2.7, -18.4),
			(2.4, -17.5),
			(2.2, -16.9),
			(2.0, -16.0),
			(1.8, -15.3),
			(1.6, -14.6),
			(1.4, -13.5),
			(1.2, -12.4),
			(1.1, -11.6),
			(1.0, -11.0),
			(0.9, -10.3),
			(0.8, -9.6),
			(0.7, -8.9),
			(0.6, -8.1),
			(0.5, -7.2),
			(0.4, -6.0),
			(0.3, -4.5),
			(0.2, -2.9),
			(0.1, -0.8),
			(0.0, 0.0)
		},
		[FrequencyBand.BeiDouB1] = new(double, double)[23]
		{
			(4.0, -15.1),
			(3.5, -14.0),
			(3.0, -13.1),
			(2.5, -12.1),
			(2.3, -11.2),
			(2.0, -10.8),
			(1.9, -9.3),
			(1.8, -9.1),
			(1.65, -8.7),
			(1.5, -8.1),
			(1.35, -7.5),
			(1.2, -6.5),
			(1.0, -6.1),
			(0.9, -5.3),
			(0.8, -4.9),
			(0.7, -3.8),
			(0.6, -3.3),
			(0.5, -2.9),
			(0.4, -2.0),
			(0.3, -1.2),
			(0.25, -1.0),
			(0.1, -0.2),
			(0.0, 0.0)
		},
		[FrequencyBand.BeiDouB2] = new(double, double)[23]
		{
			(4.0, -19.2),
			(3.5, -17.8),
			(3.0, -16.3),
			(2.6, -15.1),
			(2.3, -14.2),
			(2.0, -13.0),
			(1.8, -12.1),
			(1.65, -11.5),
			(1.5, -10.8),
			(1.35, -10.0),
			(1.2, -9.1),
			(1.1, -8.5),
			(1.0, -7.8),
			(0.9, -7.1),
			(0.8, -6.4),
			(0.7, -5.6),
			(0.6, -4.9),
			(0.5, -4.1),
			(0.4, -3.2),
			(0.3, -2.3),
			(0.2, -1.2),
			(0.1, -0.3),
			(0.0, 0.0)
		},
		[FrequencyBand.BeiDouB3] = new(double, double)[21]
		{
			(6.0, -25.3),
			(5.0, -24.1),
			(4.0, -22.3),
			(3.5, -20.8),
			(3.0, -19.5),
			(2.5, -18.2),
			(2.0, -16.6),
			(1.7, -15.0),
			(1.4, -13.5),
			(1.2, -12.2),
			(1.0, -10.9),
			(0.9, -9.7),
			(0.8, -8.8),
			(0.7, -7.7),
			(0.6, -6.7),
			(0.5, -5.4),
			(0.4, -4.3),
			(0.3, -2.9),
			(0.2, -1.7),
			(0.1, -0.8),
			(0.0, 0.0)
		},
		[FrequencyBand.NavicS] = new(double, double)[14]
		{
			(3.9, -18.2),
			(3.6, -16.8),
			(3.3, -15.8),
			(3.0, -14.8),
			(2.7, -14.0),
			(2.4, -13.3),
			(2.1, -12.3),
			(1.8, -10.9),
			(1.5, -9.5),
			(1.2, -7.7),
			(0.9, -5.7),
			(0.6, -3.4),
			(0.3, -1.2),
			(0.0, 0.0)
		}
	};

	private static readonly Dictionary<FrequencyBand, (double Level, double Attenuation)[]> galileoNoisePoints = new Dictionary<FrequencyBand, (double, double)[]>
	{
		[FrequencyBand.NavicL5] = new(double, double)[23]
		{
			(8.0, -24.9),
			(7.0, -23.9),
			(6.0, -22.4),
			(5.0, -20.7),
			(4.0, -18.8),
			(3.5, -17.3),
			(3.0, -16.2),
			(2.5, -14.8),
			(2.0, -13.2),
			(1.7, -11.7),
			(1.4, -10.5),
			(1.2, -9.3),
			(1.0, -7.9),
			(0.9, -6.8),
			(0.8, -6.0),
			(0.7, -5.2),
			(0.6, -4.5),
			(0.5, -3.5),
			(0.4, -2.6),
			(0.3, -1.6),
			(0.2, -0.8),
			(0.1, -0.3),
			(0.0, 0.0)
		},
		[FrequencyBand.BeiDouB2] = new(double, double)[23]
		{
			(8.0, -25.3),
			(7.0, -24.2),
			(6.0, -22.7),
			(5.0, -21.1),
			(4.0, -19.1),
			(3.5, -17.5),
			(3.0, -16.5),
			(2.5, -15.2),
			(2.0, -13.8),
			(1.7, -12.4),
			(1.4, -10.7),
			(1.2, -9.4),
			(1.0, -8.2),
			(0.9, -7.3),
			(0.8, -6.4),
			(0.7, -5.6),
			(0.6, -4.7),
			(0.5, -3.7),
			(0.4, -2.9),
			(0.3, -2.1),
			(0.2, -1.4),
			(0.1, -0.6),
			(0.0, 0.0)
		}
	};

	private static readonly Dictionary<FrequencyBand, (double Level, double Attenuation)[]> navicNoisePoints = new Dictionary<FrequencyBand, (double, double)[]> { [FrequencyBand.NavicL5] = new(double, double)[14]
	{
		(3.9, -18.2),
		(3.6, -16.8),
		(3.3, -15.8),
		(3.0, -14.8),
		(2.7, -14.0),
		(2.4, -13.3),
		(2.1, -12.3),
		(1.8, -10.9),
		(1.5, -9.5),
		(1.2, -7.7),
		(0.9, -5.7),
		(0.6, -3.4),
		(0.3, -1.2),
		(0.0, 0.0)
	} };

	public const int NoisePeriod = 862;

	public static AlignedBuffer<double> GetNoiseSamples(FrequencyBand frequencyBand, in double attenuation, IEnumerable<Signal> signals)
	{
		if (attenuation >= 0.0)
		{
			return AlignedBuffer<double>.Empty;
		}
		(double, double)[] noisePoints = generalNoisePoints[frequencyBand];
		Dictionary<FrequencyBand, FixedSizeDictionary<double, AlignedBuffer<double>>> dictionary = generalNoiseBuffer;
		SignalType[] source = Signal.GetIndividualSignalTypes(signals.Select((Signal s) => s.SignalType)).ToArray();
		if ((frequencyBand == FrequencyBand.NavicL5 && (source.Contains(SignalType.GalileoE5aI) || source.Contains(SignalType.GalileoE5aQ)) && ((!source.Contains(SignalType.GpsL5I) && !source.Contains(SignalType.GpsL5Q)) || (source.Contains(SignalType.GalileoE1BC) && !source.Contains(SignalType.GpsL1CA)))) || (frequencyBand == FrequencyBand.BeiDouB2 && (source.Contains(SignalType.GalileoE5bI) || source.Contains(SignalType.GalileoE5bQ))))
		{
			noisePoints = galileoNoisePoints[frequencyBand];
			dictionary = galileoNoiseBuffer;
		}
		if (frequencyBand == FrequencyBand.NavicL5 && source.Contains(SignalType.NavicL5SPS))
		{
			noisePoints = navicNoisePoints[frequencyBand];
			dictionary = navicNoiseBuffer;
		}
		if (!dictionary.TryGetValue(frequencyBand, out var value))
		{
			value = new FixedSizeDictionary<double, AlignedBuffer<double>>(32);
			value.ItemRecycled += delegate(object? s, ItemRecycledEventArgs<double, AlignedBuffer<double>> e)
			{
				e.Value.Dispose();
			};
			dictionary.Add(frequencyBand, value);
		}
		if (!value.TryGetValue(attenuation, out var value2))
		{
			if (!noiseIndexOffsets.TryGetValue(frequencyBand, out var value3))
			{
				value3 = newNoiseIndexOffset;
				noiseIndexOffsets.Add(frequencyBand, value3);
				newNoiseIndexOffset += 158;
				if (newNoiseIndexOffset >= 862)
				{
					newNoiseIndexOffset -= 862;
				}
			}
			double noiseLevelForAttenuation = GetNoiseLevelForAttenuation(in attenuation, noisePoints);
			int size = 862;
			value2 = new AlignedBuffer<double>(in size);
			Span<double> span = value2.Span;
			for (int i = 0; i < 862; i++)
			{
				int num = i + value3;
				if (num >= 862)
				{
					num -= 862;
				}
				span[i] = gaussianNoise[num] * noiseLevelForAttenuation;
			}
			value.Add(attenuation, value2);
		}
		return value2;
	}

	private static double GetNoiseLevelForAttenuation(in double attenuation, (double Level, double Attenuation)[] noisePoints)
	{
		for (int i = 1; i < noisePoints.Length; i++)
		{
			double item = noisePoints[i].Attenuation;
			if (attenuation < item)
			{
				double item2 = noisePoints[i].Level;
				double item3 = noisePoints[i - 1].Level;
				double item4 = noisePoints[i - 1].Attenuation;
				return (attenuation - item4) / (item - item4) * (item2 - item3) + item3;
			}
		}
		return 0.0;
	}
}
