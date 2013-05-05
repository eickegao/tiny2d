using System;
using System.Collections.Generic;
using MonoTouch.AudioToolbox;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace ExEn
{
	internal static class AudioSessionManager
	{
		// TODO: need to better enforce what can and cannot be done while the audio system is offline
		internal static volatile bool audioSystemAvailable = false;

		internal static void Setup()
		{
			Debug.WriteLine("AudioSessionManager.Setup()");

			AudioSession.Initialize();

			AudioSession.Interrupted += (o, e) =>
			{
				Debug.WriteLine("AudioSession.Interrupted");
				audioSystemAvailable = false;
				AudioSession.SetActive(false);
			};

			// Want to reactivate on resume from interruption
			AudioSession.Resumed += (o, e) =>
			{
				Debug.WriteLine("AudioSession.Resumed");
				AudioSession.SetActive(true);
				audioSystemAvailable = true;
				SoundEffectThread.RestartAllRestarable();
			};

			// Checking if Other Audio is Playing During App Launch
			bool otherAudioIsPlaying = AudioSession.OtherAudioIsPlaying;
			MediaPlayer.otherAudioIsPlaying = otherAudioIsPlaying;

			Debug.WriteLine("AudioSession.OtherAudioIsPlaying == " + otherAudioIsPlaying);

			// For some unknown reason, setting category on the simulator fails with an unknown error code (-50)
			try
			{
				if(otherAudioIsPlaying)
					AudioSession.Category = AudioSessionCategory.AmbientSound;
				else
					AudioSession.Category = AudioSessionCategory.SoloAmbientSound;
			}
			catch
			{
				Debug.WriteLine("Exception when setting AudioSession.Category");
			}


			AudioSession.SetActive(true);
			audioSystemAvailable = true;
		}
	}
}
