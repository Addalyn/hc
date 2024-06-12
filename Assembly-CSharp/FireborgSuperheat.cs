using System.Collections.Generic;

public class FireborgSuperheat : GenericAbility_Container
{
    [Separator("Superheat")]
    public int m_superheatDuration = 2;
    public int m_igniteExtraDamageIfSuperheated;

    private Fireborg_SyncComponent m_syncComp;
    private AbilityMod_FireborgSuperheat m_abilityMod;

    protected override void SetupTargetersAndCachedVars()
    {
        m_syncComp = GetComponent<Fireborg_SyncComponent>();
        base.SetupTargetersAndCachedVars();
    }

    protected override void AddSpecificTooltipTokens(List<TooltipTokenEntry> tokens, AbilityMod modAsBase)
    {
        base.AddSpecificTooltipTokens(tokens, modAsBase);
        AddTokenInt(tokens, "SuperheatDuration", string.Empty, m_superheatDuration);
        AddTokenInt(tokens, "IgniteExtraDamageIfSuperheated", string.Empty, m_igniteExtraDamageIfSuperheated);
    }

    public int GetSuperheatDuration() // TODO FIREBORG does not affect sequence-holding effect, does not increase cooldown
    {
        return m_abilityMod != null
            ? m_abilityMod.m_superheatDurationMod.GetModifiedValue(m_superheatDuration)
            : m_superheatDuration;
    }

    public int GetIgniteExtraDamageIfSuperheated() // TODO FIREBORG unused, always 0
    {
        return m_abilityMod != null
            ? m_abilityMod.m_igniteExtraDamageIfSuperheatedMod.GetModifiedValue(m_igniteExtraDamageIfSuperheated)
            : m_igniteExtraDamageIfSuperheated;
    }

    protected override void GenModImpl_SetModRef(AbilityMod abilityMod)
    {
        m_abilityMod = abilityMod as AbilityMod_FireborgSuperheat;
    }

    protected override void GenModImpl_ClearModRef()
    {
        m_abilityMod = null;
    }
    
#if SERVER
    public override void Run(List<AbilityTarget> targets, ActorData caster, ServerAbilityUtils.AbilityRunData additionalData)
    {
        base.Run(targets, caster, additionalData);

        m_syncComp.Networkm_superheatLastCastTurn = GameFlowData.Get().CurrentTurn;
    }
#endif
}