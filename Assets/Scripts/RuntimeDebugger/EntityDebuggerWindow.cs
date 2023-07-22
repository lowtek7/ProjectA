using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlitzEcs;
using Core.Unity;
using Core.Utility;
using Game.Ecs.Component;
using MackySoft.SerializeReferenceExtensions.Editor;
using Service;
using Service.World;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RuntimeDebugger
{
	public class EntityDebuggerWindow : EditorWindow
	{
		private Vector2 scrollPos;

		private readonly Dictionary<int, Dictionary<int, bool>> foldMap = new Dictionary<int, Dictionary<int, bool>>();

		private readonly HashSet<Type> _includedComponents = new();
		private readonly HashSet<Type> _excludedComponents = new();

		private readonly List<Type> _removedByFilterBuffer = new();

		private void OnGUI()
		{
			if (ServiceManager.TryGetService(out IWorldGetterService worldGetterService))
			{
				var world = worldGetterService.World;

				if (world == null)
				{
					return;
				}

				var entityCount = world.EntityCount;
				var componentCount = world.ComponentCount;

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label("Included");

					if (GUILayout.Button("Add"))
					{
						OnAdd(_includedComponents);
					}
				}

				foreach (var type in _includedComponents)
				{
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Label(type.FullName);

						if (GUILayout.Button(" - "))
						{
							_removedByFilterBuffer.Add(type);
						}
					}
				}

				foreach (var type in _removedByFilterBuffer)
				{
					_includedComponents.Remove(type);
				}

				_removedByFilterBuffer.Clear();

				EditorGUILayout.Space();

				using (new GUILayout.HorizontalScope())
				{
					GUILayout.Label("Excluded");

					if (GUILayout.Button("Add"))
					{
						OnAdd(_excludedComponents);
					}
				}

				foreach (var type in _excludedComponents)
				{
					using (new GUILayout.HorizontalScope())
					{
						GUILayout.Label(type.FullName);

						if (GUILayout.Button(" - "))
						{
							_removedByFilterBuffer.Add(type);
						}
					}
				}

				foreach (var type in _removedByFilterBuffer)
				{
					_excludedComponents.Remove(type);
				}

				_removedByFilterBuffer.Clear();

				EditorGUILayout.Space();

				using (var scope = new EditorGUILayout.ScrollViewScope(scrollPos))
				{
					var index = 0;

					foreach (var entityId in world.EntityIds)
					{
						if (entityCount <= index)
						{
							break;
						}

						index++;

						if (!foldMap.TryGetValue(entityId, out var componentFoldMap))
						{
							componentFoldMap = new Dictionary<int, bool>();
							foldMap[entityId] = componentFoldMap;
						}

						var entity = new Entity(world, entityId);

						if (_includedComponents.Count > 0 || _excludedComponents.Count > 0)
						{
							var needShow = true;

							var includedCount = 0;

							for (int i = 0; i < componentCount; i++)
							{
								if (world.TryGetIComponentPool(i, out var pool) && pool.Contains(entityId))
								{
									var componentType = pool.ComponentType;

									if (_excludedComponents.Contains(componentType))
									{
										needShow = false;
										break;
									}

									if (_includedComponents.Contains(componentType))
									{
										includedCount++;
									}
								}
							}

							if (!needShow || includedCount < _includedComponents.Count)
							{
								continue;
							}
						}

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

		private void OnAdd(HashSet<Type> target)
		{
			var componentTypes = TypeUtility.GetTypesWithInterface(typeof(IComponent)).ToList();

			foreach (var type in target)
			{
				componentTypes.Remove(type);
			}

			var state = new AdvancedDropdownState();
			var popup = new AdvancedTypePopup(componentTypes,
				13,
				state);

			popup.SetTypes(componentTypes);
			popup.Show(new Rect(0, 0, 400, 400));

			popup.OnItemSelected += (item) =>
			{
				target.Add(item.Type);
			};
		}
	}
}
