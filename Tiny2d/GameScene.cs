using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace Tiny2d
{
	public class GameScene : SceneBase
	{
		internal bool isInGame;     
		internal bool _hasBeenUpdatedOnce;
		
		#region Fields
		
		int _nextId = 0;
		private Color _clearColor = Color.CornflowerBlue;
		private bool _willRenderNotActive = true;
		
		#endregion
		
		#region Properties
		

		public bool WillRenderNotActive 
		{ 
			get { return _willRenderNotActive; } 
			set { _willRenderNotActive = value; } 
		}

		public bool Enabled 
		{ 
			get; 
			set; 
		}

		public Color ClearColor
		{
			get { return _clearColor; }
			set { _clearColor = value; }
		}
		
		#endregion
		
		#region Constructor
		

		public GameScene()
		{
			
		}
		
		#endregion
		
		#region Methods

		

		
		#endregion        
	}    
}
