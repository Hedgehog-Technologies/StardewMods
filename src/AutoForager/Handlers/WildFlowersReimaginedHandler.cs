using AutoForager.Integrations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			if (grassCandidate == null) return false;
			if (wfrWrapper == null) return false;
			var flowerGrass = wfrWrapper.AsFlowerGrass(grassCandidate);
			if (flowerGrass == null) return false;
			// Crop is a from a NetAttribute, to keep the behaviors the same as the source code the type is marked as Crop instead of Crop?.
			if (flowerGrass.Crop == null) return false;
			return true;
		}

		/// <summary>
		/// Handles foraging from the FlowerGrass.
		/// </summary>
		/// <param name="grassCandidate">Flower Grass to harvest.</param>
		public void Handle(Grass grassCandidate)
		{
			// safety checks, this should not trigger under any normal flow.
			if (wfrWrapper == null) return;
			var flowerGrass = wfrWrapper.AsFlowerGrass(grassCandidate);
			if (flowerGrass == null) return;
			// TODO: Add the enable checks.
			
			// Harvest the flower.
			flowerGrass.Harvest();
		}
	}
}
