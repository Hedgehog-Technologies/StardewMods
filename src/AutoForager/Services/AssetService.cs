using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.WildTrees;
using AutoForager.Classes;
using AutoForager.Extensions;
using HedgeTech.Common.Extensions;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Services
{
	/// <summary>
	/// Manages asset loading, caching, and parsing for forageable items.
	/// </summary>
	internal class AssetService
	{
		private readonly IMonitor _monitor;
		private readonly ModConfig _config;
		private readonly ForageableItemTracker _forageableTracker;

		// Asset caches
		private Dictionary<string, FruitTreeData> _fruitTreeCache = [];
		private Dictionary<string, LocationData> _locationCache = [];
		private Dictionary<string, ObjectData> _objectCache = [];
		private Dictionary<string, WildTreeData> _wildTreeCache = [];

		public AssetService(IMonitor monitor, ModConfig config, ForageableItemTracker forageableTracker)
		{
			_monitor = monitor;
			_config = config;
			_forageableTracker = forageableTracker;
		}

		/// <summary>
		/// Loads all initial asset data from the game content.
		/// </summary>
		public void LoadInitialAssets()
		{
			_monitor.Log("Loading initial asset data", _config.DebugLogLevel());

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
			_monitor.Log("Reloading all assets", _config.DebugLogLevel());

			ItemRegistry.ResetCache();

			UpdateFruitTreeCache(DataLoader.FruitTrees(Game1.content));
			UpdateWildTreeCache(DataLoader.WildTrees(Game1.content));
			UpdateObjectCache(DataLoader.Objects(Game1.content));
			UpdateLocationCache(DataLoader.Locations(Game1.content));
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
			_monitor.Log("Parsing Fruit Tree data", _config.DebugLogLevel());

			_forageableTracker.FruitTreeForageables.Clear();
			_forageableTracker.FruitTreeForageables.AddRange(
				ForageableItem.ParseFruitTreeData(
					data,
					_config?.ForageToggles[Constants.FruitTreeToggleKey],
					_monitor));
			_forageableTracker.FruitTreeForageables.SortByDisplayName();
		}

		/// <summary>
		/// Parses Wild Tree data into forageable items.
		/// </summary>
		private void ParseWildTreeData(Dictionary<string, WildTreeData> data)
		{
			_monitor.Log("Parsing Wild Tree data", _config.DebugLogLevel());

			_forageableTracker.WildTreeForageables.Clear();
			_forageableTracker.WildTreeForageables.AddRange(
				ForageableItem.ParseWildTreeData(
				data,
				_config?.ForageToggles[Constants.WildTreeToggleKey],
				_monitor));
			_forageableTracker.WildTreeForageables.SortByDisplayName();
		}

		/// <summary>
		/// Parses Object data into forageable and bush items.
		/// </summary>
		private void ParseObjectData(Dictionary<string, ObjectData> data)
		{
			var parsedObjectForageableItems = ForageableItem.ParseObjectData(data, _config, _monitor);

			_monitor.Log("Parsing Object data", _config.DebugLogLevel());

			_forageableTracker.ObjectForageables.Clear();
			_forageableTracker.ObjectForageables.AddRange(parsedObjectForageableItems.Item1);
			_forageableTracker.ObjectForageables.SortByDisplayName();

			_forageableTracker.BushForageables.Clear();
			_forageableTracker.BushForageables.AddRange(parsedObjectForageableItems.Item2);
			_forageableTracker.BushForageables.SortByDisplayName();

			if (_locationCache is not null && _locationCache.Count > 0)
			{
				_monitor.Log("Sub-Object: Parsing Location data", _config.DebugLogLevel());

				_forageableTracker.ObjectForageables.AddOrMergeCustomFieldsRange(
					ForageableItem.ParseLocationData(
						_locationCache,
						_config?.ForageToggles[Constants.ForagingToggleKey],
						_monitor));
				_forageableTracker.ObjectForageables.SortByDisplayName();
			}
		}

		/// <summary>
		/// Parses Location data into forageable items.
		/// </summary>
		private void ParseLocationData(Dictionary<string, LocationData> data)
		{
			if (_objectCache is null || _objectCache.Count == 0)
			{
				_monitor.Log("Sub-Location: Grabbing Object data", _config.DebugLogLevel());
				_objectCache = DataLoader.Objects(Game1.content);
			}

			_monitor.Log("Parsing Location data", _config.DebugLogLevel());

			_forageableTracker.ObjectForageables.AddOrMergeCustomFieldsRange(
				ForageableItem.ParseLocationData(
					data,
					_config?.ForageToggles[Constants.ForagingToggleKey],
					_monitor));
			_forageableTracker.ObjectForageables.SortByDisplayName();
		}
	}
}
