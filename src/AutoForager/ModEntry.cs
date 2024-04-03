using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.FruitTrees;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Objects;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using AutoForager.Classes;
using AutoForager.Helpers;
using AutoForager.Integrations;

using SObject = StardewValley.Object;
using Constants = AutoForager.Helpers.Constants;

namespace AutoForager
{
    /// <summary>
    /// The mod entry point.
    /// </summary>
    public class ModEntry : Mod
    {
        private ModConfig _config;

        private bool _gameStarted;
        private Vector2 _previousTilePosition;

        private readonly List<string> _overrideItemIds;
        private readonly List<string> _ignoreItemIds;

        private readonly ForageableItemTracker _forageableTracker;

        private readonly Dictionary<Vector2, string> _artifactPredictions;
        private readonly Dictionary<string, Dictionary<string, int>> _trackingCounts;

        private DateTime _nextErrorMessage;

        private readonly Dictionary<string, string> _cpBushBlooms;
        private readonly Dictionary<string, string> _cpForageables;
        private readonly Dictionary<string, string> _cpFruitTrees;
        private readonly Dictionary<string, string> _cpWildTrees;

        private BushBloomWrapper _bbw;

        #region Asset Cache

        private Dictionary<string, FruitTreeData> _fruitTreeCache = new();
        private Dictionary<string, FruitTreeData> FruitTreeCache
        {
            get => _fruitTreeCache;
            set
            {
                _fruitTreeCache = value;
                ParseAssets(value);
            }
        }

        private Dictionary<string, LocationData> _locationCache = new();
        private Dictionary<string, LocationData> LocationCache
        {
            get => _locationCache;
            set
            {
                _locationCache = value;
                ParseAssets(value);
            }
        }

        private Dictionary<string, ObjectData> _objectCache = new();
        private Dictionary<string, ObjectData> ObjectCache
        {
            get => _objectCache;
            set
            {
                _objectCache = value;
                ParseAssets(value);
            }
        }

        private Dictionary<string, WildTreeData> _wildTreeCache = new();
        private Dictionary<string, WildTreeData> WildTreeCache
        {
            get => _wildTreeCache;
            set
            {
                _wildTreeCache = value;
                ParseAssets(value);
            }
        }

        private void ParseAssets<T>(Dictionary<string, T> data)
        {
            if (data is Dictionary<string, FruitTreeData> fruitTreeData)
            {
                _forageableTracker.FruitTreeForageables.Clear();
                _forageableTracker.FruitTreeForageables.AddRange(ForageableItem.ParseFruitTreeData(fruitTreeData, _config?.ForageToggles[Constants.FruitTreeToggleKey], Monitor));
                _forageableTracker.FruitTreeForageables.SortByDisplayName();
                Monitor.Log("Parsing Fruit Tree Data", LogLevel.Debug);
            }
            else if (data is Dictionary<string, LocationData> locationData)
            {
                if (ObjectCache is null || ObjectCache.Count == 0)
                {
                    ObjectCache = Game1.content.Load<Dictionary<string, ObjectData>>(Constants.ObjectsAssetName);
                    Monitor.Log("Sub-Location: Grabbing Object Data", LogLevel.Debug);
                }

                _forageableTracker.ArtifactForageables.Clear();
                _forageableTracker.ArtifactForageables.AddRange(ForageableItem.ParseLocationData(ObjectCache, locationData, _config?.ForageToggles[Constants.ForagingToggleKey]));
                _forageableTracker.ArtifactForageables.SortByDisplayName();
                Monitor.Log("Parsing Location Data", LogLevel.Debug);
            }
            else if (data is Dictionary<string, ObjectData> objectData)
            {
                _forageableTracker.ObjectForageables.Clear();
                _forageableTracker.ObjectForageables.AddRange(ForageableItem.ParseObjectData(objectData, _config?.ForageToggles[Constants.ForagingToggleKey], Monitor));
                _forageableTracker.ObjectForageables.SortByDisplayName();
                Monitor.Log("Parsing Object Data", LogLevel.Debug);

                if (LocationCache is not null && LocationCache.Count > 0)
                {
                    _forageableTracker.ArtifactForageables.Clear();
                    _forageableTracker.ArtifactForageables.AddRange(ForageableItem.ParseLocationData(objectData, LocationCache, _config?.ForageToggles[Constants.ForagingToggleKey]));
                    _forageableTracker.ArtifactForageables.SortByDisplayName();
                    Monitor.Log("Sub-Object: Parsing Location Data", LogLevel.Debug);
                }
            }
            else if (data is Dictionary<string, WildTreeData> wildTreeData)
            {
                _forageableTracker.WildTreeForageables.Clear();
                _forageableTracker.WildTreeForageables.AddRange(ForageableItem.ParseWildTreeData(wildTreeData, _config?.ForageToggles[Constants.WildTreeToggleKey], Monitor));
                _forageableTracker.WildTreeForageables.SortByDisplayName();
                Monitor.Log("Parsing Wild Tree Data", LogLevel.Debug);
            }

            if (_config is not null && _gameStarted)
            {
                Monitor.Log("Reregistering Generic Mod Config Menu", LogLevel.Debug);
                _config.RegisterModConfigMenu(Helper, ModManifest);
            }
        }

