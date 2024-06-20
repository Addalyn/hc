// ROGUES
// SERVER
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Passive_Scamp : Passive
{
	[Separator("Suit Shield Effect Data")]
	public StandardActorEffectData m_shieldEffectData;
	[Separator("Whether to zero out energy when shield is depleted")]
	public bool m_clearEnergyOnSuitRemoval;
	[Separator("Energy Orbs")]
	public int m_orbDuration = 4;
	[Space(5f)]
	public int m_orbNumToSpawn = 5;
	public float m_orbMinSpawnDist = 1f;
	public float m_orbMaxSpawnDist = 10f;
	public int m_orbEnergyGainOnTrigger = 15;
	public StandardEffectInfo m_orbTriggerEffect;
	[Header("-- Whether to clear orbs on death")]
	public bool m_clearOrbsOnDeath = true;
	[Separator("Reset Energy on Respawn?")]
	public bool m_resetEnergyOnRespawn;
	[Separator("Approximate duration of orb spawn animation")]
	public float m_orbSpawnAnimDuration = 3f; // TODO SCAMP ?
	[Separator("Sequences for Energy Orb")]
	public GameObject m_orbSpawnCasterSeqPrefab;
	[Header("-- Optional sequence to show projectile towards each spawned orb")]
	public GameObject m_orbSpawnProjectileSeqPrefab;
	[Space(10f)]
	public GameObject m_orbPersistentSeqPrefab;
	public GameObject m_orbTriggerSeqPrefab;
	
	
#if SERVER
	// custom
	private ScampSuitToggle m_ultimateAbility;
	private AbilityData.ActionType m_ultimateAbilityActionType;
	private ScampAoeTether m_tetherAbility;
	private AbilityData.ActionType m_tetherAbilityActionType;
	private Scamp_SyncComponent m_syncComp;
	private int m_pendingCdrOnTether;

	private static readonly List<Int2> s_orbLocations = new List<Int2>
	{
		new Int2(3, 3),
		new Int2(-3, 3),
		new Int2(3, -3),
		new Int2(-3, -3),
		new Int2(4, 0),
		new Int2(-4, 0),
		new Int2(0, 4),
		new Int2(0, -4),
		new Int2(4, 1),
		new Int2(-4, 1),
		new Int2(4, -1),
		new Int2(-4, -1),
		new Int2(1, 4),
		new Int2(1, -4),
		new Int2(-1, 4),
		new Int2(-1, -4),
	};
#endif

	public int GetMaxSuitShield()
	{
		return m_shieldEffectData.m_absorbAmount;
	}
	
#if SERVER
	// custom
	private int NumOrbsToSpawn => m_orbNumToSpawn + m_ultimateAbility.GetExtraOrbsToSpawnOnSuitLost();
	
	// custom
	protected override void OnStartup()
	{
		base.OnStartup();

		AbilityData abilityData = Owner.GetAbilityData();
		m_ultimateAbility = abilityData.GetAbilityOfType(typeof(ScampSuitToggle)) as ScampSuitToggle;
		m_ultimateAbilityActionType = abilityData.GetActionTypeOfAbility(m_ultimateAbility);
		m_tetherAbility = abilityData.GetAbilityOfType(typeof(ScampAoeTether)) as ScampAoeTether;
		m_tetherAbilityActionType = abilityData.GetActionTypeOfAbility(m_tetherAbility);
		m_syncComp = Owner.GetComponent<Scamp_SyncComponent>();
	}
	
	// custom
	public override void OnTurnStart()
	{
		base.OnTurnStart();

		int currentTurn = GameFlowData.Get().CurrentTurn;
		if (currentTurn == 1)
		{
			CreateShield();
		}

		if (m_clearOrbsOnDeath && currentTurn == Owner.LastDeathTurn + 1)
		{
			DestroyOrbs();
		}
		
		m_syncComp.Networkm_suitWasActiveOnTurnStart = m_syncComp.m_suitActive;
		m_syncComp.Networkm_suitShieldingOnTurnStart = (uint)GetCurrentAbsorb();
	}

	// custom
	public override void OnActorRespawn()
	{
		base.OnActorRespawn();
		CreateShield();

		if (m_resetEnergyOnRespawn)
		{
			Owner.SetTechPoints(0);
		}
	}

	// custom
	public override void OnTurnEnd()
	{
		base.OnTurnEnd();
		CheckShield();

		if (m_pendingCdrOnTether > 0)
		{
			ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(Owner, Owner.GetFreePos()));
			actorHitResults.AddMiscHitEvent(new MiscHitEventData_AddToCasterCooldown(
				m_tetherAbilityActionType,
				-m_pendingCdrOnTether));
			MovementResults.SetupAndExecuteAbilityResultsOutsideResolution(
				Owner,
				Owner,
				actorHitResults,
				m_tetherAbility);
			m_pendingCdrOnTether = 0;
		}
	}

	// custom
	public override void OnAbilityPhaseEnd(AbilityPriority phase)
	{
		base.OnAbilityPhaseEnd(phase);

		if (phase == AbilityPriority.Combat_Final)
		{
			CheckShield();
		}
	}

	// custom
	private void CheckShield()
	{
		List<StandardActorEffect> effects = GetShieldEffects();
		if (m_syncComp.m_suitActive && !effects.Any(e => e.CanAbsorb()))
		{
			DestroyShield(effects);
			m_syncComp.Networkm_suitActive = false;
			m_syncComp.Networkm_lastSuitLostTurn = (uint)GameFlowData.Get().CurrentTurn;

			if (m_clearEnergyOnSuitRemoval)
			{
				Owner.SetTechPoints(0);
			}
		}
	}

	// custom
	public List<ScampOrbEffect> GetOrbs()
	{
		return ServerEffectManager.Get()
			.GetWorldEffectsByCaster(Owner, typeof(ScampOrbEffect))
			.Select(e => e as ScampOrbEffect)
			.ToList();
	}

	// custom
	private void CreateShield()
	{
		ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(Owner, Owner.GetFreePos()));
		actorHitResults.AddStandardEffectInfo(new StandardEffectInfo
		{
			m_applyEffect = true,
			m_effectData = m_shieldEffectData
		});
		MovementResults.SetupAndExecuteAbilityResultsOutsideResolution(
			Owner,
			Owner,
			actorHitResults,
			m_ultimateAbility);
	}
	
	// custom
	private void DestroyShield(List<StandardActorEffect> effects)
	{
		ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(Owner, Owner.GetFreePos()));
		foreach (StandardActorEffect effect in effects)
		{
			actorHitResults.AddEffectForRemoval(effect);
		}
		actorHitResults.AddMiscHitEvent(
			new MiscHitEventData_OverrideCooldown(
				m_ultimateAbilityActionType,
				m_ultimateAbility.GetCooldownOverrideOnSuitDestroy()));
		actorHitResults.AddStandardEffectInfo(m_ultimateAbility.GetEffectForSuitLost());
		
		SequenceSource seqSource = new SequenceSource(null, null);
		MovementResults movementResults = new MovementResults(MovementStage.INVALID);
		movementResults.SetupForHitOutsideResolution(
			Owner,
			Owner,
			actorHitResults,
			m_ultimateAbility,
			null,
			Owner.GetCurrentBoardSquare(),
			seqSource,
			true);
		
		movementResults.AddSequenceStartOverride(
			new ServerClientUtils.SequenceStartData(
				m_orbSpawnCasterSeqPrefab,
				Owner.GetCurrentBoardSquare(),
				Owner.AsArray(),
				Owner,
				seqSource),
			seqSource);

		SequenceSource permSeqSource = seqSource.GetShallowCopy();
		permSeqSource.RemoveAtEndOfTurn = false;
		foreach (BoardSquare boardSquare in GetSquaresToSpawnOrbsOn())
		{
			PositionHitResults positionHitResults = new PositionHitResults(new PositionHitParameters(boardSquare.ToVector3()));
			positionHitResults.AddEffect(
				new ScampOrbEffect(
					m_ultimateAbility.AsEffectSource(),
					boardSquare,
					Owner,
					m_orbDuration,
					m_orbEnergyGainOnTrigger,
					m_orbTriggerEffect,
					m_orbPersistentSeqPrefab,
					m_orbTriggerSeqPrefab));
			
			movementResults.AddSequenceStartOverride(
				new ServerClientUtils.SequenceStartData(
					m_orbSpawnProjectileSeqPrefab,
					boardSquare.ToVector3(),
					Owner.AsArray(),
					Owner,
					permSeqSource),
				permSeqSource);
			movementResults.GetPowerUpResults().StorePositionHit(positionHitResults);
		}
		movementResults.ExecuteUnexecutedMovementHits(false);
		if (ServerResolutionManager.Get() != null)
		{
			ServerResolutionManager.Get().SendNonResolutionActionToClients(movementResults);
		}
	}
	
	// custom
	private void DestroyOrbs()
	{
		List<ScampOrbEffect> orbEffects = GetOrbs();
		if (orbEffects.Count == 0)
		{
			return;
		}

		ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(Owner, Owner.GetFreePos()));
		foreach (ScampOrbEffect orbEffect in orbEffects)
		{
			actorHitResults.AddEffectForRemoval(orbEffect);
		}
		
		SequenceSource seqSource = new SequenceSource(null, null);
		MovementResults movementResults = new MovementResults(MovementStage.INVALID);
		movementResults.SetupForHitOutsideResolution(
			Owner,
			Owner,
			actorHitResults,
			m_ultimateAbility,
			null,
			Owner.GetCurrentBoardSquare(),
			seqSource,
			true);

		foreach (ScampOrbEffect orbEffect in orbEffects)
		{
			movementResults.AddSequenceStartOverride(orbEffect.GetEffectEndSeqData(seqSource), seqSource);
		}
		movementResults.ExecuteUnexecutedMovementHits(false);
		if (ServerResolutionManager.Get() != null)
		{
			ServerResolutionManager.Get().SendNonResolutionActionToClients(movementResults);
		}
	}

	
	// custom
	private List<BoardSquare> GetSquaresToSpawnOrbsOn()
	{
		int numOrbsToSpawn = NumOrbsToSpawn;
		List<BoardSquare> res = new List<BoardSquare>();
		BoardSquare centerSquare = Owner.GetCurrentBoardSquare();
		foreach (Int2 delta in s_orbLocations)
		{
			int x = centerSquare.x + delta.x;
			int y = centerSquare.y + delta.y;
			BoardSquare orbSquare = Board.Get().GetSquareFromIndex(x, y);

			if (orbSquare != null
			    && orbSquare.IsValidForGameplay()
			    && centerSquare.GetLOS(x, y))
			{
				res.Add(orbSquare);

				if (res.Count == numOrbsToSpawn)
				{
					return res;
				}
			}
		}

		IEnumerable<BoardSquare> squaresInRadius = AreaEffectUtils
			.GetSquaresInRadius(centerSquare, m_orbMaxSpawnDist, false, Owner)
			.Where(
				s =>
				{
					float dist = centerSquare.HorizontalDistanceInSquaresTo(s);
					return dist >= m_orbMinSpawnDist
					       && dist <= m_orbMaxSpawnDist
					       && centerSquare.GetLOS(s.x, s.y)
					       && !res.Contains(s);
				})
			.OrderBy(s => centerSquare.HorizontalDistanceInSquaresTo(s));

		int missing = numOrbsToSpawn - res.Count;
		res.AddRange(squaresInRadius.Take(missing)); // theoretically can be not enough but what can we do at this point
		return res;
	}

	// custom
	private List<StandardActorEffect> GetShieldEffects()
	{
		return ServerEffectManager
			.Get()
			.GetEffectsOnTargetByCaster(Owner, Owner, typeof(StandardActorEffect))
			.Where(e => e.Parent.Ability == m_ultimateAbility)
			.Select(e => e as StandardActorEffect)
			.ToList();
	}
	
	// custom
	public int GetCurrentAbsorb()
	{
		return GetShieldEffects().Select(e => e.Absorbtion.m_absorbRemaining).Sum();
	}
	
	// custom
	public void SetPendingCdrOnTether(int cdr)
	{
		m_pendingCdrOnTether = cdr;
	}
	
	// custom
	public void OnTetherBroken()
	{
		m_pendingCdrOnTether = 0;
	}
#endif
}
