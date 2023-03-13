using System.Collections.Generic;
using UnityEngine;

public class ValkyrieGuard : Ability
{
	[Header("-- Shield effect")]
	public StandardEffectInfo m_shieldEffectInfo;
	[Header("-- Hit reactions")]
	public int m_techPointGainPerCoveredHit = 5;
	public int m_techPointGainPerTooCloseForCoverHit;
	public StandardEffectInfo m_coveredHitReactionEffect;
	public StandardEffectInfo m_tooCloseForCoverHitReactionEffect;
	[Header("-- Duration --")]
	public int m_coverDuration = 1;
	public bool m_coverLastsForever = true;
	[Header("-- Cover Ignore Min Dist?")]
	public bool m_coverIgnoreMinDist = true;
	[Header("-- Sequences")]
	public GameObject m_removeShieldSequencePrefab;
	public GameObject m_applyShieldSequencePrefab;
	private Valkyrie_SyncComponent m_syncComponent;
	private AbilityMod_ValkyrieGuard m_abilityMod;
	private StandardEffectInfo m_cachedShieldEffectInfo;
	private StandardEffectInfo m_cachedCoveredHitReactionEffect;
	private StandardEffectInfo m_cachedTooCloseForCoverHitReactionEffect;

	private void Start()
	{
		if (m_abilityName == "Base Ability")
		{
			m_abilityName = "Aim Shield";
		}
		Setup();
	}

	private void Setup()
	{
		m_syncComponent = base.GetComponent<Valkyrie_SyncComponent>();
		base.Targeter = new AbilityUtil_Targeter_ValkyrieGuard(this, 1f, true, false, false)
		{
			m_addCasterToActorsInRange = true
		};
		SetCachedFields();
	}

	private void SetCachedFields()
	{
		m_cachedShieldEffectInfo = (m_abilityMod ? m_abilityMod.m_shieldEffectInfoMod.GetModifiedValue(m_shieldEffectInfo) : m_shieldEffectInfo);
		m_cachedCoveredHitReactionEffect = (m_abilityMod ? m_abilityMod.m_coveredHitReactionEffectMod.GetModifiedValue(m_coveredHitReactionEffect) : m_coveredHitReactionEffect);
		m_cachedTooCloseForCoverHitReactionEffect = (m_abilityMod ? m_abilityMod.m_tooCloseForCoverHitReactionEffectMod.GetModifiedValue(m_tooCloseForCoverHitReactionEffect) : m_tooCloseForCoverHitReactionEffect);
	}

	public StandardEffectInfo GetShieldEffectInfo()
	{
		if (m_cachedShieldEffectInfo == null)
		{
			return m_shieldEffectInfo;
		}
		return m_cachedShieldEffectInfo;
	}

	public int GetTechPointGainPerCoveredHit()
	{
		if (!m_abilityMod)
		{
			return m_techPointGainPerCoveredHit;
		}
		return m_abilityMod.m_techPointGainPerCoveredHitMod.GetModifiedValue(m_techPointGainPerCoveredHit);
	}

	public int GetTechPointGainPerTooCloseForCoverHit()
	{
		if (!m_abilityMod)
		{
			return m_techPointGainPerTooCloseForCoverHit;
		}
		return m_abilityMod.m_techPointGainPerTooCloseForCoverHitMod.GetModifiedValue(m_techPointGainPerTooCloseForCoverHit);
	}

	public StandardEffectInfo GetCoveredHitReactionEffect()
	{
		if (m_cachedCoveredHitReactionEffect == null)
		{
			return m_coveredHitReactionEffect;
		}
		return m_cachedCoveredHitReactionEffect;
	}

	public StandardEffectInfo GetTooCloseForCoverHitReactionEffect()
	{
		if (m_cachedTooCloseForCoverHitReactionEffect == null)
		{
			return m_tooCloseForCoverHitReactionEffect;
		}
		return m_cachedTooCloseForCoverHitReactionEffect;
	}

	public int GetExtraDamageNextShieldThrowPerCoveredHit()
	{
		if (!m_abilityMod)
		{
			return 0;
		}
		return m_abilityMod.m_extraDamageNextShieldThrowPerCoveredHitMod.GetModifiedValue(0);
	}

	public int GetMaxExtraDamageNextShieldThrow()
	{
		return m_abilityMod ? m_abilityMod.m_maxExtraDamageNextShieldThrow.GetModifiedValue(0) : 0;
	}

