using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Utility
{
	public class TypeUtility
	{
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
	}
}
