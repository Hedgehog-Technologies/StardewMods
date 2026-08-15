using System;
using StardewModdingAPI;
using HedgeTech.Common.Interfaces;

namespace FullFishingBar
{
	public class ModConfig
	{
		public bool IsEnabled { get; set; }
		public bool OnlyCorkBobber { get; set; }

		private float _barSizePercentage;
		public int BarSizePercentageInt
		{
			get => (int)Math.Clamp(_barSizePercentage * 100, 10.0, 100.0);
			set => _barSizePercentage = Math.Clamp(value / 100.0F, 0.1F, 1.0F);
		}

		public bool ExceptBossFish { get; set; }
		public bool AutoHook { get; set; }

		public ModConfig()
		{
			ResetToDefault();
		}

		public void ResetToDefault()
		{
			IsEnabled = true;
			OnlyCorkBobber = false;
			_barSizePercentage = 1.0F;
			ExceptBossFish = false;
			AutoHook = false;
		}

		public void RegisterModConfigMenu(IModHelper helper, IManifest manifest)
		{
			if (!helper.ModRegistry.IsLoaded(IGenericModConfigMenu.UniqueId)) return;

			var gmcmApi = helper.ModRegistry.GetApi<IGenericModConfigMenu>(IGenericModConfigMenu.UniqueId);
			if (gmcmApi is null) return;

			try
			{
				gmcmApi.Unregister(manifest);
			}
			catch { }

			gmcmApi.Register(
				mod: manifest,
				reset: ResetToDefault,
				save: () => helper.WriteConfig(this));

			gmcmApi.AddSectionTitle(
				mod: manifest,
				text: I18n.Section_General);

			gmcmApi.AddBoolOption(
				mod: manifest,
				name: I18n.Option_Enabled_Name,
				tooltip: I18n.Option_Enabled_Tooltip,
				getValue: () => IsEnabled,
				setValue: (val) => IsEnabled = val);

			gmcmApi.AddSectionTitle(
				mod: manifest,
				text: I18n.Section_Customizations);

			gmcmApi.AddBoolOption(
				mod: manifest,
				name: I18n.Option_OnlyCorkBobber_Name,
				tooltip: I18n.Option_OnlyCorkBobber_Tooltip,
				getValue: () => OnlyCorkBobber,
				setValue: (val) => OnlyCorkBobber = val);

			gmcmApi.AddNumberOption(
				mod: manifest,
				name: I18n.Option_BarSizePercentage_Name,
				tooltip: I18n.Option_BarSizePercentage_Tooltip,
				getValue: () => BarSizePercentageInt,
				setValue: (val) => BarSizePercentageInt = val,
				min: 10,
				max: 100,
				interval: 1,
				formatValue: (val) => $"{val}%");

			gmcmApi.AddBoolOption(
				mod: manifest,
				name: I18n.Option_ExceptBossFish_Name,
				tooltip: I18n.Option_ExceptBossFish_Tooltip,
				getValue: () => ExceptBossFish,
				setValue: (val) => ExceptBossFish = val);

			gmcmApi.AddBoolOption(
				mod: manifest,
				name: I18n.Option_AutoHook_Name,
				tooltip: I18n.Option_AutoHook_Tooltip,
				getValue: () => AutoHook,
				setValue: (val) => AutoHook = val);
		}

		public float GetBarSizePercentage() => Math.Clamp(_barSizePercentage, 0.1F, 1.0F);
	}
}
