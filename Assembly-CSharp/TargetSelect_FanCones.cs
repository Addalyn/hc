// ROGUES
// SERVER
using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_FanCones : GenericAbility_TargetSelectBase
{
    [Separator("Targeting Properties")]
    public ConeTargetingInfo m_coneInfo;
    [Space(10f)]
    public int m_coneCount = 3;
    [Header("Starting offset, move towards forward/aim direction")]
    public float m_coneStartOffsetInAimDir;
    [Header("Starting offset, move towards left/right")]
    public float m_coneStartOffsetToSides;
    [Header("Starting offset, move towards each cone's direction")]
    public float m_coneStartOffsetInConeDir;
    [Header("-- If Fixed Angle")]
    public float m_angleInBetween = 10f;
    [Header("-- If Interpolating Angle")]
    public bool m_changeAngleByCursorDistance = true;
    public float m_targeterMinAngle;
    public float m_targeterMaxAngle = 180f;
    public float m_startAngleOffset;
    [Space(10f)]
    public float m_targeterMinInterpDistance = 0.5f;
    public float m_targeterMaxInterpDistance = 4f;
    [Separator("Sequences")]
    public GameObject m_castSequencePrefab;

    private TargetSelectMod_FanCones m_targetSelMod;
    private ConeTargetingInfo m_cachedConeInfo;

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
            ContextKeys.s_HitCount.GetName(),
            "on every hit actor, number of cone hits on target");
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add(ContextKeys.s_HitCount.GetName());
    }

    public override void Initialize()
    {
        SetCachedFields();
        ConeTargetingInfo coneInfo = GetConeInfo();
        coneInfo.m_affectsAllies = IncludeAllies();
        coneInfo.m_affectsEnemies = IncludeEnemies();
        coneInfo.m_affectsCaster = IncludeCaster();
        coneInfo.m_penetrateLos = IgnoreLos();
    }

    private void SetCachedFields()
    {
        m_cachedConeInfo = m_targetSelMod != null
            ? m_targetSelMod.m_coneInfoMod.GetModifiedValue(m_coneInfo)
            : m_coneInfo;
    }

    public ConeTargetingInfo GetConeInfo()
    {
        return m_cachedConeInfo ?? m_coneInfo;
    }

    public int GetConeCount()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneCountMod.GetModifiedValue(m_coneCount)
            : m_coneCount;
    }

    public float GetConeStartOffsetInAimDir()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneStartOffsetInAimDirMod.GetModifiedValue(m_coneStartOffsetInAimDir)
            : m_coneStartOffsetInAimDir;
    }

    public float GetConeStartOffsetToSides()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneStartOffsetToSidesMod.GetModifiedValue(m_coneStartOffsetToSides)
            : m_coneStartOffsetToSides;
    }

    public float GetConeStartOffsetInConeDir()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_coneStartOffsetInConeDirMod.GetModifiedValue(m_coneStartOffsetInConeDir)
            : m_coneStartOffsetInConeDir;
    }

    public float GetAngleInBetween()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_angleInBetweenMod.GetModifiedValue(m_angleInBetween)
            : m_angleInBetween;
    }

    public bool ChangeAngleByCursorDistance()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_changeAngleByCursorDistanceMod.GetModifiedValue(m_changeAngleByCursorDistance)
            : m_changeAngleByCursorDistance;
    }

    public float GetTargeterMinAngle()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_targeterMinAngleMod.GetModifiedValue(m_targeterMinAngle)
            : m_targeterMinAngle;
    }

    public float GetTargeterMaxAngle()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_targeterMaxAngleMod.GetModifiedValue(m_targeterMaxAngle)
            : m_targeterMaxAngle;
    }

    public float GetStartAngleOffset()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_startAngleOffsetMod.GetModifiedValue(m_startAngleOffset)
            : m_startAngleOffset;
    }

    protected virtual bool UseCasterPosForLoS()
    {
        return false;
    }

    protected virtual bool CustomLoS(ActorData actor, ActorData caster)
    {
        return true;
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        AbilityUtil_Targeter_TricksterCones targeter = new AbilityUtil_Targeter_TricksterCones(
            ability,
            GetConeInfo(),
            GetConeCount(),
            GetConeCount,
            GetConeOrigins,
            GetConeDirections,
            GetFreePosForAim,
            false,
            UseCasterPosForLoS())
        {
            m_customDamageOriginDelegate = GetDamageOriginForTargeter
        };
        return new List<AbilityUtil_Targeter> { targeter };
    }

    private Vector3 GetDamageOriginForTargeter(
        AbilityTarget currentTarget,
        Vector3 defaultOrigin,
        ActorData actorToAdd,
        ActorData caster)
    {
        return caster.GetFreePos();
    }

    public Vector3 GetFreePosForAim(AbilityTarget currentTarget, ActorData caster)
    {
        return currentTarget.FreePos;
    }

    public virtual List<Vector3> GetConeOrigins(AbilityTarget currentTarget, Vector3 targeterFreePos, ActorData caster)
    {
        List<Vector3> list = new List<Vector3>();
        Vector3 losCheckPos = caster.GetLoSCheckPos();
        Vector3 aimDirection = currentTarget.AimDirection;
        Vector3 normalized = Vector3.Cross(aimDirection, Vector3.up).normalized;
        int coneCount = GetConeCount();
        int halfConeCount = coneCount / 2;
        bool evenCones = coneCount % 2 == 0;
        float aimDirOffset = GetConeStartOffsetInAimDir() * Board.SquareSizeStatic;
        float sideOffsetDir = GetConeStartOffsetToSides() * Board.SquareSizeStatic;
        for (int i = 0; i < coneCount; i++)
        {
            Vector3 b = Vector3.zero;
            if (aimDirOffset != 0f)
            {
                b = aimDirOffset * aimDirection;
            }

            if (sideOffsetDir > 0f)
            {
                if (evenCones)
                {
                    if (i < halfConeCount)
                    {
                        b -= (halfConeCount - i) * sideOffsetDir * normalized;
                    }
                    else
                    {
                        b += (i - halfConeCount + 1) * sideOffsetDir * normalized;
                    }
                }
                else if (i < halfConeCount)
                {
                    b -= (halfConeCount - i) * sideOffsetDir * normalized;
                }
                else if (i > halfConeCount)
                {
                    b += (i - halfConeCount) * sideOffsetDir * normalized;
                }
            }

            list.Add(losCheckPos + b);
        }

        if (GetConeStartOffsetInConeDir() > 0f)
        {
            List<Vector3> coneDirections = GetConeDirections(currentTarget, targeterFreePos, caster);
            float d = GetConeStartOffsetInConeDir() * Board.SquareSizeStatic;
            for (int i = 0; i < coneDirections.Count; i++)
            {
                list[i] += d * coneDirections[i];
            }
        }

        return list;
    }

    public virtual List<Vector3> GetConeDirections(
        AbilityTarget currentTarget,
        Vector3 targeterFreePos,
        ActorData caster)
    {
        List<Vector3> list = new List<Vector3>();
        float angleInBetween = GetAngleInBetween();
        int coneCount = GetConeCount();
        if (ChangeAngleByCursorDistance())
        {
            float angleTotal = coneCount <= 1
                ? 0f
                : AbilityCommon_FanLaser.CalculateFanAngleDegrees(
                    currentTarget,
                    caster,
                    GetTargeterMinAngle(),
                    GetTargeterMaxAngle(),
                    m_targeterMinInterpDistance,
                    m_targeterMaxInterpDistance,
                    0f);

            angleInBetween = coneCount > 1
                ? angleTotal / (coneCount - 1)
                : 0f;
        }

        float aimAngle = VectorUtils.HorizontalAngle_Deg(currentTarget.AimDirection) + GetStartAngleOffset();
        float startAngle = aimAngle - 0.5f * (coneCount - 1) * angleInBetween;
        for (int i = 0; i < coneCount; i++)
        {
            list.Add(VectorUtils.AngleDegreesToVector(startAngle + i * angleInBetween));
        }

        return list;
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_FanCones;
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
        Dictionary<ActorData, int> hitActorsAndHitCount = GetHitActorsAndHitCount(
            targets,
            caster,
            out _,
            out _,
            out _,
            out _,
            nonActorTargetInfo);
        foreach (ActorData actorData in hitActorsAndHitCount.Keys)
        {
            AddHitActor(actorData, caster.GetLoSCheckPos());
            SetActorContext(actorData, ContextKeys.s_HitCount.GetKey(), hitActorsAndHitCount[actorData]);
        }
    }

    // rogues
    protected Dictionary<ActorData, int> GetHitActorsAndHitCount(
        List<AbilityTarget> targets,
        ActorData caster,
        out List<List<ActorData>> actorsForSequence,
        out List<Vector3> coneEndPosList,
        out List<Vector3> coneStartPosList,
        out int numConesWithHits,
        List<NonActorTargetInfo> nonActorTargetInfo)
    {
        actorsForSequence = new List<List<ActorData>>();
        coneEndPosList = new List<Vector3>();
        numConesWithHits = 0;
        Dictionary<ActorData, int> dictionary = new Dictionary<ActorData, int>();
        List<Vector3> coneDirections = GetConeDirections(targets[0], targets[0].FreePos, caster);
        List<Vector3> coneOrigins = GetConeOrigins(targets[0], targets[0].FreePos, caster);
        coneStartPosList = coneOrigins;
        ConeTargetingInfo coneInfo = m_coneInfo;
        for (int i = 0; i < coneDirections.Count; i++)
        {
            Vector3 coneDirection = coneDirections[i];
            Vector3 coneOrigin = coneOrigins[i];
            List<ActorData> actorsInCone = AreaEffectUtils.GetActorsInCone(
                coneOrigin,
                VectorUtils.HorizontalAngle_Deg(coneDirection),
                coneInfo.m_widthAngleDeg,
                coneInfo.m_radiusInSquares,
                coneInfo.m_backwardsOffset,
                coneInfo.m_penetrateLos,
                caster,
                TargeterUtils.GetRelevantTeams(caster, coneInfo.m_affectsAllies, coneInfo.m_affectsEnemies),
                nonActorTargetInfo);
            if (coneInfo.m_affectsCaster && i == 0)
            {
                actorsInCone.Add(caster);
            }

            foreach (ActorData actorData in actorsInCone.ToArray())
            {
                if (!CustomLoS(actorData, caster))
                {
                    actorsInCone.Remove(actorData);
                }
            }

            actorsForSequence.Add(actorsInCone);
            coneEndPosList.Add(coneOrigin + coneInfo.m_radiusInSquares * Board.SquareSizeStatic * coneDirection);
            if (actorsInCone.Count > 0)
            {
                numConesWithHits++;
            }

            foreach (ActorData actorData in actorsInCone)
            {
                if (dictionary.ContainsKey(actorData))
                {
                    dictionary[actorData]++;
                }
                else
                {
                    dictionary[actorData] = 1;
                }
            }
        }

        return dictionary;
    }

    // rogues
    public override List<ServerClientUtils.SequenceStartData> CreateSequenceStartData(
        List<AbilityTarget> targets,
        ActorData caster,
        ServerAbilityUtils.AbilityRunData additionalData,
        Sequence.IExtraSequenceParams[] extraSequenceParams = null)
    {
        List<ServerClientUtils.SequenceStartData> list = new List<ServerClientUtils.SequenceStartData>();
        bool containsSelf = additionalData.m_abilityResults.HitActorList().Contains(caster);
        GetHitActorsAndHitCount(
            targets,
            caster,
            out List<List<ActorData>> actorsForSequence,
            out List<Vector3> coneEndPosList,
            out List<Vector3> coneStartPosList,
            out _,
            null);
        for (int i = 0; i < actorsForSequence.Count; i++)
        {
            List<Sequence.IExtraSequenceParams> sequenceParams = new List<Sequence.IExtraSequenceParams>();
            if (extraSequenceParams != null)
            {
                sequenceParams.AddRange(extraSequenceParams);
            }

            sequenceParams.AddRange(CreateConeSequenceExtraParam(coneStartPosList[i], coneEndPosList[i]));
            list.Add(
                new ServerClientUtils.SequenceStartData(
                    m_castSequencePrefab,
                    coneStartPosList[i],
                    actorsForSequence[i].ToArray(),
                    caster,
                    additionalData.m_sequenceSource,
                    sequenceParams.ToArray()));
        }

        if (containsSelf)
        {
            list.Add(
                new ServerClientUtils.SequenceStartData(
                    SequenceLookup.Get().GetSimpleHitSequencePrefab(),
                    caster.GetFreePos(),
                    caster.AsArray(),
                    caster,
                    additionalData.m_sequenceSource));
        }

        return list;
    }

    // rogues
    public virtual Sequence.IExtraSequenceParams[] CreateConeSequenceExtraParam(
        Vector3 coneStartPos,
        Vector3 coneEndPos)
    {
        return new BlasterStretchConeSequence.ExtraParams
        {
            lengthInSquares = m_coneInfo.m_radiusInSquares,
            angleInDegrees = m_coneInfo.m_widthAngleDeg,
            forwardAngle = VectorUtils.HorizontalAngle_Deg(coneEndPos - coneStartPos)
        }.ToArray();
    }

    // TODO
    /*

    public override void CalcHitTargets(List<AbilityTarget> targets, ActorData caster, List<NonActorTargetInfo> nonActorTargetInfo)
    {
        this.ResetContextData();
        base.CalcHitTargets(targets, caster, nonActorTargetInfo);
        Vector3 vector = caster.GetLoSCheckPos();
        Vector3 aimDirection = targets[0].AimDirection;
        if (!Mathf.Approximately(this.m_backwardsDistanceOffset, 0f))
        {
            vector = caster.GetLoSCheckPos() - aimDirection.normalized * this.m_backwardsDistanceOffset;
        }
        float coneCenterAngleDegrees = VectorUtils.HorizontalAngle_Deg(aimDirection);
        int numActiveLayers = this.GetNumActiveLayers();
        this.GetNonActorSpecificContext().SetValue(ContextKeys.s_LayersActive.GetKey(), numActiveLayers);
        foreach (ActorData actorData in this.GetConeHitActors(targets, caster, nonActorTargetInfo))
        {
            this.AddHitActor(actorData, vector, false);
            int num = 0;
            while (num < this.m_cachedRadiusList.Count && num < numActiveLayers)
            {
                if (AreaEffectUtils.IsSquareInConeByActorRadius(actorData.GetCurrentBoardSquare(), vector, coneCenterAngleDegrees, this.GetConeWidthAngle(), this.m_cachedRadiusList[num], 0f, base.IgnoreLos(), caster))
                {
                    base.SetActorContext(actorData, ContextKeys.s_Layer.GetKey(), num);
                    break;
                }
                num++;
            }
        }
    }

    private List<ActorData> GetConeHitActors(List<AbilityTarget> targets, ActorData caster, List<NonActorTargetInfo> nonActorTargetInfo)
    {
        Vector3 aimDirection = targets[0].AimDirection;
        float coneCenterAngleDegrees = VectorUtils.HorizontalAngle_Deg(aimDirection);
        Vector3 vector = caster.GetLoSCheckPos();
        if (!Mathf.Approximately(this.m_backwardsDistanceOffset, 0f))
        {
            vector = caster.GetLoSCheckPos() - aimDirection.normalized * this.m_backwardsDistanceOffset;
        }
        List<ActorData> actorsInCone = AreaEffectUtils.GetActorsInCone(vector, coneCenterAngleDegrees, this.GetConeWidthAngle(), this.GetMaxConeRadius(), 0f, base.IgnoreLos(), caster, TargeterUtils.GetRelevantTeams(caster, base.IncludeAllies(), base.IncludeEnemies()), nonActorTargetInfo);
        if (base.IncludeCaster() && !actorsInCone.Contains(caster))
        {
            actorsInCone.Add(caster);
        }
        TargeterUtils.SortActorsByDistanceToPos(ref actorsInCone, vector);
        return actorsInCone;
    }

    public override List<ServerClientUtils.SequenceStartData> CreateSequenceStartData(List<AbilityTarget> targets, ActorData caster, ServerAbilityUtils.AbilityRunData additionalData, Sequence.IExtraSequenceParams[] extraSequenceParams = null)
    {
        List<ServerClientUtils.SequenceStartData> list = new List<ServerClientUtils.SequenceStartData>();
        List<Sequence.IExtraSequenceParams> list2 = new List<Sequence.IExtraSequenceParams>();
        if (extraSequenceParams != null)
        {
            list2.AddRange(extraSequenceParams);
        }
        BlasterStretchConeSequence.ExtraParams extraParams = new BlasterStretchConeSequence.ExtraParams();
        extraParams.angleInDegrees = this.GetConeWidthAngle();
        extraParams.forwardAngle = VectorUtils.HorizontalAngle_Deg(targets[0].AimDirection);
        extraParams.lengthInSquares = this.GetMaxConeRadius();
        if (!Mathf.Approximately(this.m_backwardsDistanceOffset, 0f))
        {
            extraParams.useStartPosOverride = true;
            extraParams.startPosOverride = targets[0].FreePos - targets[0].AimDirection.normalized * this.m_backwardsDistanceOffset;
        }
        list2.Add(extraParams);
        ServerClientUtils.SequenceStartData item = new ServerClientUtils.SequenceStartData(this.m_coneSequencePrefab, caster.GetCurrentBoardSquare(), additionalData.m_abilityResults.HitActorsArray(), caster, additionalData.m_sequenceSource, list2.ToArray());
        list.Add(item);
        return list;
    }

    public float GetConeWidthAngle()
    {
        if (this.m_targetSelMod == null)
        {
            return this.m_coneWidthAngle;
        }
        return this.m_targetSelMod.m_coneWidthAngleMod.GetModifiedValue(this.m_coneWidthAngle);
    }

    public float GetMaxConeRadius()
    {
        float result = 0f;
        int numActiveLayers = this.GetNumActiveLayers();
        if (numActiveLayers > 0)
        {
            result = this.m_cachedRadiusList[numActiveLayers - 1];
        }
        return result;
    }

    public int GetNumActiveLayers()
    {
        if (this.m_delegateNumActiveLayers != null)
        {
            return this.m_delegateNumActiveLayers(this.m_cachedRadiusList.Count);
        }
        return this.m_cachedRadiusList.Count;
    }

    */
#endif
}