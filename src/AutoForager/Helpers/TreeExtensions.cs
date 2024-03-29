﻿using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace AutoForager.Helpers
{
    public static class TreeExtensions
    {
        public static List<string> GetSeedAndSeedItemIds(this Tree tree)
        {
            var itemIds = new List<string>();
            var treeData = tree.GetData();

            if (treeData.SeedItemId is not null)
            {
                itemIds.Add(treeData.SeedItemId);
            }

            if (treeData.SeedDropItems is not null)
            {
                foreach (var item in treeData.SeedDropItems)
                {
                    if (item?.ItemId is not null)
                    {
                        itemIds.Add(item.ItemId);
                    }
                }
            }

            return itemIds;
        }
    }
}
