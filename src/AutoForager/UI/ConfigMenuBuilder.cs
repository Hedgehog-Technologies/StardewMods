using System;
using System.Linq;
using StardewModdingAPI;
using AutoForager.Classes;
using AutoForager.Extensions;
using HedgeTech.Common.Extensions;
using HedgeTech.Common.Interfaces;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.UI
{
	/// <summary>
	/// Builds the Generic Mod Config Menu UI for AutoForager.
	/// </summary>
	internal class ConfigMenuBuilder
	{
		private const string GMCM_UNIQUE_ID = "spacechase0.GenericModConfigMenu";
		private const string WFR_UNIQUE_ID = "jpp.WildFlowersReimagined";

		private readonly IModHelper _helper;
		private readonly IManifest _manifest;
		private readonly ModConfig _config;
		private readonly ForageableItemTracker _forageableTracker;
		private readonly CategoryComparer _comparer;

		public ConfigMenuBuilder(
			IModHelper helper,
			IManifest manifest,
			ModConfig config,
			ForageableItemTracker forageableTracker,
			CategoryComparer comparer)
		{
			_helper = helper;
			_manifest = manifest;
			_config = config;
			_forageableTracker = forageableTracker;
			_comparer = comparer;
		}

		/// <summary>
		/// Registers the mod configuration menu with GMCM.
		/// </summary>
		public void RegisterMenu()
		{
			if (!_helper.ModRegistry.IsLoaded(GMCM_UNIQUE_ID)) return;

			var gmcmApi = _helper.ModRegistry.GetApi<IGenericModConfigMenu>(GMCM_UNIQUE_ID);
			if (gmcmApi is null) return;

			// Unregister if already registered
			try
			{
				gmcmApi.Unregister(_manifest);
			}
			catch { }

			// Register the mod
			gmcmApi.Register(
				mod: _manifest,
				reset: _config.ResetToDefault,
				save: () => _helper.WriteConfig(_config));

			// Build menu sections
			BuildGeneralSection(gmcmApi);
			BuildPageLinks(gmcmApi);
			BuildAdvancedSection(gmcmApi);
			BuildWildTreesPage(gmcmApi);
			BuildFruitTreesPage(gmcmApi);
			BuildBushesPage(gmcmApi);
			BuildForageablesPage(gmcmApi);
			BuildWildFlowersReimaginedPage(gmcmApi);
		}


		/// <summary>
		/// Builds the general settings section.
		/// </summary>
		private void BuildGeneralSection(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_General_Text);

			// AutoForagingEnabled
			gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.AutoForagingEnabledId,
				name: I18n.Option_AutoForagingEnabled_Name,
				tooltip: I18n.Option_AutoForagingEnabled_Tooltip,
				getValue: () => _config.AutoForagingEnabled,
				setValue: val => _config.AutoForagingEnabled = val);

			// ToggleForager
			gmcmApi.AddKeybindList(
				mod: _manifest,
				fieldId: Constants.ToggleForagerId,
				name: I18n.Option_ToggleForager_Name,
				tooltip: I18n.Option_ToggleForager_Tooltip,
				getValue: () => _config.ToggleForagerKeybind,
				setValue: val => _config.ToggleForagerKeybind = val);

			// UsePlayerMagnetism
			gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.UsePlayerMagnetismId,
				name: I18n.Option_UsePlayerMagnetism_Name,
				tooltip: () => I18n.Option_UsePlayerMagnetism_Tooltip(I18n.Option_ShakeDistance_Name()),
				getValue: () => _config.UsePlayerMagnetism,
				setValue: val => _config.UsePlayerMagnetism = val);

			// ShakeDistance
			gmcmApi.AddNumberOption(
				mod: _manifest,
				fieldId: Constants.ShakeDistanceId,
				name: I18n.Option_ShakeDistance_Name,
				tooltip: () => I18n.Option_ShakeDistance_Tooltip(I18n.Option_UsePlayerMagnetism_Name()),
				getValue: () => _config.ShakeDistance,
				setValue: val => _config.ShakeDistance = val);

			// RequireHoe
			gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.RequireHoeId,
				name: () => I18n.Option_RequireHoe_Name(Environment.NewLine),
				tooltip: I18n.Option_RequireHoe_Tooltip,
				getValue: () => _config.RequireHoe,
				setValue: val => _config.RequireHoe = val);

			// RequireToolMoss
			gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.RequireToolMossId,
				name: () => I18n.Option_RequireToolMoss_Name(Environment.NewLine),
				tooltip: I18n.Option_RequireToolMoss_Tooltip,
				getValue: () => _config.RequireToolMoss,
				setValue: val => _config.RequireToolMoss = val);

			// RequirePan
			gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.RequirePanId,
				name: I18n.Option_RequirePan_Name,
				tooltip: I18n.Option_RequirePan_Tooltip,
				getValue: () => _config.RequirePan,
				setValue: val => _config.RequirePan = val);

			// IgnoreMushroomLogTrees
			gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.IgnoreMushroomLogTreesId,
				name: () => I18n.Option_IgnoreMushroomLogTrees_Name(Environment.NewLine),
				tooltip: I18n.Option_IgnoreMushroomLogTrees_Tooltip,
				getValue: () => _config.IgnoreMushroomLogTrees,
				setValue: val => _config.IgnoreMushroomLogTrees = val);
		}

		/// <summary>
		/// Builds the page links section.
		/// </summary>
		private void BuildPageLinks(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_TogglePages_Text);

			gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.BushesPageId,
				text: I18n.Link_Bushes_Text);

			gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.ForageablesPageId,
				text: I18n.Link_Forageables_Text);

			gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.FruitTreesPageId,
				text: I18n.Link_FruitTrees_Text);

			gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.WildTreesPageId,
				text: I18n.Link_WildTrees_Text);

			// Only load Wild Flower config page is the mod is present, skip otherwise
			if (_helper.ModRegistry.IsLoaded(WFR_UNIQUE_ID))
			{
				gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.WildFlowersReimaginedPageId,
				text: I18n.Link_WildFlowersReimagine_Text);
			}
		}

		/// <summary>
		/// Builds the advanced settings section.
		/// </summary>
		private void BuildAdvancedSection(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_Advanced_Text);

			gmcmApi.AddBoolOption(
				mod: _manifest,
				name: I18n.Option_ElevateDebugLogs_Name,
				tooltip: I18n.Option_ElevateDebugLogs_Tooltip,
				getValue: () => _config.ElevateDebugLogs,
				setValue: val => _config.ElevateDebugLogs = val);
		}

		/// <summary>
		/// Builds the wild trees configuration page.
		/// </summary>
		private void BuildWildTreesPage(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.WildTreesPageId,
				pageTitle: I18n.Page_WildTrees_Title);

			gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_WildTree_Text);

			gmcmApi.AddParagraph(
				mod: _manifest,
				text: I18n.Page_WildTrees_Description);

			foreach (var currentGroup in _forageableTracker.WildTreeForageables.GroupByCategory(_helper, comparer: _comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: _manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => $"{item.ItemId} - {item.InternalName}",
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							_config.ForageToggles[Constants.WildTreeToggleKey].AddOrUpdate(item.InternalName, val);
							_config.UpdateEnabled();
						});
				}
			}
		}

		/// <summary>
		/// Builds the fruit trees configuration page.
		/// </summary>
		private void BuildFruitTreesPage(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.FruitTreesPageId,
				pageTitle: I18n.Page_FruitTrees_Title);

			// FruitsReadyToShake
			gmcmApi.AddNumberOption(
				mod: _manifest,
				fieldId: Constants.FruitsReadyToShakeId,
				name: I18n.Option_FruitsReadyToShake_Name,
				tooltip: I18n.Option_FruitsReadyToShake_Tooltip,
				getValue: () => _config.FruitsReadyToShake,
				setValue: val => _config.FruitsReadyToShake = val,
				min: Constants.MinFruitsReady,
				max: Constants.MaxFruitsReady);

			gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_FruitTrees_Text);

			gmcmApi.AddParagraph(
				mod: _manifest,
				text: I18n.Page_FruitTrees_Description);

			foreach (var currentGroup in _forageableTracker.FruitTreeForageables.GroupByCategory(_helper, comparer: _comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: _manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => $"{item.ItemId} - {item.InternalName}",
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							_config.ForageToggles[Constants.FruitTreeToggleKey].AddOrUpdate(item.InternalName, val);
							_config.UpdateEnabled();
						});
				}
			}
		}

		/// <summary>
		/// Builds the bushes configuration page.
		/// </summary>
		private void BuildBushesPage(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.BushesPageId,
				pageTitle: I18n.Page_Bushes_Title);

			gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_Bushes_Text);

			gmcmApi.AddParagraph(
				mod: _manifest,
				text: I18n.Page_Bushes_Description);

			// Bush Blooms
			BuildBushBloomSection(gmcmApi);

			// Custom Bushes
			BuildCustomBushSection(gmcmApi);

			// Vanilla Bushes
			BuildVanillaBushSection(gmcmApi);
		}

		/// <summary>
		/// Builds the bush bloom section.
		/// </summary>
		private void BuildBushBloomSection(IGenericModConfigMenu gmcmApi)
		{
			foreach (var currentGroup in _forageableTracker.BushForageables
				.Where(b => b.CustomFields.ContainsKey(Constants.CustomFieldBushBloomCategory)).ToList()
				.GroupByCategory(_helper, Constants.CustomFieldBushBloomCategory, _comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: _manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => I18n.Option_ToggleAction_Description_Reward(
							I18n.Action_Shake_Future().ToLowerInvariant(),
							I18n.Subject_Bushes(),
							item.DisplayName),
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							_config.ForageToggles[Constants.BushToggleKey].AddOrUpdate(item.InternalName, val);
							_config.UpdateEnabled();
						});
				}
			}
		}

		/// <summary>
		/// Builds the custom bush section.
		/// </summary>
		private void BuildCustomBushSection(IGenericModConfigMenu gmcmApi)
		{
			foreach (var currentGroup in _forageableTracker.BushForageables
				.Where(b => b.CustomFields.ContainsKey(Constants.CustomFieldCustomBushCategory)).ToList()
				.GroupByCategory(_helper, Constants.CustomFieldCustomBushCategory, _comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: _manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => I18n.Option_ToggleAction_Description_Reward(
							I18n.Action_Shake_Future().ToLowerInvariant(),
							I18n.Subject_Bushes(),
							item.DisplayName),
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							_config.ForageToggles[Constants.BushToggleKey].AddOrUpdate(item.InternalName, val);
							_config.UpdateEnabled();
						});
				}
			}
		}

		/// <summary>
		/// Builds the vanilla bush section.
		/// </summary>
		private void BuildVanillaBushSection(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Category_Vanilla);

			// ShakeTeaBushes
			gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.ShakeTeaBushesId,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_TeaBushes()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Shake_Future().ToLowerInvariant(),
					I18n.Subject_TeaBushes(),
					I18n.Reward_TeaLeaves()),
				getValue: _config.GetTeaBushesEnabled,
				setValue: val =>
				{
					_config.SetTeaBushesEnabled(val);
					_config.UpdateEnabled();
				});

			// ShakeWalnutBushes
			gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.ShakeWalnutBushesId,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_WalnutBushes()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward_Note(
					I18n.Action_Shake_Future().ToLowerInvariant(),
					I18n.Subject_WalnutBushes(),
					I18n.Reward_GoldenWalnuts(),
					I18n.Note_ShakeWalnutBushes()),
				getValue: _config.GetWalnutBushesEnabled,
				setValue: val =>
				{
					_config.SetWalnutBushesEnabled(val);
					_config.UpdateEnabled();
				});
		}

		/// <summary>
		/// Builds the forageables configuration page.
		/// </summary>
		private void BuildForageablesPage(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.ForageablesPageId,
				pageTitle: I18n.Page_Forageables_Title);

			// Special forageable types
			BuildSpecialForageableOptions(gmcmApi);

			gmcmApi.AddParagraph(
				mod: _manifest,
				text: I18n.Page_Forageables_Description);

			// Regular forageables
			foreach (var currentGroup in _forageableTracker.ObjectForageables.GroupByCategory(_helper, comparer: _comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: _manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => $"{item.ItemId} - {item.InternalName}",
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							_config.ForageToggles[Constants.ForagingToggleKey].AddOrUpdate(item.InternalName, val);
							_config.UpdateEnabled();
						});
				}
			}
		}

		/// <summary>
		/// Builds the special forageable options (artifact spots, mushroom boxes, etc.).
		/// </summary>
		private void BuildSpecialForageableOptions(IGenericModConfigMenu gmcmApi)
		{
			// Artifact Spots
			gmcmApi.AddBoolOption(
				mod: _manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_ArtifactSpot()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Dig_Future().ToLowerInvariant(),
					I18n.Subject_ArtifactSpot(),
					I18n.Reward_Buried_Items()),
				getValue: () => _config.ForageArtifactSpots,
				setValue: val => _config.ForageArtifactSpots = val);

			// Seed Spots
			gmcmApi.AddBoolOption(
				mod: _manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_SeedSpot()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Dig_Future().ToLowerInvariant(),
					I18n.Subject_SeedSpot(),
					I18n.Reward_Buried_Seeds()),
				getValue: () => _config.ForageSeedSpots,
				setValue: val => _config.ForageSeedSpots = val);

			// Mushroom Boxes
			gmcmApi.AddBoolOption(
				mod: _manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_MushroomBoxes()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Forage_Future().ToLowerInvariant(),
					I18n.Subject_MushroomBoxes(),
					I18n.Reward_Mushrooms()),
				getValue: () => _config.ForageMushroomBoxes,
				setValue: val => _config.ForageMushroomBoxes = val);

			// Mushroom Logs
			gmcmApi.AddBoolOption(
				mod: _manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_MushroomLogs()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Forage_Future().ToLowerInvariant(),
					I18n.Subject_MushroomLogs(),
					I18n.Reward_Mushrooms()),
				getValue: () => _config.ForageMushroomLogs,
				setValue: val => _config.ForageMushroomLogs = val);

			// Tappers
			gmcmApi.AddBoolOption(
				mod: _manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_Tappers()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Forage_Future().ToLowerInvariant(),
					I18n.Subject_Tappers(),
					I18n.Reward_TappedTree()),
				getValue: () => _config.ForageTappers,
				setValue: val => _config.ForageTappers = val);

			// Panning Spots
			gmcmApi.AddBoolOption(
				mod: _manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_PanningSpots()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Sift_Future().ToLowerInvariant(),
					I18n.Subject_PanningSpots(),
					I18n.Reward_Ores()),
				getValue: () => _config.ForagePanningSpots,
				setValue: val => _config.ForagePanningSpots = val);
		}
		private void BuildWildFlowersReimaginedPage(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.WildFlowersReimaginedPageId,
				pageTitle: I18n.Page_WildFlowersReimagined_Title);

			var paragraphText = _forageableTracker.FlowersForageables.Count > 0 ? I18n.Page_WildFlowersReimagined_Description() : I18n.Page_WildFlowersReimagined_EarlyFallbackDescription();

			gmcmApi.AddParagraph(
				mod: _manifest,
				text: () => paragraphText);

			foreach (var item in _forageableTracker.FlowersForageables)
			{

				gmcmApi.AddBoolOption(
					mod: _manifest,
					name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
					tooltip: () => $"{item. ItemId} - {item.InternalName}",
					getValue: () => item.IsEnabled,
					setValue: val => 
					{
						item.IsEnabled = val;
						_config.ForageToggles[Constants.FlowerGrassToggleKey].AddOrUpdate(item.InternalName, val);
						_config.UpdateEnabled();
					});
			}
		}
	}
}
