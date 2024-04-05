using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley.GameData;
using StardewValley;
using StardewValley.TerrainFeatures;
using AutoForager.Extensions;

namespace AutoForager.Integrations
{
    internal class CustomBushWrapper
    {
        private const string _minVersion = "1.0.3";
        private const string _cbUniqueId = "furyx639.CustomBush";
        public const string _dataPath = _cbUniqueId + "/Data";

        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;

        private readonly ICustomBushApi? _customBushApi;

        public CustomBushWrapper(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;

            if (helper.ModRegistry.IsLoaded(_cbUniqueId))
            {
                var customBush = helper.ModRegistry.Get(_cbUniqueId);

                if (customBush is not null)
                {
                    var cbName = customBush.Manifest.Name;
                    var cbVersion = customBush.Manifest.Version;

                    if (cbVersion.IsEqualToOrNewerThan(_minVersion))
                    {
                        monitor.Log(I18n.Log_Wrapper_ModFound(cbName, I18n.Category_CustomBushes()), LogLevel.Info);
                        _customBushApi = helper.ModRegistry.GetApi<ICustomBushApi>(_cbUniqueId);
                    }
                    else
                    {
                        monitor.Log(I18n.Log_Wrapper_OldVersion(cbName, cbVersion, _minVersion), LogLevel.Warn);
                    }
                }
                else
                {
                    monitor.Log(I18n.Log_Wrapper_ManifestError("Custom Bush"), LogLevel.Warn);
                }
            }
        }

        public bool IsCustomBush(Bush bush) => _customBushApi?.IsCustomBush(bush) ?? false;

        public bool TryGetDrops(Bush bush, out IEnumerable<(GenericSpawnItemDataWithCondition, Season? season, float change)>? drops)
        {
            drops = null;
            return _customBushApi?.TryGetDrops(bush, out drops) ?? false;
        }
    }

    public interface ICustomBushApi
    {
        /// <summary>Determines if the given Bush instance is a custom bush.</summary>
        /// <param name="bush">The bush instance to check.</param>
        /// <returns>True if the bush is a custom bush, otherwise false.</returns>
        public bool IsCustomBush(Bush bush);

        /// <summary>Tries to get the custom bush model associated with the given bush.</summary>
        /// <param name="bush">The bush.</param>
        /// <param name="customBush">When this method returns, contains the custom bush associated with the given bush, if found; otherwise, it contains null.</param>
        /// <returns>true if the custom bush associated with the given bush is found; otherwise, false.</returns>
        public bool TryGetCustomBush(Bush bush, out ICustomBush? customBush);

        /// <summary>Tries to get the drops from a bush.</summary>
        /// <param name="bush">The bush to get the drops from.</param>
        /// <param name="drops">When this method returns, contains the drops from the bush, or null if there are no drops available. This parameter is passed uninitialized.</param>
        /// <returns>true if the drops were successfully retrieved; otherwise, false.</returns>
        public bool TryGetDrops(
            Bush bush,
            out IEnumerable<(GenericSpawnItemDataWithCondition, Season? Season, float Chance)>? drops);
    }

    public class CustomBushDrop : GenericSpawnItemDataWithCondition
    {
        /// <summary>Gets or sets the specific season when the item can be produced.</summary>
        public Season? Season { get; set; }

        /// <summary>Gets or sets the probability that the item will be produced.</summary>
        public float Chance { get; set; } = 1f;
    }

    public class CustomBush : ICustomBush
    {
        /// <summary>Gets or sets the items produced by this custom bush.</summary>
        public List<CustomBushDrop> ItemsProduced { get; set; } = new();

        /// <inheritdoc />
        public int AgeToProduce { get; set; } = 20;

        /// <inheritdoc />
        public int DayToBeginProducing { get; set; } = 22;

        /// <inheritdoc />
        public string Description { get; set; } = string.Empty;

        /// <inheritdoc />
        public string DisplayName { get; set; } = string.Empty;

        /// <inheritdoc />
        public string IndoorTexture { get; set; } = string.Empty;

        /// <inheritdoc />
        public List<Season> Seasons { get; set; } = new();

        /// <inheritdoc />
        public List<PlantableRule> PlantableLocationRules { get; set; } = new();

        /// <inheritdoc />
        public string Texture { get; set; } = string.Empty;

        /// <inheritdoc />
        public int TextureSpriteRow { get; set; }
    }

    public interface ICustomBush
    {
        /// <summary>Gets or sets the age needed to produce.</summary>
        public int AgeToProduce { get; set; }

        /// <summary>Gets or sets the day of month to begin producing.</summary>
        public int DayToBeginProducing { get; set; }

        /// <summary>Gets or sets the description of the bush.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the display name of the bush.</summary>
        public string DisplayName { get; set; }

        /// <summary>Gets or sets the default texture used when planted indoors.</summary>
        public string IndoorTexture { get; set; }

        /// <summary>Gets or sets the season in which this bush will produce its drops.</summary>
        public List<Season> Seasons { get; set; }

        /// <summary>Gets or sets the rules which override the locations that custom bushes can be planted in.</summary>
        public List<PlantableRule> PlantableLocationRules { get; set; }

        /// <summary>Gets or sets the texture of the tea bush.</summary>
        public string Texture { get; set; }

        /// <summary>Gets or sets the row index for the custom bush's sprites.</summary>
        public int TextureSpriteRow { get; set; }
    }
}
