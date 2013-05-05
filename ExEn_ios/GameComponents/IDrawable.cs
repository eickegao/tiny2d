using System;

namespace Microsoft.Xna.Framework
{
	public interface IDrawable
	{
		int DrawOrder { get; }
		bool Visible { get; }

		event EventHandler DrawOrderChanged;
		event EventHandler VisibleChanged;

		void Draw(GameTime gameTime);
	}
}
