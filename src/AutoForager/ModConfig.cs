using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using AutoForager.Classes;
using HedgeTech.Common.Extensions;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager
{
	public class ModConfig
	{
		private readonly ForageableItemTracker _forageableTracker;
		private CategoryComparer _comparer = new();

		#region General Properties

		public bool AutoForagingEnabled { get; set; }
		public KeybindList ToggleForagerKeybind { get; set; } = new();
		public bool UsePlayerMagnetism { get; set; }
		public int ShakeDistance { get; set; }
		public bool RequireHoe { get; set; }
		public bool RequireToolMoss { get; set; }
		public bool RequirePan { get; set; }
		public bool IgnoreMushroomLogTrees { get; set; }
		public bool ElevateDebugLogs { get; set; }

		private int _fruitsReadyToShake;
		public int FruitsReadyToShake
		{
			get => _fruitsReadyToShake;
			set => _fruitsReadyToShake = Math.Clamp(value, Constants.MinFruitsReady, Constants.MaxFruitsReady);
		}

		public bool ForageArtifactSpots { get; set; }
		public bool ForageSeedSpots { get; set; }
		public bool ForageMushroomBoxes { get; set; }
		public bool ForageMushroomLogs { get; set; }
		public bool ForageTappers { get; set; }
		public bool ForagePanningSpots { get; set; }

		public Dictionary<string, Dictionary<string, bool>> ForageToggles { get; set; }

		private bool _anyBushesEnabled;
		public bool AnyBushEnabled() => _anyBushesEnabled;

		public bool GetTeaBushesEnabled() => ForageToggles[Constants.BushToggleKey][Constants.TeaBushKey];
		public void SetTeaBushesEnabled(bool value) => ForageToggles[Constants.BushToggleKey][Constants.TeaBushKey] = value;

		public bool GetWalnutBushesEnabled() => ForageToggles[Constants.BushToggleKey][Constants.WalnutBushKey];
		public void SetWalnutBushesEnabled(bool value) => ForageToggles[Constants.BushToggleKey][Constants.WalnutBushKey] = value;

		#endregion General Properties

		public ModConfig()
		{
			_forageableTracker = ForageableItemTracker.Instance;

			ForageToggles = new()
			{
				{ Constants.BushToggleKey, new() },
				{ Constants.ForagingToggleKey, new() },
				{ Constants.FruitTreeToggleKey, new() },
				{ Constants.WildTreeToggleKey, new() },
				{ Constants.FlowerGrassToggleKey, new() }
			};

			ResetToDefault();
		}

		public void UpdateUtilities(IEnumerable<IContentPack> packs)
		{
			_comparer = new CategoryComparer(packs);
		}

		public void AddFtmCategories(Dictionary<string, string> ftmCategories)
		{
			_comparer.AddFtmCategories(ftmCategories);
		}

		public void ResetToDefault()
		{
			ToggleForagerKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.H),
				new Keybind(SButton.RightAlt, SButton.H));

			AutoForagingEnabled = true;
			UsePlayerMagnetism = false;
			ShakeDistance = 2;
			RequireHoe = true;
			RequireToolMoss = true;
			RequirePan = true;
			IgnoreMushroomLogTrees = true;
			ElevateDebugLogs = false;
			FruitsReadyToShake = Constants.MinFruitsReady;

			ForageArtifactSpots = true;
			ForageSeedSpots = true;
			ForageMushroomBoxes = true;
			ForageMushroomLogs = true;
			ForageTappers = true;
			ForagePanningSpots = true;

			foreach (var toggleDict in ForageToggles)
			{
				if (_forageableTracker is not null || toggleDict.Key == Constants.BushToggleKey)
				{
					if (toggleDict.Key.Equals(Constants.BushToggleKey))
					{
						toggleDict.Value[Constants.TeaBushKey] = true;
						toggleDict.Value[Constants.WalnutBushKey] = false;
					}
					else if (toggleDict.Key.Equals(Constants.ForagingToggleKey))
					{
						ResetTracker(_forageableTracker?.ObjectForageables, toggleDict.Value);
					}
					else if (toggleDict.Key.Equals(Constants.FruitTreeToggleKey))
					{
						ResetTracker(_forageableTracker?.FruitTreeForageables, toggleDict.Value);
					}
					else if (toggleDict.Key.Equals(Constants.WildTreeToggleKey))
					{
						ResetTracker(_forageableTracker?.WildTreeForageables, toggleDict.Value);
					}
					else if (toggleDict.Key.Equals(Constants.FlowerGrassToggleKey))
					{
						ResetTracker(_forageableTracker?.FlowersForageables, toggleDict.Value);
					}
				}
			}
		}

		public void Merge(ModConfig b)
		{
			this.AutoForagingEnabled = b.AutoForagingEnabled;
			this.ToggleForagerKeybind = b.ToggleForagerKeybind;
			this.UsePlayerMagnetism = b.UsePlayerMagnetism;
			this.ShakeDistance = b.ShakeDistance;
			this.RequireHoe = b.RequireHoe;
			this.RequireToolMoss = b.RequireToolMoss;
			this.RequirePan = b.RequirePan;
			this.IgnoreMushroomLogTrees = b.IgnoreMushroomLogTrees;
			this.ElevateDebugLogs = b.ElevateDebugLogs;
			this.FruitsReadyToShake = b.FruitsReadyToShake;
			this.ForageArtifactSpots = b.ForageArtifactSpots;
			this.ForageSeedSpots = b.ForageSeedSpots;
			this.ForageMushroomBoxes = b.ForageMushroomBoxes;
			this.ForageMushroomLogs = b.ForageMushroomLogs;
			this.ForageTappers = b.ForageTappers;
			this.ForagePanningSpots = b.ForagePanningSpots;

			foreach (var kvp in b.ForageToggles)
			{
				if (!this.ForageToggles.ContainsKey(kvp.Key))
				{
					this.ForageToggles[kvp.Key] = new Dictionary<string, bool>(kvp.Value);
				}
				else
				{
					foreach (var innerKvp in kvp.Value)
					{
						this.ForageToggles[kvp.Key][innerKvp.Key] = innerKvp.Value;
					}
				}
			}
		}

		public void UpdateEnabled(IModHelper? helper = null)
		{
			if (_forageableTracker is not null)
			{
				foreach (var toggleDict in ForageToggles)
				{
					if (toggleDict.Key.Equals(Constants.BushToggleKey)
						&& toggleDict.Value.Keys.Any())
					{
						_anyBushesEnabled = toggleDict.Value.Any(b => b.Value)
							|| toggleDict.Value[Constants.TeaBushKey]
							|| toggleDict.Value[Constants.WalnutBushKey];
					}
					else if (toggleDict.Key.Equals(Constants.ForagingToggleKey))
					{
						UpdateTrackerEnables(_forageableTracker.ObjectForageables, toggleDict.Value);
					}
					else if (toggleDict.Key.Equals(Constants.FruitTreeToggleKey))
					{
						UpdateTrackerEnables(_forageableTracker.FruitTreeForageables, toggleDict.Value);
					}
					else if (toggleDict.Key.Equals(Constants.WildTreeToggleKey))
					{
						UpdateTrackerEnables(_forageableTracker.WildTreeForageables, toggleDict.Value);
					}
					else if (toggleDict.Key.Equals(Constants.FlowerGrassToggleKey))
					{
						UpdateTrackerEnables(_forageableTracker.FlowersForageables, toggleDict.Value);
					}
				}
			}

			helper?.WriteConfig(this);
		}

		public LogLevel DebugLogLevel()
		{
			return ElevateDebugLogs
				? LogLevel.Debug
				: LogLevel.Trace;
		}

		private static void UpdateTrackerEnables(List<ForageableItem> items, Dictionary<string, bool> dict)
		{
			if (items.Count == 0) return;

			foreach (var toggle in dict)
			{
				var item = items.FirstOrDefault(f => f?.InternalName.Equals(toggle.Key) ?? false, null);

				if (item is not null)
				{
					item.IsEnabled = toggle.Value;
				}
			}

			foreach (var item in items)
			{
				dict.AddOrUpdate(item.InternalName, item.IsEnabled);
			}
		}

		private static void ResetTracker(List<ForageableItem>? items, Dictionary<string, bool> dict)
		{
			if (items.IsNullOrEmpty()) return;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
			foreach (var item in items)
			{
				item.ResetToDefaultEnabled();
				dict.Add(item.InternalName, item.IsEnabled);
			}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
		}
	}
}
