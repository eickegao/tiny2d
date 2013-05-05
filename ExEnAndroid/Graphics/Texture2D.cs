using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using OpenTK.Graphics.ES11;
using Android.Graphics;
using Android.Opengl;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture2D
	{
		#region Buffer for Loading Texture

		static object textureLoadBufferLockObject = new object();
		static byte[] textureLoadBuffer = null;
		const int textureLoadBufferLength = 1024 * 1024 * 4; // 4MB = 1024*1024 RGBA texture

		/// <summary>Requires textureLoadBufferLockObject is locked!</summary>
		static void CreateTextureLoadBuffer()
		{
			if(textureLoadBuffer == null)
				textureLoadBuffer = new byte[textureLoadBufferLength];
		}

		#endregion
		
		
		
		// As per XNA API:
		public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
		{
			return FromStream(graphicsDevice, stream, string.Empty);
		}
		
		internal static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream, string name)
		{
			All filter = All.Linear;
			
			Bitmap sourceBitmap = BitmapFactory.DecodeStream(stream);
			int pixelWidth = sourceBitmap.Width;
			int pixelHeight = sourceBitmap.Height;
			
			// Scale up to the next power-of-two
			// TODO: check device capabilities to see if this is necessary:
			int potWidth = pixelWidth;
			int potHeight = pixelHeight;
			if(( potWidth & ( potWidth-1)) != 0) { int w = 1; while(w <  potWidth) { w *= 2; }  potWidth = w; }
			if((potHeight & (potHeight-1)) != 0) { int h = 1; while(h < potHeight) { h *= 2; } potHeight = h; }
			
			// TODO: optimise this!
			int[] data = new int[potWidth * potHeight];
			sourceBitmap.GetPixels(data, 0, potWidth, 0, 0, pixelWidth, pixelHeight);
			Bitmap bitmap = Bitmap.CreateBitmap(potWidth, potHeight, sourceBitmap.GetConfig());
			bitmap.SetPixels(data, 0, potWidth, 0, 0, potWidth, potHeight);
			
			uint textureId = 0;
			GL.GenTextures(1, ref textureId);
			GL.BindTexture(All.Texture2D, textureId);
			GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)filter);
			GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)filter);
			GLUtils.TexImage2D((int)All.Texture2D, 0, bitmap, 0);
		
			return new Texture2D(pixelWidth, pixelHeight, pixelWidth, pixelHeight,
					potWidth, potHeight, textureId, name);
		}
		
	}
}

