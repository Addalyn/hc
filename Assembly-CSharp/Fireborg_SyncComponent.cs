using System.Collections.Generic;
using System.Linq;
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
    
#if SERVER
    // custom
    internal HashSet<ActorData> m_actorsIgnitedThisTurn = new HashSet<ActorData>();
    // custom
    internal HashSet<ActorData> m_actorsIgnitedThisTurn_Fake = new HashSet<ActorData>();
    // custom
    internal HashSet<ActorData> m_actorsHitByGroundFireThisTurn = new HashSet<ActorData>();
    // custom
    internal HashSet<ActorData> m_actorsHitByGroundFireThisTurn_Fake = new HashSet<ActorData>();
#endif

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
    
#if SERVER
    // custom
    public FireborgIgnitedEffect MakeIgnitedEffect(EffectSource parent, ActorData caster, ActorData target)
    {
        HashSet<ActorData> set = ServerActionBuffer.Get().GatheringFakeResults
            ? m_actorsIgnitedThisTurn_Fake 
            : m_actorsIgnitedThisTurn;
        
        if (set.Add(target))
        {
            return new FireborgIgnitedEffect(
                parent,
                target,
                caster,
                m_ignitedEffectData,
                m_ignitedTriggerDamage,
                m_ignitedTriggerEffect,
                m_ignitedTriggerEnergyOnCaster);
        }

        return null;
    }
    
    // custom
    public FireborgGroundFireEffect MakeGroundFireEffect(
        EffectSource parent,
        ActorData caster,
        List<BoardSquare> affectedSquares)
    {
        return new FireborgGroundFireEffect(
            parent,
            affectedSquares
                .Select(s => new StandardMultiAreaGroundEffect.GroundAreaInfo(s, s.ToVector3(), AbilityAreaShape.SingleSquare))
                .ToList(),
            caster,
            new GroundEffectField
            {
                duration = 1,
                shape = AbilityAreaShape.SingleSquare,
                damageAmount = InSuperheatMode()
                    ? m_groundFireDamageSuperheated
                    : m_groundFireDamageNormal,
                effectOnEnemies = m_groundFireEffect,
                effectOnAllies = new StandardEffectInfo
                {
                    m_applyEffect = false,
                    m_effectData = new StandardActorEffectData()
                },
                perSquareSequences = true,
                persistentSequencePrefab = InSuperheatMode() && m_superheatedGroundFireSquareSeqPrefab != null
                    ? m_superheatedGroundFireSquareSeqPrefab
                    : m_groundFirePerSquareSeqPrefab,
                enemyHitSequencePrefab = m_groundFireOnHitSeqPrefab
            },
            InSuperheatMode() && m_groundFireAddsIgniteIfSuperheated);
    }
    
    // custom
    public PositionHitResults MakeGroundFireEffectResults(
        Ability ability,
        ActorData caster,
        List<BoardSquare> affectedSquares,
        Vector3 posForHit,
        int duration, // TODO FIREBORG
        bool isReal,
        out FireborgGroundFireEffect effect)
    {
        affectedSquares = affectedSquares.Where(s => s.IsValidForGameplay()).ToList();
        effect = MakeGroundFireEffect(ability.AsEffectSource(), caster, affectedSquares);
        effect.AddToActorsHitThisTurn(GetActorsHitByGroundFireThisTurn(isReal).ToList());
        
        PositionHitResults posHitResults = new PositionHitResults(new PositionHitParameters(posForHit));
        EffectResults effectResults = new EffectResults(effect, caster, isReal);
        effect.GatherEffectResults(ref effectResults, isReal);
        SequenceSource sequenceSource = new SequenceSource(null, null);
        foreach (ActorHitResults hitResults in effectResults.m_actorToHitResults.Values)
        {
            ActorData hitActor = hitResults.m_hitParameters.Target;
            MovementResults movementResults = new MovementResults(MovementStage.INVALID);
            movementResults.SetupTriggerData(caster, null);
            movementResults.SetupGameplayDataForAbility(ability, caster);
            movementResults.SetupSequenceData(null, hitActor.GetCurrentBoardSquare(), sequenceSource);
            movementResults.AddActorHitResultsForReaction(hitResults);
            posHitResults.AddReactionOnPositionHit(movementResults);
        }
        
        return posHitResults;
    }
    
    // custom
    public HashSet<ActorData> GetActorsHitByGroundFireThisTurn(bool isReal)
    {
        return isReal ? m_actorsHitByGroundFireThisTurn : m_actorsHitByGroundFireThisTurn_Fake;
    }
#endif
}