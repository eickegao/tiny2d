using System;
using System.Collections.Generic;
using MonoTouch.UIKit;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;

namespace Microsoft.Xna.Framework
{
	public class ExEnEmTouchApplication : UIApplicationDelegate
	{

		// TODO: make Game register itself automatically!
		protected Game game = null;


		public override void OnResignActivation(UIApplication application)
		{
			Console.WriteLine("ExEnEmTouchApplication.OnResignActivation()");

			if(game != null)
				game.IsActive = false;
		}

		public override void OnActivated(UIApplication application)
		{
			Console.WriteLine("ExEnEmTouchApplication.OnActivated()");

			if(game != null)
				game.IsActive = true;

			MediaPlayer.MusicRestartHack();
		}


		public override void WillTerminate(UIApplication application)
		{
			Console.WriteLine("ExEnEmTouchApplication.WillTerminate()");

			if(game != null)
				game.DoTermination();
		}

		public override void DidEnterBackground(UIApplication application)
		{
			Console.WriteLine("ExEnEmTouchApplication.DidEnterBackground()");

			if(game != null)
				game.DoEnterBackground();
		}

		public override void WillEnterForeground(UIApplication application)
		{
			Console.WriteLine("ExEnEmTouchApplication.WillEnterForeground()");

			if(game != null)
				game.DoEnterForeground();
		}
	}
}
