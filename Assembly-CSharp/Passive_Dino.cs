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
    }

    public override void OnTurnEnd()
    {
        base.OnTurnEnd();

        if (m_syncComp != null && m_primaryAbility != null)
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