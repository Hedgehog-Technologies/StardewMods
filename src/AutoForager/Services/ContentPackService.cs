using AutoForager.Integrations;
using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace AutoForager.Services
{
	/// <summary>
	/// LEGACY: Manages content packs loading and integration with other mods.
	/// TECH_DEBT: Remove this class in favor of direct integration with other mods via their APIs
	/// </summary>
	[Obsolete("This class is no longer used. Content pack loading and integration is now handled by the respective integration classes.")]
	internal class ContentPackService
	{
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
	}
}
