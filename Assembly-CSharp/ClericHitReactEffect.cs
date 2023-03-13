using System;
using System.Collections.Generic;

#if SERVER
// added in rogues
public class ClericHitReactEffect : Effect
{
	private int m_energyOnCasterPerHit;

	public ClericHitReactEffect(EffectSource parent, ActorData caster, int energyOnCasterPerIncomingHit) : base(parent, null, caster, caster)
	{
		m_energyOnCasterPerHit = energyOnCasterPerIncomingHit;
		m_time.duration = 1;
	}

	public override void GatherResultsInResponseToActorHit(ActorHitResults incomingHit, ref List<AbilityResults_Reaction> reactions, bool isReal)
	{
		if (incomingHit.HasDamage)
		{
			ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(base.Caster, base.Caster.GetFreePos()));
			actorHitResults.AddTechPointGain(this.m_energyOnCasterPerHit);
			AbilityResults_Reaction item = new AbilityResults_Reaction(this, actorHitResults, SequenceLookup.Get().GetSimpleHitSequencePrefab(), base.Caster.GetCurrentBoardSquare(), base.SequenceSource, incomingHit.m_reactionDepth, isReal, incomingHit, null);
			reactions.Add(item);
		}
	}
}
#endif