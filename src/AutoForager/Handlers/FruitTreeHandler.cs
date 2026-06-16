using System.Linq;
using StardewValley.TerrainFeatures;
using AutoForager.Extensions;
using HedgeTech.Common.Extensions;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles foraging from Fruit Trees.
	/// </summary>
	internal class FruitTreeHandler : BaseForagingHandler
	{
		/// <summary>
		/// Determines if this handler can process the given Fruit Tree.
		/// </summary>
		public bool CanHandle(FruitTree fruitTree)
		{
			if (fruitTree == null) return false;
			if (fruitTree.stump.Value) return false;
			if (fruitTree.growthStage.Value < Constants.FruitTreeMatureStage) return false;

			var fruitCount = fruitTree.fruit.Count;
			if (fruitCount <= 0 || fruitCount < Config.FruitsReadyToShake) return false;

			// Check if any fruit is enabled
			var fruitItemIds = fruitTree.GetFruitItemIds();
			return Context.ForageableTracker.FruitTreeForageables.Any(i =>
				fruitItemIds.Contains(i.QualifiedItemId) && i.IsEnabled);
		}

		/// <summary>
		/// Handles foraging from the given Fruit Tree.
		/// </summary>
		public void Handle(FruitTree fruitTree)
		{
			var fruitItemIds = fruitTree.GetFruitItemIds();

			fruitTree.performUseAction(fruitTree.Tile);
			LogDebug($"Fruit Tree shaken: {string.Join(", ", fruitItemIds)}");

			// Track all fruits
			foreach (var id in fruitItemIds)
			{
				var name = id;

				if (Context.ForageableTracker.FruitTreeForageables.TryGetItem(id, out var ftItem))
				{
					name = ftItem?.DisplayName ?? id;
				}

				TrackItem(Constants.FruitTreeKey, name);
			}
		}
	}
}
