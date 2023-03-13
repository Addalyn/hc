using System;
using System.Collections.Generic;
using UnityEngine;

#if SERVER
// added in rogues
public class MovingGroundEffect : StandardGroundEffect
{
	public MovingGroundEffect(EffectSource parent, BoardSquare targetSquare, Vector3 shapeFreePos, ActorData target, ActorData caster, GroundEffectField fieldInfo) : base(parent, targetSquare, shapeFreePos, target, caster, fieldInfo)
	{
	}

	public override void GatherMovementResultsFromSegment(ActorData mover, MovementInstance movementInstance, MovementStage movementStage, BoardSquarePathInfo sourcePath, BoardSquarePathInfo destPath, ref List<MovementResults> movementResultsList)
	{
		if (mover != base.Target)
		{
			base.GatherMovementResultsFromSegment(mover, movementInstance, movementStage, sourcePath, destPath, ref movementResultsList);
			return;
		}
		base.TargetSquare = destPath.square;
		m_shapeFreePos = destPath.square.GetOccupantRefPos();
		base.CalculateAffectedSquares();
		if (m_fieldInfo.ignoreMovementHits)
		{
			return;
		}
		if (!this.ShouldHitThisTurn())
		{
			return;
		}
		ServerAbilityUtils.TriggeringPathInfo triggeringPathInfo = new ServerAbilityUtils.TriggeringPathInfo(mover, destPath);
		List<NonActorTargetInfo> nonActorTargetInfo = new List<NonActorTargetInfo>();
		List<ActorData> affectableActorsInField = m_fieldInfo.GetAffectableActorsInField(base.TargetSquare, m_shapeFreePos, base.Caster, nonActorTargetInfo);
		MovementResults movementResults = new MovementResults(movementStage);
		movementResults.SetupTriggerData(triggeringPathInfo);
		EffectResults effectResults = null;
		GameObject gameObject = null;
		foreach (ActorData actorData in affectableActorsInField)
		{
			if (!m_actorsHitThisTurn.Contains(actorData) && m_fieldInfo.CanBeAffected(actorData, base.Caster))
			{
				ActorHitResults actorHitResults = new ActorHitResults(new ActorHitParameters(triggeringPathInfo)
				{
					Target = actorData
				});
				this.SetupActorHitResults(ref actorHitResults, triggeringPathInfo.m_triggeringPathSegment.square);
				if (gameObject == null)
				{
					if (actorData.GetTeam() != base.Caster.GetTeam())
					{
						gameObject = m_fieldInfo.enemyHitSequencePrefab;
					}
					else
					{
						gameObject = m_fieldInfo.allyHitSequencePrefab;
					}
				}
				if (effectResults == null)
				{
					effectResults = movementResults.SetupGameplayData(this, actorHitResults);
				}
				else
				{
					effectResults.StoreActorHit(actorHitResults);
				}
				m_actorsHitThisTurn.Add(actorData);
			}
		}
		if (effectResults != null && !effectResults.m_actorToHitResults.IsNullOrEmpty<KeyValuePair<ActorData, ActorHitResults>>())
		{
			movementResults.SetupSequenceData(gameObject, triggeringPathInfo.m_triggeringPathSegment.square, base.SequenceSource, null, true);
			movementResultsList.Add(movementResults);
		}
	}
}
#endif
