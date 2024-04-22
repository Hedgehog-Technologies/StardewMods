using FullFishingBar;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

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
			_config = helper.ReadConfig<ModConfig>();

			helper.Events.Display.MenuChanged += OnMenuChanged;
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
		}

		private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
		{
			if (!_config.IsEnabled) return;

			if (e.NewMenu is BobberBar)
			{
				var bb = e.NewMenu as BobberBar;

				if (bb is not null)
				{
					bb.bobberBarHeight = BobberBar.bobberBarTrackHeight;
				}
			}
		}

		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			_config.RegisterModConfigMenu(Helper, ModManifest);
		}
	}
}
