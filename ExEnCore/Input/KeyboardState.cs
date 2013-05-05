using System;

namespace Microsoft.Xna.Framework.Input
{
	public enum KeyState
	{
		Up = 0,
		Down = 1
	}


	public struct KeyboardState
	{

		#region Key Data

		// Array of 256 bits:
		private uint keys0, keys1, keys2, keys3, keys4, keys5, keys6, keys7;

		public bool InternalGetKey(Keys key)
		{
			uint mask = (uint)1 << (((int)key) & 0x1f);

			uint element;
			switch(((int)key) >> 5)
			{
				case 0: element = keys0; break;
				case 1: element = keys1; break;
				case 2: element = keys2; break;
				case 3: element = keys3; break;
				case 4: element = keys4; break;
				case 5: element = keys5; break;
				case 6: element = keys6; break;
				case 7: element = keys7; break;
				default: element = 0; break;
			}
			
			return (element & mask) != 0;
		}

		public void InternalSetKey(Keys key)
		{
			uint mask = (uint)1 << (((int)key) & 0x1f);
			switch(((int)key) >> 5)
			{
				case 0: keys0 |= mask; break;
				case 1: keys1 |= mask; break;
				case 2: keys2 |= mask; break;
				case 3: keys3 |= mask; break;
				case 4: keys4 |= mask; break;
				case 5: keys5 |= mask; break;
				case 6: keys6 |= mask; break;
				case 7: keys7 |= mask; break;
			}
		}

		public void InternalClearKey(Keys key)
		{
			uint mask = (uint)1 << (((int)key) & 0x1f);
			switch(((int)key) >> 5)
			{
				case 0: keys0 &= ~mask; break;
				case 1: keys1 &= ~mask; break;
				case 2: keys2 &= ~mask; break;
				case 3: keys3 &= ~mask; break;
				case 4: keys4 &= ~mask; break;
				case 5: keys5 &= ~mask; break;
				case 6: keys6 &= ~mask; break;
				case 7: keys7 &= ~mask; break;
			}
		}

		public void InternalClearAllKeys()
		{
			keys0 = 0;
			keys1 = 0;
			keys2 = 0;
			keys3 = 0;
			keys4 = 0;
			keys5 = 0;
			keys6 = 0;
			keys7 = 0;
		}

		#endregion


		#region XNA Interface

		public KeyboardState(params Keys[] keys)
		{
			keys0 = 0;
			keys1 = 0;
			keys2 = 0;
			keys3 = 0;
			keys4 = 0;
			keys5 = 0;
			keys6 = 0;
			keys7 = 0;

			if(keys != null)
				foreach(Keys k in keys)
					InternalSetKey(k);
		}

		public KeyState this[Keys key]
		{
			get { return InternalGetKey(key) ? KeyState.Down : KeyState.Up; }
		}

		public bool IsKeyDown(Keys key)
		{
			return InternalGetKey(key);
		}

		public bool IsKeyUp(Keys key)
		{
			return !InternalGetKey(key);
		}

		#endregion


		#region GetPressedKeys()

		private static uint CountBits(uint v)
		{
			// http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
			v = v - ((v >> 1) & 0x55555555);                    // reuse input as temporary
			v = (v & 0x33333333) + ((v >> 2) & 0x33333333);     // temp
			return ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
		}

		private static int AddKeysToArray(uint keys, int offset, Keys[] pressedKeys, int index)
		{
			for(int i = 0; i < 32; i++)
			{
				if((keys & (1 << i)) != 0)
					pressedKeys[index++] = (Keys)(offset + i);
			}
			return index;
		}

		public Keys[] GetPressedKeys()
		{
			uint count = CountBits(keys0) + CountBits(keys1) + CountBits(keys2) + CountBits(keys3)
					+ CountBits(keys4) + CountBits(keys5) + CountBits(keys6) + CountBits(keys7);
			Keys[] keys = new Keys[count];

			int index = 0;
			if(keys0 != 0) index = AddKeysToArray(keys0, 0*32, keys, index);
			if(keys1 != 0) index = AddKeysToArray(keys1, 1*32, keys, index);
			if(keys2 != 0) index = AddKeysToArray(keys2, 2*32, keys, index);
			if(keys3 != 0) index = AddKeysToArray(keys3, 3*32, keys, index);
			if(keys4 != 0) index = AddKeysToArray(keys4, 4*32, keys, index);
			if(keys5 != 0) index = AddKeysToArray(keys5, 5*32, keys, index);
			if(keys6 != 0) index = AddKeysToArray(keys6, 6*32, keys, index);
			if(keys7 != 0) index = AddKeysToArray(keys7, 7*32, keys, index);

			return keys;
		}

		#endregion


		#region Objet and Equality

		public override int GetHashCode()
		{
			return (int)(keys0 ^ keys1 ^ keys2 ^ keys3 ^ keys4 ^ keys5 ^ keys6 ^ keys7);
		}

		public static bool operator ==(KeyboardState a, KeyboardState b)
		{
			return a.keys0 == b.keys0
			    && a.keys1 == b.keys1
			    && a.keys2 == b.keys2
			    && a.keys3 == b.keys3
			    && a.keys4 == b.keys4
			    && a.keys5 == b.keys5
			    && a.keys6 == b.keys6
			    && a.keys7 == b.keys7;
		}

		public static bool operator !=(KeyboardState a, KeyboardState b)
		{
			return !(a == b);
		}

		public override bool Equals(object obj)
		{
			return obj is KeyboardState && this == (KeyboardState)obj;
		}

		#endregion

	}
}
