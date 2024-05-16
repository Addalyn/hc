using System.Collections.Generic;
using UnityEngine;

#if SERVER
// custom
public class IceborgNovaOnReactEffect : StandardActorEffect
{
    private int m_damageAmount;
    private int m_energyOnTargetPerReaction;
    private int m_energyOnCasterPerReaction;
    private int m_extraEnergyPerNovaCoreTrigger;
    private Iceborg_SyncComponent m_syncComp;
    private GameObject m_onTriggerSeqPrefab;
    private List<ActorData> m_actorsHitThisTurn;
    private List<ActorData> m_actorsHitThisTurn_fake;
    private bool m_endEarly;

    public IceborgNovaOnReactEffect(
        EffectSource parent, 
        BoardSquare targetSquare, 
        ActorData target, 
        ActorData caster, 
        StandardActorEffectData data, 
        int damageAmount, 
        GameObject onTriggerSeqPrefab,
        int energyOnTargetPerReaction,
        int energyOnCasterPerReaction,
        int extraEnergyPerNovaCoreTrigger) : base(parent, targetSquare, target, caster, data)
    {
        m_actorsHitThisTurn = new List<ActorData>();
        m_actorsHitThisTurn_fake = new List<ActorData>();
        m_damageAmount = damageAmount;
        m_syncComp = parent.Ability.GetComponent<Iceborg_SyncComponent>();
        m_onTriggerSeqPrefab = onTriggerSeqPrefab;
        m_energyOnTargetPerReaction = energyOnTargetPerReaction;
        m_energyOnCasterPerReaction = energyOnCasterPerReaction;
        m_extraEnergyPerNovaCoreTrigger = extraEnergyPerNovaCoreTrigger;
        m_endEarly = false;
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        m_actorsHitThisTurn.Clear();
        m_actorsHitThisTurn_fake.Clear();
    }

    public override void GatherResultsInResponseToActorHit(
        ActorHitResults incomingHit,
        ref List<AbilityResults_Reaction> reactions,
        bool isReal)
    {
        if (!incomingHit.HasDamage)
        {
            return;
        }

        ActorData incomingHitCaster = incomingHit.m_hitParameters.Caster;

        if (!GetWasHitThisTurn(incomingHitCaster, isReal))
        {
            SetWasHitThisTurn(incomingHitCaster, isReal);

            GetReactionsForTarget(incomingHit, isReal, ref reactions);
            GetReactionsForEnemy(incomingHit, isReal, ref reactions);
            GetReactionsForCaster(incomingHit, isReal, ref reactions);
        }
    }

    private void GetReactionsForCaster(ActorHitResults incomingHit, bool isReal, ref List<AbilityResults_Reaction> reactions)
    {
        // Give energy to caster for each hit
        if (m_energyOnCasterPerReaction > 0)
        {
            ActorHitParameters actorHitParameters = new ActorHitParameters(Caster, Caster.GetFreePos());
            ActorHitResults actorHitResults = new ActorHitResults(m_energyOnCasterPerReaction, HitActionType.TechPointsGain, actorHitParameters);
            AbilityResults_Reaction targetAbilityResultsReaction = new AbilityResults_Reaction();
            targetAbilityResultsReaction.SetupGameplayData(
                this,
                actorHitResults,
                incomingHit.m_reactionDepth,
                null,
                isReal,
                incomingHit);
            targetAbilityResultsReaction.SetupSequenceData(null, Caster.GetCurrentBoardSquare(), SequenceSource);
            reactions.Add(targetAbilityResultsReaction);
        }
    }

    private void GetReactionsForTarget(ActorHitResults incomingHit, bool isReal, ref List<AbilityResults_Reaction> reactions)
    {
        // Give energy to target if needed
        if (m_energyOnTargetPerReaction > 0)
        {
            ActorHitParameters targetActorHitParameters = new ActorHitParameters(Target, Target.GetFreePos());
            ActorHitResults targetActorHitResults = new ActorHitResults(
                m_energyOnTargetPerReaction,
                HitActionType.TechPointsGain,
                targetActorHitParameters);
            AbilityResults_Reaction targetAbilityResultsReaction = new AbilityResults_Reaction();
            targetAbilityResultsReaction.SetupGameplayData(
                this,
                targetActorHitResults,
                incomingHit.m_reactionDepth,
                null,
                isReal,
                incomingHit);
            targetAbilityResultsReaction.SetupSequenceData(
                m_onTriggerSeqPrefab,
                Target.GetCurrentBoardSquare(),
                SequenceSource);
            reactions.Add(targetAbilityResultsReaction);
        }
    }

    private void GetReactionsForEnemy(ActorHitResults incomingHit, bool isReal, ref List<AbilityResults_Reaction> reactions)
    {
        // Deal damage and give NovaCore Effect
        ActorData incomingHitCaster = incomingHit.m_hitParameters.Caster;
        AbilityResults_Reaction abilityResults_Reaction = new AbilityResults_Reaction();
        ActorHitParameters hitParameters = new ActorHitParameters(incomingHitCaster, Target.GetFreePos());
        ActorHitResults actorHitResults = new ActorHitResults(
            m_damageAmount,
            HitActionType.Damage,
            (StandardEffectInfo)null,
            hitParameters);
        actorHitResults.CanBeReactedTo = false;
        actorHitResults.AddEffect(
            m_syncComp.CreateNovaCoreEffect(
                Parent,
                incomingHitCaster.GetCurrentBoardSquare(),
                incomingHitCaster,
                Caster,
                m_extraEnergyPerNovaCoreTrigger));
        abilityResults_Reaction.SetupGameplayData(
            this,
            actorHitResults,
            incomingHit.m_reactionDepth,
            null,
            isReal,
            incomingHit);
        abilityResults_Reaction.SetupSequenceData(m_onTriggerSeqPrefab, Target.GetCurrentBoardSquare(), SequenceSource);
        abilityResults_Reaction.SetExtraFlag(ClientReactionResults.ExtraFlags.ClientExecuteOnFirstDamagingHit);
        reactions.Add(abilityResults_Reaction);
    }

    private bool GetWasHitThisTurn(ActorData actor, bool isReal)
    {
        if (isReal)
        {
            return m_actorsHitThisTurn.Contains(actor);
        }
        return m_actorsHitThisTurn_fake.Contains(actor);
    }

    private void SetWasHitThisTurn(ActorData actor, bool isReal)
    {
        if (isReal)
        {
            m_actorsHitThisTurn.Add(actor);
        }
        else
        {
            m_actorsHitThisTurn_fake.Add(actor);
        }
    }

    public override void OnAbilityAndMovementDone()
    {
        base.OnAbilityAndMovementDone();
        if(m_actorsHitThisTurn != null && m_actorsHitThisTurn.Count > 0) 
        {
            m_endEarly = true;
        }
    }

    public override bool ShouldEndEarly()
    {
        return base.ShouldEndEarly() || m_endEarly;
    }
}
#endif