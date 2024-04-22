﻿using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using FullFishingBar;
using System;

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
					bobberBarMenu.bobberBarHeight = Math.Min(
						Math.Max(
							(int)(BobberBar.bobberBarTrackHeight * _config.BarSizePercentage),
							(int)(BobberBar.bobberBarTrackHeight * 0.1)),
						BobberBar.bobberBarTrackHeight);
				}
			}
		}

		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			_config.RegisterModConfigMenu(Helper, ModManifest);
		}
	}
}
