using System.Collections.Generic;
using UnityEngine;

public class ClientReactionResults
{
    public enum ExtraFlags
    {
        None = 0,
        ClientExecuteOnFirstDamagingHit = 1,
        TriggerOnFirstDamageIfReactOnAttacker = 2,
        TriggerOnFirstDamageOnReactionCaster = 4
    }

    private ClientEffectResults m_effectResults;
    private List<ServerClientUtils.SequenceStartData> m_seqStartDataList;
    private byte m_extraFlags;
    private bool m_playedReaction;

    public ClientReactionResults(
        ClientEffectResults effectResults,
        List<ServerClientUtils.SequenceStartData> seqStartDataList,
        byte extraFlags)
    {
        m_effectResults = effectResults;
        m_seqStartDataList = seqStartDataList;
        m_extraFlags = extraFlags;
    }

    public bool HasSequencesToStart()
    {
        if (m_seqStartDataList == null || m_seqStartDataList.Count == 0)
        {
            return false;
        }

        foreach (ServerClientUtils.SequenceStartData seqStartData in m_seqStartDataList)
        {
            if (seqStartData != null && seqStartData.HasSequencePrefab())
            {
                return true;
            }
        }

        return false;
    }

    public bool HasUnexecutedReactionOnActor(ActorData actor)
    {
        return m_effectResults.HasUnexecutedHitOnActor(actor);
    }

    public bool ReactionHitsDone()
    {
        return PlayedReaction() && m_effectResults.DoneHitting();
    }

    public bool PlayedReaction()
    {
        return m_playedReaction;
    }

    public void PlayReaction()
    {
        if (m_playedReaction)
        {
            return;
        }

        m_playedReaction = true;
        if (HasSequencesToStart())
        {
            foreach (ServerClientUtils.SequenceStartData current in m_seqStartDataList)
            {
                current.CreateSequencesFromData(OnReactionHitActor, OnReactionHitPosition);
            }
        }
        else
        {
            if (ClientAbilityResults.DebugTraceOn)
            {
                Log.Warning(
                    ClientAbilityResults.s_clientHitResultHeader
                    + GetDebugDescription()
                    + ": no Sequence to start, executing results directly");
            }

            m_effectResults.RunClientEffectHits();
        }
    }

    internal void OnReactionHitActor(ActorData target)
    {
        m_effectResults.OnEffectHitActor(target);
    }

    internal void OnReactionHitPosition(Vector3 position)
    {
        m_effectResults.OnEffectHitPosition(position);
    }

    internal byte GetExtraFlags()
    {
        return m_extraFlags;
    }

    internal ActorData GetCaster()
    {
        return m_effectResults.GetCaster();
    }

    internal Dictionary<ActorData, ClientActorHitResults> GetActorHitResults()
    {
        return m_effectResults.GetActorHitResults();
    }

    internal Dictionary<Vector3, ClientPositionHitResults> GetPosHitResults()
    {
        return m_effectResults.GetPosHitResults();
    }

    internal string GetDebugDescription()
    {
        return m_effectResults.GetDebugDescription();
    }
}