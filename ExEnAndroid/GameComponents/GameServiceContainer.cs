using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
	public class GameServiceContainer : IServiceProvider
	{
		Dictionary<Type, object> services = new Dictionary<Type, object>();

		public void AddService(Type type, object service)
		{
			services.Add(type, service);
		}

		public void RemoveService(Type type)
		{
			services.Remove(type);
		}

		public object GetService(Type type)
		{
			object retval = null;
			services.TryGetValue(type, out retval);
			return retval;
		}


		internal IEnumerable<object> GetAllServices()
		{
			return services.Values;
		}
	}
}
