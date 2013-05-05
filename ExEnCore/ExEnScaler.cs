using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.ES11;
using System.Diagnostics;

namespace Microsoft.Xna.Framework
{
	// This enumeration maps to iOS's UIInterfaceOrientation
	public enum ExEnInterfaceOrientation
	{
		Portrait,
		PortraitUpsideDown,
		LandscapeRight,
		LandscapeLeft
	}

	/// <summary>
	/// Handles transformations between logical and real display coordinates
	/// which is used to for handling device orientation and retina display.
	/// </summary>
	public class ExEnScaler
	{

		#region Configuration

		public ExEnScaler(ExEnInterfaceOrientation orientation, Point renderbufferSize, Point deviceSize)
		{
			Change(orientation, renderbufferSize, deviceSize);
		}
		
		public void Change(ExEnInterfaceOrientation orientation, Point renderbufferSize, Point deviceSize)
		{
			this.orientation = orientation;
			this.renderbufferSize = renderbufferSize;
			this.deviceSize = deviceSize;
			Recalculate();
		}
		
		
		// The Android backing surface is auto-rotated and resized by the operating system,
		// iOS does not support this (on the fast path on all versions - see ExEnEmTouchGameView.cs)
		// so we do its rotation for it.
#if ANDROID
		private const bool systemHandlesOrientationItself = true;
#else
		private const bool systemHandlesOrientationItself = false;
#endif
		
		ExEnInterfaceOrientation orientation;
		/// <summary>The screen orientation.</summary>
		public ExEnInterfaceOrientation Orientation
		{
			get { return orientation; }
			set { if(orientation != value) { orientation = value; Recalculate(); } }
		}

		Point renderbufferSize;
		/// <summary>Size of the render buffer. Either DeviceSize or double it due to retina display.</summary>
		public Point RenderbufferSize
		{
			get { return renderbufferSize; }
			set { renderbufferSize = value; Recalculate(); }
		}

		Point deviceSize;
		/// <summary>Size of the device in portrait orientation (size of the touch surface).</summary>
		public Point DeviceSize
		{
			get { return deviceSize; }
			set { deviceSize = value; Recalculate(); }
		}
		
		
		// TODO: split DeviceSize into reported device size from OS and logical device size for XNA
		//       for handling iPad-as-huge-retina-display-phone mode and other fancy scaler tricks.

		#endregion


		#region Matricies

		// Rotate the display to match the orientation
		// Because the projected area is always (-1,-1) to (1,1) - no need to scale or translate this
		Matrix projection;

		FractionTransform2D logicalToRender;
		FractionTransform2D touchToLogical;

		#endregion


		#region Derived Information

		public int AssetLoadScale { get; private set; }

		/// <summary>The size of the client area, after rotation.</summary>
		public Point ClientSize { get; private set; }

		#endregion


		#region Recalculate

		public event Action Changed;

		void Recalculate()
		{
			FractionTransform2D rotate = FractionTransform2D.Identity;
			FractionTransform2D inverseRotate = FractionTransform2D.Identity;
			
			if(!systemHandlesOrientationItself)
			{
				// Setup projection matrix and client size
				switch(orientation)
				{
					case ExEnInterfaceOrientation.Portrait:
						projection = Matrix.Identity;
						// rotation matrix is identity
						ClientSize = deviceSize;
						break;
	
					case ExEnInterfaceOrientation.PortraitUpsideDown:
						projection = Matrix.CreateRotationZ(MathHelper.Pi);
						rotate = inverseRotate = FractionTransform2D.CreateRotation(-1, 0);
						ClientSize = deviceSize;
						break;
	
					case ExEnInterfaceOrientation.LandscapeLeft:
						projection = Matrix.CreateRotationZ(MathHelper.PiOver2);
						rotate = FractionTransform2D.CreateRotation(0, 1);
						inverseRotate = FractionTransform2D.CreateRotation(0, -1);
						ClientSize = new Point(deviceSize.Y, deviceSize.X);
						break;
	
					case ExEnInterfaceOrientation.LandscapeRight:
						projection = Matrix.CreateRotationZ(3 * MathHelper.PiOver2);
						rotate = FractionTransform2D.CreateRotation(0, -1);
						inverseRotate = FractionTransform2D.CreateRotation(0, 1);
						ClientSize = new Point(deviceSize.Y, deviceSize.X);
						break;
				}
			}
			else
			{
				projection = Matrix.Identity;
				ClientSize = deviceSize;
			}

			// Setup transformation matrices:
			// Orthographic projection into projection space, use orientation projection matrix,
			// inverse orthographic project back to new client space
			logicalToRender = FractionTransform2D.CreateOrthographic(0, ClientSize.X, ClientSize.Y, 0)
					* rotate * FractionTransform2D.CreateOrthographicInverse(0, renderbufferSize.X, 0, renderbufferSize.Y);
			touchToLogical = FractionTransform2D.CreateOrthographic(0, deviceSize.X, deviceSize.Y, 0)
					* inverseRotate * FractionTransform2D.CreateOrthographicInverse(0, ClientSize.X, ClientSize.Y, 0);

			// Setup asset load scale
			float deviceScale = Math.Max((float)renderbufferSize.X / (float)deviceSize.X, (float)renderbufferSize.Y / (float)deviceSize.Y);
			AssetLoadScale = deviceScale > 1.5f ? 2 : 1;

			if(Changed != null)
				Changed();
		}

		#endregion


		#region OpenGL Matrix Functions

		/// <summary>Set matrix to projection and initialize it suitable for the device orientation.</summary>
		public void SetMatrixModeProjection()
		{
			GL.MatrixMode(All.Projection);
			GL.LoadMatrix(ref projection.M11);
		}

		#endregion


		#region Transformations

		public Point TouchToLogical(int x, int y)
		{
			return touchToLogical.Transform(new Point(x, y));
		}

		public void LogicalToRender(ref Rectangle rectangle)
		{
			Point a = new Point(rectangle.X, rectangle.Y);
			Point b = new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height);
			a = logicalToRender.Transform(a);
			b = logicalToRender.Transform(b);

			Point min, max;
			if(a.X < b.X) { min.X = a.X; max.X = b.X; } else { max.X = a.X; min.X = b.X; }
			if(a.Y < b.Y) { min.Y = a.Y; max.Y = b.Y; } else { max.Y = a.Y; min.Y = b.Y; }

			rectangle.X = min.X;
			rectangle.Y = min.Y;
			rectangle.Width = (max.X-min.X);
			rectangle.Height = (max.Y-min.Y);
		}

		#endregion

	}
}
