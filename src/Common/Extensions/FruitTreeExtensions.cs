using System.Collections.Generic;
using StardewValley.TerrainFeatures;

namespace HedgeTech.Common.Extensions
{
	public static class FruitTreeExtensions
	{
		public static List<string> GetFruitItemIds(this FruitTree fruitTree)
		{
			var itemIds = new List<string>();

			foreach (var fruit in fruitTree.fruit)
			{
				itemIds.Add(fruit.QualifiedItemId);
			}

			return itemIds;
		}
	}
}
