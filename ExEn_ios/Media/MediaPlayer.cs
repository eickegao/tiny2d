using System;
using Microsoft.Xna.Framework.Audio;
using MonoTouch.AudioToolbox;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Media
{
	public static class MediaPlayer
	{
		internal static bool otherAudioIsPlaying = false; // set by AudioSessionManager
		public static bool GameHasControl { get { return !otherAudioIsPlaying; } }

		static Song currentSong;
		static SoundEffectInstance soundEffectInstance;

		public static object lockObject = new object();


		// Fix strange behaviour when the music does not start playing again after locking the screen
		internal static void MusicRestartHack()
		{
			Console.WriteLine("MediaPlayer.MusicRestartHack()");

			lock(lockObject)
			{
				if(soundEffectInstance != null && IsRepeating)
					soundEffectInstance.Play();
			}
		}


		#region Play Controls

		private static void InternalStop()
		{
			if(soundEffectInstance != null)
				soundEffectInstance.Dispose();
			soundEffectInstance = null;
			currentSong = null;
		}

		public static void Play(Song song)
		{
			lock(lockObject)
			{
				InternalStop();

				currentSong = song;

				// Shut down any external music
				// For some unknown reason, setting category on the simulator fails with an unknown error code (-50)
				try { AudioSession.Category = AudioSessionCategory.SoloAmbientSound; }
				catch { }

				soundEffectInstance = new SoundEffectInstance(currentSong.soundFile, true);
				SetSongVolume();
				soundEffectInstance.IsLooped = isRepeating;
				soundEffectInstance.Play();
			}
		}

		public static void Pause()
		{
			lock(lockObject)
			{
				if(soundEffectInstance != null)
					soundEffectInstance.Pause();
			}
		}

		public static void Resume()
		{
			lock(lockObject)
			{
				if(soundEffectInstance != null)
					soundEffectInstance.Resume();
			}
		}

		public static void Stop()
		{
			lock(lockObject)
			{
				InternalStop();

				try { AudioSession.Category = AudioSessionCategory.AmbientSound; }
				catch { }
			}
		}

		#endregion


		#region Volume handling

		static bool isMuted = false;
		public static bool IsMuted
		{
			get { return isMuted; }
			set
			{
				lock(lockObject)
				{
					isMuted = value;
					SetSongVolume();
				}
			}
		}

		static float volume = 1f;
		public static float Volume
		{
			get { return volume; }
			set
			{
				lock(lockObject)
				{
					volume = value;
					SetSongVolume();
				}
			}
		}

		private static void SetSongVolume()
		{
			if(soundEffectInstance != null)
				soundEffectInstance.Volume = isMuted ? 0f : volume;
		}

		#endregion


		#region IsRepeating

		static bool isRepeating = false;
		public static bool IsRepeating
		{
			get { return isRepeating; }
			set
			{
				lock(lockObject)
				{
					isRepeating = value;
					if(soundEffectInstance != null)
						soundEffectInstance.IsLooped = isRepeating;
				}
			}
		}

		#endregion

	}
}

