using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content
{
	/// <summary>
	/// Inheriting from ContentManager is not supported in ExEn 
	/// (Load and Unload are not virtual)
	/// </summary>
	public class ContentManager : IDisposable
	{
		Dictionary<string, object> assets = new Dictionary<string, object>();

		public IServiceProvider ServiceProvider { get; private set; }

		private string _rootDirectory;
		public string RootDirectory
		{
			get { return _rootDirectory; }
			set
			{
				if(value == null)
					throw new ArgumentNullException("value");
				if(IsDisposed)
					throw new ObjectDisposedException(this.ToString());
				if(assets.Count > 0)
					throw new InvalidOperationException("Cannot change RootDirectory after assets have been loaded");
				_rootDirectory = value;
			}
		}


		#region Construction

		public ContentManager(IServiceProvider serviceProvider) : this(serviceProvider, string.Empty) { }
		public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
		{
			if(serviceProvider == null)
				throw new ArgumentNullException("serviceProvider");
			if(rootDirectory == null)
				throw new ArgumentNullException("rootDirectory");

			this.ServiceProvider = serviceProvider;
			this.RootDirectory = rootDirectory;
		}

		#endregion


		#region Unload and Dispose

		// API Difference: In XNA this method is virtual
		public void Unload()
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());

			foreach(object asset in assets)
			{
				IDisposable disposableAsset = asset as IDisposable;
				if(disposableAsset != null)
					disposableAsset.Dispose();
			}
			assets.Clear();
		}

		private bool IsDisposed { get { return assets == null; } }
		public void Dispose()
		{
			if(!IsDisposed)
				Unload();

			assets = null;
		}

		#endregion


		#region Pluggable Content Loaders

		public delegate object AssetLoader(string assetName, ContentManager contentManager);

		static Dictionary<Type, AssetLoader> contentLoaders = new Dictionary<Type, AssetLoader>();

		public static void RegisterLoader<T>(AssetLoader loader)
		{
			contentLoaders.Add(typeof(T), loader);
		}

		#endregion


		#region Content Loading

		// API Difference: In XNA this method is virtual
		public T Load<T>(string assetName)
		{
			if(IsDisposed)
				throw new ObjectDisposedException(this.ToString());
			if(string.IsNullOrEmpty(assetName))
				throw new ArgumentNullException("assetName");

			// Check database of loaded assets
			object result = null;
			if(assets.TryGetValue(assetName, out result))
			{
				if(!(result is T))
					throw new ContentLoadException("Cannot load \"" + assetName + "\" as " + typeof(T).Name + ", it is already loaded as " + result.GetType().Name);
				return (T)result;
			}

			// See if we have a loader for this type:
			AssetLoader loader;
			contentLoaders.TryGetValue(typeof(T), out loader);
			if(loader == null)
				throw new ContentLoadException("No loader registered for content type " + typeof(T).ToString());

			result = loader(assetName, this);
			if(result == null)
				throw new ContentLoadException("Failed to load asset \"" + assetName + "\"");

			assets.Add(assetName, result);

			return (T)result;
		}

		#endregion

	}
}
