using System;
using System.Collections.Generic;
using Android.Content.Res;
using System.IO;

namespace Microsoft.Xna.Framework.Content
{
	public static class ContentHelpers
	{
		
		public static AssetManager GetAssetManager(ContentManager contentManager)
		{
			var activity = contentManager.ServiceProvider.GetService(typeof(ExEnAndroidActivity)) as ExEnAndroidActivity;
			if(activity == null)
				throw new InvalidOperationException("ContentManager has no ExEnAndroidActivity service");
			return activity.Assets;
		}
		
		
		public static string TryGetAssetFullPath(string assetName, ContentManager contentManager, string[] extensions)
		{
			// Switch out windows-style directory seperators for the platform separator
			// Generate base path for asset (no extension or @2x)
			string rootDirectory = contentManager.RootDirectory.Replace('\\', Path.DirectorySeparatorChar);
			if(rootDirectory == ".")
				rootDirectory = "";
			string assetBasePath = Path.Combine(rootDirectory, assetName.Replace('\\', Path.DirectorySeparatorChar));
			
			AssetManager assets = GetAssetManager(contentManager);
			
			string directory = Path.GetDirectoryName(assetBasePath);
			string assetFileNameWithoutExtension = Path.GetFileName(assetBasePath);
			string[] list = assets.List(directory);
			foreach(string extension in extensions)
			{
				foreach(string fileName in list)
				{
					if(fileName == assetFileNameWithoutExtension + extension)
					{
						return assetBasePath + extension;
					}
				}
			}

			return null;
		}

		public static string GetAssetFullPath(string assetName, ContentManager contentManager, string[] extensions)
		{
			string fullPath = TryGetAssetFullPath(assetName, contentManager, extensions);
			if(fullPath != null)
				return fullPath;

			throw new ContentLoadException("Failed to load \"" + assetName
					+ "\", could not find a file with a valid extension.");
		}
	}
}
