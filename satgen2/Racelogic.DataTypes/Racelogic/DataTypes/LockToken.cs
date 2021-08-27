using System;

namespace Racelogic.DataTypes
{
	public struct LockToken : IDisposable, IEquatable<LockToken>
	{
		private BasicLock parent;

		internal LockToken(BasicLock parent)
		{
			this.parent = parent;
		}

		public override bool Equals(object obj)
		{
			if (obj is LockToken)
			{
				return Equals((LockToken)obj);
			}
			return false;
		}

		public bool Equals(LockToken other)
		{
			return parent == other.parent;
		}

		public static bool operator ==(LockToken struct1, LockToken struct2)
		{
			return struct1.Equals(struct2);
		}

		public static bool operator !=(LockToken struct1, LockToken struct2)
		{
			return !struct1.Equals(struct2);
		}

		public override int GetHashCode()
		{
			int num = base.GetHashCode();
			if (parent != null)
			{
				num ^= parent.GetHashCode();
			}
			return num;
		}

		public void Dispose()
		{
			parent?.Unlock();
			parent = null;
		}
	}
}
