
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles Ore Panning Spots.
	/// </summary>
	internal class PanningHandler : BaseForagingHandler
	{
		private const string PANNING_SOUND = "coin";

		public override int Priority => 80; // Check panning last (special case)

		/// <summary>
		/// Checks for and handles panning spots in the current location.
		/// This is a special case that doesn't follow the tile-by-tile pattern.
		/// </summary>
		public void CheckAndHandle()
		{
			if (!Config.ForagePanningSpots) return;
			if (Context.Location == null) return;

			var orePanPoint = Context.Location.orePanPoint?.Value ?? Point.Zero;
			if (orePanPoint.Equals(Point.Zero)) return;

			var orePanVector = orePanPoint.ToVector2();

			// Check if panning spot is within radius
			var playerTile = Context.Player.Tile;
			var distance = Vector2.Distance(playerTile, orePanVector);

			if (distance > Context.ForagingRadius) return;

			HandlePanningSpot(orePanVector, orePanPoint);
		}

		/// <summary>
		/// Handles a panning spot.
		/// </summary>
		private void HandlePanningSpot(Vector2 orePanVector, Point orePanPoint)
		{
			// Get or create pan
			var panItem = Context.GetOrCreateTool<Pan>(Config.RequirePan);
			if (panItem == null) return;

			// Get panned items
			var items = panItem.getPanItems(Context.Location, Context.Player);
			if (items is null || items.Count == 0) return;

			// Calculate direction to player
			var dist = Context.Player.TilePoint - orePanPoint;
			var normal = dist.ToVector2();
			normal.Normalize();

			// Find open tile to spawn items
			var currentVec = orePanVector;
			while (currentVec != Context.Player.Tile)
			{
				currentVec += normal;

				if (Context.Location.isTileLocationOpen(currentVec))
				{
					// Move once more for good measure
					currentVec += normal;

					// Spawn all panned items
					foreach (var item in items)
					{
						if (item is not null)
						{
							CreateItemDebris(item, currentVec);
							TrackItem(Constants.ForageableKey, item.DisplayName);
						}
					}

					// Play sound and clear panning spot
					Context.Location.localSound(PANNING_SOUND, orePanVector / Constants.TileSize);
					Context.Location.orePanPoint!.Value = Point.Zero;

					// Spawn additional panning spots based on pan upgrade level
					SpawnAdditionalPanningSpots(panItem);

					break;
				}
			}
		}

		/// <summary>
		/// Spawns additonal panning spots based on the pan upgrade level.
		/// </summary>
		private void SpawnAdditionalPanningSpots(Pan panItem)
		{
			for (int i = 0; i < panItem.UpgradeLevel - 1; i++)
			{
				if (Context.Location.performOrePanTenMinuteUpdate(Game1.random))
				{
					break;
				}

				// 50% chance for additional spot (except Island North)
				if (Game1.random.NextDouble() < Constants.AdditionalPanningSpotChance
					&& Context.Location.performOrePanTenMinuteUpdate(Game1.random)
					&& Context.Location is not IslandNorth)
				{
					break;
				}
			}
		}
	}
}
