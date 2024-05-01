using System.Linq;
using StardewValley;
using HedgeTech.Common.Extensions;

namespace HedgeTech.Common.Utilities
{
	public static class ItemUtilities
	{
		public static string? GetItemIdFromName(string name)
		{
			string? itemId = null;

			if (!name.IsNullOrEmpty())
			{
				name = name.Trim();

				if (int.TryParse(name, out var i))
				{
					name = $"(O){i}";
				}

				var item = ItemRegistry.GetMetadata(name);

				if (!item.Exists())
				{
					item = ItemRegistry.GetMetadata($"(O){name}");
				}

				if (item.Exists())
				{
					itemId = item.QualifiedItemId.Substring(3);
				}
				else
				{
					itemId = Game1.objectData
						.Where(d => d.Value.Name.IEquals(name))
						.Select(d => d.Key)
						.FirstOrDefault();
				}
			}

			return itemId;
		}
	}
}
