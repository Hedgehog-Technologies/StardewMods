using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.Objects;
using StardewValley.GameData.WildTrees;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using AutoForager.Classes;
using AutoForager.Handlers;
using AutoForager.Services;
using HedgeTech.Common.Extensions;
using HedgeTech.Common.Helpers;

using Constants = AutoForager.Helpers.Constants;
using SObject = StardewValley.Object;

namespace AutoForager
{
	public class ModEntry : Mod
	{
		// Services
		private ConfigurationService _configService;
		private AssetService _assetService;
		private ContentPackService _contentPackService;

		// Handlers
		private WildTreeHandler _wildTreeHandler;
		private FruitTreeHandler _fruitTreeHandler;
		private BushHandler _bushHandler;
		private TerrainFeatureHandler _terrainFeatureHandler;
		private ObjectHandler _objectHandler;
		private ArtifactSpotHandler _artifactSpotHandler;
		private MachineHandler _machineHandler;
		private PanningHandler _panningHandler;

		// State
		private ModConfig _config;
		private readonly JsonHelper _jsonHelper;
		private readonly ForageableItemTracker _forageableTracker;

		private bool _gameStarted = false;
		private Vector2 _previousTilePosition;

		// Mushroom log tracking
		private readonly List<Tree> _mushroomLogTrees;
		private readonly object _mushroomLogTreesLock;

		// Item lists
		private readonly List<string> _overrideItemIds;
		private readonly List<string> _ignoreItemIds;

		// Tracking counts
		private readonly Dictionary<string, Dictionary<string, int>> _trackingCounts;

		public ModEntry()
		{
			_jsonHelper = new JsonHelper();
			_forageableTracker = ForageableItemTracker.Instance;
			_gameStarted = false;

			_mushroomLogTrees = [];
			_mushroomLogTreesLock = new();

			_overrideItemIds = [
				"(O)80",  // Quartz
				"(O)82",  // Fire Quartz
				"(O)84",  // Frozen Tear
				"(O)86",  // Earth Crystal
				"(O)107", // Dinosaur Egg
				"(O)152", // Seaweed
				"(O)174", // Large Egg (white)
				"(O)176", // Egg (white)
				"(O)180", // Egg (brown)
				"(O)182", // Large Egg (brown)
				"(O)289", // Ostrich Egg
				"(O)296", // Salmonberry
				"(O)305", // Void Egg
				"(O)416", // Snow Yam
				"(O)430", // Truffle
				"(O)440", // Wool
				"(O)442", // Duck Egg
				"(O)444", // Duck Feather
				"(O)446", // Rabbit's Foot
				"(O)613", // Apple
				"(O)634", // Apricot
				"(O)635", // Orange
				"(O)636", // Peach
				"(O)637", // Pomegranate
				"(O)638", // Cherry
				"(O)851", // Magma Cap
				"(O)852", // Dragon Tooth
				"(O)928", // Golden Egg
				"(O)Moss" // Moss
			];

			_ignoreItemIds = [
				"(O)78",       // Cave Carrot
				"(O)166",      // Treasure Chest
				"(O)463",      // Drum Block
				"(O)464",      // Flute Block
				"(O)590",      // Artifact Spot
				"(O)922",      // Supply Crate
				"(O)923",      // Supply Crate
				"(O)924",      // Supply Crate
				"(O)SeedSpot", // Seed Spot
			];

			_trackingCounts = new()
			{
				{ Constants.BushKey, new() },
				{ Constants.ForageableKey, new() },
				{ Constants.FruitTreeKey, new() },
				{ Constants.WildTreeKey, new() }
			};
		}

		public override void Entry(IModHelper helper)
		{
			I18n.Init(helper.Translation);

			// Initialize services
			_configService = new ConfigurationService(helper, ModManifest, Monitor, _jsonHelper);
			_config = _configService.LoadConfiguration();

			_assetService = new AssetService(Monitor, _config, _forageableTracker);
			_contentPackService = new ContentPackService(Monitor, helper, _config);

			// Register event handlers
			helper.Events.Content.AssetReady += OnAssetReady;
			helper.Events.Content.AssetRequested += OnAssetRequested;
			helper.Events.Content.LocaleChanged += OnLocaleChanged;
			helper.Events.GameLoop.DayEnding += OnDayEnding;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
			helper.Events.GameLoop.UpdateTicked += InitializeMod;
			helper.Events.Input.ButtonsChanged += OnButtonsChanged;
			helper.Events.Player.Warped += OnPlayerWarped;
			helper.Events.World.ObjectListChanged += OnObjectListChanged;

			if (_config.AutoForagingEnabled)
			{
				helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
			}
		}

