#if UNITY_EDITOR
using Core.Unity;
using Game.Ecs.Component;
using UnityEditor;
using UnityEngine;

namespace Tool.Visual.Gizmos
{
	[Gizmo(typeof(CapsuleColliderComponent))]
	public class CapsuleColliderComponentGizmoDrawer : IGizmosDrawer
	{
		public void OnDrawGizmo(Transform transform, IComponent component)
		{
		}

		public void OnDrawGizmoSelected(Transform transform, IComponent component)
		{
			if (component is CapsuleColliderComponent capsuleColliderComponent)
			{
				var center = transform.position + capsuleColliderComponent.Center;
				//var rot = Quaternion.LookRotation(capsuleColliderComponent.Direction, Vector3.up);

				DrawWireCapsule(center, transform.rotation, capsuleColliderComponent.Radius, capsuleColliderComponent.Height, Color.red);
			}
		}

		public static void DrawWireCapsule(Vector3 _pos, Quaternion _rot, float _radius, float _height, Color _color = default(Color))
		{
			if (_color != default(Color))
				Handles.color = _color;
			Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
			using (new Handles.DrawingScope(angleMatrix))
			{
				var pointOffset = (_height - (_radius * 2)) / 2;

				//draw sideways
				Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
				Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
				Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
				Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
				//draw frontways
				Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
				Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
				Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
				Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
				//draw center
				Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
				Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);
			}
		}
	}
}
#endif
