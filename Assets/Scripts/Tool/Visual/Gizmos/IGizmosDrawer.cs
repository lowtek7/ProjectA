using System;
using Core.Unity;
using UnityEngine;

namespace Tool.Visual.Gizmos
{
	/// <summary>
	/// 컴포넌트 타입에 대한 기즈모 어트리뷰트
	/// 생성한 컴포넌트 타입 기반으로 기즈모 타입이 지정된다.
	/// </summary>
	public class GizmoAttribute : Attribute
	{
		public Type ComponentType { get; }

		public GizmoAttribute(Type componentType)
		{
			ComponentType = componentType;
		}
	}

	/// <summary>
	/// 기즈모 드로어의 인터페이스
	/// </summary>
	public interface IGizmosDrawer
	{
		void OnDrawGizmo(Transform transform, IComponent component);

		void OnDrawGizmoSelected(Transform transform, IComponent component);
	}

	public class UnknownDrawer : IGizmosDrawer
	{
		private static UnknownDrawer instance = new UnknownDrawer();

		public static UnknownDrawer Instance => instance;

		public void OnDrawGizmo(Transform transform, IComponent component)
		{
		}

		public void OnDrawGizmoSelected(Transform transform, IComponent component)
		{
		}
	}
}
