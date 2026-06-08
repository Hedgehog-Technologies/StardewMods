using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using AutoForager.Classes;
using HedgeTech.Common.Extensions;

namespace AutoForager.Services
{
	internal class ForagingContext : IForagingContext
	{
		private const int ERROR_MESSAGE_THROTTLE_SECONDS = 10;

		private readonly Dictionary<string, Dictionary<string, int>> _trackingCounts;
		private DateTime _nextErrorMessage;

		public Farmer Player => Game1.player;
		public GameLocation Location => Game1.currentLocation;
		public ModConfig Config { get; }
		public IMonitor Monitor { get; }
		public ForageableItemTracker ForageableTracker { get; }

		public Point PlayerTilePoint => Player.TilePoint;

		public int ForagingRadius
		{
			get
			{
				if (Config.UsePlayerMagnetism)
				{
					return Player.GetAppliedMagneticRadius() / Game1.tileSize;
				}
				return Config.ShakeDistance;
			}
		}

		public ForagingContext(
			ModConfig config,
			IMonitor monitor,
			ForageableItemTracker forageableTracker,
			Dictionary<string, Dictionary<string, int>> trackingCounts)
		{
			Config = config;
			Monitor = monitor;
			ForageableTracker = forageableTracker;
			_trackingCounts = trackingCounts;
			_nextErrorMessage = DateTime.MinValue;
		}

		public void TrackForagedItem(string category, string displayName)
		{
			if (_trackingCounts.TryGetValue(category, out var categoryDict))
			{
				categoryDict.AddOrIncrement(displayName);
			}
		}

		public bool PlayerHasTool<T>() where T : Tool
		{
			return Player.Items.Any(i => i is T);
		}

		public T? GetOrCreateTool<T>(bool requireInInventory) where T : Tool, new()
		{
			var tool = Player.Items.FirstOrDefault(i => i is T, null) as T;

			if (tool == null && !requireInInventory)
			{
				tool = new T();
				Monitor.Log($"Created temporary {typeof(T).Name} tool", Config.DebugLogLevel());
			}

			return tool;
		}

		public void ShowThrottledError(string message)
		{
			if (_nextErrorMessage < DateTime.UtcNow)
			{
				Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type));
				_nextErrorMessage = DateTime.UtcNow.AddSeconds(ERROR_MESSAGE_THROTTLE_SECONDS);
			}
		}
	}
}
