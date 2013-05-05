using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES11;

namespace Microsoft.Xna.Framework.Graphics
{
	public enum BlendFunction
	{
		Add = All.FuncAddOes,
		Subtract = All.FuncSubtractOes,
		ReverseSubtract = All.FuncReverseSubtractOes,
		
#if !ANDROID // not supported on Android
		Min = All.MinExt,
		Max = All.MaxExt,
#endif
	}

	public enum Blend
	{
		One = All.One,
		Zero = All.Zero,
		SourceColor = All.SrcColor,
		InverseSourceColor = All.OneMinusSrcColor,
		SourceAlpha = All.SrcAlpha,
		InverseSourceAlpha = All.OneMinusSrcAlpha,
		DestinationColor = All.DstColor,
		InverseDestinationColor = All.OneMinusDstColor,
		DestinationAlpha = All.DstAlpha,
		InverseDestinationAlpha = All.OneMinusDstAlpha,
		//BlendFactor = All.ConstantColor, // Not supported by OpenGL ES 1.1
		//InverseBlendFactor = All.OneMinusConstantColor, // Not supported by OpenGL ES 1.1
		SourceAlphaSaturation = All.SrcAlphaSaturate,
	} 

	public class BlendState
	{
		// Rather than fiddling with P/Invoke and extension methods to get blending working
		// I will simply implement this to match ExEnSilver for now. Might do a full implementation later.
		// An OpenGL ES 2.0 implementation could more easily implement everything.

		#region Static blend states

		public static readonly BlendState Additive;
		public static readonly BlendState AlphaBlend;
		public static readonly BlendState NonPremultiplied;
		public static readonly BlendState Opaque;

		static BlendState()
		{
			AlphaBlend = new BlendState(Blend.One, Blend.InverseSourceAlpha, "BlendState.AlphaBlend");
			NonPremultiplied = new BlendState(Blend.SourceAlpha, Blend.InverseSourceAlpha, "BlendState.NonPremultiplied");
			Additive = new BlendState(Blend.SourceAlpha, Blend.One, "BlendState.Additive");
			Opaque = new BlendState(Blend.One, Blend.Zero, "BlendState.Opaque");
		}

		#endregion

		public string Name { get; private set; }

		private Blend srcBlend, dstBlend;

		private BlendState(Blend srcBlend, Blend dstBlend, string name)
		{
			this.srcBlend = srcBlend;
			this.dstBlend = dstBlend;
			this.Name = name;
		}


		internal void Apply()
		{
			GL.Enable(All.Blend);
			GL.BlendFunc((All)srcBlend, (All)dstBlend);
		}

	}
}
