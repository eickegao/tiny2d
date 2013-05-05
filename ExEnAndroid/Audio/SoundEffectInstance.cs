using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
	public enum SoundState
	{
		Playing,
		Paused,
		Stopped
	}
	
	
	public class SoundEffectInstance
	{
		#region Looped Streams
		
		internal static List<int> loopedStreams = new List<int>();
		
		internal static void ActivityPaused()
		{
			lock(loopedStreams)
			{
				foreach(int stream in loopedStreams)
				{
					SoundEffect.pool.Pause(stream);
				}
			}
		}
		
		internal static void ActivityResumed()
		{
			lock(loopedStreams)
			{
				foreach(int stream in loopedStreams)
				{
					SoundEffect.pool.Resume(stream);
				}
			}
		}
		
		static void RegisterLoopedStream(int stream)
		{
			lock(loopedStreams)
			{
				loopedStreams.Add(stream);
			}
		}
		
		static void UnregisterLoopedStream(int stream)
		{
			lock(loopedStreams)
			{
				loopedStreams.Remove(stream);
			}
		}
		
		#endregion
		
		
		SoundEffect soundEffect;
		
		internal SoundEffectInstance(SoundEffect soundEffect)
		{
			this.soundEffect = soundEffect;
		}
		
		
		int streamId;


		#region IDisposable Members

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			Stop();
			IsDisposed = true;
		}

		#endregion


		#region Play Controls (XNA API)
		
		bool hasPlayed;
		
		public void Play()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());
			
			hasPlayed = true;
			streamId = SoundEffect.pool.Play(soundEffect.soundId,
					LeftVolume, RightVolume, 1, _isLooped ? -1 : 0, 1);
			
			if(IsLooped)
				loopedStreams.Add(streamId);
		}

		public void Resume()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			if(streamId != 0)
			{
				SoundEffect.pool.Resume(streamId);
				
				if(IsLooped)
					loopedStreams.Add(streamId);
			}
		}

		public void Stop()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());
			
			if(streamId != 0)
			{
				if(IsLooped)
					loopedStreams.Remove(streamId);
				
				SoundEffect.pool.Stop(streamId);
			}
		}

		public void Pause()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			if(streamId != 0)
			{
				if(IsLooped)
					loopedStreams.Remove(streamId);
				
				SoundEffect.pool.Pause(streamId);
			}
		}

		#endregion

		
		#region Sound State (XNA API)
		
		float LeftVolume { get { return _volume * (_pan > 0f ? 1f-_pan : 1f); } }
		float RightVolume { get { return _volume * (_pan < 0f ? 1f+_pan : 1f); } }
		
		private float _volume = 1;
		public float Volume
		{
			get { return _volume; }
			set
			{
				if(IsDisposed)
					throw new ObjectDisposedException(this.ToString());

				_volume = MathHelper.Clamp(value, 0, 1);
				if(streamId != 0)
					SoundEffect.pool.SetVolume(streamId, LeftVolume, RightVolume);
			}
		}
		
		private float _pan = 0;
		public float Pan
		{
			get { return _pan; }
			set
			{
				if(IsDisposed)
					throw new ObjectDisposedException(this.ToString());

				_pan = MathHelper.Clamp(value, -1, 1);
				if(streamId != 0)
					SoundEffect.pool.SetVolume(streamId, LeftVolume, RightVolume);
			}
		}

		private bool _isLooped = false;
		public bool IsLooped
		{
			get { return _isLooped; }
			set
			{
				if(IsDisposed)
					throw new ObjectDisposedException(this.ToString());
				if(hasPlayed)
					throw new InvalidOperationException("Cannot change loop mode after starting playback");
				
				_isLooped = value;
				if(streamId != 0)
					SoundEffect.pool.SetLoop(streamId, _isLooped ? -1 : 0);
			}
		}

		#endregion

		
	}
}

