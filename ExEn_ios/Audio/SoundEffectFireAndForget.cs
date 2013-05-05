using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio
{
	public partial class SoundEffect : IDisposable
	{
		Queue<SoundEffectInstance> fireAndForgetQueue = new Queue<SoundEffectInstance>();

		public bool Play()
		{
			return Play(1, 0, 0);
		}

		public bool Play(float volume, float pitch, float pan)
		{
			SoundEffectInstance instance = null;

			if(fireAndForgetQueue.Count > 0)
			{
				SoundEffectInstance i = fireAndForgetQueue.Peek();
				if(i.ReadyToReFire)
				{
					instance = fireAndForgetQueue.Dequeue();
					instance.Stop(); // Rewind
				}
			}

			if(instance == null)
				instance = CreateInstance();

			instance.Volume = volume;
			instance.Play();

			fireAndForgetQueue.Enqueue(instance);

			return true;
		}

		void DisposeFireAndForgetQueue()
		{
			foreach(var instance in fireAndForgetQueue)
				instance.Dispose();
			fireAndForgetQueue.Clear();
		}
	}
}

