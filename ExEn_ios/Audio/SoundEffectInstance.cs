using System;
using Microsoft.Xna.Framework;
using MonoTouch.AudioToolbox;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MonoTouch;
using System.Threading;
using System.Collections.Generic;
using ExEn;


// TODO: 
// Possibly need to create a more explicit audio thread and give it its own "run loop"
// This will allow these enhancements:
// - Make handling of hardware failure when restarting music nice (rather than brute force)
// - Potentially nicer architecture all around for fixing bugs (see below)
// - Ensure that AudioQueue functions are not called when audio is interrupted (at the moment only Start and Prime are handled directly)
//

// KNOWN BUGS:
//
// - Cannot detect otherMusicPlaying state on return from background
// - Simultaneouly started sounds are not played at the same time
// - Occasionally a sound will not actually play (unknown cause)
//

// MISSING FEATURES:
//
// - Cannot handle resuming sounds that are interrupted (eg: dialouge) 
//   (note: possibly can handle this by manually pausing/unpausing when deactivated)
// - No master volume for sound effects
//


namespace Microsoft.Xna.Framework.Audio
{
	public enum SoundState
	{
		Playing,
		Paused,
		Stopped
	}

	// Everything in this class happens on the SoundEffectThread (except for callsbacks from AudioQueue)
	internal class InternalSoundEffectInstance : IDisposable
	{
		SoundEffect soundEffect;
		AudioFile audioFile { get { return soundEffect.audioFile; } }

		OutputAudioQueue queue;
		AudioQueue.AudioQueuePropertyChanged IsRunningChangedCallback;

		const int numberOfBuffers = 3;
		IntPtr[] buffers;

		bool hardware;

		public InternalSoundEffectInstance(SoundEffect soundEffect, bool hardware)
		{
			this.soundEffect = soundEffect;
			this.hardware = hardware;
		}

		public void Setup()
		{
			if(soundEffect.IsDisposed)
				throw new ObjectDisposedException(soundEffect.ToString());

			buffers = new IntPtr[numberOfBuffers];

			queue = new OutputAudioQueue(soundEffect.description);
			queue.OutputCompleted += new EventHandler<OutputCompletedEventArgs>(HandleOutputBuffer);
			IsRunningChangedCallback = new AudioQueue.AudioQueuePropertyChanged(IsRunningChanged);
			queue.AddListener(AudioQueueProperty.IsRunning, IsRunningChangedCallback);

			// Set hardware mode
			unsafe
			{
				const AudioQueueProperty HardwareCodecPolicy = (AudioQueueProperty)1634820976; // 'aqcp'
				const uint PreferSoftware = 3, PreferHardware = 4;
				uint policy = hardware ? PreferHardware : PreferSoftware;
				queue.SetProperty(HardwareCodecPolicy, Marshal.SizeOf(typeof(uint)), new IntPtr(&policy));
			}

			AllocatePacketDescriptionsArray();
			queue.MagicCookie = audioFile.MagicCookie;
			AllocateBuffers();
			queue.Volume = 1;
			PrimeBuffers();
		}


		#region Packet Descriptions Array

		IntPtr packetDescriptionArray = IntPtr.Zero;

		unsafe void AllocatePacketDescriptionsArray()
		{
			if(soundEffect.isVBR)
			{
				packetDescriptionArray = Marshal.AllocHGlobal(
						soundEffect.packetsToRead * Marshal.SizeOf(typeof(AudioStreamPacketDescription)));
			}
			else
			{
				packetDescriptionArray = IntPtr.Zero;
			}
		}

		#endregion


		#region Stream data from file

		unsafe void AllocateBuffers()
		{
			for(int i = 0; i < numberOfBuffers; i++)
			{
				var status = queue.AllocateBuffer(soundEffect.bufferSize, out buffers[i]);
				if(status != AudioQueueStatus.Ok)
					throw new OutOfMemoryException();
			}
		}

		unsafe void PrimeBuffers()
		{
			currentPacket = 0;
			for(int i = 0; i < numberOfBuffers; i++)
			{
				if(!HandleOutputBuffer((AudioQueueBuffer*)buffers[i].ToPointer(), true))
					break;
			}

			int temp;
			queue.Prime(0, out temp);
		}


