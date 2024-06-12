// ROGUES
// SERVER

// empty in reactor, missing in rogues. custom
public class Passive_Fireborg : Passive
{
#if SERVER
    private Fireborg_SyncComponent m_syncComp;
    
    protected override void OnStartup()
    {
        base.OnStartup();

        m_syncComp = Owner.GetComponent<Fireborg_SyncComponent>();
    }
    
    public override void OnTurnStart()
    {
        base.OnTurnStart();
        
        m_syncComp.m_actorsIgnitedThisTurn.Clear();
        m_syncComp.m_actorsIgnitedThisTurn_Fake.Clear();
    }
#endif
}
