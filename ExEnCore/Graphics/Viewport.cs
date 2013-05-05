using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public struct Viewport
	{
		public Viewport(int x, int y, int width, int height)
		{
			this.X = x; this.Y = y; this.Width = width; this.Height = height;
		}

		public Viewport(Rectangle bounds)
		{
			X = bounds.X; Y = bounds.Y; Width = bounds.Width; Height = bounds.Height;
		}


		public int X;
		public int Y;
		public int Width;
		public int Height;

		public Rectangle TitleSafeArea
		{
			// The only platform that has a defined title-safe region is Xbox 360
			get { return new Rectangle(X, Y, Width, Height); }
		}

		public Rectangle Bounds
		{
			get { return new Rectangle(X, Y, Width, Height); }
			set
			{
				this.X = value.X;
				this.Y = value.Y;
				this.Width = value.Width;
				this.Height = value.Height;
			}
		}

		public float AspectRatio
		{
			get { return Height != 0 ? (float)Width / (float)Height : 0f; }
		}
	}
}