		// Binding required because MonoTouch's AudioFile -> AudioQueue has no nice linkage
		[DllImport(Constants.AudioToolboxLibrary)]
		private static extern int AudioFileReadPackets(IntPtr audioFile,
				bool useCache, ref uint numBytes, IntPtr outPacketDescriptions,
				long startPacket, ref uint numPackets, IntPtr outputBuffer);
		// Binding just to stop MonoTouch messing about in AudioQueue.EnqueueBuffer
		[DllImport(Constants.AudioToolboxLibrary)]
		private static extern AudioQueueStatus AudioQueueEnqueueBuffer(IntPtr AQ,
				IntPtr audioQueueBuffer, uint nPackets, IntPtr packetDescriptions);


		// Ask the callback to loop the audio
		// Write on main thread, read in callback
		volatile bool loop = false;

		// Current position of the callback in the loop
		// Read/write in callback, write on main thread only when audio is stopped
		long currentPacket = 0;


		unsafe private bool ReadFileIntoBuffer(AudioQueueBuffer* buffer, out uint numBytesReadFromFile, out uint numPackets)
		{
			numBytesReadFromFile = 0;
			numPackets = (uint)soundEffect.packetsToRead;

			if(AudioFileReadPackets(audioFile.Handle, false, ref numBytesReadFromFile,
					packetDescriptionArray, currentPacket, ref numPackets,
					buffer->AudioData) != 0)
			{
				// An error occured
				queue.Stop(false);
				return false;
			}

			return true;
		}

		// THIS HAPPENS ON ANOTHER THREAD
		unsafe bool HandleOutputBuffer(AudioQueueBuffer* buffer, bool priming)
		{
			uint numBytesReadFromFile;
			uint numPackets;

			if(!ReadFileIntoBuffer(buffer, out numBytesReadFromFile, out numPackets))
				return false; // ERROR

			if(loop && numPackets == 0)
			{
				currentPacket = 0; // Restart from beginning
				if(!ReadFileIntoBuffer(buffer, out numBytesReadFromFile, out numPackets))
					return false; // ERROR
			}

			if(numPackets > 0) // have we recieved data?
			{
				buffer->AudioDataByteSize = numBytesReadFromFile;
				AudioQueueStatus status = AudioQueueEnqueueBuffer(queue.Handle,
						new IntPtr(buffer), (packetDescriptionArray != IntPtr.Zero ? numPackets : 0),
						packetDescriptionArray);
				currentPacket += numPackets;
			}
			else
			{
				if(!priming) // Stop the queue (if priming - queue isn't running anyway)
					queue.Stop(false);
				return false; // No audio remains
			}

			return true; // More audio available
		}

		unsafe void HandleOutputBuffer(object sender, OutputCompletedEventArgs e)
		{
			HandleOutputBuffer(e.UnsafeBuffer, false);
		}

		#endregion


		#region IDisposable Members

		public bool IsDisposed
		{
			get { return queue == null; }
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				UnregisterRestartable();
				queue.RemoveListener(AudioQueueProperty.IsRunning, IsRunningChangedCallback);
				queue.Dispose(); // this will dispose of the buffers allocated
				queue = null;
			}

			// Dispose of unmanaged resources...
			Marshal.FreeHGlobal(packetDescriptionArray);
		}

		~InternalSoundEffectInstance()
		{
			Dispose(false);
		}

		#endregion


		#region Interruption Restart Control

		static List<InternalSoundEffectInstance> restartableList = new List<InternalSoundEffectInstance>();

		internal static void RestartAllRestartable()
		{
			Debug.WriteLine("InternalSoundEffectInstance.RestartAllRestartable()");
			foreach(var instance in restartableList)
				instance.Restart();
		}

		void Restart()
		{
			Debug.WriteLine("InternalSoundEffectInstance.Restart()");
			AudioQueueStatus status = queue.Start();
			Debug.WriteLine("queue status == " + status.ToString());
			if(status == (AudioQueueStatus)1752656245) // 'hwiu' - hardware in use
			{
				Debug.WriteLine("(hardware in use)");
				SoundEffectThread.Enqueue(new SoundEffectInstance.WorkItem(this, DoRetryRestart));
			}
		}


