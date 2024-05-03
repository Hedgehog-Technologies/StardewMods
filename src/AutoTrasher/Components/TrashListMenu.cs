using System.Collections.Generic;
using AutoTrasher.Components.Elements;
using HedgeTech.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace AutoTrasher.Components
{
	internal class TrashListMenu : IClickableMenu
	{
		private const int ButtonBorderWidth = 4 * Game1.pixelZoom;
		private const int ItemsPerPage = 10;

		private readonly IMonitor _monitor;
		private readonly ModConfig _config;

		private readonly List<ClickableComponent> _optionSlots;
		private readonly List<OptionsElement> _options;
		private ClickableComponent _title;
		private ClickableTextureComponent _upArrow;
		private ClickableTextureComponent _downArrow;
		private ClickableTextureComponent _scrollbar;
		private Rectangle _scrollbarRunner;

		private string _hoverText;

		private TitleMenu REMOVEME;
		private CharacterCustomization REMOVEME2;

		public TrashListMenu(IMonitor monitor, ModConfig config)
		{
			_monitor = monitor;
			_config = config;

			_optionSlots = new();
			_options = new();

			_hoverText = string.Empty;

			Game1.playSound("bigSelect");

			ResetComponents();
			SetOptions();
		}

		public override void draw(SpriteBatch b)
		{
			if (!Game1.options.showMenuBackground)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			}

			base.draw(b);

			// Draw Menu Box
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

			// Draw Title Box
			{
				var bounds = Game1.dialogueFont.MeasureString(_title.name);
				var outerWidth = (int)bounds.X + ButtonBorderWidth * 2;
				var outerHeight = (int)bounds.Y + Game1.tileSize / 3;
				var offsetX = -outerWidth / 2;

				drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), _title.bounds.X + offsetX, _title.bounds.Y, outerWidth, outerHeight + Game1.tileSize / 16, Color.White * 1f, drawShadow: true);

				var innerDrawPosition = new Vector2(_title.bounds.X + ButtonBorderWidth + offsetX, _title.bounds.Y + ButtonBorderWidth);
				Utility.drawTextWithShadow(b, _title.name, Game1.dialogueFont, innerDrawPosition, Game1.textColor);
			}

			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

			if (!GameMenu.forcePreventClose)
			{
				// Draw Scrollbar when scrolling can happen
				if (_options.Count > ItemsPerPage)
				{
					_upArrow.draw(b);
					_downArrow.draw(b);
					drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), _scrollbarRunner.X, _scrollbarRunner.Y, _scrollbarRunner.Width, _scrollbarRunner.Height, Color.White, Game1.pixelZoom, false);
					_scrollbar.draw(b);
				}
			}

			if (_hoverText != string.Empty)
			{
				drawHoverText(b, _hoverText, Game1.smallFont);
			}

			// Draw mouse / cursor
			if (!Game1.options.hardwareCursor)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.getOldMouseX(), Game1.getOldMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16), Color.White, 0f, Vector2.Zero, Game1.pixelZoom + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
			}
		}

		private void ResetComponents()
		{
			width = 800 + borderWidth * 2;
			height = 600 + borderWidth * 2;
			xPositionOnScreen = Game1.uiViewport.Width / 2 - (width - (int)(Game1.tileSize / 2.4f)) / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2;

			// TITLE
			_title = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2, yPositionOnScreen, Game1.tileSize * 4, Game1.tileSize), "Trash List");

			// SCROLL UI
			var scrollbarOffset = Game1.tileSize * 4 / 16;
			_upArrow = new ClickableTextureComponent("up-arrow", new Rectangle(xPositionOnScreen + width + scrollbarOffset, yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
			_downArrow = new ClickableTextureComponent("down-arrow", new Rectangle(xPositionOnScreen + width + scrollbarOffset, yPositionOnScreen + height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);
			_scrollbar = new ClickableTextureComponent("scrollbar", new Rectangle(_upArrow.bounds.X + Game1.pixelZoom * 3, _upArrow.bounds.Y + _upArrow.bounds.Height + Game1.pixelZoom, 6 * Game1.pixelZoom, 10 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(435, 463, 6, 10), Game1.pixelZoom);
			_scrollbarRunner = new Rectangle(_scrollbar.bounds.X, _upArrow.bounds.Y + _upArrow.bounds.Height + Game1.pixelZoom, _scrollbar.bounds.Width, height - Game1.tileSize * 2 - _upArrow.bounds.Height - Game1.pixelZoom * 2);
			//SetScrollbarToCurrentIndex();

			// OPTION SLOTS
			_optionSlots.Clear();
			for (int i = 0; i < ItemsPerPage; i++)
			{
				_optionSlots.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + Game1.tileSize / 4, yPositionOnScreen + Game1.tileSize * 5 / 4 + Game1.pixelZoom + i * ((height - Game1.tileSize * 2) / ItemsPerPage), width - Game1.tileSize / 2, (height - Game1.tileSize * 2) / ItemsPerPage + Game1.pixelZoom), string.Concat(i)));
			}
		}

		private void SetOptions()
		{
			var slotWidth = _optionSlots[0].bounds.Width;

			_options.Clear();
			foreach (var item in _config.TrashItems)
			{
				_options.Add(new TrashOptionsButton(
					label: ItemUtilities.GetItemNameFromId(item) ?? item,
					slotWidth: slotWidth,
					toggle: () => RemoveTrashItem(item)));
			}
		}

		private void RemoveTrashItem(string itemId)
		{
			_config.RemoveTrashItem(itemId);
			ResetComponents();
			SetOptions();
		}

		private void SetScrollbarToCurrentIndex()
		{ }
	}
}
