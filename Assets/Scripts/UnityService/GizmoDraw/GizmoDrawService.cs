#if UNITY_EDITOR
using System.Collections.Generic;
using BlitzEcs;
using Service.GizmoDraw;
using UnityEditor;
using UnityEngine;

namespace UnityService.GizmoDraw
{
	[UnityService(typeof(IGizmoDrawService))]
	public class GizmoDrawService : MonoBehaviour, IGizmoDrawService
	{
		private struct DrawWireCubeRequest
		{
			public Color color;
			public Vector3 center;
			public Vector3 size;
		}

		private struct DrawSolidSphereRequest
		{
			public Color color;
			public Vector3 center;
			public float radius;
		}

		private struct DrawWireSphereRequest
		{
			public Color color;
			public Vector3 center;
			public float radius;
		}

		private struct DrawWireArcRequest
		{
			public Color color;
			public Vector3 center;
			public Vector3 from;
			public float angle;
			public float radius;
		}

		private struct DrawSolidArcRequest
		{
			public Color color;
			public Vector3 center;
			public Vector3 from;
			public float angle;
			public float radius;
		}

		private struct DrawLineRequest
		{
			public Vector3 from;
			public Vector3 to;
			public Color color;
		}

		private struct DrawTextRequest
		{
			public Vector3 position;
			public string text;
		}

		private class DrawBuffer
		{
			public readonly List<DrawWireCubeRequest> DrawWireCubeRequests = new();
			public readonly List<DrawSolidSphereRequest> DrawSolidSphereRequests = new();
			public readonly List<DrawWireSphereRequest> DrawWireSphereRequests = new();
			public readonly List<DrawWireArcRequest> DrawWireArcRequests = new();
			public readonly List<DrawSolidArcRequest> DrawSolidArcRequests = new();
			public readonly List<DrawLineRequest> DrawLineRequests = new();
			public readonly List<DrawTextRequest> DrawTextRequests = new();

			public void Clear()
			{
				DrawWireCubeRequests.Clear();
				DrawSolidSphereRequests.Clear();
				DrawWireSphereRequests.Clear();
				DrawWireArcRequests.Clear();
				DrawSolidArcRequests.Clear();
				DrawLineRequests.Clear();
				DrawTextRequests.Clear();
			}
		}

		private readonly DrawBuffer[] _drawBuffers = { new(), new() };

		private int _drawTargetBufferIndex = 0;

		private DrawBuffer CurrentDrawBuffer => _drawBuffers[_drawTargetBufferIndex];
		private DrawBuffer PrevDrawBuffer => _drawBuffers[1 - _drawTargetBufferIndex];

		private bool _hasNewRequest = false;
		private int _lastRequestedFrame = -1;

		/// <summary>
		/// 평면의 방향.
		/// FIXME : z축이 카메라 방향인 것으로 산정하고 했으므로,
		/// </summary>
		private readonly Vector3 _normalDir = Vector3.forward;

		private bool HasNewRequest
		{
			get => _hasNewRequest;
			set {
				if (value)
				{
					if (!_hasNewRequest)
					{
						// 새 요청이 들어오면 이전 요청들은 더 이상 들고 있을 필요가 없어서 비워줌
						PrevDrawBuffer.Clear();
					}
					else if (_lastRequestedFrame != Time.frameCount)
					{
						// 요청된 프레임이 변경되었는데 아직 hasNewRequest 상태라면 한 번 비워줌
						// FIXME : OnDrawGizmo과 프레임 종료 직전까지 사이에서 발생하는 요청은 무조건 무시되도록 되어있음
						CurrentDrawBuffer.Clear();
					}

					_lastRequestedFrame = Time.frameCount;
				}
				else
				{
					// GizmoDrawing이 한 번 끝났으므로 버퍼 인덱스 옮겨줌
					if (_hasNewRequest)
					{
						_drawTargetBufferIndex = 1 - _drawTargetBufferIndex;
					}
				}

				_hasNewRequest = value;
			}
		}

		public void Init(World world)
		{
			_lastRequestedFrame = Time.frameCount;
		}

