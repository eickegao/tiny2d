using System;

namespace Microsoft.Xna.Framework.Input.Touch
{
	public static class TouchPanel
	{
		#region Capabilities

		static TouchPanelCapabilities caps = new TouchPanelCapabilities
		{
			IsConnected = true,
			MaximumTouchCount = TouchCollection.MaxTouches
		};
		
		public static TouchPanelCapabilities GetCapabilities()
		{
			return caps;
		}

		#endregion


		#region Internal State Management

		internal static TouchLocation AdvanceTouchLocation(TouchLocation original,
				TouchLocationState newState, Vector2 newPosition)
		{
			return new TouchLocation(original.Id, newState, newPosition, original.State, original.Position);
		}

		internal static TouchLocation AdvanceTouchLocation(TouchLocation original,
				TouchLocationState newState)
		{
			return new TouchLocation(original.Id, newState, original.Position, original.State, original.Position);
		}


		// Do not access except within lock(TouchInputManager.lockObject) { }
		internal static TouchCollection touches = default(TouchCollection);

		#endregion


		public static TouchCollection GetState()
		{
			lock(TouchInputManager.lockObject)
			{
				TouchCollection retval = touches;

				// Advance touches to the next state in their life cycle (*after* the state is queried)
				int i = 0;
				while(i < touches.Count)
				{
					TouchLocation original = touches[i];
					if(original.State == TouchLocationState.Pressed)
						touches[i] = AdvanceTouchLocation(original, TouchLocationState.Moved);
					if(original.State == TouchLocationState.Released || original.State == TouchLocationState.Invalid)
					{
						touches.RemoveAt(i);
						continue;
					}

					i++;
				}

				return retval;
			}
		}


		// Weird that these have public setters...
		// The backing fields are atomic, so locking is unnecessary
		public static int DisplayWidth { get; set; }
		public static int DisplayHeight { get; set; }
		public static DisplayOrientation DisplayOrientation { get; set; }
	}
}

