#if UNITY_EDITOR
using UnityEngine;

namespace Service.GizmoDraw
{
	public interface IGizmoDrawService : IGameService
	{
		void DrawWireCube(Vector3 position, Vector3 size, Color color);

		// void DrawSphere(Vector3 position, float size, Vector3 color);
		//
		// void DrawLine(Vector3 position, float size, Vector3 color);
		//
		// void DrawArc(Vector3 position, Vector3 size, Vector3 color);
	}
}
#endif
