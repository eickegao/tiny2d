using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Microsoft.Xna.Framework.Content
{
	public static class ContentHelpers
	{
		public static string TryGetAssetFullPath(string assetName, ContentManager contentManager, string[] extensions)
		{
			// Generate base path for asset (no extension or @2x)
			// Switch out windows-style directory seperators for the platform separator
			string assetBasePath = contentManager.RootDirectory.Replace('\\', Path.DirectorySeparatorChar)
					+ Path.DirectorySeparatorChar + assetName.Replace('\\', Path.DirectorySeparatorChar);

			// Try each extension
			foreach(string extension in extensions)
			{
				string path = assetBasePath + extension;
				if(File.Exists(path))
					return path;
			}

			return null;
		}

		public static string GetAssetFullPath(string assetName, ContentManager contentManager, string[] extensions)
		{
			string fullPath = TryGetAssetFullPath(assetName, contentManager, extensions);
			if(fullPath != null)
				return fullPath;

			throw new ContentLoadException("Failed to load \"" + assetName
					+ "\", could not find a file with a valid extension. Remember that iOS is case-sensitive.");
		}
	}
}
