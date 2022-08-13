namespace Racelogic.Utilities;

public static class SystemUtils
{
	public static void SetThreadExecutionMode(ThreadExecutionModes threadExecutionMode = ThreadExecutionModes.None)
	{
		NativeMethods.ExecutionStates executionStates = NativeMethods.ExecutionStates.Continuous;
		if (threadExecutionMode.HasFlag(ThreadExecutionModes.AwayMode))
		{
			executionStates |= NativeMethods.ExecutionStates.AwayModeRequired;
		}
		if (threadExecutionMode.HasFlag(ThreadExecutionModes.KeepDisplayOn))
		{
			executionStates |= NativeMethods.ExecutionStates.DisplayRequired;
		}
		if (threadExecutionMode.HasFlag(ThreadExecutionModes.KeepSystemAwake))
		{
			executionStates |= NativeMethods.ExecutionStates.SystemRequired;
		}
		NativeMethods.SetThreadExecutionState(executionStates);
	}
}
