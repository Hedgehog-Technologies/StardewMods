﻿using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using AutoForager.Classes;
using AutoForager.Helpers;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager
{
    internal class ModConfig
    {
        private readonly ForageableItemTracker _forageableTracker;
        private IMonitor? _monitor;

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

        public bool GetSalmonberryBushesEnabled() => ForageToggles[Constants.BushToggleKey][Constants.SalmonBerryBushKey];
        private void SetSalmonberryBushesEnabled(bool value) => ForageToggles[Constants.BushToggleKey][Constants.SalmonBerryBushKey] = value;

        public bool GetBlackberryBushesEnabled() => ForageToggles[Constants.BushToggleKey][Constants.BlackberryBushKey];
        private void SetBlackberryBushesEnabled(bool value) => ForageToggles[Constants.BushToggleKey][Constants.BlackberryBushKey] = value;

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

        public void UpdateMonitor(IMonitor monitor)
        {
            _monitor = monitor;
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
                        toggleDict.Value[Constants.SalmonBerryBushKey] = true;
                        toggleDict.Value[Constants.BlackberryBushKey] = true;
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
            if (!helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu")) return;

            var gmcmApi = helper.ModRegistry.GetApi<IGenericModConfigMenu>("spacechase0.GenericModConfigMenu");
            if (gmcmApi is null) return;

            try
            {
                gmcmApi.Unregister(manifest);
            }
            catch { }

            gmcmApi.Register(manifest, ResetToDefault, () => helper.WriteConfig(this));

            /* General */

            gmcmApi.AddSectionTitle(
                mod: manifest,
                text: I18n.Section_General_Text);

            // IsForagerActive
            gmcmApi.AddBoolOption(
                mod: manifest,
                fieldId: Constants.IsForagerActiveId,
                name: I18n.Option_IsForagerActive_Name,
                tooltip: I18n.Option_IsForagerActive_Tooltip,
                getValue: () => IsForagerActive,
                setValue: val => IsForagerActive = val);

            // ToggleForager
            gmcmApi.AddKeybindList(
                mod: manifest,
                fieldId: Constants.ToggleForagerId,
                name: I18n.Option_ToggleForager_Name,
                tooltip: I18n.Option_ToggleForager_Tooltip,
                getValue: () => ToggleForagerKeybind,
                setValue: val => ToggleForagerKeybind = val);

            // UsePlayerMagnetism
            gmcmApi.AddBoolOption(
                mod: manifest,
                fieldId: Constants.UsePlayerMagnetismId,
                name: I18n.Option_UsePlayerMagnetism_Name,
                tooltip: () => I18n.Option_UsePlayerMagnetism_Tooltip(I18n.Option_ShakeDistance_Name()),
                getValue: () => UsePlayerMagnetism,
                setValue: val => UsePlayerMagnetism = val);

            // ShakeDistance
            gmcmApi.AddNumberOption(
                mod: manifest,
                fieldId: Constants.ShakeDistanceId,
                name: I18n.Option_ShakeDistance_Name,
                tooltip: () => I18n.Option_ShakeDistance_Tooltip(I18n.Option_UsePlayerMagnetism_Name()),
                getValue: () => ShakeDistance,
                setValue: val => ShakeDistance = val);

            // RequireHoe
            gmcmApi.AddBoolOption(
                mod: manifest,
                fieldId: Constants.RequireHoeId,
                name: () => I18n.Option_RequireHoe_Name(Environment.NewLine),
                tooltip: I18n.Option_RequireHoe_Tooltip,
                getValue: () => RequireHoe,
                setValue: val => RequireHoe = val);

            // RequireToolMoss
            gmcmApi.AddBoolOption(
                mod: manifest,
                fieldId: Constants.RequireToolMossId,
                name: () => I18n.Option_RequireToolMoss_Name(Environment.NewLine),
                tooltip: I18n.Option_RequireToolMoss_Tooltip,
                getValue: () => RequireToolMoss,
                setValue: val => RequireToolMoss = val);

            /* Page Links Section */

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

            /* Wild Trees */

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

            foreach (var item in _forageableTracker.WildTreeForageables)
            {
                gmcmApi.AddBoolOption(
                    mod: manifest,
                    name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
                    getValue: () => item.IsEnabled,
                    setValue: val =>
                    {
                        item.IsEnabled = val;
                        ForageToggles[Constants.WildTreeToggleKey].AddOrUpdate(item.InternalName, val);
                        UpdateEnabled();
                    });
            }

            /* Fruit Trees */

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
                getValue: () => FruitsReadyToShake,
                setValue: val => FruitsReadyToShake = val,
                min: Constants.MinFruitsReady,
                max: Constants.MaxFruitsReady);

            gmcmApi.AddSectionTitle(
                mod: manifest,
                text: I18n.Section_FruitTrees_Text);

            gmcmApi.AddParagraph(
                mod: manifest,
                text: I18n.Page_FruitTrees_Description);

            foreach (var item in _forageableTracker.FruitTreeForageables)
            {
                gmcmApi.AddBoolOption(
                    mod: manifest,
                    name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
                    getValue: () => item.IsEnabled,
                    setValue: val =>
                    {
                        item.IsEnabled = val;
                        ForageToggles[Constants.FruitTreeToggleKey].AddOrUpdate(item.InternalName, val);
                        UpdateEnabled();
                    });
            }

            /* Bushes */

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

            // ShakeSalmonberries
            gmcmApi.AddBoolOption(
                mod: manifest,
                fieldId: Constants.ShakeSalmonberriesId,
                name: () => I18n.Option_ToggleAction_Name(I18n.Subject_SalmonberryBushes()),
                tooltip: () => I18n.Option_ToggleAction_Description_Reward(
                    I18n.Action_Shake_Future().ToLowerInvariant(),
                    I18n.Subject_SalmonberryBushes(),
                    I18n.Reward_Salmonberries()),
                getValue: GetSalmonberryBushesEnabled,
                setValue: val =>
                {
                    SetSalmonberryBushesEnabled(val);
                    UpdateEnabled();
                });

            // ShakeBlackberries
            gmcmApi.AddBoolOption(
                mod: manifest,
                fieldId: Constants.ShakeBlackberriesId,
                name: () => I18n.Option_ToggleAction_Name(I18n.Subject_BlackberryBushes()),
                tooltip: () => I18n.Option_ToggleAction_Description_Reward(
                    I18n.Action_Shake_Future().ToLowerInvariant(),
                    I18n.Subject_BlackberryBushes(),
                    I18n.Reward_Blackberries()),
                getValue: GetBlackberryBushesEnabled,
                setValue: val =>
                {
                    SetBlackberryBushesEnabled(val);
                    UpdateEnabled();
                });

            // ShakeTeaBushes
            gmcmApi.AddBoolOption(
                mod: manifest,
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
            gmcmApi.AddBoolOption(
                mod: manifest,
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

            gmcmApi.AddPage(
                mod: manifest,
                pageId: Constants.ForageablesPageId,
                pageTitle: I18n.Page_Forageables_Title);

            gmcmApi.AddParagraph(
                mod: manifest,
                text: I18n.Page_Forageables_Description);

            var groupedItems = _forageableTracker.ObjectForageables
                .GroupBy(f =>
                {
                    string? category = null;

                    if (!(f.CustomFields?.TryGetValue(Constants.CustomFieldCategoryKey, out category) ?? false))
                    {
                        foreach (var prefix in Constants.KnownModPrefixes)
                        {
                            if (f.ItemId.StartsWith(prefix.Key))
                            {
                                category = prefix.Value;
                                break;
                            }
                        }

                        category ??= "Other";
                    }

                    return category;
                })
                .OrderBy(g => g.Key, new CategoryComparer());

            foreach (var currentGroup in groupedItems)
            {
                gmcmApi.AddSectionTitle(
                    mod: manifest,
                    text: () => currentGroup.Key);

                foreach (var item in currentGroup)
                {
                    gmcmApi.AddBoolOption(
                        mod: manifest,
                        name: () => I18n.Option_ToggleAction_Name(item.DisplayName),
                        getValue: () => item.IsEnabled,
                        setValue: val =>
                        {
                            item.IsEnabled = val;
                            ForageToggles[Constants.ForagingToggleKey].AddOrUpdate(item.InternalName, val);
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
                        _anyBushesEnabled = toggleDict.Value[Constants.SalmonBerryBushKey]
                            || toggleDict.Value[Constants.BlackberryBushKey]
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
                }
            }

            helper?.WriteConfig(this);
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

        /// <summary>Register a method to notify when any option registered by this mod is edited through the config UI.</summary>
        /// <param name="mod">The mod's manifest.</param>
        /// <param name="onChange">The method to call with the option's unique field ID and new value.</param>
        /// <remarks>Options use a randomized ID by default; you'll likely want to specify the <c>fieldId</c> argument when adding options if you use this.</remarks>
        void OnFieldChanged(IManifest mod, Action<string, object> onChange);

        /// <summary>Get the currently-displayed mod config menu, if any.</summary>
        /// <param name="mod">The manifest of the mod whose config menu is being shown, or <c>null</c> if not applicable.</param>
        /// <param name="page">The page ID being shown for the current config menu, or <c>null</c> if not applicable. This may be <c>null</c> even if a mod config menu is shown (e.g. because the mod doesn't have pages).</param>
        /// <returns>Returns whether a mod config menu is being shown.</returns>
        bool TryGetCurrentMenu(out IManifest mod, out string page);

        /// <summary>Remove a mod from the config UI and delete all its options and pages.</summary>
        /// <param name="mod">The mod's manifest.</param>
        void Unregister(IManifest mod);
    }
}
