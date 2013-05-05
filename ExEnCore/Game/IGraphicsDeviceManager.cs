using System;

namespace Microsoft.Xna.Framework
{
	public interface IGraphicsDeviceManager
	{
		bool BeginDraw();
		void CreateDevice();
		void EndDraw();
	}
}
