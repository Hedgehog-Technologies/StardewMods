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
		public List<string> TrashItems { get; set; }

		public ModConfig()
		{
			ResetToDefault();
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

		private void ResetToDefault()
		{
			ToggleTrasherKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.T),
				new Keybind(SButton.RightAlt, SButton.T));

			OpenMenu = new KeybindList(new Keybind(SButton.LeftAlt, SButton.L));

			TrashItems = new List<string>
			{
				"168", // Trash
				"169", // Driftwood
				"170", // Broken Glasses
				"171", // Broken CD
				"172", // Soggy Newspaper
				"747", // Rotten Plant
				"748" // Rotten Plant
			};
		}
	}
}
