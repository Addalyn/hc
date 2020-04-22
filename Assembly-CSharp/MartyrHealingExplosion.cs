using System.Collections.Generic;
using UnityEngine;

public class MartyrHealingExplosion : MartyrLaserBase
{
	[Header("-- Targeting")]
	public LaserTargetingInfo m_laserInfo;

	public StandardEffectInfo m_laserHitEffect;

	public float m_explosionRadius = 2.5f;

	public bool m_laserCanHitAllies;

	public bool m_laserCanHitEnemies = true;

	public bool m_forceMaxLaserDistance = true;

	public bool m_explodeOnlyOnLaserHit = true;

	public bool m_explosionCanHitCaster;

	[Header("-- Damage, Healing & Crystal Bonuses")]
	public int m_baseLaserDamage = 20;

	public int m_baseExplosionHealing = 15;

	public int m_additionalDamagePerCrystalSpent;

	public int m_additionalHealingPerCrystalSpent;

	public float m_additionalRadiusPerCrystalSpent = 0.25f;

	public List<MartyrBasicAttackThreshold> m_thresholdBasedCrystalBonuses;

	[Header("-- Sequences")]
	public GameObject m_projectileSequence;

	private Martyr_SyncComponent m_syncComponent;

	private LaserTargetingInfo m_cachedLaserInfo;

	private StandardEffectInfo m_cachedLaserHitEffect;

	private void Start()
	{
		if (m_abilityName == "Base Ability")
		{
			while (true)
			{
				switch (3)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			m_abilityName = "Martyr Healing Explosion";
		}
		m_syncComponent = GetComponent<Martyr_SyncComponent>();
		SetCachedFields();
		SetupTargeter();
	}

	protected override Martyr_SyncComponent GetSyncComponent()
	{
		return m_syncComponent;
	}

	protected void SetupTargeter()
	{
		AbilityUtil_Targeter_MartyrLaser abilityUtil_Targeter_MartyrLaser = new AbilityUtil_Targeter_MartyrLaser(this, GetCurrentLaserWidth(), GetCurrentLaserRange(), GetCurrentLaserPenetrateLoS(), GetCurrentLaserMaxTargets(), m_laserCanHitEnemies, m_laserCanHitAllies, false, !m_forceMaxLaserDistance, m_explodeOnlyOnLaserHit, GetCurrentExplosionRadius(), GetCurrentInnerExplosionRadius(), true, false, m_explosionCanHitCaster);
		abilityUtil_Targeter_MartyrLaser.m_delegateLaserWidth = base.GetCurrentLaserWidth;
		abilityUtil_Targeter_MartyrLaser.m_delegateLaserRange = base.GetCurrentLaserRange;
		abilityUtil_Targeter_MartyrLaser.m_delegatePenetrateLos = base.GetCurrentLaserPenetrateLoS;
		abilityUtil_Targeter_MartyrLaser.m_delegateMaxTargets = base.GetCurrentLaserMaxTargets;
		abilityUtil_Targeter_MartyrLaser.m_delegateConeRadius = GetCurrentExplosionRadius;
		abilityUtil_Targeter_MartyrLaser.m_delegateInnerConeRadius = GetCurrentInnerExplosionRadius;
		base.Targeter = abilityUtil_Targeter_MartyrLaser;
	}

	private void SetCachedFields()
	{
		m_cachedLaserInfo = m_laserInfo;
		m_cachedLaserHitEffect = m_laserHitEffect;
	}

	public override LaserTargetingInfo GetLaserInfo()
	{
		return (m_cachedLaserInfo == null) ? m_laserInfo : m_cachedLaserInfo;
	}

	public StandardEffectInfo GetLaserHitEffect()
	{
		StandardEffectInfo result;
		if (m_cachedLaserHitEffect != null)
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			result = m_cachedLaserHitEffect;
		}
		else
		{
			result = m_laserHitEffect;
		}
		return result;
	}

	public float GetBaseExplosionRadius()
	{
		return m_explosionRadius;
	}

	public float GetBonusRadiusPerCrystalSpent()
	{
		return m_additionalRadiusPerCrystalSpent;
	}

	public int GetBaseDamage()
	{
		return m_baseLaserDamage;
	}

	public int GetBaseExplosionHealing()
	{
		return m_baseExplosionHealing;
	}

	public int GetBonusDamagePerCrystalSpent()
	{
		return m_additionalDamagePerCrystalSpent;
	}

	public int GetBonusHealingPerCrystalSpent()
	{
		return m_additionalHealingPerCrystalSpent;
	}

