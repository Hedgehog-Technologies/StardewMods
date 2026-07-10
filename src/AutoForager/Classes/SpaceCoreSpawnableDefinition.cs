using System.Collections.Generic;
using Newtonsoft.Json;
using StardewValley.Enchantments;
using StardewValley.GameData;

namespace AutoForager.Classes
{
	public class SpaceCoreSpawnableDefinition
	{
		/// <summary>
		/// The category of the spawnable item (e.g., "Forageable", "Ore", "Monster", "Artifact", "ResourceClump").
		/// </summary>
		[JsonProperty(nameof(Type))]
		public int Type { get; set; }

		/// <summary>
		/// The item or object ID to spawn (e.g., "(O)16" for Wild Horseradish or a custom item ID).
		/// </summary>
		[JsonProperty(nameof(ForageableItemData))]
		public List<Weighted<GenericSpawnItemDataWithCondition>> ForageableItemData { get; set; } = [];

		/// <summary>
		/// The probability of this item spawning on a given day / attempt (0.0 to 1.0).
		/// </summary>
		[JsonProperty(nameof(Chance))]
		public float Chance { get; set; }

		/// <summary>
		/// A list of valid seasons for this spawnable (e.g., ["spring", "summer"]).
		/// </summary>
		[JsonProperty(nameof(Seasons))]
		public List<string> Seasons { get; set; } = [];

		/// <summary>
		/// A list of locations names or location context tags where this item is allowed to appear.
		/// </summary>
		[JsonProperty(nameof(Locations))]
		public List<string> Locations { get; set; } = [];

		/// <summary>
		/// Optional Stardew Valley 1.6 Game State Query (GSQ) that must evaluate to true for spawning.
		/// </summary>
		[JsonProperty(nameof(Condition))]
		public string Condition { get; set; }

		/// <summary>
		/// Optional: Minimum quantity or stack size when spawned.
		/// </summary>
		[JsonProperty(nameof(MinAmount))]
		public int? MinAmount { get; set; }

		/// <summary>
		/// Optional: Maximum quantity or stack size when spawned.
		/// </summary>
		[JsonProperty(nameof(MaxAmount))]
		public int? MaxAmount { get; set; }

		/// <summary>
		/// Catch-all dictionary for any custom, extra, or type-specific properties in the JSON
		/// (e.g., Monster HP, Ore hardness, custom and tokens) so deserialization never fails.
		/// </summary>
		[JsonExtensionData]
		public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
	}

	public class Weighted<T>
	{
		public double Weight { get; set; }
		public T? Value { get; set; }

		public Weighted()
			: this(1.0D, default)
		{ }

		public Weighted(T value)
			: this(1.0D, value)
		{ }

		public Weighted(double weight, T? value)
		{
			this.Weight = weight;
			this.Value = value;
		}
	}
}
