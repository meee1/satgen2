namespace Racelogic.Gnss.SatGen;

internal readonly struct SignalParams
{
	private readonly AkimaCoeff[] phaseAccumulatorCoefficients;

	private readonly AkimaCoeff[] chipIndexInterpolatorCoefficients;

	private unsafe readonly sbyte* modulationPointer;

	private readonly double signalLevel;

	public static readonly SignalParams Empty;

	public AkimaCoeff[] PhaseAccumulatorCoefficients => phaseAccumulatorCoefficients;

	public AkimaCoeff[] ChipIndexInterpolatorCoefficients => chipIndexInterpolatorCoefficients;

	internal unsafe sbyte* ModulationPointer => modulationPointer;

	public double SignalLevel => signalLevel;

	public bool IsEmpty => SignalLevel == 0.0;

	public unsafe SignalParams(AkimaCoeff[] phaseAccumulatorCoeffs, AkimaCoeff[] chipIndexInterpolatorCoeffs, sbyte* modulationPointer, in double signalLevel)
	{
		phaseAccumulatorCoefficients = phaseAccumulatorCoeffs;
		chipIndexInterpolatorCoefficients = chipIndexInterpolatorCoeffs;
		this.modulationPointer = modulationPointer;
		this.signalLevel = signalLevel;
	}
}
