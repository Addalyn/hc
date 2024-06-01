using System.Linq;

// TODO ICEBORG bot
// empty in reactor
public class Passive_Iceborg : Passive
{
    // custom
#if SERVER
    Iceborg_SyncComponent m_syncComp;
    private IceborgSelfShield m_shieldAbility;
    private bool m_pendingShieldDepletionAnimation;
    
    protected override void OnStartup()
    {
        base.OnStartup();

        m_syncComp = Owner.GetComponent<Iceborg_SyncComponent>();
        m_shieldAbility = Owner.GetAbilityData().GetAbilityOfType(typeof(IceborgSelfShield)) as IceborgSelfShield;
    }
    
    public override void OnTurnEnd()
    {
        base.OnTurnEnd();
        
        AbilityData abilityData = Owner.GetAbilityData();
        if (abilityData != null && abilityData.HasQueuedAbilityOfType(typeof(IceborgSelfShield)))
        {
            m_pendingShieldDepletionAnimation = true;
            
            if (Owner.AbsorbPoints == 0)
            {
                int shieldOnNextTurnIfDepleted = m_shieldAbility.GetShieldOnNextTurnIfDepleted();
                if (shieldOnNextTurnIfDepleted > 0)
                {
                    Log.Info($"ICEBORG PASSIVE {Owner.m_displayName} applying additional shielding");
                    StandardActorEffect shieldEffect = GenericAbility_Container.CreateShieldEffect(
                        m_shieldAbility,
                        Owner,
                        shieldOnNextTurnIfDepleted,
                        1);
                
                    ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(Owner, Owner.GetFreePos()));
                    actorHitResults.AddEffect(shieldEffect);
                    MovementResults.SetupAndExecuteAbilityResultsOutsideResolution(
                        Owner,
                        Owner,
                        actorHitResults,
                        m_shieldAbility);
                }
                else
                {
                    Log.Info($"ICEBORG PASSIVE {Owner.m_displayName} has no additional shielding");
                }
            }
            else
            {
                Log.Info($"ICEBORG PASSIVE {Owner.m_displayName}'s shield is not depleted: {Owner.AbsorbPoints}");
            }
        }
        else
        {
            Log.Info($"ICEBORG PASSIVE {Owner.m_displayName} did not use shield this turn");
        }

        // these will be updated in corresponding effects if needed
        m_syncComp.Networkm_damageAreaCanMoveThisTurn = false;
        m_syncComp.ClearNovaCoreActorIndex();
    }

    public override void OnTurnStart()
    {
        base.OnTurnStart();

        m_syncComp.m_actorsReceivingNovaCoreThisTurn.Clear();
        m_syncComp.m_actorsReceivingNovaCoreThisTurn_Fake.Clear();

        m_syncComp.Networkm_selfShieldLowHealthOnTurnStart = !Owner.IsDead() && m_shieldAbility.IsCasterLowHealth(Owner);
        
        if (m_pendingShieldDepletionAnimation)
        {
            if (Owner.IsDead())
            {
                m_pendingShieldDepletionAnimation = false;
            }
            else
            {
                Effect activeEffect = ServerEffectManager.Get()
                    .GetEffectsOnTargetByCaster(Owner, Owner, typeof(StandardActorEffect))
                    .FirstOrDefault(eff => eff.Parent.Ability is IceborgSelfShield);
                if (activeEffect is null)
                {
                    // play shield depletion animation
                    MovementResults.SetupAndExecuteAbilityResultsOutsideResolution(
                        Owner,
                        Owner,
                        new ActorHitResults(new ActorHitParameters(Owner, Owner.GetFreePos())),
                        m_shieldAbility,
                        true,
                        m_shieldAbility.m_shieldRemoveSeqPrefab);
                    m_pendingShieldDepletionAnimation = false;
                }
            }
        }
    }
#endif
}
