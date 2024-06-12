using System.Collections.Generic;
using System.Runtime.InteropServices;
using AbilityContextNamespace;
using UnityEngine;
using UnityEngine.Networking;

public class Fireborg_SyncComponent : NetworkBehaviour
{
    [Separator("Ignited Effect")]
    public StandardActorEffectData m_ignitedEffectData;
    public int m_ignitedTriggerDamage = 5;
    public StandardEffectInfo m_ignitedTriggerEffect;
    public int m_ignitedTriggerEnergyOnCaster;
    [Separator("Ground Fire Effect")]
    public int m_groundFireDamageNormal = 6;
    public int m_groundFireDamageSuperheated = 8;
    public StandardEffectInfo m_groundFireEffect;
    public bool m_groundFireAddsIgniteIfSuperheated = true;
    [Separator("Sequences")]
    public GameObject m_groundFirePerSquareSeqPrefab;
    public GameObject m_groundFireOnHitSeqPrefab;
    [Header("-- Superheated versions")]
    public GameObject m_superheatedGroundFireSquareSeqPrefab;

    [SyncVar]
    internal int m_superheatLastCastTurn;

    internal SyncListUInt m_actorsInGroundFireOnTurnStart = new SyncListUInt();
    private AbilityData m_abilityData;
    private FireborgSuperheat m_superheatAbility;
    private AbilityData.ActionType m_superheatActionType = AbilityData.ActionType.INVALID_ACTION;
    public static ContextNameKeyPair s_cvarSuperheated = new ContextNameKeyPair("Superheated");
    private HashSet<ActorData> m_ignitedActorsThisTurn = new HashSet<ActorData>();
    private static int kListm_actorsInGroundFireOnTurnStart = 1427115255;

    public int Networkm_superheatLastCastTurn
    {
        get => m_superheatLastCastTurn;
        [param: In]
        set => SetSyncVar(value, ref m_superheatLastCastTurn, 1u);
    }

    static Fireborg_SyncComponent()
    {
        RegisterSyncListDelegate(
            typeof(Fireborg_SyncComponent),
            kListm_actorsInGroundFireOnTurnStart,
            InvokeSyncListm_actorsInGroundFireOnTurnStart);
        NetworkCRC.RegisterBehaviour("Fireborg_SyncComponent", 0);
    }

    public void ResetIgnitedActorsTrackingThisTurn()
    {
        m_ignitedActorsThisTurn.Clear();
    }

    private void Start()
    {
        m_abilityData = GetComponent<AbilityData>();
        m_superheatAbility = m_abilityData.GetAbilityOfType<FireborgSuperheat>();
        if (m_superheatAbility != null)
        {
            m_superheatActionType = m_abilityData.GetActionTypeOfAbility(m_superheatAbility);
        }
    }

    public static string GetSuperheatedCvarUsage()
    {
        return ContextVars.GetContextUsageStr(
            s_cvarSuperheated.GetName(),
            "1 if caster is in Superheated mode, 0 otherwise",
            false);
    }

    public bool InSuperheatMode()
    {
        if (m_superheatAbility == null)
        {
            return false;
        }

        if (m_superheatLastCastTurn > 0
            && GameFlowData.Get().CurrentTurn < m_superheatLastCastTurn + m_superheatAbility.GetSuperheatDuration())
        {
            return true;
        }

        return m_abilityData.HasQueuedAction(m_superheatActionType);
    }

    public void SetSuperheatedContextVar(ContextVars abilityContext)
    {
        bool flag = InSuperheatMode();
        abilityContext.SetValue(s_cvarSuperheated.GetKey(), flag ? 1 : 0);
    }

    public void AddGroundFireTargetingNumber(ActorData target, ActorData caster, TargetingNumberUpdateScratch results)
    {
        if (target.GetTeam() == caster.GetTeam())
        {
            return;
        }

        int groundFireDamage = InSuperheatMode()
            ? m_groundFireDamageSuperheated
            : m_groundFireDamageNormal;
        if (groundFireDamage > 0)
        {
            results.m_damage += groundFireDamage;
        }
    }

    public string GetTargetPreviewAccessoryString(
        AbilityTooltipSymbol symbolType,
        Ability ability,
        ActorData targetActor,
        ActorData caster)
    {
        if (symbolType != AbilityTooltipSymbol.Damage)
        {
            return null;
        }

        int groundFireDamage = InSuperheatMode()
            ? m_groundFireDamageSuperheated
            : m_groundFireDamageNormal;
        if (groundFireDamage > 0)
        {
            return "\n+ " + AbilityUtils.CalculateDamageForTargeter(
                caster,
                targetActor,
                ability,
                groundFireDamage,
                false);
        }

        return null;
    }

    private void UNetVersion()
    {
    }

    protected static void InvokeSyncListm_actorsInGroundFireOnTurnStart(NetworkBehaviour obj, NetworkReader reader)
    {
        if (!NetworkClient.active)
        {
            Debug.LogError("SyncList m_actorsInGroundFireOnTurnStart called on server.");
            return;
        }

        ((Fireborg_SyncComponent)obj).m_actorsInGroundFireOnTurnStart.HandleMsg(reader);
    }

    private void Awake()
    {
        m_actorsInGroundFireOnTurnStart.InitializeBehaviour(this, kListm_actorsInGroundFireOnTurnStart);
    }

    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
        if (forceAll)
        {
            writer.WritePackedUInt32((uint)m_superheatLastCastTurn);
            SyncListUInt.WriteInstance(writer, m_actorsInGroundFireOnTurnStart);
            return true;
        }

        bool flag = false;
        if ((syncVarDirtyBits & 1) != 0)
        {
            if (!flag)
            {
                writer.WritePackedUInt32(syncVarDirtyBits);
                flag = true;
            }

            writer.WritePackedUInt32((uint)m_superheatLastCastTurn);
        }

        if ((syncVarDirtyBits & 2) != 0)
        {
            if (!flag)
            {
                writer.WritePackedUInt32(syncVarDirtyBits);
                flag = true;
            }

            SyncListUInt.WriteInstance(writer, m_actorsInGroundFireOnTurnStart);
        }

        if (!flag)
        {
            writer.WritePackedUInt32(syncVarDirtyBits);
        }

        return flag;
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        if (initialState)
        {
            m_superheatLastCastTurn = (int)reader.ReadPackedUInt32();
            SyncListUInt.ReadReference(reader, m_actorsInGroundFireOnTurnStart);
            return;
        }

        int num = (int)reader.ReadPackedUInt32();
        if ((num & 1) != 0)
        {
            m_superheatLastCastTurn = (int)reader.ReadPackedUInt32();
        }

        if ((num & 2) != 0)
        {
            SyncListUInt.ReadReference(reader, m_actorsInGroundFireOnTurnStart);
        }
    }
}