using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using AutoForager.Classes;

namespace AutoForager.Helpers
{
    internal class CategoryComparer : IComparer<string>
    {
        private readonly List<string> _packCategories = new();

        int IComparer<string>.Compare(string? x, string? y)
        {
            if (x is null || y is null)
            {
                throw new NullReferenceException();
            }

            if (x.Equals(y)) return 0;

            // Push to bottom of grouping
            if (x.Equals("Other")) return 1;
            if (y.Equals("Other")) return -1;

            var xIsPack = _packCategories.Contains(x);
            var yIsPack = _packCategories.Contains(y);
            if (xIsPack && yIsPack)
            {
                return string.Compare(x, y);
            }
            else if (xIsPack)
            {
                return 1;
            }
            else if (yIsPack)
            {
                return -1;
            }

            if (x.Equals("Special")) return 1;
            if (y.Equals("Special")) return -1;

            // Push to top of grouping
            if (x.Equals("Vanilla")) return -1;
            if (y.Equals("Vanilla")) return 1;

            return string.Compare(x, y);
        }

        public CategoryComparer(IEnumerable<IContentPack> packs)
        {
            _packCategories = packs.Select(p => p?.ReadJsonFile<ContentEntry>("content.json"))
                .Select(e => e?.Category ?? "Unknown")
                .ToList();

            _packCategories.Sort();
        }

        public CategoryComparer() { }
    }
}
