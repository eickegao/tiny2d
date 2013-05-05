using System;
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
	public class ExEnAndroidActivity : Activity
	{
		#region Android Activity Defaults
		
		// NOTE: API level 13 requires declaring screenSize as well as orientation
		public const ConfigChanges DefaultConfigChanges =
				ConfigChanges.Orientation | ConfigChanges.KeyboardHidden;
		
		public const string DefaultTheme = "@android:style/Theme.NoTitleBar.Fullscreen";
		
		#endregion
	
		internal ExEnAndroidSurfaceView surface = null;
		
	
		
		protected override void OnCreate(Bundle savedInstanceState)
		{
			ExEnLog.WriteLine("ExEnAndroidActivity.OnCreate");
			base.OnCreate(savedInstanceState);
		}
		
		protected override void OnDestroy()
		{
			ExEnLog.WriteLine("ExEnAndroidActivity.OnDestroy");
			base.OnDestroy();
		}
		
		protected override void OnRestart()
		{
			ExEnLog.WriteLine("ExEnAndroidActivity.OnRestart");
			base.OnStart();
		}
		
		protected override void OnStart()
		{
			ExEnLog.WriteLine("ExEnAndroidActivity.OnStart");
			base.OnStart();
		}
	
		protected override void OnStop()
		{
			ExEnLog.WriteLine("ExEnAndroidActivity.OnStop");
			base.OnStop();
		}
		
		
		protected override void OnResume()
		{
			ExEnLog.WriteLine("ExEnAndroidActivity.OnResume");
			
			base.OnResume();
			surface.ActivityResumed();
			MediaPlayer.Setup();
			SoundEffectInstance.ActivityResumed();
		}
		
		protected override void OnPause()
		{
			ExEnLog.WriteLine("ExEnAndroidActivity.OnPause");
			
			SoundEffectInstance.ActivityPaused();
			MediaPlayer.TearDown();
			surface.ActivityPaused();
			base.OnPause();
		}
		
		
		public override void OnConfigurationChanged(Configuration newConfig)
		{
			ExEnLog.WriteLine("ExEnAndroidActivity.OnConfigurationChanged");
			base.OnConfigurationChanged(newConfig);
		}
		
	}
}

