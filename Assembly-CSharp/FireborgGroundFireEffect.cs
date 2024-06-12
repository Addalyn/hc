using System.Collections.Generic;

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

    protected override void ProcessActorHit(ActorHitResults actorHitResults)
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
    }
}