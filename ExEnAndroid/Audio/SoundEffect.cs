using System;
using Android.Media;
using Android.Content.Res;

namespace Microsoft.Xna.Framework.Audio
{
	public class SoundEffect
	{
		#region Android SoundPool
		
		internal static SoundPool pool;
		internal static void Setup()
		{
			pool = new Android.Media.SoundPool(32, Android.Media.Stream.Music, 0);
		}
	
		#endregion
			
		
		internal int soundId;
		
		internal SoundEffect(AssetFileDescriptor afd)
		{
			soundId = pool.Load(afd, 1);
		}
		
		
		public SoundEffectInstance CreateInstance()
		{
			return new SoundEffectInstance(this);
		}
		
		
		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			pool.Unload(soundId);
			IsDisposed = true;
		}
		
		
		#region Play
		
		public bool Play()
		{
			return Play(1, 0, 0);
		}
		
		public bool Play(float volume, float pitch, float pan)
		{
			// Use sound-pool's built-in fire-and-forget mode:
			float left = volume * (pan > 0f ? 1f-pan : 1f);
			float right = volume * (pan < 0f ? 1f+pan : 1f);
			int streamId = pool.Play(soundId, left, right, 1, 0, 1);
			return streamId != 0;
		}
			
		#endregion
	}
}

