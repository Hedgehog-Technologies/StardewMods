using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using AutoForager.Services;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Abstract base class for all foraging handlers.
	/// Provides common functionality and defines the handler contract.
	/// </summary>
	internal abstract class BaseForagingHandler
	{
		protected const string HARVEST_SOUND_ID = "harvest";

		protected IForagingContext Context { get; private set; }
		protected IMonitor Monitor => Context.Monitor;
		protected ModConfig Config => Context.Config;

		/// <summary>
		/// Initializes the handler with the foraging context.
		/// Called once when the handler is registered.
		/// </summary>
		/// <param name="context"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public virtual void Initialize(IForagingContext context)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
		}

		/// <summary>
		/// Gets the priority of this handler. Lower values are checked first.
		/// Default is 100. Override to change execution order.
		/// </summary>
		public virtual int Priority => 100;

		/// <summary>
		/// Logs a message at the configured debug level.
		/// </summary>
		protected void LogDebug(string message)
		{
			Monitor.Log(message, Config.DebugLogLevel());
		}

		/// <summary>
		/// Logs a message once (prevents log spam).
		/// </summary>
		protected void LogOnce(string message, LogLevel level = LogLevel.Info)
		{
			Monitor.LogOnce(message, level);
		}

		/// <summary>
		/// Tracks a foraged item in the statistics.
		/// </summary>
		protected void TrackItem(string category, string displayName)
		{
			Context.TrackForagedItem(category, displayName);
		}

		/// <summary>
		/// Creates item debris at the specified location.
		/// </summary>
		protected void CreateItemDebris(Item item, Vector2 tile, int direction = -1)
		{
			var position = tile * 64.0f;
			Game1.createItemDebris(item, position, direction, null, -1);
		}

		/// <summary>
		/// Plays a sound effect at the player's location.
		/// </summary>
		protected void PlaySound(string soundName)
		{
			Game1.playSound(soundName);
		}

		/// <summary>
		/// Determines the quality of a foraged item based on foraging level and professions.
		/// </summary>
		/// <param name="random"></param>
		/// <returns></returns>
		protected int DetermineForageQuality(Random random)
		{
			var player = Context.Player;
			var foragingLevel = (float)player.ForagingLevel;

			// Botanist profession (always iridium quality)
			if (player.professions.Contains(Farmer.botanist))
			{
				return 4;
			}

			// Gold quality (foraging level / 30 chance)
			if (random.NextDouble() < (double)(foragingLevel / 30.0f))
			{
				return 2;
			}

			// Silver quality (foraging level / 15 chance)
			if (random.NextDouble() < (double)(foragingLevel / 15.0f))
			{
				return 1;
			}

			return 0; // Normal quality
		}
	}
}
