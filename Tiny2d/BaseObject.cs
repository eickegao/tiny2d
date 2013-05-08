using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tiny2d
{
	public class BaseObject
	{
		#region Fields
		private string _name;
		private int _id;
		#endregion
		
		#region Properties

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public int ID
		{
			get { return _id;}
			set { _id = value;}
		}

		#endregion

	}
}