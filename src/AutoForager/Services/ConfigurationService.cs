using System.Collections.Generic;
using System.Threading.Tasks;
using StardewModdingAPI;
using AutoForager.Classes;
using AutoForager.UI;
using HedgeTech.Common.Helpers;

namespace AutoForager.Services
{
	/// <summary>
	/// Manages configuration state, loading, saving, and UI registration.
	/// </summary>
	internal class ConfigurationService
	{
		private readonly IModHelper _helper;
		private readonly IManifest _manifest;
		private readonly IMonitor _monitor;
		private readonly JsonHelper _jsonHelper;

		private ModConfig _config;
		private ConfigMenuBuilder? _menuBuilder;

		public ModConfig Config => _config;

		public ConfigurationService(
			IModHelper helper,
			IManifest manifest,
			IMonitor monitor,
			JsonHelper jsonHelper)
		{
			_helper = helper;
			_manifest = manifest;
			_monitor = monitor;
			_jsonHelper = jsonHelper;
			_config = new ModConfig();
		}

		/// <summary>
		/// Loads configuration from disk and merges with defaults.
		/// </summary>
		public ModConfig LoadConfiguration()
		{
			var savedConfig = _helper.ReadConfig<ModConfig>();
			_config.Merge(savedConfig);
			return _config;
		}

		/// <summary>
		/// Saves the current configuration to disk.
		/// </summary>
		public void SaveConfiguration()
		{
			_helper.WriteConfig(_config);
			_monitor.Log(_jsonHelper.Serialize(_config), LogLevel.Trace);
		}

		/// <summary>
		/// Saves configuration asynchonously.
		/// </summary>
		public async Task SaveConfigurationAsync()
		{
			await Task.Run(SaveConfiguration);
		}

		/// <summary>
		/// Updates configuration utilities with content pack information.
		/// </summary>
		public void UpdateUtilities(IEnumerable<IContentPack> packs)
		{
			_config.UpdateUtilities(packs);
		}

		/// <summary>
		/// Adds FTM categories to the configuration.
		/// </summary>
		public void AddFtmCategories(Dictionary<string, string> ftmCategories)
		{
			_config.AddFtmCategories(ftmCategories);
		}

		/// <summary>
		/// Updates enabled states for all forageables.
		/// </summary>
		public void UpdateEnabled()
		{
			_config.UpdateEnabled(_helper);
		}

		/// <summary>
		/// Registers the configuration menu with Generic Mod Config Menu.
		/// </summary>
		public void RegisterConfigMenu(
			ForageableItemTracker forageableTracker,
			CategoryComparer comparer)
		{
			_menuBuilder = new ConfigMenuBuilder(
				_helper,
				_manifest,
				_config,
				forageableTracker,
				comparer);

			_menuBuilder.RegisterMenu();
		}

		/// <summary>
		/// Toggles auto-foraging on/off.
		/// </summary>
		public async Task<bool> ToggleAutoForagingAsync()
		{
			_config.AutoForagingEnabled = !_config.AutoForagingEnabled;

			var saveTask = SaveConfigurationAsync();
			await saveTask;

			var success = saveTask.Status == TaskStatus.RanToCompletion;
			_monitor.Log(
				success ? "Config saved successfully!" : $"Saving config unsuccessful {saveTask.Status}",
				_config.DebugLogLevel());

			return _config.AutoForagingEnabled;
		}
	}
}
