using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

#if SERVER
// custom
public class IceborgNovaCoreEffect : StandardActorEffect
{
    private int m_damageAmount;
    private GameObject m_explosionSequencePrefab;
    private Iceborg_SyncComponent m_syncComp;
    private Passive_Iceborg m_passive;
    private bool m_explosionDone;
    private bool m_canExplodeThisTurn;
    private bool m_wasHitThisTurn;
    private bool m_wasHitThisTurn_fake;
    private int m_extraEnergyPerExplosion;

    public IceborgNovaCoreEffect(
        EffectSource parent,
        BoardSquare targetSquare,
        ActorData target,
        ActorData caster,
        StandardActorEffectData effectData,
        int damageOnDetonation,
        GameObject explosionSequencePrefab,
        int extraEnergyPerExplosion)
        : base(parent, targetSquare, target, caster, effectData)
    {
        m_effectName = "Iceborg Nova Core Target Effect";
        m_damageAmount = damageOnDetonation;
        m_explosionSequencePrefab = explosionSequencePrefab;
        m_syncComp = parent.Ability.GetComponent<Iceborg_SyncComponent>();
        m_passive = parent.Ability.GetComponent<Passive_Iceborg>();
        m_canExplodeThisTurn = false; // to trigger the next turn
        m_time.duration = effectData.m_duration;
        m_extraEnergyPerExplosion = extraEnergyPerExplosion;
    }

    public override void OnStart()
    {
        base.OnStart();
        if (m_syncComp != null)
        {
            m_syncComp.AddNovaCoreActorIndex(TargetActorIndex);
        }
    }

    public override void OnTurnStart()
    {
        Log.Info("IcebordNovaCoreEffect -> OnTurnStart");
        base.OnTurnStart();
        m_canExplodeThisTurn = true;
        m_explosionDone = false;
        m_wasHitThisTurn = false;
        m_wasHitThisTurn_fake = false;
    }

    /*public override void OnAbilityPhaseStart(AbilityPriority phase)
        {
            if (phase == AbilityPriority.Prep_Defense)
            {
                m_wasHitThisTurn = false;
                m_wasHitThisTurn_fake = false;
                m_wasHitByNonCasterAllyThisTurn = false;
                m_wasHitByNonCasterAllyThisTurn_fake = false;
            }
            base.OnAbilityPhaseStart(phase);
        }*/

    /*
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            if (GetWasHitThisTurn(true) && !m_explosionDone)
            {
                m_explosionDone = true;
            }

            // custom
            bool applyCdr = m_explosionReduceCooldownOnlyIfHitByAlly
                ? GetWasHitByNonCasterAllyThisTurn(true)
                : GetWasHitThisTurn(true);
            if (applyCdr)
            {
                m_passive.SetPendingCdrDaggerTrigger(m_explosionCooldownReduction, AbilityData.ActionType.ABILITY_3);
            }
            // end custom
        }*/

    // custom
    public override bool ShouldEndEarly()
    {
        Log.Info("IceborgNovaCoreEffect -> shouldEndEarly");
        bool result = base.ShouldEndEarly() || m_explosionDone;
        return result;
    }

    public override List<ServerClientUtils.SequenceStartData> GetEffectHitSeqDataList()
    {
        return new List<ServerClientUtils.SequenceStartData>();
    }

    public override void GatherResultsInResponseToActorHit(
        ActorHitResults incomingHit,
        ref List<AbilityResults_Reaction> reactions,
        bool isReal)
    {
        if (!incomingHit.HasDamage
            || m_explosionDone
            || !m_canExplodeThisTurn)
        {
            return;
        }

        if (!GetWasHitThisTurn(isReal))
        {
            SetWasHitThisTurn(true, isReal);
            AbilityResults_Reaction abilityResults_Reaction = new AbilityResults_Reaction();
            ActorHitParameters hitParameters = new ActorHitParameters(Target, Target.GetFreePos());
            ActorHitResults actorHitResults = new ActorHitResults(m_damageAmount, HitActionType.Damage, (StandardEffectInfo)null, hitParameters);
            actorHitResults.AddTechPointGainOnCaster(m_syncComp.m_delayedAoeEnergyPerExplosion + m_extraEnergyPerExplosion);
            actorHitResults.CanBeReactedTo = false;
            // rogues
            // actorHitResults.ModifyDamageCoeff(m_damageAmount, m_damageAmount);
            if (m_data.m_sequencePrefabs != null && m_data.m_sequencePrefabs.Length != 0)
            {
                for (int i = 0; i < m_data.m_sequencePrefabs.Length; i++)
                {
                    actorHitResults.AddEffectSequenceToEnd(m_data.m_sequencePrefabs[i], m_guid);
                }
            }
            abilityResults_Reaction.SetupGameplayData(
                this,
                actorHitResults,
                incomingHit.m_reactionDepth,
                null,
                isReal,
                incomingHit);
            abilityResults_Reaction.SetupSequenceData(
                m_explosionSequencePrefab,
                Target.GetCurrentBoardSquare(),
                SequenceSource);
            abilityResults_Reaction.SetExtraFlag(ClientReactionResults.ExtraFlags.ClientExecuteOnFirstDamagingHit);
            reactions.Add(abilityResults_Reaction);
        }
        //if (incomingHit.m_hitParameters.Caster != Caster)
        //{
        //    SetWasHitByNonCasterAllyThisTurn(true, isReal);
        //}
    }

    private bool GetWasHitThisTurn(bool isReal)
    {
        return isReal
            ? m_wasHitThisTurn
            : m_wasHitThisTurn_fake;
    }

    private void SetWasHitThisTurn(bool wasHitThisTurn, bool isReal)
    {
        if (isReal)
        {
            m_wasHitThisTurn = wasHitThisTurn;
        }
        else
        {
            m_wasHitThisTurn_fake = wasHitThisTurn;
        }
    }

    /*private bool GetWasHitByNonCasterAllyThisTurn(bool isReal)
        {
            return isReal
                ? m_wasHitByNonCasterAllyThisTurn
                : m_wasHitByNonCasterAllyThisTurn_fake;
        }

        private void SetWasHitByNonCasterAllyThisTurn(bool wasHitByNonCasterAllyThisTurn, bool isReal)
        {
            if (isReal)
            {
                m_wasHitByNonCasterAllyThisTurn = wasHitByNonCasterAllyThisTurn;
            }
            else
            {
                m_wasHitByNonCasterAllyThisTurn_fake = wasHitByNonCasterAllyThisTurn;
            }
        }
        */

    // custom
    public override void OnEnd()
    {
        Log.Info("IceborgNovaCoreEffect -> OnEnd");
        base.OnEnd();
        m_syncComp.RemoveNovaCoreActorIndex(TargetActorIndex);
        Log.Info(new StackTrace().ToString());
    }
}
#endif