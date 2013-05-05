using System;
using System.Drawing;
using ExEnCore;
using Microsoft.Xna.Framework.Input;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.iPhoneOS;
using Microsoft.Xna.Framework.Graphics;
using MonoTouch.CoreGraphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
	internal class ExEnEmTouchGameViewController : UIViewController
	{
		// This class is responsible for handling interface orientation
		#region Notes
		//
		// According to Apple's documentations on PowerVR SGX hardware on iOS > 4.2 we should
		// be letting UIViewController handle the orientation. On older hardware and OSes
		// we should let OpenGL handle orientation and transform touches ourselves (see: ExEnEmTouchScaler).
		// 
		// ...oooor we could just handle orientation ourselves everywhere and save ourselves two code paths.
		//
		// To keep to the fast code-path, let the UIViewController attempt to rotate the UIView,
		// but block it from doing so inside ExEnEmTouchGameView. For good measure, set it to identity afterwards.
		//
		// Note that an iOS application will ALWAYS start up in Portrait mode, and then get spun around into
		// Landscape mode after starting up before being displayed. There is no real way around this.
		//
		#endregion

		Game game;
		GraphicsDeviceManager gdm;
		ExEnEmTouchGameView gameView;

		public ExEnEmTouchGameViewController(Game game, GraphicsDeviceManager gdm)
		{
			this.game = game;
			this.gdm = gdm;
		}

		public override void LoadView()
		{
			this.gameView = new ExEnEmTouchGameView(game, gdm);
			this.View = this.gameView;
		}

		public override bool ShouldAutorotateToInterfaceOrientation(UIInterfaceOrientation toInterfaceOrientation)
		{
			// Note that iOS's *interface* orientation swaps landscape left/right,
			// compared to *device* orientation on both iOS and WP7

			switch(toInterfaceOrientation)
			{
				case UIInterfaceOrientation.LandscapeLeft:
					return ((gdm.appliedSupportedOrientations & DisplayOrientation.LandscapeRight) != 0);
				
				case UIInterfaceOrientation.LandscapeRight:
					return ((gdm.appliedSupportedOrientations & DisplayOrientation.LandscapeLeft) != 0);

				case UIInterfaceOrientation.Portrait:
					return ((gdm.appliedSupportedOrientations & DisplayOrientation.Portrait) != 0);

				case UIInterfaceOrientation.PortraitUpsideDown:
					// To match WP7: iPhone does not use PortraitUpsideDown
					// But the iPad does support it to match Apple's guidelines
					return ((gdm.appliedSupportedOrientations & DisplayOrientation.Portrait) != 0)
							&& UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;
			}

			return false;
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			Console.WriteLine("ExEnEmTouchGameViewController.WillRotate(to = " + toInterfaceOrientation + ", " + duration + ")");
			gameView.blockOrientationChange = true;
		}

		static ExEnInterfaceOrientation ConvertOrientation(UIInterfaceOrientation orientation)
		{
			switch(orientation)
			{
				default:
				case UIInterfaceOrientation.Portrait: return ExEnInterfaceOrientation.Portrait;
				case UIInterfaceOrientation.PortraitUpsideDown: return ExEnInterfaceOrientation.PortraitUpsideDown;
				case UIInterfaceOrientation.LandscapeLeft: return ExEnInterfaceOrientation.LandscapeLeft;
				case UIInterfaceOrientation.LandscapeRight: return ExEnInterfaceOrientation.LandscapeRight;
			}
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			Console.WriteLine("ExEnEmTouchGameViewController.DidRotate(from = " + fromInterfaceOrientation + ")");

			gameView.blockOrientationChange = false;
			// Reset the transformation (for good measure) - return to the fast-path for OpenGL:
			gameView.Transform = CGAffineTransform.MakeIdentity();
			gameView.Bounds = UIScreen.MainScreen.Bounds;

			if(gdm != null && gdm.GraphicsDevice != null)
			{
				gdm.GraphicsDevice.Scaler.Orientation = ConvertOrientation(InterfaceOrientation);
			}

			// If we have just rotated, force the game to redraw (filling the back buffer)
			// This prevents the back buffer in the old orientation being presented
			gameView.ForceDraw();
		}
		
		// Implementing this removes the rotation effect and causes the view to simply snap
		// This actually looks better than the animated overlay animating over the top of the snap
		// This does, however, cause iOS to generate a warning message
		public override void WillAnimateSecondHalfOfRotation(UIInterfaceOrientation fromInterfaceOrientation, double duration)
		{
			if(gdm != null && gdm.GraphicsDevice != null)
			{
				gdm.GraphicsDevice.Scaler.Orientation = ConvertOrientation(InterfaceOrientation);
			}
		}
	}

	public class ExEnEmTouchGameView : iPhoneOSGameView
	{
		Game game;
		GraphicsDeviceManager gdm;

		internal GameLoop GameLoop { get { return game.gameLoop; } }

		readonly RectangleF ScreenBoundary;


		#region Construct

		public ExEnEmTouchGameView(Game game, GraphicsDeviceManager gdm) : base(UIScreen.MainScreen.Bounds)
		{
			// Setup iPhoneOSGameView:
			LayerRetainsBacking = false;
			LayerColorFormat = EAGLColorFormat.RGBA8;

			// Setup UIView:
			MultipleTouchEnabled = true;
			AutoResize = false;

			// Setup members:
			this.game = game;
			this.gdm = gdm;
			eachTouchEnumerator = EachTouch;

			// Display size:
			ScreenBoundary = UIScreen.MainScreen.Bounds;
			game.Window.ClientBounds = new Rectangle(0, 0, (int)ScreenBoundary.Width, (int)ScreenBoundary.Height);

			// Game loop and timing:
			updateGameTime = new GameTime();
			drawGameTime = new GameTime();
			GameLoop.Update = DoUpdate;
		}

		#endregion


		#region Startup Handling

		// see also: isPausing
		bool isStarting = false;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if(isStarting)
			{
				if(gdm != null)
				{
					// Frame buffer was created, so now we have a graphics device to create:
					gdm.InternalCreateDevice(this);
				}

				if(game != null)
				{
					game.DoStartup();

					// Need to draw something to the back buffer and swap it, to avoid
					// having a black frame appear between the splash screen
					// and the game itself:
					TimeSpan target = GameLoop.TargetElapsedTime;
					if(!game.iOSFasterStartup)
					{
						updateGameTime.Update(target);
						game.Update(updateGameTime);
					}
					drawGameTime.Update(target);
					game.DoDraw(drawGameTime); // This will swap buffers
				}
			}
		}

		/// <summary>Start the game - performing initialization.</summary>
		public void StartGame()
		{
			isStarting = true;
			RunAtTargetFramerate(); // Will call CreateFrameBuffer and then OnLoad
			isStarting = false;
		}

		#endregion


		#region Layer

		[Export("layerClass")]
		static Class LayerClass()
		{
			return iPhoneOSGameView.GetLayerClass();
		}

		protected override void ConfigureLayer(CAEAGLLayer eaglLayer)
		{
			eaglLayer.Opaque = true;

			// Scale OpenGL layer to the scale of the main layer
			// On iPhone 4 this makes the renderbuffer size the same as actual device resolution
			// On iPad with user-selected scale of 2x at startup, this will trigger but has no effect on the renderbuffer
			if(UIScreen.MainScreen.Scale != 1)
				eaglLayer.ContentsScale = UIScreen.MainScreen.Scale;
		}

		#endregion


		#region Frame Buffer

		int renderbufferWidth;
		int renderbufferHeight;

		public Point RenderbufferSize { get { return new Point(renderbufferWidth, renderbufferHeight); } }
		public Point DeviceSize { get { return new Point((int)ScreenBoundary.Width, (int)ScreenBoundary.Height); } }

		protected override void CreateFrameBuffer()
		{
			if(isPausing)
				return; // See note on isPausing

			ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
			base.CreateFrameBuffer();

			// Determine actual render buffer size (due to possible Retina Display scaling)
			// http://developer.apple.com/library/ios/#documentation/iphone/conceptual/iphoneosprogrammingguide/SupportingResolutionIndependence/SupportingResolutionIndependence.html#//apple_ref/doc/uid/TP40007072-CH10-SW11
			unsafe
			{
				int width = 0, height = 0;
				GL.Oes.GetRenderbufferParameter(All.RenderbufferOes, All.RenderbufferWidthOes, &width);
				GL.Oes.GetRenderbufferParameter(All.RenderbufferOes, All.RenderbufferHeightOes, &height);

				renderbufferWidth = width;
				renderbufferHeight = height;
			}
		}

		protected override void DestroyFrameBuffer()
		{
			if(isPausing)
				return; // see note on isPausing

			base.DestroyFrameBuffer();
		}

		#endregion

		
		#region Handling for Backgrounding Applications

		/// <summary>Calls Run(double) with the framerate specified by Game.TargetElapsedTime</summary>
		void RunAtTargetFramerate()
		{
			//Run(1.0/game.TargetElapsedTime.TotalSeconds);
			Run(1.0/GameLoop.TargetElapsedTime.TotalSeconds);
		}


		// HACK HACK HACK!
		/// <summary>
		/// This is an amazing hack that is based on the knowledge that Run will call CreateFrameBuffer
		/// and that Stop will call DestroyFrameBuffer. We don't want to do either (we don't want to touch
		/// the OpenGL state while the application is being backgrounded/foregrounded). So when the application
		/// is pausing (from DidEnterBackground) simply don't allow CreateFrameBuffer or DestroyFrameBuffer to run.
		/// Also OnLoad, the Load event, OnUnload and the Unload event are called - assume they pose no problem
		/// (although the same technique could be used if they do).
		/// 
		/// The reason that Run and Stop need to be called at all is to stop the timer that is dispatching
		/// update and draw events (which will touch the OpenGL context and get us killed when backgrounded).
		/// </summary>
		bool isPausing = false;


		public void Pause()
		{
			isPausing = true;
			Stop();
		}

		public void Resume()
		{
			RunAtTargetFramerate();
			isPausing = false;
		}

		#endregion


		#region Draw and Update Pump (iPhoneOSGameView)

		private GameTime drawGameTime;

		internal void ForceDraw()
		{
			if(game != null)
			{
				drawGameTime.Update(TimeSpan.Zero);
				game.DoDraw(drawGameTime);
			}
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			MakeCurrent();

			if(game != null)
			{
				drawGameTime.Update(TimeSpan.FromSeconds(e.Time));
				game.DoDraw(drawGameTime);
			}
		}


		private GameTime updateGameTime;

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			TimeSpan time = TimeSpan.FromSeconds(e.Time);

			if(game != null)
			{
				GameLoop.Tick(time);
			}
			else
			{
				GameLoop.Reset();
			}
		}

		void DoUpdate(TimeSpan time)
		{
			updateGameTime.Update(time);
			game.Update(updateGameTime);
		}

		#endregion


		#region Touch Handling (UIView)

		void EachTouch(NSObject touchObject, ref bool stop)
		{
			UITouch touch = (UITouch)touchObject;

			int id = touch.Handle.ToInt32();

			PointF location = touch.LocationInView(this);
			Point position = game.GraphicsDevice.Scaler.TouchToLogical((int)location.X, (int)location.Y);

			var phase = touch.Phase;
			switch(phase)
			{
				case UITouchPhase.Began:
					TouchInputManager.BeginTouch(id, position);
					break;

				default:
				case UITouchPhase.Stationary:
					break; // Do nothing

				case UITouchPhase.Moved:
					TouchInputManager.MoveTouch(id, position);
					break;

				case UITouchPhase.Cancelled:
				case UITouchPhase.Ended:
					TouchInputManager.EndTouch(id, position, phase == UITouchPhase.Cancelled);
					break;
			}

			stop = false;
		}

		NSSetEnumerator eachTouchEnumerator;

		public override void TouchesBegan(NSSet touches, UIEvent e)
		{
			base.TouchesBegan(touches, e);
			lock(TouchInputManager.lockObject)
				e.TouchesForView(this).Enumerate(eachTouchEnumerator);
		}

		public override void TouchesEnded(NSSet touches, UIEvent e)
		{
			base.TouchesEnded(touches, e);
			lock(TouchInputManager.lockObject)
				e.TouchesForView(this).Enumerate(eachTouchEnumerator);
		}

		public override void TouchesMoved(NSSet touches, UIEvent e)
		{
			base.TouchesMoved(touches, e);
			lock(TouchInputManager.lockObject)
				e.TouchesForView(this).Enumerate(eachTouchEnumerator);
		}

		public override void TouchesCancelled(NSSet touches, UIEvent e)
		{
			base.TouchesCancelled(touches, e);
			lock(TouchInputManager.lockObject)
				e.TouchesForView(this).Enumerate(eachTouchEnumerator);
		}

		#endregion


		#region Orientation Change Blocking

		// Prevents iOS from ever changing the orientation of this view
		// This looks a lot neater, preventing a lot of unpleasent flickering when the ExEn Scaler changes.
		internal bool blockOrientationChange = false;

		public override MonoTouch.CoreGraphics.CGAffineTransform Transform
		{
			get { return base.Transform; }
			set { if(!blockOrientationChange) base.Transform = value; }
		}

		public override RectangleF Bounds
		{
			get { return base.Bounds; }
			set { if(!blockOrientationChange) base.Bounds = value; }
		}

		public override PointF Center
		{
			get { return base.Center; }
			set { if(!blockOrientationChange) base.Center = value; }
		}

		#endregion

	}
}
