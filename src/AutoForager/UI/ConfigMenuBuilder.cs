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
	internal class ConfigMenuBuilder(
		IModHelper helper,
		IManifest manifest,
		ModConfig config,
		ForageableItemTracker forageableTracker,
		CategoryComparer comparer)
	{
		private const string WFR_UNIQUE_ID = "jpp.WildFlowersReimagined";

		/// <summary>
		/// Registers the mod configuration menu with GMCM.
		/// </summary>
		public void RegisterMenu()
		{
			if (!helper.ModRegistry.IsLoaded(IGenericModConfigMenu.UniqueId)) return;

			var gmcmApi = helper.ModRegistry.GetApi<IGenericModConfigMenu>(IGenericModConfigMenu.UniqueId);
			if (gmcmApi is null) return;

			// Unregister if already registered
			try
			{
				gmcmApi.Unregister(manifest);
			}
			catch { }

			// Register the mod
			gmcmApi.Register(
				mod: manifest,
				reset: config.ResetToDefault,
				save: () => helper.WriteConfig(config));

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
				mod: manifest,
				text: I18n.Section_General_Text);

			// AutoForagingEnabled
			gmcmApi.AddBoolOption(
				mod: manifest,
				fieldId: Constants.AutoForagingEnabledId,
				name: I18n.Option_AutoForagingEnabled_Name,
				tooltip: I18n.Option_AutoForagingEnabled_Tooltip,
				getValue: () => config.AutoForagingEnabled,
				setValue: val => config.AutoForagingEnabled = val);

			// ToggleForager
			gmcmApi.AddKeybindList(
				mod: manifest,
				fieldId: Constants.ToggleForagerId,
				name: I18n.Option_ToggleForager_Name,
				tooltip: I18n.Option_ToggleForager_Tooltip,
				getValue: () => config.ToggleForagerKeybind,
				setValue: val => config.ToggleForagerKeybind = val);

			// UsePlayerMagnetism
			gmcmApi.AddBoolOption(
				mod: manifest,
				fieldId: Constants.UsePlayerMagnetismId,
				name: I18n.Option_UsePlayerMagnetism_Name,
				tooltip: () => I18n.Option_UsePlayerMagnetism_Tooltip(I18n.Option_ShakeDistance_Name()),
				getValue: () => config.UsePlayerMagnetism,
				setValue: val => config.UsePlayerMagnetism = val);

			// ShakeDistance
			gmcmApi.AddNumberOption(
				mod: manifest,
				fieldId: Constants.ShakeDistanceId,
				name: I18n.Option_ShakeDistance_Name,
				tooltip: () => I18n.Option_ShakeDistance_Tooltip(I18n.Option_UsePlayerMagnetism_Name()),
				getValue: () => config.ShakeDistance,
				setValue: val => config.ShakeDistance = val);

			// MaxInteractionsPerMove
			gmcmApi.AddNumberOption(
				mod: manifest,
				fieldId: Constants.MaxInteractionsPerMoveId,
				name: I18n.Option_MaxInteractionsPerMove_Name,
				tooltip: I18n.Option_MaxInteractionsPerMove_Tooltip,
				getValue: () => config.MaxInteractionsPerMove ?? 0,
				setValue: val => config.MaxInteractionsPerMove = val == 0 ? null : val,
				min: 0,
				max: Constants.AbsoluteMaxInteractions);

			// RequireHoe
			gmcmApi.AddBoolOption(
				mod: manifest,
				fieldId: Constants.RequireHoeId,
				name: () => I18n.Option_RequireHoe_Name(Environment.NewLine),
				tooltip: I18n.Option_RequireHoe_Tooltip,
				getValue: () => config.RequireHoe,
				setValue: val => config.RequireHoe = val);

			// RequireToolMoss
			gmcmApi.AddBoolOption(
				mod: manifest,
				fieldId: Constants.RequireToolMossId,
				name: () => I18n.Option_RequireToolMoss_Name(Environment.NewLine),
				tooltip: I18n.Option_RequireToolMoss_Tooltip,
				getValue: () => config.RequireToolMoss,
				setValue: val => config.RequireToolMoss = val);

			// RequirePan
			gmcmApi.AddBoolOption(
				mod: manifest,
				fieldId: Constants.RequirePanId,
				name: I18n.Option_RequirePan_Name,
				tooltip: I18n.Option_RequirePan_Tooltip,
				getValue: () => config.RequirePan,
				setValue: val => config.RequirePan = val);

			// IgnoreMushroomLogTrees
			gmcmApi.AddBoolOption(
				mod: manifest,
				fieldId: Constants.IgnoreMushroomLogTreesId,
				name: () => I18n.Option_IgnoreMushroomLogTrees_Name(Environment.NewLine),
				tooltip: I18n.Option_IgnoreMushroomLogTrees_Tooltip,
				getValue: () => config.IgnoreMushroomLogTrees,
				setValue: val => config.IgnoreMushroomLogTrees = val);
		}

		/// <summary>
		/// Builds the page links section.
		/// </summary>
		private void BuildPageLinks(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddSectionTitle(
				mod: manifest,
				text: I18n.Section_TogglePages_Text);

			gmcmApi.AddPageLink(
				mod: manifest,
				pageId: Constants.BushesPageId,
				text: I18n.Link_Bushes_Text);

			gmcmApi.AddPageLink(
				mod: manifest,
				pageId: Constants.ForageablesPageId,
				text: I18n.Link_Forageables_Text);

			gmcmApi.AddPageLink(
				mod: manifest,
				pageId: Constants.FruitTreesPageId,
				text: I18n.Link_FruitTrees_Text);

			gmcmApi.AddPageLink(
				mod: manifest,
				pageId: Constants.WildTreesPageId,
				text: I18n.Link_WildTrees_Text);

			// Only load Wild Flower config page if the mod is present, skip otherwise
			if (helper.ModRegistry.IsLoaded(WFR_UNIQUE_ID))
			{
				gmcmApi.AddPageLink(
					mod: manifest,
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
				mod: manifest,
				text: I18n.Section_Advanced_Text);

			gmcmApi.AddBoolOption(
				mod: manifest,
				name: I18n.Option_ElevateDebugLogs_Name,
				tooltip: I18n.Option_ElevateDebugLogs_Tooltip,
				getValue: () => config.ElevateDebugLogs,
				setValue: val => config.ElevateDebugLogs = val);
		}

		/// <summary>
		/// Builds the wild trees configuration page.
		/// </summary>
		private void BuildWildTreesPage(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddPage(
				mod: manifest,
				pageId: Constants.WildTreesPageId,
				pageTitle: I18n.Page_WildTrees_Title);

			gmcmApi.AddSectionTitle(
				mod: manifest,
				text: I18n.Section_WildTree_Text);

			gmcmApi.AddParagraph(
				mod: manifest,
				text: I18n.Page_WildTrees_Description);

			foreach (var currentGroup in forageableTracker.WildTreeForageables.GroupByCategory(helper, comparer: comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => $"{item.ItemId} - {item.InternalName}",
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							config.ForageToggles[Constants.WildTreeToggleKey].AddOrUpdate(item.InternalName, val);
							config.UpdateEnabled();
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
				mod: manifest,
				pageId: Constants.FruitTreesPageId,
				pageTitle: I18n.Page_FruitTrees_Title);

			// FruitsReadyToShake
			gmcmApi.AddNumberOption(
				mod: manifest,
				fieldId: Constants.FruitsReadyToShakeId,
				name: I18n.Option_FruitsReadyToShake_Name,
				tooltip: I18n.Option_FruitsReadyToShake_Tooltip,
				getValue: () => config.FruitsReadyToShake,
				setValue: val => config.FruitsReadyToShake = val,
				min: Constants.MinFruitsReady,
				max: Constants.MaxFruitsReady);

			gmcmApi.AddSectionTitle(
				mod: manifest,
				text: I18n.Section_FruitTrees_Text);

			gmcmApi.AddParagraph(
				mod: manifest,
				text: I18n.Page_FruitTrees_Description);

			foreach (var currentGroup in forageableTracker.FruitTreeForageables.GroupByCategory(helper, comparer: comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => $"{item.ItemId} - {item.InternalName}",
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							config.ForageToggles[Constants.FruitTreeToggleKey].AddOrUpdate(item.InternalName, val);
							config.UpdateEnabled();
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
				mod: manifest,
				pageId: Constants.BushesPageId,
				pageTitle: I18n.Page_Bushes_Title);

			gmcmApi.AddSectionTitle(
				mod: manifest,
				text: I18n.Section_Bushes_Text);

			gmcmApi.AddParagraph(
				mod: manifest,
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
			foreach (var currentGroup in forageableTracker.BushForageables
				.Where(b => b.CustomFields.ContainsKey(Constants.CustomFieldBushBloomCategory)).ToList()
				.GroupByCategory(helper, Constants.CustomFieldBushBloomCategory, comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => I18n.Option_ToggleAction_Description_Reward(
							I18n.Action_Shake_Future().ToLowerInvariant(),
							I18n.Subject_Bushes(),
							item.DisplayName),
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							config.ForageToggles[Constants.BushToggleKey].AddOrUpdate(item.InternalName, val);
							config.UpdateEnabled();
						});
				}
			}
		}

		/// <summary>
		/// Builds the custom bush section.
		/// </summary>
		private void BuildCustomBushSection(IGenericModConfigMenu gmcmApi)
		{
			foreach (var currentGroup in forageableTracker.BushForageables
				.Where(b => b.CustomFields.ContainsKey(Constants.CustomFieldCustomBushCategory)).ToList()
				.GroupByCategory(helper, Constants.CustomFieldCustomBushCategory, comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => I18n.Option_ToggleAction_Description_Reward(
							I18n.Action_Shake_Future().ToLowerInvariant(),
							I18n.Subject_Bushes(),
							item.DisplayName),
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							config.ForageToggles[Constants.BushToggleKey].AddOrUpdate(item.InternalName, val);
							config.UpdateEnabled();
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
				mod: manifest,
				text: I18n.Category_Vanilla);

			// ShakeTeaBushes
			gmcmApi.AddBoolOption(
				mod: manifest,
				fieldId: Constants.ShakeTeaBushesId,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_TeaBushes()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Shake_Future().ToLowerInvariant(),
					I18n.Subject_TeaBushes(),
					I18n.Reward_TeaLeaves()),
				getValue: config.GetTeaBushesEnabled,
				setValue: val =>
				{
					config.SetTeaBushesEnabled(val);
					config.UpdateEnabled();
				});

			// ShakeWalnutBushes
			gmcmApi.AddBoolOption(
				mod: manifest,
				fieldId: Constants.ShakeWalnutBushesId,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_WalnutBushes()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward_Note(
					I18n.Action_Shake_Future().ToLowerInvariant(),
					I18n.Subject_WalnutBushes(),
					I18n.Reward_GoldenWalnuts(),
					I18n.Note_ShakeWalnutBushes()),
				getValue: config.GetWalnutBushesEnabled,
				setValue: val =>
				{
					config.SetWalnutBushesEnabled(val);
					config.UpdateEnabled();
				});
		}

		/// <summary>
		/// Builds the forageables configuration page.
		/// </summary>
		private void BuildForageablesPage(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddPage(
				mod: manifest,
				pageId: Constants.ForageablesPageId,
				pageTitle: I18n.Page_Forageables_Title);

			// Special forageable types
			BuildSpecialForageableOptions(gmcmApi);

			gmcmApi.AddParagraph(
				mod: manifest,
				text: I18n.Page_Forageables_Description);

			// Regular forageables
			foreach (var currentGroup in forageableTracker.ObjectForageables.GroupByCategory(helper, comparer: comparer))
			{
				gmcmApi.AddSectionTitle(
					mod: manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					gmcmApi.AddBoolOption(
						mod: manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						tooltip: () => $"{item.ItemId} - {item.InternalName}",
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							config.ForageToggles[Constants.ForagingToggleKey].AddOrUpdate(item.InternalName, val);
							config.UpdateEnabled();
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
				mod: manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_ArtifactSpot()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Dig_Future().ToLowerInvariant(),
					I18n.Subject_ArtifactSpot(),
					I18n.Reward_Buried_Items()),
				getValue: () => config.ForageArtifactSpots,
				setValue: val => config.ForageArtifactSpots = val);

			// Seed Spots
			gmcmApi.AddBoolOption(
				mod: manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_SeedSpot()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Dig_Future().ToLowerInvariant(),
					I18n.Subject_SeedSpot(),
					I18n.Reward_Buried_Seeds()),
				getValue: () => config.ForageSeedSpots,
				setValue: val => config.ForageSeedSpots = val);

			// Mushroom Boxes
			gmcmApi.AddBoolOption(
				mod: manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_MushroomBoxes()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Forage_Future().ToLowerInvariant(),
					I18n.Subject_MushroomBoxes(),
					I18n.Reward_Mushrooms()),
				getValue: () => config.ForageMushroomBoxes,
				setValue: val => config.ForageMushroomBoxes = val);

			// Mushroom Logs
			gmcmApi.AddBoolOption(
				mod: manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_MushroomLogs()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Forage_Future().ToLowerInvariant(),
					I18n.Subject_MushroomLogs(),
					I18n.Reward_Mushrooms()),
				getValue: () => config.ForageMushroomLogs,
				setValue: val => config.ForageMushroomLogs = val);

			// Tappers
			gmcmApi.AddBoolOption(
				mod: manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_Tappers()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Forage_Future().ToLowerInvariant(),
					I18n.Subject_Tappers(),
					I18n.Reward_TappedTree()),
				getValue: () => config.ForageTappers,
				setValue: val => config.ForageTappers = val);

			// Panning Spots
			gmcmApi.AddBoolOption(
				mod: manifest,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_PanningSpots()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Sift_Future().ToLowerInvariant(),
					I18n.Subject_PanningSpots(),
					I18n.Reward_Ores()),
				getValue: () => config.ForagePanningSpots,
				setValue: val => config.ForagePanningSpots = val);
		}

		/// <summary>
		/// Builds the WildFlowersReimagined Flower section
		/// </summary>
		private void BuildWildFlowersReimaginedPage(IGenericModConfigMenu gmcmApi)
		{
			gmcmApi.AddPage(
				mod: manifest,
				pageId: Constants.WildFlowersReimaginedPageId,
				pageTitle: I18n.Page_WildFlowersReimagined_Title);

			var paragraphText = forageableTracker.FlowerForageables.Count > 0 ? I18n.Page_WildFlowersReimagined_Description() : I18n.Page_WildFlowersReimagined_EarlyFallbackDescription();

			gmcmApi.AddParagraph(
				mod: manifest,
				text: () => paragraphText);

			foreach (var item in forageableTracker.FlowerForageables)
			{
				gmcmApi.AddBoolOption(
					mod: manifest,
					name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
					tooltip: () => $"{item. ItemId} - {item.InternalName}",
					getValue: () => item.IsEnabled,
					setValue: val => 
					{
						item.IsEnabled = val;
						config.ForageToggles[Constants.FlowerGrassToggleKey].AddOrUpdate(item.InternalName, val);
						config.UpdateEnabled();
					});
			}
		}
	}
}
