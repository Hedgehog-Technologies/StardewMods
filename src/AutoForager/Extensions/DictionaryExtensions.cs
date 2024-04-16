using System.Collections.Generic;

namespace AutoForager.Extensions
{
	public static class DictionaryExtensions
	{
		public static int SumAll(this Dictionary<string, int> dict)
		{
			var sum = 0;

			foreach (var kvp in dict)
			{
				sum += kvp.Value;
			}

			return sum;
		}

		public static int SumAll(this Dictionary<string, Dictionary<string, int>> dict)
		{
			var sum = 0;

			foreach (var kvp in dict)
			{
				if (kvp.Value is null) continue;

				sum += kvp.Value.SumAll();
			}

			return sum;
		}

		public static void AddOrIncrement<TKey>(this Dictionary<TKey, int> dict, TKey key) where TKey : notnull
		{
			if (dict.ContainsKey(key))
			{
				dict[key] += 1;
			}
			else
			{
				dict.Add(key, 1);
			}
		}

		public static void AddOrIncrement<TKey>(this Dictionary<TKey, int> dict, IEnumerable<TKey> keys) where TKey : notnull
		{
			foreach (var key in keys)
			{
				dict.AddOrIncrement(key);
			}
		}

		public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
			where TKey : notnull
			where TValue : notnull
		{
			if (dict.ContainsKey(key) && !dict[key].Equals(value))
			{
				dict[key] = value;
			}
			else if (!dict.ContainsKey(key))
			{
				dict.Add(key, value);
			}
		}
	}
}
