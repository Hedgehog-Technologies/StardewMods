using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


namespace AutoForager.Integrations
{
	/// <summary>
	/// Integration with the WildFlowersReimagined mod.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="helper"></param>
	internal class WildFlowersReimaginedWrapper(IMonitor monitor, IModHelper helper) : BaseIntegrationWrapper<IWildFlowersReimaginedApi>(monitor, helper, "3.3.3", "jpp.WildFlowersReimagined", I18n.Category_WildFlowerReimagined())
	{

		/// <summary>
		/// Get the list of known flowers.
		/// </summary>
		/// <returns>List of object data.</returns>
		public List<ItemMetadata> GetKnownFlowers()
		{
			List < ItemMetadata > flowerList = new();
			if (ModApi is not null)
			{
				flowerList = ModApi.GetKnownFlowers();
			}
			return flowerList;
		}

		/// <summary>
		/// Attempts to cast the Grass to FlowerGrass.
		/// </summary>
		/// <param name="candidate">Grass to check</param>
		/// <returns>FlowerGrass if the cast is successful, null otherwise.</returns>
		public IFlowerGrass? AsFlowerGrass(Grass candidate)
		{
			if (ModApi is not null)
			{
				return ModApi.AsFlowerGrass(candidate);
			}
			return null;
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
		/// <param name="candidate"></param>
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
