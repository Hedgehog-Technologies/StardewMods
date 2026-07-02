using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AutoForager.Services
{
	/// <summary>
	/// Tracks tiles and objects that have been interacted with today to prevent duplicate processing.
	/// Resets daily to allow re-foraging after day transition.
	/// </summary>
	internal class DailyInteractionTracker
	{
		private readonly HashSet<Vector2> _processedTiles;
		private readonly HashSet<int> _processedObjectHashes;

		public DailyInteractionTracker()
		{
			_processedTiles = [];
			_processedObjectHashes = [];
		}

		/// <summary>
		/// Checks if the specified tile was already processed today.
		/// </summary>
		/// <param name="tile">The tile location to check.</param>
		/// <returns>True if already processed, false otherwise.</returns>
		public bool WasProcessedToday(Vector2 tile)
		{
			return _processedTiles.Contains(tile);
		}

		/// <summary>
		/// Checks if the specified object was already processed today.
		/// Uses object hash code for tracking.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		/// <returns>True if already processed, false otherwise.</returns>
		public bool WasProcessedToday(object obj)
		{
			if (obj is null) return false;
			return _processedObjectHashes.Contains(obj.GetHashCode());
		}

		/// <summary>
		/// Marks the specified tile as processed today.
		/// </summary>
		/// <param name="tile">The tile location to mark.</param>
		public void MarkProcessed(Vector2 tile)
		{
			_processedTiles.Add(tile);
		}

		/// <summary>
		/// Marks the specified object as processed today.
		/// Uses object hash code for tracking.
		/// </summary>
		/// <param name="obj">The object to mark.</param>
		public void MarkProcessed(object obj)
		{
			if (obj is not null)
			{
				_processedObjectHashes.Add(obj.GetHashCode());
			}
		}

		/// <summary>
		/// Resets all tracking data. Should be called at the start of each new day.
		/// </summary>
		public void Reset()
		{
			_processedTiles.Clear();
			_processedObjectHashes.Clear();
		}

		/// <summary>
		/// Gets the total count of processed items (tiles + objects) for debugging.
		/// </summary>
		/// <returns>The total number of tracked items.</returns>
		public int GetProcessedCount()
		{
			return _processedTiles.Count + _processedObjectHashes.Count;
		}
	}
}
