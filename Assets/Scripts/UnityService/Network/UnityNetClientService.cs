using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using BlitzEcs;
using Core.Utility;
using Game.Ecs.Component;
using Game.Unit;
using LiteNetLib;
using LiteNetLib.Utils;
using MemoryPack;
using Network.NetCommand.Client.Entity;
using Network.NetCommand.Client.Player;
using Network.NetCommand.Server.Player;
using Network.Packet;
using Network.Packet.Handler;
using Service;
using Service.Network;
using UnityEngine;

namespace UnityService.Network
{
	[UnityService(typeof(INetClientService))]
	public class UnityNetClientService : MonoBehaviour, INetClientService, INetEventListener, INetLogger, IGameServiceCallback, ILoaderService
	{
		private NetManager netClient;

		private NetDataWriter writer;

		private PacketManager packetManager;

		[SerializeField]
		private bool onlineMode = false;

		[SerializeField]
		private string hostAddress;

		private int currentPort;

		private readonly Queue<INetCommand> commands = new Queue<INetCommand>();

		private readonly Dictionary<int, int> netIdToEntityIds = new Dictionary<int, int>();

		private static int DefaultPort => 47221;

		private int currentNetId = -1;

		private byte[] buffer = new byte[4096];


		/// <summary>
		/// 임시로 월드 스테이지 Guid 설정.
		/// </summary>
		private Guid worldStageGuid;

		public int NetId => currentNetId;

		private World currentWorld;

		private NetPeer localPeer;

		public void Init(World world)
		{
			netIdToEntityIds.Clear();

			currentWorld = world;
			currentNetId = -1;
			packetManager = new PacketManager();
			netClient = new NetManager(this);
			writer = new NetDataWriter();

			packetManager.Init();

			netClient.UnconnectedMessagesEnabled = true;
			netClient.Start();

			// 포트번호가 주소에 포함되어 있는지 체크. 없다면 기본 포트 사용.
			if (hostAddress.Contains(':'))
			{
				var addressArray = hostAddress.Split(':');

				currentPort = Convert.ToInt32(addressArray[1]);
				localPeer = netClient.Connect(addressArray[0], currentPort, writer);
			}
			else
			{
				currentPort = DefaultPort;
				localPeer = netClient.Connect(hostAddress, DefaultPort, writer);
			}
		}

		/// <summary>
		/// 실제로 전송 전에 큐에 넣어준다.
		/// </summary>
		/// <param name="command"></param>
		public void SendCommand(INetCommand command)
		{
			commands.Enqueue(command);
		}

		private void OnDestroy()
		{
			while (commands.TryDequeue(out var command))
			{
				if (command is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}

			if (netClient != null)
			{
				netClient.Stop(true);
				netClient = null;
			}

			packetManager = null;
			currentWorld = null;

			netIdToEntityIds.Clear();
		}

		private void Update()
		{
			if (netClient is not null && localPeer.ConnectionState == ConnectionState.Connected)
			{
				netClient.PollEvents();

				var peer = netClient.FirstPeer;

				if (peer is { ConnectionState: ConnectionState.Connected })
				{
					// 여기서 패킷을 보내야한다.
					while (commands.TryDequeue(out var command))
					{
						if (command is IDisposable disposable)
						{
							// 일단 임시로 메모리 스트림 사용...
							using (var ms = new MemoryStream())
							{
								using (var binaryWriter = new BinaryWriter(ms))
								{
									// Writer를 사용해 패킷 생성
									var commandBytes = MemoryPackSerializer.Serialize(command.GetType(), command,
										MemoryPackSerializerOptions.Utf8);
									var length = Convert.ToUInt16(commandBytes.Length);
									var opcode = command.Opcode;

									binaryWriter.Write(length);
									binaryWriter.Write(opcode);
									binaryWriter.Write(commandBytes);

									// 패킷 보내기
									var data = ms.ToArray();
									peer.Send(data, DeliveryMethod.ReliableOrdered);

									Debug.Log($"Client Send : {data}");

									// 커맨드를 Dispose
									disposable.Dispose();
								}
							}
						}
					}
				}
				else
				{
					if (commands.Count > 0)
					{
						commands.Clear();
					}
				}
			}
		}

		public void OnPeerConnected(NetPeer peer)
		{
			currentNetId = peer.RemoteId;

			Debug.Log($"Client Connected. PeerId : {peer.Id}, Net Id : {currentNetId}");
		}

		public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
		{
			Debug.Log($"Client Disconnected. PeerId : {peer.Id}");

			currentNetId = -1;
		}

		public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
		{
		}

		public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
		{
			using (var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default))
			{
				var length = reader.GetUShort();
				var opcodeValue = reader.GetShort();
				var opcode = (Opcode) opcodeValue;

				reader.GetBytes(buffer, length);

				var body = buffer.AsSpan(new Range(0, length));

				var memoryPackReader = new MemoryPackReader(body, state);
				// command를 불러오기전에 Disconnect 체크가 필요한가?
				var command = packetManager.ToCommand(ref memoryPackReader);

				if (command is IDisposable disposable)
				{
					switch (opcode)
					{
						case Opcode.ERROR_CODE:
							break;
						case Opcode.CCMD_PLAYER_JOIN:
						{
							if (command is CCMD_PLAYER_SERVERJOIN playerServerJoin)
							{
								var netId = playerServerJoin.Id;

								SpawnNetPlayer(netId);
							}
							break;
						}
						case Opcode.CMD_ENTITY_MOVE:
						{
							if (command is CMD_ENTITY_MOVE entityMove)
							{
								MoveEntity(entityMove.Id, entityMove);
							}
							break;
						}
					}

					disposable.Dispose();
				}
			}
		}

