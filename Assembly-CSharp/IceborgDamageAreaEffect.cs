// ROGUES
// SERVER
using UnityEngine;

#if SERVER
// custom
public class IceborgDamageAreaEffect : StandardGroundEffect
{
    int m_damageChangePerTurn;
    int m_extraDamageOnInitialCast;
    int m_minDamage;
    
    Iceborg_SyncComponent m_syncComp;

    public IceborgDamageAreaEffect(
        EffectSource parent,
        BoardSquare targetSquare,
        Vector3 shapeFreePos,
        ActorData target,
        ActorData caster,
        GroundEffectField fieldInfo,
        GameObject fieldRemoveOnMoveSeqPrefab,
        int damageChangePerTurn,
        int extraDamageOnInitialCast,
        int minDamage)
        : base(parent, targetSquare, shapeFreePos, target, caster, fieldInfo)
    {
        m_damageChangePerTurn = damageChangePerTurn;
        m_extraDamageOnInitialCast = extraDamageOnInitialCast;
        m_minDamage = minDamage;
        m_syncComp = parent.Ability.GetComponent<Iceborg_SyncComponent>();
    }

    public override void SetupActorHitResults(ref ActorHitResults actorHitRes, BoardSquare targetSquare)
    {
        base.SetupActorHitResults(ref actorHitRes, targetSquare);

        if(m_syncComp.GetTurnsSinceInitialCast() == 0)
        {
            actorHitRes.AddBaseDamage(m_extraDamageOnInitialCast);
        }
        else
        {            
            actorHitRes.AddBaseDamage(m_damageChangePerTurn * m_syncComp.GetTurnsSinceInitialCast());
            if(actorHitRes.BaseDamage < m_minDamage)
            {
                actorHitRes.SetBaseDamage(m_minDamage);
            }
        }
    }

    
    /*public override List<ServerClientUtils.SequenceStartData> GetEffectStartSeqDataList()
    {
        //base.GetEffectStartSeqDataList();
        List<ServerClientUtils.SequenceStartData> seqDataList = new List<ServerClientUtils.SequenceStartData>();//base.GetEffectStartSeqDataList();

        //seqDataList.Add(new ServerClientUtils.SequenceStartData(m_fieldRemoveOnMoveSeqPrefab, GetShapeCenter(), null, Caster, SequenceSource));

        //ServerClientUtils.SequenceStartData persistentSequenceStartData = new ServerClientUtils.SequenceStartData(m_fieldInfo.persistentSequencePrefab, GetShapeCenter(), Quaternion.identity, null, Caster, SequenceSource);
        
        //seqDataList.Add(persistentSequenceStartData);

        return seqDataList;
    }// */
    

    /*public override List<ServerClientUtils.SequenceStartData> GetEffectHitSeqDataList()
    {
        var sequences = base.GetEffectHitSeqDataList();
        //var persistentSequenceStartData = new ServerClientUtils.SequenceStartData(m_fieldInfo.persistentSequencePrefab, GetShapeCenter(), Quaternion.identity, null, Caster, SequenceSource);
        //persistentSequenceStartData.SetRemoveAtEndOfTurn(false);
        
        //sequences.Add(persistentSequenceStartData);
        //sequences.Add(new ServerClientUtils.SequenceStartData(m_fieldRemoveOnMoveSeqPrefab, GetShapeCenter(), null, Caster, SequenceSource));

        return sequences;
        
    }// */

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        m_syncComp.Networkm_damageAreaCanMoveThisTurn = true;
        m_shapeFreePos = m_syncComp.m_damageAreaFreePos;
    }

    public override void OnEnd()
    {
        base.OnEnd();
        m_syncComp.Networkm_damageAreaCanMoveThisTurn = false;
    }
}
#endif