	public int GetCoverDuration()
	{
		return (!m_abilityMod) ? m_coverDuration : m_abilityMod.m_coverDurationMod.GetModifiedValue(m_coverDuration);
	}

	public bool CoverLastsForever()
	{
		if (!m_abilityMod)
		{
			return m_coverLastsForever;
		}
		return m_abilityMod.m_coverLastsForeverMod.GetModifiedValue(m_coverLastsForever);
	}

	public AbilityModCooldownReduction GetCooldownReductionOnNoBlock()
	{
		if (!m_abilityMod)
		{
			return null;
		}
		return m_abilityMod.m_cooldownReductionNoBlocks;
	}

	public bool CoverIgnoreMinDist()
	{
		if (!m_abilityMod)
		{
			return m_coverIgnoreMinDist;
		}
		return m_abilityMod.m_coverIgnoreMinDistMod.GetModifiedValue(m_coverIgnoreMinDist);
	}

	public int GetExtraAbsorb()
	{
		if (m_syncComponent)
		{
			return m_syncComponent.m_extraAbsorbForGuard;
		}
		return 0;
	}

	protected override void OnApplyAbilityMod(AbilityMod abilityMod)
	{
		if (abilityMod.GetType() == typeof(AbilityMod_ValkyrieGuard))
		{
			m_abilityMod = (abilityMod as AbilityMod_ValkyrieGuard);
			Setup();
		}
	}

	protected override void OnRemoveAbilityMod()
	{
		m_abilityMod = null;
		Setup();
	}

	protected override void AddSpecificTooltipTokens(List<TooltipTokenEntry> tokens, AbilityMod modAsBase)
	{
		AbilityMod.AddToken_EffectInfo(tokens, m_shieldEffectInfo, "ShieldEffectInfo", m_shieldEffectInfo);
		AddTokenInt(tokens, "TechPointGainPerCoveredHit", string.Empty, m_techPointGainPerCoveredHit);
		AddTokenInt(tokens, "TechPointGainPerTooCloseForCoverHit", string.Empty, m_techPointGainPerTooCloseForCoverHit);
		AbilityMod.AddToken_EffectInfo(tokens, m_coveredHitReactionEffect, "CoveredHitReactionEffect", m_coveredHitReactionEffect);
		AbilityMod.AddToken_EffectInfo(tokens, m_tooCloseForCoverHitReactionEffect, "TooCloseForCoverHitReactionEffect", m_tooCloseForCoverHitReactionEffect);
	}

	protected override List<AbilityTooltipNumber> CalculateAbilityTooltipNumbers()
	{
		List<AbilityTooltipNumber> list = new List<AbilityTooltipNumber>();
		list.Add(new AbilityTooltipNumber(AbilityTooltipSymbol.Absorb, AbilityTooltipSubject.Self, 0));
		return list;
	}

	public override Dictionary<AbilityTooltipSymbol, int> GetCustomNameplateItemTooltipValues(ActorData targetActor, int currentTargeterIndex)
	{
		Dictionary<AbilityTooltipSymbol, int> dictionary = new Dictionary<AbilityTooltipSymbol, int>();
		if (targetActor == base.ActorData)
		{
			dictionary[AbilityTooltipSymbol.Absorb] = GetExtraAbsorb();
		}
		return dictionary;
	}

	public override Vector3 GetRotateToTargetPos(List<AbilityTarget> targets, ActorData caster)
	{
		BoardSquare square = Board.Get().GetSquare(targets[0].GridPos);
		if (square != null)
		{
			Vector3 vector;
			VectorUtils.GetDirectionAndOffsetToClosestSide(square, targets[0].FreePos, false, out vector);
			return square.ToVector3() + vector;
		}
		return base.GetRotateToTargetPos(targets, caster);
	}

	public override bool CustomCanCastValidation(ActorData caster)
	{
		return caster != null && caster.GetAbilityData() != null && !caster.GetAbilityData().HasQueuedAbilityOfType(typeof(ValkyrieDashAoE));
	}

	public override TargetingParadigm GetControlpadTargetingParadigm(int targetIndex)
	{
		return TargetingParadigm.Direction;
	}