	protected override void AddSpecificTooltipTokens(List<TooltipTokenEntry> tokens, AbilityMod modAsBase)
	{
		base.AddSpecificTooltipTokens(tokens, modAsBase);
		AbilityMod.AddToken_EffectInfo(tokens, m_laserHitEffect, "LaserHitEffect", m_laserHitEffect);
		tokens.Add(new TooltipTokenInt("BaseLaserDamage", "Damage from laser hits with no crystal bonus", GetBaseDamage()));
		tokens.Add(new TooltipTokenInt("BaseExplosionHealing", "Healing from explosion hits with no crystal bonus", GetBaseExplosionHealing()));
		tokens.Add(new TooltipTokenInt("DamagePerCrystal", "Damage added per crystal spent", GetBonusDamagePerCrystalSpent()));
		tokens.Add(new TooltipTokenInt("HealingPerCrystal", "Healing added per crystal spent", GetBonusHealingPerCrystalSpent()));
		tokens.Add(new TooltipTokenFloat("WidthPerCrystal", "Width added per crystal spent", GetBonusWidthPerCrystalSpent()));
		tokens.Add(new TooltipTokenFloat("LengthPerCrystal", "Length added per crystal spent", GetBonusLengthPerCrystalSpent()));
		tokens.Add(new TooltipTokenFloat("RadiusPerCrystal", "Explosion radius added per crystal spent", GetBonusRadiusPerCrystalSpent()));
	}

	protected override List<MartyrLaserThreshold> GetThresholdBasedCrystalBonusList()
	{
		List<MartyrLaserThreshold> list = new List<MartyrLaserThreshold>();
		foreach (MartyrBasicAttackThreshold thresholdBasedCrystalBonuse in m_thresholdBasedCrystalBonuses)
		{
			list.Add(thresholdBasedCrystalBonuse);
		}
		return list;
	}

	private int GetCurrentLaserDamage(ActorData caster)
	{
		int num = (GetCurrentPowerEntry(caster) as MartyrBasicAttackThreshold)?.m_additionalDamage ?? 0;
		return GetBaseDamage() + m_syncComponent.SpentDamageCrystals(caster) * GetBonusDamagePerCrystalSpent() + num;
	}

	private int GetCurrentExplosionHealing(ActorData caster)
	{
		MartyrHealingExplosionThreshold martyrHealingExplosionThreshold = GetCurrentPowerEntry(caster) as MartyrHealingExplosionThreshold;
		int num;
		if (martyrHealingExplosionThreshold != null)
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			num = martyrHealingExplosionThreshold.m_additionalHealing;
		}
		else
		{
			num = 0;
		}
		int num2 = num;
		return GetBaseExplosionHealing() + m_syncComponent.SpentDamageCrystals(caster) * GetBonusHealingPerCrystalSpent() + num2;
	}

	public override float GetCurrentExplosionRadius()
	{
		MartyrBasicAttackThreshold martyrBasicAttackThreshold = GetCurrentPowerEntry(base.ActorData) as MartyrBasicAttackThreshold;
		float num;
		if (martyrBasicAttackThreshold != null)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				break;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			num = martyrBasicAttackThreshold.m_additionalRadius;
		}
		else
		{
			num = 0f;
		}
		float num2 = num;
		return GetBaseExplosionRadius() + (float)m_syncComponent.SpentDamageCrystals(base.ActorData) * GetBonusRadiusPerCrystalSpent() + num2;
	}

	protected override List<AbilityTooltipNumber> CalculateAbilityTooltipNumbers()
	{
		List<AbilityTooltipNumber> numbers = new List<AbilityTooltipNumber>();
		AbilityTooltipHelper.ReportDamage(ref numbers, AbilityTooltipSubject.Primary, GetBaseDamage());
		m_laserHitEffect.ReportAbilityTooltipNumbers(ref numbers, AbilityTooltipSubject.Primary);
		AbilityTooltipHelper.ReportHealing(ref numbers, AbilityTooltipSubject.Secondary, GetBaseExplosionHealing());
		return numbers;
	}

	public override Dictionary<AbilityTooltipSymbol, int> GetCustomNameplateItemTooltipValues(ActorData targetActor, int currentTargeterIndex)
	{
		Dictionary<AbilityTooltipSymbol, int> symbolToValue = new Dictionary<AbilityTooltipSymbol, int>();
		Ability.AddNameplateValueForSingleHit(ref symbolToValue, base.Targeter, targetActor, GetCurrentLaserDamage(base.ActorData));
		Ability.AddNameplateValueForSingleHit(ref symbolToValue, base.Targeter, targetActor, GetCurrentExplosionHealing(base.ActorData), AbilityTooltipSymbol.Healing, AbilityTooltipSubject.Secondary);
		return symbolToValue;
	}
}
