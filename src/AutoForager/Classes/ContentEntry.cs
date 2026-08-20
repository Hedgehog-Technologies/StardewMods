using System.Collections.Generic;

namespace AutoForager.Classes
{
	internal class ContentEntry
	{
		public string Category { get; set; }
		public List<string>? Forageables { get; set; }
		public List<string>? FruitTrees { get; set; }
		public List<string>? WildTrees { get; set; }
		public List<string>? IgnoredItems { get; set; }

		public ContentEntry()
		{
			Category = string.Empty;
			Forageables = [];
			FruitTrees = [];
			WildTrees = [];
			IgnoredItems = [];
		}
	}
}
