using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using StardewModdingAPI;
using AutoForager.Classes;
using AutoForager.Integrations;
using HedgeTech.Common.Extensions;
using HedgeTech.Common.Utilities;

namespace AutoForager.Services
{
	/// <summary>
	/// Manages content packs loading and integration with other mods.
	/// </summary>
	internal class ContentPackService
	{
		private const string CONTENT_PACK_FILE_NAME = "content.json";

		private readonly IMonitor _monitor;
		private readonly IModHelper _helper;
		private readonly ModConfig _config;

		private readonly Dictionary<string, string> _cpForageables;
		private readonly Dictionary<string, string> _cpFruitTrees;
		private readonly Dictionary<string, string> _cpWildTrees;
		private readonly Dictionary<string, string> _bushBloomItems;
		private readonly Dictionary<string, string> _customTeaBushItems;
		private readonly Dictionary<string, string> _ftmForageables;

		private BushBloomWrapper? _bushBloomWrapper;
		private CustomBushWrapper? _customBushWrapper;
		private FarmTypeManagerWrapper? _ftmWrapper;

		public Dictionary<string, string> CPForageables => _cpForageables;
		public Dictionary<string, string> CPFruitTrees => _cpFruitTrees;
		public Dictionary<string, string> CPWildTrees => _cpWildTrees;
		public Dictionary<string, string> BushBloomItems => _bushBloomItems;
		public Dictionary<string, string> CustomTeaBushItems => _customTeaBushItems;
		public Dictionary<string, string> FTMForageables => _ftmForageables;

		public BushBloomWrapper? BushBloomWrapper => _bushBloomWrapper;
		public CustomBushWrapper? CustomBushWrapper => _customBushWrapper;
		public FarmTypeManagerWrapper? FtmWrapper => _ftmWrapper;

		public ContentPackService(IMonitor monitor, IModHelper helper, ModConfig config)
		{
			_monitor = monitor;
			_helper = helper;
			_config = config;

			_cpForageables = [];
			_cpFruitTrees = [];
			_cpWildTrees = [];
			_bushBloomItems = [];
			_customTeaBushItems = [];
			_ftmForageables = [];
		}

		/// <summary>
		/// Loads all content packs and initalizes integrations.
		/// </summary>
		public async Task LoadAllContentAsync(List<string> ignoreItemIds)
		{
			var packs = _helper.ContentPacks.GetOwned();

			// Parse content packs
			ParseContentPacks(packs, ignoreItemIds);

			// Initialize integrations
			await InitializeBushBloomAsync();
			await InitializeCustomBushAsync();
			InitializeFarmTypeManager();
		}

		/// <summary>
		/// Parses content packs for custom forageable definitions.
		/// </summary>
		private void ParseContentPacks(IEnumerable<IContentPack> packs, List<string> ignoreItemIds)
		{
			foreach (var pack in packs)
			{
				if (pack is null) continue;

				try
				{
					var content = pack.ReadJsonFile<ContentEntry>(CONTENT_PACK_FILE_NAME);
					_monitor.LogOnce($"Found content pack: {pack.DirectoryPath}", _config.DebugLogLevel());

					if (content?.Forageables is not null)
					{
						ProcessForageables(content.Forageables, content.Category);
					}

					if (content?.FruitTrees is not null)
					{
						ProcessFruitTrees(content.FruitTrees, content.Category);
					}

					if (content?.WildTrees is not null)
					{
						ProcessWildTrees(content.WildTrees, content.Category);
					}

					if (content?.IgnoredItems is not null)
					{
						ProcessIgnoredItems(content.IgnoredItems, ignoreItemIds);
					}
				}
				catch
				{
					_monitor.Log(I18n.Log_ContentPack_LoadError(Path.Combine(pack.DirectoryPath, CONTENT_PACK_FILE_NAME)), LogLevel.Error);
				}
			}
		}

		/// <summary>
		/// Processes forageable items from content packs.
		/// </summary>
		private void ProcessForageables(List<string> forageables, string category)
		{
			foreach (var itemId in forageables)
			{
				if (_cpForageables.ContainsKey(itemId))
				{
					_monitor.LogOnce($"Already found an item with ItemId [{itemId}] with category [{_cpForageables[itemId]}] when trying to add category [{category}].", LogLevel.Trace);
				}
				else
				{
					_monitor.LogOnce($"Found content pack forageable: {itemId} - {category}", LogLevel.Trace);
					_cpForageables.Add(itemId, category);
				}
			}
		}

