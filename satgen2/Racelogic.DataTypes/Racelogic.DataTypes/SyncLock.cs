using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Racelogic.DataTypes;

public class SyncLock : BasicLock
{
	private int lockCount;

	private volatile Thread owner;

	protected const int maxStackCount = 25;

	public Thread Owner
	{
		[DebuggerStepThrough]
		get
		{
			return owner;
		}
	}

	public SyncLock()
	{
	}

	public SyncLock(string name)
		: base(name)
	{
	}

	public SyncLock(int defaultTimeout)
		: base(defaultTimeout)
	{
	}

	public SyncLock(string name, int defaultTimeout)
		: base(name, defaultTimeout)
	{
	}

	public override LockToken Lock(int timeout)
	{
		LockToken result;
		try
		{
			result = base.Lock(timeout);
		}
		catch (LockTimeoutException)
		{
			string text;
			if (owner != null)
			{
				text = "thread \"" + owner.Name + "\"";
				if (owner.IsThreadPoolThread)
				{
					text = "threadpool " + text;
				}
			}
			else
			{
				text = "No Owner";
			}
			string text2 = "thread \"" + Thread.CurrentThread.Name + "\"";
			if (Thread.CurrentThread.IsThreadPoolThread)
			{
				text2 = "threadpool " + text2;
			}
			string stackTrace = GetStackTrace(owner);
			throw new LockTimeoutException("Failed to acquire lock \"{0}\" for {1} within timeout {2}ms. The lock has already been acquired by {3}.\nThe stack trace of the lock holder:\n{4}", base.Name, text2, timeout, text, stackTrace);
		}
		if (Interlocked.Increment(ref lockCount) == 1)
		{
			owner = Thread.CurrentThread;
		}
		return result;
	}

	protected internal override void Unlock()
	{
		base.Unlock();
		if (Interlocked.Decrement(ref lockCount) == 0)
		{
			owner = null;
		}
	}

	protected virtual string GetStackTrace(Thread thread)
	{
		if (thread != Thread.CurrentThread)
		{
			return string.Empty;
		}
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		StringBuilder stringBuilder = new StringBuilder();
		StackFrame frame = stackTrace.GetFrame(1);
		stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "  At line {0} of {1}\n", frame.GetFileLineNumber(), frame.GetFileName()));
		for (int i = 1; i < stackTrace.FrameCount && i < 25; i++)
		{
			frame = stackTrace.GetFrame(i);
			MethodBase method = frame.GetMethod();
			stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "  {0}.{1}()\n", method.DeclaringType, method.Name));
		}
		return stringBuilder.ToString();
	}
}
