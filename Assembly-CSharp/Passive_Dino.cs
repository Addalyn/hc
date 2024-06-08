// ROGUES
// SERVER
using System.Collections.Generic;

// empty in reactor, missing in rogues. custom
public class Passive_Dino : Passive
{
#if SERVER
    private Dino_SyncComponent m_syncComp;
    private DinoLayerCones m_primaryAbility;

    private int m_pendingPowerLevel = -1;

    protected override void OnStartup()
    {
        base.OnStartup();
        m_syncComp = GetComponent<Dino_SyncComponent>();
        m_primaryAbility = Owner.GetAbilityData().GetAbilityOfType(typeof(DinoLayerCones)) as DinoLayerCones;
        Owner.OnKnockbackHitExecutedDelegate += OnKnockbackMovementHitExecuted;
    }

    private void OnDestroy()
    {
        Owner.OnKnockbackHitExecutedDelegate -= OnKnockbackMovementHitExecuted;
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        
        if (m_syncComp != null)
        {
            bool isInReadyStance = IsInReadyStance();
            if (isInReadyStance)
            {
                m_syncComp.CallRpcSetDashReadyStanceAnimParams(1, true);
            }
            m_syncComp.CallRpcResetDashOrShieldTargeter(isInReadyStance);
        }
    }

    private bool IsInReadyStance()
    {
        return m_syncComp != null && m_syncComp.m_dashOrShieldInReadyStance;
    }

    public override void OnAbilityPhaseStart(AbilityPriority phase)
    {
        base.OnAbilityPhaseStart(phase);

        if (phase == AbilityPriority.Prep_Offense && IsInReadyStance())
        {
            m_syncComp.CallRpcSetDashReadyStanceAnimParams(0, false);
        }
    }

    public override void OnTurnEnd()
    {
        base.OnTurnEnd();

        if (m_syncComp != null)
        {
            if (m_primaryAbility != null)
            {
                if (m_pendingPowerLevel >= 0)
                {
                    m_syncComp.Networkm_layerConePowerLevel = (short)m_pendingPowerLevel;
                }
                else
                {
                    m_syncComp.Networkm_layerConePowerLevel++;
                }

                m_pendingPowerLevel = -1;
            }

            m_syncComp.Networkm_dashOrShieldInReadyStance =
                m_syncComp.m_dashOrShieldLastCastTurn == GameFlowData.Get().CurrentTurn;
        }
    }
    
    private void OnKnockbackMovementHitExecuted(ActorData target, ActorHitResults hitRes)
    {
        if (hitRes.HasDamage && ServerActionBuffer.Get().HasStoredAbilityRequestOfType(Owner, typeof(DinoTargetedKnockback)))
        {
            Owner.GetFreelancerStats().AddToValueOfStat(
                FreelancerStats.DinoStats.KnockbackDamageOnCastAndKnockback,
                hitRes.FinalDamage);
        }
    }

    public override void OnMiscHitEventUpdate(List<MiscHitEventPassiveUpdateParams> updateParams)
    {
        foreach (MiscHitEventPassiveUpdateParams param in updateParams)
        {
            if (param is SetPowerLevelParam powerLevelParam)
            {
                m_pendingPowerLevel = powerLevelParam.m_powerLevel;
            }
        }
    }

    public class SetPowerLevelParam : MiscHitEventPassiveUpdateParams
    {
        public readonly int m_powerLevel;

        public SetPowerLevelParam(int powerLevel)
        {
            m_powerLevel = powerLevel;
        }
    }
#endif
}