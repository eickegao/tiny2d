using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Graphics.ES11;


namespace Microsoft.Xna.Framework.Graphics
{
	[StructLayout(LayoutKind.Sequential)]
	struct SpriteVertex
	{
		public Vector2 position;
		public Vector2 uv;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct SpriteVertices
	{
		public SpriteVertex v0;
		public SpriteVertex v1;
		public SpriteVertex v2;
		public SpriteVertex v3;
	}


	public enum SpriteSortMode
	{
		Deferred,

		// These are not supported and currently behave as Deferred
		Immediate,
		Texture,
		BackToFront,
		FrontToBack,
	}

	// Should be impossible to construct
	public class Effect { private Effect() { } }


	public class SpriteBatch : IDisposable
	{
		private static Matrix identityMatrix = Matrix.Identity;

		private GraphicsDevice device;
		public GraphicsDevice GraphicsDevice { get { return device; } }


		#region XNA API Overloads

		#region Begin()

		public void Begin()
		{
			InternalBegin(BlendState.AlphaBlend, ref identityMatrix);
		}

		public void Begin(SpriteSortMode sortMode, BlendState blendState)
		{
			InternalBegin(blendState, ref identityMatrix);
		}

		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
		{
			InternalBegin(blendState, ref identityMatrix);
		}

		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
		{
			InternalBegin(blendState, ref identityMatrix);
		}

		public void Begin(SpriteSortMode sortMode, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect, Matrix transformMatrix)
		{
			InternalBegin(blendState, ref transformMatrix);
		}

		#endregion

		#region Draw()

