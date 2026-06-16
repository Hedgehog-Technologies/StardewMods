using Microsoft.Xna.Framework;
using System;
using System.Linq;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using AutoForager.Classes;
using HedgeTech.Common.Extensions;

using Constants = AutoForager.Helpers.Constants;
using SObject = StardewValley.Object;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles foraging from special terrain features (ginger, spring onions).
	/// </summary>
	internal class TerrainFeatureHandler : BaseForagingHandler
	{
		private const string SPRING_ONION_QUALIFIED_ID = "(O)399";
		private const string GINGER_QUALIFIED_ID = "(O)829";

		/// <summary>
		/// Determines if this handler can process the given Hoe Dirt.
		/// </summary>
		public bool CanHandle(HoeDirt hoeDirt)
		{
			if (hoeDirt == null) return false;
			if (!(hoeDirt.crop?.forageCrop.Value ?? false)) return false;
			if (hoeDirt.crop?.whichForageCrop.Value.IsNullOrEmpty() ?? true) return false;

			return true;
		}

		/// <summary>
		/// Handles foraging from Hoe Dirt.
		/// </summary>
		public void Handle(HoeDirt hoeDirt, Vector2 tile)
		{
			var whichCrop = hoeDirt.crop.whichForageCrop.Value;

			switch (whichCrop)
			{
				case Crop.forageCrop_springOnionID:
					HandleSpringOnion(hoeDirt, tile);
					break;

				case Crop.forageCrop_gingerID:
					HandleGinger(hoeDirt, tile);
					break;

				default:
					LogDebug($"Unknown forage crop type: {whichCrop}");
					break;
			}
		}

		/// <summary>
		/// Handles Spring Onion harvesting.
		/// </summary>
		private void HandleSpringOnion(HoeDirt hoeDirt, Vector2 tile)
		{
			var springOnion = Context.ForageableTracker.ObjectForageables
				.FirstOrDefault(i => i.QualifiedItemId.Equals(SPRING_ONION_QUALIFIED_ID));

			if (springOnion == default(ForageableItem) || !springOnion.IsEnabled) return;

			var x = (int)tile.X;
			var y = (int)tile.Y;

			ForageItem(
				ItemRegistry.Create<SObject>(SPRING_ONION_QUALIFIED_ID),
				tile,
				Utility.CreateDaySaveRandom(x * Constants.SpringOnionRandomSeedMultiplier, y * Constants.SpringOnionRandomSeedMultiplier),
				Constants.SpringOnionForageXp);
			hoeDirt.destroyCrop(false);
			PlaySound(HARVEST_SOUND_ID);

			TrackItem(Constants.ForageableKey, springOnion.DisplayName);
		}

		/// <summary>
		/// Handles Ginger Root harvesting.
		/// </summary>
		private void HandleGinger(HoeDirt hoeDirt, Vector2 tile)
		{
			var ginger = Context.ForageableTracker.ObjectForageables
				.FirstOrDefault(i => i.QualifiedItemId.Equals(GINGER_QUALIFIED_ID));

			if (ginger == default(ForageableItem) || !ginger.IsEnabled) return;

			// Check Hoe requirement
			if (Config.RequireHoe && !Context.PlayerHasTool<Hoe>())
			{
				Context.ShowThrottledError(I18n.Message_MissingHoe(I18n.Subject_GingerRoots()));
				LogOnce(I18n.Log_MissingHoe(I18n.Subject_GingerRoots(), I18n.Option_RequireHoe_Name(" ")));
				return;
			}

			hoeDirt.crop?.hitWithHoe((int)tile.X, (int)tile.Y, hoeDirt.Location, hoeDirt);
			hoeDirt.destroyCrop(false);

			TrackItem(Constants.ForageableKey, ginger.DisplayName);
		}

		/// <summary>
		/// Forages an item with quality calculation.
		/// </summary>
		private void ForageItem(SObject obj, Vector2 vec, Random random, int xpGained = 0)
		{
			obj.Quality = DetermineForageQuality(random);
			Context.Player.gainExperience(Farmer.foragingSkill, xpGained);
			CreateItemDebris(obj.getOne(), vec);
		}
	}
}
