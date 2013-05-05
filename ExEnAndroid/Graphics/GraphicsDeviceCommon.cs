using System;
using OpenTK.Graphics.ES11;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class GraphicsDevice : IDisposable
	{

		#region Constructor, Scaler and Presentation Parameters

		private PresentationParameters _presentationParameters = new PresentationParameters();
		public PresentationParameters PresentationParameters { get { return _presentationParameters; } }

		public ExEnScaler Scaler { get; private set; }

		internal GraphicsDevice(ExEnScaler scaler)
		{
			this.Scaler = scaler;
			Scaler.Changed += new Action(ScalerWasChanged);
			ScalerWasChanged();
		}

		internal void ScalerWasChanged()
		{
			PresentationParameters.BackBufferWidth = Scaler.ClientSize.X;
			PresentationParameters.BackBufferHeight = Scaler.ClientSize.Y;
			PresentationParameters.IsFullScreen = true; // Always full screen

			PresentationParameters.ExEnInterfaceOrientation = Scaler.Orientation;

			// NOTE: the XNA Device orientation (and the iOS *device* orientation for that matter)
			//       have LandscapeLeft and LandscapeRight reversed compared to the iOS *interface* orientation
			switch(Scaler.Orientation)
			{
				case ExEnInterfaceOrientation.LandscapeLeft: PresentationParameters.DisplayOrientation = DisplayOrientation.LandscapeRight; break;
				case ExEnInterfaceOrientation.LandscapeRight: PresentationParameters.DisplayOrientation = DisplayOrientation.LandscapeLeft; break;

				case ExEnInterfaceOrientation.Portrait:
				case ExEnInterfaceOrientation.PortraitUpsideDown:
					PresentationParameters.DisplayOrientation = DisplayOrientation.Portrait;
					break;

				default: PresentationParameters.DisplayOrientation = DisplayOrientation.Default; break;
			}

			Viewport = new Viewport(PresentationParameters.Bounds);
			
			TouchPanel.DisplayWidth = PresentationParameters.BackBufferWidth;
			TouchPanel.DisplayHeight = PresentationParameters.BackBufferHeight;
			TouchPanel.DisplayOrientation = PresentationParameters.DisplayOrientation;
		}

		#endregion


		#region Viewport

		private Viewport viewport;
		public Viewport Viewport
		{
			get { return viewport; }
			set
			{
				viewport = value;

				Rectangle r = new Rectangle(value.X, value.Y, value.Width, value.Height);
				Scaler.LogicalToRender(ref r);
				GL.Viewport(r.X, r.Y, r.Width, r.Height);
			}
		}


		public void SetupClientProjection()
		{
			Scaler.SetMatrixModeProjection();
			GL.Ortho(0, viewport.Width, viewport.Height, 0, -1, 1);
		}

		#endregion


		#region Disposal

		public bool IsDisposed { get; private set; }

		public void Dispose()
		{
			IsDisposed = true;
		}

		#endregion


		public void Clear(Color color)
		{
			Vector4 vector = color.ToVector4();
			GL.ClearColor(vector.X, vector.Y, vector.Z, vector.W);
			GL.Clear((uint)All.ColorBufferBit);
		}

	}
}

