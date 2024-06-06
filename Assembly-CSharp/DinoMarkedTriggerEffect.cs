// ROGUES
// SERVER
using System.Collections.Generic;
using System.Linq;
using AbilityContextNamespace;
using UnityEngine;

#if SERVER
// custom
public class DinoMarkedTriggerEffect : Effect
{
    private readonly List<BoardSquare> m_targetSquares;
    private readonly AbilityAreaShape m_shape;
    private readonly bool m_delayedHitIgnoreLos;
    private readonly int m_extraDamage;
    private readonly OnHitAuthoredData m_delayedOnHitData;
    private readonly GameObject m_markerSeqPrefab;
    private readonly GameObject m_triggerSeqPrefab;

    public DinoMarkedTriggerEffect(
        EffectSource parent,
        List<BoardSquare> targetSquares,
        ActorData caster,
        AbilityAreaShape shape,
        bool delayedHitIgnoreLos,
        int extraDamage,
        OnHitAuthoredData delayedOnHitData,
        GameObject markerSeqPrefab,
        GameObject triggerSeqPrefab)
        : base(parent, targetSquares[0], null, caster)
    {
        m_targetSquares = targetSquares;
        m_shape = shape;
        m_delayedHitIgnoreLos = delayedHitIgnoreLos;
        m_extraDamage = extraDamage;
        m_delayedOnHitData = delayedOnHitData;
        m_markerSeqPrefab = markerSeqPrefab;
        m_triggerSeqPrefab = triggerSeqPrefab;
        
        HitPhase = AbilityPriority.Combat_Damage;
        m_time.duration = 1;
    }

    public override List<ServerClientUtils.SequenceStartData> GetEffectStartSeqDataList()
    {
        return m_targetSquares.Select(
                targetSquare => new ServerClientUtils.SequenceStartData(
                    m_markerSeqPrefab,
                    targetSquare.ToVector3(),
                    null,
                    Caster,
                    SequenceSource))
            .ToList();
    }

    public override List<ServerClientUtils.SequenceStartData> GetEffectHitSeqDataList()
    {
        List<ServerClientUtils.SequenceStartData> list = base.GetEffectHitSeqDataList();
        foreach (BoardSquare targetSquare in m_targetSquares)
        {
            list.Add(
                new ServerClientUtils.SequenceStartData(
                    m_triggerSeqPrefab,
                    targetSquare.ToVector3(),
                    m_effectResults.HitActorsArray(), // TODO DINO check
                    Caster,
                    SequenceSource));
        }
        return list;
    }
    
    // private static HashSet<BoardSquare> GetSquaresInShape(
    //     List<BoardSquare> targetSquares,
    //     AbilityAreaShape shape,
    //     bool ignoreLos,
    //     ActorData caster)
    // {
    //     HashSet<BoardSquare> result = new HashSet<BoardSquare>();
    //     foreach (BoardSquare targetSquare in targetSquares)
    //     {
    //         List<BoardSquare> squaresInShape = AreaEffectUtils.GetSquaresInShape(
    //             shape,
    //             targetSquare.ToVector3(),
    //             targetSquare,
    //             ignoreLos,
    //             caster);
    //         result.UnionWith(squaresInShape);
    //     }
    //
    //     return result;
    // }

    public override void GatherEffectResults(ref EffectResults effectResults, bool isReal)
    {
        base.GatherEffectResults(ref effectResults, isReal);

        HashSet<ActorData> alreadyHitActors = new HashSet<ActorData>();
        foreach (BoardSquare targetSquare in m_targetSquares)
        {
            List<ActorData> actorsInShape = AreaEffectUtils.GetActorsInShape(
                m_shape,
                targetSquare.ToVector3(),
                targetSquare,
                m_delayedHitIgnoreLos,
                Caster,
                Caster.GetOtherTeams(),
                null);
            foreach (ActorData actorData in actorsInShape)
            {
                if (!alreadyHitActors.Add(actorData))
                {
                    continue;
                }
                
                // TODO DINO check energy gain on hit
                // TODO DINO test ignoreCover
                ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(actorData, targetSquare.ToVector3())); // we ignore cover, so it doesn't really matter which one we pick
                ActorHitContext actorHitContext = new ActorHitContext();
                actorHitContext.m_contextVars.SetValue(
                    DinoMarkedAreaAttack.s_cvarInCenter.GetKey(),
                    actorData.GetCurrentBoardSquare() == targetSquare ? 1 : 0); // TODO DINO check damage not in center
                GenericAbility_Container.ApplyActorHitData(
                    Caster,
                    actorData,
                    actorHitResults,
                    m_delayedOnHitData,
                    actorHitContext);
                actorHitResults.AddBaseDamage(m_extraDamage);
                effectResults.StoreActorHit(actorHitResults);
            }
        }
    }
}
#endif