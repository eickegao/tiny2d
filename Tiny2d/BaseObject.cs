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
	public class BaseObject
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

		private List<BaseObject> _children;
		private bool _isVisible;
		private BaseObject _parent;
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

		public BaseObject Parent
		{
			get { return _parent;}
			set { _parent = value;}
		}

		public object userData
		{
			get { return _userData;}
			set { _userData = value;}
		}


		public List<BaseObject> Children {
			get { return _children; }
		}
		#endregion

		public BaseObject(){
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

			_children = new List<BaseObject>();
			_parent = null;
			_userData = null;
		}

		public void CleanUp() 
		{
			foreach (BaseObject child in _children) {
				child.CleanUp();
			}
		}

		public virtual BaseObject AddChild(BaseObject child) 
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

		private void InsertChild(BaseObject child) 
		{
			int i = 0;
			bool added = false;

			int z = child.ZOrder;

			foreach (BaseObject node in _children)
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
		
		
		public virtual void RemoveChild(BaseObject child, bool cleanup) 
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

			_children.ForEach(delegate(BaseObject child) 
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
		
		private void DetachChild(BaseObject child, bool cleanup) {
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
		
		public BaseObject GetChildByTag(int tag) 
		{
			BaseObject result = null;
			foreach (BaseObject node in _children)
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
			
			foreach (BaseObject child in _children) 
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
			
			foreach (BaseObject child in _children) 
			{
				if (child.ZOrder >= 0) 
				{
					child.Visit();
				}
			}
		}
		
		
		
		public virtual void OnEnter() 
		{
			foreach (BaseObject child in _children) 
			{
				child.OnEnter();
			}
			_isRunning = true;
		}

		public void OnEnterTransitionDidFinish() 
		{
			foreach (BaseObject child in _children) 
			{
				child.OnEnterTransitionDidFinish();
			}
		}
		
		public virtual void OnExit() 
		{
			_isEnable = false;
			foreach (BaseObject child in _children) 
			{
				child.OnExit();
			}
		}
	}

}