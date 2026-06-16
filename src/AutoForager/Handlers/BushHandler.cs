using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using AutoForager.Extensions;
using AutoForager.Integrations;

using Constants = AutoForager.Helpers.Constants;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles foraging from Bushes (berry bushes, tea bushes, walnut bushes, custom bushes).
	/// </summary>
	internal class BushHandler : BaseForagingHandler
	{
		private readonly CustomBushWrapper? _customBushWrapper;

		public BushHandler(CustomBushWrapper? customBushWrapper)
		{
			_customBushWrapper = customBushWrapper;
		}

		public override int Priority => 30; // Check Bushes after Trees

		/// <summary>
		/// Determines if this handler can process the given Bush.
		/// </summary>
		public bool CanHandle(Bush bush)
		{
			if (bush == null) return false;
			if (!Config.AnyBushEnabled()) return false;
			if (bush.townBush.Value) return false;
			if (!bush.inBloom()) return false;
			if (bush.tileSheetOffset.Value != Constants.BushActionableTileSheetOffset) return false;

			if (!bush.isActionable())
			{
				Monitor.Log($"A bush feature of size [{bush.size.Value}] was marked as not actionable. This shouldn't be possible.", LogLevel.Warn);
				LogDebug($"Size: [{bush.size.Value}]; Location: [{bush.Location.NameOrUniqueName}]; Tile Location: [{bush.Tile}]; Town Bush: [{bush.townBush.Value}]");
				return false;
			}

			return CheckBushType(bush);
		}

		/// <summary>
		/// Handles foraging from the given bush.
		/// </summary>
		public void Handle(Bush bush)
		{
			bush.performUseAction(bush.Tile);
		}

		/// <summary>
		/// Checks if the bush type is enabled and tracks the item.
		/// </summary>
		private bool CheckBushType(Bush bush)
		{
			switch (bush.size.Value)
			{
				case 0:
				case 1:
				case (int)Constants.BushSize.Berry:
					return HandleBloomingBush(bush);

				case (int)Constants.BushSize.Tea:
					return HandleTeaBush(bush);

				case (int)Constants.BushSize.Walnut:
					return HandleWalnutBush(bush);

				default:
					Monitor.Log($"Unknown Bush size: [{bush.size.Value}]", LogLevel.Warn);
					return false;
			}
		}

		/// <summary>
		/// Handles blooming bushes (salmonberry, blackberry, etc.).
		/// </summary>
		private bool HandleBloomingBush(Bush bush)
		{
			var bloomItem = bush.GetShakeOffItem();
			if (bloomItem is null) return false;

			if (!Context.ForageableTracker.BushForageables.TryGetItem(bloomItem, out var item))
			{
				Context.ForageableTracker.BushForageables.TryGetItem("(O)" + bloomItem, out item);
			}

			if (item is null || !item.IsEnabled) return false;

			TrackItem(Constants.BushKey, item.DisplayName);
			return true;
		}

		/// <summary>
		/// Handles tea bushes (vanilla and custom).
		/// </summary>
		private bool HandleTeaBush(Bush bush)
		{
			// Check for custom bush
			if (_customBushWrapper?.IsCustomBush(bush) ?? false)
			{
				return HandleCustomBush(bush);
			}

			// Vanilla tea bush
			if (!Config.GetTeaBushesEnabled())
			{
				LogOnce(I18n.Log_DisabledConfig(I18n.Subject_TeaBushes(), I18n.Option_ToggleAction_Name(I18n.Subject_TeaBushes())), LogLevel.Info);
				return false;
			}

			TrackItem(Constants.BushKey, I18n.Subject_TeaBushes());
			return true;
		}

		/// <summary>
		/// Handles custom bushes from the Custom Bush mod.
		/// </summary>
		private bool HandleCustomBush(Bush bush)
		{
			var shakeOffItem = bush.modData[CustomBushWrapper.ShakeOffItemKey];

			if (!Context.ForageableTracker.BushForageables.TryGetItem(shakeOffItem, out var bItem)
				|| !(bItem?.IsEnabled ?? false))
			{
				LogOnce($"{shakeOffItem} was not shaken from custom bush as it does not exist or is disabled.", LogLevel.Info);
				return false;
			}

			TrackItem(Constants.BushKey, bItem.DisplayName);
			return true;
		}

		/// <summary>
		/// Handles walnut bushes (Ginger Island).
		/// </summary>
		private bool HandleWalnutBush(Bush bush)
		{
			if (!Config.GetWalnutBushesEnabled())
			{
				LogOnce(I18n.Log_DisabledConfig(I18n.Subject_WalnutBushes(), I18n.Option_ToggleAction_Name(I18n.Subject_WalnutBushes())), LogLevel.Info);
				return false;
			}

			TrackItem(Constants.BushKey, I18n.Subject_WalnutBushes());
			return true;
		}
	}
}