		internal static Action<InternalSoundEffectInstance, float> DoRetryRestart = (s, v) => { s.RetryRestart(); }; 

		// Keep trying until the hardware becomes free, when recovering from interruption
		void RetryRestart()
		{
			Debug.WriteLine("InternalSoundEffectInstance.RetryRestart()");
			if(registeredToRestart)
			{
				Thread.Sleep(10);
				Restart();
			}
		}

		bool registeredToRestart = false;

		void RegisterRestartable()
		{
			if(!registeredToRestart)
			{
				restartableList.Add(this);
				registeredToRestart = true;
			}
		}

		void UnregisterRestartable()
		{
			if(registeredToRestart)
			{
				restartableList.Remove(this);
				registeredToRestart = false;
			}
		}

		#endregion


		#region Play Controls

		bool audioPaused = false;

		public void Play()
		{
			//Debug.WriteLine("InternalSoundEffectInstance.Play");

			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			// ONLY Loops are restartable
			// (note: if non-loops are made restartable, need to add UnregisterRestartable
			// via SoundEffectThread to HandleOutputBuffer when the sound ends, or to the IsRunning property change.)
			if(loop) 
				RegisterRestartable();

			if(AudioSessionManager.audioSystemAvailable)
				queue.Start();
			else if(!loop)
				readyToReFire = true; // Just pretend we played ;)
		}

		public void Stop()
		{
			//Debug.WriteLine("InternalSoundEffectInstance.Stop");

			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			UnregisterRestartable();
			audioPaused = false;
			queue.Stop(true);
			currentPacket = 0;

			if(AudioSessionManager.audioSystemAvailable)
				PrimeBuffers();
		}

		public void Pause()
		{
			//Debug.WriteLine("InternalSoundEffectInstance.Pause");

			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			UnregisterRestartable();
			audioPaused = true;
			queue.Pause();
		}

		#endregion


		#region Sound State

