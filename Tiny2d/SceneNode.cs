using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


#define TAG_INVALID     -1

namespace Tiny2d
{
	public class SceneNode
	{
		#region Fields
		private string _name;
		private int _id;
		private int _tag;
		private int _zOrder;

		private bool _isTransformDirty;
		private float _rotation;
		private float _scaleX;
		private float _scaleY;
		private Point _position;
		private bool _isRunning;
		private bool _isEnable;
		private Point _anchorPoint;
		private int _width;
		private int _height;

		private List<SceneNode> _children;
		private bool _isVisible;
		private SceneNode _parent;
		private object _userData;

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

		public int Tag
		{
			get { return _tag;}
			set { _tag = value;}
		}

		public int ZOrder
		{
			get { return _zOrder;}
		}
			
		public float Rotation 
		{
			get { return _rotation; }
			set { if (_rotation != value) 
					{
						_rotation = value;
						_isTransformDirty = true;
					}
				}
		}
		
		public float scaleX 
		{
			get { return _scaleX; }
			set {
					_scaleX = value;
					_isTransformDirty = true;
				}
		}
		
		public float scaleY 
		{
			get { return _scaleY; }
			set {
					_scaleY = value;
					_isTransformDirty = true;
				}
		}

		public  Point Position
		{
			get { return _position;}
			set { _position = value;}
		}

		public bool isRunning
		{
			get { return _isRunning;}
			set { _isRunning = value;}
		}

		public bool isEnable
		{
			get { return _isEnable;}
			set { _isEnable = value;}
		}

		public  Point anchorPoint
		{
			get { return _anchorPoint;}
			set { _anchorPoint = value;}
		}

		public int Width
		{
			get { return _width;}
			set { _width = value;}
		}

		public int Height
		{
			get { return _height;}
			set { _height = value;}
		}

		public bool isVisible
		{
			get { return _isVisible;}
			set { _isVisible = value;}
		}

		public SceneNode Parent
		{
			get { return _parent;}
			set { _parent = value;}
		}

		public object userData
		{
			get { return _userData;}
			set { _userData = value;}
		}


		public List<SceneNode> Children {
			get { return _children; }
		}
		#endregion

		public SceneNode(){
			_name = "";
			_id = 0;
			_tag = TAG_INVALID;
			_zOrder = 0;
			
			_isTransformDirty = false;
			_rotation = 0;
			_scaleX = 1;
			_scaleY = 1;
			_position = Point.Zero;
			_isRunning = false;
			_isEnable = true;
			_anchorPoint = Point.Zero;
			_width = 0;
			_height = 0;
			_isVisible = true;

			_children = new List<SceneNode>();
			_parent = null;
			_userData = null;
		}

		public void CleanUp() 
		{
			foreach (SceneNode child in _children) {
				child.CleanUp();
			}
		}

		public virtual SceneNode AddChild(SceneNode child) 
		{
			if (child == null) 
			{
				throw new ArgumentNullException("child");
			}
			if (child.Parent != null) 
			{
				throw new ArgumentException("Child already has a parent", "child");
			}

			InsertChild(child);

			child.Parent = this;
			
			if (_isRunning) 
			{
				child.OnEnter();
			}
			
			return this;
		}

		private void InsertChild(SceneNode child) 
		{
			int i = 0;
			bool added = false;

			int z = child.ZOrder;

			foreach (SceneNode node in _children)
			{
				if (node.ZOrder > z) 
				{
					added = true;
					_children.Insert(i, child);
					break;
				}
				++i;
			}
			
			if (!added) 
			{
				_children.Add(child);
			}
			
			child._zOrder = z;
		}
		
		
		public virtual void RemoveChild(SceneNode child, bool cleanup) 
		{
			if (child != null) 
			{
				if (Children.Contains(child)) 
				{
					DetachChild(child, cleanup);
				}
			}
		}
		
		public void RemoveChildByTag(int tag, bool cleanup) 
		{
			RemoveChild(GetChildByTag(tag), cleanup);
		}
		
		public void RemoveAllChildren(bool cleanup) 
		{

			_children.ForEach(delegate(SceneNode child) 
			               {
				if(_isRunning) 
				{
					child.OnExit();
				}
				
				if (cleanup) {
					child.CleanUp();
				}
				
				child.Parent = null;
			});
			
			_children.Clear();
		}
		
		private void DetachChild(SceneNode child, bool cleanup) {
			if (_isRunning) 
			{
				child.OnExit();
			}
			
			if (cleanup) 
			{
				child.CleanUp();
			}
			
			child.Parent = null;
			
			_children.Remove(child);
		}
		
		public SceneNode GetChildByTag(int tag) 
		{
			SceneNode result = null;
			foreach (SceneNode node in _children)
			{
				if(node.Tag == tag)
				{
					result = node;
					break;
				}
			}
			return result;
		}
		
		public virtual void Draw()
		{

		}
		
		public virtual void Visit() 
		{
			if (!_isVisible) 
			{
				return;
			}
			
			foreach (SceneNode child in _children) 
			{
				if (child.ZOrder < 0)
				{
					child.Visit();
				} 
				else 
				{
					break;
				}
			}
			
			Draw();
			
			foreach (SceneNode child in _children) 
			{
				if (child.ZOrder >= 0) 
				{
					child.Visit();
				}
			}
		}
		
		
		
		public virtual void OnEnter() 
		{
			foreach (SceneNode child in _children) 
			{
				child.OnEnter();
			}
			_isRunning = true;
		}

		public void OnEnterTransitionDidFinish() 
		{
			foreach (SceneNode child in _children) 
			{
				child.OnEnterTransitionDidFinish();
			}
		}
		
		public virtual void OnExit() 
		{
			_isEnable = false;
			foreach (SceneNode child in _children) 
			{
				child.OnExit();
			}
		}
	}

}