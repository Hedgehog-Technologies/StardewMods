using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using HedgeTech.Common.Classes;
using HedgeTech.Common.Interfaces;

namespace AutoTrasher
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	public class ModConfig
	{
		private const string _gmcmUniqueId = "spacechase0.GenericModConfigMenu";

		private const int _minReclaimItems = 5;
		private const int _maxReclaimItems = 100;

		private IModHelper? _helper;

		public KeybindList ToggleTrasherKeybind { get; set; }
		public KeybindList OpenTrashMenuKeybind { get; set; }
		public KeybindList AddTrashKeybind { get; set; }

		private int _reclaimableItemCount;
		public int ReclaimableItemCount
		{
			get => _reclaimableItemCount;
			set => _reclaimableItemCount = Math.Clamp(value, _minReclaimItems, _maxReclaimItems);
		}

		public bool AllowAllItems { get; set; }
		public List<string> TrashList { get; set; }

		public ModConfig()
		{
			ResetToDefault();

			TrashList = new List<string>
			{
				"(O)168", // Trash
				"(O)169", // Driftwood
				"(O)170", // Broken Glasses
				"(O)171", // Broken CD
				"(O)172", // Soggy Newspaper
				"(O)747", // Rotten Plant
				"(O)748" // Rotten Plant
			};
		}

		private void ResetToDefault()
		{
			ToggleTrasherKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.R),
				new Keybind(SButton.RightAlt, SButton.R));

			OpenTrashMenuKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.L),
				new Keybind(SButton.RightAlt, SButton.L));

			AddTrashKeybind = new KeybindList(
				new Keybind(SButton.LeftAlt, SButton.X),
				new Keybind(SButton.RightAlt, SButton.X));

			AllowAllItems = false;

			ReclaimableItemCount = 10;
		}

		public void Initialize(IModHelper helper)
		{
			_helper = helper;

			foreach (var item in new List<string>(TrashList))
			{
				if (!item.StartsWith('('))
				{
					TrashList.Remove(item);
				}
			}

			helper.WriteConfig(this);
		}

		public void RemoveTrashItemFromTrashList(string itemId)
		{
			TrashList.Remove(itemId);
			_helper?.WriteConfig(this);
		}

		public void AddTrashItemToTrashList(string itemId)
		{
			TrashList.Add(itemId);
			_helper?.WriteConfig(this);
		}

		public void RegisterModConfigMenu(IModHelper helper, IManifest manifest, LimitedList<Item> reclaimItems)
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
				name: I18n.Option_ToggleTrasherKeybind_Name,
				tooltip: I18n.Option_ToggleTrasherKeybind_Tooltip,
				getValue: () => ToggleTrasherKeybind,
				setValue: val => ToggleTrasherKeybind = val);

			gmcmApi.AddKeybindList(
				mod: manifest,
				name: I18n.Option_OpenTrashMenuKeybind_Name,
				tooltip: I18n.Option_OpenTrashMenuKeybind_Tooltip,
				getValue: () => OpenTrashMenuKeybind,
				setValue: val => OpenTrashMenuKeybind = val);

			gmcmApi.AddKeybindList(
				mod: manifest,
				name: I18n.Option_AddTrashKeybind_Name,
				tooltip: I18n.Option_AddTrashKeybind_Tooltip,
				getValue: () => AddTrashKeybind,
				setValue: val => AddTrashKeybind = val);

			gmcmApi.AddNumberOption(
				mod: manifest,
				name: I18n.Option_ReclaimableItemCount_Name,
				tooltip: I18n.Option_ReclaimableItemCount_Tooltip,
				min: _minReclaimItems,
				max: _maxReclaimItems,
				getValue: () => ReclaimableItemCount,
				setValue: (val) =>
				{
					ReclaimableItemCount = val;
					reclaimItems.UpdateMaxSize(ReclaimableItemCount);
				});

			gmcmApi.AddBoolOption(
				mod: manifest,
				name: I18n.Option_AllowAllItems_Name,
				tooltip: I18n.Option_AllowAllItems_Tooltip,
				getValue: () => AllowAllItems,
				setValue: (val) => AllowAllItems = val);
		}
	}
}
