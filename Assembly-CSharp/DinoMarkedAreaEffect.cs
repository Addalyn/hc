// ROGUES
// SERVER
using System.Collections.Generic;
using System.Linq;
using AbilityContextNamespace;
using UnityEngine;

#if SERVER
// custom
public class DinoMarkedAreaEffect : Effect
{
    private readonly List<ActorData> m_targets;
    private readonly int m_delayTurns;
    private readonly AbilityAreaShape m_shape;
    private readonly bool m_delayedHitIgnoreLos;
    private readonly int m_extraDamage;
    private readonly int m_energyToAllyOnDamageHit;
    private readonly OnHitAuthoredData m_delayedOnHitData;
    private readonly GameObject m_markerSeqPrefab;
    private readonly GameObject m_triggerSeqPrefab;

    private List<BoardSquare> m_targetSquares;

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
        GameObject markerSeqPrefab,
        GameObject triggerSeqPrefab)
        : base(parent, targetSquare, null, caster)
    {
        m_targets = targets;
        m_delayTurns = delayTurns;
        m_shape = shape;
        m_delayedHitIgnoreLos = delayedHitIgnoreLos;
        m_extraDamage = extraDamage;
        m_energyToAllyOnDamageHit = energyToAllyOnDamageHit;
        m_delayedOnHitData = delayedOnHitData;
        m_markerSeqPrefab = markerSeqPrefab;
        m_triggerSeqPrefab = triggerSeqPrefab;

        HitPhase = AbilityPriority.Combat_Damage;
        m_time.duration = m_delayTurns + 1;
        UpdateTargetSquares();
    }

    public override bool AddActorAnimEntryIfHasHits(AbilityPriority phaseIndex)
    {
        return m_time.age >= m_delayTurns && HitPhase == phaseIndex;
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        UpdateTargetSquares();

        if (m_targetSquares.Count > 0)
        {
            MovementResults movementResults = new MovementResults(MovementStage.INVALID);
            movementResults.SetupTriggerData(Caster, null);
            movementResults.SetupGameplayDataForAbility(Parent.Ability, Caster);
            movementResults.SetupSequenceData(
                SequenceLookup.Get().GetSimpleHitSequencePrefab(),
                Caster.GetCurrentBoardSquare(),
                SequenceSource);
            movementResults.AddSequenceStartOverride(
                new ServerClientUtils.SequenceStartData(
                    SequenceLookup.Get().GetSimpleHitSequencePrefab(),
                    new Vector3(1.0f, 1.0f, 1.0f),
                    null,
                    Caster,
                    SequenceSource),
                SequenceSource);
            foreach (BoardSquare targetSquare in m_targetSquares)
            {
                movementResults.AddSequenceStartOverride(
                    new ServerClientUtils.SequenceStartData(
                        m_markerSeqPrefab,
                        targetSquare,
                        targetSquare.OccupantActor?.AsArray(),
                        Caster,
                        SequenceSource),
                    SequenceSource,
                    false);
            }

            movementResults.ExecuteUnexecutedMovementHits(false);
            if (ServerResolutionManager.Get() != null)
            {
                ServerResolutionManager.Get().SendNonResolutionActionToClients(movementResults);
            }
        }
    }

    public override List<ServerClientUtils.SequenceStartData> GetEffectHitSeqDataList()
    {
        List<ServerClientUtils.SequenceStartData> list = base.GetEffectHitSeqDataList();
        SequenceSource source = SequenceSource.GetShallowCopy();
        source.SetWaitForClientEnable(true);
        foreach (BoardSquare targetSquare in m_targetSquares)
        {
            list.Add(
                new ServerClientUtils.SequenceStartData(
                    m_triggerSeqPrefab,
                    targetSquare,
                    GetHitActorsInShape(targetSquare).ToArray(),
                    Caster,
                    source));
        }

        return list;
    }

    private void UpdateTargetSquares()
    {
        m_targetSquares = m_targets.Select(
                actor => actor.IsDead()
                    ? actor.GetMostRecentDeathSquare()
                    : actor.GetCurrentBoardSquare())
            .ToList();
    }

    public override void GatherEffectResults(ref EffectResults effectResults, bool isReal)
    {
        if (m_time.age < m_delayTurns)
        {
            return;
        }

        HashSet<ActorData> alreadyHitActors = new HashSet<ActorData>();
        foreach (BoardSquare targetSquare in m_targetSquares)
        {
            List<ActorData> actorsInShape = GetHitActorsInShape(targetSquare);
            foreach (ActorData actorData in actorsInShape)
            {
                if (!alreadyHitActors.Add(actorData))
                {
                    continue;
                }

                // TODO DINO check energy gain on hit
                // TODO DINO test ignoreCover
                ActorHitResults actorHitResults =
                    new ActorHitResults(
                        new ActorHitParameters(
                            actorData,
                            targetSquare
                                .ToVector3())); // we ignore cover, so it doesn't really matter which one we pick
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

            PositionHitResults posHitResult =
                new PositionHitResults(new PositionHitParameters(targetSquare.ToVector3()));
            posHitResult.AddSequenceToEnd(m_markerSeqPrefab, SequenceSource, targetSquare.ToVector3());
            effectResults.StorePositionHit(posHitResult);
        }
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

        ActorHitParameters hitParameters = new ActorHitParameters(
            incomingHit.m_hitParameters.Caster,
            Target.GetFreePos());
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
        abilityResults_Reaction.SetupSequenceData(
            SequenceLookup.Get().GetSimpleHitSequencePrefab(),
            Target.GetCurrentBoardSquare(),
            SequenceSource);
        abilityResults_Reaction.SetSequenceCaster(Target);
        abilityResults_Reaction.SetExtraFlag(ClientReactionResults.ExtraFlags.ClientExecuteOnFirstDamagingHit);
        reactions.Add(abilityResults_Reaction);
    }

    public override List<Vector3> CalcPointsOfInterestForCamera()
    {
        return m_targetSquares.Select(s => s.ToVector3()).ToList();
    }

    private List<ActorData> GetHitActorsInShape(BoardSquare targetSquare)
    {
        List<ActorData> actorsInShape = AreaEffectUtils.GetActorsInShape(
            m_shape,
            targetSquare.ToVector3(),
            targetSquare,
            m_delayedHitIgnoreLos,
            Caster,
            Caster.GetOtherTeams(),
            null);
        return actorsInShape;
    }
}
#endif