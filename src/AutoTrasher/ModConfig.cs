using System.Collections.Generic;

namespace AutoTrasher
{
	public class ModConfig
	{
		public List<string> TrashItems { get; set; }

		public ModConfig()
		{
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
