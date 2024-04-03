using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Integrations
{
    internal class BushBloomWrapper
    {
        private readonly IMonitor _monitor;
        private readonly IModHelper _helper;

        private readonly IBushBloomModApi? _bbm;

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

            if (helper.ModRegistry.IsLoaded(Constants.BushBloomModUniqueId))
            {
                monitor.Log("Bush Bloom Mod found - Loading active bush schedules", LogLevel.Info);
                _bbm = helper.ModRegistry.GetApi<IBushBloomModApi>(Constants.BushBloomModUniqueId);
            }
        }

        public async Task GetSchedules()
        {
            if (_bbm is not null)
            {
                var tries = 10;

                while (!_bbm.IsReady() && tries-- > 0)
                {
                    await Task.Delay(500);
                }

                _monitor.Log($"{_bbm.IsReady()} - {tries}", LogLevel.Alert);
                foreach (var sched in _bbm.GetAllSchedules())
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

    public interface IBushBloomModApi
    {
        bool IsReady();
        void ReloadSchedules();
        (string, WorldDate, WorldDate)[] GetActiveSchedules(string season, int dayOfMonth, int? year = null, GameLocation? location = null, Vector2? tile = null);
        (string, WorldDate, WorldDate)[] GetAllSchedules();
    }
}
