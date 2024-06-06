// ROGUES
// SERVER
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if SERVER
// custom
public class DinoTargetedKnockbackEffect : StandardActorEffect
{
    private readonly bool m_doHitsAroundKnockbackDest;
    private readonly AbilityAreaShape m_shape;
    private readonly OnHitAuthoredData m_hitData;
    private readonly GameObject m_sequencePrefab;
    
    public DinoTargetedKnockbackEffect(
        EffectSource parent,
        BoardSquare targetSquare,
        ActorData target,
        ActorData caster,
        bool doHitsAroundKnockbackDest,
        AbilityAreaShape shape,
        OnHitAuthoredData hitData,
        GameObject sequencePrefab)
        : base(parent, targetSquare, target, caster, new StandardActorEffectData())
    {
        m_doHitsAroundKnockbackDest = doHitsAroundKnockbackDest;
        m_shape = shape;
        m_hitData = hitData;
        m_sequencePrefab = sequencePrefab;
        m_time.duration = 1;
    }

    public override void GatherMovementResults(MovementCollection movement, ref List<MovementResults> movementResultsList)
    {
        base.GatherMovementResults(movement, ref movementResultsList);

        MovementInstance movementInstance = movement.m_movementInstances.FirstOrDefault(mi => mi.m_mover == Target);
        if (movement.m_movementStage != MovementStage.Knockback
            || movementInstance == null
            || movementInstance.m_path == null)
        {
            return;
        }

        BoardSquarePathInfo pathEndpoint = movementInstance.m_path.GetPathEndpoint();
        BoardSquare targetSquare = pathEndpoint.square;
        ActorData mover = movementInstance.m_mover;
        ServerAbilityUtils.TriggeringPathInfo triggeringPathInfo =
            new ServerAbilityUtils.TriggeringPathInfo(mover, pathEndpoint);
        
        
        MovementResults movementResults = new MovementResults(movement.m_movementStage);
        movementResults.SetupTriggerData(triggeringPathInfo);
        movementResults.SetupGameplayData(this, null);
        movementResults.SetupSequenceData(m_sequencePrefab, targetSquare, SequenceSource);
        movementResultsList.Add(movementResults);

        if (m_doHitsAroundKnockbackDest)
        {
            List<ActorData> actorsInShape = AreaEffectUtils.GetActorsInShape(
                m_shape,
                targetSquare.ToVector3(),
                targetSquare,
                true,
                Caster,
                Caster.GetOtherTeams(),
                null);
            
            foreach (ActorData hitActor in actorsInShape)
            {
                if (hitActor == mover)
                {
                    continue;
                }
            
                ActorHitParameters hitParams = new ActorHitParameters(mover, targetSquare.ToVector3());
                ActorHitResults actorHitResults = new ActorHitResults(hitParams);
                GenericAbility_Container.ApplyActorHitData(Caster, mover, actorHitResults, m_hitData);
                movementResults.GetEffectResults().StoreActorHit(actorHitResults);
            }
        }
    }
}
#endif