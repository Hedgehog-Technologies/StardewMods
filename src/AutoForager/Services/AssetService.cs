using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.WildTrees;
using StardewValley.ItemTypeDefinitions;
using AutoForager.Classes;
using AutoForager.Extensions;
using HedgeTech.Common.Extensions;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Services
{
	/// <summary>
	/// Manages asset loading, caching, and parsing for forageable items.
	/// </summary>
	internal class AssetService(IMonitor monitor, IModHelper helper, ModConfig config, ForageableItemTracker forageableTracker)
	{

		// Asset caches
		private Dictionary<string, FruitTreeData> _fruitTreeCache = [];
		private Dictionary<string, LocationData> _locationCache = [];
		private Dictionary<string, ObjectData> _objectCache = [];
		private Dictionary<string, WildTreeData> _wildTreeCache = [];

		/// <summary>
		/// Loads all initial asset data from the game content.
		/// </summary>
		public void LoadInitialAssets()
		{
			monitor.Log("Loading initial asset data", config.DebugLogLevel());

			UpdateFruitTreeCache(DataLoader.FruitTrees(Game1.content));
			UpdateWildTreeCache(DataLoader.WildTrees(Game1.content));
			UpdateObjectCache(DataLoader.Objects(Game1.content));
			UpdateLocationCache(DataLoader.Locations(Game1.content));
		}

		/// <summary>
		/// Reloads all assets (typically after locale change).
		/// </summary>
		public void ReloadAllAssets()
		{
			monitor.Log("Reloading all assets", config.DebugLogLevel());

			ItemRegistry.ResetCache();

			UpdateFruitTreeCache(DataLoader.FruitTrees(Game1.content));
			UpdateWildTreeCache(DataLoader.WildTrees(Game1.content));
			UpdateObjectCache(DataLoader.Objects(Game1.content));
			UpdateLocationCache(DataLoader.Locations(Game1.content));
		}

		/// <summary>
		/// Load the Flower data from Wild Flowers Reimagined
		/// </summary>
		/// <param name="flowerData">List if ItemMetadata flowers</param>
		public void LoadFlowerData(List<ItemMetadata> flowerData)
		{
			// This should be a no op, but kept for consistency
			forageableTracker.FlowerForageables.Clear();
			forageableTracker.FlowerForageables.AddRange(ForageableItem.ParseFlowerData(flowerData,
				config?.ForageToggles[Constants.FlowerGrassToggleKey],
				monitor
			));
			forageableTracker.FlowerForageables.SortByDisplayName();
		}

		/// <summary>
		/// Handles asset ready events for dynamic reloading.
		/// </summary>
		public void HandleAssetReady(string assetName)
		{
			if (assetName.IEquals(Constants.FruitTreesAssetName))
			{
				UpdateFruitTreeCache(DataLoader.FruitTrees(Game1.content));
			}
			else if (assetName.IEquals(Constants.WildTreesAssetName))
			{
				UpdateWildTreeCache(DataLoader.WildTrees(Game1.content));
			}
			else if (assetName.IEquals(Constants.ObjectsAssetName))
			{
				UpdateObjectCache(DataLoader.Objects(Game1.content));
			}
			else if (assetName.IEquals(Constants.LocationsAssetName))
			{
				UpdateLocationCache(DataLoader.Locations(Game1.content));
			}
			else if (assetName.IEquals(Constants.SpaceCoreSpawnableAssetName))
			{
				HandleSpaceCoreSpawnables();
			}
		}

		/// <summary>
		/// Updates the fruit tree cache and parses the data.
		/// </summary>
		private void UpdateFruitTreeCache(Dictionary<string, FruitTreeData> data)
		{
			_fruitTreeCache = data;
			ParseFruitTreeData(data);
		}

		/// <summary>
		/// Updates the wild tree cache and parses the data.
		/// </summary>
		private void UpdateWildTreeCache(Dictionary<string, WildTreeData> data)
		{
			_wildTreeCache = data;
			ParseWildTreeData(data);
		}

		/// <summary>
		/// Updates the object cache and parses the data.
		/// </summary>
		private void UpdateObjectCache(Dictionary<string, ObjectData> data)
		{
			_objectCache = data;
			ParseObjectData(data);
		}

		/// <summary>
		/// Updates the location cache and parses the data.
		/// </summary>
		private void UpdateLocationCache(Dictionary<string, LocationData> data)
		{
			_locationCache = data;
			ParseLocationData(data);
		}

		/// <summary>
		/// Parses Fruit Tree data into forageable items.
		/// </summary>
		/// <param name="data"></param>
		private void ParseFruitTreeData(Dictionary<string, FruitTreeData> data)
		{
			monitor.Log("Parsing Fruit Tree data", config.DebugLogLevel());

			forageableTracker.FruitTreeForageables.Clear();
			forageableTracker.FruitTreeForageables.AddRange(
				ForageableItem.ParseFruitTreeData(
					data,
					config?.ForageToggles[Constants.FruitTreeToggleKey],
					monitor));
			forageableTracker.FruitTreeForageables.SortByDisplayName();
		}

		/// <summary>
		/// Parses Wild Tree data into forageable items.
		/// </summary>
		private void ParseWildTreeData(Dictionary<string, WildTreeData> data)
		{
			monitor.Log("Parsing Wild Tree data", config.DebugLogLevel());

			forageableTracker.WildTreeForageables.Clear();
			forageableTracker.WildTreeForageables.AddRange(
				ForageableItem.ParseWildTreeData(
				data,
				config?.ForageToggles[Constants.WildTreeToggleKey],
				monitor));
			forageableTracker.WildTreeForageables.SortByDisplayName();
		}

		/// <summary>
		/// Parses Object data into forageable and bush items.
		/// </summary>
		private void ParseObjectData(Dictionary<string, ObjectData> data)
		{
			var parsedObjectForageableItems = ForageableItem.ParseObjectData(data, config, monitor);

			monitor.Log("Parsing Object data", config.DebugLogLevel());

			forageableTracker.ObjectForageables.Clear();
			forageableTracker.ObjectForageables.AddRange(parsedObjectForageableItems.Item1);
			forageableTracker.ObjectForageables.SortByDisplayName();

			forageableTracker.BushForageables.Clear();
			forageableTracker.BushForageables.AddRange(parsedObjectForageableItems.Item2);
			forageableTracker.BushForageables.SortByDisplayName();

			if (_locationCache is not null && _locationCache.Count > 0)
			{
				monitor.Log("Sub-Object: Parsing Location data", config.DebugLogLevel());

				forageableTracker.ObjectForageables.AddOrMergeCustomFieldsRange(
					ForageableItem.ParseLocationData(
						_locationCache,
						config?.ForageToggles[Constants.ForagingToggleKey],
						monitor));
				forageableTracker.ObjectForageables.SortByDisplayName();
			}
		}

		/// <summary>
		/// Parses Location data into forageable items.
		/// </summary>
		private void ParseLocationData(Dictionary<string, LocationData> data)
		{
			if (_objectCache is null || _objectCache.Count == 0)
			{
				monitor.Log("Sub-Location: Grabbing Object data", config.DebugLogLevel());
				_objectCache = DataLoader.Objects(Game1.content);
			}

			monitor.Log("Parsing Location data", config.DebugLogLevel());

			forageableTracker.ObjectForageables.AddOrMergeCustomFieldsRange(
				ForageableItem.ParseLocationData(
					data,
					config?.ForageToggles[Constants.ForagingToggleKey],
					monitor));
			forageableTracker.ObjectForageables.SortByDisplayName();
		}

		private void HandleSpaceCoreSpawnables()
		{
			try
			{
				var rawDict = helper.GameContent.Load<IDictionary>(Constants.SpaceCoreSpawnableAssetName);
				var forageables = new Dictionary<string, SpaceCoreSpawnableDefinition>();

				monitor.Log($"{rawDict.Count} spacecore forageables", LogLevel.Info);

				foreach (DictionaryEntry entry in rawDict)
				{
					if (entry.Value == null || entry.Key == null) continue;

					var model = JObject.FromObject(entry.Value).ToObject<SpaceCoreSpawnableDefinition>();
					if (model != null && model.Type == 1)
					{
						forageables[entry.Key.ToString()!] = model;
					}
				}

				monitor.Log($"Loaded {forageables.Count} forageable definitions.", config.DebugLogLevel());

				foreach (var (key, def) in forageables)
				{
					foreach (var weightedData in def.ForageableItemData)
					{
						var forageableData = weightedData.Value;
						if (forageableData is null) continue;

						var itemData = ItemRegistry.GetData(forageableData.ItemId)
							?? ItemRegistry.GetData("(O)" + forageableData.ItemId)
							?? null;
						if (itemData is null) continue;

						var enabled = true;
						if (config.ForageToggles[Constants.ForagingToggleKey]?.TryGetValue(itemData.InternalName, out var configEnabled) ?? false)
						{
							enabled = configEnabled;
						}

						var forageableItem = new ForageableItem(itemData, [], enabled);
						forageableTracker.ObjectForageables.AddDistinct(forageableItem);
					}

					monitor.Log($"[{key}] Item: {def.ForageableItemData} | Chance: {def.Chance * 100}% | Seasons: {string.Join(", ", def.Seasons)}", config.DebugLogLevel());

					if (def.AdditionalData.Count > 0)
					{
						monitor.Log($"\t-> Contains extra properties: {string.Join(", ", def.AdditionalData.Keys)}", config.DebugLogLevel());
					}
				}
			}
			catch (System.Exception ex)
			{
				monitor.Log($"Failed to load SpaceCore spawnables: {ex.Message}", LogLevel.Error);
			}
		}
	}
}
