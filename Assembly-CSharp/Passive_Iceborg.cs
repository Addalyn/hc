// empty in reactor
public class Passive_Iceborg : Passive
{
#if SERVER
    Iceborg_SyncComponent m_syncComp;
    protected override void OnStartup()
    {
        base.OnStartup();

        m_syncComp = Owner.GetComponent<Iceborg_SyncComponent>();
    }

    /*public override void OnTurnStart()
    {
        base.OnTurnStart();

        foreach(ActorData actor in m_syncComp.GetActorsWithNovaCore())
        {
            Log.Info("Adding NovaCore visual effect to " + actor.DisplayName);
            StandardEffectInfo effectInfo = new StandardEffectInfo();
            effectInfo.m_effectData = m_syncComp.GetNovaCoreEffectData();
            ServerEffectManager.Get().ApplyEffect(effectInfo.CreateEffect(AsEffectSource(), actor, Owner));
        }
    }*/
#endif
}
