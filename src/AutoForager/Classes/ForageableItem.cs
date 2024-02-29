using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.WildTrees;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;
using AutoForager.Helpers;

namespace AutoForager.Classes
{
    public class ForageableItem : IComparable
    {
        private readonly string _itemId;
        public string ItemId => _itemId;

        private readonly string _qualifiedItemId;
        public string QualifiedItemId => _qualifiedItemId;

        private readonly string _internalName;
        public string InternalName => _internalName;

        private readonly string _displayName;
        public string DisplayName => TokenParser.ParseText(_displayName);

        private readonly Dictionary<string, string> _customFields;
        public Dictionary<string, string> CustomFields => _customFields;

        private readonly bool _defaultIsEnabled;
        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public void ResetToDefaultEnabled()
        {
            _isEnabled = _defaultIsEnabled;
        }

        public ForageableItem(string itemId, string qualifiedItemId, string internalName, string displayName, Dictionary<string, string> customFields, bool enabled = false)
        {
            _itemId = itemId;
            _qualifiedItemId = qualifiedItemId;
            _internalName = internalName;
            _displayName = displayName;
            _customFields = customFields;
            _isEnabled = enabled;
            _defaultIsEnabled = enabled;
        }

        public ForageableItem(ParsedItemData data, Dictionary<string, string> customFields, bool enabled = false)
            : this(data.ItemId, data.QualifiedItemId, data.InternalName, data.DisplayName, customFields, enabled)
        { }

        public static IEnumerable<ForageableItem> Parse(IDictionary<string, FruitTreeData> data)
        {
            var forageItems = new List<ForageableItem>();

            foreach (var kvp in data)
            {
                foreach (var fruit in kvp.Value.Fruit)
                {
                    var customFields = kvp.Value.CustomFields;
                    if (customFields == null || !customFields.ContainsKey(Constants.CustomFieldForageableKey)) continue;

                    forageItems.AddDistinct(new ForageableItem(ItemRegistry.GetData(fruit.ItemId), customFields, true));
                }
            }

            return forageItems;
        }

        public static IEnumerable<ForageableItem> Parse(IDictionary<string, WildTreeData> data)
        {
            var forageItems = new List<ForageableItem>();

            foreach (var kvp in data)
            {
                var customFields = kvp.Value.CustomFields;
                if (customFields == null || !customFields.ContainsKey(Constants.CustomFieldForageableKey)) continue;

                var seedAndSeedItemIds = new List<string> { kvp.Value.SeedItemId };

                if (kvp.Value.SeedDropItems != null)
                {
                    seedAndSeedItemIds.AddRange(kvp.Value.SeedDropItems.Select(i => i.ItemId));
                }

                foreach (var seedItem in seedAndSeedItemIds)
                {
                    forageItems.AddDistinct(new ForageableItem(ItemRegistry.GetData(seedItem), customFields, true));
                }
            }

            return forageItems;
        }

        public static IEnumerable<ForageableItem> Parse(IDictionary<string, ObjectData> data)
        {
            var forageItems = new List<ForageableItem>();

            foreach (var kvp in data)
            {
                var customFields = kvp.Value.CustomFields;
                if (customFields == null || !customFields.ContainsKey(Constants.CustomFieldForageableKey)) continue;

                var qualifiedItemId = "(O)" + kvp.Key;
                forageItems.AddDistinct(new ForageableItem(ItemRegistry.GetData(qualifiedItemId), customFields, false));
            }

            return forageItems;
        }

        public static IEnumerable<ForageableItem> Parse(IDictionary<string, ObjectData> oData, IDictionary<string, LocationData> lData)
        {
            var forageItems = new List<ForageableItem>();

            foreach (var kvp in lData)
            {
                var artifactSpots = kvp.Value.ArtifactSpots;
                if (artifactSpots == null || artifactSpots.Count <= 0) continue;

                foreach (var artifact in artifactSpots)
                {
                    List<string> itemIds;

                    if (artifact.RandomItemId != null)
                    {
                        itemIds = artifact.RandomItemId;
                    }
                    else if (artifact.ItemId != null)
                    {
                        itemIds = new() { artifact.ItemId };
                    }
                    else
                    {
                        continue;
                    }

                    foreach (var itemId in itemIds)
                    {
                        var artifactId = itemId.Substring(itemId.IndexOf(')') + 1);
                        if (!oData.ContainsKey(artifactId)) continue;

                        var objData = oData[artifactId];
                        if (objData == null || objData.CustomFields == null || !objData.CustomFields.ContainsKey(Constants.CustomFieldForageableKey)) continue;

                        forageItems.AddDistinct(new ForageableItem(ItemRegistry.GetData(itemId), objData.CustomFields, false));
                    }
                }
            }

            return forageItems;
        }

        public int CompareTo(object? obj)
        {
            if (obj == null) return 1;

            if (obj is ForageableItem otherForageable)
            {
                return QualifiedItemId.CompareTo(otherForageable.QualifiedItemId);
            }
            else
            {
                throw new ArgumentException($"Object is not a ${nameof(ForageableItem)}");
            }
        }
    }
}
