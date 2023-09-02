using System.Collections.Generic;
using UnityEngine;

public class NPCBrain_Tutorial : NPCBrain
{
	public enum AttackPattern
	{
		AlwaysAttackPlayer,
		AttackPlayerIfInLoS,
		AttackNearestEnemy,
		NeverAttack
	}

	public enum MovementPattern
	{
		StayStill,
		MoveOncePlayerInLoS,
		MoveAsap
	}

	public AttackPattern m_attackPattern = AttackPattern.AttackPlayerIfInLoS;
	private Transform m_destination;
	public MovementPattern m_movementPattern;
	public float m_attackRange = 5f;

	public override NPCBrain Create(BotController bot, Transform destination)
	{
		NPCBrain_Tutorial nPCBrain_Tutorial = bot.gameObject.AddComponent<NPCBrain_Tutorial>();
		nPCBrain_Tutorial.m_attackPattern = m_attackPattern;
		nPCBrain_Tutorial.m_movementPattern = m_movementPattern;
		nPCBrain_Tutorial.m_destination = destination;
		nPCBrain_Tutorial.m_attackRange = m_attackRange;
		return nPCBrain_Tutorial;
	}

	public bool IsPlayerInLoS()
	{
		bool isVisibleToEnemy = GetComponent<ActorData>().IsActorVisibleToAnyEnemy();
		ActorData localPlayer = SinglePlayerManager.Get().GetLocalPlayer();
		if (localPlayer == null)
		{
			return false;
		}
		ActorStatus actorStatus = localPlayer.GetComponent<ActorStatus>();
		bool isNotHidden = !localPlayer.IsInBrush()
		             || actorStatus.HasStatus(StatusType.CantHideInBrush)
		             || actorStatus.HasStatus(StatusType.Revealed);
		return isVisibleToEnemy && isNotHidden;
	}

	public ActorData FindNearestEnemy()
	{
		ActorData nearestEnemy = null;
		ActorData actorData = GetComponent<ActorData>();
		BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
		float minDistToEnemy = float.MaxValue;
		List<ActorData> actors = GameFlowData.Get().GetActors();
		foreach (ActorData otherActor in actors)
		{
			if (otherActor.GetTeam() == actorData.GetTeam())
			{
				continue;
			}
			BoardSquare enemySquare = otherActor.GetCurrentBoardSquare();
			if (enemySquare == null)
			{
				continue;
			}
			float distToEnemy = currentBoardSquare.HorizontalDistanceOnBoardTo(enemySquare);
			if (distToEnemy < minDistToEnemy)
			{
				minDistToEnemy = distToEnemy;
				nearestEnemy = otherActor;
			}
		}
		return nearestEnemy;
	}

	public void MoveToDestination()
	{
		if (m_destination == null) return;
		ActorData actorData = GetComponent<ActorData>();
		ActorMovement actorMovement = GetComponent<ActorMovement>();
		ActorTurnSM actorTurnSM = GetComponent<ActorTurnSM>();
		BoardSquare targetSquare = Board.Get().GetSquareFromTransform(m_destination);
		if (actorData != null
		    && actorMovement != null
		    && actorTurnSM != null
		    && targetSquare != null)
		{
			BoardSquare closestMoveableSquareTo = actorMovement.GetClosestMoveableSquareTo(targetSquare);
			if (closestMoveableSquareTo != null
			    && closestMoveableSquareTo != actorData.GetCurrentBoardSquare())
			{
				actorTurnSM.SelectMovementSquareForMovement(closestMoveableSquareTo);
			}
		}
	}
}
