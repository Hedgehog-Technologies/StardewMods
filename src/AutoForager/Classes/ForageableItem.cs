using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TokenizableStrings;
using AutoForager.Extensions;
using HedgeTech.Common.Extensions;

using SObject = StardewValley.Object;
using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Classes
{
	public class ForageableItem(string itemId, string qualifiedItemId, string internalName, string displayName, Dictionary<string, string> customFields, bool enabled = false) : IComparable
	{
		private static readonly ItemQueryContext queryContext = new();

		private readonly string _itemId = itemId;
		public string ItemId => _itemId;

		private readonly string _qualifiedItemId = qualifiedItemId;
		public string QualifiedItemId => _qualifiedItemId;

		private readonly string _internalName = internalName;
		public string InternalName => _internalName;

		private readonly string _displayName = displayName;
		public string DisplayName => TokenParser.ParseText(_displayName);

		private readonly Dictionary<string, string> _customFields = customFields;
		public Dictionary<string, string> CustomFields => _customFields;

		private readonly bool _defaultIsEnabled = enabled;
		private bool _isEnabled = enabled;
		public bool IsEnabled
		{
			get => _isEnabled;
			set => _isEnabled = value;
		}

		public void ResetToDefaultEnabled()
		{
			_isEnabled = _defaultIsEnabled;
		}

		public ForageableItem(ParsedItemData data, Dictionary<string, string> customFields, bool enabled = false)
			: this(data.ItemId, data.QualifiedItemId, data.InternalName, data.DisplayName, customFields, enabled)
		{ }

		public ForageableItem(SObject item, Dictionary<string, string> customFields, bool enabled = false)
			: this(item.QualifiedItemId, item.QualifiedItemId, item.Name, item.DisplayName, customFields, enabled)
		{ }

		public static IEnumerable<ForageableItem> ParseFruitTreeData(IDictionary<string, FruitTreeData> data, IDictionary<string, bool>? configValues = null, IMonitor? monitor = null)
		{
			var forageItems = new List<ForageableItem>();

			foreach (var kvp in data)
			{
				foreach (var fruit in kvp.Value.Fruit)
				{
					try
					{
						var customFields = kvp.Value.CustomFields;
						if (customFields is null || !customFields.ContainsKey(Constants.CustomFieldForageableKey)) continue;

						var enabled = true;
						var fruitQueryData = ItemQueryResolver.TryResolve(fruit, queryContext, logError: (query, msg) => monitor?.Log($"Failed to parse Fruit Tree item query '{query}': {msg}", LogLevel.Warn));

						if (fruitQueryData.Count > 1)
						{
							monitor?.Log($"Found multiple items for fruit tree fruit entry [{fruit.ItemId ?? fruit.Id}] in tree: {kvp.Key}", LogLevel.Info);
						}
						else if (fruitQueryData.Count == 0)
						{
							monitor?.Log($"Failed to retrieve data for fruit tree fruit entry [{fruit.ItemId ?? fruit.Id}] in tree: {kvp.Key}.{Environment.NewLine}" +
								$"\tThis is likely due to a misconfiguration from the mod that tree is added by.{Environment.NewLine}" +
								$"\tPlease reach out to that tree mod author with this information to get it fixed.{Environment.NewLine}" +
								$"\tParsing will continue, this should not impact the rest of your gameplay experience.", LogLevel.Warn);
							continue;
						}

						foreach (var result in fruitQueryData)
						{
							if (result == null || result.Item is not SObject sObj)
							{
								monitor?.Log($"Failed to retrieve data for {fruit.ItemId ?? fruit.Id} while parsing fruit tree with key: {kvp.Key}.{Environment.NewLine}" +
									$"\tParsing will continue, this should not impact the rest of your gameplay experience.", LogLevel.Warn);
								continue;
							}

							if (configValues is not null && configValues.TryGetValue(sObj.name, out var configEnabled))
							{
								enabled = configEnabled;
							}

							forageItems.AddDistinct(new ForageableItem(sObj, customFields, enabled));
						}
					}
					catch (Exception ex)
					{
						monitor?.Log($"{kvp.Key} - {fruit?.ItemId} - {fruit?.ObjectInternalName}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}", LogLevel.Error);
					}

				}
			}

			return forageItems;
		}

		public static IEnumerable<ForageableItem> ParseWildTreeData(IDictionary<string, WildTreeData> data, IDictionary<string, bool>? configValues = null, IMonitor? monitor = null)
		{
			var forageItems = new List<ForageableItem>();

			foreach (var kvp in data)
			{
				var customFields = kvp.Value.CustomFields;
				if (customFields is null || !customFields.ContainsKey(Constants.CustomFieldForageableKey)) continue;

				var seedItemId = kvp.Value.SeedItemId;
				var seedItemData = ItemRegistry.GetData(seedItemId);

				if (seedItemData is not null)
				{
					var enabled = true;

					if (configValues is not null && configValues.TryGetValue(seedItemData.InternalName, out var configEnabled))
					{
						enabled = configEnabled;
					}

					forageItems.AddDistinct(new ForageableItem(seedItemData, customFields, enabled));
				}

				var seedAndSeedItemIds = new List<WildTreeItemData>();

				if (kvp.Value.SeedDropItems is not null)
				{
					seedAndSeedItemIds.AddRange(kvp.Value.SeedDropItems);
				}

				if (kvp.Value.ShakeItems is not null)
				{
					seedAndSeedItemIds.AddRange(kvp.Value.ShakeItems);
				}

				foreach (var seedItem in seedAndSeedItemIds)
				{
					try
					{
						if (seedItem is null) continue;

						var enabled = true;
						var seedQueryResult = ItemQueryResolver.TryResolve(seedItem, queryContext, logError: (query, msg) => monitor?.Log($"Failed to parse Wild Tree item query '{query}': {msg}", LogLevel.Warn));

						if (seedQueryResult.Count > 1)
						{
							monitor?.Log($"Found multiple items for wild tree seed/shake entry [{seedItem.ItemId ?? seedItem.Id}] in tree: {kvp.Key}", LogLevel.Info);
						}
						else if (seedQueryResult.Count == 0)
						{
							monitor?.Log($"Failed to retrieve data for wild tree seed/shake entry [{seedItem.ItemId ?? seedItem.Id}] in tree: {kvp.Key}.{Environment.NewLine}" +
								$"\tThis is likely due to a misconfiguration from the mod that tree is added by.{Environment.NewLine}" +
								$"\tPlease reach out to that tree mod author with this information to get it fixed.{Environment.NewLine}" +
								$"\tParsing will continue, this should not impact the rest of your gameplay experience.", LogLevel.Warn);
							continue;
						}

						foreach (var result in seedQueryResult)
						{
							if (result == null || result.Item is not SObject sObj)
							{
								monitor?.Log($"Failed to retrieve data for {seedItem.ItemId ?? seedItem.Id} while parsing wild tree with key: {kvp.Key}.{Environment.NewLine}" +
									$"\tParsing will continue, this should not impact the rest of your gameplay experience.", LogLevel.Warn);
								continue;
							}

							if (configValues is not null && configValues.TryGetValue(sObj.name, out var configEnabled))
							{
								enabled = configEnabled;
							}

							forageItems.AddDistinct(new ForageableItem(sObj, customFields, enabled));
						}
					}
					catch (Exception ex)
					{
						monitor?.Log($"{kvp.Key} - {seedItem}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}", LogLevel.Error);
					}
				}
			}

			return forageItems;
		}

		public static (IEnumerable<ForageableItem>, IEnumerable<ForageableItem>) ParseObjectData(IDictionary<string, ObjectData> data, ModConfig? config = null, IMonitor? monitor = null)
		{
			var forageItems = new List<ForageableItem>();
			var bushItems = new List<ForageableItem>();
			var forageableConfig = config?.ForageToggles[Constants.ForagingToggleKey];
			var bushConfig = config?.ForageToggles[Constants.BushToggleKey];

			foreach (var kvp in data)
			{
				try
				{
					var customFields = kvp.Value.CustomFields;

					if (customFields is not null)
					{
						var qualifiedItemId = "(O)" + kvp.Key;
						var itemData = ItemRegistry.GetData(qualifiedItemId) ?? ItemRegistry.GetData(kvp.Key);
						var internalName = itemData?.InternalName ?? kvp.Value.Name;

						if (customFields.TryGetValue(Constants.CustomFieldForageableKey, out var isForageable) && isForageable.IEquals("true"))
						{
							var enabled = true;

							if (forageableConfig is not null && forageableConfig.TryGetValue(internalName, out var configEnabled))
							{
								enabled = configEnabled;
							}

							if (itemData is not null)
							{
								forageItems.AddDistinct(new ForageableItem(itemData, customFields, enabled));
							}
							else
							{
								var kvpValue = kvp.Value;
								forageItems.AddDistinct(new ForageableItem(kvp.Key, qualifiedItemId, kvpValue.Name, kvpValue.DisplayName, customFields, enabled));
							}
						}

						if (customFields.TryGetValue(Constants.CustomFieldBushKey, out var isBush) && isBush.IEquals("true"))
						{
							var enabled = true;

							if (bushConfig is not null && bushConfig.TryGetValue(internalName, out var configEnabled))
							{
								enabled = configEnabled;
							}

							if (itemData is not null)
							{
								bushItems.AddDistinct(new ForageableItem(itemData, customFields, enabled));
							}
							else
							{
								var kvpValue = kvp.Value;
								bushItems.AddDistinct(new ForageableItem(kvp.Key, qualifiedItemId, kvpValue.Name, kvpValue.DisplayName, customFields, enabled));
							}
						}
					}
				}
				catch (Exception ex)
				{
					monitor?.Log($"{kvp.Key} - {kvp.Value.Name}{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}", LogLevel.Error);
				}

			}

			return (forageItems, bushItems);
		}

		public static IEnumerable<ForageableItem> ParseLocationData(IDictionary<string, LocationData> data, IDictionary<string, bool>? configValues = null, IMonitor? monitor = null)
		{
			var forageItems = new List<ForageableItem>();

			foreach (var location in data.Values)
			{
				var forage = location.Forage;

				if (forage.IsNullOrEmpty()) continue;

				foreach (var forageObj in forage)
				{
					var itemQueryResult = ItemQueryResolver.TryResolve(forageObj, queryContext, logError: (query, msg) => monitor?.Log($"Failed to parse Location item query '{query}': {msg}", LogLevel.Warn));

					if (itemQueryResult.Count > 1)
					{
						monitor?.Log($"Found multiple items for forage entry [{forageObj.ItemId ?? forageObj.Id}] in location: {TokenParser.ParseText(location.DisplayName)}", LogLevel.Info);
					}
					else if (itemQueryResult.Count == 0)
					{
						monitor?.Log($"Failed to retrieve data for forage entry [{forageObj.ItemId ?? forageObj.Id}] in location: {TokenParser.ParseText(location.DisplayName)}.{Environment.NewLine}" +
							$"\tThis is likely due to a misconfiguration from the mod that location is added by.{Environment.NewLine}" +
							$"\tPlease reach out to that location mod author with this information to get it fixed.{Environment.NewLine}" +
							$"\tParsing will continue, this should not impact the rest of your gameplay experience.", LogLevel.Warn);
						continue;
					}

					foreach (var result in itemQueryResult)
					{
						if (result == null || result.Item is not SObject sObj)
						{
							monitor?.Log($"Failed to retrieve data for {forageObj} while parsing location: {TokenParser.ParseText(location.DisplayName)}.{Environment.NewLine}" +
								$"\tParsing will continue, this should not impact the rest of your gameplay experience.", LogLevel.Warn);
							continue;
						}

						var enabled = true;
						if (configValues is not null && configValues.TryGetValue(sObj.name, out var configEnabled))
						{
							enabled = configEnabled;
						}

						forageItems.AddDistinct(new ForageableItem(sObj, new() { { Constants.CustomFieldCategoryKey, "Locations" } }, enabled));
					}
				}
			}

			return forageItems;
		}

		public int CompareTo(object? obj)
		{
			if (obj is null) return 1;

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
