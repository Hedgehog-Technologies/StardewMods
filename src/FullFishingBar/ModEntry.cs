using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Menus;
using StardewValley.Tools;

namespace FullFishingBar
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
			helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
		}

		private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
		{
			if (!_config.IsEnabled) return;

			if (e.NewMenu is BobberBar)
			{
				var bobberBarMenu = e.NewMenu as BobberBar;

				if (bobberBarMenu is not null)
				{
					if (_config.ExceptBossFish && bobberBarMenu.bossFish) return;
					if (_config.OnlyCorkBobber && !bobberBarMenu.bobbers.Contains("(O)695")) return;

					bobberBarMenu.bobberBarHeight = GetBobberBarSize();
				}
			}
		}

		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			_config.RegisterModConfigMenu(Helper, ModManifest);
		}

		private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
		{
			if (!Context.IsPlayerFree) return;
			if (!_config.AutoHook) return;
			if (Game1.currentLocation is null || Game1.player is null) return;
			if (Game1.player.CurrentTool is null) return;
			if (Game1.player.CurrentTool is not FishingRod rod) return;
			if (rod.hasEnchantmentOfType<AutoHookEnchantment>()) return;
			if (!rod.isFishing || !rod.isNibbling || rod.hit || rod.isReeling || rod.pullingOutOfWater || rod.fishCaught) return;

			rod.timePerBobberBob = 1f;
			rod.timeUntilFishingNibbleDone = FishingRod.maxTimeToNibble;
			rod.DoFunction(Game1.player.currentLocation, (int)rod.bobber.X, (int)rod.bobber.Y, 1, Game1.player);
			Rumble.rumble(0.95f, 200f);
		}

		private int GetBobberBarSize()
		{
			var trackHeight = BobberBar.bobberBarTrackHeight;
			var configSizePercentage = _config.GetBarSizePercentage();
			var barSize = (int)(trackHeight * configSizePercentage);

			return barSize;
		}
	}
}