		#region Event Handlers

		[EventPriority(EventPriority.Low)]
		private void OnAssetReady(object? send, AssetReadyEventArgs e)
		{
			if (!_gameStarted) return;
			_assetService.HandleAssetReady(e.Name.BaseName);
		}

		[EventPriority(EventPriority.Low)]
		private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
		{
			var assetName = e.Name.BaseName;

			if (assetName.IEquals(Constants.FruitTreesAssetName))
			{
				e.Edit(EditFruitTrees);
			}
			else if (assetName.IEquals(Constants.ObjectsAssetName))
			{
				e.Edit(EditObjects);
			}
			else if (assetName.IEquals(Constants.WildTreesAssetName))
			{
				e.Edit(EditWildTrees);
			}
		}

		private void OnLocaleChanged(object? sender, LocaleChangedEventArgs e)
		{
			_assetService.ReloadAllAssets();
			_configService.UpdateUtilities(Helper.ContentPacks.GetOwned());
			_configService.RegisterConfigMenu(_forageableTracker, new CategoryComparer(Helper.ContentPacks.GetOwned()));
			_configService.UpdateEnabled();
		}

		private void OnDayEnding(object? sender, DayEndingEventArgs e)
		{
			LogDailyStatistics();
		}

		private void OnDayStarted(object? sender, DayStartedEventArgs e)
		{
			_previousTilePosition = Game1.player.Tile;
		}

		private async void InitializeMod(object? sender, UpdateTickedEventArgs e)
		{
			if (IsTitleMenuInteractable() || Context.IsPlayerFree)
			{
				Helper.Events.GameLoop.UpdateTicked -= InitializeMod;

				// Load content packs and integrations
				await _contentPackService.LoadAllContentAsync(_ignoreItemIds);

				// Initialize handlers
				InitializeHandlers();

				// Invalidate and load assets
				try
				{
					Helper.GameContent.InvalidateCache(Constants.ObjectsAssetName);
				}
				catch (Exception ex)
				{
					Monitor.Log($"{ex.Message}{Environment.NewLine}{ex.StackTrace}", LogLevel.Warn);
				}

				_assetService.LoadInitialAssets();

				// Configure and register menu
				_configService.UpdateUtilities(Helper.ContentPacks.GetOwned());
				_configService.AddFtmCategories(_contentPackService.FTMForageables);
				_configService.RegisterConfigMenu(_forageableTracker, new CategoryComparer(Helper.ContentPacks.GetOwned()));
				_configService.UpdateEnabled();

				_gameStarted = true;
			}
		}

		private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
		{
			if (!Context.IsPlayerFree) return;
			if (Game1.currentLocation is null || Game1.player is null) return;
			if (Game1.player.Tile.Equals(_previousTilePosition)) return;

			_previousTilePosition = Game1.player.Tile;

			var context = CreateForagingContext();
			var playerTilePoint = context.PlayerTilePoint;
			var radius = context.ForagingRadius;

			foreach (var vec in GetTilesToCheck(playerTilePoint, radius))
			{
				// Handle terrain features
				if (Game1.currentLocation.terrainFeatures.TryGetValue(vec, out var feature))
				{
					HandleTerrainFeature(feature, vec, context);
				}

				// Handle objects
				if (Game1.currentLocation.Objects.TryGetValue(vec, out var obj))
				{
					HandleObject(obj, vec, feature, context);
				}

				// Handle large terrain features (large bushes)
				var largeTerrainFeature = Game1.currentLocation.getLargeTerrainFeatureAt((int)vec.X, (int)vec.Y);
				if (largeTerrainFeature is Bush largeBush)
				{
					if (_bushHandler.CanHandle(largeBush))
					{
						_bushHandler.Handle(largeBush);
					}
				}
			}

			// Handle panning (special case - location-based, not tile-based)
			_panningHandler.CheckAndHandle();
		}

		private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
		{
			if (Game1.activeClickableMenu is not null) return;
			if (!_config.ToggleForagerKeybind.JustPressed()) return;

			var newState = _configService.ToggleAutoForagingAsync().Result;

			var stateText = newState ? I18n.State_Activated() : I18n.State_Deactivated();
			var message = I18n.Message_AutoForagerToggled(stateText);

			Monitor.Log(message, LogLevel.Info);
			Game1.addHUDMessage(new HUDMessage(message) { noIcon = true });

			if (newState)
			{
				Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
			}
			else
			{
				Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
			}
		}

		private void OnPlayerWarped(object? sender, WarpedEventArgs e)
		{
			if (e.Player is null || !e.Player.Equals(Game1.player)) return;
			CheckForMushroomLogTrees(e.NewLocation);
		}

