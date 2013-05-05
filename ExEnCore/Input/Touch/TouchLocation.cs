using System;

namespace Microsoft.Xna.Framework.Input.Touch
{
	public enum TouchLocationState
	{
		Invalid = 0, // default
		Released,
		Pressed,
		Moved
	}


	public struct TouchLocation : IEquatable<TouchLocation>
	{

		#region Data

		int id;
		TouchLocationState state;
		Vector2 position;
		
		public int Id { get { return id; } }
		public TouchLocationState State { get { return state; } }
		public Vector2 Position { get { return position; } }
		
		TouchLocationState previousState;
		Vector2 previousPosition;

		public bool TryGetPreviousLocation(out TouchLocation previousLocation)
		{
			previousLocation.id = id;
			previousLocation.state = previousState;
			previousLocation.position = previousPosition;
			previousLocation.previousState = TouchLocationState.Invalid;
			previousLocation.previousPosition = Vector2.Zero;
			return previousState != TouchLocationState.Invalid;
		}

		#endregion


		#region Construction

		public TouchLocation(int id, TouchLocationState state, Vector2 position)
		{
			this.id = id;
			this.state = state;
			this.position = position;
			this.previousState = TouchLocationState.Invalid;
			this.previousPosition = Vector2.Zero;
		}

		public TouchLocation(int id, TouchLocationState state, Vector2 position,
				TouchLocationState previousState, Vector2 previousPosition)
		{
			this.id = id;
			this.state = state;
			this.position = position;
			this.previousState = previousState;
			this.previousPosition = previousPosition;
		}

		#endregion


		#region Equality Test

		public override bool Equals(object other)
		{
			return other is TouchLocation && this.Equals((TouchLocation)other);
		}

		public bool Equals(TouchLocation other)
		{
			return this.id == other.id && this.position == other.position && this.state == other.state
					&& this.previousPosition == other.previousPosition
					&& this.previousState == other.previousState;
		}

		public static bool operator==(TouchLocation value1, TouchLocation value2)
		{
			return value1.Equals(value2);
		}

		public static bool operator!=(TouchLocation value1, TouchLocation value2)
		{
			return !value1.Equals(value2);
		}

		public override int GetHashCode()
		{
			return id.GetHashCode() + position.GetHashCode() + state.GetHashCode()
					+ previousPosition.GetHashCode() + previousState.GetHashCode();
		}

		#endregion


		public override string ToString()
		{
			return "{Position:" + position.ToString() + "}";
		}

	}
}
