using System.Collections.Generic;
using System.Linq;

namespace HedgeTech.Common.Extensions
{
	public static class ListExtensions
	{
		public static void AddDistinct<T>(this List<T> items, T newItem)
			where T : notnull
		{
			if (items.Any(i => i.Equals(newItem))) return;

			items.Add(newItem);
		}

		public static bool IsNullOrEmpty<T>(this List<T>? list)
		{
			if (list is null) return true;

			return !list.Any();
		}
	}
}
