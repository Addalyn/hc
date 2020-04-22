using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class PowerUp : NetworkBehaviour
{
	public enum PowerUpCategory
	{
		NoCategory,
		Healing,
		Might,
		Movement,
		Energy
	}

	public interface IPowerUpListener
	{
		void OnPowerUpDestroyed(PowerUp powerUp);

		PowerUp[] GetActivePowerUps();

		void SetSpawningEnabled(bool enabled);

		void OnTurnTick();

		bool IsPowerUpSpawnPoint(BoardSquare square);

		void AddToSquaresToAvoidForRespawn(HashSet<BoardSquare> squaresToAvoid, ActorData forActor);
	}

	public class ExtraParams : Sequence.IExtraSequenceParams
	{
		public int m_pickupTeamAsInt;

		public bool m_ignoreSpawnSpline;

		public override void XSP_SerializeToStream(IBitStream stream)
		{
			sbyte value = (sbyte)m_pickupTeamAsInt;
			stream.Serialize(ref value);
			stream.Serialize(ref m_ignoreSpawnSpline);
		}

		public override void XSP_DeserializeFromStream(IBitStream stream)
		{
			sbyte value = (sbyte)m_pickupTeamAsInt;
			stream.Serialize(ref value);
			m_pickupTeamAsInt = value;
			stream.Serialize(ref m_ignoreSpawnSpline);
		}
	}

	private static int s_nextPowerupGuid;

	public Ability m_ability;

	public string m_powerUpName;

	public string m_powerUpToolTip;

	[AudioEvent(false)]
	public string m_audioEventPickUp = "ui_pickup_health";

	public GameObject m_sequencePrefab;

	public bool m_restrictPickupByTeam;

	public PowerUpCategory m_chatterCategory;

	[SyncVar]
	private Team m_pickupTeam = Team.Objects;

	[SyncVar(hook = "HookSetGuid")]
	private int m_guid = -1;

	[SyncVar]
	private uint m_sequenceSourceId;

	private List<int> m_clientSequenceIds = new List<int>();

	private BoardSquare m_boardSquare;

	[SyncVar]
	public bool m_isSpoil;

	[SyncVar]
	public bool m_ignoreSpawnSplineForSequence;

	public int m_duration;

	private int m_age;

	private bool m_pickedUp;

	private bool m_stolen;

	private ActorTag m_tags;

	private bool m_markedForRemoval;

	private SequenceSource _sequenceSource;

	private static int kRpcRpcOnPickedUp;

	private static int kRpcRpcOnSteal;

	public int Guid
	{
		get
		{
			return m_guid;
		}
		private set
		{
		}
	}

	public IPowerUpListener powerUpListener
	{
		get;
		set;
	}

	public BoardSquare boardSquare => m_boardSquare;

	public Team PickupTeam
	{
		get
		{
			return m_pickupTeam;
		}
		set
		{
		}
	}

	internal SequenceSource SequenceSource
	{
		get
		{
			if (_sequenceSource == null)
			{
				_sequenceSource = new SequenceSource(null, null, false);
			}
			return _sequenceSource;
		}
	}

	public Team Networkm_pickupTeam
	{
		get
		{
			return m_pickupTeam;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref m_pickupTeam, 1u);
		}
	}

	public int Networkm_guid
	{
		get
		{
			return m_guid;
		}
		[param: In]
		set
		{
			ref int guid = ref m_guid;
			if (NetworkServer.localClientActive)
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
				if (!base.syncVarHookGuard)
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
					base.syncVarHookGuard = true;
					HookSetGuid(value);
					base.syncVarHookGuard = false;
				}
			}
			SetSyncVar(value, ref guid, 2u);
		}
	}

	public uint Networkm_sequenceSourceId
	{
		get
		{
			return m_sequenceSourceId;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref m_sequenceSourceId, 4u);
		}
	}

	public bool Networkm_isSpoil
	{
		get
		{
			return m_isSpoil;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref m_isSpoil, 8u);
		}
	}

	public bool Networkm_ignoreSpawnSplineForSequence
	{
		get
		{
			return m_ignoreSpawnSplineForSequence;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref m_ignoreSpawnSplineForSequence, 16u);
		}
	}

	static PowerUp()
	{
		kRpcRpcOnPickedUp = -430057904;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PowerUp), kRpcRpcOnPickedUp, InvokeRpcRpcOnPickedUp);
		kRpcRpcOnSteal = 1919536730;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PowerUp), kRpcRpcOnSteal, InvokeRpcRpcOnSteal);
		NetworkCRC.RegisterBehaviour("PowerUp", 0);
	}

	public void SetPickupTeam(Team value)
	{
		Networkm_pickupTeam = value;
	}

	public void AddTag(string powerupTag)
	{
		if (m_tags == null)
		{
			while (true)
			{
				switch (4)
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
			m_tags = base.gameObject.GetComponent<ActorTag>();
			if (m_tags == null)
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
				m_tags = base.gameObject.AddComponent<ActorTag>();
			}
		}
		m_tags.AddTag(powerupTag);
	}

	public bool HasTag(string powerupTag)
	{
		if (m_tags != null)
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
					return m_tags.HasTag(powerupTag);
				}
			}
		}
		return false;
	}

	public void ClientSpawnSequences()
	{
		if (!NetworkClient.active)
		{
			return;
		}
		while (true)
		{
			switch (3)
			{
			case 0:
				continue;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (m_clientSequenceIds.Count != 0)
			{
				return;
			}
			while (true)
			{
				switch (1)
				{
				case 0:
					continue;
				}
				Board board = Board.Get();
				Vector3 position = base.transform.position;
				float x = position.x;
				Vector3 position2 = base.transform.position;
				BoardSquare boardSquareSafe = board.GetBoardSquareSafe(x, position2.z);
				ExtraParams extraParams = new ExtraParams();
				if (m_restrictPickupByTeam)
				{
					while (true)
					{
						switch (4)
						{
						case 0:
							continue;
						}
						break;
					}
					extraParams.m_pickupTeamAsInt = (int)m_pickupTeam;
				}
				else
				{
					extraParams.m_pickupTeamAsInt = 2;
				}
				extraParams.m_ignoreSpawnSpline = m_ignoreSpawnSplineForSequence;
				Sequence.IExtraSequenceParams[] extraParams2 = new Sequence.IExtraSequenceParams[1]
				{
					extraParams
				};
				if (_sequenceSource == null)
				{
					_sequenceSource = new SequenceSource(null, null, m_sequenceSourceId, false);
				}
				Sequence[] array = SequenceManager.Get().CreateClientSequences(m_sequencePrefab, boardSquareSafe, null, null, SequenceSource, extraParams2);
				bool flag = false;
				if (array == null)
				{
					return;
				}
				for (int i = 0; i < array.Length; i++)
				{
					array[i].RemoveAtTurnEnd = false;
					m_clientSequenceIds.Add(array[i].Id);
					if (flag)
					{
						continue;
					}
					while (true)
					{
						switch (2)
						{
						case 0:
							continue;
						}
						break;
					}
					if (PowerUpManager.Get() != null)
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
						array[i].transform.parent = PowerUpManager.Get().GetSpawnedPersistentSequencesRoot().transform;
						flag = true;
					}
				}
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
		}
	}

	public void CalculateBoardSquare()
	{
		Board board = Board.Get();
		Vector3 position = base.transform.position;
		float x = position.x;
		Vector3 position2 = base.transform.position;
		m_boardSquare = board.GetBoardSquareSafe(x, position2.z);
	}

	public void SetDuration(int duration)
	{
		m_duration = duration;
	}

	private void Awake()
	{
		if (!NetworkServer.active)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			Networkm_guid = s_nextPowerupGuid++;
			SequenceSource sequenceSource = SequenceSource;
			Networkm_sequenceSourceId = sequenceSource.RootID;
			return;
		}
	}

	private void HookSetGuid(int guid)
	{
		Networkm_guid = guid;
		PowerUpManager.Get().SetPowerUpGuid(this, guid);
	}

	private void Start()
	{
		if (m_ability == null)
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
			Log.Error(string.Concat("PowerUp ", this, " needs a valid Ability assigned in the inspector for its prefab"));
		}
		base.transform.parent = PowerUpManager.Get().GetSpawnedPowerupsRoot().transform;
		base.tag = "powerup";
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		int layer = LayerMask.NameToLayer("PowerUp");
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
		while (true)
		{
			switch (5)
			{
			case 0:
				continue;
			}
			if (m_boardSquare == null)
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
				CalculateBoardSquare();
			}
			if (!NetworkClient.active)
			{
				return;
			}
			while (true)
			{
				switch (1)
				{
				case 0:
					continue;
				}
				if (PowerUpManager.Get() != null)
				{
					while (true)
					{
						switch (4)
						{
						case 0:
							continue;
						}
						break;
					}
					PowerUpManager.Get().TrackClientPowerUp(this);
				}
				ClientSpawnSequences();
				return;
			}
		}
	}

	[Server]
	public void CheckForPickupOnSpawn()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PowerUp::CheckForPickupOnSpawn()' called on client");
		}
	}

	public void CheckForPickupOnTurnStart()
	{
	}

	public void MarkSequencesForRemoval()
	{
		m_markedForRemoval = true;
		if (!(SequenceManager.Get() != null))
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
			for (int i = 0; i < m_clientSequenceIds.Count; i++)
			{
				Sequence sequence = SequenceManager.Get().FindSequence(m_clientSequenceIds[i]);
				if (sequence != null)
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
					sequence.MarkForRemoval();
				}
			}
			while (true)
			{
				switch (1)
				{
				default:
					return;
				case 0:
					break;
				}
			}
		}
	}

	public bool WasMarkedForRemoval()
	{
		return m_markedForRemoval;
	}

	public void SetHideSequence(bool hide)
	{
		for (int i = 0; i < m_clientSequenceIds.Count; i++)
		{
			Sequence sequence = SequenceManager.Get().FindSequence(m_clientSequenceIds[i]);
			if (!(sequence != null))
			{
				continue;
			}
			Transform transform = sequence.gameObject.transform;
			Vector3 localPosition;
			if (hide)
			{
				while (true)
				{
					switch (4)
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
				localPosition = new Vector3(0f, -100f, 0f);
			}
			else
			{
				localPosition = Vector3.zero;
			}
			transform.localPosition = localPosition;
		}
	}

	[ClientRpc]
	private void RpcOnPickedUp(int pickedUpByActorIndex)
	{
		Client_OnPickedUp(pickedUpByActorIndex);
	}

	public void Client_OnPickedUp(int pickedUpByActorIndex)
	{
		ActorData actorData = GameFlowData.Get().FindActorByActorIndex(pickedUpByActorIndex);
		FogOfWar clientFog = FogOfWar.GetClientFog();
		if (clientFog != null)
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
			if (clientFog.IsVisible(boardSquare))
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
				string audioEventPickUp = m_audioEventPickUp;
				GameObject gameObject;
				if (actorData == null)
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
					gameObject = base.gameObject;
				}
				else
				{
					gameObject = actorData.gameObject;
				}
				AudioManager.PostEvent(audioEventPickUp, gameObject);
			}
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			child.gameObject.SetActive(false);
		}
		while (true)
		{
			switch (2)
			{
			case 0:
				continue;
			}
			GameEventManager.Get().FireEvent(GameEventManager.EventType.PowerUpActivated, new GameEventManager.PowerUpActivatedArgs
			{
				byActor = actorData,
				powerUp = this
			});
			MarkSequencesForRemoval();
			return;
		}
	}

	[ClientRpc]
	private void RpcOnSteal(int actorIndexFor3DAudio)
	{
		Client_OnSteal(actorIndexFor3DAudio);
	}

	public void Client_OnSteal(int actorIndexFor3DAudio)
	{
		FogOfWar clientFog = FogOfWar.GetClientFog();
		if (clientFog != null)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (clientFog.IsVisible(boardSquare))
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
				ActorData actorData = GameFlowData.Get().FindActorByActorIndex(actorIndexFor3DAudio);
				string audioEventPickUp = m_audioEventPickUp;
				GameObject gameObject;
				if (actorData == null)
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
					gameObject = base.gameObject;
				}
				else
				{
					gameObject = actorData.gameObject;
				}
				AudioManager.PostEvent(audioEventPickUp, gameObject);
			}
		}
		MarkSequencesForRemoval();
	}

	[Server]
	internal void Destroy()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PowerUp::Destroy()' called on client");
			return;
		}
		if (powerUpListener != null)
		{
			powerUpListener.OnPowerUpDestroyed(this);
		}
		NetworkServer.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		if (NetworkClient.active)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (PowerUpManager.Get() != null)
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
				PowerUpManager.Get().UntrackClientPowerUp(this);
			}
			MarkSequencesForRemoval();
			m_clientSequenceIds.Clear();
		}
		if (!(PowerUpManager.Get() != null))
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
			PowerUpManager.Get().OnPowerUpDestroy(this);
			return;
		}
	}

	public bool TeamAllowedForPickUp(Team team)
	{
		int result;
		if (m_restrictPickupByTeam)
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
			result = ((team == PickupTeam) ? 1 : 0);
		}
		else
		{
			result = 1;
		}
		return (byte)result != 0;
	}

	internal void OnTurnTick()
	{
	}

	public bool CanBeStolen()
	{
		return true;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcOnPickedUp(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcOnPickedUp called on server.");
		}
		else
		{
			((PowerUp)obj).RpcOnPickedUp((int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcOnSteal(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
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
					Debug.LogError("RPC RpcOnSteal called on server.");
					return;
				}
			}
		}
		((PowerUp)obj).RpcOnSteal((int)reader.ReadPackedUInt32());
	}

	public void CallRpcOnPickedUp(int pickedUpByActorIndex)
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
					Debug.LogError("RPC Function RpcOnPickedUp called on client.");
					return;
				}
			}
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcOnPickedUp);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)pickedUpByActorIndex);
		SendRPCInternal(networkWriter, 0, "RpcOnPickedUp");
	}

	public void CallRpcOnSteal(int actorIndexFor3DAudio)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcOnSteal called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcOnSteal);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.WritePackedUInt32((uint)actorIndexFor3DAudio);
		SendRPCInternal(networkWriter, 0, "RpcOnSteal");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
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
					writer.Write((int)m_pickupTeam);
					writer.WritePackedUInt32((uint)m_guid);
					writer.WritePackedUInt32(m_sequenceSourceId);
					writer.Write(m_isSpoil);
					writer.Write(m_ignoreSpawnSplineForSequence);
					return true;
				}
			}
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & 1) != 0)
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
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)m_pickupTeam);
		}
		if ((base.syncVarDirtyBits & 2) != 0)
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
			if (!flag)
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
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)m_guid);
		}
		if ((base.syncVarDirtyBits & 4) != 0)
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
			if (!flag)
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
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32(m_sequenceSourceId);
		}
		if ((base.syncVarDirtyBits & 8) != 0)
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
			if (!flag)
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
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(m_isSpoil);
		}
		if ((base.syncVarDirtyBits & 0x10) != 0)
		{
			if (!flag)
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
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(m_ignoreSpawnSplineForSequence);
		}
		if (!flag)
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
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			m_pickupTeam = (Team)reader.ReadInt32();
			m_guid = (int)reader.ReadPackedUInt32();
			m_sequenceSourceId = reader.ReadPackedUInt32();
			m_isSpoil = reader.ReadBoolean();
			m_ignoreSpawnSplineForSequence = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
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
			m_pickupTeam = (Team)reader.ReadInt32();
		}
		if ((num & 2) != 0)
		{
			HookSetGuid((int)reader.ReadPackedUInt32());
		}
		if ((num & 4) != 0)
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
			m_sequenceSourceId = reader.ReadPackedUInt32();
		}
		if ((num & 8) != 0)
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
			m_isSpoil = reader.ReadBoolean();
		}
		if ((num & 0x10) == 0)
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
			m_ignoreSpawnSplineForSequence = reader.ReadBoolean();
			return;
		}
	}
}
