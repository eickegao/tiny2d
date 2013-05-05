using System;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using Javax.Microedition.Khronos.Egl;

using Android.Views;
using Android.Content;
using Android.Runtime;

using ExEnCore;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Framework
{
	[Serializable]
	public class ExEnSurfaceException : Exception
	{
		public ExEnSurfaceException() { ExEnLog.WriteLine("ExEnSurfaceException created"); }
		public ExEnSurfaceException(string message) : base(message) { ExEnLog.WriteLine("ExEnSurfaceException: " + message); }
		public ExEnSurfaceException(string message, Exception inner) : base(message, inner) { ExEnLog.WriteLine("ExEnSurfaceException: " + message); }
		protected ExEnSurfaceException(
				System.Runtime.Serialization.SerializationInfo info,
				System.Runtime.Serialization.StreamingContext context) : base(info, context) { ExEnLog.WriteLine("ExEnSurfaceException created"); }
	}
	
	
	public class ExEnAndroidSurfaceView : SurfaceView, ISurfaceHolderCallback
	{
		Game game;
		GraphicsDeviceManager gdm;
		ExEnAndroidActivity activity;
		
		GameLoop GameLoop { get { return game.gameLoop; } }
		
		public ExEnAndroidSurfaceView(Game game, GraphicsDeviceManager gdm, ExEnAndroidActivity activity) : base(activity)
		{
			this.game = game;
			this.gdm = gdm;
			this.activity = activity;
			
			inputScaler = new ExEnScaler(ExEnInterfaceOrientation.Portrait, new Point(1, 1), new Point(1, 1));
			
			Holder.AddCallback(this);
			Holder.SetType(SurfaceType.Gpu);
			
			updateGameTime = new GameTime();
			drawGameTime = new GameTime();
			GameLoop.Update = DoUpdate;
			GameLoop.Draw = DoDraw;
		}
		
		
		#region Calls to Game object
		
		private GameTime updateGameTime;
		
		void DoUpdate(TimeSpan time)
		{
			updateGameTime.Update(time);
			game.Update(updateGameTime);
		}
		
		private GameTime drawGameTime;
		
		void DoDraw(TimeSpan time)
		{
			drawGameTime.Update(time);
			game.DoDraw(drawGameTime);
		}
		
		#endregion
		
		
		
		#region Cross-Thread Communication
		
		object lockObject = new object();
		
		// True if the Android SurfaceView surface is available (SurfaceCreated/SurfaceDestroyed)
		bool surfaceAvailable;
		
		// True if the Android SurfaceView has a pending state change that needs to be handled
		bool surfaceChanged;
		// The data associated with a surface change. Read-only in game thread, read/write in UI thread.
		int surfaceWidth, surfaceHeight;
		ExEnInterfaceOrientation orientation;
		
		// True if the Android Activity is in a paused state (ActivityPaused/ActivityResumed)
		bool activityPaused;
		
		// True if the OpenGL ES surface/context has been set up
		bool eglSurfaceAvailable;
		bool eglContextAvailable;
		
		
		#region Helpers
		
		static ExEnInterfaceOrientation ConvertOrientation(int width, int height, SurfaceOrientation orientation)
		{
			if(width > height) // Current orientation is landscape
			{
				switch(orientation)
				{
				case SurfaceOrientation.Rotation0: // natural landscape device
				case SurfaceOrientation.Rotation90:
					return ExEnInterfaceOrientation.LandscapeRight;
				case SurfaceOrientation.Rotation180: // natural landscape device
				case SurfaceOrientation.Rotation270:
					return ExEnInterfaceOrientation.LandscapeLeft;
				}
			}
			else // Current orientation is portrait (this is probably a bug on square screens - but who uses those?)
			{
				switch(orientation)
				{
				case SurfaceOrientation.Rotation0:
				case SurfaceOrientation.Rotation270: // natural landscape device
					return ExEnInterfaceOrientation.Portrait;
				case SurfaceOrientation.Rotation180:
				case SurfaceOrientation.Rotation90: // natural landscape device
					return ExEnInterfaceOrientation.PortraitUpsideDown;
				}
			}
			
			// This line should never be reached
			System.Diagnostics.Debug.Assert(false);
			return ExEnInterfaceOrientation.Portrait;
		}
		
		#endregion
		
		#endregion
		
		
		#region UI Thread Events
		
		// As ISurfaceHolderCallback
		public void SurfaceCreated(ISurfaceHolder holder)
		{
			lock(lockObject)
			{
				ExEnLog.WriteLine("ExEnAndroidSurfaceView.SurfaceCreated");
				
				// Set initial display data
				surfaceWidth = Width;
				surfaceHeight = Height;
				orientation = ConvertOrientation(Width, Height,
						(SurfaceOrientation)activity.WindowManager.DefaultDisplay.Orientation);
				
				surfaceAvailable = true;
				
				Monitor.PulseAll(lockObject); // Signal game thread to reevaluate surface state
			}
			
			UpdateInputScaler();
		}
		
		// As ISurfaceHolderCallback
		public void SurfaceDestroyed(ISurfaceHolder holder)
		{
			lock(lockObject)
			{
				ExEnLog.WriteLine("ExEnAndroidSurfaceView.SurfaceDestroyed");
				
				surfaceAvailable = false;
				
				// Wait for the game thread to stop rendering and give up the EGL surface before returning
				// (because, after returning, Android will destroy the backing surface)
				Monitor.PulseAll(lockObject); // Signal game thread to reevaluate surface state
				while(eglSurfaceAvailable)
				{
					Monitor.Wait(lockObject);
				}
				
				ExEnLog.WriteLine("ExEnAndroidSurfaceView.SurfaceDestroyed Completed");
			}
		}
		
		// As ISurfaceHolderCallback
		public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
		{
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.SurfaceChanged(format = " + format
					+ ", width = " + width + ", height = " + height + ")");
			
			lock(lockObject)
			{
				// Update display data
				surfaceChanged = true;
				surfaceWidth = width;
				surfaceHeight = height;
				
				// Android sillyness notes:
				// - getOrientation gets deprecated and renamed to getRotation in API level 8
				// - Rotation is relative to the "natural" device orientation
				orientation = ConvertOrientation(surfaceWidth, surfaceHeight,
						(SurfaceOrientation)activity.WindowManager.DefaultDisplay.Orientation);
				
				// TODO, BUG:
				// If the device is flipped 180 degrees, Android is "helpful" by just
				//   pretending nothing happened and silently flipping your display surface
				//   I'm yet to find an event (that isn't a private API) that I can hook
				//   to get a notification when this happens. So for now, simply assume that the
				//   client game code doesn't really care about the difference between landscape-left/-right.
				// 
				// Here's a Stack Overflow question, in case a solution is found:
				// http://stackoverflow.com/questions/7329823/android-event-for-all-interface-orientation-changes
			}
			
			UpdateInputScaler();
		}
		
		// Called from ExEnAndroidActivity
		public void ActivityPaused()
		{
			lock(lockObject)
			{
				ExEnLog.WriteLine("ExEnAndroidSurfaceView.ActivityPaused");
				
				activityPaused = true;
				Monitor.PulseAll(lockObject); // Signal game thread to reevaluate surface state
			}
		}
		
		// Called from ExEnAndroidActivity
		public void ActivityResumed()
		{
			lock(lockObject)
			{
				ExEnLog.WriteLine("ExEnAndroidSurfaceView.ActivityResumed");
				
				activityPaused = false;
				Monitor.PulseAll(lockObject); // Signal game thread to reevaluate surface state
			}			
		}
		
		#endregion
		
		
		#region Game Thread
		
		Thread gameThread;
		
		public void StartGame()
		{
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.StartGame");
			
			gameThread = new Thread(GameThreadMain);
			gameThread.Start();
		}
		
		// Wait for a rendering surface to become available
		// Returns false to signal the game thread to exit
		private bool WaitForEGLSurface()
		{
			lock(lockObject)
			{
				while(true) // Until a surface is available for rendering on
				{
					if(eglSurfaceAvailable && (activityPaused || !surfaceAvailable))
					{
						ExEnLog.WriteLine("Destroying EGL surface");
						
						// Surface we are using needs to go away
						DestroyEGLSurface();
					}
					else if((!eglSurfaceAvailable && !activityPaused && surfaceAvailable) || lostEglContext)
					{
						// We can (re)create the EGL surface (not paused, surface available)
						ExEnLog.WriteLine("(Re)Creating EGL surface");
						
						if(eglContextAvailable && !lostEglContext)
						{
							try
							{
								ExEnLog.WriteLine("Creating EGL surface for existing EGL context");
								CreateEGLSurface(); // Sets eglSurfaceAvailable = true on success
							}
							catch(ExEnSurfaceException e)
							{
								ExEnLog.WriteLine("Surface creation failed with message: " + e.Message);
							}
						}
						
						if(!eglSurfaceAvailable || lostEglContext) // Start or Restart due to context loss
						{
							ExEnLog.WriteLine("Context (re)creation required");
							
							bool contextWentAway = false;
							if(lostEglContext || eglContextAvailable) // context loss
							{
								ExEnLog.WriteLine("EGL context was lost, destroying dead context...");
								DestroyEGLContext();
								contextWentAway = true;
							}
							
							CreateEGLContext();
							CreateEGLSurface();
							
							if(contextWentAway)
							{
								// TODO: BUG:
								ExEnLog.WriteLine("TEXTURE RECREATION NOT YET SUPPORTED!");
								System.Diagnostics.Debug.Assert(false, "TEXTURE RECREATION NOT YET SUPPORTED!");
								//
								// At this point any textures (and possibly in the future: other 
								// graphics resources) have been lost, so all texture objects need
								// to be signaled to reload themselves
								//
								// Textures loaded from file should be pulled from flash (they need to
								// remember where they came from). Textures loaded from streams or
								// that are modified dynamically need to keep a backup in main memory.
								//
								// Also: GraphicsDevice.DeviceLost, DeviceReset, etc (and similar on GDM)
								//       events need to be triggered.
								//
								// Also: need a reliable way to test this
								//       (alternately: comment out "CreateEGLSurface()" in the first attempt)
							}
							
							ExEnLog.WriteLine("EGL surface creation finished");
						}
					}
					
					// If we've got a surface to render to, then do so:
					if(eglSurfaceAvailable)
						return true;
					else
					{
						ExEnLog.WriteLine("No EGL surface available, waiting...");
						
						// Otherwise wait to be signaled that a relevant state was changed
						Monitor.Wait(lockObject);
						continue; // and try again
					}
				}
			}
		}
		
		// Update the game's scaler if necessary
		private void HandleSurfaceChange()
		{
			lock(lockObject)
			{
				if(surfaceChanged)
				{
					ExEnLog.WriteLine("Handling surface change");

					var gd = game.GraphicsDevice;
					if(gd != null)
					{
						var scaler = gd.Scaler;
						if(scaler != null)
						{
							scaler.Change(this.orientation, new Point(surfaceWidth, surfaceHeight),
									new Point(surfaceWidth, surfaceHeight));
						}
					}
					
					surfaceChanged = false;
				}
			}
		}
		
		
		
		// Roughly tuned:
		static readonly TimeSpan sleepPrecision = TimeSpan.FromMilliseconds(3);
		
		void WasteTimeUntil(TimeSpan when)
		{
			TimeSpan timeLeft;
			while((timeLeft = (when - Now())) > TimeSpan.Zero)
			{
				if(timeLeft > sleepPrecision)
					Thread.Sleep(1);
				else
					Thread.SpinWait(1); // Appropriate value?
			}
		}
		
		TimeSpan Now()
		{
			// TODO: can this be made higher resolution?
			// TODO: can we inline this so that we skip the Java layer?
			//       (call uptimeMillis, systemTime, or clock_gettime directly?)
			// Note: doing these things may require re-tuning WasteTimeUntil
			long ms = Android.OS.SystemClock.UptimeMillis();
			return TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * ms);
		}
		
		TimeSpan frameBeginTime, previousFrameBeginTime;
		bool hasGameStarted = false;

		void GameThreadMain()
		{
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.GameThreadMain");
			
			try
			{
				while(true) // Each frame
				{
					frameBeginTime = Now();
					
					if(WaitForEGLSurface() == false)
						return; // Finish the game thread
					
					HandleSurfaceChange();
					
					if(!hasGameStarted) // Run once
					{
						hasGameStarted = true;
						if(game != null)
							game.DoStartup();
						
						// Start the ball rolling:
						previousFrameBeginTime = frameBeginTime - GameLoop.TargetElapsedTime;
					}
					
					// Update the game loop (calls Game.Update and Game.Draw)
					// with the amount of time the last frame took
					GameLoop.Tick(frameBeginTime - previousFrameBeginTime);
					
					// If the game is running fixed-time-step, waste any excess time
					if(GameLoop.IsFixedTimeStep)
					{
						// Delay until the time when the frame is expected to end.
						// (Any stutter or drift will be handled by GameLoop)
						WasteTimeUntil(frameBeginTime + GameLoop.TargetElapsedTime);
					}
					
					// We are about to become the previous frame
					previousFrameBeginTime = frameBeginTime;
				}
			}
			finally
			{
				lock(lockObject)
				{
					if(eglSurfaceAvailable)
						DestroyEGLSurface();
					if(eglContextAvailable)
						DestroyEGLContext();
				}
			}
		}
		
		// Note: not locked - only ever accessed in render thread
		bool lostEglContext;
		
		public void SwapBuffers()
		{
			if(!egl.EglSwapBuffers(eglDisplay, eglSurface))
			{
				if(egl.EglGetError() == EGL11Consts.EglContextLost)
				{
					if(lostEglContext)
						ExEnLog.WriteLine("Lost EGL context");
					lostEglContext = true;
				}
			}
		}
		
		#endregion
		
		
		#region EGL Surface and Context handling
		// Functionality here happens within WaitForEGLSurface, while lockObject is locked
		
		IEGL10 egl;
		EGLSurface eglSurface;
		EGLContext eglContext;
		EGLDisplay eglDisplay;
		EGLConfig eglConfig;
		
		private string GetEGLErrorAsString()
		{
			switch(egl.EglGetError())
			{
				case EGL10Consts.EglSuccess: return "Success";
				
				case EGL10Consts.EglNotInitialized:    return "Not Initialized";
					
				case EGL10Consts.EglBadAccess:         return "Bad Access";
				case EGL10Consts.EglBadAlloc:          return "Bad Allocation";
				case EGL10Consts.EglBadAttribute:      return "Bad Attribute";
				case EGL10Consts.EglBadConfig:         return "Bad Config";
				case EGL10Consts.EglBadContext:        return "Bad Context";
				case EGL10Consts.EglBadCurrentSurface: return "Bad Current Surface";
				case EGL10Consts.EglBadDisplay:        return "Bad Display";
				case EGL10Consts.EglBadMatch:          return "Bad Match";
				case EGL10Consts.EglBadNativePixmap:   return "Bad Native Pixmap";
				case EGL10Consts.EglBadNativeWindow:   return "Bad Native Window";
				case EGL10Consts.EglBadParameter:      return "Bad Parameter";
				case EGL10Consts.EglBadSurface:        return "Bad Surface";
				
				default: return "Unknown Error";
			}
		}
		
		private string AddEGLError(string message)
		{
			return message + " (EGL Error: " + GetEGLErrorAsString() + ")";
		}
		
		private void CreateEGLContext()
		{
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.CreateEGLContext Begin");
			
			// Assumes lockObject is locked
			
			lostEglContext = false;
			
			egl = EGLContext.EGL.JavaCast<IEGL10>();
			
			eglDisplay = egl.EglGetDisplay(EGL10Consts.EglDefaultDisplay);
			if(eglDisplay == EGL10Consts.EglNoDisplay)
				throw new ExEnSurfaceException("Could not get EGL display");
			
			int[] version = new int[2];
			if(!egl.EglInitialize(eglDisplay, version))
				throw new ExEnSurfaceException(AddEGLError("Could not initialize EGL display"));
			
			ExEnLog.WriteLine("EGL Version: " + version[0] + "." + version[1]);
			
			// TODO: allow GraphicsDeviceManager to specify a frame buffer configuration
			// TODO: test this configuration works on many devices:
			int[] configAttribs = new int[] {
					//EGL10Consts.EglRedSize, 5,
					//EGL10Consts.EglGreenSize, 6,
					//EGL10Consts.EglBlueSize, 5,
					//EGL10Consts.EglAlphaSize, 0,
					//EGL10Consts.EglDepthSize, 4,
					//EGL10Consts.EglStencilSize, 0,
					EGL10Consts.EglNone };
			EGLConfig[] configs = new EGLConfig[1];
			int[] numConfigs = new int[1];
			if(!egl.EglChooseConfig(eglDisplay, configAttribs, configs, 1, numConfigs))
				throw new ExEnSurfaceException(AddEGLError("Could not get EGL config"));
			if(numConfigs[0] == 0)
				throw new ExEnSurfaceException("No valid EGL configs found");
			eglConfig = configs[0];
			
			const int EglContextClientVersion = 0x3098;
			int[] contextAttribs = new int[] { EglContextClientVersion, 1, EGL10Consts.EglNone };
			eglContext = egl.EglCreateContext(eglDisplay, eglConfig, EGL10Consts.EglNoContext, contextAttribs);
			if(eglContext == null || eglContext == EGL10Consts.EglNoContext)
			{
				eglContext = null;
				throw new ExEnSurfaceException(AddEGLError("Could not create EGL context"));
			}
				
			eglContextAvailable = true;
			
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.CreateEGLContext End");
		}
		
		private void DestroyEGLContext()
		{
			// Assumes lockObject is locked
			
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.DestroyEGLContext Begin");
			
			if(eglContext != null)
			{
				if(!egl.EglDestroyContext(eglDisplay, eglContext))
					throw new ExEnSurfaceException(AddEGLError("Could not destroy EGL context"));
				eglContext = null;
			}
			if(eglDisplay != null)
			{
				if(!egl.EglTerminate(eglDisplay))
					throw new ExEnSurfaceException(AddEGLError("Could not terminate EGL connection"));
				eglDisplay = null;
			}
			
			eglContextAvailable = false;
			
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.DestroyEGLContext End");
		}
						
		private bool VerifyEGLContext()
		{
			var context = egl.EglGetCurrentContext();
			return (context != EGL10Consts.EglNoContext && egl.EglGetError() != EGL11Consts.EglContextLost);
		}
		
		private void CreateEGLSurface()
		{
			// Assumes lockObject is locked
			
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.CreateEGLSurface Begin");
			
			// If there is an existing surface, destroy the old one
			DestroySurfaceHelper();
			
			eglSurface = egl.EglCreateWindowSurface(eglDisplay, eglConfig, (Java.Lang.Object)this.Holder, null);
			if(eglSurface == null || eglSurface == EGL10Consts.EglNoSurface)
				throw new ExEnSurfaceException(AddEGLError("Could not create EGL window surface"));
			
			if(!egl.EglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext))
				throw new ExEnSurfaceException(AddEGLError("Could not make EGL current"));
			
			eglSurfaceAvailable = true;
			
			if(gdm != null)
				gdm.InternalDeviceReady(this, surfaceWidth, surfaceHeight, orientation);
			
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.CreateEGLSurface End");
		}
		
		private void DestroySurfaceHelper()
		{
			// Assumes lockObject is locked
			
			if(!(eglSurface == null || eglSurface == EGL10Consts.EglNoSurface))
			{
				if(!egl.EglMakeCurrent(eglDisplay, EGL10Consts.EglNoSurface,
							EGL10Consts.EglNoSurface, EGL10Consts.EglNoContext))
					throw new ExEnSurfaceException(AddEGLError("Could not unbind EGL surface"));
				
				if(!egl.EglDestroySurface(eglDisplay, eglSurface))
					throw new ExEnSurfaceException(AddEGLError("Could not destroy EGL surface"));
			}
			
			eglSurface = null;
		}
		
		private void DestroyEGLSurface()
		{
			// Assumes lockObject is locked
			
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.DestroyEGLSurface Begin");
			
			eglSurfaceAvailable = false;
			DestroySurfaceHelper();
			Monitor.PulseAll(lockObject); // Signal to SurfaceDestroyed that eglSurfaceAvailable changed
			
			ExEnLog.WriteLine("ExEnAndroidSurfaceView.DestroyEGLSurface End");
		}
		
		#endregion
				
		
		
		#region Input Handling
		// Occurs on UI thread
		
		// Owned by the UI thread, the game thread's scaler will "pick up"
		// the same surface changes that are applied to this scaler,
		// on the next iteration of the game thread.
		ExEnScaler inputScaler;
		
		void UpdateInputScaler()
		{
			inputScaler.Change(orientation, new Point(surfaceWidth, surfaceHeight),
					new Point(surfaceWidth, surfaceHeight));
		}
		
		public override bool OnTouchEvent(MotionEvent e)
		{
			// This runs on a different thread to the main game loop (note the locking)
			// TODO: does this contribute to input lag?
			
			Monitor.Enter(TouchInputManager.lockObject); // Convert to lock?
			try
			{
				bool processed = false; // Event has been processed
					
				int count = e.PointerCount;
				MotionEventActions a = e.Action;
				MotionEventActions action = a & MotionEventActions.Mask;
				// According to the docs, this does not actually produce the "Id" (despite the name), but the "Index"!
				//int actionPointerIndex = ((int)a & MotionEvent.ActionPointerIdMask) >> MotionEvent.ActionPointerIdShift;
				
				for(int i = 0; i < count; i++) // for each pointer (unordered)
				{
					int id = e.GetPointerId(i);
					
					var location = new System.Drawing.PointF(e.GetX(i), e.GetY(i));
					Point position = inputScaler.TouchToLogical((int)location.X, (int)location.Y);
					
					switch(action)
					{
					case MotionEventActions.Down:
						TouchInputManager.SanityCheckAllTouchesUp();
						goto case MotionEventActions.PointerDown; // Fall through
					case MotionEventActions.PointerDown:
						TouchInputManager.BeginTouch(id, position);
						processed = true;
						break;
						
						
					case MotionEventActions.Move:
						TouchInputManager.MoveTouch(id, position);
						processed = true;
						break;
						
						
					case MotionEventActions.Up:
					case MotionEventActions.PointerUp:
					case MotionEventActions.Cancel:
						TouchInputManager.EndTouch(id, position, action == MotionEventActions.Cancel);
						processed = true;
						break;
					}
				}
				
				if(processed)
					return true;
				else
					return base.OnTouchEvent(e);
			}
			finally
			{
				Monitor.Exit(TouchInputManager.lockObject);
					
				// http://stackoverflow.com/questions/792185/why-are-touch-events-destroying-my-android-framerate
				// http://stackoverflow.com/questions/4342464/android-touch-seriously-slowing-my-application
				// Thread.Sleep(16); // TODO: is this necessary?
			}
		}
		
		#endregion
		
		
	}
}

