using System;

namespace Microsoft.Xna.Framework
{
	public class GameComponent : IGameComponent, IUpdateable, IDisposable
	{

		#region Constructor

		public Game Game { get; private set; }

		public GameComponent(Game game)
		{
			this.Game = game;
		}

		#endregion


		#region Disposal

		~GameComponent() { this.Dispose(false); }
		public void Dispose() { Dispose(true); }

		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(Disposed != null)
					Disposed(this, EventArgs.Empty);
			}
		}

		public event EventHandler Disposed;

		#endregion


		#region Enabled

		private bool _enabled = true;
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if(_enabled != value)
				{
					_enabled = value;
					OnEnabledChanged(this, EventArgs.Empty);
				}
			}
		}

		protected virtual void OnEnabledChanged(object sender, EventArgs args)
		{
			if(EnabledChanged != null)
				EnabledChanged(sender, args);
		}

		public event EventHandler EnabledChanged;

		#endregion


		#region Update Order

		private int _updateOrder = 0;
		public int UpdateOrder
		{
			get { return _updateOrder; }
			set
			{
				if(_updateOrder != value)
				{
					_updateOrder = value;
					OnUpdateOrderChanged(this, EventArgs.Empty);
				}
			}
		}

		protected virtual void OnUpdateOrderChanged(object sender, EventArgs args)
		{
			if(UpdateOrderChanged != null)
				UpdateOrderChanged(sender, args);
		}

		public event EventHandler UpdateOrderChanged;

		#endregion


		#region Public Virtual Functions

		public virtual void Initialize() { }

		public virtual void Update(GameTime gameTime) { }

		#endregion

	}
}
