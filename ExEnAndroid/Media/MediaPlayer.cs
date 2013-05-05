using System;
using MediaError=Android.Media.MediaError;
using AMP=Android.Media.MediaPlayer;

namespace Microsoft.Xna.Framework.Media
{
	public static class MediaPlayer
	{
		#region AMP setup and management
		
		static object lockObject = new object();
		static AMP player;
		
		// True if AMP has been used and must be reset to change songs
		static bool playRequiresReset;
		
		// True if in the Started or Paused states (can pause/resume/stop immediately, otherwise queue)
		static bool hasStarted;
		// Queue up pausing and stopping, if they are called before Prepared state is reached
		static bool queuePause;
		static bool queueStop;
		
		// The currently playing song, if non-null at Setup, then resume playback at position
		static Song currentSong;
		
		// Queued seek position, for when starting a song when resuming an activity
		static int queueSeek;
		
		
		static bool HandleError(AMP mp, MediaError what, int extra)
		{
			lock(lockObject)
			{
				DoPlayerReset();
				return true;
			}
		}
		
		// Asyncronously called after AMP finishes preparation started in Play()
		static void HandlePrepared(object sender, EventArgs args)
		{
			lock(lockObject)
			{
				if(queueSeek != 0)
					player.SeekTo(queueSeek);
				
				player.Start();
				hasStarted = true;
				
				if(queueStop)
				{
					player.Stop();
					hasStarted = false;
				}
				else if(queuePause)
				{
					player.Pause();
				}
				
				queueStop = false;
				queuePause = false;
			}
		}	
		
		
		// Called by the Activity's OnResume
		internal static void Setup()
		{
			lock(lockObject)
			{
				if(player != null)
					return; // Already setup
				
				player = new AMP();
				
				//player.Error = HandleError;
				player.Prepared += HandlePrepared;
				playRequiresReset = false; // player starts in the Idle state
				
				player.Looping = isRepeating;
				SetPlayerVolume();
				
				// Check if we are returning from activity-pause and should resume a song...
				if(currentSong != null)
				{
					InternalPlay(currentSong);
				}
			}
		}
		
		// Called by the Activity's OnPause
		internal static void TearDown()
		{
			lock(lockObject)
			{
				if(player == null)
					return; // Already torn down
				
				// If we're currently playing a song, store the seek position for resuming
				queueSeek = hasStarted ? player.CurrentPosition : 0;
				queuePause = !player.IsPlaying;
				
				// Shut down player and release resources
				player.Release();
				player = null;
				
				hasStarted = false; // start queuing stop/resume commands
			}
		}
		
		
		static void DoPlayerReset()
		{
			hasStarted = false;
			
			queuePause = false;
			queueStop = false;
			queueSeek = 0;
			
			player.Reset();
			playRequiresReset = false;
			
			player.Looping = isRepeating;
			SetPlayerVolume();
		}
		
		#endregion

		
		#region Play Controls
		
		public static void Play(Song song)
		{
			lock(lockObject)
			{
				// If player is or goes un-ready (activity pauses), set state for restarting song
				currentSong = song;
				
				// Ignore all previous commands when starting a new track
				queuePause = false;
				queueStop = false;
				queueSeek = 0;
				
				// Start queuing any new commands until playback starts
				hasStarted = false;
				
				if(player != null) // If player is not available, song has been queued for when it is
					InternalPlay(song);
			}
		}
		
		static void InternalPlay(Song song)
		{
			if(playRequiresReset)
			{
				DoPlayerReset();
			}
			
			playRequiresReset = true; // About to exit Idle state (can only call SetDataSource once)
			using(var fd = song.GetFileDescriptor())
			{
				player.SetDataSource(fd);
			}
			player.PrepareAsync();
		}
		
		
		public static void Pause()
		{
			lock(lockObject)
			{
				if(hasStarted && player != null && player.IsPlaying)
				{
					player.Pause();
				}
				else
				{
					queuePause = true;
				}
			}
		}

		public static void Resume()
		{
			lock(lockObject)
			{
				if(hasStarted && player != null)
				{
					player.Start();
				}
				else
				{
					queuePause = false;
				}
			}
		}

		public static void Stop()
		{
			lock(lockObject)
			{
				currentSong = null;
				
				if(hasStarted)
				{
					if(player != null)
						player.Stop();
					hasStarted = false;
				}
				else
				{
					queueStop = true;
				}
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
					SetPlayerVolume();
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
					SetPlayerVolume();
				}
			}
		}

		private static void SetPlayerVolume()
		{
			// TODO: what is the correct scaling for volume?
			float v = isMuted ? 0 : volume;
			
			if(player != null)
				player.SetVolume(v, v);
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
					
					if(player != null)
						player.Looping = isRepeating;
				}
			}
		}

		#endregion
	}
}

