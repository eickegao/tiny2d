using System;
using Microsoft.Xna.Framework;
using MonoTouch.AudioToolbox;
using System.Collections.Generic;
using MonoTouch.CoreFoundation;

namespace Microsoft.Xna.Framework.Audio
{
	public partial class SoundEffect : IDisposable
	{

		#region Information for Audio Queue

		internal AudioFile audioFile;
		internal AudioStreamBasicDescription description;

		#region DeriveBufferSize from Apple Docs

		internal int bufferSize;
		internal int packetsToRead;

		void DeriveBufferSize(double seconds)
		{
			int maxPacketSize = audioFile.PacketSizeUpperBound; // (matches Apple's docs)

			// An upper bound for the audio queue buffer size, in bytes.
			// 320 KB: This corresponds to approximately five seconds of stereo, 24 bit audio at a sample rate of 96 kHz.
			const int maxBufferSize = 0x50000;

			// A lower bound for the audio queue buffer size, in bytes.
			// 16 KB.
			const int minBufferSize = 0x4000;

			if(description.FramesPerPacket != 0) // data formats that define a fixed number of frames per packet
			{
				// derive the audio queue buffer size
				double packetsForTime = description.SampleRate / description.FramesPerPacket * seconds;
				bufferSize = (int)(packetsForTime * maxPacketSize);
			}
			else // not a fixed number of frames per packet
			{
				// derive a reasonable audio queue buffer size based on the maximum packet size and the upper bound
				bufferSize = (maxBufferSize > maxPacketSize ? maxBufferSize : maxPacketSize);
			}

			if(bufferSize > maxBufferSize && bufferSize > maxPacketSize)
				bufferSize = maxBufferSize;
			else if(bufferSize < minBufferSize)
				bufferSize = minBufferSize;

			packetsToRead = bufferSize / maxPacketSize;
		}

		#endregion

		internal bool isVBR = false;

		#endregion


		internal SoundEffect(string assetName, bool isMusic)
		{
			// use of CFUrl.FromFile is necessary in case assetName contains spaces (which must be url-encoded)
			audioFile = AudioFile.Open(CFUrl.FromFile(assetName), AudioFilePermission.Read, 0);

			if(audioFile == null)
				throw new Content.ContentLoadException("Could not open sound effect " + assetName);

			description = audioFile.StreamBasicDescription;
			DeriveBufferSize(0.5);
			isVBR = (description.BytesPerPacket == 0 || description.FramesPerPacket == 0);

			if(!isMusic)
				firstInstance = new SoundEffectInstance(this, false);
		}


		SoundEffectInstance firstInstance = null;

		public SoundEffectInstance CreateInstance()
		{
			if(firstInstance != null)
			{
				SoundEffectInstance instance = firstInstance;
				firstInstance  = null;
				return instance;
			}
			else
				return new SoundEffectInstance(this, false);
		}


		public bool IsDisposed
		{
			get { return audioFile == null; }
		}

		public void Dispose()
		{
			DisposeFireAndForgetQueue();

			audioFile.Dispose();
			audioFile = null;
		}

	}
}

