using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
	public class DrawableGameComponent : GameComponent, IDrawable
	{
		public DrawableGameComponent(Game game) : base(game) { }

		public GraphicsDevice GraphicsDevice { get { return Game.GraphicsDevice; } }


		#region Visible

		private bool _visible = true;
		public bool Visible
		{
			get { return _visible; }
			set
			{
				if(_visible != value)
				{
					_visible = value;
					OnVisibleChanged(this, EventArgs.Empty);
				}
			}
		}

		protected virtual void OnVisibleChanged(object sender, EventArgs args)
		{
			if(VisibleChanged != null)
				VisibleChanged(sender, args);
		}

		public event EventHandler VisibleChanged;

		#endregion


		#region Draw Order

		private int _drawOrder = 0;
		public int DrawOrder
		{
			get { return _drawOrder; }
			set
			{
				if(_drawOrder != value)
				{
					_drawOrder = value;
					OnDrawOrderChanged(this, EventArgs.Empty);
				}
			}
		}

		protected virtual void OnDrawOrderChanged(object sender, EventArgs args)
		{
			if(DrawOrderChanged != null)
				DrawOrderChanged(sender, args);
		}

		public event EventHandler DrawOrderChanged;

		#endregion


		#region Protected Virtual Functions

		public virtual void Draw(GameTime gameTime) { }

		protected virtual void LoadContent() { }
		protected virtual void UnloadContent() { }

		#endregion


		#region Overrides

		public override void Initialize()
		{
			LoadContent();
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				UnloadContent();
			}
			base.Dispose(disposing);
		}

		#endregion

	}
}