        #endregion Asset Cache

        public ModEntry()
        {
            _config = new();
            _gameStarted = false;

            _cpBushBlooms = new();
            _cpForageables = new();
            _cpFruitTrees = new();
            _cpWildTrees = new();

            _overrideItemIds = new()
            {
                "(O)152", // Seaweed
                "(O)416", // Snow Yam
                "(O)430", // Truffle
                "(O)851", // Magma Cap
                "(O)Moss" // Moss
            };

            _ignoreItemIds = new()
            {
                "(O)78"  // Cave Carrot
            };

            _forageableTracker = ForageableItemTracker.Instance;
            _artifactPredictions = new();

            _trackingCounts = new()
            {
                { Constants.BushKey, new() },
                { Constants.ForageableKey, new() },
                { Constants.FruitTreeKey, new() },
                { Constants.WildTreeKey, new() }
            };

            _nextErrorMessage = DateTime.MinValue;
        }

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            var packs = helper.ContentPacks.GetOwned();

            _config = helper.ReadConfig<ModConfig>();
            _config.UpdateEnabled(helper);
            _config.UpdateUtilities(Monitor, packs);

            ParseContentPacks(packs);

            helper.Events.Content.AssetReady += OnAssetReady;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.LocaleChanged += OnLocaleChanged;
            helper.Events.GameLoop.DayEnding += OnDayEnding;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged; // Maybe change this to pressed or released?
            helper.Events.Player.Warped += OnPlayerWarped;
        }

        [EventPriority(EventPriority.Low)]
        private void OnAssetReady(object? sender, AssetReadyEventArgs e)
        {
            if (!_gameStarted) return;

            var name = e.Name.BaseName;

            if (name.IEquals(Constants.FruitTreesAssetName))
            {
                FruitTreeCache = Game1.content.Load<Dictionary<string, FruitTreeData>>(Constants.FruitTreesAssetName);
            }
            else if (name.IEquals(Constants.LocationsAssetName))
            {
                LocationCache = Game1.content.Load<Dictionary<string, LocationData>>(Constants.LocationsAssetName);
            }
            else if (name.IEquals(Constants.ObjectsAssetName))
            {
                ObjectCache = Game1.content.Load<Dictionary<string, ObjectData>>(Constants.ObjectsAssetName);
            }
            else if (name.IEquals(Constants.WildTreesAssetName))
            {
                WildTreeCache = Game1.content.Load<Dictionary<string, WildTreeData>>(Constants.WildTreesAssetName);
            }
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
            ItemRegistry.ResetCache();

            FruitTreeCache = Game1.content.Load<Dictionary<string, FruitTreeData>>(Constants.FruitTreesAssetName);
            WildTreeCache = Game1.content.Load<Dictionary<string, WildTreeData>>(Constants.WildTreesAssetName);
            ObjectCache = Game1.content.Load<Dictionary<string, ObjectData>>(Constants.ObjectsAssetName);
            LocationCache = Game1.content.Load<Dictionary<string, LocationData>>(Constants.LocationsAssetName);

            _config.RegisterModConfigMenu(Helper, ModManifest);
            _config.UpdateEnabled(Helper);
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
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

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            _previousTilePosition = Game1.player.Tile;
        }

