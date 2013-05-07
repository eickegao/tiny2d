using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System;
using System.Collections.Generic;
using System.Text;

namespace Tiny2d.Components
{
    [Serializable]
    public abstract class SceneComponent : IDeepCopy
    {
        #region Fields
        private GameScene _owner;
        private bool _enabled;

        #endregion

        #region Properties

        public GameScene Owner
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

		public SceneComponent()
        {

        }

        #endregion

        #region Methods

        public virtual void CopyValuesTo(object target)
        {
			SceneComponent component = target as SceneComponent;
            component.Enabled = this.Enabled;
        }

        public abstract void OnRegister();

        public abstract void Update(float elapsedTime);

        internal void SetOwner(GameScene owner)
        {
            _owner = owner;
        }

        public virtual void OnUnRegister()
        {

        }
		

        #endregion
    }
}
