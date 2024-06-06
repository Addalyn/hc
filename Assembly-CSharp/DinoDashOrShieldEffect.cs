using UnityEngine;

#if SERVER
// custom
public class DinoDashOrShieldEffect : StandardActorEffect
{
    private readonly StandardEffectInfo m_delayedShieldInfo;
    private readonly int m_healIfNoDash;
    private readonly ActorModelData.ActionAnimationType m_noDashShieldAnimIndex;
    private readonly AbilityData.ActionType m_abilityActionType;
    private readonly int m_abilityCooldown;
    private readonly GameObject m_onTriggerSequencePrefab;
    
    public DinoDashOrShieldEffect(
        EffectSource parent,
        BoardSquare targetSquare,
        ActorData target,
        ActorData caster,
        StandardEffectInfo delayedShieldInfo,
        int healIfNoDash,
        ActorModelData.ActionAnimationType noDashShieldAnimIndex,
        AbilityData.ActionType abilityActionType,
        int abilityCooldown,
        GameObject onTriggerSequencePrefab)
        : base(parent, targetSquare, target, caster, new StandardActorEffectData())
    {
        m_delayedShieldInfo = delayedShieldInfo;
        m_healIfNoDash = healIfNoDash;
        m_noDashShieldAnimIndex = noDashShieldAnimIndex;
        m_abilityActionType = abilityActionType;
        m_abilityCooldown = abilityCooldown;
        m_onTriggerSequencePrefab = onTriggerSequencePrefab;
        
        HitPhase = AbilityPriority.Prep_Defense;
        m_time.duration = 2;
    }

    public override int GetCasterAnimationIndex(AbilityPriority phaseIndex)
    {
        return (int)m_noDashShieldAnimIndex;
    }

    private bool ApplyEffect()
    {
        AbilityData abilityData = Caster.GetAbilityData();
        return abilityData != null && !abilityData.HasQueuedAbilityOfType(typeof(DinoDashOrShield));
    }

    public override ServerClientUtils.SequenceStartData GetEffectHitSeqData()
    {
        if (!ApplyEffect())
        {
            return base.GetEffectHitSeqData();
        }
        
        return new ServerClientUtils.SequenceStartData(
            m_onTriggerSequencePrefab,
            TargetSquare,
            new[] { Target },
            Caster,
            SequenceSource);
    }

    public override void GatherEffectResults(ref EffectResults effectResults, bool isReal)
    {
        if (!ApplyEffect())
        {
            return;
        }
        
        ActorHitResults hitRes = new ActorHitResults(new ActorHitParameters(Target, Caster.GetFreePos()));
        hitRes.AddStandardEffectInfo(m_delayedShieldInfo);
        hitRes.AddBaseHealing(m_healIfNoDash);
        hitRes.AddMiscHitEvent(
            new MiscHitEventData_AddToCasterCooldown(m_abilityActionType, m_abilityCooldown + 1));
        effectResults.StoreActorHit(hitRes);
    }
}
#endif