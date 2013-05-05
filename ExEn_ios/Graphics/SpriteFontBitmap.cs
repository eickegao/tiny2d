using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Microsoft.Xna.Framework.Graphics
{
	internal static class SpriteFontBinaryReaderExtensions
	{
		public static Rectangle ReadRectangle(this BinaryReader br)
		{
			return new Rectangle(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
		}

		public static Vector3 ReadVector3(this BinaryReader br)
		{
			return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		}
	}


#if SILVERLIGHT
	public class SpriteFontBitmap : SpriteFont
#else
	public class SpriteFont
#endif
	{

		#region Public XNA Properties

		ReadOnlyCollection<char> _characters = null;

#if SILVERLIGHT
		override
#endif
		public ReadOnlyCollection<char> Characters
		{
			get
			{
				if(_characters == null)
					_characters = new ReadOnlyCollection<char>(characterData.Keys.ToList());
				return _characters;
			}
		}

#if SILVERLIGHT
		override
#endif
		public int LineSpacing { get; set; }

#if SILVERLIGHT
		override
#endif
		public float Spacing { get; set; }

#if SILVERLIGHT
		override
#endif
		public char? DefaultCharacter { get; set; }

		#endregion


		#region Metrics and Texture

		/// <summary>Internal scaling to apply, used to support iOS @2x fonts.</summary>
		internal float internalScale = 1f;

		struct GlyphData
		{
			public Rectangle Glyph;
			public Rectangle Cropping;

			/// <summary>Kerning data matches the ABCFLOAT structure from GetCharABCWidthsFloat from win32.</summary>
			public Vector3 Kerning;

			public override string ToString()
			{
				return string.Format("{{Glyph:{0}, Cropping:{1}, Kerning:{2}}}", Glyph, Cropping, Kerning);
			}
		}

		Dictionary<char, GlyphData> characterData = new Dictionary<char, GlyphData>();

		internal Texture2D texture;

		#endregion


		#region Construction

		internal
#if SILVERLIGHT
		SpriteFontBitmap
#else
		SpriteFont
#endif
		(Texture2D texture, Stream metricsDataStream, float internalScale)
		{
			this.texture = texture;
			this.internalScale = internalScale;

			using(BinaryReader br = new BinaryReader(metricsDataStream, Encoding.Unicode))
			{
				// Check for signature "ExEnFont"
				if(br.ReadInt32() != 0x6E457845 || br.ReadInt32() != 0x746E6F46)
					throw new ContentLoadException("Invalid ExEn font metrics file");
				if(br.ReadInt32() != 0)
					throw new ContentLoadException("Invalid version of ExEn font metrics file");

				// Read common properties:
				LineSpacing = br.ReadInt32();
				Spacing = br.ReadInt32();
				if(br.ReadBoolean())
					DefaultCharacter = br.ReadChar();
				else
					DefaultCharacter = null;

				// Read glyph list:
				int count = br.ReadInt32();
				for(int i = 0; i < count; i++)
				{
					char c = br.ReadChar();
					GlyphData g = new GlyphData();
					g.Glyph = br.ReadRectangle();
					g.Cropping = br.ReadRectangle();
					g.Kerning = br.ReadVector3();
					characterData.Add(c, g);
				}
			}
		}

		#endregion


		#region Measuring

#if SILVERLIGHT
		override
#endif
		public Vector2 MeasureString(string text)
		{
			if(string.IsNullOrEmpty(text))
				return Vector2.Zero;

			// NOTE: XNA will remove negative A and C spacing at the beginning and ends of lines

			// Spacing is from GetCharABCWidthsFloat
			int additionalLineCount = 0;
			float lineHeight = LineSpacing;
			bool lineStart = true; // if true, ignore A spacing
			float afterCharacterSpace = 0f; // delay C spacing until the following character is added
			float lineWidth = 0f;
			float blockWidth = 0f;

			foreach(char c in text)
			{
				if(c == '\r')
					continue;

				if(c == '\n')
				{
					blockWidth = Math.Max(lineWidth, blockWidth);
					lineWidth = 0;
					lineStart = true;
					additionalLineCount++;
					continue;
				}

				if(!characterData.ContainsKey(c))
					continue;

				// Add spacing from the previous character, if there is one
				if(!lineStart)	
					lineWidth += afterCharacterSpace + Spacing;

				Vector3 kerning = characterData[c].Kerning;
				lineWidth += kerning.X + kerning.Y; // A and B space
				afterCharacterSpace = kerning.Z;

				lineHeight = Math.Max(characterData[c].Cropping.Height, lineHeight);

				lineStart = false;
			}

			blockWidth = Math.Max(lineWidth, blockWidth);
			return new Vector2(blockWidth * internalScale, (lineHeight + additionalLineCount * LineSpacing) * internalScale);
		}

#if SILVERLIGHT
		override
#endif
		public Vector2 MeasureString(StringBuilder text)
		{
			return MeasureString(text.ToString());
		}

		#endregion


		#region Drawing

#if SILVERLIGHT
		override
#endif
		internal void InternalDrawString(SpriteBatch sb, string text, Vector2 position,
				Color color, float rotation, Vector2 origin, Vector2 scale,
				SpriteEffects effects, float layerDepth)
		{
			if(effects != SpriteEffects.None)
				throw new NotImplementedException("Flipped text is not implemented");

			Vector2 offset = origin / internalScale;
			bool lineStart = true;

			foreach(char c in text)
			{
				if(c == '\r')
					continue;

				if(c == '\n')
				{
					offset.Y -= LineSpacing;
					offset.X = origin.X;
					lineStart = true;
					continue;
				}

				if(characterData.ContainsKey(c) == false)
					continue; // TODO: Make this match XNA behaviour

				GlyphData g = characterData[c];

				if(!lineStart)
					offset.X -= g.Kerning.X; // add A spacing

				sb.InternalDraw(texture, position, g.Glyph,
						color, rotation, offset - new Vector2(g.Cropping.X, g.Cropping.Y),
						scale * internalScale, SpriteEffects.None, layerDepth);

				offset.X -= (g.Kerning.Y + g.Kerning.Z + Spacing); // add B, C and user spacing

				lineStart = false;
			}
		}

#if SILVERLIGHT
		override
#endif
		internal void InternalDrawString(SpriteBatch sb, StringBuilder text, Vector2 position,
				Color color, float rotation, Vector2 origin, Vector2 scale,
				SpriteEffects effects, float layerDepth)
		{
			InternalDrawString(sb, text.ToString(), position, color, rotation, origin, scale, effects, layerDepth);
		}

		#endregion

	}
}
