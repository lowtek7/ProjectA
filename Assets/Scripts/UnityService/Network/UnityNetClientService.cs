using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using BlitzEcs;
using LiteNetLib;
using LiteNetLib.Utils;
using MemoryPack;
using Network.Packet.Handler;
using Service.Network;
using UnityEngine;

namespace UnityService.Network
{
	[UnityService(typeof(INetClientService))]
	public class UnityNetClientService : MonoBehaviour, INetClientService, INetEventListener, INetLogger
	{
		private NetManager netClient;

		private NetDataWriter writer;

		private PacketManager packetManager;

		[SerializeField]
		private string hostAddress;

		private int currentPort;

		private readonly Queue<INetCommand> commands = new Queue<INetCommand>();

		private static int DefaultPort => 47221;

		private int currentNetId = -1;

		public int NetId => currentNetId;

		public void Init(World world)
		{
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
				netClient.Connect(addressArray[0], currentPort, writer);
			}
			else
			{
				currentPort = DefaultPort;
				netClient.Connect(hostAddress, DefaultPort, writer);
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
		}

		private void Update()
		{
			if (netClient is not null)
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
				var memoryPackReader = new MemoryPackReader(reader.RawData, state);
				// command를 불러오기전에 Disconnect 체크가 필요한가?
				var command = packetManager.ToCommand(ref memoryPackReader);

				if (command is IDisposable disposable)
				{
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
	}
}