		private void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
		{
			if (!e.IsCurrentLocation) return;
			if (!e.Removed.Any(r => r.Value.QualifiedItemId.IEquals("(BC)MushroomLog"))
				&& !e.Added.Any(r => r.Value.QualifiedItemId.IEquals("(BC)MushroomLog")))
			{
				return;
			}

			CheckForMushroomLogTrees(e.Location);
		}

		#endregion Event Handlers

		#region Handler Management

		private void InitializeHandlers()
		{
			var context = CreateForagingContext();

			// Initialize all handlers
			_wildTreeHandler = new WildTreeHandler(_mushroomLogTrees, _mushroomLogTreesLock);
			_wildTreeHandler.Initialize(context);

			_fruitTreeHandler = new FruitTreeHandler();
			_fruitTreeHandler.Initialize(context);

			_bushHandler = new BushHandler(_contentPackService.CustomBushWrapper);
			_bushHandler.Initialize(context);

			_terrainFeatureHandler = new TerrainFeatureHandler();
			_terrainFeatureHandler.Initialize(context);

			_objectHandler = new ObjectHandler();
			_objectHandler.Initialize(context);

			_artifactSpotHandler = new ArtifactSpotHandler();
			_artifactSpotHandler.Initialize(context);

			_machineHandler = new MachineHandler();
			_machineHandler.Initialize(context);

			_panningHandler = new PanningHandler();
			_panningHandler.Initialize(context);
		}

		private void HandleTerrainFeature(TerrainFeature feature, Vector2 tile, IForagingContext context)
		{
			switch (feature)
			{
				case Tree tree:
					if (_wildTreeHandler.CanHandle(tree))
					{
						_wildTreeHandler.Handle(tree);
					}
					break;

				case FruitTree fruitTree:
					if (_fruitTreeHandler.CanHandle(fruitTree))
					{
						_fruitTreeHandler.Handle(fruitTree);
					}
					break;

				case Bush bush:
					if (_bushHandler.CanHandle(bush))
					{
						_bushHandler.Handle(bush);
					}
					break;

				case HoeDirt hoeDirt:
					if (_terrainFeatureHandler.CanHandle(hoeDirt))
					{
						_terrainFeatureHandler.Handle(hoeDirt, tile);
					}
					break;
			}
		}

		private void HandleObject(SObject obj, Vector2 tile, TerrainFeature? terrainFeature, IForagingContext context)
		{
			// Priority order: artifact spots > machines > regular objects
			if (_artifactSpotHandler.CanHandle(obj))
			{
				_artifactSpotHandler.Handle(obj, tile);
			}
			else if (_machineHandler.CanHandle(obj))
			{
				_machineHandler.Handle(obj, tile, terrainFeature);
			}
			else if (_objectHandler.CanHandle(obj))
			{
				_objectHandler.Handle(obj, tile);
			}
		}

		private IForagingContext CreateForagingContext()
		{
			return new ForagingContext(_config, Monitor, _forageableTracker, _trackingCounts);
		}

		#endregion Handler Management

		#region Asset Editing

