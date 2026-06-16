using StardewModdingAPI;
using HedgeTech.Common.Extensions;
using HedgeTech.Common.Interfaces;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Integrations
{
	internal class BaseIntegrationWrapper<TModInterface> where TModInterface : class
	{
		protected const string CPUniqueId = "Pathoschild.ContentPatcher";

		protected const int ReadyRetries = Constants.IntegrationReadyRetries;
		protected const int ReadyRetryWaitMs = Constants.IntegrationReadyRetryWaitMs;

		protected string MinVersion { get; }
		protected string UniqueId { get; }
		protected IMonitor Monitor { get; }
		protected IModHelper Helper { get; }

		protected TModInterface? ModApi { get; }
		protected IContentPatcherApi? ContentPatcherApi { get; }

		protected BaseIntegrationWrapper(IMonitor monitor, IModHelper helper, string minVersion, string uniqueId, string logSubject, bool verifyContentPatcher = false)
		{
			MinVersion = minVersion;
			UniqueId = uniqueId;
			Monitor = monitor;
			Helper = helper;

			ModApi = null;
			ContentPatcherApi = null;

			if (Helper.ModRegistry.IsLoaded(UniqueId)
				&& (!verifyContentPatcher || Helper.ModRegistry.IsLoaded(CPUniqueId)))
			{
				var modIntegration = Helper.ModRegistry.Get(UniqueId);

				if (modIntegration is not null)
				{
					var modName = modIntegration.Manifest.Name;
					var modVersion = modIntegration.Manifest.Version;

					if (modVersion.IsEqualToOrNewerThan(MinVersion))
					{
						Monitor.Log($"{modName} found - Loading {logSubject}", LogLevel.Info);
						ModApi = Helper.ModRegistry.GetApi<TModInterface>(UniqueId);

						if (verifyContentPatcher)
						{
							ContentPatcherApi = Helper.ModRegistry.GetApi<IContentPatcherApi>(CPUniqueId);
						}
					}
					else
					{
						Monitor.Log($"{modName} is version {modVersion}. Minimum version {MinVersion} is needed for {modName} functionalities, please consider updating.", LogLevel.Warn);
					}
				}
				else
				{
					Monitor.Log($"Unable to retrieve manifest for {UniqueId} to verify installed version. Proceeding without {UniqueId} functionalities.", LogLevel.Warn);
				}
			}
		}
	}
}
