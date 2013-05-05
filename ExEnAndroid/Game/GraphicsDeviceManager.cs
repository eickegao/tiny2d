using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
	public partial class GraphicsDeviceManager : IGraphicsDeviceManager, IGraphicsDeviceService, IDisposable
	{
		public static readonly int DefaultBackBufferWidth = 480;
		public static readonly int DefaultBackBufferHeight = 800;
		
	
		#region Game Startup

		void IGraphicsDeviceManager.CreateDevice()
		{
			throw new NotSupportedException("CreateDevice is not supported on ExEn's GraphicsDeviceManager");
		}
		
		private ExEnAndroidActivity activity;
		private ExEnAndroidSurfaceView surfaceView;
		
		public GraphicsDevice GraphicsDevice { get; private set; }
		
		
		internal void InternalDeviceReady(ExEnAndroidSurfaceView gameView,
				int width, int height, ExEnInterfaceOrientation orientation)
		{
			Point size = new Point(width, height);
			
			if(GraphicsDevice == null)
			{
				GraphicsDevice = new GraphicsDevice(new ExEnScaler(orientation, size, size));
				OnDeviceCreated(this, EventArgs.Empty);
			}
			else
			{
				// NOTE: Should the graphics device scaler be updated here?
			}
		}
		

		internal void StartGame(ExEnAndroidActivity activity)
		{
			if(activity.surface != null)
				throw new InvalidOperationException("Game has already been started");
			
			this.activity = activity;
			
			// Note: If the handling of the graphics device gets more complicated,
			//       this may have to be split into functions around what needs to be done before,
			//       during and after graphics device creation:
			ApplyChanges();
			
			// Create the game's view:
			surfaceView = new ExEnAndroidSurfaceView(game, this, activity);
			activity.surface = surfaceView;

			// Start the game running in the rendering thread
			surfaceView.StartGame();
			
			// Now that Game has started, make the view visible:
			activity.SetContentView(surfaceView);
			
			// Return outwards to the activity's OnCreate method...
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
			
			// Set Android requested orientations
			bool portrait = (appliedSupportedOrientations & DisplayOrientation.Portrait) != 0;
			bool landscapeLeft = (appliedSupportedOrientations & DisplayOrientation.LandscapeLeft) != 0;
			bool landscapeRight = (appliedSupportedOrientations & DisplayOrientation.LandscapeRight) != 0;
			// BUG: Android makes it difficult to support both landscape orientations... 
			//      so for the time being, ExEn for Android doesn't either.
			if(landscapeLeft != landscapeRight)
				ExEnLog.WriteLine("WARNING: ExEn for Android cannot handle mismatched landscape modes");
			bool landscape = landscapeLeft || landscapeRight;
			if(portrait == landscape) // both (or neither, which shouldn't happen)
			{
				//activity.SetRequestedOrientation(Android.Content.PM.ScreenOrientation.Sensor);
			}
			else if(landscape)
			{
				// NOTE: Would prefer to use sensorLandscape, but that is only introduced in API level 9
				// TODO: If adding support for different API versions, add a conditional that uses
				//       sensorLandscape on API level 9+
				//activity.SetRequestedOrientation(Android.Content.PM.ScreenOrientation.Landscape);
			}
			else // portrait
			{
				// As above, would prefer to use sensorPortrait, but this requires API level 9
				// I'm fairly sure that a phone-style device will treat sensorPortrait as portrait
				// (no upside-down-portrait), and a pad will support it fully. This matches the
				// difference between iPhone and iPad.
				//activity.SetRequestedOrientation(Android.Content.PM.ScreenOrientation.Portrait);
			}
		}

		#endregion
		

		#region Drawing

		public bool BeginDraw()
		{
			return true;
		}

		public void EndDraw()
		{
			surfaceView.SwapBuffers();
		}
		
		#endregion
		
	}
}

