using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
	public partial class Game
	{
		#region Game Startup

		public Game()
		{
			BuiltInLoaders.Register();
			SoundEffect.Setup();
			
			// TODO BUG: set Window size correctly!
			this.Window = new GameWindow();
			
			this.Content = new ContentManager(services);
		}
		
		
		public void Start(ExEnAndroidActivity activity)
		{
			// The graphics device manager will hopefully have been created by the derived class's constructor
			// It must be a GraphicsDeviceManager because it holds an ExEnAndroidSurfaceView that also handles our Draw/Update loop
			if(graphicsDeviceManager == null)
				throw new InvalidOperationException("Game requires that a GraphicsDeviceManager is created before calling Start");
			
			// Add the activity as a service (used by ContentManager)
			this.services.AddService(typeof(ExEnAndroidActivity), activity);
			
			// Start the game
			graphicsDeviceManager.StartGame(activity);
		}

		internal void DoStartup()
		{
			Initialize();
			BeginRun();
		}

		#endregion
		
	}
}

