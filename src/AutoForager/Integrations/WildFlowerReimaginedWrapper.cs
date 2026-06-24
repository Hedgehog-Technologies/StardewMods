using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;


namespace AutoForager.Integrations
{
	/// <summary>
	/// Integration with the WildFlowersReimagined mod.
	/// </summary>
	internal class WildFlowersReimaginedWrapper(IMonitor monitor, IModHelper helper) : BaseIntegrationWrapper<IWildFlowersReimaginedApi>(monitor, helper, "3.3.3", "jpp.WildFlowersReimagined", I18n.Category_WildFlowerReimagined())
	{
		/// <summary>
		/// Get the list of known flowers.
		/// </summary>
		/// <returns>List of object data.</returns>
		public List<ItemMetadata> GetKnownFlowers()
		{
			return ModApi?.GetKnownFlowers() ?? [];
		}

		/// <summary>
		/// Attempts to cast the Grass to FlowerGrass.
		/// </summary>
		/// <param name="candidate">Grass to check</param>
		/// <returns>FlowerGrass if the cast is successful, null otherwise.</returns>
		public IFlowerGrass? AsFlowerGrass(Grass candidate)
		{
			return ModApi?.AsFlowerGrass(candidate) ?? null;
		}
	}

	/// <summary>
	/// WildFlowersReimagined API
	/// </summary>
	public interface IWildFlowersReimaginedApi
	{
		/// <summary>
		/// Returns the List of known flowers.
		/// </summary>
		/// <returns>List of ItemMetadata objects that are known to be valid flowers.</returns>
		public List<ItemMetadata> GetKnownFlowers();

		/// <summary>
		/// Cast a Grass TerrainFeature and attempts to down cast it to the GrassFlower IGrassFlower interface.
		/// </summary>
		/// <param name="candidate">Grass to check</param>
		/// <returns>IFlowerGrass if it's a FlowerGrass, null otherwise.</returns>
		public IFlowerGrass? AsFlowerGrass(Grass candidate);
	}


	/// <summary>
	/// Public interface for the FlowerGrass Terrain Feature.
	/// </summary>
	public interface IFlowerGrass
	{
		/// <summary>
		/// Get the underlaying Flower as a crop.
		/// </summary>
		public Crop Crop { get; }

		/// <summary>
		/// Harvest the Flower from the FlowerGrass.
		/// </summary>
		public void Harvest();
	}
}
