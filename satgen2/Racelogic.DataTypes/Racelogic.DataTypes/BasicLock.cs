using System;
using System.Diagnostics;
using System.Threading;

namespace Racelogic.DataTypes;

public class BasicLock
{
	private readonly int defaultTimeout;

	private readonly string name;

	private readonly object monitor = new object();

	public int DefaultTimeout
	{
		[DebuggerStepThrough]
		get
		{
			return defaultTimeout;
		}
	}

	public string Name
	{
		[DebuggerStepThrough]
		get
		{
			return name;
		}
	}

	public object Monitor
	{
		[DebuggerStepThrough]
		get
		{
			return monitor;
		}
	}

	public BasicLock()
		: this(null, -1)
	{
	}

	public BasicLock(string name)
		: this(name, -1)
	{
	}

	public BasicLock(int defaultTimeout)
		: this(null, defaultTimeout)
	{
	}

	public BasicLock(string name, int defaultTimeout)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = "Anonymous Lock";
		}
		if (defaultTimeout < -1 || defaultTimeout > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("defaultTimeout", "Invalid timeout specified, the value must be greater or equal zero.");
		}
		this.name = name;
		this.defaultTimeout = defaultTimeout;
	}

	public LockToken Lock()
	{
		return Lock(defaultTimeout);
	}

	public LockToken Lock(TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", "Invalid timeout specified");
		}
		return Lock((int)num);
	}

	public virtual LockToken Lock(int timeout)
	{
		if (timeout < -1 || timeout > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", "Invalid timeout specified");
		}
		if (!System.Threading.Monitor.TryEnter(monitor, timeout))
		{
			throw new LockTimeoutException("Failed to acquire lock {0}", name);
		}
		return new LockToken(this);
	}

	protected internal virtual void Unlock()
	{
		System.Threading.Monitor.Exit(monitor);
	}
}
