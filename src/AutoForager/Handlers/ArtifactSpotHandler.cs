using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Tools;
using HedgeTech.Common.Extensions;

using Constants = AutoForager.Helpers.Constants;
using SObject = StardewValley.Object;

namespace AutoForager.Handlers
{
	/// <summary>
	/// Handles digging artifact spots and seed spots.
	/// </summary>
	internal class ArtifactSpotHandler : BaseForagingHandler
	{
		private const string ARTIFACT_SPOT_QUALIFIED_ID = "(O)590";
		private const string SEED_SPOT_QUALIFIED_ID = "(O)SeedSpot";

		public override int Priority => 60; // Check after regular Objects

		/// <summary>
		/// Determines if this handler can process the given Object.
		/// </summary>
		public bool CanHandle(SObject obj)
		{
			if (obj == null) return false;

			var isArtifactSpot = Config.ForageArtifactSpots && obj.QualifiedItemId.IEquals(ARTIFACT_SPOT_QUALIFIED_ID);
			var isSeedSpot = Config.ForageSeedSpots && obj.QualifiedItemId.IEquals(SEED_SPOT_QUALIFIED_ID);

			return isArtifactSpot || isSeedSpot;
		}

		public void Handle(SObject obj)
		{
			// Check Hoe requirement
			if (Config.RequireHoe && !Context.PlayerHasTool<Hoe>())
			{
				Context.ShowThrottledError(I18n.Message_MissingHoe(obj.name));
				LogOnce(I18n.Log_MissingHoe(obj.Name, I18n.Option_RequireHoe_Name(" ")), LogLevel.Info);
				return;
			}

			// Get Hoe tool
			var tool = Context.GetOrCreateTool<Hoe>(Config.RequireHoe);

			if (tool == null)
			{
				LogDebug($"Failed to get instance of Hoe tool - RequireHoe: {Config.RequireHoe}");
				return;
			}

			// Perform the dig action
			tool.lastUser = Context.Player;
			obj.performToolAction(tool);

			TrackItem(Constants.ForageableKey, obj.DisplayName);
		}
	}
}
