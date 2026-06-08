using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using AutoForager.Classes;

namespace AutoForager.Services
{
	/// <summary>
	/// Provides shared context and utilities for all foraging handlers.
	/// </summary>
	internal interface IForagingContext
	{
		/// <summary>
		/// Gets the current player instance.
		/// </summary>
		Farmer Player { get; }

		/// <summary>
		/// Gets the current game location.
		/// </summary>
		GameLocation Location { get; }

		/// <summary>
		/// Gets the mod configuration.
		/// </summary>
		ModConfig Config { get; }

		/// <summary>
		/// Gets the monitor for logging.
		/// </summary>
		IMonitor Monitor { get; }

		/// <summary>
		/// Gets the forageable item tracker.
		/// </summary>
		ForageableItemTracker ForageableTracker { get; }

		/// <summary>
		/// Gets the player's tile position.
		/// </summary>
		Point PlayerTilePoint { get; }

		/// <summary>
		/// Gets the foraging radius based on config.
		/// </summary>
		int ForagingRadius { get; }

		/// <summary>
		/// Adds tracking count for a foraged item.
		/// </summary>
		/// <param name="category">The category key (bushes forageables, fruit trees, wild trees)</param>
		/// <param name="displayName">The display name of the item.</param>
		void TrackForagedItem(string category, string displayName);

		/// <summary>
		/// Checks if a tool of the specified type exists in the player's inventory.
		/// </summary>
		/// <typeparam name="T">The tool type to check for.</typeparam>
		/// <returns>True if the player has the tool.</returns>
		bool PlayerHasTool<T>() where T : Tool;

		/// <summary>
		/// Gets a tool from the player's inventory, or creates a temporary one if allowed.
		/// </summary>
		/// <typeparam name="T">The tool type to get.</typeparam>
		/// <param name="requireInInventory">Whether the tool must be in inventory.</param>
		/// <returns>The tool instance, or null if not found and required.</returns>
		T? GetOrCreateTool<T>(bool requireInInventory) where T : Tool, new();

		/// <summary>
		/// Shows an error message to the player (throttled to prevent spam).
		/// </summary>
		/// <param name="message">The message to display.</param>
		void ShowThrottledError(string message);
	}
}
