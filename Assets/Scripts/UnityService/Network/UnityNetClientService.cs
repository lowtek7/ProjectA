using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using BlitzEcs;
using Core.Utility;
using Game.Ecs.Component;
using LiteNetLib;
using LiteNetLib.Utils;
using MemoryPack;
using Network.Packet.Handler;
using RAMG.Packets;
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

		private readonly Queue<INetCommand> periodCommands = new Queue<INetCommand>();

		private readonly Dictionary<int, INetCommand> periodCommandMap = new Dictionary<int, INetCommand>();

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

		private float timePassed;

		public void Init(World world)
		{
			netIdToEntityIds.Clear();

			timePassed = 0f;
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
			if (!onlineMode)
			{
				return;
			}

			commands.Enqueue(command);
		}

		public void SendPeriodCommand(PeriodPacketType periodPacketType, INetCommand command)
		{
		}

		public void SendPeriodCommand(string key, PeriodPacketType periodPacketType, INetCommand command)
		{
			var hash = key.GetHashCode();

			if (periodCommandMap.TryGetValue(hash, out var prevCommand))
			{
				if (prevCommand is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}

			periodCommandMap[hash] = command;
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
			var dt = Time.deltaTime;

			if (netClient is not null && localPeer.ConnectionState == ConnectionState.Connected)
			{
				netClient.PollEvents();

				var peer = netClient.FirstPeer;

				if (peer is { ConnectionState: ConnectionState.Connected })
				{
					timePassed += dt;

					// 나중에 period 타입에 따라 분리해서 보낼 예정... 지금은 그냥 0.5초마다 싱크하게 간단히 구현.
					if (timePassed >= 0.5f)
					{
						foreach (var keyValues in periodCommandMap)
						{
							var command = keyValues.Value;

							SendCommandInternal(peer, command);
						}

						periodCommandMap.Clear();

						foreach (var command in periodCommands)
						{
							SendCommandInternal(peer, command);
						}

						periodCommands.Clear();

						timePassed = 0f;
					}

					// FIXME: 나중에는 커맨드가 쌓여있으면 모아서 보내자.

					// 여기서 패킷을 보내야한다.
					while (commands.TryDequeue(out var command))
					{
						SendCommandInternal(peer, command);
					}
				}
				else
				{
					if (commands.Count > 0)
					{
						commands.Clear();
					}

					if (periodCommands.Count > 0)
					{
						periodCommands.Clear();
					}

					if (periodCommandMap.Count > 0)
					{
						periodCommandMap.Clear();
					}
				}
			}
		}

		private void SendCommandInternal(NetPeer peer, INetCommand command)
		{
			// 일단 임시로 메모리 스트림 사용...
			using (var ms = new MemoryStream())
			{
				using (var binaryWriter = new BinaryWriter(ms))
				{
					// Writer를 사용해 패킷 생성
					var commandBytes = MemoryPackSerializer.Serialize(command.GetType(), command, MemoryPackSerializerOptions.Utf8);
					var length = Convert.ToUInt16(commandBytes.Length);
					var opcode = command.Opcode;

					binaryWriter.Write(length);
					binaryWriter.Write(opcode);
					binaryWriter.Write(commandBytes);

					// 패킷 보내기
					var data = ms.ToArray();
					peer.Send(data, DeliveryMethod.ReliableOrdered);

					Debug.Log($"Client Send : {data}");

					if (command is IDisposable disposable)
					{
						// 커맨드를 Dispose
						disposable.Dispose();
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
			using var state = MemoryPackReaderOptionalStatePool.Rent(MemoryPackSerializerOptions.Default);

			var length = reader.GetUShort();
			var opcodeValue = reader.GetShort();
			var opcode = (Opcode) opcodeValue;

			reader.GetBytes(buffer, length);

			var body = buffer.AsSpan(new Range(0, length));
			var memoryPackReader = new MemoryPackReader(body, state);
			// command를 불러오기전에 Disconnect 체크가 필요한가?
			var command = packetManager.ToCommand(opcode, ref memoryPackReader);

			Debug.Log($"OnNetworkReceive. Peer Id : {peer.Id}, Remote Id : {peer.RemoteId}, length : {length}, opcode : {opcode} command : {command}");

			if (command is IDisposable disposable)
			{
				switch (opcode)
				{
					case Opcode.CCMD_PLAYER_WORLD_JOIN:
					{
						if (command is CCMD_PLAYER_WORLD_JOIN playerServerJoin)
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
					case Opcode.CMD_ENTITY_POS_SYNC:
					{
						if (command is CMD_ENTITY_POS_SYNC entityPosSync)
						{
							PosSyncEntity(entityPosSync.Id, entityPosSync);
						}
						break;
					}
					case Opcode.CMD_ENTITY_ROTATE:
					{
						if (command is CMD_ENTITY_ROTATE entityRotate)
						{
							RotateEntity(entityRotate.Id, entityRotate);
						}
						break;
					}
					case Opcode.CMD_ENTITY_TELEPORT:
					{
						if (command is CMD_ENTITY_TELEPORT entityTeleport)
						{
							TeleportEntity(entityTeleport.Id, entityTeleport);
						}
						break;
					}
				}

				disposable.Dispose();
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

				if (!entity.Has<NetworkEntityComponent>())
				{
					entity.Add<NetworkEntityComponent>();
				}

				entity.Get<NetworkEntityComponent>().EntityRole = EntityRole.Local;
				entity.Get<NetworkEntityComponent>().NetId = currentNetId;

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
			entity.Add(new UnitComponent
			{
				SourceGuid = Guid.Parse("e43acb59-4e52-48c7-8715-1025d20f345b")
			});
			entity.Add(new UnitInstanceComponent
			{
				InstanceGuid = Guid.NewGuid()
			});
			entity.Add(new TransformComponent());
			entity.Add(new PlayerComponent());
			entity.Add(new CapsuleColliderComponent
			{
				Center = new Vector3(0, 0.5f, 0),
				Direction = new Vector3(0, 1.0f, 0),
				Radius = 0.25f,
				Height = 2f
			});
			entity.Add(new GravityComponent());
			entity.Add(new NetworkEntityComponent
			{
				NetId = netId,
				EntityRole = EntityRole.Remote
			});
			entity.Add(new NetMovementComponent());
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

				if (entity.IsAlive && entity.Has<TransformComponent>() && entity.Has<MovementComponent>())
				{
					ref var netMovementComponent = ref entity.Get<NetMovementComponent>();

					netMovementComponent.Velocity = new Vector3(entityMove.VelocityX, entityMove.VelocityY, entityMove.VelocityZ);
					netMovementComponent.IsMoving = entityMove.MoveType != MoveType.None;
				}
			}
		}

		/// <summary>
		/// 캐릭터가 움직이지 않는 경우 pos 싱크 시도
		/// </summary>
		/// <param name="netId"></param>
		/// <param name="entityPosSync"></param>
		private void PosSyncEntity(int netId, CMD_ENTITY_POS_SYNC entityPosSync)
		{
			if (netIdToEntityIds.TryGetValue(netId, out var entityId))
			{
				var entity = new Entity(currentWorld, entityId);

				if (entity.IsAlive && entity.Has<TransformComponent>())
				{
					var targetPos = new Vector3(entityPosSync.X, entityPosSync.Y, entityPosSync.Z);
					ref var transformComponent = ref entity.Get<TransformComponent>();
					ref var netMovementComponent = ref entity.Get<NetMovementComponent>();

					if (!transformComponent.Position.IsAlmostCloseTo(targetPos))
					{
						transformComponent.Position = targetPos;
					}

					netMovementComponent.Velocity = Vector3.zero;
					netMovementComponent.IsMoving = false;
				}
			}
		}

		private void RotateEntity(int netId, CMD_ENTITY_ROTATE entityRotate)
		{
			if (netIdToEntityIds.TryGetValue(netId, out var entityId))
			{
				var entity = new Entity(currentWorld, entityId);

				if (entity.IsAlive && entity.Has<TransformComponent>() && entity.Has<MovementComponent>())
				{
					ref var netMovementComponent = ref entity.Get<NetMovementComponent>();

					netMovementComponent.GoalRotation = new Quaternion(entityRotate.X, entityRotate.Y, entityRotate.Z, entityRotate.W);
				}
			}
		}

		private void TeleportEntity(int netId, CMD_ENTITY_TELEPORT entityTeleport)
		{
			if (netIdToEntityIds.TryGetValue(netId, out var entityId))
			{
				var entity = new Entity(currentWorld, entityId);

				if (entity.IsAlive && entity.Has<TransformComponent>() && entity.Has<MovementComponent>())
				{
					ref var transformComponent = ref entity.Get<TransformComponent>();

					transformComponent.Position = new Vector3(entityTeleport.X, entityTeleport.Y, entityTeleport.Z);
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
