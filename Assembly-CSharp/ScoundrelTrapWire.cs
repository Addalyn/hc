using System.Collections.Generic;
using UnityEngine;

public class ScoundrelTrapWire : Ability
{
	public AbilityGridPattern m_pattern = AbilityGridPattern.Plus_Two_x_Two;

	public float m_barrierSizeScale = 1f;

	public StandardBarrierData m_barrierData;

	private AbilityMod_ScoundrelTrapWire m_abilityMod;

	private void Start()
	{
		if (m_abilityName == "Base Ability")
		{
			m_abilityName = "Trap Wires";
		}
		if (m_pattern != 0)
		{
			ModdedBarrierData().SetupForPattern(m_pattern);
		}
		SetupTargeter();
	}

	private void SetupTargeter()
	{
		if (m_pattern == AbilityGridPattern.NoPattern)
		{
			Targeter = new AbilityUtil_Targeter_Barrier(this, ModdedBarrierData().m_width * ModdedBarrierScale());
		}
		else
		{
			AbilityUtil_Targeter_Grid abilityUtil_Targeter_Grid = new AbilityUtil_Targeter_Grid(this, m_pattern, ModdedBarrierScale());
			Targeter = abilityUtil_Targeter_Grid;
		}
		Targeter.ShowArcToShape = true;
	}

	protected override void AddSpecificTooltipTokens(List<TooltipTokenEntry> tokens, AbilityMod modAsBase)
	{
		ModdedBarrierData().AddTooltipTokens(tokens, "Wall");
	}

	protected override List<AbilityTooltipNumber> CalculateAbilityTooltipNumbers()
	{
		List<AbilityTooltipNumber> numbers = new List<AbilityTooltipNumber>();
		ModdedBarrierData().ReportAbilityTooltipNumbers(ref numbers);
		return numbers;
	}

	protected override void OnApplyAbilityMod(AbilityMod abilityMod)
	{
		if (abilityMod.GetType() != typeof(AbilityMod_ScoundrelTrapWire))
		{
			Debug.LogError("Trying to apply wrong type of ability mod");
			return;
		}

		m_abilityMod = (abilityMod as AbilityMod_ScoundrelTrapWire);
		SetupTargeter();
	}

	protected override void OnRemoveAbilityMod()
	{
		m_abilityMod = null;
		SetupTargeter();
	}

	public StandardBarrierData ModdedBarrierData()
	{
		if (m_abilityMod != null)
		{
			return m_abilityMod.m_barrierDataMod.GetModifiedValue(m_barrierData);
		}
		return m_barrierData;
	}

	private float ModdedBarrierScale()
	{
		float num = m_barrierSizeScale;
		if (m_abilityMod != null)
		{
			num = m_abilityMod.m_barrierScaleMod.GetModifiedValue(num);
		}
		return num;
	}

	public List<GameObject> ModdedBarrierSequencePrefab()
	{
		if ((m_abilityMod?.m_barrierSequence?.Count ?? 0) > 0)
		{
			return m_abilityMod.m_barrierSequence;
		}
		return ModdedBarrierData().m_barrierSequencePrefabs;
	}
}
