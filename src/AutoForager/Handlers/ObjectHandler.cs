using Microsoft.Xna.Framework;
using System;
using StardewModdingAPI;
using StardewValley;
using AutoForager.Extensions;

using Constants = AutoForager.Helpers.Constants;
using SObject = StardewValley.Object;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles foraging spawned Objects (wild items on the ground).
	/// </summary>
	internal class ObjectHandler : BaseForagingHandler
	{
		public override int Priority => 50; // Check objects after terrain features

		/// <summary>
		/// Determines if this handler can process the given Object.
		/// </summary>
		public bool CanHandle(SObject obj)
		{
			if (obj == null) return false;
			if (!obj.IsSpawnedObject) return false;

			// Check if it's an enabled forageable
			if (Context.ForageableTracker.ObjectForageables.TryGetItem(obj.QualifiedItemId, out var objItem)
				&& (objItem?.IsEnabled ?? false))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Handles foraging a spawned Object.
		/// </summary>
		public void Handle(SObject obj, Vector2 tile)
		{
			if (!Context.ForageableTracker.ObjectForageables.TryGetItem(obj.QualifiedItemId, out var objItem))
			{
				return;
			}

			// Forage the item with quality calculation
			ForageItem(obj, tile, Utility.CreateDaySaveRandom(tile.X, tile.Y * 777.0f), 7, true);

			// Remove the object from the location
			if (Context.Location.removeObject(tile, false) == null)
			{
				// Failed to remove object the proper way (likely custom object without canBeGrabbed flag)
				// Force remove to prevent infinite spawns
				Context.Location.objects.Remove(tile);
				LogOnce($"Force removed object [{obj.DisplayName}] at {tile} in {Context.Location.Name} due to failed normal removal.", LogLevel.Info);
			}

			PlaySound(HARVEST_SOUND_ID);

			if (objItem != null)
			{
				TrackItem(Constants.ForageableKey, objItem.DisplayName);
			}
		}

		/// <summary>
		/// Forages an item with quality calculation, experience gain, and gatherer profession check.
		/// </summary>
		private void ForageItem(SObject obj, Vector2 vec, Random random, int xpGained = 0, bool checkGatherer = false)
		{
			var player = Context.Player;
			var foragingLevel = (float)player.ForagingLevel;
			var professions = player.professions;
			var isForage = obj.isForage();
			var skill = obj.isAnimalProduct() ? Farmer.farmingSkill : Farmer.foragingSkill;

			// Determine quality
			if (professions.Contains(Farmer.botanist) && isForage)
			{
				obj.Quality = 4; // Iridium quality for botanist profession
			}
			else if (isForage)
			{
				obj.Quality = DetermineForageQuality(random);
			}

			vec *= 64.0f;

			// Gain experience
			player.gainExperience(skill, xpGained);

			// Create item debris
			Game1.createItemDebris(obj.getOne(), vec, -1, null, -1);

			// Check for Gatherer profession (double harvest chance)
			if (checkGatherer && isForage && professions.Contains(Farmer.gatherer) && random.NextDouble() < 0.2)
			{
				player.gainExperience(Farmer.foragingSkill, xpGained); // Extra XP for double harvest
				Game1.createItemDebris(obj.getOne(), vec, -1, null, -1);
			}
		}
	}
}
