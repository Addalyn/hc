// ROGUES
// SERVER
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if SERVER
// custom
public class DinoMarkedAreaEffect : StandardActorEffect
{
    private readonly List<ActorData> m_targets;
    private readonly int m_delayTurns;
    private readonly AbilityAreaShape m_shape;
    private readonly bool m_delayedHitIgnoreLos;
    private readonly int m_extraDamage;
    private readonly int m_energyToAllyOnDamageHit;
    private readonly OnHitAuthoredData m_delayedOnHitData;
    private readonly GameObject m_firstTurnMarkerSeqPrefab;
    private readonly GameObject m_markerSeqPrefab;
    private readonly GameObject m_triggerSeqPrefab;
    
    public DinoMarkedAreaEffect(
        EffectSource parent,
        BoardSquare targetSquare,
        ActorData caster,
        List<ActorData> targets,
        int delayTurns,
        AbilityAreaShape shape,
        bool delayedHitIgnoreLos,
        int extraDamage,
        int energyToAllyOnDamageHit,
        OnHitAuthoredData delayedOnHitData,
        GameObject firstTurnMarkerSeqPrefab,
        GameObject markerSeqPrefab,
        GameObject triggerSeqPrefab)
        : base(parent, targetSquare, null, caster, new StandardActorEffectData())
    {
        m_targets = targets;
        m_delayTurns = delayTurns;
        m_shape = shape;
        m_delayedHitIgnoreLos = delayedHitIgnoreLos;
        m_extraDamage = extraDamage;
        m_energyToAllyOnDamageHit = energyToAllyOnDamageHit;
        m_delayedOnHitData = delayedOnHitData;
        m_firstTurnMarkerSeqPrefab = firstTurnMarkerSeqPrefab;
        m_markerSeqPrefab = markerSeqPrefab;
        m_triggerSeqPrefab = triggerSeqPrefab;

        HitPhase = AbilityPriority.Prep_Defense;
        m_time.duration = 2;
    }

    public override List<ServerClientUtils.SequenceStartData> GetEffectStartSeqDataList()
    {
        return m_targets.Select(
                target => new ServerClientUtils.SequenceStartData(
                    m_firstTurnMarkerSeqPrefab,
                    target.GetFreePos(),
                    target.AsArray(),
                    Caster,
                    SequenceSource))
            .ToList();
    }


    public override void GatherEffectResults(ref EffectResults effectResults, bool isReal)
    {
        if (m_time.age < m_delayTurns)
        {
            return;
        }
        
        BoardSquare targetSquare = Target.IsDead()
            ? Target.GetMostRecentDeathSquare()
            : Target.GetCurrentBoardSquare();
        
        PositionHitResults positionHitResults = new PositionHitResults(new PositionHitParameters(targetSquare.ToVector3()));
        positionHitResults.AddEffect(new DinoMarkedTriggerEffect(
            Parent,
            m_targets.Select(actor => actor.GetCurrentBoardSquare()).ToList(),
            Caster,
            m_shape,
            m_delayedHitIgnoreLos,
            m_extraDamage,
            m_delayedOnHitData,
            m_markerSeqPrefab,
            m_triggerSeqPrefab));
        positionHitResults.AddEffectSequenceToEnd(m_firstTurnMarkerSeqPrefab, m_guid);
        
        // List<StandardMultiAreaGroundEffect.GroundAreaInfo> groundAreaInfos = m_targets
        //     .Select(actor => new StandardMultiAreaGroundEffect.GroundAreaInfo(
        //         actor.GetCurrentBoardSquare(),
        //         actor.GetFreePos(),
        //         m_shape))
        //     .ToList();
        // GroundEffectField groundEffectField = new GroundEffectField
        // {
        //     canIncludeCaster = false,
        //     shape = m_shape,
        //     ignoreMovementHits = true,
        //     endIfHasDoneHits = true,
        //     ignoreNonCasterAllies = true,
        //     damageAmount = 
        //         
        // };
        // // groundEffectField.penetrateLos = m_delayedHitIgnoreLos; // TODO DINO
        // positionHitResults.AddEffect(new StandardMultiAreaGroundEffect(
        //     Parent,
        //     groundAreaInfos,
        //     Caster,
        //     groundEffectField));
        effectResults.StorePositionHit(positionHitResults);
    }

    public override void GatherResultsInResponseToActorHit(
        ActorHitResults incomingHit,
        ref List<AbilityResults_Reaction> reactions,
        bool isReal)
    {
        if (!incomingHit.HasDamage || m_time.age > 0)
        {
            return;
        }
        
        ActorHitParameters hitParameters = new ActorHitParameters(incomingHit.m_hitParameters.Caster, Target.GetFreePos());
        ActorHitResults actorHitResults = new ActorHitResults(hitParameters);
        actorHitResults.TriggeringHit = incomingHit;
        actorHitResults.CanBeReactedTo = false;
        actorHitResults.AddTechPointGain(m_energyToAllyOnDamageHit);
        
        AbilityResults_Reaction abilityResults_Reaction = new AbilityResults_Reaction();
        abilityResults_Reaction.SetupGameplayData(
            this,
            new List<ActorHitResults> { actorHitResults },
            incomingHit.m_reactionDepth,
            isReal);
        // TODO DINO sequences?
        abilityResults_Reaction.SetExtraFlag(ClientReactionResults.ExtraFlags.ClientExecuteOnFirstDamagingHit);
        reactions.Add(abilityResults_Reaction);
    }
}
#endif