        // We are using OneSecondUpdateTicked instead of GameStart because we don't want to block other events during game start
        // but need to more or less blind wait ~1 second before loading assets to give time for various cache's to catch up with
        // mods being loaded.
        [EventPriority(EventPriority.Low)]
        private async void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            _bbw = new BushBloomWrapper(Monitor, Helper);
            await _bbw.GetSchedules();

            foreach (var sched in _bbw.Schedules)
            {
                // $TODO - Setup storing of bush blooms
                // $TODO - Hook up bush bloom storage to config
                // $TODO - Update bush check logic to check bush bloom storage
            }

            FruitTreeCache = Game1.content.Load<Dictionary<string, FruitTreeData>>(Constants.FruitTreesAssetName);
            WildTreeCache = Game1.content.Load<Dictionary<string, WildTreeData>>(Constants.WildTreesAssetName);
            ObjectCache = Game1.content.Load<Dictionary<string, ObjectData>>(Constants.ObjectsAssetName);
            LocationCache = Game1.content.Load<Dictionary<string, LocationData>>(Constants.LocationsAssetName);

            _config.RegisterModConfigMenu(Helper, ModManifest);
            _config.UpdateEnabled(Helper);

            _gameStarted = true;

            Helper.Events.GameLoop.OneSecondUpdateTicked -= OnOneSecondUpdateTicked;
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree) return;
            if (Game1.currentLocation is null || Game1.player is null) return;
            if (Game1.player.Tile.Equals(_previousTilePosition)) return;

            _previousTilePosition = Game1.player.Tile;
            var playerTileLocationPoint = Game1.player.TilePoint;
            var playerMagnetism = Game1.player.GetAppliedMagneticRadius();
            var radius = _config.UsePlayerMagnetism ? playerMagnetism / Game1.tileSize : _config.ShakeDistance;

            foreach (var vec in GetTilesToCheck(playerTileLocationPoint, radius))
            {
                if (Game1.currentLocation.terrainFeatures.TryGetValue(vec, out var feature))
                {
                    switch (feature)
                    {
                        case Tree tree:
                            if (tree.stump.Value) continue;
                            if (tree.growthStage.Value < 5 || (!tree.hasSeed.Value && !tree.hasMoss.Value)) continue;

                            var seedItemIds = tree.GetSeedAndSeedItemIds();
                            if (!tree.wasShakenToday.Value && tree.hasSeed.Value && (Game1.IsMultiplayer || Game1.player.ForagingLevel >= 1) && tree.isActionable()
                                && _forageableTracker.WildTreeForageables.Any(i => (seedItemIds.Contains(i.QualifiedItemId) || seedItemIds.Contains(i.ItemId)) && i.IsEnabled))
                            {
                                tree.performUseAction(tree.Tile);
                                Monitor.Log($"Tree shaken: {string.Join(", ", seedItemIds)}", LogLevel.Debug);

                                foreach (var id in seedItemIds)
                                {
                                    var name = id;

                                    if (_forageableTracker.WildTreeForageables.TryGetItem(id, out var wtItem))
                                    {
                                        name = wtItem?.DisplayName ?? id;
                                    }

                                    _trackingCounts[Constants.WildTreeKey].AddOrIncrement(name);
                                }
                            }
                            else
                            {
                                Monitor.Log($"Tree not shaken: {string.Join(",", seedItemIds)}", LogLevel.Debug);
                            }

                            if (tree.hasMoss.Value
                                && _forageableTracker.ObjectForageables.TryGetItem("(O)Moss", out var mossItem)
                                && (mossItem?.IsEnabled ?? false))
                            {
                                Tool? tool = new GenericTool();

                                if (_config.RequireToolMoss)
                                {
                                    tool = Game1.player.CurrentTool;
                                    tool ??= Game1.player.Items.FirstOrDefault(i => i is Tool, null) as Tool;

                                    if (tool is null)
                                    {
                                        if (_nextErrorMessage < DateTime.UtcNow)
                                        {
                                            Game1.addHUDMessage(new HUDMessage(I18n.Message_MissingToolMoss(), HUDMessage.error_type));
                                            _nextErrorMessage = DateTime.UtcNow.AddSeconds(10);
                                        }

                                        Monitor.LogOnce(I18n.Log_MissingToolMoss(I18n.Option_RequireToolMoss_Name(" ")), LogLevel.Debug);
                                        continue;
                                    }
                                }

                                tree.performToolAction(tool, -1, tree.Tile);
                                _trackingCounts[Constants.ForageableKey].AddOrIncrement(mossItem.DisplayName);
                            }

                            break;

                        case FruitTree fruitTree:
                            if (fruitTree.stump.Value) continue;
                            if (fruitTree.growthStage.Value < 4) continue;

                            var fruitCount = fruitTree.fruit.Count;
                            if (fruitCount <= 0 || fruitCount < _config.FruitsReadyToShake) continue;

                            var fruitItemIds = fruitTree.GetFruitItemIds();
                            if (_forageableTracker.FruitTreeForageables.Any(i => fruitItemIds.Contains(i.QualifiedItemId) && i.IsEnabled))
                            {
                                fruitTree.performUseAction(fruitTree.Tile);
                                Monitor.Log($"Fruit Tree shaken: {string.Join(", ", fruitItemIds)}", LogLevel.Debug);

                                foreach (var id in fruitItemIds)
                                {
                                    var name = id;

                                    if (_forageableTracker.FruitTreeForageables.TryGetItem(id, out var ftItem))
                                    {
                                        name = ftItem?.DisplayName ?? id;
                                    }

                                    _trackingCounts[Constants.FruitTreeKey].AddOrIncrement(name);
                                }
                            }
                            else
                            {
                                Monitor.Log($"Fruit Tree not shaken: {string.Join(", ", fruitItemIds)}", LogLevel.Debug);
                            }

                            break;

                        case Bush bush:
                            if (!CheckBush(bush)) continue;

                            bush.performUseAction(bush.Tile);

                            break;

                        case HoeDirt hoeDirt:
                            if (!(hoeDirt.crop?.forageCrop.Value ?? false) || (hoeDirt.crop?.whichForageCrop.Value.IsNullOrEmpty() ?? true)) continue;
                            if (!_forageableTracker.ObjectForageables.Any(i =>
                            {
                                if (i.IsEnabled && i.CustomFields.TryGetValue(Constants.CustomFieldCategoryKey, out var category))
                                {
                                    return category.IEquals("Special");
                                }

                                return false;
                            })) continue;

                            Vector2 tile;
                            var whichCrop = hoeDirt.crop.whichForageCrop.Value;

                            switch (whichCrop)
                            {
                                case Crop.forageCrop_springOnionID:
                                    var springOnion = _forageableTracker.ObjectForageables.FirstOrDefault(i => i.QualifiedItemId.Equals("(O)399"));

                                    if (springOnion != default(ForageableItem) && springOnion.IsEnabled)
                                    {
                                        tile = hoeDirt.Tile;
                                        var x = (int)tile.X;
                                        var y = (int)tile.Y;

                                        ForageItem(ItemRegistry.Create<SObject>("(O)399"), tile, Utility.CreateDaySaveRandom(x * 1000, y * 2000), 3);
                                        hoeDirt.destroyCrop(false);
                                        Game1.playSound("harvest");

                                        _trackingCounts[Constants.ForageableKey].AddOrIncrement(springOnion.DisplayName);
                                    }

                                    break;

                                case Crop.forageCrop_gingerID:
                                    var ginger = _forageableTracker.ObjectForageables.FirstOrDefault(i => i.QualifiedItemId == "(O)829");

                                    if (ginger != default(ForageableItem) && ginger.IsEnabled)
                                    {
                                        if (_config.RequireHoe && !Game1.player.Items.Any(i => i is Hoe))
                                        {
                                            if (_nextErrorMessage < DateTime.UtcNow)
                                            {
                                                Game1.addHUDMessage(new HUDMessage(I18n.Message_MissingHoe(I18n.Subject_GingerRoots()), HUDMessage.error_type));
                                                _nextErrorMessage = DateTime.UtcNow.AddSeconds(10);
                                            }

                                            Monitor.LogOnce(I18n.Log_MissingHoe(I18n.Subject_GingerRoots(), I18n.Option_RequireHoe_Name(" ")), LogLevel.Debug);
                                            continue;
                                        }

                                        tile = hoeDirt.Tile;
                                        hoeDirt.crop?.hitWithHoe((int)tile.X, (int)tile.Y, hoeDirt.Location, hoeDirt);
                                        hoeDirt.destroyCrop(false);

                                        _trackingCounts[Constants.ForageableKey].AddOrIncrement(ginger.DisplayName);
                                    }

                                    break;

                                default:
                                    // $TODO - Improve error message
                                    Monitor.Log($"No good case: {whichCrop}", LogLevel.Debug);
                                    break;
                            }

                            break;
                    }
                }

                if (Game1.currentLocation.Objects.TryGetValue(vec, out var obj))
                {
                    // Forageable Item
                    if (obj.isForage() && obj.IsSpawnedObject && !obj.questItem.Value)
                    {
                        if (_forageableTracker.ObjectForageables.TryGetItem(obj.QualifiedItemId, out var objItem) && (objItem?.IsEnabled ?? false))
                        {
                            ForageItem(obj, vec, Utility.CreateDaySaveRandom(vec.X, vec.Y * 777f), 7, true);

                            Game1.player.currentLocation.removeObject(vec, false);
                            Game1.playSound("harvest");

                            _trackingCounts[Constants.ForageableKey].AddOrIncrement(objItem.DisplayName);
                        }
                    }
                    // Artifact Spot
                    else if (obj.QualifiedItemId.Equals("(O)590") && _artifactPredictions.ContainsKey(vec))
                    {
                        var prediction = _artifactPredictions[vec];

                        if (_forageableTracker.ArtifactForageables.TryGetItem(prediction, out var objItem) && (objItem?.IsEnabled ?? false))
                        {
                            if (_config.RequireHoe && !Game1.player.Items.Any(i => i is Hoe))
                            {
                                if (_nextErrorMessage < DateTime.UtcNow)
                                {
                                    Game1.addHUDMessage(new HUDMessage(I18n.Message_MissingHoe(objItem.DisplayName), HUDMessage.error_type));
                                    _nextErrorMessage = DateTime.UtcNow.AddSeconds(10);
                                }

                                Monitor.LogOnce(I18n.Log_MissingHoe(objItem.DisplayName, I18n.Option_RequireHoe_Name(" ")), LogLevel.Debug);
                                continue;
                            }

                            Game1.currentLocation.digUpArtifactSpot((int)vec.X, (int)vec.Y, Game1.player);

                            if (!Game1.currentLocation.terrainFeatures.ContainsKey(vec))
                            {
                                Game1.currentLocation.makeHoeDirt(vec, ignoreChecks: true);
                            }

                            Game1.currentLocation.playSound("hoeHit");
                            Game1.currentLocation.removeObject(vec, false);

                            _trackingCounts[Constants.ForageableKey].AddOrIncrement(objItem.DisplayName);
                        }
                    }
                }

                var largeTerrainFeature = Game1.currentLocation.getLargeTerrainFeatureAt((int)vec.X, (int)vec.Y);
                if (largeTerrainFeature is not null && largeTerrainFeature is Bush largeBush)
                {
                    if (CheckBush(largeBush))
                    {
                        largeBush.performUseAction(vec);
                    }
                }
            }
        }

        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (Game1.activeClickableMenu is not null) return;
            if (!_config.ToggleForagerKeybind.JustPressed()) return;

            _config.IsForagerActive = !_config.IsForagerActive;
            Task.Run(() => Helper.WriteConfig(_config)).ContinueWith(t =>
                Monitor.Log(t.Status == TaskStatus.RanToCompletion ? "Config saved successfully!" : $"Saving config unsuccessful {t.Status}", LogLevel.Debug));

            var state = _config.IsForagerActive ? I18n.State_Activated() : I18n.State_Deactivated();
            var message = I18n.Message_AutoForagerToggled(state);

            Monitor.Log(message, LogLevel.Info);
            Game1.addHUDMessage(new HUDMessage(message) { noIcon = true });

            if (_config.IsForagerActive)
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
            if (!e.IsLocalPlayer) return;

            _artifactPredictions.Clear();

            var mapLoc = e.NewLocation;
            var objsPairs = mapLoc.Objects.Pairs;

            foreach (var objPair in objsPairs)
            {
                var vec = objPair.Key;
                var x = (int)vec.X;
                var y = (int)vec.Y;
                var obj = objPair.Value;

                // $NOTE - Sourced from GameLocation.digUpArtifactSpot
                if (obj.QualifiedItemId == "(O)590")
                {
                    var random = Utility.CreateDaySaveRandom(x * 2000, y);
                    var locData = mapLoc.GetData();
                    var context = new ItemQueryContext(mapLoc, Game1.player, random);
                    IEnumerable<ArtifactSpotDropData> artifactSpotDrops = LocationCache["Default"].ArtifactSpots;

                    if (locData is not null && locData.ArtifactSpots?.Count > 0)
                    {
                        artifactSpotDrops = artifactSpotDrops.Concat(locData.ArtifactSpots);
                    }

                    artifactSpotDrops = artifactSpotDrops.OrderBy(p => p.Precedence);
                    foreach (var drop in artifactSpotDrops)
                    {
                        if (!random.NextBool(drop.Chance) || (drop.Condition is not null && !GameStateQuery.CheckConditions(drop.Condition, mapLoc, Game1.player, null, null, random))) continue;

                        var item = ItemQueryResolver.TryResolveRandomItem(drop, context, avoidRepeat: false, null, null, null, delegate (string query, string error)
                        {
                            Monitor.Log($"Error on query resolve: {mapLoc.NameOrUniqueName} ({drop.ItemId}): {query}{Environment.NewLine}{error}", LogLevel.Debug);
                        });

                        if (item is null) continue;

                        if (_forageableTracker.ArtifactForageables.Any(i => i.QualifiedItemId.Equals(item.QualifiedItemId)))
                        {
                            _artifactPredictions.Add(vec, item.QualifiedItemId);
                        }

                        if (!drop.ContinueOnDrop) break;
                    }
                }
            }
        }

        private bool CheckBush(Bush bush)
        {
            if (!_config.AnyBushEnabled()) return false;
            if (bush.townBush.Value) return false;
            if (!bush.inBloom()) return false;
            if (bush.tileSheetOffset.Value != 1) return false;

            if (!bush.isActionable())
            {
                Monitor.Log($"A bush feature of size [{bush.size.Value}] was marked as not actionable. This shouldn't be possible.", LogLevel.Warn);
                Monitor.Log($"Size: [{bush.size.Value}]; Location: [{bush.Location.NameOrUniqueName}]; Tile Location [{bush.Tile}]; Town Bush: [{bush.townBush.Value}]", LogLevel.Debug);
            }

            switch (bush.size.Value)
            {
                // Forageable Bushes
                case 0:
                case 1:
                case 2:
                    var season = Game1.currentSeason;
                    var isSpring = season.IEquals("spring");
                    var isFall = season.IEquals("fall");

                    if (!isSpring && !isFall) return false;

                    if (isSpring && !_config.GetSalmonberryBushesEnabled())
                    {
                        Monitor.LogOnce(I18n.Log_DisabledConfig(I18n.Subject_SalmonberryBushes(), I18n.Option_ToggleAction_Name(I18n.Subject_SalmonberryBushes())), LogLevel.Debug);
                        return false;
                    }

                    if (isFall && !_config.GetBlackberryBushesEnabled())
                    {
                        Monitor.LogOnce(I18n.Log_DisabledConfig(I18n.Subject_BlackberryBushes(), I18n.Option_ToggleAction_Name(I18n.Subject_BlackberryBushes())), LogLevel.Debug);
                        return false;
                    }

                    var subjectName = isSpring
                        ? I18n.Subject_SalmonberryBushes()
                        : I18n.Subject_BlackberryBushes();

                    _trackingCounts[Constants.BushKey].AddOrIncrement(subjectName);

                    break;

                // Tea Bushes
                case 3:
                    if (!_config.GetTeaBushesEnabled())
                    {
                        Monitor.LogOnce(I18n.Log_DisabledConfig(I18n.Subject_TeaBushes(), I18n.Option_ToggleAction_Name(I18n.Subject_TeaBushes())), LogLevel.Debug);
                        return false;
                    }

                    _trackingCounts[Constants.BushKey].AddOrIncrement(I18n.Subject_TeaBushes());

                    break;

                // Walnut Bushes
                case 4:
                    if (!_config.GetWalnutBushesEnabled())
                    {
                        Monitor.LogOnce(I18n.Log_DisabledConfig(I18n.Subject_WalnutBushes(), I18n.Option_ToggleAction_Name(I18n.Subject_WalnutBushes())), LogLevel.Debug);
                        return false;
                    }

                    _trackingCounts[Constants.BushKey].AddOrIncrement(I18n.Subject_WalnutBushes());

                    break;

                default:
                    Monitor.Log($"Unknown Bush size: [{bush.size.Value}]", LogLevel.Warn);
                    return false;
            }

            return true;
        }

        private void EditFruitTrees(IAssetData asset)
        {
            var fruitTreeData = asset.AsDictionary<string, FruitTreeData>();

            foreach (var fruitTree in fruitTreeData.Data)
            {
                fruitTree.Value.CustomFields ??= new();
                fruitTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldForageableKey, "true");

                if (Constants.VanillaFruitTrees.Contains(fruitTree.Key))
                {
                    fruitTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, "Vanilla");
                }
                else if (_cpFruitTrees.TryGetValue(fruitTree.Key, out var category))
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

                string? category = null;

                if (_cpForageables.TryGetValue(obj.Key, out var cpValue))
                {
                    category = cpValue;
                }
                else if ((obj.Value.ContextTags?.Contains("forage_item") ?? false)
                    || _overrideItemIds.Any(i => obj.Key.IEquals(i.Substring(3))))
                {
                    if (Constants.KnownCategoryLookup.TryGetValue(obj.Key, out var knownValue))
                    {
                        category = knownValue;
                    }
                    else
                    {
                        category = string.Empty;
                    }
                }

                if (category is not null)
                {
                    obj.Value.CustomFields ??= new();
                    obj.Value.CustomFields.AddOrUpdate(Constants.CustomFieldForageableKey, "true");

                    if (category != string.Empty)
                    {
                        obj.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, category);
                    }
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

                wildTree.Value.CustomFields ??= new();
                wildTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldForageableKey, "true");

                if (Constants.VanillaWildTrees.Contains(wildTree.Key))
                {
                    wildTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, "Vanilla");
                }
                else if (_cpWildTrees.TryGetValue(wildTree.Key, out var category))
                {
                    wildTree.Value.CustomFields.AddOrUpdate(Constants.CustomFieldCategoryKey, category);
                }
            }
        }

        private void ParseContentPacks(IEnumerable<IContentPack> packs)
        {
            foreach (var pack in packs)
            {
                if (pack is not null)
                {
                    try
                    {
                        var content = pack.ReadJsonFile<ContentEntry>("content.json");
                        Monitor.Log($"Found content pack: {pack.DirectoryPath}", LogLevel.Debug);

                        if (content?.Forageables is not null)
                        {
                            foreach (var itemId in content.Forageables)
                            {
                                if (_cpForageables.ContainsKey(itemId))
                                {
                                    Monitor.Log($"Already found an item with ItemId [{itemId}] with category [{_cpForageables[itemId]}] when trying to add category [{content.Category}]. Please verify you don't have duplicate or conflicting content packs.", LogLevel.Warn);
                                }
                                else
                                {
                                    Monitor.Log($"Found content pack forageable: {itemId} - {content.Category}", LogLevel.Debug);
                                    _cpForageables.Add(itemId, content.Category);
                                }
                            }
                        }

                        if (content?.FruitTrees is not null)
                        {
                            foreach (var treeId in content.FruitTrees)
                            {
                                if (_cpFruitTrees.ContainsKey(treeId))
                                {
                                    Monitor.Log($"Already found a Fruit Tree with Id [{treeId}] with category [{_cpFruitTrees[treeId]}] when trying to add category [{content.Category}]. Please verify you don't have duplicate or conflicting content packs.", LogLevel.Warn);
                                }
                                else
                                {
                                    _cpFruitTrees.Add(treeId, content.Category);
                                }
                            }
                        }

                        if (content?.WildTrees is not null)
                        {
                            foreach (var treeId in content.WildTrees)
                            {
                                if (_cpWildTrees.ContainsKey(treeId))
                                {
                                    Monitor.Log($"Already found a Wild Tree with Id [{treeId}] with category [{_cpWildTrees[treeId]}] when trying to add category [{content.Category}]. Please verify you don't have duplicate or conflicting content packs.", LogLevel.Warn);
                                }
                                else
                                {
                                    _cpWildTrees.Add(treeId, content.Category);
                                }
                            }
                        }
                    }
                    catch
                    {
                        Monitor.Log($"Unable to load content pack: {Path.Combine(pack.DirectoryPath, "content.json")}. Review that file for syntax errors.", LogLevel.Error);
                    }
                }
            }
        }

        private static void ForageItem(SObject obj, Vector2 vec, Random random, int xpGained = 0, bool checkGatherer = false)
        {
            var foragingLevel = Game1.player.ForagingLevel;
            var professions = Game1.player.professions;

            if (professions.Contains(16))
            {
                obj.Quality = 4;
            }
            else if (random.NextDouble() < (double)(foragingLevel / 30f))
            {
                obj.Quality = 2;
            }
            else if (random.NextDouble() < (double)(foragingLevel / 15f))
            {
                obj.Quality = 1;
            }

            vec *= 64.0f;

            Game1.player.gainExperience(2, xpGained);
            Game1.createItemDebris(obj.getOne(), vec, -1, null, -1);

            if (checkGatherer && professions.Contains(13) && random.NextDouble() < 0.2)
            {
                Game1.player.gainExperience(2, xpGained);
                Game1.createItemDebris(obj.getOne(), vec, -1, null, -1);
            }
        }

        private static IEnumerable<Vector2> GetTilesToCheck(Point playerLocation, int radius)
        {
            for (int x = Math.Max(playerLocation.X - radius, 0); x <= playerLocation.X + radius; x++)
                for (int y = Math.Max(playerLocation.Y - radius, 0); y <= playerLocation.Y + radius; y++)
                    yield return new Vector2(x, y);

            yield break;
        }
    }
}
