using System.Collections.Generic;

namespace HedgeTech.Common.Classes
{
	public class LimitedList<T>
	{
		private readonly int _maxSize;
		private List<T> _items;

		public LimitedList(int maxSize)
		{
			_maxSize = maxSize;
			_items = new();
		}

		public void Add(T item)
		{
			_items.Add(item);

			if (_items.Count >= _maxSize)
			{
				_items.RemoveAt(0);
			}
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _items.Count) return;

			_items.RemoveAt(index);
		}

		public void Remove(T item)
		{
			if (_items.Contains(item))
			{
				_items.Remove(item);
			}
		}

		public IEnumerable<T> GetEnumerator()
		{
			for (int i = 9; i >= 0; i--)
			{
				if (i > _items.Count) continue;

				yield return _items[i];
			}

			yield break;
		}
	}
}
