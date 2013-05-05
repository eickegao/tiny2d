using System.IO;
using System;
using MonoTouch.UIKit;
using System.Drawing;
using Microsoft.Xna.Framework.Content;
using OpenTK.Graphics.ES11;
using MonoTouch.CoreGraphics;

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


		#region Platform decode data

		static Texture2D FromUIImage(UIImage uiImage, string name)
		{
			All filter = All.Linear;

			CGImage image = uiImage.CGImage;
			if(uiImage == null)
				throw new ArgumentNullException("uiImage");

			// TODO: could use this to implement lower-bandwidth textures
			//bool hasAlpha = (image.AlphaInfo == CGImageAlphaInfo.First || image.AlphaInfo == CGImageAlphaInfo.Last
			//		|| image.AlphaInfo == CGImageAlphaInfo.PremultipliedFirst || image.AlphaInfo == CGImageAlphaInfo.PremultipliedLast);

			// Image dimentions:
			Point logicalSize = new Point((int)uiImage.Size.Width, (int)uiImage.Size.Height);

			int pixelWidth = uiImage.CGImage.Width;
			int pixelHeight = uiImage.CGImage.Height;

			// Round up the target texture width and height to powers of two:
			int potWidth = pixelWidth;
			int potHeight = pixelHeight;
			if(( potWidth & ( potWidth-1)) != 0) { int w = 1; while(w <  potWidth) { w *= 2; }  potWidth = w; }
			if((potHeight & (potHeight-1)) != 0) { int h = 1; while(h < potHeight) { h *= 2; } potHeight = h; }
			
			// Scale down textures that are too large...
			CGAffineTransform transform = CGAffineTransform.MakeIdentity();
			while((potWidth > 1024) || (potHeight > 1024))
			{
				potWidth /= 2;    // Note: no precision loss - it's a power of two
				potHeight /= 2;
				pixelWidth /= 2;  // Note: precision loss - assume possibility of dropping a pixel at each step is ok
				pixelHeight /= 2;
				transform.Multiply(CGAffineTransform.MakeScale(0.5f, 0.5f));
			}

			lock(textureLoadBufferLockObject)
			{
				CreateTextureLoadBuffer();

				unsafe
				{
					fixed(byte* data = textureLoadBuffer)
					{
						using(var colorSpace = CGColorSpace.CreateDeviceRGB())
						using(var context = new CGBitmapContext(new IntPtr(data), potWidth, potHeight,
								8, 4 * potWidth, colorSpace, CGImageAlphaInfo.PremultipliedLast))
						{
							context.ClearRect(new RectangleF(0, 0, potWidth, potHeight));
							context.TranslateCTM(0, potHeight - pixelHeight); // TODO: this does not play nice with the precision-loss above (keeping half-pixel to the edge)

							if(!transform.IsIdentity)
								context.ConcatCTM(transform);

							context.DrawImage(new RectangleF(0, 0, image.Width, image.Height), image);

							uint textureId = 0;
							/*textureId = new uint[1];
							textureId[0]= 0;
							GL.GenTextures(1,textureId);*/
							GL.GenTextures(1, ref textureId);
							GL.BindTexture(All.Texture2D, textureId);
							GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)filter);
							GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)filter);
							GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, (int)potWidth, (int)potHeight, 0, All.Rgba, All.UnsignedByte, new IntPtr(data));

							return new Texture2D(logicalSize.X, logicalSize.Y,
									pixelWidth, pixelHeight, potWidth, potHeight,
									textureId, name);
						}
					}
				}
			}
		}

		#endregion


		#region Load-From Functions

		/// <summary>
		/// Load a texture from the iPhone app bundle.
		/// On a Retina Display device, this will attempt to load the @2x variant first.
		/// </summary>
		public static Texture2D FromBundle(GraphicsDevice graphicsDevice, string filename)
		{
			UIImage image = UIImage.FromBundle(filename);
			if(image == null)
				throw new ContentLoadException("Error loading \"" + filename + "\" from bundle");

			return FromUIImage(image, Path.GetFileNameWithoutExtension(filename));
		}

		/// <summary>
		/// Load a texture from a file.
		/// </summary>
		public static Texture2D FromFile(GraphicsDevice graphicsDevice, string filename)
		{
			UIImage image = UIImage.FromFile(filename);
			if(image == null)
				throw new ContentLoadException("Error loading \"" + filename + "\"");

			return FromUIImage(image, Path.GetFileNameWithoutExtension(filename));
		}

		#endregion

	}
}

