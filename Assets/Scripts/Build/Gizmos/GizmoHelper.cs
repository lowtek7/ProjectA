#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace Build.Gizmos
{
	public static class GizmoHelper
	{
		private static readonly Dictionary<Type, IGizmosDrawer> Drawers = new Dictionary<Type, IGizmosDrawer>();

		[InitializeOnLoadMethod]
		static void EditorInitialize()
		{
			Drawers.Clear();
		}

		public static bool TryGetDrawer(Type type, out IGizmosDrawer drawer)
		{
			drawer = UnknownDrawer.Instance;

			if (Drawers.TryGetValue(type, out var result))
			{
				if (result == UnknownDrawer.Instance)
				{
					return false;
				}
				else
				{
					drawer = result;
					return true;
				}
			}

			// 타입을 가져오려고 해본다.
			var types = TypeCache.GetTypesWithAttribute<GizmoAttribute>();

			foreach (var targetType in types)
			{
				GizmoAttribute gizmoAttribute = targetType.GetCustomAttribute<GizmoAttribute>();

				if (gizmoAttribute != null)
				{
					if (gizmoAttribute.ComponentType == type)
					{
						drawer = Activator.CreateInstance(targetType) as IGizmosDrawer;

						if (drawer != null)
						{
							Drawers[type] = drawer;
							return true;
						}
					}
				}
			}

			Drawers[type] = UnknownDrawer.Instance;
			return false;
		}
	}
}

#endif
