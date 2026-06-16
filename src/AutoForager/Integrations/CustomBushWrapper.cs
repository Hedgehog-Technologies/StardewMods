using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley.GameData;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace AutoForager.Integrations
{
	internal class CustomBushWrapper : BaseIntegrationWrapper<ICustomBushApi>
	{
		public string ShakeOffItemKey { get; }

		public CustomBushWrapper(IMonitor monitor, IModHelper helper)
			: base(monitor, helper, "1.0.4", "furyx639.CustomBush", I18n.Category_CustomBushes(), true)
		{
			ShakeOffItemKey = UniqueId + "/ShakeOff";
		}

		public bool IsCustomBush(Bush bush) => ModApi?.IsCustomBush(bush) ?? false;

		public async Task<IEnumerable<string>> GetDrops()
		{
			var customDrops = new List<string>();

			if (ModApi is not null && ContentPatcherApi is not null)
			{
				var remainingRetries = ReadyRetries;

				while (!ContentPatcherApi.IsConditionsApiReady && remainingRetries-- > 0)
				{
					await Task.Delay(ReadyRetryWaitMs);
				}

				remainingRetries += 1;
				var retryTime = (ReadyRetries - remainingRetries) * ReadyRetryWaitMs;

				Monitor.Log($"Custom Bush status: Content Patcher Ready: {ContentPatcherApi.IsConditionsApiReady} - Remaining Retries: {remainingRetries} / {ReadyRetries} - Total time: {retryTime}ms", LogLevel.Debug);

				if (ContentPatcherApi.IsConditionsApiReady)
				{
					var bushes = ModApi.GetData();

					foreach (var (Id, Data) in bushes)
					{
						Monitor.Log(Id, LogLevel.Debug);

						if (ModApi.TryGetDrops(Id, out var drops))
						{
							customDrops.AddRange(drops?.Select(d => d.ItemId) ?? []);
						}
					}
				}
				else
				{
					Monitor.Log($"Custom Bush or Content Patcher was not ready within {retryTime}ms. Continuing without Custom Bush integration.", LogLevel.Warn);
				}
			}

			return customDrops;
		}
	}

	public interface ICustomBushDrop : ISpawnItemData
	{
		/// <summary>Gets the specific season when the item can be produced.</summary>
		public Season? Season { get; }

		/// <summary>Gets the probability that the item will be produced.</summary>
		public float Chance { get; }

		/// <summary>A game state query which indicates whether the item should be added. Defaults to always added.</summary>
		public string? Condition { get; }

		/// <summary>An ID for this entry within the current list (not the item itself, which is <see cref="P:StardewValley.GameData.GenericSpawnItemData.ItemId" />). This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.</summary>
		public string? Id { get; }
	}

	public interface ICustomBush
	{
		/// <summary>Gets the age needed to produce.</summary>
		public int AgeToProduce { get; }

		/// <summary>Gets the day of month to begin producing.</summary>
		public int DayToBeginProducing { get; }

		/// <summary>Gets the description of the bush.</summary>
		public string Description { get; }

		/// <summary>Gets the display name of the bush.</summary>
		public string DisplayName { get; }

		/// <summary>Gets the default texture used when planted indoors.</summary>
		public string IndoorTexture { get; }

		/// <summary>Gets the season in which this bush will produce its drops.</summary>
		public List<Season> Seasons { get; }

		/// <summary>Gets the rules which override the locations that custom bushes can be planted in.</summary>
		public List<PlantableRule> PlantableLocationRules { get; }

		/// <summary>Gets the texture of the tea bush.</summary>
		public string Texture { get; }

		/// <summary>Gets the row index for the custom bush's sprites.</summary>
		public int TextureSpriteRow { get; }

		/// <summary>Retrieves the items produced by the custom bush.</summary>
		/// <returns>An enumerable collection of objects implementing the ICustomBushDrop interface. Each object represents an item produced by the custom bush.</returns>
		//public IEnumerable<ICustomBushDrop> GetItemsProduced();
	}

	public interface ICustomBushApi
	{
		/// <summary>Gets the data model for all Custom Bush.</summary>
		public IEnumerable<(string Id, ICustomBush Data)> GetData();

		/// <summary>Determines if the given Bush instance is a custom bush.</summary>
		/// <param name="bush">The bush instance to check.</param>
		/// <returns>True if the bush is a custom bush, otherwise false.</returns>
		public bool IsCustomBush(Bush bush);

		/// <summary>Tries to get the custom bush model associated with the given bush.</summary>
		/// <param name="bush">The bush.</param>
		/// <param name="customBush">When this method returns, contains the custom bush associated with the given bush, if found; otherwise, it contains null.</param>
		/// <returns>true if the custom bush associated with the given bush is found; otherwise, false.</returns>
		public bool TryGetCustomBush(Bush bush, out ICustomBush? customBush);

		/// <summary>Tries to get the custom bush drop associated with the given bush id.</summary>
		/// <param name="id">The id of the bush.</param>
		/// <param name="drops">When this method returns, contains the items produced by the custom bush.</param>
		/// <returns>true if the drops associated with the given id is found; otherwise, false.</returns>
		public bool TryGetDrops(string id, out IList<ICustomBushDrop>? drops);
	}
}
