using System.Collections.Generic;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using AutoForager.Extensions;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Integrations
{
    internal class BushBloomWrapper
    {
        private const string _minVersion = "1.1.9";
        private const string _bbUniqueId = "NCarigon.BushBloomMod";

        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;

        private readonly IBushBloomApi? _bushBloomApi;

        private readonly List<string> _knownShakeOffItems;
        public List<string> KnownShakeOffItems => _knownShakeOffItems;

        private readonly List<BloomSchedule> _schedules;
        public List<BloomSchedule> Schedules => _schedules;

        public BushBloomWrapper(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;

            _knownShakeOffItems = new List<string>();
            _knownShakeOffItems.AddRange(Constants.VanillaBushBlooms);
            _schedules = new();

            if (helper.ModRegistry.IsLoaded(_bbUniqueId))
            {
                var bushBloom = helper.ModRegistry.Get(_bbUniqueId);

                if (bushBloom is not null)
                {
                    var bbName = bushBloom.Manifest.Name;
                    var bbVersion = bushBloom.Manifest.Version;

                    if (bbVersion.IsEqualToOrNewerThan(_minVersion))
                    {
                        monitor.Log(I18n.Log_Wrapper_ModFound(bbName, I18n.Subject_BushBloomSchedules()), LogLevel.Info);
                        _bushBloomApi = helper.ModRegistry.GetApi<IBushBloomApi>(_bbUniqueId);
                    }
                    else
                    {
                        monitor.Log(I18n.Log_Wrapper_OldVersion(bbName, bbVersion, _minVersion), LogLevel.Warn);
                    }
                }
                else
                {
                    monitor.Log(I18n.Log_Wrapper_ManifestError("Bush Bloom Mod"), LogLevel.Warn);
                }
            }
        }

        public async Task<List<BloomSchedule>> UpdateSchedules()
        {
            if (_bushBloomApi is not null)
            {
                var tries = 60;

                while (!_bushBloomApi.IsReady() && tries-- > 0)
                {
                    await Task.Delay(500);
                }

                _monitor.Log($"Bush Bloom Mod status: {_bushBloomApi.IsReady()} - {tries}", LogLevel.Debug);
                foreach (var sched in _bushBloomApi.GetAllSchedules())
                {
                    _schedules.Add(new BloomSchedule(sched));
                }
            }
            else
            {
                _schedules.AddRange(new BloomSchedule[]
                {
                    new BloomSchedule("296", new WorldDate(1, Season.Spring, 15), new WorldDate(1, Season.Spring, 18)),
                    new BloomSchedule("410", new WorldDate(1, Season.Fall, 8), new WorldDate(1, Season.Fall, 11))
                });
            }

            return Schedules;
        }
    }

    public class BloomSchedule
    {
        public string ItemId { get; }
        public WorldDate StartDate { get; }
        public WorldDate EndDate { get; }

        public BloomSchedule(string itemId, WorldDate startDate, WorldDate endDate)
        {
            ItemId = itemId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public BloomSchedule((string, WorldDate, WorldDate) schedule)
            : this(schedule.Item1, schedule.Item2, schedule.Item3)
        { }
    }

    public interface IBushBloomApi
    {
        /// <summary>
        /// Specifies whether BBM successfully parsed all schedules.
        /// </summary>
        bool IsReady();

        /// <summary>
        /// Returns an array of (item_id, first_day, last_day) for all blooming schedules.
        /// </summary>
        (string, WorldDate, WorldDate)[] GetAllSchedules();
    }
}
