using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace Microsoft.Xna.Framework.Storage
{
	public class StorageContainer : IDisposable
	{
		private readonly string _path;
		private readonly StorageDevice _device;
		private readonly string _name;

		public StorageContainer(StorageDevice device, string name)
		{
			_device = device;
			_name = name;
			_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)+System.IO.Path.DirectorySeparatorChar+name;
			// Creathe the "device" if need
			if (!Directory.Exists(_path))
			{
				Directory.CreateDirectory(_path);
			}
		}

		public string Path
		{
			get
			{
				return _path;
			}
		}
		
		 public Microsoft.Xna.Framework.Storage.StorageDevice StorageDevice
		{
			get
			{
				return _device;
			}
		}

		public static string TitleLocation 
		{ 
			get
			{
				return Directory.GetParent(Assembly.GetEntryAssembly().Location).ToString();
			}
		}
		
		public string TitleName
		{
			get
			{
				return _name;
			}
		}


		#region IDisposable Members

		public void Dispose()
		{
			
		}

		#endregion
	}
}
