using System;
#if UNITY_EDITOR
using Build.Gizmos;
#endif
using Core.Unity;
using UnityEngine;

namespace Build
{
	/// <summary>
	/// 엔티티의 컴포넌트 스펙들을 기록해두는 비헤이비어
	/// 기록해둔 정보를 토대로 엔티티를 빌드하는데 쓰인다.
	/// </summary>
	public class ComponentSpecBehaviour : MonoBehaviour
	{
		[SerializeReference]
		[SubclassSelector]
		private IComponent[] components = Array.Empty<IComponent>();

		public IComponent[] Components => components;

#if UNITY_EDITOR
		/// <summary>
		/// 컴포넌트 내용을 기반으로 필요한 gizmo를 그려준다.
		/// </summary>
		private void OnDrawGizmosSelected()
		{
			foreach (var component in Components)
			{
				if (GizmoHelper.TryGetDrawer(component.GetType(), out var drawer))
				{
					drawer.OnDrawGizmoSelected(gameObject.transform, component);
				}
			}
		}
#endif
	}
}
