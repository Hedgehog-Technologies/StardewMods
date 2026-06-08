using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using AutoForager.Extensions;
using HedgeTech.Common.Extensions;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles foraging from Wild Trees, including seeds and moss.
	/// </summary>
	internal class WildTreeHandler : BaseForagingHandler
	{
		private readonly List<Tree> _mushroomLogTrees;
		private readonly object _mushroomLogTreesLock;

		public WildTreeHandler(List<Tree> mushroomLogTrees, object mushroomLogTreesLock)
		{
			_mushroomLogTrees = mushroomLogTrees;
			_mushroomLogTreesLock = mushroomLogTreesLock;
		}

		public override int Priority => 10; // Check Wild Trees before other terrain features

		/// <summary>
		/// Determines if this handler can process the given tree.
		/// </summary>
		public bool CanHandle(Tree tree)
		{
			if (tree == null) return false;
			if (tree.stump.Value) return false;
			if (tree.growthStage.Value < 5) return false;

			// Must have seeds, moss, or shake items to be harvestable
			var hasHarvestable = tree.hasSeed.Value
				|| tree.hasMoss.Value
				|| (tree.GetData().ShakeItems?.Any() ?? false);

			return hasHarvestable;
		}

		/// <summary>
		/// Handles foraging from a Wild Tree.
		/// </summary>
		public void Handle(Tree tree)
		{
			// Handle tree shaking for seeds
			if (ShouldShakeTree(tree))
			{
				ShakeTree(tree);
			}

			// Handle moss harvesting
			if (ShouldHarvestMoss(tree))
			{
				HarvestMoss(tree);
			}
		}

		/// <summary>
		/// Determines if the tree should be shaken for seeds and/or items.
		/// </summary>
		private bool ShouldShakeTree(Tree tree)
		{
			if (tree.wasShakenToday.Value) return false;
			if (!Game1.IsMultiplayer && Game1.player.ForagingLevel < 1) return false;
			if (!tree.isActionable()) return false;

			// Check if tree has seeds that are enabled in the config
			var seedItemIds = tree.GetSeedAndSeedItemIds();
			if (tree.hasSeed.Value
				&& Context.ForageableTracker.WildTreeForageables.Any(i =>
					(seedItemIds.Contains(i.QualifiedItemId) || seedItemIds.Contains(i.ItemId))
					&& i.IsEnabled))
			{
				return true;
			}

			// Check if tree has shake items that are enabled in the config
			var shakeItems = tree.GetData().ShakeItems;
			if (shakeItems?.Any(s =>
				Context.ForageableTracker.WildTreeForageables.Any(i =>
					(s.ItemId.Contains(i.QualifiedItemId) || s.ItemId.Contains(i.ItemId))
					&& i.IsEnabled)) ?? false)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Shakes the tree to collect seeds or items.
		/// </summary>
		private void ShakeTree(Tree tree)
		{
			tree.performUseAction(tree.Tile);

			var seedItemIds = tree.GetSeedAndSeedItemIds();
			LogDebug($"Tree shaken: Seeds: {string.Join(", ", seedItemIds)}; Items: {string.Join(", ", tree.GetData().ShakeItems?.Select(s => s.ItemId) ?? [])}");

			// Track all seed items
			foreach (var id in seedItemIds)
			{
				var name = id;

				if (Context.ForageableTracker.WildTreeForageables.TryGetItem(id, out var wtItem))
				{
					name = wtItem?.DisplayName ?? id;
				}

				TrackItem(Constants.WildTreeKey, name);
			}
		}

		/// <summary>
		/// Determines if moss should be harvested from the given tree.
		/// </summary>
		private bool ShouldHarvestMoss(Tree tree)
		{
			if (!tree.hasMoss.Value) return false;

			// Check if moss is enabled
			if (!Context.ForageableTracker.ObjectForageables.TryGetItem("(O)Moss", out var mossItem)
				|| !(mossItem?.IsEnabled ?? false))
			{
				return false;
			}

			// Check if tree is a mushroom log tree and should be ignored
			if (Config.IgnoreMushroomLogTrees && IsMushroomLogTree(tree))
			{
				return false;
			}

			// Check tool requirement
			if (Config.RequireToolMoss && !Context.PlayerHasTool<Tool>())
			{
				Context.ShowThrottledError(I18n.Message_MissingToolMoss());
				LogOnce(I18n.Log_MissingToolMoss(I18n.Option_RequireToolMoss_Name(" ")), LogLevel.Info);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Harvests moss from the given tree.
		/// </summary>
		private void HarvestMoss(Tree tree)
		{
			Tool tool = new GenericTool { lastUser = Game1.player };

			tree.performToolAction(tool, -1, tree.Tile);

			if (Context.ForageableTracker.ObjectForageables.TryGetItem("(O)Moss", out var mossItem)
				&& mossItem != null)
			{
				TrackItem(Constants.ForageableKey, mossItem.DisplayName);
			}
		}

		/// <summary>
		/// Checks if the tree is associated with a mushroom log.
		/// </summary>
		private bool IsMushroomLogTree(Tree tree)
		{
			lock (_mushroomLogTreesLock)
			{
				return _mushroomLogTrees.Contains(tree)
					|| _mushroomLogTrees.Any(t => t.Tile.Equals(tree.Tile));
			}
		}
	}
}
