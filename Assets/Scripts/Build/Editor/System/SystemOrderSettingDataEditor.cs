using System;
using System.Collections.Generic;
using System.Linq;
using Core.Unity;
using Core.Utility;
using Game.Asset;
using MackySoft.SerializeReferenceExtensions.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace Build.Editor.System
{
	[CustomEditor(typeof(SystemOrderSettingData))]
	public class SystemOrderSettingDataEditor : UnityEditor.Editor
	{
		private ReorderableList _reorderableList;
		private AdvancedTypePopup _popupCache;
		private List<Type> _typeList;

		private void OnEnable()
		{
			_reorderableList = new ReorderableList(serializedObject,
				serializedObject.FindProperty("systemOrders"),
				true, true, true, true);
			
			_reorderableList.drawElementCallback = DrawElementCallback;
			_reorderableList.onAddDropdownCallback = OnAddDropdownCallback;

			if (_popupCache != null)
			{
				_popupCache.OnItemSelected -= PopupCacheOnOnItemSelected;
			}

			_popupCache = null;
			_typeList = null;
		}

		private void OnDisable()
		{
			if (_popupCache != null)
			{
				_popupCache.OnItemSelected -= PopupCacheOnOnItemSelected;
			}

			_popupCache = null;
			_typeList = null;
		}

		private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
		{
			_typeList = TypeUtility.GetTypesWithInterface(typeof(ISystem)).ToList();
			
			var listProperty = list.serializedProperty;

			for (int i = 0; i < listProperty.arraySize; i++)
			{
				var fullName = listProperty.GetArrayElementAtIndex(i).stringValue;
				var index = _typeList.FindIndex(x => x.FullName == fullName);

				if (index >= 0)
				{
					_typeList.RemoveAt(index);
				}
			}
			
			if (_popupCache == null)
			{
				var state = new AdvancedDropdownState();
				_popupCache = new AdvancedTypePopup(_typeList,
					13,
					state);
				
				_popupCache.OnItemSelected += PopupCacheOnOnItemSelected;
			}

			buttonRect.x -= 450;
			buttonRect.width = 400;

			_popupCache.SetTypes(_typeList);
			_popupCache.Show(buttonRect);
		}

		private void PopupCacheOnOnItemSelected(AdvancedTypePopupItem item)
		{
			var listProperty = _reorderableList.serializedProperty;

			listProperty.arraySize++;

			listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1).stringValue = item.Type.FullName;
			listProperty.serializedObject.ApplyModifiedProperties();
			listProperty.serializedObject.Update();
		}

		private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			var element = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
			
			EditorGUI.LabelField(rect, element.stringValue);
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			_reorderableList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}
	}
}
