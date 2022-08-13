using System.Collections.Generic;
using System.Linq;

namespace Racelogic.Maths;

public static class Combinatorics
{
	public static IEnumerable<T[][]> GetAllPartitions<T>(T[] elements, int maxNumGroups)
	{
		return from p in GetAllPartitionsRecurse(elements, maxNumGroups, elements.Length)
			select p.Select((List<T> g) => g.ToArray()).ToArray();
	}

	public static IEnumerable<List<List<T>>> GetAllPartitionsVolatile<T>(T[] elements, int maxNumGroups)
	{
		return GetAllPartitionsRecurse(elements, maxNumGroups, elements.Length);
	}

	private static IEnumerable<List<List<T>>> GetAllPartitionsRecurse<T>(T[] elements, int maxNumGroups, int elementCount)
	{
		if (elementCount <= 0)
		{
			yield return new List<List<T>>(0);
			yield break;
		}
		elementCount--;
		T element = elements[elementCount];
		foreach (List<List<T>> group in GetAllPartitionsRecurse(elements, maxNumGroups, elementCount))
		{
			int groupSize = group.Count;
			if (groupSize <= maxNumGroups)
			{
				List<T>[] array = group.ToArray();
				foreach (List<T> list in array)
				{
					list.Add(element);
					yield return group;
					list.RemoveAt(list.Count - 1);
				}
				if (groupSize < maxNumGroups)
				{
					List<T> item = new List<T>(1) { element };
					group.Add(item);
					yield return group;
					group.RemoveAt(groupSize);
				}
			}
		}
	}
}
