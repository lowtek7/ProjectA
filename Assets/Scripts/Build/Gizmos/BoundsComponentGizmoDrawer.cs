#if UNITY_EDITOR
using Game.Ecs.Component;
using Core.Unity;
using UnityEditor;
using UnityEngine;

namespace Build.Gizmos
{
	/// <summary>
	/// 유니티 에디터에만 존재해야하는 코드.
	/// </summary>
	[Gizmo(typeof(BoundsComponent))]
	public class BoundsComponentGizmoDrawer : IGizmosDrawer
	{
		public void OnDrawGizmo(Transform transform, IComponent component)
		{
		}

		public void OnDrawGizmoSelected(Transform transform, IComponent component)
		{
			if (component is BoundsComponent boundsComponent)
			{
				var bounds = boundsComponent.GetBounds(transform.position);

				Handles.DrawWireCube(bounds.center, bounds.size);
			}
		}
	}
}

#endif
