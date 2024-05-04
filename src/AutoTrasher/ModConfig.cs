using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using HedgeTech.Common.Interfaces;

namespace AutoTrasher
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public class ModConfig
	{
		private const string _gmcmUniqueId = "spacechase0.GenericModConfigMenu";

		private IModHelper? _helper;

		public KeybindList ToggleTrasherKeybind { get; set; }
		public KeybindList OpenTrashMenu { get; set; }
		public KeybindList SetTrash { get; set; }
		public List<string> TrashItems { get; set; }

		public ModConfig()
		{
			ResetToDefault();

			TrashItems = new List<string>
			{
				"168", // Trash
				"169", // Driftwood
				"170", // Broken Glasses
				"171", // Broken CD
				"172", // Soggy Newspaper
				"747", // Rotten Plant
				"748", // Rotten Plant

				// REMOVE ME - TESTING ONLY
				"268", // Trash
				"269", // Driftwood
				"270", // Broken Glasses
				"271", // Broken CD
				"272", // Soggy Newspaper
				"847", // Rotten Plant
				"848" // Rotten Plant
			};
		}

		private void ResetToDefault()
		{
			ToggleTrasherKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.T),
				new Keybind(SButton.RightAlt, SButton.T));

			OpenTrashMenu = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.L),
				new Keybind(SButton.RightAlt, SButton.L));

			SetTrash = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.X),
				new Keybind(SButton.RightAlt, SButton.X));
		}

		public void AddHelper(IModHelper helper)
		{
			_helper = helper;
		}

		public void RemoveTrashItem(string itemId)
		{
			TrashItems.Remove(itemId);
			_helper?.WriteConfig(this);
		}

		public void AddTrashItem(string itemId)
		{
			TrashItems.Add(itemId);
			_helper?.WriteConfig(this);
		}

		public void RegisterModConfigMenu(IModHelper helper, IManifest manifest)
		{
			if (!helper.ModRegistry.IsLoaded(_gmcmUniqueId)) return;

			var gmcmApi = helper.ModRegistry.GetApi<IGenericModConfigMenu>(_gmcmUniqueId);
			if (gmcmApi is null) return;

			try
			{
				gmcmApi.Unregister(manifest);
			}
			catch { }

			gmcmApi.Register(
				mod: manifest,
				reset: ResetToDefault,
				save: () => helper.WriteConfig(this));

			gmcmApi.AddSectionTitle(
				mod: manifest,
				text: () => "General");

			gmcmApi.AddKeybindList(
				mod: manifest,
				name: () => "Toggle Trasher Keybind",
				tooltip: () => "Keybinding to toggle the Auto Trasher on and off.",
				getValue: () => ToggleTrasherKeybind,
				setValue: val => ToggleTrasherKeybind = val);

			gmcmApi.AddKeybindList(
				mod: manifest,
				name: () => "Open Trash List Menu Keybind",
				tooltip: () => "Keybinding to open the Trash List Menu.",
				getValue: () => OpenTrashMenu,
				setValue: val => OpenTrashMenu = val);

			gmcmApi.AddKeybindList(
				mod: manifest,
				name: () => "Add Item to Trash List Keybind",
				tooltip: () => "Keybinding to add the currently hovered inventory item to the trash list.",
				getValue: () => SetTrash,
				setValue: val => SetTrash = val);
		}
	}
}
