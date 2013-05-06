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
	/// <summary>
	/// A base class for anything with a name in the Tiny2d framework
	/// </summary>
	public class BaseObject
	{
		#region Fields
		private string _name;
		internal int id;
		#endregion
		
		#region Properties
		/// <summary>
		/// Gets or Sets the name used to indentify the item
		/// </summary>
		#if WINDOWS 
		[CategoryAttribute("Design"), DescriptionAttribute("Indicates the name used to indentify the item"),Browsable(true) ]
		#endif
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		#endregion

	}
}