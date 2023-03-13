using System.Collections.Generic;
using UnityEngine;

#if SERVER
// added in rogues
public class ClericHealReactionEffect : StandardActorEffect
{
	private StandardEffectInfo m_reactionEffectOnDamager;

	private GameObject m_reactionSequencePrefab;

	private List<ActorData> m_actorsReactedToThisTurn = new List<ActorData>();

	private List<ActorData> m_actorsReactedToThisTurnFake = new List<ActorData>();

	public ClericHealReactionEffect(EffectSource parent, ActorData target, ActorData caster, StandardActorEffectData data, StandardEffectInfo reactionEffectOnDamager, GameObject reactionSequence) : base(parent, null, target, caster, data)
	{
		m_reactionEffectOnDamager = reactionEffectOnDamager;
		m_reactionSequencePrefab = reactionSequence;
	}

	public override void OnTurnStart()
	{
		base.OnTurnStart();
		m_actorsReactedToThisTurn.Clear();
		m_actorsReactedToThisTurnFake.Clear();
	}

	public override void OnAbilityPhaseStart(AbilityPriority phase)
	{
		base.OnAbilityPhaseStart(phase);
		if (phase == AbilityPriority.Prep_Defense)
		{
			m_actorsReactedToThisTurn.Clear();
			m_actorsReactedToThisTurnFake.Clear();
		}
	}

	public override void GatherResultsInResponseToActorHit(ActorHitResults incomingHit, ref List<AbilityResults_Reaction> reactions, bool isReal)
	{
		bool flag = false;
		if (incomingHit.HasDamage && incomingHit.CanBeReactedTo)
		{
			if (isReal)
			{
				if (!m_actorsReactedToThisTurn.Contains(incomingHit.m_hitParameters.Caster))
				{
					flag = true;
					m_actorsReactedToThisTurn.Add(incomingHit.m_hitParameters.Caster);
				}
			}
			else if (!m_actorsReactedToThisTurnFake.Contains(incomingHit.m_hitParameters.Caster))
			{
				flag = true;
				m_actorsReactedToThisTurnFake.Add(incomingHit.m_hitParameters.Caster);
			}
		}
		if (flag)
		{
			ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(incomingHit.m_hitParameters.Caster, base.Target.GetFreePos()));
			actorHitResults.AddStandardEffectInfo(m_reactionEffectOnDamager);
			SplineProjectileSequence.DelayedProjectileExtraParams delayedProjectileExtraParams = new SplineProjectileSequence.DelayedProjectileExtraParams();
			delayedProjectileExtraParams.overrideStartPos = base.Target.GetLoSCheckPos();
			delayedProjectileExtraParams.useOverrideStartPos = true;
			AbilityResults_Reaction abilityResults_Reaction = new AbilityResults_Reaction(this, actorHitResults, m_reactionSequencePrefab, incomingHit.m_hitParameters.Caster.GetCurrentBoardSquare(), base.SequenceSource, incomingHit.m_reactionDepth, isReal, incomingHit, delayedProjectileExtraParams.ToArray());
			abilityResults_Reaction.SetExtraFlag(ClientReactionResults.ExtraFlags.TriggerOnFirstDamageIfReactOnAttacker);
			reactions.Add(abilityResults_Reaction);
		}
	}
}
#endif