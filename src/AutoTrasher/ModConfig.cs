using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AutoTrasher
{
	public class ModConfig
	{
		public KeybindList ToggleTrasherKeybind { get; set; }
		public List<string> TrashItems { get; set; }

		public ModConfig()
		{
			ToggleTrasherKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.T),
				new Keybind(SButton.RightAlt, SButton.T));

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
