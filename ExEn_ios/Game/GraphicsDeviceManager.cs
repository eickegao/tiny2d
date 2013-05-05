using System;
using Microsoft.Xna.Framework.Graphics;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework
{
	public partial class GraphicsDeviceManager : IGraphicsDeviceManager, IGraphicsDeviceService, IDisposable
	{
		public static readonly int DefaultBackBufferWidth = 320;
		public static readonly int DefaultBackBufferHeight = 480;
		

		#region Game Startup

		void IGraphicsDeviceManager.CreateDevice()
		{
			throw new NotSupportedException("CreateDevice is not supported on ExEn's GraphicsDeviceManager");
		}

		private UIWindow mainWindow;
		private ExEnEmTouchGameViewController gameViewController;
		internal ExEnEmTouchGameView gameView;
		public GraphicsDevice GraphicsDevice { get; internal set; }

		internal void InternalCreateDevice(ExEnEmTouchGameView gameView)
		{
			// Switching from Portrait will be handled by ExEnEmTouchGameViewController
			ExEnScaler scaler = new ExEnScaler(ExEnInterfaceOrientation.Portrait, gameView.RenderbufferSize, gameView.DeviceSize);
			GraphicsDevice = new GraphicsDevice(scaler);
			OnDeviceCreated(this, EventArgs.Empty);
		}

		internal void StartGame()
		{
			// Note: If the handling of the graphics device gets more complicated,
			//       this may have to be split into functions around what needs to be done before,
			//       during and after graphics device creation:
			ApplyChanges();

			// Create the game's window and view:
			mainWindow = new UIWindow(UIScreen.MainScreen.Bounds);
			gameViewController = new ExEnEmTouchGameViewController(game, this);
			gameView = (ExEnEmTouchGameView)gameViewController.View;

			// Start the game
			gameView.StartGame();
			// Calling StartGame does this:
			//  - creates the frame buffer
			//  - Calls InternalCreateDevice on this (creating the graphics device and calling DoDeviceCreated)
			//  - calls Initialize and then Update/Draw on Game (to fill backbuffer before becoming visible)

			// Now that Game has started, make the window visible:
			mainWindow.Add(gameView);
			mainWindow.MakeKeyAndVisible();
		}

		#endregion


		#region Applied Device Settings

		// Device settings that have been applied with Apply Changes
		internal DisplayOrientation appliedSupportedOrientations;

		#endregion


		#region Modify device settings

		private void ThrowIfNotService()
		{
			if(!ReferenceEquals(game.Services.GetService(typeof(IGraphicsDeviceService)), this))
				throw new InvalidOperationException("Graphics device settings cannot be applied, because this GraphicsDeviceManager is not the Game's IGraphicsDeviceService provider");
		}

		public void ApplyChanges()
		{
			ThrowIfNotService();

			// Store software settings
			appliedSupportedOrientations = SupportedOrientations;
			if(appliedSupportedOrientations == DisplayOrientation.Default)
			{
				if(PreferredBackBufferHeight > PreferredBackBufferWidth)
					appliedSupportedOrientations = DisplayOrientation.Portrait;
				else
					appliedSupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
			}

			Console.WriteLine("Setting supported orientations = " + appliedSupportedOrientations);

			// Not supporting this for the time being...
			// UIApplication.SharedApplication.StatusBarHidden = IsFullScreen;
		}

		#endregion


		#region Drawing

		public bool BeginDraw()
		{
			return true;
		}

		public void EndDraw()
		{
			gameView.SwapBuffers();
		}

		#endregion

	}
}
