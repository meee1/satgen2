using System.Diagnostics;
using System.Threading;

namespace Racelogic.DataTypes;

public class OrderLock : SyncLock
{
	private volatile OrderLock innerLock;

	public OrderLock InnerLock
	{
		[DebuggerStepThrough]
		get
		{
			return innerLock;
		}
		[DebuggerStepThrough]
		set
		{
			innerLock = value;
		}
	}

	public OrderLock()
	{
	}

	public OrderLock(string name)
		: base(name)
	{
	}

	public OrderLock(int defaultTimeout)
		: base(defaultTimeout)
	{
	}

	public OrderLock(string name, int defaultTimeout)
		: base(name, defaultTimeout)
	{
	}

	public OrderLock SetInnerLock(OrderLock inner)
	{
		InnerLock = inner;
		return this;
	}

	public override LockToken Lock(int timeout)
	{
		OrderLock orderLock = InnerLock;
		if (orderLock != null)
		{
			Thread currentThread = Thread.CurrentThread;
			if (base.Owner != currentThread)
			{
				while (orderLock != null)
				{
					if (orderLock.Owner == currentThread)
					{
						string stackTrace = GetStackTrace(orderLock.Owner);
						throw new LockOrderException("Unable to acquire lock \"{0}\" for thread \"{1}\" as lock \"{2}\" is already held by thread \"{3}\"\r\nThe stack trace of lock \"{2}\" holder (\"{3}\"):\r\n{4}", base.Name, currentThread.Name, orderLock.Name, orderLock.Owner.Name, stackTrace);
					}
					orderLock = orderLock.InnerLock;
				}
			}
		}
		return base.Lock(timeout);
	}
}
