// ROGUES
// SERVER
using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_ChargeAoE : GenericAbility_TargetSelectBase
{
    [Separator("Targeting Properties")]
    public float m_radiusAroundStart = 2f;
    public float m_radiusAroundEnd = 2f;
    public float m_rangeFromLine = 2f;
    public bool m_trimPathOnTargetHit;
    [Separator("Sequences")]
    public GameObject m_castSequencePrefab;
    public bool m_seqUseTrimmedDestAsTargetPos;

    private int m_maxTargets;
    private TargetSelectMod_ChargeAoE m_targetSelMod;

#if SERVER
    public static ContextNameKeyPair s_cvarInStart = new ContextNameKeyPair("InStart"); // added in rogues
    public static ContextNameKeyPair s_cvarInEnd = new ContextNameKeyPair("InEnd"); // added in rogues
#endif

    public override string GetUsageForEditor()
    {
        return "Intended for single click charge abilities, with line and AoE on either end.\n"
               + GetContextUsageStr(
                   ContextKeys.s_InEndAoe.GetName(),
                   "on hit actor, 1 if in AoE near end of laser, 0 otherwise")
               + GetContextUsageStr(
                   ContextKeys.s_ChargeEndPos.GetName(),
                   "non-actor specific, charge end position",
                   false);
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add(ContextKeys.s_InEndAoe.GetName());
        names.Add(ContextKeys.s_ChargeEndPos.GetName());
    }

    public float GetRadiusAroundStart()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_radiusAroundStartMod.GetModifiedValue(m_radiusAroundStart)
            : m_radiusAroundStart;
    }

    public float GetRadiusAroundEnd()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_radiusAroundEndMod.GetModifiedValue(m_radiusAroundEnd)
            : m_radiusAroundEnd;
    }

    public float GetRangeFromLine()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_rangeFromLineMod.GetModifiedValue(m_rangeFromLine)
            : m_rangeFromLine;
    }

    public bool TrimPathOnTargetHit()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_trimPathOnTargetHitMod.GetModifiedValue(m_trimPathOnTargetHit)
            : m_trimPathOnTargetHit;
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        AbilityUtil_Targeter_ChargeAoE targeter = new AbilityUtil_Targeter_ChargeAoE(
            ability,
            GetRadiusAroundStart(),
            GetRadiusAroundEnd(),
            GetRangeFromLine(),
            m_maxTargets,
            false,
            IgnoreLos());
        targeter.SetAffectedGroups(IncludeEnemies(), IncludeAllies(), IncludeCaster());
        targeter.TrimPathOnTargetHit = TrimPathOnTargetHit();
        targeter.ForceAddTargetingActor = IncludeCaster();
        return new List<AbilityUtil_Targeter> { targeter };
    }

    public override bool HandleCustomTargetValidation(
        Ability ability,
        ActorData caster,
        AbilityTarget target,
        int targetIndex,
        List<AbilityTarget> currentTargets)
    {
        BoardSquare targetSquare = Board.Get().GetSquare(target.GridPos);
        return targetSquare != null
               && targetSquare.IsValidForGameplay()
               && targetSquare != caster.GetCurrentBoardSquare()
               && KnockbackUtils.CanBuildStraightLineChargePath(
                   caster,
                   targetSquare,
                   caster.GetCurrentBoardSquare(),
                   false,
                   out _);
    }

    public override ActorData.MovementType GetMovementType()
    {
        return ActorData.MovementType.Charge;
    }

    public static BoardSquare GetTrimOnHitDestination(
        AbilityTarget currentTarget,
        BoardSquare startSquare,
        float lineHalfWidthInSquares,
        ActorData caster,
        List<Team> relevantTeams,
        bool forServer)
    {
        BoardSquare targetSquare = Board.Get().GetSquare(currentTarget.GridPos);
        Vector3 abilityLineEndpoint = BarrierManager.Get().GetAbilityLineEndpoint(
            caster,
            startSquare.ToVector3(),
            targetSquare.ToVector3(),
            out bool collision,
            out _);
        if (collision)
        {
            targetSquare = KnockbackUtils.GetLastValidBoardSquareInLine(startSquare.ToVector3(), abilityLineEndpoint);
        }

        BoardSquarePathInfo chargePath = KnockbackUtils.BuildStraightLineChargePath(
            caster, 
            targetSquare,
            startSquare,
            false);
        TrimChargePathOnActorHit(
            chargePath,
            startSquare,
            lineHalfWidthInSquares,
            caster,
            relevantTeams,
            forServer,
            out BoardSquare destSquare);
        return destSquare;
    }

    public static void TrimChargePathOnActorHit(
        BoardSquarePathInfo chargePath,
        BoardSquare startSquare,
        float lineHalfWidthInSquares,
        ActorData caster,
        List<Team> relevantTeams,
        bool forServer,
        out BoardSquare destSquare)
    {
        destSquare = startSquare;
        if (chargePath == null || chargePath.next == null || lineHalfWidthInSquares <= 0f)
        {
            return;
        }
                
        destSquare = chargePath.GetPathEndpoint().square;
        Vector3 startLosCheckPos = startSquare.GetOccupantLoSPos();
        Vector3 destLosCheckPos = destSquare.GetOccupantLoSPos();
        List<ActorData> actors = AreaEffectUtils.GetActorsInBoxByActorRadius(
            startLosCheckPos,
            destLosCheckPos,
            2f * lineHalfWidthInSquares,
            false,
            caster,
            relevantTeams);
        actors.Remove(caster);
        if (!forServer)
        {
            TargeterUtils.RemoveActorsInvisibleToClient(ref actors);
        }

        Vector3 vector = destLosCheckPos - startLosCheckPos;
        vector.y = 0f;
        vector.Normalize();
        TargeterUtils.SortActorsByDistanceToPos(ref actors, startLosCheckPos, vector);
        if (actors.Count <= 0)
        {
            return;
        }

        Vector3 projectionPoint = VectorUtils.GetProjectionPoint(
            vector,
            startLosCheckPos,
            actors[0].GetLoSCheckPos());
        BoardSquarePathInfo step = chargePath.next;
        float dist = VectorUtils.HorizontalPlaneDistInWorld(projectionPoint, step.square.ToVector3());
        while (step.next != null)
        {
            float nextDist = VectorUtils.HorizontalPlaneDistInWorld(
                projectionPoint,
                step.next.square.ToVector3());
            if (nextDist > dist && step.square.IsValidForGameplay())
            {
                step.next.prev = null;
                step.next = null;
                destSquare = step.square;
                return;
            }

            dist = nextDist;
            step = step.next;
        }
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_ChargeAoE;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }

