using System.Linq;
using StardewValley.TerrainFeatures;
using AutoForager.Integrations;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles foraging FlowerGrass from Wild Flowers Reimagined.
	/// </summary>
	/// <param name="wfrWrapper"></param>
	internal class WildFlowersReimaginedHandler(WildFlowersReimaginedWrapper? wfrWrapper) : BaseForagingHandler
	{
		/// <summary>
		/// Determines if this handler can process given the current grass.
		/// </summary>
		/// <param name="grassCandidate">Potential flower grass. Grass was chosen as the closes Shared type between the mods.</param>
		/// <returns>If this Grass should be process by the handler</returns>
		public bool CanHandle(Grass grassCandidate)
		{
			// Return early if any of this happen:
			// - the candidate is null
			// - the wrapper is null
			// - the candidate is not a flower grass ( normal grass or another grass child )
			// - the FlowerGrass doesn't have crop ( already harvested )
			if (grassCandidate is null) return false;

			var flowerGrass = wfrWrapper?.AsFlowerGrass(grassCandidate) ?? null;
			if (flowerGrass is null) return false;

			// Crop is a from a NetAttribute, to keep the behaviors the same as the source code the type is marked as Crop instead of Crop?.
			if (flowerGrass.Crop is null) return false;

			var harvestId = flowerGrass.Crop.GetData().HarvestItemId;
			return Context.ForageableTracker.FlowerForageables.Any(i => harvestId == i.ItemId && i.IsEnabled);
			
		}

		/// <summary>
		/// Handles foraging from the FlowerGrass.
		/// </summary>
		/// <param name="grassCandidate">Flower Grass to harvest.</param>
		public void Handle(Grass grassCandidate)
		{
			var flowerGrass = wfrWrapper?.AsFlowerGrass(grassCandidate) ?? null;
			if (flowerGrass is null) return;

			// Harvest the flower.
			flowerGrass.Harvest();
		}
	}
}
