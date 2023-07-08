using UnityEngine;

public class AbilityUtil_Targeter_AoE_AroundActor : AbilityUtil_Targeter_AoE_Smooth
{
	private bool m_canTargetOnAlly;
	private bool m_canTargetOnEnemy;
	private bool m_canTargetOnSelf;
	private bool m_lockToGridPos = true;

	public ActorData m_lastCenterActor;
	public AbilityTooltipSubject m_allyOccupantSubject = AbilityTooltipSubject.Ally;
	public AbilityTooltipSubject m_enemyOccupantSubject = AbilityTooltipSubject.Primary;

	public AbilityUtil_Targeter_AoE_AroundActor(
		Ability ability,
		float radius,
		bool penetrateLoS,
		bool aoeAffectsEnemies = true,
		bool aoeAffectsAllies = false,
		int maxTargets = -1,
		bool canTargetOnEnemy = false,
		bool canTargetOnAlly = true,
		bool canTargetOnSelf = true)
		: base(ability, radius, penetrateLoS, aoeAffectsEnemies, aoeAffectsAllies, maxTargets)
	{
		m_canTargetOnEnemy = canTargetOnEnemy;
		m_canTargetOnAlly = canTargetOnAlly;
		m_canTargetOnSelf = canTargetOnSelf;
	}

	protected override Vector3 GetRefPos(AbilityTarget currentTarget, ActorData targetingActor, float range)
	{
		Vector3 result = base.GetRefPos(currentTarget, targetingActor, range);
		if (m_lockToGridPos
		    && Board.Get() != null
		    && currentTarget != null)
		{
			BoardSquare targetSquare = Board.Get().GetSquare(currentTarget.GridPos);
			if (targetSquare != null)
			{
				result = targetSquare.ToVector3();
			}
		}
		return result;
	}

	public override void UpdateTargeting(AbilityTarget currentTarget, ActorData targetingActor)
	{
		base.UpdateTargeting(currentTarget, targetingActor);
		m_lastCenterActor = null;
		BoardSquare boardSquare = Board.Get().GetSquareFromVec3(GetRefPos(currentTarget, targetingActor, GetCurrentRangeInSquares()));
		ActorData occupantActor = boardSquare.OccupantActor;
		if (occupantActor == null)
		{
			return;
		}
		if (occupantActor == targetingActor && m_canTargetOnSelf)
		{
			AddActorInRange(targetingActor, targetingActor.GetFreePos(), targetingActor, m_allyOccupantSubject);
			m_lastCenterActor = occupantActor;
		}
		else if (occupantActor.GetTeam() == targetingActor.GetTeam() && m_canTargetOnAlly)
		{
			AddActorInRange(occupantActor, targetingActor.GetFreePos(), targetingActor, m_allyOccupantSubject);
			m_lastCenterActor = occupantActor;
		}
		else if (occupantActor.GetTeam() != targetingActor.GetTeam() && m_canTargetOnEnemy)
		{
			AddActorInRange(occupantActor, targetingActor.GetFreePos(), targetingActor, m_enemyOccupantSubject);
			m_lastCenterActor = occupantActor;
		}
	}
}
