using System;
using Microsoft.Xna.Framework;


namespace Microsoft.Xna.Framework.Input
{
	public struct GamePadState
	{
		//public GamePadState(GamePadThumbSticks thumbSticks, GamePadTriggers triggers, GamePadButtons buttons, GamePadDPad dPad) { }
		//public GamePadState(Vector2 leftThumbStick, Vector2 rightThumbStick, float leftTrigger, float rightTrigger, params Buttons[] buttons) { }

		public GamePadButtons Buttons { get { return new GamePadButtons(); } }
		public GamePadDPad DPad { get { return new GamePadDPad(); } }
		public bool IsConnected { get {return false; } }
		public int PacketNumber { get { return 0; } }
		public GamePadThumbSticks ThumbSticks { get { return new GamePadThumbSticks(); } }
		public GamePadTriggers Triggers { get { return new GamePadTriggers(); } }

		public bool IsButtonDown(Buttons button) { return false; }
		public bool IsButtonUp(Buttons button) { return true; }


		public static bool operator==(GamePadState left, GamePadState right) { return true; }
		public static bool operator!=(GamePadState left, GamePadState right) { return false; }
		public override bool Equals(object obj) { return true; }
		public override int GetHashCode() { return 0; }
		public override string ToString() { return "GamePadState"; }
	}

	public struct GamePadThumbSticks
	{
		//public GamePadThumbSticks(Vector2 leftThumbstick, Vector2 rightThumbstick);

		public Vector2 Left { get { return Vector2.Zero; } }
		public Vector2 Right { get { return Vector2.Zero; } }


		public static bool operator==(GamePadThumbSticks left, GamePadThumbSticks right) { return true; }
		public static bool operator!=(GamePadThumbSticks left, GamePadThumbSticks right) { return false; }
		public override bool Equals(object obj) { return true; }
		public override int GetHashCode() { return 0; }
		public override string ToString() { return "GamePadThumbSticks"; }
	}

	public struct GamePadTriggers
	{
		//public GamePadTriggers(float leftTrigger, float rightTrigger);

		public float Left { get { return 0f; } }
		public float Right { get { return 0f; } }


		public static bool operator==(GamePadTriggers left, GamePadTriggers right) { return true; }
		public static bool operator!=(GamePadTriggers left, GamePadTriggers right) { return false; }
		public override bool Equals(object obj) { return true; }
		public override int GetHashCode() { return 0; }
		public override string ToString() { return "GamePadTriggers"; }
	}

	public struct GamePadButtons
	{
		//public GamePadButtons(Buttons buttons);

		public ButtonState A { get { return ButtonState.Released; } }
		public ButtonState B { get { return ButtonState.Released; } }
		public ButtonState Back { get { return ButtonState.Released; } }
		public ButtonState BigButton { get { return ButtonState.Released; } }
		public ButtonState LeftShoulder { get { return ButtonState.Released; } }
		public ButtonState LeftStick { get { return ButtonState.Released; } }
		public ButtonState RightShoulder { get { return ButtonState.Released; } }
		public ButtonState RightStick { get { return ButtonState.Released; } }
		public ButtonState Start { get { return ButtonState.Released; } }
		public ButtonState X { get { return ButtonState.Released; } }
		public ButtonState Y { get { return ButtonState.Released; } }


		public static bool operator==(GamePadButtons left, GamePadButtons right) { return true; }
		public static bool operator!=(GamePadButtons left, GamePadButtons right) { return false; }
		public override bool Equals(object obj) { return true; }
		public override int GetHashCode() { return 0; }
		public override string ToString() { return "GamePadButtons"; }
	}

	public struct GamePadDPad
	{
		//public GamePadDPad(ButtonState upValue, ButtonState downValue, ButtonState leftValue, ButtonState rightValue);

		public ButtonState Down { get { return ButtonState.Released; } }
		public ButtonState Left { get { return ButtonState.Released; } }
		public ButtonState Right { get { return ButtonState.Released; } }
		public ButtonState Up { get { return ButtonState.Released; } }


		public static bool operator==(GamePadDPad left, GamePadDPad right) { return true; }
		public static bool operator!=(GamePadDPad left, GamePadDPad right) { return false; }
		public override bool Equals(object obj) { return true; }
		public override int GetHashCode() { return 0; }
		public override string ToString() { return "GamePadDPad"; }
	}
}
