using System.Collections.Generic;
using UnityEngine;

public class Passive_Valkyrie : Passive
{
	private Valkyrie_SyncComponent m_syncComp;
	private ValkyrieGuard m_guardAbility;
	private ValkyrieDashAoE m_dashAbility;
	private ValkyriePullToLaserCenter m_ultAbility;
	private bool m_tookDamageThisTurn;
	private bool m_guardIsUp;
	private int m_lastUltCastTurn = -1;
	private int m_lastGuardCastTurn = -1;

	public int DamageThroughGuardCoverThisTurn
	{
		get;
		private set;
	}

#if SERVER
	//Added in rouges
	protected override void OnStartup()
	{
		base.OnStartup();
		m_syncComp = base.Owner.GetComponent<Valkyrie_SyncComponent>();
		AbilityData component = base.Owner.GetComponent<AbilityData>();
		if (component != null)
		{
			m_guardAbility = (component.GetAbilityOfType(typeof(ValkyrieGuard)) as ValkyrieGuard);
			m_dashAbility = (component.GetAbilityOfType(typeof(ValkyrieDashAoE)) as ValkyrieDashAoE);
			m_ultAbility = (component.GetAbilityOfType(typeof(ValkyriePullToLaserCenter)) as ValkyriePullToLaserCenter);
		}
		base.Owner.OnKnockbackHitExecutedDelegate += OnKnockbackMovementHitExecuted;
	}

	//Added in rouges
	private void OnDestroy()
	{
		base.Owner.OnKnockbackHitExecutedDelegate -= OnKnockbackMovementHitExecuted;
	}