		public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
		{
		}

		public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
		{
		}

		public void OnConnectionRequest(ConnectionRequest request)
		{
		}

		public void WriteNet(NetLogLevel level, string str, params object[] args)
		{
			Debug.LogFormat(str, args);
		}

		public void OnActivate()
		{
		}

		public void OnDeactivate()
		{
		}

		public void OnLoadWorld()
		{
			// 임시로 플레이어 컴포넌트 구성요소 캐싱 해두기.
			var query = new Query<PlayerComponent, StageSpecComponent>(currentWorld);

			foreach (var entity in query)
			{
				var stageSpecComponent = entity.Get<StageSpecComponent>();

				worldStageGuid = stageSpecComponent.StageGuid;
				break;
			}

			// 여기서 플레이어가 입장했다고 서버에다 알려줘야한다.
			// 그렇게 된다면 서버에서는 다른 플레이어 정보도 같이 날려 줄 것이다.
			if (localPeer.ConnectionState == ConnectionState.Connected)
			{
				var command = SCMD_PLAYER_LOGIN.Create();

				command.Id = currentNetId;
				command.Time = DateTime.UtcNow.ToUnixTime();

				SendCommand(command);
			}

			Debug.Log("NetClientService:OnLoadWorld Call");
		}

		private void SpawnNetPlayer(int netId)
		{
			if (netIdToEntityIds.ContainsKey(netId))
			{
				return;
			}

			var entity = currentWorld.Spawn();
			entity.Add(new MovementComponent
			{
				WalkSpeed = 1,
				RunSpeed = 3,
				RotateSpeed = 1200
			});
			entity.Add(new PlayerComponent
			{
				PlayerType = PlayerType.Remote
			});
			entity.Add(new CapsuleColliderComponent
			{
				Center = new Vector3(0, 0.5f, 0),
				Direction = new Vector3(0, 1.0f, 0),
				Radius = 0.25f,
				Height = 2f
			});
			entity.Add(new GravityComponent());
			entity.Add(new NetIdComponent
			{
				NetId = netId
			});
			entity.Add(new StageSpecComponent
			{
				StageGuid = worldStageGuid
			});

			netIdToEntityIds.Add(netId, entity.Id);
		}

		/// <summary>
		/// 일단은 단순히 좌표만 동기화 중.
		/// </summary>
		/// <param name="netId"></param>
		/// <param name="entityMove"></param>
		private void MoveEntity(int netId, CMD_ENTITY_MOVE entityMove)
		{
			if (netIdToEntityIds.TryGetValue(netId, out var entityId))
			{
				var entity = new Entity(currentWorld, entityId);

				if (entity.IsAlive && entity.Has<TransformComponent>())
				{
					ref var transformComponent = ref entity.Get<TransformComponent>();

					transformComponent.Position = new Vector3(entityMove.X, entityMove.Y, entityMove.Z);
				}
			}
		}

		public void OnAwake()
		{
		}

		public IEnumerator Load()
		{
			Debug.Log("NetClientService:Load start");

			if (onlineMode)
			{
				// net id가 셋팅 될때 까지 대기시키기.
				while (currentNetId == -1)
				{
					yield return null;
				}
			}

			Debug.Log("NetClientService:Load complete");
		}
	}
}