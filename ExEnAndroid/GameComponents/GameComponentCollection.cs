using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
	public sealed class GameComponentCollection : Collection<IGameComponent>
	{
		#region Collection Functions

		protected override void InsertItem(int index, IGameComponent item)
		{
			HandleInsert(item);
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			HandleRemove(this[index]);
			base.RemoveItem(index);
		}

		protected override void ClearItems()
		{
			foreach(var item in this)
				HandleRemove(item);
			base.ClearItems();
		}

		protected override void SetItem(int index, IGameComponent item)
		{
			throw new NotSupportedException();
		}

		#endregion


		#region Insert Remove Handling

		void HandleInsert(IGameComponent item)
		{
			IDrawable drawable = (item as IDrawable);
			if(drawable != null)
			{
				AddDrawable(drawable);
			}

			IUpdateable updateable = (item as IUpdateable);
			if(updateable != null)
			{
				AddUpdateable(updateable);
			}

			if(initializeCalled)
				item.Initialize();
			else
			{
				if(pendingInitialize == null)
					pendingInitialize = new List<IGameComponent>();
				pendingInitialize.Add(item);
			}
		}

		void HandleRemove(IGameComponent item)
		{
			IDrawable drawable = (item as IDrawable);
			if(drawable != null)
			{
				RemoveDrawable(drawable);
			}

			IUpdateable updateable = (item as IUpdateable);
			if(updateable != null)
			{
				RemoveUpdateable(updateable);
			}

			if(pendingInitialize != null)
				pendingInitialize.Remove(item);
		}

		#endregion


		#region Initialize

		bool initializeCalled = false;
		List<IGameComponent> pendingInitialize;

		internal void Initialize()
		{
			initializeCalled = true;

			if(pendingInitialize != null)
			{
				foreach(var item in pendingInitialize)
					item.Initialize();
				pendingInitialize = null;
			}
		}

		#endregion


		#region Update

		bool updateListDirty = false;
		List<IUpdateable> updateingList = new List<IUpdateable>();
		List<IUpdateable> updateingListInUse = new List<IUpdateable>();

		internal void Update(GameTime gameTime)
		{
			// Take a copy of the list to use while updating, in case the list is modified while updating
			if(updateListDirty)
			{
				updateingListInUse.Clear();
				updateingListInUse.AddRange(updateingList.OrderBy(o => o.UpdateOrder));
				updateListDirty = false;
			}

			foreach(var item in updateingListInUse)
			{
				if(item.Enabled)
					item.Update(gameTime);
			}
		}

		void ComponentUpdateOrderChanged(object sender, EventArgs e)
		{
			updateListDirty = true;
		}

		private void AddUpdateable(IUpdateable item)
		{
			updateingList.Add(item);
			item.UpdateOrderChanged += new EventHandler(ComponentUpdateOrderChanged);
			updateListDirty = true;
		}

		private void RemoveUpdateable(IUpdateable item)
		{
			updateingList.Remove(item);
			item.UpdateOrderChanged -= new EventHandler(ComponentUpdateOrderChanged);
			updateListDirty = true;
		}

		#endregion


		#region Draw

		bool drawListDirty = false;
		List<IDrawable> drawingList = new List<IDrawable>();
		List<IDrawable> drawingListInUse = new List<IDrawable>();

		internal void Draw(GameTime gameTime)
		{
			// Take a copy of the list to use while drawing, in case the list is modified while drawing
			if(drawListDirty)
			{
				drawingListInUse.Clear();
				drawingListInUse.AddRange(drawingList.OrderBy(o => o.DrawOrder));
				drawListDirty = false;
			}

			foreach(var item in drawingListInUse)
			{
				if(item.Visible)
					item.Draw(gameTime);
			}
		}

		void ComponentDrawOrderChanged(object sender, EventArgs e)
		{
			drawListDirty = true;
		}

		private void AddDrawable(IDrawable item)
		{
			drawingList.Add(item);
			item.DrawOrderChanged += new EventHandler(ComponentDrawOrderChanged);
			drawListDirty = true;
		}

		private void RemoveDrawable(IDrawable item)
		{
			drawingList.Remove(item);
			item.DrawOrderChanged -= new EventHandler(ComponentDrawOrderChanged);
			drawListDirty = true;
		}

		#endregion

	}
}
