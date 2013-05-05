using System;

namespace Microsoft.Xna.Framework
{
	public interface IUpdateable
	{
		bool Enabled { get; }
		int UpdateOrder { get; }

		event EventHandler EnabledChanged;
		event EventHandler UpdateOrderChanged;

		void Update(GameTime gameTime);
	}
}
