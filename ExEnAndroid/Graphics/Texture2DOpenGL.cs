using System;
using OpenTK.Graphics.ES11;

namespace Microsoft.Xna.Framework.Graphics
{
	// ExEn does not implement the XNA GraphicsResource class, inherit directly from IDisposable
	public partial class Texture2D : IDisposable
	{

		#region Texture Information

		// The logical size of the texture is the size as it appears to XNA
		internal int logicalWidth, logicalHeight;

		// The pixel size of the texture is the used area of the OpenGL surface
		internal int pixelWidth, pixelHeight;
		// The pot (power-of-two) size of the texture is actual size of the OpenGL surface, the used area is in the bottom left
		internal int potWidth, potHeight;

		// The OpenGL ID for the texture
		internal uint textureId;


		internal Texture2D(int logicalWidth, int logicalHeight, int pixelWidth, int pixelHeight,
				int potWidth, int potHeight, uint textureId, string name)
		{
			this.logicalWidth = logicalWidth;
			this.logicalHeight = logicalHeight;
			this.pixelWidth = pixelWidth;
			this.pixelHeight = pixelHeight;
			this.potWidth = potWidth;
			this.potHeight = potHeight;
			RecalculateRatio();

			this.textureId = textureId;

			this.Name = name;
		}

		#endregion


		#region Finalize and Dispose

		~Texture2D() { Dispose(false); }

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private bool disposed = false;
		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				GL.DeleteTextures(1, ref textureId);
				disposed = true;
			}
		}

		#endregion


		#region Texture-Coordinate Conversion

		// Ratio of XNA pixels to texture coordinates
		internal float texWidthRatio, texHeightRatio;

		internal void RecalculateRatio()
		{
			// Determine conversion ratio from pixels to texture-coordinates
			texWidthRatio  = ((float)pixelWidth  / (float)logicalWidth)  / (float)potWidth;
			texHeightRatio = ((float)pixelHeight / (float)logicalHeight) / (float)potHeight;
		}

		#endregion


		#region XNA API

		public string Name { get; set; }
		public string Tag { get; set; }

		public int Width { get { return logicalWidth; } }
		public int Height { get { return logicalHeight; } }
		public Rectangle Bounds
		{
			get { return new Rectangle(0, 0, logicalWidth, logicalHeight); }
		}

		#endregion

	}
}

