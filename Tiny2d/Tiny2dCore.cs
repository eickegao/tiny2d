using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


using System;
using System.Collections.Generic;
using System.Text;
using IceCream.SceneItems;

namespace Tiny2d
{
	
	public static class Tiny2dCore
	{
		#region Fields
		
		public static GameTime GameTime;   
		

		internal static bool activeSceneChanged = false;
		internal static GraphicsDevice graphicsDevice;
		private static List<SceneItem> _itemsToDelete = new List<SceneItem>();
		internal static SpriteBatch _afterBatch=null;
		#endregion
		
		#region Methods
		public static void DrawIceCreamScene(GraphicsDevice device, float elapsed, IceScene scene)
		{
			IceProfiler.StartProfiling(IceProfilerNames.ICE_CORE_DRAW);            
			foreach (SceneItem _item in scene.SceneItems)
			{
				if (_item.IsTemplate == true)
				{
					continue;
				}
				_item.Draw(elapsed);
			}
			IceProfiler.StopProfiling(IceProfilerNames.ICE_CORE_DRAW);
		}

		public static void RenderIceCream()
		{
			IceProfiler.StartProfiling(IceProfilerNames.ICE_CORE_RENDER);           
			DrawingManager.RenderScene();                            
			IceProfiler.StopProfiling(IceProfilerNames.ICE_CORE_RENDER);
		}

		internal static void UpdateIceCream(float elapsed)
		{
			if (SceneManager.ActiveScene == null || !SceneManager.ActiveScene.Enabled)
			{
				return;
			}
			
			IceProfiler.StartProfiling(IceProfilerNames.ICE_CORE_MAIN_UPDATE);

			if (SceneManager.ActiveScene.isInGame == false)
			{
				SceneManager.ActiveScene.isInGame = true;
			}
			if (SceneManager.ActiveScene._hasBeenUpdatedOnce == false)
			{
				SceneManager.ActiveScene._hasBeenUpdatedOnce = true;
			}            
			SceneManager.ActiveScene.RemoveItems();
			SceneManager.ActiveScene.RegisterItems();

			Input.InputCore.Update(elapsed);

			for (int i = 0; i < SceneManager.ActiveScene.SceneComponents.Count; i++)
			{
				IceSceneComponent comp = SceneManager.ActiveScene.SceneComponents[i];
				if (comp.Enabled == false)
				{
					continue;
				}
				comp.Update(elapsed);
				if (activeSceneChanged == true)
				{
					IceProfiler.StopProfiling(IceProfilerNames.ICE_CORE_MAIN_UPDATE);
					return;
				}
			}

			for (int i = 0; i < SceneManager.ActiveScene.SceneItems.Count; i++)
			{
				SceneItem _item = SceneManager.ActiveScene.SceneItems[i];
				if (_item.IsTemplate == true)
				{
					continue;
				}
				_item.Update(elapsed);
			}
			
			for (int i = 0; i < SceneManager.ActiveScene.SceneItems.Count; i++)
			{
				SceneItem _item = SceneManager.ActiveScene.SceneItems[i];
				if (_item.IsTemplate == true)
				{
					continue;
				}
				UpdateItemsComponents(_item);
				if (_item.MarkForDelete == true)
				{
					_itemsToDelete.Add(_item);
				}
				if (activeSceneChanged == true)
				{
					IceProfiler.StopProfiling(IceProfilerNames.ICE_CORE_MAIN_UPDATE);
					return;
				}
			}
			
			for (int i = 0; i < SceneManager.ActiveScene.ActiveCameras.Count; i++)
			{
				SceneManager.ActiveScene.ActiveCameras[i].Update(elapsed);
			}

			
			IceProfiler.StopProfiling(IceProfilerNames.ICE_CORE_MAIN_UPDATE);
		}

		
		private static void UpdateItemsComponents(SceneItem item)
		{
			if (item.Components == null)
			{
				return;
			}
			for (int i = 0; i < item.Components.Count; i++)
			{
				IceComponent _component = item.Components[i];
				if (_component.Enabled == true)
				{
					_component.Update(IceCream.Game.Instance.Elapsed);
				}                
			}
		}
		
		#endregion
	}
}
