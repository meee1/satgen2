using System;

namespace Racelogic.Utilities;

public class FakeRLLoggerForIntegrationTest : RLLogger, IRLLogger
{
	public bool CalledLogException { get; private set; }

	public bool CalledLogMessage { get; private set; }

	public bool CalledLogShortMessage { get; private set; }

	public string LastMessageProvided { get; private set; }

	public bool UnhandledErrorLogged { get; private set; }

	public override void LogException(Exception ex, bool handled = false)
	{
		base.LogException(ex, handled);
		CalledLogException = true;
	}

	public override void LogMessage(string message)
	{
		base.LogMessage(message);
		CalledLogMessage = true;
	}

	public override void LogShortMessage(string message)
	{
		base.LogShortMessage(message);
		CalledLogException = true;
	}

	protected override void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		base.OnCurrentDomainUnhandledException(sender, e);
		UnhandledErrorLogged = true;
	}

	public new static IRLLogger GetLogger()
	{
		if (RLLogger._instance == null)
		{
			RLLogger._instance = new FakeRLLoggerForIntegrationTest();
		}
		return RLLogger._instance;
	}
}
