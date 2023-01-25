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
		[SerializeReference, SubclassSelector]
		private List<IComponent> components = new List<IComponent>();

		public List<IComponent> Components => components;
	}

	/// <summary>
	/// 컴포넌트 디자이너에게 정보를 주기위해서 상속받아야하는 인터페이스
	/// </summary>
	public interface IComponent
	{
		
	}
}
