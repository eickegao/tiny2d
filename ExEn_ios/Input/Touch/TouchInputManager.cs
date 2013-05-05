using System;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework.Input
{
	/// <summary>
	/// Internal class for handling touch input.
	/// All methods must be called with in lock(TouchInputManager.lockObject) { }.
	/// </summary>
	internal static class TouchInputManager
	{
		static internal object lockObject = new object();


		static int? firstTouchId = null;

		static int IndexOfTouch(int id)
		{
			for(int i = 0; i < TouchPanel.touches.Count; i++)
				if(TouchPanel.touches[i].Id == id)
					return i;
			return -1;
		}


		[System.Diagnostics.Conditional("DEBUG")]
		internal static void SanityCheckAllTouchesUp()
		{
#if DEBUG
			foreach(var touch in TouchPanel.touches)
			{
				System.Diagnostics.Debug.Assert(touch.State == TouchLocationState.Invalid
						|| touch.State == TouchLocationState.Released);
			}
#endif
		}


		internal static void BeginTouch(int id, Point position)
		{
			int index = IndexOfTouch(id);

			// Add new touch to collection
			if(index == -1 && TouchPanel.touches.Count < TouchCollection.MaxTouches)
			{
				TouchPanel.touches.Add(new TouchLocation(id, TouchLocationState.Pressed,
						new Vector2(position.X, position.Y)));
			}

			// Set mouse state
			if(!firstTouchId.HasValue)
			{
				firstTouchId = id;
				Mouse.currentState.X = position.X;
				Mouse.currentState.Y = position.Y;
				Mouse.currentState.LeftButton = ButtonState.Pressed;
			}
		}


		internal static void MoveTouch(int id, Point position)
		{
			int index = IndexOfTouch(id);

			// Check that the "Pressed" state has been registered before going to "Moved" state
			if(index >= 0 && TouchPanel.touches[index].State != TouchLocationState.Pressed)
			{
				TouchLocation original = TouchPanel.touches[index];
				TouchPanel.touches[index] = TouchPanel.AdvanceTouchLocation(original,
						TouchLocationState.Moved, new Vector2(position.X, position.Y));
			}

			if(firstTouchId.HasValue && firstTouchId.Value == id)
			{
				Mouse.currentState.X = position.X;
				Mouse.currentState.Y = position.Y;
			}
		}


		internal static void EndTouch(int id, Point position, bool cancel)
		{
			int index = IndexOfTouch(id);

			if(index >= 0)
			{
				TouchLocation original = TouchPanel.touches[index];
				TouchPanel.touches[index] = TouchPanel.AdvanceTouchLocation(original,
						cancel ? TouchLocationState.Invalid : TouchLocationState.Released,
						new Vector2(position.X, position.Y));
			}

			if(firstTouchId.HasValue && firstTouchId.Value == id)
			{
				if(!cancel) // do not change mouse state for cancelled
					Mouse.currentState.LeftButton = ButtonState.Released;
				firstTouchId = null;
			}
		}
	}




	// Framework Mouse class
	public static class Mouse
	{
		// Do not access except within lock(TouchInputManager.lockObject) { }
		static internal MouseState currentState;

		public static MouseState GetState()
		{
			lock(TouchInputManager.lockObject)
			{
				return currentState;
			}
		}
	}
}
