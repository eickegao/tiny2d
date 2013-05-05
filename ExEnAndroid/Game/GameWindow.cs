using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
	public class GameWindow
	{
		internal GameWindow()
		{
			CurrentOrientation = DisplayOrientation.Default;
			ClientBounds = new Rectangle(0, 0, 800, 600); // Approximately matches XNA
		}


		#region Window Size

		public Rectangle ClientBounds { get; internal set; }
		public event EventHandler<EventArgs> ClientSizeChanged;
		
		internal void DoClientSizeChange(Rectangle clientBounds)
		{
			this.ClientBounds = clientBounds;
			if(ClientSizeChanged != null)
				ClientSizeChanged(this, EventArgs.Empty);
		}

		#endregion


		#region Orientation

		public DisplayOrientation CurrentOrientation { get; private set; }
		public event EventHandler<EventArgs> OrientationChanged;

		internal void DoOrientationChange(DisplayOrientation orientation)
		{
			this.CurrentOrientation = orientation;
			if(OrientationChanged != null)
				OrientationChanged(this, EventArgs.Empty);
		}

		#endregion


		#region Non-functional on iOS

		public string Title { get { return string.Empty; } set { } }
		public IntPtr Handle { get { return IntPtr.Zero; } }
		public string ScreenDeviceName { get { return string.Empty; } }
		public bool AllowUserResizing { get { return false; } set { } }

#pragma warning disable 0067 // Event never used
		public event EventHandler<EventArgs> ScreenDeviceNameChanged;
#pragma warning restore 0067

		#endregion

	}
}
