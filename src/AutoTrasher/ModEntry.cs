using AutoTrasher.Extensions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Linq;

namespace AutoTrasher
{
	/// <summary>
	/// The mod entry point.
	/// </summary>
	public class ModEntry : Mod
	{
		private ModConfig _config = new();

		public override void Entry(IModHelper helper)
		{
			_config = helper.ReadConfig<ModConfig>();

			helper.Events.Player.InventoryChanged += OnInventoryChanged;
		}

		private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
		{
			if (!e.IsLocalPlayer) return;

			var messages = Game1.hudMessages;

			foreach (var item in e.Added)
			{
				if (item is null) continue;

				var addMessages = messages.Where(m => item.QualifiedItemId.IEquals(Helper.Reflection.GetField<Item>(messages, "messageSubject")?.GetValue()?.QualifiedItemId));

				foreach (var addMessage in addMessages)
				{
					Game1.hudMessages.Remove(addMessage);
				}
			}
		}

		private void MoveItemToShippingBin(Item item)
		{

		}

		private int RemoveItemFromInventory(Item item)
		{
			var reclamationPrice = Utility.getTrashReclamationPrice(item, Game1.player);

			if (reclamationPrice > 0)
			{
				Game1.player.Money += reclamationPrice;
				Game1.soundBank.PlayCue("coin");
			}

			Game1.player.removeItemFromInventory(item);
			Game1.soundBank.PlayCue("trashcan");

			return reclamationPrice;
		}
	}
}
