using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Android.Content.Res;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework.Content
{	
	internal static class BuiltInLoaders
	{
		static readonly string[] texture2DExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };
		static readonly string[] spriteFontTextureExtensions = { "-exenfont.png" };
		static readonly string[] spriteFontMetricsExtensions = { "-exenfont.exenfont" };
		static readonly string[] soundEffectExtensions = { ".wav", ".mp3", ".ogg", ".mp4", ".m4a", ".aac" };
		static readonly string[] songExtensions = { ".mp3", ".ogg", ".mp4", ".m4a", ".aac", ".wav" };
		
		
		static GraphicsDevice GetGraphicsDevice(ContentManager contentManager)
		{
			var gds = contentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
			if(gds == null)
				throw new InvalidOperationException("No graphics device service");
			var gd = gds.GraphicsDevice;
			if(gd == null)
				throw new InvalidOperationException("No graphics device");
			return gd;
		}		

		
		static Texture2D LoadTexture(string assetName, ContentManager contentManager)
		{
			string assetPath = ContentHelpers.GetAssetFullPath(assetName, contentManager, texture2DExtensions);

			using(Stream stream = ContentHelpers.GetAssetManager(contentManager).Open(assetPath))
			{
				// TODO: what does XNA use for the Name property on content-manager loaded textures?
				return Texture2D.FromStream(GetGraphicsDevice(contentManager), stream, assetName);
			}
		}
		
		
		static SpriteFont LoadSpriteFont(string assetName, ContentManager contentManager)
		{
			GraphicsDevice graphicsDevice = GetGraphicsDevice(contentManager);

			string texturePath = ContentHelpers.GetAssetFullPath(assetName, contentManager, spriteFontTextureExtensions);
			string metricsPath = ContentHelpers.GetAssetFullPath(assetName, contentManager, spriteFontMetricsExtensions);
			
			Texture2D texture;
			using(Stream stream = ContentHelpers.GetAssetManager(contentManager).Open(texturePath))
			{
				texture = Texture2D.FromStream(GetGraphicsDevice(contentManager), stream, texturePath);
			}

			using(Stream metricsStream = ContentHelpers.GetAssetManager(contentManager).Open(metricsPath))
			{
				return new SpriteFont(texture, metricsStream, 1f);
			}
		}
		
		
		static SoundEffect LoadSoundEffect(string assetName, ContentManager contentManager)
		{			
			string assetPath = ContentHelpers.GetAssetFullPath(assetName, contentManager, soundEffectExtensions);
			using(var afd = ContentHelpers.GetAssetManager(contentManager).OpenFd(assetPath))
			{
				return new SoundEffect(afd);
			}
		}
		
		
		static Song LoadSong(string assetName, ContentManager contentManager)
		{
			string assetPath = ContentHelpers.GetAssetFullPath(assetName, contentManager, songExtensions);
			return new Song(assetPath, contentManager);
		}
		

		static bool hasRegistered = false;
		internal static void Register()
		{
			if(!hasRegistered)
			{
				ContentManager.RegisterLoader<Texture2D>(LoadTexture);
				ContentManager.RegisterLoader<SpriteFont>(LoadSpriteFont);
				ContentManager.RegisterLoader<SoundEffect>(LoadSoundEffect);
				ContentManager.RegisterLoader<Song>(LoadSong);
				hasRegistered = true;
			}
		}
	}
}
