using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public interface IGraphicsDeviceService
	{
		GraphicsDevice GraphicsDevice { get; }

		event EventHandler<EventArgs> DeviceCreated;
		event EventHandler<EventArgs> DeviceDisposing;
		event EventHandler<EventArgs> DeviceReset;
		event EventHandler<EventArgs> DeviceResetting;
	}
}
