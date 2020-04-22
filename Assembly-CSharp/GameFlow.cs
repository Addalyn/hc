using Fabric;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class GameFlow : NetworkBehaviour
{
	public class SetHumanInfoMessage : MessageBase
	{
		public string m_userName;

		public string m_buildVersion;

		public string m_accountIdString;

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(m_userName);
			writer.Write(m_buildVersion);
			writer.Write(m_accountIdString);
		}

		public override void Deserialize(NetworkReader reader)
		{
			m_userName = reader.ReadString();
			m_buildVersion = reader.ReadString();
			m_accountIdString = reader.ReadString();
		}
	}

	public class SelectCharacterMessage : MessageBase
	{
		public string m_characterName;

		public int m_skinIndex;

		public int m_patternIndex;

		public int m_colorIndex;

		public override void Serialize(NetworkWriter writer)
		{
			writer.Write(m_characterName);
			writer.WritePackedUInt32((uint)m_skinIndex);
			writer.WritePackedUInt32((uint)m_patternIndex);
			writer.WritePackedUInt32((uint)m_colorIndex);
		}

		public override void Deserialize(NetworkReader reader)
		{
			m_characterName = reader.ReadString();
			m_skinIndex = (int)reader.ReadPackedUInt32();
			m_patternIndex = (int)reader.ReadPackedUInt32();
			m_colorIndex = (int)reader.ReadPackedUInt32();
		}
	}

	public class SetTeamFinalizedMessage : MessageBase
	{
		public int m_team;

		public override void Serialize(NetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)m_team);
		}

		public override void Deserialize(NetworkReader reader)
		{
			m_team = (int)reader.ReadPackedUInt32();
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PlayerComparer : IEqualityComparer<Player>
	{
		public bool Equals(Player x, Player y)
		{
			return x == y;
		}

		public int GetHashCode(Player obj)
		{
			return obj.GetHashCode();
		}
	}

	private Dictionary<Player, PlayerDetails> m_playerDetails = new Dictionary<Player, PlayerDetails>(default(PlayerComparer));

	private static GameFlow s_instance;

	private static int kRpcRpcDisplayConsoleText;

	private static int kRpcRpcSetMatchTime;

	internal Dictionary<Player, PlayerDetails> playerDetails => m_playerDetails;

	static GameFlow()
	{
		kRpcRpcDisplayConsoleText = -789469928;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameFlow), kRpcRpcDisplayConsoleText, InvokeRpcRpcDisplayConsoleText);
		kRpcRpcSetMatchTime = -559523706;
		NetworkBehaviour.RegisterRpcDelegate(typeof(GameFlow), kRpcRpcSetMatchTime, InvokeRpcRpcSetMatchTime);
		NetworkCRC.RegisterBehaviour("GameFlow", 0);
	}

	public override void OnStartClient()
	{
	}

	private void Client_OnDestroy()
	{
	}

	[Client]
	internal void SendCastAbility(ActorData caster, AbilityData.ActionType actionType, List<AbilityTarget> targets)
	{
		if (!NetworkClient.active)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					Debug.LogWarning("[Client] function 'System.Void GameFlow::SendCastAbility(ActorData,AbilityData/ActionType,System.Collections.Generic.List`1<AbilityTarget>)' called on server");
					return;
				}
			}
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.StartMessage(50);
		networkWriter.Write(caster.ActorIndex);
		networkWriter.Write((int)actionType);
		AbilityTarget.SerializeAbilityTargetList(targets, networkWriter);
		networkWriter.FinishMessage();
		ClientGameManager.Get().Client.SendWriter(networkWriter, 0);
	}

	internal static GameFlow Get()
	{
		return s_instance;
	}

	private void Awake()
	{
		s_instance = this;
	}

	private void Start()
	{
		OnLoadedLevel();
		GameFlowData.s_onGameStateChanged += OnGameStateChanged;
		if (NetworkServer.active)
		{
			return;
		}
		while (true)
		{
			switch (6)
			{
			case 0:
				continue;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (GameFlowData.Get() != null)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						continue;
					}
					OnGameStateChanged(GameFlowData.Get().gameState);
					return;
				}
			}
			return;
		}
	}

	private void OnLoadedLevel()
	{
		HighlightUtils.Get().HideCursor = false;
	}

	private void OnDestroy()
	{
		Client_OnDestroy();
		if (EventManager.Instance != null)
		{
			while (true)
			{
				switch (3)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
		}
		GameFlowData.s_onGameStateChanged -= OnGameStateChanged;
		s_instance = null;
	}

	private void OnGameStateChanged(GameState newState)
	{
		FogOfWar clientFog = FogOfWar.GetClientFog();
		if (clientFog != null)
		{
			clientFog.MarkForRecalculateVisibility();
		}
		if (newState != GameState.BothTeams_Decision)
		{
			while (true)
			{
				switch (4)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					if (newState != GameState.BothTeams_Resolve)
					{
						while (true)
						{
							switch (3)
							{
							default:
								return;
							case 0:
								break;
							}
						}
					}
					AudioManager.PostEvent("sw_game_state", AudioManager.EventAction.SetSwitch, "game_state_resolve");
					AudioManager.PostEvent("ui_resolution_cam_start");
					AudioManager.GetMixerSnapshotManager().SetMix_ResolveCam();
					if (GameEventManager.Get() != null)
					{
						while (true)
						{
							switch (7)
							{
							case 0:
								break;
							default:
								GameEventManager.Get().FireEvent(GameEventManager.EventType.ClientResolutionStarted, null);
								return;
							}
						}
					}
					return;
				}
			}
		}
		AudioManager.PostEvent("sw_game_state", AudioManager.EventAction.SetSwitch, "game_state_decision");
		AudioManager.GetMixerSnapshotManager().SetMix_DecisionCam();
	}

	internal void CheckTutorialAutoselectCharacter()
	{
		if (m_playerDetails.Count != 1)
		{
			return;
		}
		while (true)
		{
			switch (2)
			{
			case 0:
				continue;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (GameFlowData.Get().GetNumAvailableCharacterResourceLinks() != 1)
			{
			}
			return;
		}
	}

	public Player GetPlayerFromConnectionId(int connectionId)
	{
		using (Dictionary<Player, PlayerDetails>.Enumerator enumerator = m_playerDetails.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<Player, PlayerDetails> current = enumerator.Current;
				Player key = current.Key;
				if (key.m_connectionId == connectionId)
				{
					return current.Key;
				}
			}
			while (true)
			{
				switch (3)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					goto end_IL_000c;
				}
			}
			end_IL_000c:;
		}
		return default(Player);
	}

	public string GetPlayerHandleFromConnectionId(int connectionId)
	{
		string empty = string.Empty;
		using (Dictionary<Player, PlayerDetails>.Enumerator enumerator = m_playerDetails.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<Player, PlayerDetails> current = enumerator.Current;
				Player key = current.Key;
				if (key.m_connectionId == connectionId)
				{
					while (true)
					{
						switch (2)
						{
						case 0:
							break;
						default:
							if (1 == 0)
							{
								/*OpCode not supported: LdMemberToken*/;
							}
							return current.Value.m_handle;
						}
					}
				}
			}
			while (true)
			{
				switch (5)
				{
				case 0:
					break;
				default:
					return empty;
				}
			}
		}
	}

	public string GetPlayerHandleFromAccountId(long accountId)
	{
		string empty = string.Empty;
		using (Dictionary<Player, PlayerDetails>.Enumerator enumerator = m_playerDetails.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<Player, PlayerDetails> current = enumerator.Current;
				Player key = current.Key;
				if (key.m_accountId == accountId)
				{
					while (true)
					{
						switch (3)
						{
						case 0:
							break;
						default:
							if (1 == 0)
							{
								/*OpCode not supported: LdMemberToken*/;
							}
							return current.Value.m_handle;
						}
					}
				}
			}
			while (true)
			{
				switch (2)
				{
				case 0:
					break;
				default:
					return empty;
				}
			}
		}
	}

	[ClientRpc]
	private void RpcDisplayConsoleText(DisplayConsoleTextMessage message)
	{
		if (message.RestrictVisibiltyToTeam != Team.Invalid)
		{
			while (true)
			{
				switch (5)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (!(GameFlowData.Get().activeOwnedActorData != null))
			{
				return;
			}
			while (true)
			{
				switch (7)
				{
				case 0:
					continue;
				}
				break;
			}
			if (GameFlowData.Get().activeOwnedActorData.GetTeam() != message.RestrictVisibiltyToTeam)
			{
				return;
			}
			while (true)
			{
				switch (7)
				{
				case 0:
					continue;
				}
				break;
			}
		}
		string empty = string.Empty;
		if (!message.Unlocalized.IsNullOrEmpty())
		{
			while (true)
			{
				switch (5)
				{
				case 0:
					continue;
				}
				break;
			}
			empty = message.Unlocalized;
		}
		else if (message.Token.IsNullOrEmpty())
		{
			while (true)
			{
				switch (2)
				{
				case 0:
					continue;
				}
				break;
			}
			empty = StringUtil.TR(message.Term, message.Context);
		}
		else
		{
			empty = string.Format(StringUtil.TR(message.Term, message.Context), message.Token);
		}
		TextConsole.Get().Write(new TextConsole.Message
		{
			Text = empty,
			MessageType = message.MessageType,
			RestrictVisibiltyToTeam = message.RestrictVisibiltyToTeam,
			SenderHandle = message.SenderHandle,
			CharacterType = message.CharacterType
		});
	}

	[ClientRpc]
	private void RpcSetMatchTime(float timeSinceMatchStart)
	{
		UITimerPanel.Get().SetMatchTime(timeSinceMatchStart);
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (!initialState)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		if (!initialState)
		{
			while (true)
			{
				switch (2)
				{
				case 0:
					continue;
				}
				break;
			}
			if (base.syncVarDirtyBits == 0)
			{
				return false;
			}
			while (true)
			{
				switch (1)
				{
				case 0:
					continue;
				}
				break;
			}
		}
		NetworkWriterAdapter networkWriterAdapter = new NetworkWriterAdapter(writer);
		int value = m_playerDetails.Count;
		networkWriterAdapter.Serialize(ref value);
		if (value >= 0)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				break;
			}
			if (value <= 20)
			{
				goto IL_009f;
			}
		}
		Log.Error("Invalid number of players: " + value);
		value = Mathf.Clamp(value, 0, 20);
		goto IL_009f;
		IL_009f:
		using (Dictionary<Player, PlayerDetails>.Enumerator enumerator = m_playerDetails.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<Player, PlayerDetails> current = enumerator.Current;
				Player key = current.Key;
				PlayerDetails value2 = current.Value;
				if (value2 != null)
				{
					while (true)
					{
						switch (7)
						{
						case 0:
							continue;
						}
						break;
					}
					key.OnSerializeHelper(networkWriterAdapter);
					value2.OnSerializeHelper(networkWriterAdapter);
				}
			}
			while (true)
			{
				switch (4)
				{
				case 0:
					continue;
				}
				break;
			}
		}
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		uint num = uint.MaxValue;
		if (!initialState)
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			num = reader.ReadPackedUInt32();
		}
		if (num == 0)
		{
			return;
		}
		while (true)
		{
			switch (2)
			{
			case 0:
				continue;
			}
			NetworkReaderAdapter networkReaderAdapter = new NetworkReaderAdapter(reader);
			int value = m_playerDetails.Count;
			networkReaderAdapter.Serialize(ref value);
			if (value >= 0)
			{
				while (true)
				{
					switch (6)
					{
					case 0:
						continue;
					}
					break;
				}
				if (value <= 20)
				{
					goto IL_0094;
				}
				while (true)
				{
					switch (1)
					{
					case 0:
						continue;
					}
					break;
				}
			}
			Log.Error("Invalid number of players: " + value);
			value = Mathf.Clamp(value, 0, 20);
			goto IL_0094;
			IL_0094:
			m_playerDetails.Clear();
			for (int i = 0; i < value; i++)
			{
				Player key = default(Player);
				PlayerDetails playerDetails = new PlayerDetails(PlayerGameAccountType.None);
				key.OnSerializeHelper(networkReaderAdapter);
				playerDetails.OnSerializeHelper(networkReaderAdapter);
				key.m_accountId = playerDetails.m_accountId;
				m_playerDetails[key] = playerDetails;
				if ((bool)GameFlowData.Get() && GameFlowData.Get().LocalPlayerData == null)
				{
					while (true)
					{
						switch (3)
						{
						case 0:
							continue;
						}
						break;
					}
					GameFlowData.Get().SetLocalPlayerData();
				}
			}
			while (true)
			{
				switch (4)
				{
				default:
					return;
				case 0:
					break;
				}
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcDisplayConsoleText(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			while (true)
			{
				switch (5)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					Debug.LogError("RPC RpcDisplayConsoleText called on server.");
					return;
				}
			}
		}
		((GameFlow)obj).RpcDisplayConsoleText(GeneratedNetworkCode._ReadDisplayConsoleTextMessage_None(reader));
	}

	protected static void InvokeRpcRpcSetMatchTime(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			while (true)
			{
				switch (2)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					Debug.LogError("RPC RpcSetMatchTime called on server.");
					return;
				}
			}
		}
		((GameFlow)obj).RpcSetMatchTime(reader.ReadSingle());
	}

	public void CallRpcDisplayConsoleText(DisplayConsoleTextMessage message)
	{
		if (!NetworkServer.active)
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					Debug.LogError("RPC Function RpcDisplayConsoleText called on client.");
					return;
				}
			}
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDisplayConsoleText);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteDisplayConsoleTextMessage_None(networkWriter, message);
		SendRPCInternal(networkWriter, 0, "RpcDisplayConsoleText");
	}

	public void CallRpcSetMatchTime(float timeSinceMatchStart)
	{
		if (!NetworkServer.active)
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					Debug.LogError("RPC Function RpcSetMatchTime called on client.");
					return;
				}
			}
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcSetMatchTime);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(timeSinceMatchStart);
		SendRPCInternal(networkWriter, 0, "RpcSetMatchTime");
	}
}
