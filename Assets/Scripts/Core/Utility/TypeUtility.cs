﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;

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
			var playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);
			List<Type> results = new List<Type>();
			foreach (var playerAssembly in playerAssemblies)
			{
				var assembly = Assembly.Load(playerAssembly.name);
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

			return results.ToArray();
		}
		
		public static Type[] GetTypesWithInterface(Type interfaceType)
		{
			var playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);
			List<Type> results = new List<Type>();
			foreach (var playerAssembly in playerAssemblies)
			{
				var assembly = Assembly.Load(playerAssembly.name);
				var types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (interfaceType == type) continue;

					var interfaces = type.GetInterfaces();
					var resultIndex = Array.FindIndex(interfaces, t => t == interfaceType);
					if (resultIndex >= 0)
					{
						results.Add(type);
					}
				}
			}

			return results.ToArray();
		}
	}
}
