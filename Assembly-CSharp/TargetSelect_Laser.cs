using System.Collections.Generic;
using System.Linq;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_Laser : GenericAbility_TargetSelectBase
{
    [Separator("Targeting Properties")]
    public float m_laserRange = 5f;
    public float m_laserWidth = 1f;
    public int m_maxTargets;
    [Separator("AoE around start")]
    public float m_aoeRadiusAroundStart;
    [Separator("Sequences")]
    public GameObject m_castSequencePrefab;
    public GameObject m_aoeAtStartSequencePrefab;

    private TargetSelectMod_Laser m_targetSelMod;

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
                   ContextKeys.s_HitOrder.GetName(),
                   "on every non-caster hit actor, order in which they are hit in laser")
               + GetContextUsageStr(
                   ContextKeys.s_DistFromStart.GetName(),
                   "on every non-caster hit actor, distance from caster");
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add(ContextKeys.s_HitOrder.GetName());
        names.Add(ContextKeys.s_DistFromStart.GetName());
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        AbilityUtil_Targeter targeter;
        if (GetAoeRadiusAroundStart() <= 0f)
        {
            targeter = new AbilityUtil_Targeter_Laser(
                ability,
                GetLaserWidth(),
                GetLaserRange(),
                IgnoreLos(),
                GetMaxTargets(),
                IncludeAllies(),
                IncludeCaster());
        }
        else
        {
            targeter = new AbilityUtil_Targeter_ClaymoreSlam(
                ability,
                GetLaserRange(),
                GetLaserWidth(),
                GetMaxTargets(),
                360f,
                GetAoeRadiusAroundStart(),
                0f,
                IgnoreLos());
        }
        targeter.SetAffectedGroups(IncludeEnemies(), IncludeAllies(), IncludeCaster());
        return new List<AbilityUtil_Targeter> { targeter };
    }

    public float GetLaserRange()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_laserRangeMod.GetModifiedValue(m_laserRange)
            : m_laserRange;
    }

    public float GetLaserWidth()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_laserWidthMod.GetModifiedValue(m_laserWidth)
            : m_laserWidth;
    }

    public int GetMaxTargets()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_maxTargetsMod.GetModifiedValue(m_maxTargets)
            : m_maxTargets;
    }

    public float GetAoeRadiusAroundStart()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_aoeRadiusAroundStartMod.GetModifiedValue(m_aoeRadiusAroundStart)
            : m_aoeRadiusAroundStart;
    }

    public override bool CanShowTargeterRangePreview(TargetData[] targetData)
    {
        return true;
    }

    public override float GetTargeterRangePreviewRadius(Ability ability, ActorData caster)
    {
        return GetLaserRange();
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_Laser;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }

    //rogues
    private void GetHitActors(
        List<AbilityTarget> targets,
        ActorData caster,
        out List<ActorData> actorsForSequence,
        out List<Vector3> targetPosForSequences,
        List<NonActorTargetInfo> nonActorTargetInfo,
        out Vector3 endPos)
    {
        actorsForSequence = new List<ActorData>();
        targetPosForSequences = new List<Vector3>();
        actorsForSequence = AreaEffectUtils.GetActorsInLaser(
            caster.GetLoSCheckPos(),
            targets[0].AimDirection,
            m_laserRange,
            m_laserWidth,
            caster,
            TargeterUtils.GetRelevantTeams(caster, m_includeAllies, m_includeEnemies), m_ignoreLos,
            m_maxTargets,
            false,
            true,
            out endPos,
            nonActorTargetInfo);
        targetPosForSequences.Add(endPos);
        if (actorsForSequence.Any())
        {
            Vector3 knockbackOriginFromLaser = AreaEffectUtils.GetKnockbackOriginFromLaser(
                actorsForSequence,
                caster,
                targets[0].AimDirection,
                endPos);
            GetNonActorSpecificContext().SetValue(ContextKeys.s_KnockbackOrigin.GetKey(), knockbackOriginFromLaser);
        }

        if (m_includeCaster && !actorsForSequence.Contains(caster))
        {
            actorsForSequence.Add(caster);
        }
    }

    //rogues
    public override void CalcHitTargets(
        List<AbilityTarget> targets,
        ActorData caster,
        List<NonActorTargetInfo> nonActorTargetInfo)
    {
        ResetContextData();
        base.CalcHitTargets(targets, caster, nonActorTargetInfo);
        // if (this.m_maxPowerUpsGrabbedOnHit > 0)
        // {
        //     VectorUtils.LaserCoords laserCoords;
        //     List<PowerUp> list;
        //     List<ActorData> hitActorsInDirectionStatic = ThiefBasicAttack.GetHitActorsInDirectionStatic(
        //         caster.GetLoSCheckPos(),
        //         targets[0].AimDirection,
        //         caster,
        //         m_laserRange,
        //         m_laserWidth,
        //         m_ignoreLos,
        //         m_maxTargets,
        //         m_includeAllies,
        //         m_includeEnemies,
        //         true,
        //         this.m_maxPowerUpsGrabbedOnHit,
        //         true,
        //         this.m_stopOnPowerUp,
        //         this.m_includeSpoilsPowerUp,
        //         true,
        //         new HashSet<PowerUp>(),
        //         out laserCoords,
        //         out list,
        //         nonActorTargetInfo,
        //         false);
        //     if (list.Count > 0)
        //     {
        //         foreach (PowerUp powerUp in list)
        //         {
        //             this.m_powerUpsHit.Add(powerUp);
        //             powerUp.OnPickedUp(caster);
        //         }
        //         AddHitActor(caster, caster.GetFreePos());
        //         SetActorContext(caster, TargetSelect_Laser.s_PowerUpsHit.GetKey(), list.Count);
        //     }
        //     for (int i = 0; i < hitActorsInDirectionStatic.Count; i++)
        //     {
        //         ActorData actor = hitActorsInDirectionStatic[i];
        //         AddHitActor(actor, caster.GetLoSCheckPos());
        //         SetActorContext(actor, ContextKeys.s_HitOrder.GetKey(), i);
        //     }
        //     return;
        // }
        List<ActorData> actorsForSequence;
        List<Vector3> targetPosForSequence;
        Vector3 endPos;
        GetHitActors(targets, caster, out actorsForSequence, out targetPosForSequence, nonActorTargetInfo, out endPos);
        for (int actorIndex = 0; actorIndex < actorsForSequence.Count; actorIndex++)
        {
            ActorData actor = actorsForSequence[actorIndex];
            AddHitActor(actor, caster.GetLoSCheckPos());
            SetActorContext(actor, ContextKeys.s_HitOrder.GetKey(), actorIndex);
        }
    }

    //rogues
    public override List<ServerClientUtils.SequenceStartData> CreateSequenceStartData(
        List<AbilityTarget> targets,
        ActorData caster,
        ServerAbilityUtils.AbilityRunData additionalData,
        Sequence.IExtraSequenceParams[] extraSequenceParams = null)
    {
        List<ServerClientUtils.SequenceStartData> list = new List<ServerClientUtils.SequenceStartData>();
        // if (this.m_maxPowerUpsGrabbedOnHit > 0)
        // {
        //     this.GetSequencePositionAndTargetsWithPowerups(
        //         targets,
        //         caster,
        //         out List<Vector3> list2,
        //         out List<List<ActorData>> list3,
        //         out List<List<PowerUp>> list4);
        //     for (int i = 0; i < list2.Count; i++)
        //     {
        //         if (list4[i].Count > 0 && this.m_powerupReturnPrefab != null)
        //         {
        //             list3[i].Remove(caster);
        //         }
        //         ServerClientUtils.SequenceStartData item = new ServerClientUtils.SequenceStartData(
        //             m_castSequencePrefab,
        //             list2[i],
        //             list3[i].ToArray(),
        //             caster,
        //             additionalData.m_sequenceSource,
        //             extraSequenceParams);
        //         list.Add(item);
        //         if (list4[i].Count > 0 && this.m_powerupReturnPrefab != null)
        //         {
        //             List<PowerUp> list5 = list4[i];
        //             for (int j = 0; j < list5.Count; j++)
        //             {
        //                 PowerUp powerUp = list5[j];
        //                 List<Sequence.IExtraSequenceParams> list6 = new List<Sequence.IExtraSequenceParams>();
        //                 list6.Add(new SplineProjectileSequence.DelayedProjectileExtraParams
        //                 {
        //                     useOverrideStartPos = true,
        //                     overrideStartPos = powerUp.gameObject.transform.position
        //                 });
        //                 list6.Add(new ThiefPowerupReturnProjectileSequence.PowerupTypeExtraParams
        //                 {
        //                     powerupCategory = (int)powerUp.m_chatterCategory
        //                 });
        //                 ServerClientUtils.SequenceStartData item2 = new ServerClientUtils.SequenceStartData(
        //                     this.m_powerupReturnPrefab,
        //                     caster.GetFreePos(),
        //                     caster.AsArray(),
        //                     caster,
        //                     additionalData.m_sequenceSource,
        //                     list6.ToArray());
        //                 list.Add(item2);
        //             }
        //         }
        //     }
        // }
        // else
        // {
        List<ActorData> actorHit;
        List<Vector3> targetPosForSequences;
        Vector3 endPos;

        GetHitActors(targets, caster, out actorHit, out targetPosForSequences, null, out endPos);
        list.Add(new ServerClientUtils.SequenceStartData(m_castSequencePrefab,
            Board.Get().GetSquareFromVec3(endPos), actorHit.ToArray(), caster, additionalData.m_sequenceSource,
            extraSequenceParams));
        //}
        return list;
    }
}