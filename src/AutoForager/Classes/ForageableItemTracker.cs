using System;
using System.Collections.Generic;

namespace AutoForager.Classes
{
	internal sealed class ForageableItemTracker
	{
		// ---------- Instance ---------- //

		private static readonly Lazy<ForageableItemTracker> _lazyInstance = new(() => new());
		public static ForageableItemTracker Instance => _lazyInstance.Value;

		// ---------- Trackers ---------- //

		private readonly List<ForageableItem> _bushForageables;
		public List<ForageableItem> BushForageables => _bushForageables;

		private readonly List<ForageableItem> _caveForageables;
		public List<ForageableItem> CaveForageables => _caveForageables;

		private readonly List<ForageableItem> _fruitTreeForageables;
		public List<ForageableItem> FruitTreeForageables => _fruitTreeForageables;

		private readonly List<ForageableItem> _objectForageables;
		public List<ForageableItem> ObjectForageables => _objectForageables;

		private readonly List<ForageableItem> _wildTreeForageables;
		public List<ForageableItem> WildTreeForageables => _wildTreeForageables;

		// ---------- Constructor ---------- //

		private ForageableItemTracker()
		{
			_bushForageables = new();
			_caveForageables = new();
			_fruitTreeForageables = new();
			_objectForageables = new();
			_wildTreeForageables = new();
		}
	}
}
