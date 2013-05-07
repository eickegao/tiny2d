using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using IceCream.Attributes;
	
namespace Tiny2d.Components
{
    [Serializable]
	public abstract class GameObjComponent : IDeepCopy
    {
        #region Fields

        private SceneItem _owner;
        private bool _enabled;

        #endregion

        #region Properties

        [XmlIgnore()]
        public SceneItem Owner
        {
            get { return _owner; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        #endregion

        #region Constructor

		public GameObjComponent()
        {

        }

        #endregion
     
        #region Methods

        public virtual void CopyValuesTo(object target)
        {
			GameObjComponent component = target as GameObjComponent;
            component.Enabled = this.Enabled;
        }

        public virtual object GetCopy()
        {
			return ComponentTypeContainer.DeepCopyGameObjComponent(this.GetType(), this);
        }        

        public abstract void OnRegister();

        public abstract void Update(float elapsedTime);

        internal void SetOwner(SceneItem owner)
        {       
            _owner = owner;
        }

        public virtual void OnUnRegister()
        {

        }

        #endregion    
    }
}
