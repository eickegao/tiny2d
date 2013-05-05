using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ExEnCore;

namespace Microsoft.Xna.Framework
{
	// Common stuff for Game
	public partial class Game : IDisposable
	{

		#region Disposal

		public void Dispose()
		{
			Dispose(true);
		}

		public event EventHandler Disposed;
		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				var componentsToDispose = Components.OfType<IDisposable>().ToArray();
				foreach(var component in componentsToDispose)
					component.Dispose();

				var servicesToDispose = Services.GetAllServices().OfType<IDisposable>().ToArray();
				foreach(var service in servicesToDispose)
					service.Dispose();

				UnloadContent();

				if(Disposed != null)
					Disposed(this, EventArgs.Empty);
			}
		}

		#endregion


		#region Components and Services

		GameServiceContainer services = new GameServiceContainer();
		public GameServiceContainer Services { get { return services; } }

		GameComponentCollection components = new GameComponentCollection();
		public GameComponentCollection Components { get { return components; } }

		#endregion


		#region Graphics Device Manager (ExEn uses a direct hookup)

		internal GraphicsDeviceManager graphicsDeviceManager = null;

		public GraphicsDevice GraphicsDevice
		{
			get
			{
				if(graphicsDeviceManager == null)
					throw new InvalidOperationException("Cannot access the GraphicsDevice without a graphics device service");

				return graphicsDeviceManager.GraphicsDevice;
			}
		}

		#endregion


		#region Activation

		bool _isActive = true;
		public bool IsActive
		{
			get { return _isActive; }
			internal set
			{
				if(_isActive != value)
				{
					_isActive = value;
					if(_isActive)
						OnActivated(this, EventArgs.Empty);
					else
						OnDeactivated(this, EventArgs.Empty);
				}
			}
		}

		public event EventHandler Activated;
		public event EventHandler Deactivated;

		protected virtual void OnDeactivated(object sender, EventArgs args)
		{
			if(Deactivated != null)
				Deactivated(sender, args);
		}

		protected virtual void OnActivated(object sender, EventArgs args)
		{
			if(Activated != null)
				Activated(sender, args);
		}

		#endregion


		#region Exiting

		internal void DoTermination()
		{
			OnExiting(this, EventArgs.Empty);
			EndRun();
			Dispose();
		}

		public event EventHandler Exiting;
		protected virtual void OnExiting(object sender, EventArgs args)
		{
			if(Exiting != null)
				Exiting(sender, args);
		}

		public void Exit()
		{
			// Does Nothing on iOS or Silverlight
			// (You're not supposed to terminate manually on iOS, and you can't on Silverlight)
		}

		#endregion


		#region Content

		public ContentManager Content { get; private set; }

		protected virtual void LoadContent() { }
		protected virtual void UnloadContent() { }

		#endregion


		#region Initialization

		protected virtual void Initialize()
		{
			components.Initialize();

			if(GraphicsDevice != null)
				LoadContent();
		}

		#endregion


		#region Running

		protected internal virtual void BeginRun() { }
		protected internal virtual void EndRun() { }

		protected internal virtual void Update(GameTime gameTime)
		{
			components.Update(gameTime);
		}

		#endregion


		#region Drawing

		protected virtual bool BeginDraw()
		{
			if(graphicsDeviceManager != null)
				return graphicsDeviceManager.BeginDraw();
			else
				return true;
		}

		protected virtual void Draw(GameTime gameTime)
		{
			components.Draw(gameTime);
		}

		protected virtual void EndDraw()
		{
			if(graphicsDeviceManager != null)
				graphicsDeviceManager.EndDraw();
		}


		bool supressDrawCurrentFrame;
		public void SuppressDraw() { supressDrawCurrentFrame = true; }

		internal void DoDraw(GameTime gameTime)
		{
			if(supressDrawCurrentFrame)
			{
				supressDrawCurrentFrame = false;
				return;
			}

			if(BeginDraw())
			{
				Draw(gameTime);
				EndDraw();
			}
		}

		#endregion


		#region Timing

		internal GameLoop gameLoop = new GameLoop();

		// For the internal use of ExEn:
		internal event Action TimingValuesChanged;

		public bool IsFixedTimeStep
		{
			get { return gameLoop.IsFixedTimeStep; }
			set
			{
				gameLoop.IsFixedTimeStep = value;
				if(TimingValuesChanged != null)
					TimingValuesChanged();
			}
		}

		public TimeSpan TargetElapsedTime
		{
			get { return gameLoop.TargetElapsedTime; }
			set
			{
				gameLoop.TargetElapsedTime = value;
				if(TimingValuesChanged != null)
					TimingValuesChanged();
			}
		}

		void ResetElapsedTime() { gameLoop.Reset(); }

		#endregion


		#region Miscelaneous

		// This does nothing on mobile platforms
		public bool IsMouseVisible { get; set; }

		public GameWindow Window { get; private set; }

		#endregion

	}
}
