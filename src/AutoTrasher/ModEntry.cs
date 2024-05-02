using AutoTrasher.Components;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace AutoTrasher
{
	/// <summary>
	/// The mod entry point.
	/// </summary>
	public class ModEntry : Mod
	{
		private ModConfig _config = new();
		private bool _isTrasherActive = true;

		public override void Entry(IModHelper helper)
		{
			_config = helper.ReadConfig<ModConfig>();

			helper.Events.Input.ButtonsChanged += OnButtonsChanged;
			helper.Events.Player.InventoryChanged += OnInventoryChanged;
		}

		private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
		{
			if (Game1.activeClickableMenu is not null) return;

			if (_config.ToggleTrasherKeybind.JustPressed())
			{
				_isTrasherActive = !_isTrasherActive;

				if (_isTrasherActive)
				{
					Helper.Events.Player.InventoryChanged += OnInventoryChanged;
				}
				else
				{
					Helper.Events.Player.InventoryChanged -= OnInventoryChanged;
				}
			}
			else if (_config.OpenMenu.JustPressed())
			{
				if (Context.IsPlayerFree && Game1.currentMinigame == null)
				{
					Game1.activeClickableMenu = new TrashListMenu(Monitor);
				}
			}
		}

		private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
		{
			if (!e.IsLocalPlayer) return;

			foreach (var item in e.Added)
			{
				if (item is null) continue;
				if (!_config.TrashItems.Contains(item.ItemId)) continue;

				var message = new HUDMessage($"Auto-trashed {item.DisplayName}")
				{
					number = item.Stack,
					type = $"autotrash_{item.Name}",
					messageSubject = item
				};

				Game1.addHUDMessage(message);
				RemoveItemFromInventory(item);
			}
		}

		private void MoveItemToShippingBin(Item item)
		{

		}

		private static void RemoveItemFromInventory(Item item)
		{
			var reclamationPrice = Utility.getTrashReclamationPrice(item, Game1.player);

			if (reclamationPrice > 0)
			{
				Game1.player.Money += reclamationPrice;
				Game1.soundBank.PlayCue("coin");
			}

			Game1.player.removeItemFromInventory(item);
			Game1.soundBank.PlayCue("trashcan");
		}
	}
}
