using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
	public partial class GraphicsDeviceManager : IGraphicsDeviceManager, IGraphicsDeviceService, IDisposable
	{

		#region Configuration Properties

		public bool IsFullScreen { get; set; }
		public int PreferredBackBufferWidth { get; set; }
		public int PreferredBackBufferHeight { get; set; }
		public DisplayOrientation SupportedOrientations { get; set; }

		// Not Implemented:
		//public SurfaceFormat PreferredBackBufferFormat { get; set; }
		//public DepthFormat PreferredDepthStencilFormat { get; set; }
		//public bool PreferMultiSampling { get; set; }
		//public bool SynchronizeWithVerticalRetrace { get; set; }


		private void SetDefaultProperties()
		{
			IsFullScreen = false;
			PreferredBackBufferWidth = DefaultBackBufferWidth;
			PreferredBackBufferHeight = DefaultBackBufferHeight;
			SupportedOrientations = DisplayOrientation.Default;
		}

		#endregion


		#region Creation (create link to Game)

		readonly Game game;

		public GraphicsDeviceManager(Game game)
		{
			if(game == null)
				throw new ArgumentNullException("game");
			if(game.Services.GetService(typeof(IGraphicsDeviceService)) != null)
				throw new InvalidOperationException("Instance of Game already has an IGraphicsDeviceService");

			SetDefaultProperties();
			IsFullScreen = true; // Override the default

			// Attach to Game
			this.game = game;
			game.Services.AddService(typeof(IGraphicsDeviceManager), this);
			game.Services.AddService(typeof(IGraphicsDeviceService), this);
			game.graphicsDeviceManager = this; // Special ExEn device hookup
		}

		#endregion


		#region Disposal

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(GraphicsDevice != null)
				{
					GraphicsDevice.Dispose();
					GraphicsDevice = null;
				}

				if(Disposed != null)
					Disposed(this, EventArgs.Empty);
			}
		}

		public event EventHandler<EventArgs> Disposed;

		#endregion


		#region Events

		public event EventHandler<EventArgs> DeviceCreated;
		protected virtual void OnDeviceCreated(object sender, EventArgs args)
		{
			if(DeviceCreated != null)
				DeviceCreated(sender, args);
		}

		public event EventHandler<EventArgs> DeviceDisposing;
		protected virtual void OnDeviceDisposing(object sender, EventArgs args)
		{
			if(DeviceDisposing != null)
				DeviceDisposing(sender, args);
		}

		public event EventHandler<EventArgs> DeviceReset;
		protected virtual void OnDeviceReset(object sender, EventArgs args)
		{
			if(DeviceReset != null)
				DeviceReset(sender, args);
		}

		public event EventHandler<EventArgs> DeviceResetting;
		protected virtual void OnDeviceResetting(object sender, EventArgs args)
		{
			if(DeviceResetting != null)
				DeviceResetting(sender, args);
		}

		#endregion


		#region ToggleFullScreen

		public void ToggleFullScreen()
		{
			// Even though the method name and documentation imply that this
			// only toggles the full-screen state, it actually does this:
			IsFullScreen = !IsFullScreen;
			ApplyChanges();
		}

		#endregion

	}
}
