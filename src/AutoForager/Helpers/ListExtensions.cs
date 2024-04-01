﻿using System.Collections.Generic;
using System.Linq;
using AutoForager.Classes;

namespace AutoForager.Helpers
{
    public static class ListExtensions
    {
        public static void AddDistinct(this List<ForageableItem> items, ForageableItem newItem)
        {
            if (items.Any(i => i.QualifiedItemId.Equals(newItem.QualifiedItemId))) return;

            items.Add(newItem);
        }

        public static IOrderedEnumerable<IGrouping<string, ForageableItem>> GroupByCategory(this List<ForageableItem> list, IComparer<string>? comparer = null)
        {
            return list.GroupBy(f =>
            {
                var category = "Other";

                if (f.CustomFields?.TryGetValue(Constants.CustomFieldCategoryKey, out var customCategory) ?? false)
                {
                    category = customCategory;
                }

                return category;
            })
            .OrderBy(g => g.Key, comparer ?? new CategoryComparer());
        }

        public static void SortByDisplayName(this List<ForageableItem> items)
        {
            items.Sort((x, y) => string.CompareOrdinal(x.DisplayName, y.DisplayName));
        }

        public static bool TryGetItem(this List<ForageableItem> items, string qualifiedItemId, out ForageableItem? item)
        {
            item = null;

            if (items is null || qualifiedItemId is null) return false;

            foreach (var fItem in items)
            {
                if (fItem.QualifiedItemId.IEquals(qualifiedItemId))
                {
                    item = fItem;
                    return true;
                }
            }

            return false;
        }

        public static bool IsNullOrEmpty<T>(this List<T>? list)
        {
            if (list is null) return true;

            return !list.Any();
        }
    }
}