		/// <summary>
		/// Processes fruit tree items from content packs.
		/// </summary>
		private void ProcessFruitTrees(List<string> fruitTrees, string category)
		{
			foreach (var treeId in fruitTrees)
			{
				if (_cpFruitTrees.ContainsKey(treeId))
				{
					_monitor.LogOnce($"Already found a Fruit Tree with Id [{treeId}] with category [{_cpFruitTrees[treeId]}] when trying to add category [{category}].", LogLevel.Trace);
				}
				else
				{
					_cpFruitTrees.Add(treeId, category);
				}
			}
		}

		/// <summary>
		/// Processes wild tree items from content packs.
		/// </summary>
		private void ProcessWildTrees(List<string> wildTrees, string category)
		{
			foreach (var treeId in wildTrees)
			{
				if (_cpWildTrees.ContainsKey(treeId))
				{
					_monitor.LogOnce($"Already found a Wild Tree with Id [{treeId}] with category [{_cpWildTrees[treeId]}] when trying to add category [{category}].", LogLevel.Trace);
				}
				else
				{
					_cpWildTrees.Add(treeId, category);
				}
			}
		}

		/// <summary>
		/// Processes ignored items from content packs.
		/// </summary>
		private void ProcessIgnoredItems(List<string> ignoredItems, List<string> ignoreItemIds)
		{
			foreach (var itemId in ignoredItems)
			{
				var qualifiedItemId = itemId;

				if (!qualifiedItemId.StartsWith("(O)"))
				{
					qualifiedItemId = $"(O){qualifiedItemId}";
				}

				ignoreItemIds.AddDistinct(qualifiedItemId);
			}
		}

		/// <summary>
		/// Initializes the Bush Bloom mod integration.
		/// </summary>
		/// <returns></returns>
		private async Task InitializeBushBloomAsync()
		{
			_bushBloomWrapper = new BushBloomWrapper(_monitor, _helper);
			var schedules = await _bushBloomWrapper.UpdateSchedules();

			foreach (var sched in schedules)
			{
				var itemId = ItemUtilities.GetItemIdFromName(sched.ItemId);

				if (itemId is not null)
				{
					if (_bushBloomItems.ContainsKey(itemId))
					{
						_monitor.LogOnce($"Already found an item with ItemId [{itemId}] with category [{_bushBloomItems[itemId]}] when trying to add category [{I18n.Category_BushBlooms()}]. Please verify you don't have duplicate or conflicting content packs.", LogLevel.Info);
					}
					else
					{
						_monitor.LogOnce($"Found Bush Bloom Schedule for: [{itemId}]", _config.DebugLogLevel());
						_bushBloomItems.Add(itemId, "Category.BushBlooms");
					}
				}
			}
		}

		/// <summary>
		/// Initializes the Custom Bush mod integration.
		/// </summary>
		private async Task InitializeCustomBushAsync()
		{
			_customBushWrapper = new CustomBushWrapper(_monitor, _helper);
			var customBushDrops = await _customBushWrapper.GetDrops();

			foreach (var drop in customBushDrops)
			{
				var itemId = ItemUtilities.GetItemIdFromName(drop);

				if (itemId is not null)
				{
					if (_customTeaBushItems.ContainsKey(itemId))
					{
						_monitor.LogOnce($"Already found an item with ItemID [{itemId}] with category [{_customTeaBushItems[itemId]}] when trying to add category [{I18n.Category_CustomBushes()}]. Please verify you don't have duplicate or conflicting content packs.", LogLevel.Info);
					}
					else
					{
						_monitor.LogOnce($"Found Custom Bush for: [{itemId}]", _config.DebugLogLevel());
						_customTeaBushItems.Add(itemId, "Category.Custombushes");
					}
				}
			}
		}

		/// <summary>
		/// Initializes the Farm Type Manager mod integration.
		/// </summary>
		private void InitializeFarmTypeManager()
		{
			_ftmWrapper = new FarmTypeManagerWrapper(_monitor, _helper);
			var ftmForageables = _ftmWrapper.UpdateForageIds();
			var logLevel = _config.DebugLogLevel();

			foreach (var kvp in ftmForageables)
			{
				var cpUniqueName = kvp.Key;
				var cpName = kvp.Key;
				var cpMod = _helper.ModRegistry.Get(cpUniqueName);

				if (cpMod is not null)
				{
					cpName = cpMod.Manifest.Name;
				}

				_monitor.Log($"{cpName} - {cpUniqueName}", logLevel);

				foreach (var value in kvp.Value)
				{
					var itemId = ItemUtilities.GetItemIdFromName(value) ?? value;

					if (_ftmForageables.ContainsKey(itemId))
					{
						_monitor.Log($"\tFound repeat forageable: {itemId}", logLevel);
					}
					else
					{
						_monitor.Log($"\tFound new forageable: {itemId}", logLevel);
						_ftmForageables.Add(itemId, cpName);
					}
				}
			}
		}
	}
}