		private void EditFruitTrees(IAssetData asset)
		{
			var fruitTreeData = asset.AsDictionary<string, FruitTreeData>();

			foreach (var fruitTree in fruitTreeData.Data)
			{
				fruitTree.Value.CustomFields ??= [];
				fruitTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldForageableKey, "true");

				if (Constants.VanillaFruitTrees.Contains(fruitTree.Key))
				{
					fruitTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, "Category.Vanilla");
				}
				else if (_contentPackService.CPFruitTrees.TryGetValue(fruitTree.Key, out var category))
				{
					fruitTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, category);
				}
			}
		}

		private void EditObjects(IAssetData asset)
		{
			var objectData = asset.AsDictionary<string, ObjectData>();

			foreach (var obj in objectData.Data)
			{
				if (_ignoreItemIds.Any(i => obj.Key.IEquals(i.Substring(3)))) continue;
				if (obj.Value.Category == SObject.litterCategory) continue;

				string? category = null;

				if (!Constants.KnownCategoryLookup.TryGetValue(obj.Key, out var knownCategory)
					&& _contentPackService.CPForageables.TryGetValue(obj.Key, out var cpCategory))
				{
					category = cpCategory;
				}
				else if ((obj.Value.ContextTags?.Contains("forage_item") ?? false)
					|| _overrideItemIds.Any(i => obj.Key.IEquals(i.Substring(3))))
				{
					if (!knownCategory.IsNullOrEmpty())
					{
						category = knownCategory;
					}
					else
					{
						category = string.Empty;
					}
				}

				if (category is not null)
				{
					obj.Value.CustomFields ??= [];
					obj.Value.CustomFields.TryAdd(Constants.CustomFieldForageableKey, "true");

					if (category != string.Empty)
					{
						obj.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, category);
					}
				}

				if (_contentPackService.BushBloomItems?.TryGetValue(obj.Key, out var bushCategory) ?? false)
				{
					obj.Value.CustomFields ??= [];
					obj.Value.CustomFields.TryAdd(Constants.CustomFieldBushKey, "true");
					obj.Value.CustomFields.TryAdd(Constants.CustomFieldBushBloomCategory, bushCategory);
				}

				if (_contentPackService.CustomTeaBushItems?.TryGetValue(obj.Key, out var customBushCategory) ?? false)
				{
					obj.Value.CustomFields ??= [];
					obj.Value.CustomFields.TryAdd(Constants.CustomFieldBushKey, "true");
					obj.Value.CustomFields.TryAdd(Constants.CustomFieldCustomBushCategory, customBushCategory);
				}

				if (_contentPackService.FTMForageables?.TryGetValue(obj.Key, out var ftmCategory) ?? false)
				{
					obj.Value.CustomFields ??= [];
					obj.Value.CustomFields.TryAdd(Constants.CustomFieldForageableKey, "true");
					obj.Value.CustomFields.TryAdd(Constants.CustomFieldCategoryKey, ftmCategory);
				}
			}
		}

		private void EditWildTrees(IAssetData asset)
		{
			var wildTreeData = asset.AsDictionary<string, WildTreeData>();

			foreach (var wildTree in wildTreeData.Data)
			{
				// Just say no to mushroom trees
				if (wildTree.Key.Equals("7")) continue;

				wildTree.Value.CustomFields ??= [];
				wildTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldForageableKey, "true");

				if (Constants.VanillaWildTrees.Contains(wildTree.Key))
				{
					wildTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, "Category.Vanilla");
				}
				else if (_contentPackService.CPWildTrees.TryGetValue(wildTree.Key, out var category))
				{
					wildTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, category);
				}
			}
		}

		#endregion Asset Editing

		#region Utility Methods

		private void CheckForMushroomLogTrees(GameLocation location)
		{
			var locObjects = location.Objects;

			lock (_mushroomLogTreesLock)
			{
				_mushroomLogTrees.Clear();

				foreach (var kvp in locObjects.Pairs)
				{
					var obj = kvp.Value;
					if (obj is null) continue;

					if (obj.QualifiedItemId.IEquals("(BC)MushroomLog"))
					{
						foreach (var vec in GetTilesToCheck(obj.TileLocation.ToPoint(), 3))
						{
							if (location.terrainFeatures.TryGetValue(vec, out var feat) && feat is Tree tree)
							{
								_mushroomLogTrees.AddDistinct(tree);
							}
						}
					}
				}
			}
		}

		private void LogDailyStatistics()
		{
			StringBuilder statMessage = new($"{Environment.NewLine}{Utility.getDateString()}:{Environment.NewLine}");
			statMessage.AppendLine(I18n.Log_Eod_TotalStat(_trackingCounts.SumAll()));

			foreach (var category in _trackingCounts)
			{
				if (category.Value.Count == 0) continue;

				statMessage.AppendLine($"[{category.Value.SumAll()}] {Helper.Translation.Get(category.Key)}:");

				foreach (var interactable in category.Value)
				{
					if (interactable.Value <= 0)
					{
						Monitor.Log($"Invalid forageable value for {interactable.Key}; {interactable.Value}. How did we get here?", LogLevel.Warn);
						continue;
					}

					statMessage.AppendLine(I18n.Log_Eod_Stat(interactable.Value, interactable.Key));
				}

				category.Value.Clear();
			}

			Monitor.Log(statMessage.ToString(), LogLevel.Info);
		}

		private bool IsTitleMenuInteractable()
		{
			if (Game1.activeClickableMenu is not TitleMenu titleMenu || TitleMenu.subMenu is not null)
			{
				return false;
			}

			var method = Helper.Reflection.GetMethod(titleMenu, "ShouldAllowInteraction", false);

			if (method is not null)
			{
				return method.Invoke<bool>();
			}
			else
			{
				return Helper.Reflection.GetField<bool>(titleMenu, "titleInPosition").GetValue();
			}
		}

		private static IEnumerable<Vector2> GetTilesToCheck(Point origin, int radius)
		{
			for (int x = Math.Max(origin.X - radius, 0); x <= origin.X + radius; x++)
			{
				for (int y = Math.Max(origin.Y - radius, 0); y <= origin.Y + radius; y++)
				{
					yield return new Vector2(x, y);
				}
			}
		}

		#endregion Utility Methods
	}
}