	//Added in rouges
	public override void OnDamaged(ActorData damageCaster, DamageSource damageSource, int damageAmount)
	{
		AbilityData abilityData = base.Owner.GetAbilityData();
		if (abilityData != null && damageAmount > 0)
		{
			if (IsCoverGuardActive(abilityData))
			{
				bool flag = false;
				if (IsDamageCoveredByGuard(damageSource, ref flag))
				{
					DamageThroughGuardCoverThisTurn += damageAmount;
					if (m_syncComp != null && m_guardAbility != null)
					{
						Valkyrie_SyncComponent syncComp = m_syncComp;
						syncComp.Networkm_extraDamageNextShieldThrow = syncComp.m_extraDamageNextShieldThrow + m_guardAbility.GetExtraDamageNextShieldThrowPerCoveredHit();
						m_syncComp.Networkm_extraDamageNextShieldThrow = Mathf.Min(m_syncComp.m_extraDamageNextShieldThrow, m_guardAbility.GetMaxExtraDamageNextShieldThrow());
					}
				}
			}
			if (abilityData.HasQueuedAbilityOfType(typeof(ValkyrieDashAoE)) && !m_tookDamageThisTurn && m_dashAbility != null && m_dashAbility.GetCooldownReductionOnHitAmount() != 0)
			{
				ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(base.Owner, base.Owner.GetFreePos()));
				actorHitResults.AddMiscHitEvent(new MiscHitEventData_AddToCasterCooldown(m_dashAbility.m_cooldownReductionIfDamagedThisTurn.abilitySlot, m_dashAbility.GetCooldownReductionOnHitAmount()));
				MovementResults.SetupAndExecuteAbilityResultsOutsideResolution(base.Owner, base.Owner, actorHitResults, m_dashAbility, true, null, null);
			}
			m_tookDamageThisTurn = true;
		}
	}

	//Added in rouges
	public bool IsCoverGuardActive(AbilityData abilityData)
	{
		return abilityData.HasQueuedAbilityOfType(typeof(ValkyrieGuard)) || (m_guardAbility != null && m_guardAbility.CoverLastsForever() && m_guardIsUp);
	}

	//Added in rouges
	public bool IsDamageCoveredByGuard(DamageSource damageSource, ref bool tooNearForCover)
	{
		ActorCover.CoverDirections coverDirection = m_syncComp.m_coverDirection;
		tooNearForCover = (GameplayData.Get().m_coverMinDistance * Board.Get().squareSize > (damageSource.DamageSourceLocation - base.Owner.GetFreePos()).magnitude);
		float num = VectorUtils.HorizontalAngle_Deg(damageSource.DamageSourceLocation - base.Owner.GetFreePos());
		float num2 = VectorUtils.HorizontalAngle_Deg(base.Owner.GetActorCover().GetCoverOffset(coverDirection));
		return Mathf.Abs(num - num2) <= GameplayData.Get().m_coverProtectionAngle * 0.5f;
	}

	//Added in rouges
	public override void OnDied(List<UnresolvedHealthChange> killers)
	{
		base.OnDied(killers);
		m_guardIsUp = false;
	}

	//Added in rouges
	public override void OnTurnStart()
	{
		base.OnTurnStart();
		m_tookDamageThisTurn = false;
	}

	//Added in rouges
	public override void OnTurnEnd()
	{
		base.OnTurnEnd();
		if (DamageThroughGuardCoverThisTurn == 0 && m_lastGuardCastTurn == GameFlowData.Get().CurrentTurn && m_guardAbility != null)
		{
			AbilityModCooldownReduction cooldownReductionOnNoBlock = m_guardAbility.GetCooldownReductionOnNoBlock();
			if (cooldownReductionOnNoBlock != null && cooldownReductionOnNoBlock.HasCooldownReduction())
			{
				ActorHitResults hitRes = new ActorHitResults(new ActorHitParameters(base.Owner, base.Owner.GetFreePos()));
				cooldownReductionOnNoBlock.AppendCooldownMiscEvents(hitRes, true, 0, 0);
				MovementResults.SetupAndExecuteAbilityResultsOutsideResolution(base.Owner, base.Owner, hitRes, m_guardAbility, true, null, null);
			}
		}
		DamageThroughGuardCoverThisTurn = 0;
		if (m_lastUltCastTurn != GameFlowData.Get().CurrentTurn)
		{
			m_syncComp.Networkm_skipDamageReductionForNextStab = false;
		}
	}

	//Added in rouges
	public override void OnAbilityCastResolved(Ability ability)
	{
		base.OnAbilityCastResolved(ability);
		if (ability is ValkyrieGuard || ability is ValkyrieDashAoE)
		{
			m_guardIsUp = true;
			if (ability is ValkyrieGuard)
			{
				m_lastGuardCastTurn = GameFlowData.Get().CurrentTurn;
				return;
			}
		}
		else if (ability is ValkyriePullToLaserCenter && m_ultAbility != null)
		{
			m_lastUltCastTurn = GameFlowData.Get().CurrentTurn;
			m_syncComp.Networkm_skipDamageReductionForNextStab = m_ultAbility.ShouldSkipDamageReductionOnNextTurnStab();
		}
	}

	//Added in rouges
	public override void OnMovementResultsGathered(MovementCollection stabilizedMovements)
	{
		if (ServerEffectManager.Get().HasEffectByCaster(base.Owner, base.Owner, typeof(ValkyrieGuardEndingEffect)))
		{
			int statIndex = 0;
			int serverIncomingDamageReducedByCoverThisTurn = base.Owner.GetActorBehavior().serverIncomingDamageReducedByCoverThisTurn;
			base.Owner.GetFreelancerStats().AddToValueOfStat(statIndex, serverIncomingDamageReducedByCoverThisTurn);
		}
	}

	//Added in rouges
	private void OnKnockbackMovementHitExecuted(ActorData target, ActorHitResults hitRes)
	{
		if (hitRes.HasDamage && ServerActionBuffer.Get().HasStoredAbilityRequestOfType(base.Owner, typeof(ValkyrieThrowShield)))
		{
			int statIndex = 1;
			base.Owner.GetFreelancerStats().AddToValueOfStat(statIndex, hitRes.FinalDamage);
		}
	}
#endif
}
