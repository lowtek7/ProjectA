using System.Collections.Generic;
using BlitzEcs;
using Service;
using Service.World;
using UnityEditor;
using UnityEngine;

namespace RuntimeDebugger
{
	public class EntityDebuggerWindow : EditorWindow
	{
		private Vector2 scrollPos;

		private Dictionary<int, Dictionary<int, bool>> foldMap = new Dictionary<int, Dictionary<int, bool>>();

		private void OnGUI()
		{
			if (ServiceManager.TryGetService(out IWorldGetterService worldGetterService))
			{
				var world = worldGetterService.World;
				var entityCount = world.EntityCount;
				var componentCount = world.ComponentCount;

				using (var scope = new EditorGUILayout.ScrollViewScope(scrollPos))
				{
					var index = 0;

					foreach (var entityId in world.EntityIds)
					{
						if (entityCount <= index)
						{
							break;
						}

						var entity = new Entity(world, entityId);

						EditorGUILayout.LabelField($"Entity [{entityId}]");

						EditorGUI.indentLevel++;

						for (int i = 0; i < componentCount; i++)
						{
							if (world.TryGetIComponentPool(i, out var pool))
							{
								var componentType = pool.ComponentType;

								if (pool.Contains(entityId))
								{
									EditorGUILayout.LabelField($"{componentType.Name}");
								}
							}
						}

						EditorGUI.indentLevel--;

						++index;
					}

					scrollPos = scope.scrollPosition;
				}
			}
		}

		[MenuItem("RAMG Project/Entity Debugger Window")]
		public static void ShowWindow()
		{
			var window = GetWindow<EntityDebuggerWindow>();

			window.titleContent = new GUIContent("Entity Debugger");
			window.Show(true);
		}
	}
}
