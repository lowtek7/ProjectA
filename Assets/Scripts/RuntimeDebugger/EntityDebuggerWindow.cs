using System.Collections.Generic;
using System.Reflection;
using BlitzEcs;
using Game.Ecs.Component;
using Service;
using Service.World;
using UnityEditor;
using UnityEngine;

namespace RuntimeDebugger
{
	public class EntityDebuggerWindow : EditorWindow
	{
		private Vector2 scrollPos;

		private readonly Dictionary<int, Dictionary<int, bool>> foldMap = new Dictionary<int, Dictionary<int, bool>>();

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

						if (!foldMap.TryGetValue(entityId, out var componentFoldMap))
						{
							componentFoldMap = new Dictionary<int, bool>();
							foldMap[entityId] = componentFoldMap;
						}

						var entity = new Entity(world, entityId);

						if (entity.Has<NameComponent>())
						{
							var entityName = entity.Get<NameComponent>().Name;

							EditorGUILayout.LabelField($"{entityName} - Entity [{entityId}]");
						}
						else
						{
							EditorGUILayout.LabelField($"Entity [{entityId}]");
						}

						EditorGUI.indentLevel++;

						for (int i = 0; i < componentCount; i++)
						{
							if (world.TryGetIComponentPool(i, out var pool))
							{
								var componentType = pool.ComponentType;

								if (pool.Contains(entityId))
								{
									componentFoldMap[i] = EditorGUILayout.Foldout(componentFoldMap.ContainsKey(i) && componentFoldMap[i], $"{componentType.Name}");

									if (componentFoldMap[i])
									{
										EditorGUI.indentLevel++;

										foreach (var fieldInfo in componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
										{
											using (new EditorGUILayout.HorizontalScope())
											{
												var value = pool.GetWithBoxing(entityId);

												EditorGUILayout.LabelField($"{fieldInfo.Name}");

												EditorGUILayout.LabelField($"{fieldInfo.GetValue(value)}");
											}
										}

										EditorGUI.indentLevel--;
									}
								}
							}
						}

						EditorGUI.indentLevel--;

						++index;
					}

					scrollPos = scope.scrollPosition;
				}
			}
			else
			{
				foldMap.Clear();
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