		// TODO: implement SoundEffect.MasterVolume
		public void SetVolume(float volume)
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			queue.Volume = volume;		
		}

		public void SetLooped()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			loop = true;
		}

		public void ClearLooped()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			loop = false;
		}

		// TODO: this could probably be implemented better by using IsRunningChanged
		public SoundState State
		{
			get
			{
				if(queue == null)
					return SoundState.Stopped;
				if(queue.IsRunning)
					return SoundState.Playing;
				else if(audioPaused)
					return SoundState.Paused;
				else
					return SoundState.Stopped;
			}
		}

		#endregion


		#region State for Fire-and-Forget sounds

		void IsRunningChanged(AudioQueueProperty property)
		{
			if(queue.IsRunning == false)
				readyToReFire = true;
		}

		internal volatile bool readyToReFire;

		#endregion

	}


	// Based on OffThreadWorkQueue from Krab
	static class SoundEffectThread
	{
		static private Queue<SoundEffectInstance.WorkItem> workQueue = new Queue<SoundEffectInstance.WorkItem>();

		static SoundEffectThread()
		{
			Thread workerThread = new Thread(Worker);
			workerThread.Start();
		}

		static public void Enqueue(SoundEffectInstance.WorkItem workItem)
		{
			if(isDisposed)
				throw new ObjectDisposedException("SoundEffectThread");

			lock(workQueue)
			{
				workQueue.Enqueue(workItem);
				Monitor.Pulse(workQueue);
			}
		}

		static bool isDisposed = false;
		static volatile bool terminateThread = false;

		static public void Dispose()
		{
			terminateThread = true; // will terminate worker thread
			Enqueue(new SoundEffectInstance.WorkItem(null, SoundEffectInstance.WorkItem.NullAction)); // Pump queue with null action
			isDisposed = true;
		}

		static private void Worker(object state)
		{
			while(!terminateThread)
			{
				SoundEffectInstance.WorkItem workItem;

				lock(workQueue)
				{
					if(workQueue.Count == 0)
						Monitor.Wait(workQueue);

					workItem = workQueue.Dequeue();
				}

				try
				{
					workItem.action(workItem.sei, workItem.value);
				}
				catch
				{
					Debug.Assert(false, "Work item exception");
				}
			}
		}

		#region Special Actions

		static internal SoundState GetState(InternalSoundEffectInstance instance)
		{
			lock(workQueue) // TODO: it'd be nice if this wasn't synchronous
			{
				return instance.State;
			}
		}

		static internal void RestartAllRestarable()
		{
			Debug.WriteLine("SoundEffectThread.RestartAllRestartable");
			// Restart on the audio queue thread
			Enqueue(new SoundEffectInstance.WorkItem(null, (i, v) => InternalSoundEffectInstance.RestartAllRestartable()));
		}

		#endregion

	}



	public class SoundEffectInstance : IDisposable
	{

		#region Threading
		
		internal struct WorkItem
		{
			public InternalSoundEffectInstance sei;
			public Action<InternalSoundEffectInstance, float> action;
			public float value;

			public WorkItem(InternalSoundEffectInstance sei,
					Action<InternalSoundEffectInstance, float> action)
			{
				this.sei = sei;
				this.action = action;
				this.value = 0f;
			}

			public WorkItem(InternalSoundEffectInstance sei,
					Action<InternalSoundEffectInstance, float> action, float value)
			{
				this.sei = sei;
				this.action = action;
				this.value = value;
			}

			internal static Action<InternalSoundEffectInstance, float> NullAction = (s, v) => { };
			internal static Action<InternalSoundEffectInstance, float> Play = (s, v) => { s.Play(); };
			internal static Action<InternalSoundEffectInstance, float> Stop = (s, v) => { s.Stop(); };
			internal static Action<InternalSoundEffectInstance, float> Pause = (s, v) => { s.Pause(); };
			internal static Action<InternalSoundEffectInstance, float> SetVolume = (s, v) => { s.SetVolume(v); };
			internal static Action<InternalSoundEffectInstance, float> SetLooped = (s, v) => { s.SetLooped(); };
			internal static Action<InternalSoundEffectInstance, float> ClearLooped = (s, v) => { s.ClearLooped(); };
			internal static Action<InternalSoundEffectInstance, float> Setup = (s, v) => { s.Setup(); };
			internal static Action<InternalSoundEffectInstance, float> Dispose = (s, v) => { s.Dispose(); };
		}

		#endregion


		InternalSoundEffectInstance instance;

		internal SoundEffectInstance(SoundEffect soundEffect, bool hardware)
		{
			instance = new InternalSoundEffectInstance(soundEffect, hardware);
			SoundEffectThread.Enqueue(new WorkItem(instance, WorkItem.Setup));
		}


		#region IDisposable Members

		public bool IsDisposed
		{
			get { return instance == null; }
		}

		public void Dispose()
		{
			SoundEffectThread.Enqueue(new WorkItem(instance, WorkItem.Dispose));
			instance = null;
		}

		#endregion


		#region Play Controls (XNA API)

		public void Play()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			instance.readyToReFire = false;
			SoundEffectThread.Enqueue(new WorkItem(instance, WorkItem.Play));
		}

		public void Resume()
		{
			Play();
		}

		public void Stop()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			SoundEffectThread.Enqueue(new WorkItem(instance, WorkItem.Stop));
		}

		public void Pause()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			SoundEffectThread.Enqueue(new WorkItem(instance, WorkItem.Pause));
		}

		#endregion

		
		#region Sound State (XNA API)

		private float _volume = 1;
		public float Volume
		{
			get { return _volume; }
			set
			{
				if(IsDisposed)
					throw new ObjectDisposedException(this.ToString());

				_volume = MathHelper.Clamp(value, 0, 1);
				SoundEffectThread.Enqueue(new WorkItem(instance, WorkItem.SetVolume, _volume));
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

				if(_isLooped != value)
				{
					_isLooped = value;
					SoundEffectThread.Enqueue(new WorkItem(instance, value ? WorkItem.SetLooped : WorkItem.ClearLooped));
				}
			}
		}

		public SoundState State
		{
			get
			{
				if(IsDisposed)
					throw new ObjectDisposedException(this.ToString());

				return SoundEffectThread.GetState(instance);
			}
		}

		#endregion


		// Only for use with SoundEffect
		internal bool ReadyToReFire { get { return instance.readyToReFire; } }
	}

}
