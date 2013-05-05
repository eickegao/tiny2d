using Microsoft.Xna.Framework;
using System;

namespace Microsoft.Xna.Framework.Input
{
	public enum GamePadDeadZone
	{
		None=0,
		IndependentAxes=1,
		Circular=2,
	}

	public static class GamePad
	{
		// Not Implemented
		//public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex) { return new GamePadCapabilities(); }
		
		public static GamePadState GetState(PlayerIndex playerIndex) { return new GamePadState(); }
		
		public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode) { return new GamePadState(); }
		
		public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor) { return false;  }
	}
}
