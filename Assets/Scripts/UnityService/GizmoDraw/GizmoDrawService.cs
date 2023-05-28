#if UNITY_EDITOR
using System.Collections.Generic;
using BlitzEcs;
using Service.GizmoDraw;
using UnityEngine;

namespace UnityService.GizmoDraw
{
	[UnityService(typeof(IGizmoDrawService))]
	public class GizmoDrawService : MonoBehaviour, IGizmoDrawService
	{
		private struct DrawCubeRequest
		{
			public Color color;
			public Vector3 position;
			public Vector3 size;
		}

		private class DrawBuffer
		{
			public List<DrawCubeRequest> DrawCubeRequests = new();

			public void Clear()
			{
				DrawCubeRequests.Clear();
			}
		}

		private DrawBuffer[] _drawBuffers = { new(), new() };

		private int _drawTargetBufferIndex = 0;

		private DrawBuffer CurrentDrawBuffer => _drawBuffers[_drawTargetBufferIndex];
		private DrawBuffer PrevDrawBuffer => _drawBuffers[1 - _drawTargetBufferIndex];

		private bool _hasNewRequest = false;
		private int _lastRequestedFrame = -1;

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

		public void DrawWireCube(Vector3 position, Vector3 size, Color color)
		{
			HasNewRequest = true;

			CurrentDrawBuffer.DrawCubeRequests.Add(new DrawCubeRequest
			{
				color = color,
				position = position,
				size = size
			});
		}

		private void OnDrawGizmos()
		{
			if (_lastRequestedFrame < 0)
			{
				return;
			}

			DrawBuffer buffer = HasNewRequest ? CurrentDrawBuffer : PrevDrawBuffer;

			foreach (var request in buffer.DrawCubeRequests)
			{
				Gizmos.color = request.color;

				Gizmos.DrawWireCube(request.position, request.size);
			}

			HasNewRequest = false;
		}
	}
}
#endif
