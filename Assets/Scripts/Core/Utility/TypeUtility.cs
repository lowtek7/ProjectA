using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Utility
{
	public class TypeUtility
	{
		/// <summary>
		/// TypeCache.GetTypesWithAttribute()을 사용하거나 이것을 사용하거나 둘 중 하나
		/// 다만 TypeCache쪽은 UnityEditor에 속하기 때문에 런타임에서는 이것을 사용해야 한다.
		/// </summary>
		/// <param name="attributeType"></param>
		/// <returns></returns>
		public static Type[] GetTypesWithAttribute(Type attributeType)
		{
			List<Type> results = new List<Type>();
			var currentAssembly = Assembly.GetExecutingAssembly();
			var referencedAssemblies = currentAssembly.GetReferencedAssemblies().ToList();

			referencedAssemblies.Insert(0, currentAssembly.GetName());
			foreach (var assemblyName in referencedAssemblies)
			{
				var assembly = Assembly.Load(assemblyName);
				if (assembly != null)
				{
					var types = assembly.GetTypes();
					foreach (var type in types)
					{
						var result = type.GetCustomAttribute(attributeType);
						if (result != null)
						{
							results.Add(type);
						}
					}
				}
			}

			return results.ToArray();
		}
		
		public static Type[] GetTypesWithInterface(Type interfaceType)
		{
			List<Type> results = new List<Type>();
			var currentAssembly = Assembly.GetExecutingAssembly();
			var referencedAssemblies = currentAssembly.GetReferencedAssemblies().ToList();

			referencedAssemblies.Insert(0, currentAssembly.GetName());
			foreach (var assemblyName in referencedAssemblies)
			{
				var assembly = Assembly.Load(assemblyName);
				if (assembly != null)
				{
					var types = assembly.GetTypes();
					foreach (var type in types)
					{
						var result = type.GetInterface(interfaceType.FullName);
						if (result != null)
						{
							results.Add(type);
						}
					}
				}
			}

			return results.ToArray();
		}
	}
}
