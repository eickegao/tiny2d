using System;
using Microsoft.Xna.Framework.Content;
using Java.IO;

namespace Microsoft.Xna.Framework
{
	public class Song : IDisposable
	{
		internal FileDescriptor GetFileDescriptor()
		{
			return ContentHelpers.GetAssetManager(contentManager).OpenFd(assetPath).FileDescriptor;
		}
		
		string assetPath;
		ContentManager contentManager;
		
		internal Song(string assetPath, ContentManager contentManager)
		{
			this.assetPath = assetPath;
			this.contentManager = contentManager;
		}


		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());
			IsDisposed = true;
		}

	}
}

