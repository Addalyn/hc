using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class FireborgDualCones : GenericAbility_Container
{
    [Separator("Extra Damage for overlap state")]
    public int m_extraDamageIfOverlap;
    public int m_extraDamageNonOverlap;
    [Separator("Add Ignited Effect If Overlap Hit")]
    public bool m_igniteTargetIfOverlapHit = true;
    public bool m_igniteTargetIfSuperheated = true;
    [Separator("Ground Fire")]
    public bool m_groundFireOnAllIfNormal;
    public bool m_groundFireOnOverlapIfNormal;
    [Space(10f)]
    public bool m_groundFireOnAllIfSuperheated = true;
    public bool m_groundFireOnOverlapIfSuperheated = true;
    [Separator("Superheat Sequence")]
    public GameObject m_superheatCastSeqPrefab;

    private Fireborg_SyncComponent m_syncComp;
    private AbilityMod_FireborgDualCones m_abilityMod;

    public override string GetUsageForEditor()
    {
        return base.GetUsageForEditor() + Fireborg_SyncComponent.GetSuperheatedCvarUsage();
    }

    public override List<string> GetContextNamesForEditor()
    {
        List<string> contextNamesForEditor = base.GetContextNamesForEditor();
        contextNamesForEditor.Add(Fireborg_SyncComponent.s_cvarSuperheated.GetName());
        return contextNamesForEditor;
    }

    protected override void SetupTargetersAndCachedVars()
    {
        m_syncComp = GetComponent<Fireborg_SyncComponent>();
        base.SetupTargetersAndCachedVars();
    }

    protected override void AddSpecificTooltipTokens(List<TooltipTokenEntry> tokens, AbilityMod modAsBase)
    {
        base.AddSpecificTooltipTokens(tokens, modAsBase);
        AddTokenInt(tokens, "ExtraDamageIfOverlap", string.Empty, m_extraDamageIfOverlap);
        AddTokenInt(tokens, "ExtraDamageNonOverlap", string.Empty, m_extraDamageNonOverlap);
    }

    public int GetExtraDamageIfOverlap()
    {
        return m_abilityMod != null
            ? m_abilityMod.m_extraDamageIfOverlapMod.GetModifiedValue(m_extraDamageIfOverlap)
            : m_extraDamageIfOverlap;
    }

    public int GetExtraDamageNonOverlap()
    {
        return m_abilityMod != null
            ? m_abilityMod.m_extraDamageNonOverlapMod.GetModifiedValue(m_extraDamageNonOverlap)
            : m_extraDamageNonOverlap;
    }

    public bool IgniteTargetIfOverlapHit()
    {
        return m_abilityMod != null
            ? m_abilityMod.m_igniteTargetIfOverlapHitMod.GetModifiedValue(m_igniteTargetIfOverlapHit)
            : m_igniteTargetIfOverlapHit;
    }

    public bool IgniteTargetIfSuperheated()
    {
        return m_abilityMod != null
            ? m_abilityMod.m_igniteTargetIfSuperheatedMod.GetModifiedValue(m_igniteTargetIfSuperheated)
            : m_igniteTargetIfSuperheated;
    }

    public bool GroundFireOnAllIfNormal()
    {
        return m_abilityMod != null
            ? m_abilityMod.m_groundFireOnAllIfNormalMod.GetModifiedValue(m_groundFireOnAllIfNormal)
            : m_groundFireOnAllIfNormal;
    }

    public bool GroundFireOnOverlapIfNormal()
    {
        return m_abilityMod != null
            ? m_abilityMod.m_groundFireOnOverlapIfNormalMod.GetModifiedValue(m_groundFireOnOverlapIfNormal)
            : m_groundFireOnOverlapIfNormal;
    }

    public bool GroundFireOnAllIfSuperheated()
    {
        return m_abilityMod != null
            ? m_abilityMod.m_groundFireOnAllIfSuperheatedMod.GetModifiedValue(m_groundFireOnAllIfSuperheated)
            : m_groundFireOnAllIfSuperheated;
    }

    public bool GroundFireOnOverlapIfSuperheated()
    {
        return m_abilityMod != null
            ? m_abilityMod.m_groundFireOnOverlapIfSuperheatedMod.GetModifiedValue(m_groundFireOnOverlapIfSuperheated)
            : m_groundFireOnOverlapIfSuperheated;
    }

    public override void PreProcessTargetingNumbers(
        ActorData targetActor,
        int currentTargetIndex,
        Dictionary<ActorData, ActorHitContext> actorHitContext,
        ContextVars abilityContext)
    {
        m_syncComp.SetSuperheatedContextVar(abilityContext);
    }

    public override void PostProcessTargetingNumbers(
        ActorData targetActor,
        int currentTargeterIndex,
        Dictionary<ActorData, ActorHitContext> actorHitContext,
        ContextVars abilityContext,
        ActorData caster,
        TargetingNumberUpdateScratch results)
    {
        if (GetExtraDamageIfOverlap() <= 0 && GetExtraDamageNonOverlap() <= 0)
        {
            return;
        }

        if (!actorHitContext.ContainsKey(targetActor))
        {
            return;
        }

        actorHitContext[targetActor].m_contextVars.TryGetInt(ContextKeys.s_HitCount.GetKey(), out int hitCount);
        if (hitCount > 1)
        {
            if (GetExtraDamageIfOverlap() > 0)
            {
                results.m_damage += GetExtraDamageIfOverlap();
            }
        }
        else
        {
            if (GetExtraDamageNonOverlap() > 0)
            {
                results.m_damage += GetExtraDamageNonOverlap();
            }
        }
    }

    public override string GetAccessoryTargeterNumberString(
        ActorData targetActor,
        AbilityTooltipSymbol symbolType,
        int baseValue)
    {
        if (ShouldAddGroundFire()
            && (ShouldAddGroundFireToAllSquares() || GetTargeterHitCountOnTarget(targetActor) > 1)
            && !m_syncComp.m_actorsInGroundFireOnTurnStart.Contains((uint)targetActor.ActorIndex))
        {
            return m_syncComp.GetTargetPreviewAccessoryString(symbolType, this, targetActor, ActorData);
        }

        return null;
    }

    private int GetTargeterHitCountOnTarget(ActorData targetActor)
    {
        if (Targeter.GetActorContextVars().TryGetValue(targetActor, out ActorHitContext actorHitContext)
            && actorHitContext.m_contextVars.TryGetInt(ContextKeys.s_HitCount.GetKey(), out int hitCount))
        {
            return hitCount;
        }

        return 0;
    }

    private bool ShouldAddGroundFire()
    {
        return m_syncComp.InSuperheatMode()
            ? GroundFireOnAllIfSuperheated() || GroundFireOnOverlapIfSuperheated()
            : GroundFireOnAllIfNormal() || GroundFireOnOverlapIfNormal();
    }

    private bool ShouldAddGroundFireToAllSquares()
    {
        return m_syncComp.InSuperheatMode()
            ? GroundFireOnAllIfSuperheated()
            : GroundFireOnAllIfNormal();
    }

    protected override void GenModImpl_SetModRef(AbilityMod abilityMod)
    {
        m_abilityMod = abilityMod as AbilityMod_FireborgDualCones;
    }

    protected override void GenModImpl_ClearModRef()
    {
        m_abilityMod = null;
    }
}