		private void OnDrawGizmos()
		{
			if (_lastRequestedFrame < 0)
			{
				return;
			}

			DrawBuffer buffer = HasNewRequest ? CurrentDrawBuffer : PrevDrawBuffer;

			foreach (var request in buffer.DrawWireCubeRequests)
			{
				Handles.color = request.color;

				Handles.DrawWireCube(request.center, request.size);
			}

			foreach (var request in buffer.DrawSolidSphereRequests)
			{
				Handles.color = request.color;

				Handles.SphereHandleCap(0, request.center, Quaternion.Euler(_normalDir), request.radius * 2, EventType.Repaint);
			}

			foreach (var request in buffer.DrawWireSphereRequests)
			{
				Gizmos.color = request.color;

				Gizmos.DrawWireSphere(request.center, request.radius);
			}

			foreach (var request in buffer.DrawWireArcRequests)
			{
				Handles.color = request.color;

				if (request.radius < 360f)
				{
					Handles.DrawWireArc(request.center, _normalDir, request.from, request.angle, request.radius);
				}
				else
				{
					Handles.DrawWireDisc(request.center, _normalDir, request.radius);
				}
			}

			foreach (var request in buffer.DrawSolidArcRequests)
			{
				Handles.color = request.color;

				if (request.radius < 360f)
				{
					Handles.DrawSolidArc(request.center, _normalDir, request.from, request.angle, request.radius);
				}
				else
				{
					Handles.DrawSolidDisc(request.center, _normalDir, request.radius);
				}
			}

			foreach (var request in buffer.DrawLineRequests)
			{
				Gizmos.color = request.color;

				Gizmos.DrawLine(request.from, request.to);
			}

			foreach (var request in buffer.DrawTextRequests)
			{
				Handles.Label(request.position, request.text);
			}

			HasNewRequest = false;
		}

		public void DrawCube(Vector3 center, Vector3 size, Color color)
		{
			HasNewRequest = true;

			CurrentDrawBuffer.DrawWireCubeRequests.Add(new DrawWireCubeRequest
			{
				color = color,
				center = center,
				size = size,
			});
		}

		public void DrawSphere(Vector3 center, float radius, Color color, DrawType type = DrawType.Wire)
		{
			HasNewRequest = true;

			if (type == DrawType.Wire)
			{
				CurrentDrawBuffer.DrawWireSphereRequests.Add(new DrawWireSphereRequest
				{
					color = color,
					center = center,
					radius = radius
				});
			}
			else
			{
				CurrentDrawBuffer.DrawSolidSphereRequests.Add(new DrawSolidSphereRequest
				{
					color = color,
					center = center,
					radius = radius
				});
			}
		}

		public void DrawArc(Vector3 center, Vector3 direction, float angle, float radius, Color color, DrawType type = DrawType.Wire)
		{
			HasNewRequest = true;

			var from = Quaternion.AngleAxis(-angle / 2, _normalDir) * direction.normalized;
			var to = Quaternion.AngleAxis( angle / 2, _normalDir) * direction.normalized;

			if (type == DrawType.Wire)
			{
				CurrentDrawBuffer.DrawWireArcRequests.Add(new DrawWireArcRequest
				{
					color = color,
					center = center,
					from = from,
					angle = angle,
					radius = radius
				});

				// 360도 이하인 경우 나머지 테두리를 채워준다
				if (angle < 360f)
				{
					// 중앙 -> 호의 왼쪽 끝
					CurrentDrawBuffer.DrawLineRequests.Add(new DrawLineRequest
					{
						color = color,
						from = center,
						to = center + from * radius
					});

					// 중앙 -> 호의 오른쪽 끝
					CurrentDrawBuffer.DrawLineRequests.Add(new DrawLineRequest
					{
						color = color,
						from = center,
						to = center + to * radius
					});
				}
			}
			else
			{
				CurrentDrawBuffer.DrawSolidArcRequests.Add(new DrawSolidArcRequest
				{
					color = color,
					center = center,
					from = from,
					angle = angle,
					radius = radius
				});
			}
		}

		public void DrawLine(Vector3 from, Vector3 to, Color color)
		{
			HasNewRequest = true;

			CurrentDrawBuffer.DrawLineRequests.Add(new DrawLineRequest
			{
				color = color,
				from = from,
				to = to
			});
		}

		public void DrawText(Vector3 position, string text)
		{
			HasNewRequest = true;

			CurrentDrawBuffer.DrawTextRequests.Add(new DrawTextRequest
			{
				position = position,
				text = text
			});
		}
	}
}
#endif
