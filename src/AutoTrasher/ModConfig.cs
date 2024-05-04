using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AutoTrasher
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public class ModConfig
	{
		private IModHelper? _helper;

		public KeybindList ToggleTrasherKeybind { get; set; }
		public KeybindList OpenMenu { get; set; }
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

		private void ResetToDefault()
		{
			ToggleTrasherKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.T),
				new Keybind(SButton.RightAlt, SButton.T));

			OpenMenu = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.L),
				new Keybind(SButton.RightAlt, SButton.L));

			SetTrash = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.X),
				new Keybind(SButton.RightAlt, SButton.X));
		}
	}
}