#if SERVER
    // rogues
    public override void CalcHitTargets(
        List<AbilityTarget> targets,
        ActorData caster,
        List<NonActorTargetInfo> nonActorTargetInfo)
    {
        ResetContextData();
        base.CalcHitTargets(targets, caster, nonActorTargetInfo);
        Vector3 startPos = caster.GetSquareAtPhaseStart().ToVector3();
        Vector3 targetPos = Board.Get().GetSquare(targets[0].GridPos).GetOccupantLoSPos();
        Vector3 loSCheckPos = caster.GetLoSCheckPos(caster.GetSquareAtPhaseStart());
        foreach (ActorData actorData in GetHitActorsInShape(loSCheckPos, targetPos, caster, nonActorTargetInfo))
        {
            AddHitActor(actorData, startPos);
            float sqrDitToStart = (actorData.GetLoSCheckPos() - startPos).sqrMagnitude;
            float sqrDistToEnd = (actorData.GetLoSCheckPos() - targetPos).sqrMagnitude;
            bool inStartAoe = sqrDitToStart <= m_radiusAroundStart * m_radiusAroundStart;
            bool inEndAoe = sqrDistToEnd <= m_radiusAroundEnd * m_radiusAroundEnd;
            SetActorContext(actorData, s_cvarInStart.GetKey(), inStartAoe ? 1 : 0);
            SetActorContext(actorData, s_cvarInEnd.GetKey(), inEndAoe ? 1 : 0);
        }

        if (IncludeCaster())
        {
            AddHitActor(caster, caster.GetCurrentBoardSquare().ToVector3());
        }
    }

    // rogues
    protected List<ActorData> GetHitActorsInShape(
        Vector3 startPos,
        Vector3 endPos,
        ActorData caster,
        List<NonActorTargetInfo> nonActorTargetInfo)
    {
        List<ActorData> actorsInRadiusOfLine = AreaEffectUtils.GetActorsInRadiusOfLine(
            startPos,
            endPos,
            m_radiusAroundStart,
            m_radiusAroundEnd,
            m_rangeFromLine,
            m_ignoreLos,
            caster,
            TargeterUtils.GetRelevantTeams(caster, m_includeAllies, m_includeEnemies),
            nonActorTargetInfo);
        ServerAbilityUtils.RemoveEvadersFromHitTargets(ref actorsInRadiusOfLine);
        return actorsInRadiusOfLine;
    }

    // rogues
    public override List<ServerClientUtils.SequenceStartData> CreateSequenceStartData(
        List<AbilityTarget> targets,
        ActorData caster,
        ServerAbilityUtils.AbilityRunData additionalData,
        Sequence.IExtraSequenceParams[] extraSequenceParams = null)
    {
        return new List<ServerClientUtils.SequenceStartData>
        {
            new ServerClientUtils.SequenceStartData(
                m_castSequencePrefab,
                Board.Get().GetSquare(targets[0].GridPos),
                additionalData.m_abilityResults.HitActorsArray(),
                caster,
                additionalData.m_sequenceSource,
                extraSequenceParams)
        };
    }
#endif
}