		public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
		{
			if(texture == null) throw new ArgumentNullException("texture");

			if(destinationRectangle.Width == 0 || destinationRectangle.Height == 0)
				return;

			Vector2 scale = ScaleForDestination(destinationRectangle.Width,
					destinationRectangle.Height, texture.Width, texture.Height);
			Vector2 position = new Vector2(destinationRectangle.X, destinationRectangle.Y);

			InternalDraw(texture, position, null, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
		
		public void Draw(Texture2D texture, Vector2 position, Color color)
		{
			if(texture == null) throw new ArgumentNullException("texture");

			InternalDraw(texture, position, null, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
		}
		
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
		{
			if(texture == null) throw new ArgumentNullException("texture");

			if(destinationRectangle.Width == 0 || destinationRectangle.Height == 0)
				return;

			Vector2 scale;
			if(sourceRectangle.HasValue)
				scale = ScaleForDestination(destinationRectangle.Width, destinationRectangle.Height, sourceRectangle.Value.Width, sourceRectangle.Value.Height);
			else
				scale = ScaleForDestination(destinationRectangle.Width, destinationRectangle.Height, texture.Width, texture.Height);
			Vector2 position = new Vector2(destinationRectangle.X, destinationRectangle.Y);

			InternalDraw(texture, position, sourceRectangle, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
		
		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
		{
			if(texture == null) throw new ArgumentNullException("texture");

			InternalDraw(texture, position, sourceRectangle, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
		}
		
		public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth)
		{
			if(texture == null) throw new ArgumentNullException("texture");

			if(destinationRectangle.Width == 0 || destinationRectangle.Height == 0)
				return;

			Vector2 scale;
			if(sourceRectangle.HasValue)
				scale = ScaleForDestination(destinationRectangle.Width, destinationRectangle.Height, sourceRectangle.Value.Width, sourceRectangle.Value.Height);
			else
				scale = ScaleForDestination(destinationRectangle.Width, destinationRectangle.Height, texture.Width, texture.Height);
			Vector2 position = new Vector2(destinationRectangle.X, destinationRectangle.Y);

			InternalDraw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
		}

		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
			InternalDraw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
		}
		
		public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
			InternalDraw(texture, position, sourceRectangle, color, rotation, origin, new Vector2(scale), effects, layerDepth);
		}

		#endregion

		#region DrawString()
		
		public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
		{
			spriteFont.InternalDrawString(this, text, position, color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
		}

		public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
		{
			spriteFont.InternalDrawString(this, text, position, color, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 1);
		}

		public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
			spriteFont.InternalDrawString(this, text, position, color, rotation, origin, new Vector2(scale), effects, layerDepth);
		}

		public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
			spriteFont.InternalDrawString(this, text, position, color, rotation, origin, scale, effects, layerDepth);
		}

		public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
		{
			spriteFont.InternalDrawString(this, text, position, color, rotation, origin, scale, effects, layerDepth);
		}

		public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
		{
			spriteFont.InternalDrawString(this, text, position, color, rotation, origin, new Vector2(scale), effects, layerDepth);
		}
		
		#endregion

		public void End()
		{
			if(!inBatch || disposed)
				throw new InvalidOperationException("SpriteBatch is in incorrect state");

			RenderSprites(); // finish off any unrendered sprites

			inBatch = false;
		}

		#endregion


		#region Sprite Batch Data

		const int MaxSpritesPerBatch = 128; // I think this number was selected arbitrarily

		private IntPtr spriteArray; // SpriteVertices[MaxSpritesPerBatch]
		private IntPtr indexArray; // ushort[MaxIndexCount]

		#region Sprite Array Functions

		private int _spriteArrayCount = 0;
		private int SpriteCount { get { return _spriteArrayCount; } }

		private bool CanAddSprite { get { return _spriteArrayCount < MaxSpritesPerBatch; } }

		private unsafe void AddSprite(ref SpriteVertices sprite)
		{
			Debug.Assert(CanAddSprite);
			((SpriteVertices*)spriteArray)[_spriteArrayCount++] = sprite;
		}

		private void ClearSprites() { _spriteArrayCount = 0; }

		#endregion

		#region Index Array Setup

		const int IndicesPerSprite = 6; // Vertices per sprite is 4 as per SpriteVertices
		const int MaxIndexCount = MaxSpritesPerBatch * IndicesPerSprite;

		private unsafe void InitializeIndexArray()
		{
			ushort* ia = (ushort*)indexArray;
			for(int i = 0; i < MaxSpritesPerBatch; i++)
			{
				ia[i*6+0] = (ushort)(i*4+0);
				ia[i*6+1] = (ushort)(i*4+1);
				ia[i*6+2] = (ushort)(i*4+2);
				ia[i*6+3] = (ushort)(i*4+3);
				ia[i*6+4] = (ushort)(i*4+2);
				ia[i*6+5] = (ushort)(i*4+1);
			}
		}

		#endregion

		#endregion


		#region Construct, Dispose, Finalize

		public SpriteBatch(GraphicsDevice graphicsDevice)
		{
			this.device = graphicsDevice;

			unsafe
			{
				spriteArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SpriteVertices)) * MaxSpritesPerBatch);
				indexArray = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ushort)) * MaxIndexCount);
			}

			InitializeIndexArray();
		}

		bool disposed = false;

		public void Dispose()
		{
			if(disposed)
				throw new ObjectDisposedException(this.ToString());
		
			disposed = true;

			unsafe
			{
				Marshal.FreeHGlobal((IntPtr)spriteArray);
				spriteArray = IntPtr.Zero;
				Marshal.FreeHGlobal((IntPtr)indexArray);
				indexArray = IntPtr.Zero;
			}

			GC.SuppressFinalize(this);
		}

		~SpriteBatch()
		{
			Dispose();
		}

		#endregion


		#region Internal Rendering

		// Per batch state:
		bool inBatch = false;
		Texture2D lastTexture;
		Color lastColor; // TODO: move colour to vertex buffer, better match XNA performance profile

		private void InternalBegin(BlendState blendState, ref Matrix matrix)
		{
			if(inBatch || disposed)
				throw new InvalidOperationException("SpriteBatch is in incorrect state");
			inBatch = true;

			if(blendState == null)
				blendState = BlendState.AlphaBlend;

			// Set up OpenGL projection matrix (client to projection)
			device.SetupClientProjection();

			// Set up user matrix (world to client)
			GL.MatrixMode(All.Modelview);
			GL.LoadMatrix(ref matrix.M11); // would you believe that this is legal?

			// Initialize OpenGL states
			GL.Disable(All.DepthTest);
			GL.EnableClientState(All.VertexArray);

			blendState.Apply();

			lastTexture = null;
		}


		private Vector2 ScaleForDestination(int destWidth, int destHeight, int srcWidth, int srcHeight)
		{
			return new Vector2((float)destWidth  / (float)srcWidth,
			                   (float)destHeight / (float)srcHeight);
		}

		internal void InternalDraw(Texture2D texture, Vector2 position,
				Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin,
				Vector2 scale, SpriteEffects effects, float layerDepth)
		{
			if(texture == null)
				throw new ArgumentNullException("texture");
			if(!inBatch || disposed)
				throw new InvalidOperationException("SpriteBatch is in incorrect state");

			// Need to change batch
			if(texture != lastTexture || color != lastColor || !CanAddSprite)
			{
				if(lastTexture != null) // if lastTexture == null, this is the first batch
					RenderSprites();
				lastTexture = texture;
				lastColor = color;
			}

			Rectangle r = sourceRectangle.HasValue ? sourceRectangle.Value : texture.Bounds;

			// From Krab.Geometry.Transform2D.CreateTranslateScaleRotateTranslate
			float M11, M12, M21, M22, OffsetX, OffsetY;
			float cos = (float)Math.Cos(rotation);
			float sin = (float)Math.Sin(rotation);
			M11 =  cos * scale.X; M12 = sin * scale.X;
			M21 = -sin * scale.Y; M22 = cos * scale.Y;
			OffsetX = -origin.X * cos * scale.X + origin.Y * sin * scale.Y + position.X;
			OffsetY = -origin.Y * cos * scale.Y - origin.X * sin * scale.X + position.Y;

			// Initial sprite width and height:
			float w = r.Width;
			float h = r.Height;

			// Create and transform position vertices (from Krab.Geometry.Transform2D.Transform)
			SpriteVertices s = new SpriteVertices();
			s.v0.position.X = w * M11 + 0 * M21 + OffsetX;
			s.v0.position.Y = w * M12 + 0 * M22 + OffsetY;
			s.v1.position.X = w * M11 + h * M21 + OffsetX;
			s.v1.position.Y = w * M12 + h * M22 + OffsetY;
			s.v2.position.X = 0 * M11 + 0 * M21 + OffsetX;
			s.v2.position.Y = 0 * M12 + 0 * M22 + OffsetY;
			s.v3.position.X = 0 * M11 + h * M21 + OffsetX;
			s.v3.position.Y = 0 * M12 + h * M22 + OffsetY;

			// Setup texture coordinates
			int fh = (int)effects & 1; // SpriteEffects.FlipHorizontally = 1
			int fv = (int)effects >> 8; // SpriteEffects.FlipVertically = 256
			s.v0.uv.X = lastTexture.texWidthRatio * (r.X + ((1-fh) * r.Width));
			s.v1.uv.X = lastTexture.texWidthRatio * (r.X + ((1-fh) * r.Width));
			s.v2.uv.X = lastTexture.texWidthRatio * (r.X + ((  fh) * r.Width));
			s.v3.uv.X = lastTexture.texWidthRatio * (r.X + ((  fh) * r.Width));
			s.v0.uv.Y = lastTexture.texHeightRatio * (r.Y + ((  fv) * r.Height));
			s.v1.uv.Y = lastTexture.texHeightRatio * (r.Y + ((1-fv) * r.Height));
			s.v2.uv.Y = lastTexture.texHeightRatio * (r.Y + ((  fv) * r.Height));
			s.v3.uv.Y = lastTexture.texHeightRatio * (r.Y + ((1-fv) * r.Height));

			AddSprite(ref s);
		}


		private unsafe void RenderSprites()
		{
			if(SpriteCount == 0)
				return;

			// Required to set the TexCoordPointer two floats forward
			float* data = (float*)spriteArray;

			GL.Enable(All.Texture2D);

			// Set the colour
			Vector4 color = lastColor.ToVector4();
			GL.Color4(color.X, color.Y, color.Z, color.W);

			// Set client states so that the Texture Coordinate Array will be used during rendering
			GL.EnableClientState(All.TextureCoordArray);

			// Bind to the texture that is associated with this image
			GL.BindTexture(All.Texture2D, lastTexture.textureId);
			GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)All.ClampToEdge);

			// Set up the VertexPointer to point to the vertices we have defined
			GL.VertexPointer(2, All.Float, 16, new IntPtr(data));

			// Set up the TexCoordPointer to point to the texture coordinates we want to use
			GL.TexCoordPointer(2, All.Float, 16, new IntPtr(data + 2));

			// Draw the vertices to the screen
			if(SpriteCount > 1)
			{
				// Draw triangles
				GL.DrawElements(All.Triangles, SpriteCount * IndicesPerSprite, All.UnsignedShort, indexArray);
			}
			else
			{
				GL.DrawArrays(All.TriangleStrip, 0, 4);
			}

			// Disable as necessary
			GL.DisableClientState(All.TextureCoordArray);

			GL.Disable(All.Texture2D);

			ClearSprites();
		}

		#endregion

	}
}

