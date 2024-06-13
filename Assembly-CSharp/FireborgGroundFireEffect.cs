// ROGUES
// SERVER
using System.Collections.Generic;

#if SERVER
// custom
public class FireborgGroundFireEffect : StandardMultiAreaGroundEffect
{
    private readonly bool m_groundFireAddsIgnite;
    private readonly Fireborg_SyncComponent m_syncComp;
    
    public FireborgGroundFireEffect(
        EffectSource parent,
        List<GroundAreaInfo> areaInfoList,
        ActorData caster,
        GroundEffectField fieldInfo,
        bool groundFireAddsIgnite)
        : base(parent, areaInfoList, caster, fieldInfo)
    {
        m_groundFireAddsIgnite = groundFireAddsIgnite;
        m_syncComp = parent.Ability.GetComponent<Fireborg_SyncComponent>();
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        
        foreach (ActorData actorData in GetActorsInShape())
        {
            uint actorIndex = (uint)actorData.ActorIndex;
            if (!m_syncComp.m_actorsInGroundFireOnTurnStart.Contains(actorIndex))
            {
                m_syncComp.m_actorsInGroundFireOnTurnStart.Add(actorIndex);
            }
        }
    }

    protected override void ProcessActorHit(ActorHitResults actorHitResults, bool isReal)
    {
        ActorData hitActor = actorHitResults.m_hitParameters.Target;
        actorHitResults.SetIgnoreTechpointInteractionForHit(true);
        if (hitActor.GetTeam() != Caster.GetTeam() && m_groundFireAddsIgnite)
        {
            FireborgIgnitedEffect fireborgIgnitedEffect = m_syncComp.MakeIgnitedEffect(Parent, Caster, hitActor);
            if (fireborgIgnitedEffect != null)
            {
                actorHitResults.AddEffect(fireborgIgnitedEffect);
            }
        }
        
        foreach (Effect effect in ServerEffectManager.Get().GetWorldEffectsByCaster(Caster, typeof(FireborgGroundFireEffect)))
        {
            if (effect is FireborgGroundFireEffect fireborgGroundFireEffect)
            {
                fireborgGroundFireEffect.AddActorHitThisTurn(hitActor, isReal);
            }
        }
        m_syncComp.GetActorsHitByGroundFireThisTurn(isReal).Add(hitActor);
    }
}
#endif