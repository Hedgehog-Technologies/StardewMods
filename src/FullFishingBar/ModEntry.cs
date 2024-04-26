using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using FullFishingBar;

namespace AutoForager
{
	/// <summary>
	/// The mod entry point.
	/// </summary>
	public class ModEntry : Mod
	{
		private ModConfig _config = new();

		public override void Entry(IModHelper helper)
		{
			I18n.Init(helper.Translation);

			_config = helper.ReadConfig<ModConfig>();

			helper.Events.Display.MenuChanged += OnMenuChanged;
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
		}

		private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
		{
			if (!_config.IsEnabled || _config.ExceptBossFish) return;

			if (e.NewMenu is BobberBar)
			{
				var bobberBarMenu = e.NewMenu as BobberBar;

				if (bobberBarMenu is not null)
				{
					if (_config.OnlyCorkBobber && !bobberBarMenu.bobbers.Contains("(O)695")) return;

					bobberBarMenu.bobberBarHeight = GetBobberBarSize();
				}
			}
		}

		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			_config.RegisterModConfigMenu(Helper, ModManifest);
		}

		private int GetBobberBarSize()
		{
			var trackHeight = BobberBar.bobberTrackHeight;
			var configSizePercentage = _config.GetBarSizePercentage();
			var barSize = (int)(trackHeight * configSizePercentage);

			return barSize;
		}
	}
}
