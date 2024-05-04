using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using AutoTrasher.Components;
using StardewValley.Menus;

using SObject = StardewValley.Object;

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
			_config.AddHelper(helper);

			helper.Events.Input.ButtonsChanged += OnButtonsChanged;
			helper.Events.Player.InventoryChanged += OnInventoryChanged;
		}

		private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
		{

			if (Game1.activeClickableMenu is not null && _config.SetTrash.JustPressed())
			{
				if (Game1.activeClickableMenu is GameMenu menu && menu?.GetCurrentPage() is InventoryPage page)
				{
					var invMenu = page?.inventory;
					var hoveredItem = invMenu?.getItemAt(Game1.getMouseX(), Game1.getMouseY());
					var notifMessage = string.Empty;
					var addNotifSubject = false;

					if (hoveredItem is not null)
					{
						if (hoveredItem is not SObject)
						{
							notifMessage = $"Cannot add {hoveredItem.DisplayName} to Trash List";
						}
						else if (!_config.TrashItems.Contains(hoveredItem.ItemId))
						{
							_config.AddTrashItem(hoveredItem.ItemId);
							notifMessage = $"{hoveredItem.DisplayName} added to Trash List";
							addNotifSubject = true;
						}
						else
						{
							notifMessage = $"{hoveredItem.DisplayName} already on Trash List";
							addNotifSubject = true;
						}
					}
					else
					{
						Monitor.Log($"No item selected when attempting to add item to Trash List", LogLevel.Debug);
					}

					if (notifMessage != string.Empty)
					{
						Monitor.Log(notifMessage, LogLevel.Info);

						var message = new HUDMessage(notifMessage)
						{
							messageSubject = addNotifSubject ? hoveredItem : null,
							type = $"autotrash_add_{hoveredItem?.ItemId ?? "-1"}",
							whatType = HUDMessage.error_type
						};

						Game1.addHUDMessage(message);
					}
				}
			}
			else
			{
				if (_config.ToggleTrasherKeybind.JustPressed())
				{
					_isTrasherActive = !_isTrasherActive;

					if (_isTrasherActive)
					{
						Helper.Events.Player.InventoryChanged += OnInventoryChanged;
						Game1.addHUDMessage(new HUDMessage("Auto Trasher has been ACTIVATED"));
					}
					else
					{
						Helper.Events.Player.InventoryChanged -= OnInventoryChanged;
						Game1.addHUDMessage(new HUDMessage("Auto Trasher has been DEACTIVATED"));
					}
				}
				else if (_config.OpenTrashMenu.JustPressed())
				{
					if (Context.IsPlayerFree && Game1.currentMinigame == null)
					{
						Game1.activeClickableMenu = new TrashListMenu(Monitor, _config);
					}
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
