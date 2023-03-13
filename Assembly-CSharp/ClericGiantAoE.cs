using System.Collections.Generic;
using UnityEngine;

public class ClericGiantAoE : Ability
{
	[Header("-- Targeting")]
	public float m_aoeRadius = 6f;

	public bool m_penetrateLoS;

	public int m_maxTargets = -1;

	[Header("-- On Hit Damage/Heal/Effect")]
	public int m_damageAmount = 30;

	public float m_damageDecreasePerSquare = 2f;

	public int m_healAmount = 30;

	public float m_healDecreasePerSquare = 2f;

	public StandardEffectInfo m_enemyHitEffect;

	public StandardEffectInfo m_allyHitEffect;

	[Header("-- Sequences")]
	public GameObject m_castSequencePrefab;

	private StandardEffectInfo m_cachedEnemyHitEffect;

	private StandardEffectInfo m_cachedAllyHitEffect;

	private void Start()
	{
		if (m_abilityName == "Base Ability")
		{
			m_abilityName = "Giant AoE";
		}
		SetupTargeter();
	}

	private void SetupTargeter()
	{
		SetCachedFields();
		base.Targeter = new AbilityUtil_Targeter_AoE_Smooth(this, GetAoeRadius(), PenetrateLoS(), true, true, GetMaxTargets());
	}

	private void SetCachedFields()
	{
		m_cachedEnemyHitEffect = m_enemyHitEffect;
		m_cachedAllyHitEffect = m_allyHitEffect;
	}

	public float GetAoeRadius()
	{
		return m_aoeRadius;
	}

	public bool PenetrateLoS()
	{
		return m_penetrateLoS;
	}

	public int GetMaxTargets()
	{
		return m_maxTargets;
	}

	public int GetDamageAmount()
	{
		return m_damageAmount;
	}

	public float GetDamageDecreasePerSquare()
	{
		return m_damageDecreasePerSquare;
	}

	public int GetHealAmount()
	{
		return m_healAmount;
	}

	public float GetHealDecreasePerSquare()
	{
		return m_healDecreasePerSquare;
	}

	public StandardEffectInfo GetEnemyHitEffect()
	{
		StandardEffectInfo result;
		if (m_cachedEnemyHitEffect != null)
		{
			result = m_cachedEnemyHitEffect;
		}
		else
		{
			result = m_enemyHitEffect;
		}
		return result;
	}

	public StandardEffectInfo GetAllyHitEffect()
	{
		StandardEffectInfo result;
		if (m_cachedAllyHitEffect != null)
		{
			result = m_cachedAllyHitEffect;
		}
		else
		{
			result = m_allyHitEffect;
		}
		return result;
	}

	protected override void AddSpecificTooltipTokens(List<TooltipTokenEntry> tokens, AbilityMod modAsBase)
	{
		AddTokenInt(tokens, "MaxTargets", string.Empty, m_maxTargets);
		AddTokenInt(tokens, "DamageAmount", string.Empty, m_damageAmount);
		AddTokenInt(tokens, "HealAmount", string.Empty, m_healAmount);
		AbilityMod.AddToken_EffectInfo(tokens, m_enemyHitEffect, "EnemyHitEffect", m_enemyHitEffect);
		AbilityMod.AddToken_EffectInfo(tokens, m_allyHitEffect, "AllyHitEffect", m_allyHitEffect);
	}

	protected override List<AbilityTooltipNumber> CalculateAbilityTooltipNumbers()
	{
		List<AbilityTooltipNumber> list = new List<AbilityTooltipNumber>();
		list.Add(new AbilityTooltipNumber(AbilityTooltipSymbol.Damage, AbilityTooltipSubject.Enemy, m_damageAmount));
		list.Add(new AbilityTooltipNumber(AbilityTooltipSymbol.Healing, AbilityTooltipSubject.Ally, m_healAmount));
		return list;
	}

	public override Dictionary<AbilityTooltipSymbol, int> GetCustomNameplateItemTooltipValues(ActorData targetActor, int currentTargeterIndex)
	{
		Dictionary<AbilityTooltipSymbol, int> dictionary = new Dictionary<AbilityTooltipSymbol, int>();
		int num = Mathf.RoundToInt((targetActor.GetLoSCheckPos() - base.ActorData.GetLoSCheckPos()).magnitude / Board.Get().squareSize);
		num = Mathf.Max(0, num - 1);
		int value = GetDamageAmount() - Mathf.RoundToInt((float)num * GetDamageDecreasePerSquare());
		int value2 = GetHealAmount() - Mathf.RoundToInt((float)num * GetHealDecreasePerSquare());
		dictionary[AbilityTooltipSymbol.Damage] = value;
		dictionary[AbilityTooltipSymbol.Healing] = value2;
		return dictionary;
	}

#if SERVER
	// Added in rouges
	private List<ActorData> GetHitTargets(List<AbilityTarget> targets, ActorData caster, List<NonActorTargetInfo> nonActorTargetInfo)
	{
		List<Team> onlyValidTeams = new List<Team>
		{
			caster.GetTeam(),
			caster.GetEnemyTeam(),
			Team.Objects
		};
		return AreaEffectUtils.GetActorsInRadius(caster.GetLoSCheckPos(), this.GetAoeRadius(), this.PenetrateLoS(), caster, onlyValidTeams, nonActorTargetInfo);
	}

	public override List<ServerClientUtils.SequenceStartData> GetAbilityRunSequenceStartDataList(List<AbilityTarget> targets, ActorData caster, ServerAbilityUtils.AbilityRunData additionalData)
	{
		List<ServerClientUtils.SequenceStartData> list = new List<ServerClientUtils.SequenceStartData>();
		ServerClientUtils.SequenceStartData item = new ServerClientUtils.SequenceStartData(this.m_castSequencePrefab, caster.GetLoSCheckPos(), additionalData.m_abilityResults.HitActorsArray(), caster, additionalData.m_sequenceSource, null);
		list.Add(item);
		return list;
	}

	public override void GatherAbilityResults(List<AbilityTarget> targets, ActorData caster, ref AbilityResults abilityResults)
	{
		List<NonActorTargetInfo> nonActorTargetInfo = new List<NonActorTargetInfo>();
		foreach (ActorData actorData in this.GetHitTargets(targets, caster, nonActorTargetInfo))
		{
			ActorHitResults actorHitResults = base.MakeActorHitRes(actorData, caster.GetLoSCheckPos());
			int num = Mathf.RoundToInt((actorData.GetLoSCheckPos() - caster.GetLoSCheckPos()).magnitude / Board.Get().squareSize);
			num = Mathf.Max(0, num - 1);
			if (actorData.GetTeam() != caster.GetTeam())
			{
				int baseDamage = this.GetDamageAmount() - Mathf.RoundToInt((float)num * this.GetDamageDecreasePerSquare());
				actorHitResults.SetBaseDamage(baseDamage);
				actorHitResults.AddStandardEffectInfo(this.GetEnemyHitEffect());
			}
			else
			{
				int baseHealing = this.GetHealAmount() - Mathf.RoundToInt((float)num * this.GetHealDecreasePerSquare());
				actorHitResults.SetBaseHealing(baseHealing);
				actorHitResults.AddStandardEffectInfo(this.GetAllyHitEffect());
			}
			abilityResults.StoreActorHit(actorHitResults);
		}
		abilityResults.StoreNonActorTargetInfo(nonActorTargetInfo);
	}
#endif
}
