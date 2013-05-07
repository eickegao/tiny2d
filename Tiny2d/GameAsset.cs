using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Tiny2d
{
	public enum AssetScope
	{
		Embedded,
		Global,
		Local,
	}

	public abstract class GameAsset
	{
		/// <summary>
		/// Gets or Sets the Name of the Asset
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Gets or Sets the Scope of the Asset
		/// </summary>
		public AssetScope Scope { get; set; }  
		/// <summary>
		/// Gets or Sets the Filename of the Asset
		/// </summary>
		public string Filename { get; set; }
		/// <summary>
		/// Gets or Sets the Parent SceneBase of the Asset
		/// </summary>
		[XmlIgnore]
		public SceneBase Parent { get; set; }
	}
}
