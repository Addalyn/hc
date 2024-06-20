using System.Collections.Generic;
using UnityEngine;

public class ScampAoeTetherEffect: StandardActorEffect
{
    private readonly float m_tetherBreakDistanceOverride; // TODO SCAMP unused, always -1
    private readonly bool m_pullToCasterInKnockback; // TODO SCAMP unused, always true. Otherwise, we are supposed to apply hits on movement.
    private readonly float m_maxKnockbackDist;
    private readonly int m_tetherBreakDamage;
    private readonly StandardEffectInfo m_tetherBreakEnemyEffect;
    private readonly GameObject m_tetherBreakTriggerSequencePrefab;
    private readonly Passive_Scamp m_passive;

    private static readonly float s_tetherBreakDist = 2.5f * Board.SquareSizeStatic;
    private static readonly float s_tetherBreakDistSqr = s_tetherBreakDist * s_tetherBreakDist;
    
    public ScampAoeTetherEffect(
        EffectSource parent,
        ActorData target,
        ActorData caster,
        StandardActorEffectData data,
        float tetherBreakDistanceOverride,
        bool pullToCasterInKnockback,
        float maxKnockbackDist,
        int tetherBreakDamage,
        StandardEffectInfo tetherBreakEnemyEffect,
        GameObject tetherBreakTriggerSequencePrefab,
        Passive_Scamp passive)
        : base(parent, null, target, caster, data)
    {
        m_tetherBreakDistanceOverride = tetherBreakDistanceOverride;
        m_pullToCasterInKnockback = pullToCasterInKnockback;
        m_maxKnockbackDist = maxKnockbackDist;
        m_tetherBreakDamage = tetherBreakDamage;
        m_tetherBreakEnemyEffect = tetherBreakEnemyEffect;
        m_tetherBreakTriggerSequencePrefab = tetherBreakTriggerSequencePrefab;
        m_passive = passive;
        HitPhase = AbilityPriority.Combat_Knockback;
        m_time.duration = 1;
    }

    private bool IsBroken()
    {
        return (Target.GetFreePos() - Caster.GetFreePos()).sqrMagnitude > s_tetherBreakDistSqr;
    }
    
    public override List<ServerClientUtils.SequenceStartData> GetEffectHitSeqDataList()
    {
        if (!IsBroken())
        {
            return base.GetEffectHitSeqDataList();
        }
		
        SequenceSource sequenceSource = SequenceSource.GetShallowCopy();
        if (AddActorAnimEntryIfHasHits(HitPhase))
        {
            sequenceSource.SetWaitForClientEnable(true);
        }
        return new List<ServerClientUtils.SequenceStartData>
        {
            new ServerClientUtils.SequenceStartData(
                m_tetherBreakTriggerSequencePrefab,
                Target.GetFreePos(),
                Target.AsArray(),
                Caster,
                sequenceSource,
                new Sequence.IExtraSequenceParams[]
                {
                    new Sequence.FxAttributeParam
                    {
                        m_paramNameCode = Sequence.FxAttributeParam.ParamNameCode.ScaleControl,
                        m_paramTarget = Sequence.FxAttributeParam.ParamTarget.MainVfx,
                        m_paramValue = 4.8f
                    },
                    new Sequence.FxAttributeParam
                    {
                        m_paramNameCode = Sequence.FxAttributeParam.ParamNameCode.ScaleControl,
                        m_paramTarget = Sequence.FxAttributeParam.ParamTarget.ImpactVfx,
                        m_paramValue = 4.8f
                    }
                })
        };
    }

	public override void GatherEffectResults(ref EffectResults effectResults, bool isReal)
	{
		if (!IsBroken())
		{
			return;
		}

        if (isReal)
        {
            m_passive.OnTetherBroken();
        }
		
		ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(Target, Caster.GetFreePos()));
        KnockbackHitData knockbackData = new KnockbackHitData(
            Target,
            Caster,
            KnockbackType.PullToSourceActor,
            Vector3.zero,
            Vector3.zero,
            m_maxKnockbackDist);
        actorHitResults.AddKnockbackData(knockbackData);
        actorHitResults.AddBaseDamage(m_tetherBreakDamage);
        actorHitResults.AddStandardEffectInfo(m_tetherBreakEnemyEffect);
            
		effectResults.StoreActorHit(actorHitResults);
	}

	public override bool AddActorAnimEntryIfHasHits(AbilityPriority phaseIndex)
	{
		return Target != null && HitPhase == phaseIndex;
	}

	public override ActorData GetActorAnimationActor()
	{
		return Target;
	}
}