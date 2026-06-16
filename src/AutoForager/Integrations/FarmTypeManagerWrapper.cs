using System.Collections.Generic;
using StardewModdingAPI;

namespace AutoForager.Integrations
{
	internal class FarmTypeManagerWrapper(IMonitor monitor, IModHelper helper) : BaseIntegrationWrapper<IFarmTypeManagerApi>(monitor, helper, "1.20.0", "Esca.FarmTypeManager", I18n.Subject_SpawnableForageIds())
	{
		public IDictionary<string, IEnumerable<string>> ForageIdsPerContentPack { get; private set; } = new Dictionary<string, IEnumerable<string>>();

		public IDictionary<string, IEnumerable<string>> UpdateForageIds()
		{
			if (ModApi is not null)
			{
				ForageIdsPerContentPack = ModApi.GetForageIDsFromContentPacks();
			}

			return ForageIdsPerContentPack;
		}
	}

	/// <summary>The public API interface for Farm Type Manager (FTM), provided through SMAPI's mod helper.</summary>
	public interface IFarmTypeManagerApi
	{
		/// <summary>Gets information about all the valid forage IDs in loaded FTM content packs. Keys are content pack IDs (or "" for other sources). Values are a list of each valid, qualified item ID.</summary>
		/// <remarks>
		/// This method will produce information about the current in-game day.
		/// It is available as soon as SMAPI has loaded all content packs, e.g. during GameLaunched events.
		/// Results may change after FTM's DayStarted events, during which FTM reloads all content pack data.
		/// Data from save-specific personal config files will only be included if "Context.IsWorldReady" is true.
		/// </remarks>
		/// <param name="includePlacedItems">If true, this list will include the IDs of forage items that are NOT normal <see cref="StardewValley.Object"/> instances. These spawn inside a custom <see cref="TerrainFeature"/> subclass, but imitate most normal forage behavior.</param>
		/// <param name="includeContainers">If true, this list will include the IDs of containers that are spawned as forage (chests, breakable barrels and crates, etc).</param>
		public IDictionary<string, IEnumerable<string>> GetForageIDsFromContentPacks(bool includePlacedItems = false, bool includeContainers = false);
	}
}
