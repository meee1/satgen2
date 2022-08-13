using System;
using System.Collections.Generic;
using System.Linq;

namespace Racelogic.Comms.Serial;

internal static class StmExtensionMethods
{
	internal static IEnumerable<byte> AddStmChecksum(this IEnumerable<byte> @this)
	{
		return (@this.Count() > 1) ? @this.Append(@this.Aggregate((byte a, byte b) => (byte)(a ^ b))) : @this.Append((byte)(~@this.First()));
	}

	internal static IEnumerable<byte> Add(this IEnumerable<byte> @this, uint data)
	{
		return @this.Append((byte)(data >> 24)).Append((byte)(data >> 16)).Append((byte)(data >> 8))
			.Append((byte)data);
	}

	internal static IEnumerable<byte> Add(this IEnumerable<byte> @this, ushort data)
	{
		return @this.Append((byte)(data >> 8)).Append((byte)data);
	}

	internal static IEnumerable<byte> AddStmCommand(this IEnumerable<byte> @this, StmCommands command)
	{
		return @this.Append((byte)command).Append((byte)(~(uint)command));
	}

	private static IEnumerable<byte> Add<T>(this IEnumerable<byte> @this, IEnumerable<T> data, Func<IEnumerable<byte>, T, IEnumerable<byte>> fn)
	{
		return data.Aggregate(@this, fn);
	}

	internal static IEnumerable<byte> Add(this IEnumerable<byte> @this, List<ushort> data)
	{
		return @this.Append((byte)(data.Count - 1 >> 8)).Append((byte)(data.Count - 1)).Add(data, (IEnumerable<byte> lb, ushort page) => lb.Add(page));
	}

	internal static IEnumerable<byte> Add(this IEnumerable<byte> @this, List<byte> data)
	{
		return @this.Append((byte)(data.Count - 1)).Add(data, (IEnumerable<byte> lb, byte page) => lb.Add(page));
	}

	internal static IEnumerable<byte> Add(this IEnumerable<byte> @this, IEnumerable<byte> data)
	{
		return @this.Concat(data);
	}

	internal static List<byte> Append(this List<byte> @this, byte data)
	{
		@this.Add(data);
		return @this;
	}

	internal static List<byte> Append(this List<byte> @this, List<byte> data)
	{
		data.Aggregate(@this, (List<byte> lb, byte b) => lb.Append(b));
		return @this;
	}
}
