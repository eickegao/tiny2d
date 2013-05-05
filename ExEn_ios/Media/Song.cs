using System;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace Microsoft.Xna.Framework.Media
{
	public class Song
	{
		internal SoundEffect soundFile;

		internal Song(string assetName)
		{
			soundFile = new SoundEffect(assetName, true);
		}


		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());
			IsDisposed = true;

			if(soundFile != null)
				soundFile.Dispose();
		}

	}
}

