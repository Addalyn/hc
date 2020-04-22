using System.Collections.Generic;
using UnityEngine;

public class HealPBAoE : Ability
{
	public AbilityAreaShape m_shape = AbilityAreaShape.Three_x_Three;

	public float m_selfHitPointsPercentOfMax;

	public float m_teamHitPointsPercentOfMax;

	public int m_selfHitPoints;

	public int m_teamHitPoints;

	public int m_energy;

	private void Start()
	{
		base.Targeter = new AbilityUtil_Targeter_Shape(this, m_shape, true);
	}

	private int CalcHealPoints(ActorData aoeTarget, ActorData caster)
	{
		bool flag = aoeTarget == caster;
		float num;
		if (flag)
		{
			while (true)
			{
				switch (4)
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
			num = m_selfHitPointsPercentOfMax;
		}
		else
		{
			num = m_teamHitPointsPercentOfMax;
		}
		float num2 = num;
		int num3 = Mathf.RoundToInt(num2 * aoeTarget.GetActorStats().GetModifiedStatFloat(StatType.MaxHitPoints));
		int num4;
		if (flag)
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
			num4 = m_selfHitPoints;
		}
		else
		{
			num4 = m_teamHitPoints;
		}
		int num5 = num4;
		return num5 + num3;
	}

	private List<ActorData> GetTargets(List<AbilityTarget> targets, ActorData caster)
	{
		return AreaEffectUtils.GetActorsInShape(m_shape, caster.GetTravelBoardSquareWorldPosition(), caster.GetCurrentBoardSquare(), true, caster, caster.GetTeams(), null);
	}
}
