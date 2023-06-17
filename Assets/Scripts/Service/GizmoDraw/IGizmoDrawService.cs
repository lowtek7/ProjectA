#if UNITY_EDITOR
using UnityEngine;

namespace Service.GizmoDraw
{
	public enum DrawType
	{
		Wire,
		Solid
	}

	public interface IGizmoDrawService : IGameService
	{
		void DrawCube(Vector3 center, Vector3 size, Color color);

		void DrawSphere(Vector3 center, float radius, Color color, DrawType type = DrawType.Wire);

		void DrawArc(Vector3 center, Vector3 direction, float angle, float radius,
			Color color, DrawType type = DrawType.Wire);

		void DrawLine(Vector3 from, Vector3 to, Color color);

		void DrawText(Vector3 position, string text);
	}
}
#endif
