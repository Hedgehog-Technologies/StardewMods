using System.Collections.Generic;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace AutoForager.Integrations
{
	internal class BushBloomWrapper(IMonitor monitor, IModHelper helper)
		: BaseIntegrationWrapper<IBushBloomApi>(monitor, helper, "1.1.9", "NCarigon.BushBloomMod", I18n.Subject_BushBloomSchedules())
	{
		private readonly List<BloomSchedule> _schedules = [];
		public List<BloomSchedule> Schedules => _schedules;

		public async Task<List<BloomSchedule>> UpdateSchedules()
		{
			var defaultBlooms = new BloomSchedule[]
			{
				new("296", new WorldDate(1, Season.Spring, 15), new WorldDate(1, Season.Spring, 18)),
				new("410", new WorldDate(1, Season.Fall, 8), new WorldDate(1, Season.Fall, 11))
			};

			if (ModApi is not null)
			{
				var remainingRetries = ReadyRetries;

				while (!ModApi.IsReady() && remainingRetries-- > 0)
				{
					await Task.Delay(ReadyRetryWaitMs);
				}

				remainingRetries += 1;
				var retryTime = (ReadyRetries - remainingRetries) * ReadyRetryWaitMs;

				Monitor.Log($"Bush Bloom Mod status: Ready: {ModApi.IsReady()} - Remaining retries: {remainingRetries} / {ReadyRetries} - Total time: {retryTime}ms", LogLevel.Debug);

				if (ModApi.IsReady())
				{
					foreach (var sched in ModApi.GetAllSchedules())
					{
						_schedules.Add(new BloomSchedule(sched));
					}
				}
				else
				{
					Monitor.Log($"Bush Bloom Mod not ready within {retryTime}ms. Continuing with only default bush blooms.", LogLevel.Warn);
				}
			}

			if (_schedules.Count == 0)
			{
				_schedules.AddRange(defaultBlooms);
			}

			return Schedules;
		}
	}

	public class BloomSchedule(string itemId, WorldDate startDate, WorldDate endDate)
	{
		public string ItemId { get; } = itemId;
		public WorldDate StartDate { get; } = startDate;
		public WorldDate EndDate { get; } = endDate;

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
