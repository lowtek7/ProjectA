using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Unity
{
	/// <summary>
	/// 엔티티가 가지고 있는 컴포넌트 정보를 셋팅하기 위한 Designer class
	/// </summary>
	public class ComponentDesigner : MonoBehaviour
	{
		[SerializeField, HideInInspector]
		private List<string> components = new List<string>();

		public List<string> Components => components;
	}

	/// <summary>
	/// 리플렉션에 의해 컴포넌트의 정보를 수집하기 위한 어트리뷰트
	/// </summary>
	public class ComponentDescriptionAttribute : Attribute
	{
		public string DisplayName { get; }
		
		public ComponentDescriptionAttribute()
		{
			DisplayName = string.Empty;
		}

		public ComponentDescriptionAttribute(string displayName)
		{
			DisplayName = displayName;
		}
	}
}
