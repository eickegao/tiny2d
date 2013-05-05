using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework.Content
{
	internal static class BuiltInLoaders
	{
		static readonly string[] texture2DExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".tif", ".tiff", ".gif" };
		static readonly string[] soundEffectExtensions = { ".wav", ".mp3", ".aiff", ".ac3" };
		static readonly string[] spriteFontTextureExtensions = { "-exenfont.png" };
		static readonly string[] spriteFontMetricsExtensions = { "-exenfont.exenfont" };
		static readonly string[] spriteFontTextureAt2xExtensions = { "-exenfont@2x.png" };
		static readonly string[] spriteFontMetricsAt2xExtensions = { "-exenfont@2x.exenfont" };


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
			return Texture2D.FromBundle(GetGraphicsDevice(contentManager), assetPath);
		}

		static SpriteFont LoadSpriteFont(string assetName, ContentManager contentManager)
		{
			GraphicsDevice graphicsDevice = GetGraphicsDevice(contentManager);

			string texturePath = null;
			string metricsPath = null;
			if(MonoTouch.UIKit.UIScreen.MainScreen.Scale > 1.0)
			{
				texturePath = ContentHelpers.TryGetAssetFullPath(assetName, contentManager, spriteFontTextureAt2xExtensions);
				metricsPath = ContentHelpers.TryGetAssetFullPath(assetName, contentManager, spriteFontMetricsAt2xExtensions);

				if(texturePath == null && metricsPath != null)
					throw new ContentLoadException("@2x texture file for font \"" + assetName + "\" is missing");
				if(texturePath != null && metricsPath == null)
					throw new ContentLoadException("@2x metrics file for font \"" + assetName + "\" is missing");
			}

			bool loadingFontAt2x = (texturePath != null && metricsPath != null);
			if(!loadingFontAt2x)
			{
				texturePath = ContentHelpers.TryGetAssetFullPath(assetName, contentManager, spriteFontTextureExtensions);
				metricsPath = ContentHelpers.TryGetAssetFullPath(assetName, contentManager, spriteFontMetricsExtensions);
			}

			Texture2D texture = Texture2D.FromFile(GetGraphicsDevice(contentManager), texturePath);
			if(texture == null)
				throw new ContentLoadException("Failed to load texture \"" + texturePath + "\" for font \"" + assetName + "\"");

			// Disable retina scaling (it will be handled by SpriteFont)
			texture.logicalWidth = texture.pixelWidth;
			texture.logicalHeight = texture.pixelHeight;
			texture.RecalculateRatio();

			using(FileStream metricsStream = File.Open(metricsPath, FileMode.Open, FileAccess.Read))
			{
				return new SpriteFont(texture, metricsStream, loadingFontAt2x ? 0.5f : 1f);
			}
		}


		static SoundEffect LoadSoundEffect(string assetName, ContentManager contentManager)
		{
			string assetPath = ContentHelpers.GetAssetFullPath(assetName, contentManager, soundEffectExtensions);
			return new SoundEffect(assetPath, false);
		}

		static Song LoadSong(string assetName, ContentManager contentManager)
		{
			string assetPath = ContentHelpers.GetAssetFullPath(assetName, contentManager, soundEffectExtensions);
			return new Song(assetPath);
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
