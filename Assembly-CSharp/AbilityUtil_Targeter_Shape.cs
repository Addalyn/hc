using System.Collections.Generic;
using UnityEngine;

public class AbilityUtil_Targeter_Shape : AbilityUtil_Targeter
{
	public enum DamageOriginType
	{
		CenterOfShape,
		CasterPos
	}

	public delegate bool IsAffectingCasterDelegate(ActorData caster, List<ActorData> actorsSoFar, bool casterInShape);

	public delegate Vector3 CustomCenterPosDelegate(ActorData caster, AbilityTarget currentTarget);

	public AbilityAreaShape m_shape;

	public bool m_penetrateLoS;

	public AffectsActor m_affectsCaster;

	public AffectsActor m_affectsBestTarget;

	private float m_heightOffset = 0.1f;

	private float m_curSpeed;

	protected OperationOnSquare_TurnOnHiddenSquareIndicator m_indicatorHandler;

	protected AbilityTooltipSubject m_enemyTooltipSubject;

	protected AbilityTooltipSubject m_allyTooltipSubject;

	protected AbilityTooltipSubject m_casterTooltipSubject;

	public DamageOriginType m_damageOriginType;

	public ActorData m_lastCenterSquareActor;

	public IsAffectingCasterDelegate m_affectCasterDelegate;

	public CustomCenterPosDelegate m_customCenterPosDelegate;

	private GridPos m_currentGridPos = GridPos.s_invalid;

	public bool UseGridPosSquarePosAsFreePos
	{
		get;
		set;
	}

	public AbilityUtil_Targeter_Shape(Ability ability, AbilityAreaShape shape, bool penetrateLoS, DamageOriginType damageOriginType = DamageOriginType.CenterOfShape, bool affectsEnemies = true, bool affectsAllies = false, AffectsActor affectsCaster = AffectsActor.Possible, AffectsActor affectsBestTarget = AffectsActor.Possible)
		: base(ability)
	{
		m_shape = shape;
		m_penetrateLoS = penetrateLoS;
		m_damageOriginType = damageOriginType;
		m_affectsCaster = affectsCaster;
		m_affectsBestTarget = affectsBestTarget;
		m_affectsEnemies = affectsEnemies;
		m_affectsAllies = affectsAllies;
		m_enemyTooltipSubject = AbilityTooltipSubject.Primary;
		m_allyTooltipSubject = AbilityTooltipSubject.Primary;
		UseGridPosSquarePosAsFreePos = false;
		m_indicatorHandler = new OperationOnSquare_TurnOnHiddenSquareIndicator(this);
		m_showArcToShape = HighlightUtils.Get().m_showTargetingArcsForShapes;
	}

	public GridPos GetCurrentGridPos()
	{
		return m_currentGridPos;
	}

	public void SetTooltipSubjectTypes(AbilityTooltipSubject enemySubject = AbilityTooltipSubject.Primary, AbilityTooltipSubject allySubject = AbilityTooltipSubject.Primary, AbilityTooltipSubject casterSubject = AbilityTooltipSubject.None)
	{
		m_enemyTooltipSubject = enemySubject;
		m_allyTooltipSubject = allySubject;
		m_casterTooltipSubject = casterSubject;
	}

