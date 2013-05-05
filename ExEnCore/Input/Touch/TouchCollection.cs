using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Input.Touch
{
	public struct TouchCollection : IList<TouchLocation>
	{
		bool isConnected;
		public bool IsConnected { get { return isConnected; } }

		public const int MaxTouches = 5;
		TouchLocation touch0, touch1, touch2, touch3, touch4;

		public TouchLocation this[int index]
		{
			get
			{
				Debug.Assert(count >= 0 && count <= MaxTouches);
				if(index >= count)
					throw new IndexOutOfRangeException();

				switch(index)
				{
					case 0: return touch0;
					case 1: return touch1;
					case 2: return touch2;
					case 3: return touch3;
					case 4: return touch4;

					default:
						throw new IndexOutOfRangeException();
				}
			}
			set
			{
				Debug.Assert(count >= 0 && count <= MaxTouches);
				if(index >= count)
					throw new IndexOutOfRangeException();

				switch(index)
				{
					case 0: touch0 = value; break;
					case 1: touch1 = value; break;
					case 2: touch2 = value; break;
					case 3: touch3 = value; break;
					case 4: touch4 = value; break;

					default:
						throw new IndexOutOfRangeException();
				}
			}
		}

		int count;
		public int Count { get { return count; } }

		public bool IsReadOnly { get { return false; } }


		public bool FindById(int id, out TouchLocation touchLocation)
		{
			for(int i = 0; i < count; i++)
			{
				if(this[i].Id == id)
				{
					touchLocation = this[i];
					return true;
				}
			}
			touchLocation = default(TouchLocation);
			return false;
		}



		#region Enumerator

		public struct Enumerator : IEnumerator<TouchLocation>
		{
			internal Enumerator(TouchCollection collection)
			{
				this.collection = collection;
				this.i = -1;
			}

			TouchCollection collection;
			int i;

			object System.Collections.IEnumerator.Current { get { return Current; } }
			public TouchLocation Current
			{
				get
				{
					return collection[i];
				}
			}

			public bool MoveNext()
			{
				return (++i < collection.Count);
			}

			public void Reset()
			{
				i = -1;
			}

			public void Dispose() { }
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator<TouchLocation> IEnumerable<TouchLocation>.GetEnumerator()
		{
			return GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion


		#region IList<TouchLocation> interface members

		public int IndexOf(TouchLocation item)
		{
			for(int i = 0; i < count; i++)
			{
				if(item == this[i])
					return i;
			}
			return -1;
		}

		public bool Contains(TouchLocation item)
		{
			return IndexOf(item) != -1;
		}

		public void Add(TouchLocation item)
		{
			if(count >= MaxTouches)
				throw new InvalidOperationException("TouchCollection is out of space");

			count++;
			this[count-1] = item;
		}

		public void Clear()
		{
			count = 0;
		}

		public void CopyTo(TouchLocation[] array, int arrayIndex)
		{
			if(array == null)
				throw new ArgumentNullException("array");
			if(arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex");
			if(count + arrayIndex > array.Length)
				throw new ArgumentOutOfRangeException("array"); // not enough space in array

			for(int i = 0; i < count; i++)
				array[arrayIndex + i] = this[i];
		}

		public void RemoveAt(int index)
		{
			this[index] = this[count-1];
			--count;
		}

		#endregion


		#region Unsupported IList<TouchLocation> interface members

		public void Insert(int index, TouchLocation item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(TouchLocation item)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
