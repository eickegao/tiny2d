using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace Tiny2d
{
	public class SceneBase:SceneNode
	{
		#region Fields

		internal ContentManager _content;
		
		#endregion
		
		#region Properties

		public ContentManager ContentManager
		{
			get { return _content; }
		}
		
		#endregion
		
		#region Methods
		
		public void InitializeContent(IServiceProvider services)
		{
			_content = new ContentManager(services);
		}
		
		public void InitializeContent(IServiceProvider services, string rootpath)
		{
			_content = new ContentManager(services, rootpath);
		}

		#endregion
	}
}

