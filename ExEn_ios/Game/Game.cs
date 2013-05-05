using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ExEn;
using MonoTouch.UIKit;


namespace Microsoft.Xna.Framework
{
	public partial class Game : IDisposable
	{
		protected internal bool iOSFasterStartup = true;


		#region Game Startup

		public Game()
		{
			BuiltInLoaders.Register();
			
			AudioSessionManager.Setup();
			UIApplication.SharedApplication.StatusBarHidden = true;

			// TODO: What is the correct startup state for Window?
			this.Window = new GameWindow();
			var bounds = UIScreen.MainScreen.Bounds;
			Window.ClientBounds = new Rectangle(0, 0, (int)bounds.Width, (int)bounds.Height);
			this.Content = new ContentManager(services);
		}

		public void Run()
		{
			// The graphics device manager will hopefully have been created by the derived class's constructor
			// It must be a GraphicsDeviceManager because it holds an ExEnEmTouchGameView that also handles our Draw/Update loop
			if(graphicsDeviceManager == null)
				throw new InvalidOperationException("Game requires that a GraphicsDeviceManager is created before calling Run");

			// Start the game (special ExEnEmTouch handling for startup, rather than going through CreateDevice)
			graphicsDeviceManager.StartGame();
		}

		internal void DoStartup()
		{
			Initialize();
			BeginRun();
		}

		#endregion


		#region Backgrounding (iOS Only)

		internal void DoEnterForeground()
		{
			OnEnterForeground(this, EventArgs.Empty);
			graphicsDeviceManager.gameView.Resume(); // Restart OpenGL
		}

		internal void DoEnterBackground()
		{
			graphicsDeviceManager.gameView.Pause(); // Prevent OpenGL from doing anything
			OnEnterBackground(this, EventArgs.Empty);
		}

		public event EventHandler EnterForeground;
		protected virtual void OnEnterForeground(object sender, EventArgs args)
		{
			if(EnterForeground != null)
				EnterForeground(sender, args);
		}

		public event EventHandler EnterBackground;
		protected virtual void OnEnterBackground(object sender, EventArgs args)
		{
			if(EnterBackground != null)
				EnterBackground(sender, args);
		}

		#endregion

	}
}