	public override bool HasRestrictedFreePosDistance(ActorData aimingActor, int targetIndex, List<AbilityTarget> targetsSoFar, out float min, out float max)
	{
		min = 1f;
		max = 1f;
		return true;
	}

#if SERVER
	//Added in rouges
	public override void Run(List<AbilityTarget> targets, ActorData caster, ServerAbilityUtils.AbilityRunData additionalData)
	{
		ActorCover.CoverDirections coverFacing = GetCoverFacing(targets);
		if (m_syncComponent != null)
		{
			m_syncComponent.Networkm_coverDirection = coverFacing;
			m_syncComponent.Networkm_extraAbsorbForGuard = 0;
		}
	}

	//Added in rouges
	public override List<ServerClientUtils.SequenceStartData> GetAbilityRunSequenceStartDataList(List<AbilityTarget> targets, ActorData caster, ServerAbilityUtils.AbilityRunData additionalData)
	{
		List<ServerClientUtils.SequenceStartData> list = new List<ServerClientUtils.SequenceStartData>();
		ValkyrieDirectionalShieldSequence.ExtraParams extraParams = new ValkyrieDirectionalShieldSequence.ExtraParams();
		extraParams.m_aimDirection = (sbyte)GetCoverFacing(targets);
		ServerClientUtils.SequenceStartData item = new ServerClientUtils.SequenceStartData(m_applyShieldSequencePrefab, caster.GetFreePos(), caster.AsArray(), caster, additionalData.m_sequenceSource, new Sequence.IExtraSequenceParams[]
		{
			extraParams
		});
		list.Add(item);
		return list;
	}

	//Added in rouges
	public override void GatherAbilityResults(List<AbilityTarget> targets, ActorData caster, ref AbilityResults abilityResults)
	{
		ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(caster, caster.GetFreePos()));
		ValkyrieGuardEndingEffect valkyrieGuardEndingEffect = ServerEffectManager.Get().GetEffect(caster, typeof(ValkyrieGuardEndingEffect)) as ValkyrieGuardEndingEffect;
		if (valkyrieGuardEndingEffect != null)
		{
			actorHitResults.AddEffectForRemoval(valkyrieGuardEndingEffect);
		}
		ActorCover.CoverDirections coverFacing = GetCoverFacing(targets);
		valkyrieGuardEndingEffect = CreateGuardEffect(coverFacing, CoverIgnoreMinDist(), caster, GetShieldEffectInfo(), GetCoverDuration(), GetTechPointGainPerCoveredHit(), GetTechPointGainPerTooCloseForCoverHit(), GetExtraAbsorb());
		actorHitResults.AddEffect(valkyrieGuardEndingEffect);
		abilityResults.StoreActorHit(actorHitResults);
	}

	//Added in rouges
	public ValkyrieGuardEndingEffect CreateGuardEffect(ActorCover.CoverDirections coverDir, bool ignoreMinDist, ActorData caster, StandardEffectInfo shieldEffectInfo, int coverDuration, int techPointGainPerCoveredHit, int techPointGainPerTooCloseForCoverHit, int extraAbsorb)
	{
		StandardActorEffectData shallowCopy = shieldEffectInfo.m_effectData.GetShallowCopy();
		shallowCopy.m_absorbAmount += extraAbsorb;
		ValkyrieGuardEndingEffect valkyrieGuardEndingEffect = new ValkyrieGuardEndingEffect(base.AsEffectSource(), null, caster, caster, shallowCopy, m_removeShieldSequencePrefab, coverDir, ignoreMinDist, techPointGainPerCoveredHit, techPointGainPerTooCloseForCoverHit, GetCoveredHitReactionEffect(), GetTooCloseForCoverHitReactionEffect());
		if (m_coverLastsForever)
		{
			valkyrieGuardEndingEffect.SetDurationBeforeStart(0);
		}
		else
		{
			valkyrieGuardEndingEffect.SetDurationBeforeStart(coverDuration);
		}
		return valkyrieGuardEndingEffect;
	}

	//Added in rouges
	private ActorCover.CoverDirections GetCoverFacing(List<AbilityTarget> targets)
	{
		ActorCover.CoverDirections result = ActorCover.CoverDirections.INVALID;
		BoardSquare square = Board.Get().GetSquare(targets[0].GridPos);
		if (square != null)
		{
			Vector3 vector;
			VectorUtils.GetDirectionAndOffsetToClosestSide(square, targets[0].FreePos, false, out vector);
			Vector3 vec = square.ToVector3() + vector * 2f;
			result = ActorCover.GetCoverDirection(square, Board.Get().GetSquareFromVec3(vec));
		}
		return result;
	}
#endif
}
