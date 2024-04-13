using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using AutoForager.Classes;
using AutoForager.Extensions;

using Constants = AutoForager.Helpers.Constants;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AutoForager
{
	public class ModConfig
	{
		private const string _gmcmUniqueId = "spacechase0.GenericModConfigMenu";

		private readonly ForageableItemTracker _forageableTracker;
		private IComparer<string> _comparer = new CategoryComparer();
		private IMonitor? _monitor;
		private IManifest _manifest;
		private IModHelper _helper;

		private IGenericModConfigMenu? _gmcmApi;

		#region General Properties

		public bool IsForagerActive { get; set; }
		public KeybindList ToggleForagerKeybind { get; set; } = new();
		public bool UsePlayerMagnetism { get; set; }
		public int ShakeDistance { get; set; }
		public bool RequireHoe { get; set; }
		public bool RequireToolMoss { get; set; }

		private int _fruitsReadyToShake;
		public int FruitsReadyToShake
		{
			get => _fruitsReadyToShake;
			set => _fruitsReadyToShake = Math.Clamp(value, Constants.MinFruitsReady, Constants.MaxFruitsReady);
		}

		public Dictionary<string, Dictionary<string, bool>> ForageToggles { get; set; }

		private bool _anyBushesEnabled;
		public bool AnyBushEnabled() => _anyBushesEnabled;

		public bool GetTeaBushesEnabled() => ForageToggles[Constants.BushToggleKey][Constants.TeaBushKey];
		private void SetTeaBushesEnabled(bool value) => ForageToggles[Constants.BushToggleKey][Constants.TeaBushKey] = value;

		public bool GetWalnutBushesEnabled() => ForageToggles[Constants.BushToggleKey][Constants.WalnutBushKey];
		private bool SetWalnutBushesEnabled(bool value) => ForageToggles[Constants.BushToggleKey][Constants.WalnutBushKey] = value;

		#endregion

		public ModConfig()
		{
			_forageableTracker = ForageableItemTracker.Instance;

			ForageToggles = new()
			{
				{ Constants.BushToggleKey, new() },
				{ Constants.ForagingToggleKey, new() },
				{ Constants.FruitTreeToggleKey, new() },
				{ Constants.WildTreeToggleKey, new() }
			};

			ResetToDefault();
		}

		public void UpdateUtilities(IMonitor monitor, IEnumerable<IContentPack> packs)
		{
			_monitor = monitor;
			_comparer = new CategoryComparer(packs);
		}

		public void ResetToDefault()
		{
			IsForagerActive = true;
			ToggleForagerKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.H),
				new Keybind(SButton.RightAlt, SButton.H));

			UsePlayerMagnetism = false;
			ShakeDistance = 2;
			RequireHoe = true;
			RequireToolMoss = true;
			FruitsReadyToShake = Constants.MinFruitsReady;

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
				}
			}
		}

		public void RegisterModConfigMenu(IModHelper helper, IManifest manifest)
		{
			if (!helper.ModRegistry.IsLoaded(_gmcmUniqueId)) return;

			_gmcmApi = helper.ModRegistry.GetApi<IGenericModConfigMenu>(_gmcmUniqueId);
			_manifest = manifest;
			_helper = helper;

			if (_gmcmApi is null) return;

			try
			{
				_gmcmApi.Unregister(_manifest);
			}
			catch { }


			_gmcmApi.Register(
				mod: _manifest,
				reset: ResetToDefault,
				save: () => _helper.WriteConfig(this));

			_gmcmApi.OnFieldChanged(_manifest, HandleOnChange);

			/* General */

			_gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_General_Text);

			// IsForagerActive
			_gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.IsForagerActiveId,
				name: I18n.Option_IsForagerActive_Name,
				tooltip: I18n.Option_IsForagerActive_Tooltip,
				getValue: () => IsForagerActive,
				setValue: val => IsForagerActive = val);

			// ToggleForager
			_gmcmApi.AddKeybindList(
				mod: _manifest,
				fieldId: Constants.ToggleForagerId,
				name: I18n.Option_ToggleForager_Name,
				tooltip: I18n.Option_ToggleForager_Tooltip,
				getValue: () => ToggleForagerKeybind,
				setValue: val => ToggleForagerKeybind = val);

			// UsePlayerMagnetism
			_gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.UsePlayerMagnetismId,
				name: I18n.Option_UsePlayerMagnetism_Name,
				tooltip: () => I18n.Option_UsePlayerMagnetism_Tooltip(I18n.Option_ShakeDistance_Name()),
				getValue: () => UsePlayerMagnetism,
				setValue: val => UsePlayerMagnetism = val);

			// ShakeDistance
			_gmcmApi.AddNumberOption(
				mod: _manifest,
				fieldId: Constants.ShakeDistanceId,
				name: I18n.Option_ShakeDistance_Name,
				tooltip: () => I18n.Option_ShakeDistance_Tooltip(I18n.Option_UsePlayerMagnetism_Name()),
				getValue: () => ShakeDistance,
				setValue: val => ShakeDistance = val);

			// RequireHoe
			_gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.RequireHoeId,
				name: () => I18n.Option_RequireHoe_Name(Environment.NewLine),
				tooltip: I18n.Option_RequireHoe_Tooltip,
				getValue: () => RequireHoe,
				setValue: val => RequireHoe = val);

			// RequireToolMoss
			_gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.RequireToolMossId,
				name: () => I18n.Option_RequireToolMoss_Name(Environment.NewLine),
				tooltip: I18n.Option_RequireToolMoss_Tooltip,
				getValue: () => RequireToolMoss,
				setValue: val => RequireToolMoss = val);

			_gmcmApi.AddParagraph(
				mod: _manifest,
				text: () => "");

			/* Page Links Section */

			_gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.BushesPageId,
				text: I18n.Link_Bushes_Text);

			_gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.ForageablesPageId,
				text: I18n.Link_Forageables_Text);

			_gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.FruitTreesPageId,
				text: I18n.Link_FruitTrees_Text);

			_gmcmApi.AddPageLink(
				mod: _manifest,
				pageId: Constants.WildTreesPageId,
				text: I18n.Link_WildTrees_Text);

			/* Wild Trees */

			_gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.WildTreesPageId,
				pageTitle: I18n.Page_WildTrees_Title);

			_gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_WildTree_Text);

			_gmcmApi.AddParagraph(
				mod: _manifest,
				text: I18n.Page_WildTrees_Description);

			foreach (var currentGroup in _forageableTracker.WildTreeForageables.GroupByCategory(_helper, comparer: _comparer))
			{
				_gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					_gmcmApi.AddBoolOption(
						mod: _manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							ForageToggles[Constants.WildTreeToggleKey].AddOrUpdate(item.InternalName, val);
							UpdateEnabled();
						});
				}
			}

			/* Fruit Trees */

			_gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.FruitTreesPageId,
				pageTitle: I18n.Page_FruitTrees_Title);

			// FruitsReadyToShake
			_gmcmApi.AddNumberOption(
				mod: _manifest,
				fieldId: Constants.FruitsReadyToShakeId,
				name: I18n.Option_FruitsReadyToShake_Name,
				tooltip: I18n.Option_FruitsReadyToShake_Tooltip,
				getValue: () => FruitsReadyToShake,
				setValue: val => FruitsReadyToShake = val,
				min: Constants.MinFruitsReady,
				max: Constants.MaxFruitsReady);

			_gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_FruitTrees_Text);

			_gmcmApi.AddParagraph(
				mod: _manifest,
				text: I18n.Page_FruitTrees_Description);

			foreach (var currentGroup in _forageableTracker.FruitTreeForageables.GroupByCategory(_helper, comparer: _comparer))
			{
				_gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					_gmcmApi.AddBoolOption(
						mod: _manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							ForageToggles[Constants.FruitTreeToggleKey].AddOrUpdate(item.InternalName, val);
							UpdateEnabled();
						});
				}
			}

			/* Bushes */

			_gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.BushesPageId,
				pageTitle: I18n.Page_Bushes_Title);

			_gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Section_Bushes_Text);

			_gmcmApi.AddParagraph(
				mod: _manifest,
				text: I18n.Page_Bushes_Description);

			// Bush Blooms
			foreach (var currentGroup in _forageableTracker.BushForageables
				.Where(b => b.CustomFields.ContainsKey(Constants.CustomFieldBushBloomCategory)).ToList()
				.GroupByCategory(_helper, Constants.CustomFieldBushBloomCategory, _comparer))
			{
				_gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					_gmcmApi.AddBoolOption(
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
							ForageToggles[Constants.BushToggleKey].AddOrUpdate(item.InternalName, val);
							UpdateEnabled();
						});
				}
			}

			// Custom Bushes
			foreach (var currentGroup in _forageableTracker.BushForageables
				.Where(b => b.CustomFields.ContainsKey(Constants.CustomFieldCustomBushCategory)).ToList()
				.GroupByCategory(_helper, Constants.CustomFieldCustomBushCategory, _comparer))
			{
				_gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					_gmcmApi.AddBoolOption(
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
							ForageToggles[Constants.BushToggleKey].AddOrUpdate(item.InternalName, val);
							UpdateEnabled();
						});
				}
			}

			_gmcmApi.AddSectionTitle(
				mod: _manifest,
				text: I18n.Category_Vanilla);

			// ShakeTeaBushes
			_gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.ShakeTeaBushesId,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_TeaBushes()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward(
					I18n.Action_Shake_Future().ToLowerInvariant(),
					I18n.Subject_TeaBushes(),
					I18n.Reward_TeaLeaves()),
				getValue: GetTeaBushesEnabled,
				setValue: val =>
				{
					SetTeaBushesEnabled(val);
					UpdateEnabled();
				});

			// ShakeWalnutBushes
			_gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: Constants.ShakeWalnutBushesId,
				name: () => I18n.Option_ToggleAction_Name(I18n.Subject_WalnutBushes()),
				tooltip: () => I18n.Option_ToggleAction_Description_Reward_Note(
					I18n.Action_Shake_Future().ToLowerInvariant(),
					I18n.Subject_WalnutBushes(),
					I18n.Reward_GoldenWalnuts(),
					I18n.Note_ShakeWalnutBushes()),
				getValue: GetWalnutBushesEnabled,
				setValue: val =>
				{
					SetWalnutBushesEnabled(val);
					UpdateEnabled();
				});

			/* Forageables */

			_gmcmApi.AddPage(
				mod: _manifest,
				pageId: Constants.ForageablesPageId,
				pageTitle: I18n.Page_Forageables_Title);

			_gmcmApi.AddParagraph(
				mod: _manifest,
				text: I18n.Page_Forageables_Description);

			_gmcmApi.AddBoolOption(
				mod: _manifest,
				fieldId: "AF_ENABLE_ALL",
				name: () => "Enable All Forage?",
				getValue: GetAllEnabled,
				setValue: SetAllEnabled);

			foreach (var currentGroup in _forageableTracker.ObjectForageables.GroupByCategory(_helper, comparer: _comparer))
			{
				_gmcmApi.AddSectionTitle(
					mod: _manifest,
					text: () => currentGroup.Key);

				foreach (var item in currentGroup)
				{
					_gmcmApi.AddBoolOption(
						mod: _manifest,
						name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
						getValue: () => item.IsEnabled,
						setValue: val =>
						{
							item.IsEnabled = val;
							ForageToggles[Constants.ForagingToggleKey].AddOrUpdate(item.InternalName, val);
							if (_forageableTracker.ArtifactForageables.TryGetItem(item.QualifiedItemId, out var artifact) && artifact is not null)
							{
								artifact.IsEnabled = val;
							}
							UpdateEnabled();
						});
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
						_anyBushesEnabled = toggleDict.Value.Any(b => b.Value);
						//UpdateTrackerEnables(_forageableTracker.BushForageables, toggleDict.Value);
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
				}
			}

			helper?.WriteConfig(this);
		}

		private bool GetAllEnabled()
		{
			foreach (var toggleDict in ForageToggles)
			{
				foreach (var kvp in toggleDict.Value)
				{
					if (!kvp.Value)
					{
						return false;
					}
				}
			}

			return true;
		}

		private void SetAllEnabled(bool enabled)
		{
			foreach (var toggleDict in ForageToggles)
			{
				foreach (var kvp in toggleDict.Value)
				{
					ForageToggles[toggleDict.Key][kvp.Key] = enabled;
					_monitor.Log($"{toggleDict.Key} - {kvp.Key} - {enabled}", LogLevel.Debug);

					// Edit the ForageableTracker values also
				}
			}

			UpdateEnabled();
			//RegisterModConfigMenu(_helper, _manifest);
			//_gmcmApi.OpenModMenu(_manifest);
		}

		private void HandleOnChange(string fieldId, object value)
		{
			if (fieldId.IEquals("AF_ENABLE_ALL"))
			{
				SetAllEnabled((bool)value);
			}
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

			dict.Clear();

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

	public interface IGenericModConfigMenu
	{
		/*********
		** Methods
		*********/

		/// <summary>Register a mod whose config can be edited through the UI.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="reset">Reset the mod's config to its default values.</param>
		/// <param name="save">Save the mod's current config to the <c>config.json</c> file.</param>
		/// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
		/// <remarks>Each mod can only be registered once, unless it's deleted via <see cref="Unregister"/> before calling this again.</remarks>
		void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

		/****
		** Basic options
		****/

		/// <summary>Add a section title at the current position in the form.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="text">The title text shown in the form.</param>
		/// <param name="tooltip">The tooltip text shown when the cursor hovers on the title, or <c>null</c> to disable the tooltip.</param>
		void AddSectionTitle(IManifest mod, Func<string> text, Func<string>? tooltip = null);

		/// <summary>Add a paragraph of text at the current position in the form.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="text">The paragraph text to display.</param>
		void AddParagraph(IManifest mod, Func<string> text);

		/// <summary>Add a boolean option at the current position in the form.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="getValue">Get the current value from the mod config.</param>
		/// <param name="setValue">Set a new value in the mod config.</param>
		/// <param name="name">The label text to show in the form.</param>
		/// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
		/// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
		void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

		/// <summary>Add an integer option at the current position in the form.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="getValue">Get the current value from the mod config.</param>
		/// <param name="setValue">Set a new value in the mod config.</param>
		/// <param name="name">The label text to show in the form.</param>
		/// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
		/// <param name="min">The minimum allowed value, or <c>null</c> to allow any.</param>
		/// <param name="max">The maximum allowed value, or <c>null</c> to allow any.</param>
		/// <param name="interval">The interval of values that can be selected.</param>
		/// <param name="formatValue">Get the display text to show for a value, or <c>null</c> to show the number as-is.</param>
		/// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
		void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string>? tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string>? formatValue = null, string? fieldId = null);


		/// <summary>Add a key binding list at the current position in the form.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="getValue">Get the current value from the mod config.</param>
		/// <param name="setValue">Set a new value in the mod config.</param>
		/// <param name="name">The label text to show in the form.</param>
		/// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
		/// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
		void AddKeybindList(IManifest mod, Func<KeybindList> getValue, Action<KeybindList> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

		/****
		** Multi-page management
		****/

		/// <summary>Start a new page in the mod's config UI, or switch to that page if it already exists. All options registered after this will be part of that page.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="pageId">The unique page ID.</param>
		/// <param name="pageTitle">The page title shown in its UI, or <c>null</c> to show the <paramref name="pageId"/> value.</param>
		/// <remarks>You must also call <see cref="AddPageLink"/> to make the page accessible. This is only needed to set up a multi-page config UI. If you don't call this method, all options will be part of the mod's main config UI instead.</remarks>
		void AddPage(IManifest mod, string pageId, Func<string>? pageTitle = null);

		/// <summary>Add a link to a page added via <see cref="AddPage"/> at the current position in the form.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="pageId">The unique ID of the page to open when the link is clicked.</param>
		/// <param name="text">The link text shown in the form.</param>
		/// <param name="tooltip">The tooltip text shown when the cursor hovers on the link, or <c>null</c> to disable the tooltip.</param>
		void AddPageLink(IManifest mod, string pageId, Func<string> text, Func<string>? tooltip = null);

		/****
		** Advanced
		****/

		/// <summary>Add an option at the current position in the form using custom rendering logic.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="name">The label text to show in the form.</param>
		/// <param name="draw">Draw the option in the config UI. This is called with the sprite batch being rendered and the pixel position at which to start drawing.</param>
		/// <param name="tooltip">The tooltip text shown when the cursor hovers on the field, or <c>null</c> to disable the tooltip.</param>
		/// <param name="beforeMenuOpened">A callback raised just before the menu containing this option is opened.</param>
		/// <param name="beforeSave">A callback raised before the form's current values are saved to the config (i.e. before the <c>save</c> callback passed to <see cref="Register"/>).</param>
		/// <param name="afterSave">A callback raised after the form's current values are saved to the config (i.e. after the <c>save</c> callback passed to <see cref="Register"/>).</param>
		/// <param name="beforeReset">A callback raised before the form is reset to its default values (i.e. before the <c>reset</c> callback passed to <see cref="Register"/>).</param>
		/// <param name="afterReset">A callback raised after the form is reset to its default values (i.e. after the <c>reset</c> callback passed to <see cref="Register"/>).</param>
		/// <param name="beforeMenuClosed">A callback raised just before the menu containing this option is closed.</param>
		/// <param name="height">The pixel height to allocate for the option in the form, or <c>null</c> for a standard input-sized option. This is called and cached each time the form is opened.</param>
		/// <param name="fieldId">The unique field ID for use with <see cref="OnFieldChanged"/>, or <c>null</c> to auto-generate a randomized ID.</param>
		/// <remarks>The custom logic represented by the callback parameters is responsible for managing its own state if needed. For example, you can store state in a static field or use closures to use a state variable.</remarks>
		void AddComplexOption(IManifest mod, Func<string> name, Action<SpriteBatch, Vector2> draw, Func<string> tooltip = null, Action beforeMenuOpened = null, Action beforeSave = null, Action afterSave = null, Action beforeReset = null, Action afterReset = null, Action beforeMenuClosed = null, Func<int> height = null, string fieldId = null);

		/// <summary>Register a method to notify when any option registered by this mod is edited through the config UI.</summary>
		/// <param name="mod">The mod's manifest.</param>
		/// <param name="onChange">The method to call with the option's unique field ID and new value.</param>
		/// <remarks>Options use a randomized ID by default; you'll likely want to specify the <c>fieldId</c> argument when adding options if you use this.</remarks>
		void OnFieldChanged(IManifest mod, Action<string, object> onChange);

		/// <summary>Open the config UI for a specific mod.</summary>
		/// <param name="mod">The mod's manifest.</param>
		void OpenModMenu(IManifest mod);

		/// <summary>Remove a mod from the config UI and delete all its options and pages.</summary>
		/// <param name="mod">The mod's manifest.</param>
		void Unregister(IManifest mod);
	}
}
