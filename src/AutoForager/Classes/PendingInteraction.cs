using System;
using Microsoft.Xna.Framework;

namespace AutoForager.Classes
{
	/// <summary>
	/// Represents a deferred foraging interaction with distance-based prioritization.
	/// </summary>
	internal class PendingInteraction : IComparable<PendingInteraction>
	{
		/// <summary>
		/// The type of interaction to perform.
		/// </summary>
		public InteractionType Type { get; }

		/// <summary>
		/// The target object to interact with (Tree, FruitTree, Bush, Object, etc.).
		/// </summary>
		public object Target { get; }

		/// <summary>
		/// The tile location of the interaction.
		/// </summary>
		public Vector2 Tile { get; }

		/// <summary>
		/// The distance from the player to this interaction (for prioritization).
		/// </summary>
		public float DistanceToPlayer { get; }

		/// <summary>
		/// Creates a new pending interaction with distance calculation.
		/// </summary>
		/// <param name="type">The type of interaction.</param>
		/// <param name="target">The target object.</param>
		/// <param name="tile">The tile location.</param>
		/// <param name="playerPosition">The player's current tile position.</param>
		public PendingInteraction(InteractionType type, object target, Vector2 tile, Point playerPosition)
		{
			Type = type;
			Target = target;
			Tile = tile;

			// Calculate Manhattan distance for prioritization (cheaper than Euclidean)
			DistanceToPlayer = Math.Abs(tile.X - playerPosition.X) + Math.Abs(tile.Y - playerPosition.Y);
		}

		/// <summary>
		/// Compares interactions by distance for sorting (closest first).
		/// </summary>
		public int CompareTo(PendingInteraction? other)
		{
			if (other is null) return 1;
			return DistanceToPlayer.CompareTo(other.DistanceToPlayer);
		}
	}

	/// <summary>
	/// Defines the types of foraging interactions.
	/// </summary>
	internal enum InteractionType
	{
		WildTree,
		FruitTree,
		Bush,
		LargeBush,
		TerrainFeature,
		Object
	}
}