	protected Vector3 GetHighlightGoalPos(AbilityTarget currentTarget, ActorData targetingActor)
	{
		BoardSquare gameplayRefSquare = GetGameplayRefSquare(currentTarget, targetingActor);
		if (gameplayRefSquare != null)
		{
			Vector3 vector;
			if (UseGridPosSquarePosAsFreePos)
			{
				while (true)
				{
					switch (2)
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
				vector = gameplayRefSquare.ToVector3();
			}
			else
			{
				vector = currentTarget.FreePos;
			}
			Vector3 freePos = vector;
			Vector3 centerOfShape = AreaEffectUtils.GetCenterOfShape(m_shape, freePos, gameplayRefSquare);
			Vector3 travelBoardSquareWorldPosition = targetingActor.GetTravelBoardSquareWorldPosition();
			centerOfShape.y = travelBoardSquareWorldPosition.y + m_heightOffset;
			return centerOfShape;
		}
		return Vector3.zero;
	}

	protected BoardSquare GetGameplayRefSquare(AbilityTarget currentTarget, ActorData targetingActor)
	{
		if (m_customCenterPosDelegate != null)
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					break;
				default:
				{
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					Vector3 vector2D = m_customCenterPosDelegate(targetingActor, currentTarget);
					return Board.Get().GetBoardSquare(vector2D);
				}
				}
			}
		}
		GridPos gridPos;
		if (GetCurrentRangeInSquares() != 0f)
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
			gridPos = currentTarget.GridPos;
		}
		else
		{
			gridPos = targetingActor.GetGridPosWithIncrementedHeight();
		}
		return Board.Get().GetBoardSquareSafe(gridPos);
	}

	public override void UpdateHighlightPosAfterClick(AbilityTarget target, ActorData targetingActor, int currentTargetIndex, List<AbilityTarget> targets)
	{
		if (base.Highlight != null)
		{
			Vector3 highlightGoalPos = GetHighlightGoalPos(target, targetingActor);
			base.Highlight.transform.position = highlightGoalPos;
		}
	}

	public override void UpdateTargeting(AbilityTarget currentTarget, ActorData targetingActor)
	{
		UpdateTargetingMultiTargets(currentTarget, targetingActor, 0, null);
	}

	public override void UpdateTargetingMultiTargets(AbilityTarget currentTarget, ActorData targetingActor, int currentTargetIndex, List<AbilityTarget> targets)
	{
		m_currentGridPos = currentTarget.GridPos;
		ClearActorsInRange();
		m_lastCenterSquareActor = null;
		bool flag = GameFlowData.Get().activeOwnedActorData == targetingActor;
		if (flag)
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
			ResetSquareIndicatorIndexToUse();
		}
		BoardSquare gameplayRefSquare = GetGameplayRefSquare(currentTarget, targetingActor);
		if (gameplayRefSquare != null)
		{
			Vector3 highlightGoalPos = GetHighlightGoalPos(currentTarget, targetingActor);
			if (base.Highlight == null)
			{
				base.Highlight = HighlightUtils.Get().CreateShapeCursor(m_shape, targetingActor == GameFlowData.Get().activeOwnedActorData);
				base.Highlight.transform.position = highlightGoalPos;
			}
			else
			{
				base.Highlight.transform.position = TargeterUtils.MoveHighlightTowards(highlightGoalPos, base.Highlight, ref m_curSpeed);
			}
			base.Highlight.SetActive(true);
			Vector3 freePos = (!UseGridPosSquarePosAsFreePos) ? currentTarget.FreePos : gameplayRefSquare.ToVector3();
			Vector3 damageOrigin;
			if (m_damageOriginType == DamageOriginType.CasterPos)
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
				damageOrigin = targetingActor.GetTravelBoardSquareWorldPositionForLos();
			}
			else
			{
				damageOrigin = AreaEffectUtils.GetCenterOfShape(m_shape, freePos, gameplayRefSquare);
			}
			List<ActorData> actors = AreaEffectUtils.GetActorsInShape(m_shape, freePos, gameplayRefSquare, m_penetrateLoS, targetingActor, GetAffectedTeams(), null);
			actors.Remove(targetingActor);
			bool flag2 = AreaEffectUtils.IsSquareInShape(targetingActor.GetCurrentBoardSquare(), m_shape, freePos, gameplayRefSquare, m_penetrateLoS, targetingActor);
			TargeterUtils.RemoveActorsInvisibleToClient(ref actors);
			if (m_affectsCaster == AffectsActor.Possible)
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
				bool num;
				if (m_affectCasterDelegate == null)
				{
					while (true)
					{
						switch (1)
						{
						case 0:
							continue;
						}
						break;
					}
					num = flag2;
				}
				else
				{
					num = m_affectCasterDelegate(targetingActor, actors, flag2);
				}
				if (num)
				{
					actors.Add(targetingActor);
				}
			}
			ActorData actorData = currentTarget.GetCurrentBestActorTarget();
			if (actorData != null)
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
				if (!actorData.IsVisibleToClient())
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
					actorData = null;
				}
			}
			m_lastCenterSquareActor = actorData;
			using (List<ActorData>.Enumerator enumerator = actors.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ActorData current = enumerator.Current;
					HandleAddActorInShape(current, targetingActor, currentTarget, damageOrigin, actorData);
				}
				while (true)
				{
					switch (1)
					{
					case 0:
						continue;
					}
					break;
				}
			}
			if (m_affectsCaster == AffectsActor.Always)
			{
				while (true)
				{
					switch (1)
					{
					case 0:
						continue;
					}
					break;
				}
				AbilityTooltipSubject abilityTooltipSubject = m_casterTooltipSubject;
				if (abilityTooltipSubject == AbilityTooltipSubject.None)
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
					abilityTooltipSubject = m_allyTooltipSubject;
				}
				AddActorInRange(targetingActor, damageOrigin, targetingActor, abilityTooltipSubject);
			}
			if (m_affectsBestTarget == AffectsActor.Always)
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
				if (actorData != null)
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
					if (actorData.GetTeam() == targetingActor.GetTeam())
					{
						while (true)
						{
							switch (1)
							{
							case 0:
								continue;
							}
							break;
						}
						AddActorInRange(actorData, damageOrigin, targetingActor, m_allyTooltipSubject);
					}
					else
					{
						AddActorInRange(actorData, damageOrigin, targetingActor, m_enemyTooltipSubject);
					}
				}
			}
			if (flag)
			{
				while (true)
				{
					switch (5)
					{
					case 0:
						continue;
					}
					break;
				}
				AreaEffectUtils.OperateOnSquaresInShape(m_indicatorHandler, m_shape, freePos, gameplayRefSquare, m_penetrateLoS, targetingActor);
			}
		}
		if (!flag)
		{
			return;
		}
		while (true)
		{
			switch (1)
			{
			case 0:
				continue;
			}
			HideUnusedSquareIndicators();
			return;
		}
	}

	protected virtual bool HandleAddActorInShape(ActorData potentialTarget, ActorData targetingActor, AbilityTarget currentTarget, Vector3 damageOrigin, ActorData bestTarget)
	{
		int num;
		if (!(potentialTarget != targetingActor))
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
			num = ((m_affectsCaster == AffectsActor.Possible) ? 1 : 0);
		}
		else
		{
			num = 1;
		}
		bool flag = (byte)num != 0;
		bool flag2 = potentialTarget != bestTarget || m_affectsBestTarget == AffectsActor.Possible;
		if (flag)
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
			if (flag2)
			{
				while (true)
				{
					switch (4)
					{
					case 0:
						break;
					default:
						if (potentialTarget == targetingActor)
						{
							while (true)
							{
								switch (5)
								{
								case 0:
									continue;
								}
								break;
							}
							AbilityTooltipSubject abilityTooltipSubject = m_casterTooltipSubject;
							if (abilityTooltipSubject == AbilityTooltipSubject.None)
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
								abilityTooltipSubject = m_allyTooltipSubject;
							}
							AddActorInRange(potentialTarget, damageOrigin, targetingActor, abilityTooltipSubject);
						}
						if (potentialTarget.GetTeam() == targetingActor.GetTeam())
						{
							AddActorInRange(potentialTarget, damageOrigin, targetingActor, m_allyTooltipSubject);
						}
						else
						{
							AddActorInRange(potentialTarget, damageOrigin, targetingActor, m_enemyTooltipSubject);
						}
						return true;
					}
				}
			}
		}
		return false;
	}
}
