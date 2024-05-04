using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace AutoTrasher.Components.Elements
{
	internal class TrashOptionsButton<TButton> : BaseOptionsElement
		where TButton : TrashOptionsButton<TButton>
	{
		private readonly Action<TButton> Toggle;
		private readonly Rectangle RemoveButtonSprite;
		private Rectangle RemoveButtonBounds;

		public TrashOptionsButton(string label, int slotWidth, Action<TButton> toggle, bool disabled = false)
			: base(label, -1, -1, slotWidth + 1, 11 * Game1.pixelZoom)
		{
			RemoveButtonSprite = new(269, 471, 14, 15);
			RemoveButtonBounds = new Rectangle(slotWidth - 28 * Game1.pixelZoom, -1 + Game1.pixelZoom * 3, 14 * Game1.pixelZoom, 15 * Game1.pixelZoom);
			Toggle = toggle;
			greyedOut = disabled;
		}

		public TrashOptionsButton(string label, int slotWidth, Action toggle, bool disabled = false)
			: this(label, slotWidth, _ => toggle(), disabled)
		{ }

		public override void receiveLeftClick(int x, int y)
		{
			if (greyedOut || RemoveButtonBounds.Contains(x, y)) return;

			Toggle((TButton)this);
		}

		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			DrawElement(b, slotX, slotY, context);
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(RemoveButtonBounds.X + slotX, RemoveButtonBounds.Y + slotY), RemoveButtonSprite, Color.White, 0.0f, Vector2.Zero, Game1.pixelZoom * 0.75f, false, 0.15f);
		}

		protected virtual void DrawElement(SpriteBatch b, int slotX, int slotY, IClickableMenu? context = null)
		{
			Utility.drawTextWithShadow(b, label, Game1.dialogueFont, new Vector2(bounds.X + slotX, bounds.Y + slotY), greyedOut ? Game1.textColor * 0.33f : Game1.textColor, 1f, 0.15f);
		}
	}

	internal class TrashOptionsButton : TrashOptionsButton<TrashOptionsButton>
	{
		public TrashOptionsButton(string label, int slotWidth, Action<TrashOptionsButton> toggle, bool disabled = false)
			: base(label, slotWidth, toggle, disabled)
		{ }

		public TrashOptionsButton(string label, int slotWidth, Action toggle, bool disabled = false)
			: base(label, slotWidth, _ => toggle(), disabled)
		{ }
	}
}
