using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using HedgeTech.Common.Extensions;

using Constants = AutoForager.Helpers.Constants;
using SObject = StardewValley.Object;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles harvesting from Machines (mushroom boxes, mushroom logs, tappers).
	/// </summary>
	internal class MachineHandler : BaseForagingHandler
	{
		private const string MUSHROOM_BOX_QUALIFIED_ID = "(BC)128";
		private const string MUSHROOM_LOG_QUALIFIED_ID = "(BC)MushroomLog";
		private const string TAPPER_CONTEXT_TAG = "tapper_item";

		public override int Priority => 70; // Check after Artifact / Seed spots

		/// <summary>
		/// Determines if this handler can process the given Object.
		/// </summary>
		public bool CanHandle(SObject obj)
		{
			if (obj == null) return false;
			if (!obj.bigCraftable.Value) return false;
			if (!obj.readyForHarvest.Value) return false;
			if (obj.heldObject.Value is null) return false;

			var isMushroomBox = Config.ForageMushroomBoxes && obj.QualifiedItemId.IEquals(MUSHROOM_BOX_QUALIFIED_ID);
			var isMushroomLog = Config.ForageMushroomLogs && obj.QualifiedItemId.IEquals(MUSHROOM_LOG_QUALIFIED_ID);
			var isTapper = Config.ForageTappers && obj.HasContextTag(TAPPER_CONTEXT_TAG);

			return isMushroomBox || isMushroomLog || isTapper;
		}

		/// <summary>
		/// Handles harvesting from a Machine.
		/// </summary>
		public void Handle(SObject obj, Vector2 tile, TerrainFeature? terrainFeature)
		{
			var heldObj = obj.heldObject.Value;

			// Fix quality and stack size
			heldObj.FixQuality();
			heldObj.FixStackSize();

			// Create item debris
			CreateItemDebris(heldObj, tile);

			// Reset machine state
			obj.heldObject.Value = null;
			obj.readyForHarvest.Value = false;
			obj.showNextIndex.Value = false;
			obj.ResetParentSheetIndex();

			// Update tapper if on a tree
			if (obj.HasContextTag(TAPPER_CONTEXT_TAG) && terrainFeature is Tree tree)
			{
				tree.UpdateTapperProduct(obj, heldObj);
			}

			// Grant experience for certain machines
			if (Constants.BigCraftableXpLookup.TryGetValue(obj.QualifiedItemId, out var xpAmount))
			{
				Context.Player.gainExperience(Farmer.foragingSkill, xpAmount);
			}

			TrackItem(Constants.ForageableKey, heldObj.DisplayName);
		}
	}
}
