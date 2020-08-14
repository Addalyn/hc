using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Barrier
{
	private string m_name;

	private Vector3 m_center;

	private Vector3 m_endpoint1;

	private Vector3 m_endpoint2;

	private Vector3 m_facingDir;

	private bool m_bidirectional;

	private bool m_makeClientGeo;

	private GameObject m_generatedClientGeometry;

	private Team m_team;

	private ActorData m_owner;

	public SpoilsSpawnData m_spoilsSpawnOnEnemyMovedThrough;

	public SpoilsSpawnData m_spoilsSpawnOnAllyMovedThrough;

	public bool m_removeAtTurnEndIfEnemyMovedThrough;

	public bool m_removeAtTurnEndIfAllyMovedThrough;

	public bool m_removeAtPhaseEndIfEnemyMovedThrough;

	public bool m_removeAtPhaseEndIfAllyMovedThrough;

	public AbilityPriority m_customEndPhase = AbilityPriority.INVALID;

	public bool m_removeAtPhaseEndIfCasterKnockedBack;

	private int m_maxHits;

	public GameplayResponseForActor m_onEnemyMovedThrough;
	public GameplayResponseForActor m_onAllyMovedThrough;
	public bool m_endOnCasterDeath;
	public EffectDuration m_time;

	public int m_guid;

	public List<Sequence> m_barrierSequences;

	private List<GameObject> m_barrierSequencePrefabs;

	private bool m_playSequences;

	private bool m_considerAsCover;

	public string Name
	{
		get
		{
			return m_name;
		}
		private set
		{
			m_name = value;
		}
	}

	public ActorData Caster
	{
		get
		{
			return m_owner;
		}
		private set
		{
			m_owner = value;
		}
	}

	public SequenceSource BarrierSequenceSource
	{
		get;
		protected set;
	}

	public BlockingRules BlocksVision
	{
		get;
		private set;
	}

	public BlockingRules BlocksAbilities
	{
		get;
		private set;
	}

	public BlockingRules BlocksMovement
	{
		get;
		private set;
	}

	public BlockingRules BlocksMovementOnCrossover
	{
		get;
		private set;
	}

	public BlockingRules BlocksPositionTargeting
	{
		get;
		private set;
	}

	public bool ConsiderAsCover
	{
		get
		{
			return m_considerAsCover;
		}
		set
		{
			m_considerAsCover = value;
		}
	}

	public Barrier(int guid, string name, Vector3 center, Vector3 facingDir, float width, bool bidirectional, BlockingRules blocksVision, BlockingRules blocksAbilities, BlockingRules blocksMovement, BlockingRules blocksPositionTargeting, bool considerAsCover, int maxDuration, ActorData owner, List<GameObject> barrierSequencePrefabs = null, bool playSequences = true, GameplayResponseForActor onEnemyMovedThrough = null, GameplayResponseForActor onAllyMovedThrough = null, int maxHits = -1, bool endOnCasterDeath = false, SequenceSource parentSequenceSource = null, Team barrierTeam = Team.Invalid)
	{
		InitBarrier(guid, name, center, facingDir, width, bidirectional, blocksVision, blocksAbilities, blocksMovement, BlockingRules.ForNobody, blocksPositionTargeting, considerAsCover, maxDuration, owner, barrierSequencePrefabs, playSequences, onEnemyMovedThrough, onAllyMovedThrough, maxHits, endOnCasterDeath, parentSequenceSource, barrierTeam);
	}

	public Vector3 GetCenterPos()
	{
		return m_center;
	}

	public Vector3 GetEndPos1()
	{
		return m_endpoint1;
	}

	public Vector3 GetEndPos2()
	{
		return m_endpoint2;
	}

	public Team GetBarrierTeam()
	{
		return m_team;
	}

	private bool UnlimitedHits()
	{
		return m_maxHits < 0;
	}

	private void InitBarrier(int guid, string name, Vector3 center, Vector3 facingDir, float width, bool bidirectional, BlockingRules blocksVision, BlockingRules blocksAbilities, BlockingRules blocksMovement, BlockingRules blocksMovementOnCrossover, BlockingRules blocksPositionTargeting, bool considerAsCover, int maxDuration, ActorData owner, List<GameObject> barrierSequencePrefabs, bool playSequences, GameplayResponseForActor onEnemyMovedThrough, GameplayResponseForActor onAllyMovedThrough, int maxHits, bool endOnCasterDeath, SequenceSource parentSequenceSource, Team barrierTeam)
	{
		m_guid = guid;
		m_name = name;
		m_center = center;
		m_facingDir = facingDir;
		m_bidirectional = bidirectional;
		Vector3 a = Vector3.Cross(facingDir, Vector3.up);
		a.Normalize();
		float d = width * Board.Get().squareSize;
		m_endpoint1 = center + a * d / 2f;
		m_endpoint2 = center - a * d / 2f;
		BlocksVision = blocksVision;
		BlocksAbilities = blocksAbilities;
		BlocksMovement = blocksMovement;
		BlocksMovementOnCrossover = blocksMovementOnCrossover;
		BlocksPositionTargeting = blocksPositionTargeting;
		m_considerAsCover = considerAsCover;
		m_owner = owner;
		if (m_owner != null)
		{
			m_team = m_owner.GetTeam();
		}
		else
		{
			m_team = barrierTeam;
		}
		m_time = new EffectDuration();
		m_time.duration = maxDuration;
		m_barrierSequencePrefabs = barrierSequencePrefabs;
		int playSequences2;
		if (playSequences)
		{
			playSequences2 = ((m_barrierSequencePrefabs != null) ? 1 : 0);
		}
		else
		{
			playSequences2 = 0;
		}
		m_playSequences = ((byte)playSequences2 != 0);
		m_barrierSequences = new List<Sequence>();
		if (m_playSequences)
		{
			BarrierSequenceSource = new SequenceSource(null, null, false, parentSequenceSource);
		}
		m_maxHits = maxHits;
		m_onEnemyMovedThrough = onEnemyMovedThrough;
		m_onAllyMovedThrough = onAllyMovedThrough;
		m_endOnCasterDeath = endOnCasterDeath;
	}

	public bool CanBeSeenThroughBy(ActorData viewer)
	{
		bool flag = IsBlocked(viewer, BlocksVision);
		return !flag;
	}

	public bool CanBeShotThroughBy(ActorData shooter)
	{
		if (BarrierManager.Get().SuppressingAbilityBlocks())
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					break;
				default:
					return true;
				}
			}
		}
		bool flag = IsBlocked(shooter, BlocksAbilities);
		return !flag;
	}

	public bool CanBeMovedThroughBy(ActorData mover)
	{
		bool flag = IsBlocked(mover, BlocksMovement);
		return !flag;
	}

	public bool CanMoveThroughAfterCrossoverBy(ActorData mover)
	{
		bool flag = IsBlocked(mover, BlocksMovementOnCrossover);
		return !flag;
	}

	public bool IsPositionTargetingBlockedFor(ActorData caster)
	{
		return IsBlocked(caster, BlocksPositionTargeting);
	}

	private bool IsBlocked(ActorData actor, BlockingRules rules)
	{
		switch (rules)
		{
		case BlockingRules.ForEverybody:
			return true;
		case BlockingRules.ForNobody:
			return false;
		case BlockingRules.ForEnemies:
			if (actor == null)
			{
				return true;
			}
			return actor.GetTeam() != m_team;
		default:
			return false;
		}
	}

	public virtual void OnStart(bool delayVisionUpdate, out List<ActorData> visionUpdaters)
	{
		visionUpdaters = new List<ActorData>();
		if (!NetworkClient.active || !m_makeClientGeo)
		{
			return;
		}
		float squareSize = Board.Get().squareSize;
		Vector3 a = m_endpoint2 - m_endpoint1;
		bool flag = Mathf.Abs(a.z) > Mathf.Abs(a.x);
		Vector3 vector = m_endpoint1 + 0.5f * a;
		m_generatedClientGeometry = GameObject.CreatePrimitive(PrimitiveType.Cube);
		m_generatedClientGeometry.transform.position = new Vector3(vector.x, 1.5f * squareSize, vector.z);
		if (flag)
		{
			while (true)
			{
				switch (2)
				{
				case 0:
					break;
				default:
					m_generatedClientGeometry.transform.localScale = new Vector3(0.25f, 2f * squareSize, a.magnitude);
					return;
				}
			}
		}
		m_generatedClientGeometry.transform.localScale = new Vector3(a.magnitude, 2f * squareSize, 0.25f);
	}

	public virtual void OnEnd()
	{
		if (NetworkServer.active)
		{
			using (List<Sequence>.Enumerator enumerator = m_barrierSequences.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Sequence current = enumerator.Current;
					if (current != null)
					{
						current.MarkForRemoval();
					}
				}
			}
		}
		if (!NetworkClient.active)
		{
			return;
		}
		while (true)
		{
			if (!m_makeClientGeo)
			{
				return;
			}
			while (true)
			{
				if (m_generatedClientGeometry != null)
				{
					Object.DestroyObject(m_generatedClientGeometry);
				}
				m_generatedClientGeometry = null;
				return;
			}
		}
	}

	public bool CanAffectVision()
	{
		int result;
		if (BlocksVision != BlockingRules.ForEnemies)
		{
			result = ((BlocksVision == BlockingRules.ForEverybody) ? 1 : 0);
		}
		else
		{
			result = 1;
		}
		return (byte)result != 0;
	}

	public bool CanAffectMovement()
	{
		int result;
		if (BlocksMovement != BlockingRules.ForEnemies)
		{
			result = ((BlocksMovement == BlockingRules.ForEverybody) ? 1 : 0);
		}
		else
		{
			result = 1;
		}
		return (byte)result != 0;
	}

	public bool CrossingBarrier(Vector3 src, Vector3 dest)
	{
		bool flag = false;
		bool flag2 = VectorUtils.IsPointInLaser(src, m_endpoint1, m_endpoint2, 0.001f);
		bool flag3 = VectorUtils.IsPointInLaser(dest, m_endpoint1, m_endpoint2, 0.001f);
		if (flag2)
		{
			while (true)
			{
				switch (3)
				{
				case 0:
					break;
				default:
					return false;
				}
			}
		}
		if (!flag3)
		{
			if (VectorUtils.OnSameSideOfLine(src, dest, m_endpoint1, m_endpoint2))
			{
				while (true)
				{
					switch (5)
					{
					case 0:
						break;
					default:
						return false;
					}
				}
			}
		}
		if (!flag3)
		{
			if (VectorUtils.OnSameSideOfLine(m_endpoint1, m_endpoint2, src, dest))
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						break;
					default:
						return false;
					}
				}
			}
		}
		if (m_bidirectional)
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					break;
				default:
					return true;
				}
			}
		}
		Vector3 lhs = src - m_center;
		float num = Vector3.Dot(lhs, m_facingDir);
		if (num > 0f)
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					break;
				default:
					return true;
				}
			}
		}
		return false;
	}

	public bool CrossingBarrierForVision(Vector3 src, Vector3 dest)
	{
		return SegmentsIntersectForVision(src, dest, m_endpoint1, m_endpoint2);
	}

	private bool SegmentsIntersectForVision(Vector3 startA, Vector3 endA, Vector3 startB, Vector3 endB)
	{
		return PointsAreCounterClockwise(startA, startB, endB) != PointsAreCounterClockwise(endA, startB, endB) && PointsAreCounterClockwise(startA, endA, startB) != PointsAreCounterClockwise(startA, endA, endB);
	}

	private static bool PointsAreCounterClockwise(Vector3 a, Vector3 b, Vector3 c)
	{
		return (c.z - a.z) * (b.x - a.x) > (b.z - a.z) * (c.x - a.x);
	}

	public Vector3 GetIntersectionPoint(Vector3 src, Vector3 dest)
	{
		Vector3 vector = dest - src;
		Vector3 directionOfSecond = m_endpoint2 - m_endpoint1;
		bool intersecting;
		Vector3 lineLineIntersection = VectorUtils.GetLineLineIntersection(src, vector, m_endpoint1, directionOfSecond, out intersecting);
		if (intersecting)
		{
			lineLineIntersection.y = src.y;
			Vector3 normalized = (-vector).normalized;
			lineLineIntersection += normalized * 0.05f;
		}
		return lineLineIntersection;
	}

	public Vector3 GetCollisionNormal(Vector3 incomingDir)
	{
		if (m_bidirectional)
		{
			if (Vector3.Dot(incomingDir, m_facingDir) > 0f)
			{
				while (true)
				{
					switch (6)
					{
					case 0:
						break;
					default:
						return -m_facingDir;
					}
				}
			}
		}
		return m_facingDir;
	}

	public Vector3 GetFacingDir()
	{
		return m_facingDir;
	}

	public static BarrierSerializeInfo BarrierToSerializeInfo(Barrier barrier)
	{
		BarrierSerializeInfo barrierSerializeInfo = new BarrierSerializeInfo();
		barrierSerializeInfo.m_guid = barrier.m_guid;
		barrierSerializeInfo.m_center = barrier.m_center;
		barrierSerializeInfo.m_widthInWorld = (barrier.m_endpoint1 - barrier.m_endpoint2).magnitude;
		barrierSerializeInfo.m_facingHorizontalAngle = VectorUtils.HorizontalAngle_Deg(barrier.m_facingDir);
		barrierSerializeInfo.m_bidirectional = barrier.m_bidirectional;
		barrierSerializeInfo.m_blocksVision = (sbyte)barrier.BlocksVision;
		barrierSerializeInfo.m_blocksAbilities = (sbyte)barrier.BlocksAbilities;
		barrierSerializeInfo.m_blocksMovement = (sbyte)barrier.BlocksMovement;
		barrierSerializeInfo.m_blocksMovementOnCrossover = (sbyte)barrier.BlocksMovementOnCrossover;
		barrierSerializeInfo.m_blocksPositionTargeting = (sbyte)barrier.BlocksPositionTargeting;
		barrierSerializeInfo.m_considerAsCover = barrier.m_considerAsCover;
		barrierSerializeInfo.m_team = (sbyte)barrier.m_team;
		int ownerIndex = ActorData.s_invalidActorIndex;
		if (barrier.m_owner != null)
		{
			ownerIndex = barrier.m_owner.ActorIndex;
		}
		barrierSerializeInfo.m_ownerIndex = ownerIndex;
		barrierSerializeInfo.m_makeClientGeo = barrier.m_makeClientGeo;
		return barrierSerializeInfo;
	}

	public static Barrier CreateBarrierFromSerializeInfo(BarrierSerializeInfo info)
	{
		BlockingRules blocksVision = (BlockingRules)info.m_blocksVision;
		BlockingRules blocksAbilities = (BlockingRules)info.m_blocksAbilities;
		BlockingRules blocksMovement = (BlockingRules)info.m_blocksMovement;
		BlockingRules blocksMovementOnCrossover = (BlockingRules)info.m_blocksMovementOnCrossover;
		BlockingRules blocksPositionTargeting = (BlockingRules)info.m_blocksPositionTargeting;
		ActorData owner = null;
		if (info.m_ownerIndex != ActorData.s_invalidActorIndex)
		{
			owner = GameFlowData.Get().FindActorByActorIndex(info.m_ownerIndex);
		}
		Vector3 facingDir = VectorUtils.AngleDegreesToVector(info.m_facingHorizontalAngle);
		float width = info.m_widthInWorld / Board.Get().squareSize;
		Barrier barrier = new Barrier(info.m_guid, string.Empty, info.m_center, facingDir, width, info.m_bidirectional, blocksVision, blocksAbilities, blocksMovement, blocksPositionTargeting, info.m_considerAsCover, -1, owner);
		barrier.BlocksMovementOnCrossover = blocksMovementOnCrossover;
		barrier.m_makeClientGeo = info.m_makeClientGeo;
		return barrier;
	}

	public virtual List<ServerClientUtils.SequenceStartData> GetSequenceStartDataList()
	{
		List<ServerClientUtils.SequenceStartData> list = new List<ServerClientUtils.SequenceStartData>();
		if (m_barrierSequencePrefabs != null && m_playSequences)
		{
			Quaternion targetRotation = Quaternion.LookRotation(m_facingDir);
			ActorData[] targetActorArray = new ActorData[0];
			foreach (GameObject prefab in m_barrierSequencePrefabs)
			{
				if (prefab != null)
				{
					bool requiresExtraParams = false;
					foreach (Sequence sequence in prefab.GetComponents<Sequence>())
					{
						if (sequence is OverwatchScanSequence || sequence is GroundLineSequence || sequence is ExoLaserHittingWallSequence)
						{
							requiresExtraParams = true;
							break;
						}
					}
					Sequence.IExtraSequenceParams[] extraParams = null;
					if (requiresExtraParams)
					{
						extraParams = new Sequence.IExtraSequenceParams[1];
						GroundLineSequence.ExtraParams extraParam = new GroundLineSequence.ExtraParams();
						extraParam.startPos = m_endpoint2;
						extraParam.endPos = m_endpoint1;
						extraParams[0] = extraParam;
					}
					ServerClientUtils.SequenceStartData seq = new ServerClientUtils.SequenceStartData(
						prefab, null, targetRotation, targetActorArray, m_owner, BarrierSequenceSource, extraParams);
					list.Add(seq);
				}
			}
		}
		return list;
	}

	public void DrawGizmos()
	{
		Vector3 b = new Vector3(0f, 0f, 0f);
		for (int i = 0; i < 3; i++)
		{
			b += new Vector3(0f, 0.3f, 0f);
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(m_endpoint1 + b, m_endpoint2 + b);
			Gizmos.color = Color.white;
			Gizmos.DrawLine(m_center + b, m_center + m_facingDir + b);
		}
	}
}
