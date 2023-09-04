// ROGUES
// SERVER
using System;
using System.Collections.Generic;
using UnityEngine;

// added in rogues
#if SERVER
[CreateAssetMenu(fileName = "NpcBrainBehavior", menuName = "Design Objects/Npc Brain Behavior")]
[Serializable]
public class BrainBehavior : ScriptableObject
{
	public bool m_abilityBeforeMovement;
	public int m_abilityActionsPerTurn = 1;
	public BrainAbility[] m_abilities = new BrainAbility[5];
	public List<BrainBehaviorCondition> m_behaviorRequirements;
	public AbilityTiebreakerType m_tiebreakerRule = AbilityTiebreakerType.HighestDamage;

	[Header("Positioning data: score each possible movement square, move to highest. Score with respect to other characters is averaged")]
	[Tooltip("How close/far do I want to be from each enemy")]
	public float m_movementOptimalRangeFromEnemy = 2f;
	[Tooltip("How close/far do I want to be from each ally")]
	public float m_movementOptimalRangeFromAlly = -1f;
	[Tooltip("Optimal Range From Enemy has this amount of wiggle room closer or farther to still be considered optimal")]
	public float m_movementOptimalRangeWindowEnemy = 1f;
	[Tooltip("Optimal Range From Ally has this amount of wiggle room closer or farther to still be considered optimal")]
	public float m_movementOptimalRangeWindowAlly = 1f;
	[Tooltip("For each square closer another character is than Optimal Range - Window, subtract this much from the score")]
	public float m_movementTooCloseLinearPenalty = 30f;
	[Tooltip("For each square farther another character is than Optimal Range + Window, subtract this much from the score")]
	public float m_movementTooFarLinearPenalty = 15f;
	[Header("LoS requirements for positioning")]
	[Tooltip("Not per actor, only applied once. If I don't have line of sight to any enemies, subtract this much from the score")]
	public float m_movementNoLoSToEnemyPenalty = 150f;
	[Tooltip("Not per actor, only applied once. If I don't have line of sight to any allies, subtract this much from the score")]
	public float m_movementNoLoSToAllyPenalty;
	[Header("Other modifiers on square score for positioning")]
	[Tooltip("How much more than default do I care about position with respect to enemies.")]
	public float m_movementEnemyScoreMult = 1f;
	[Tooltip("How much more than default do I care about position with respect to allies.")]
	public float m_movementAllyScoreMult = 1f;
	[Tooltip("How much should I preference lower health characters. EnemyScoreMult (or AllyScoreMult) += LowerHealthScoreMult * PercentOfHealthGone")]
	public float m_movementLowerHealthScoreMult;
	[Tooltip("Not per actor, only applied once. score += CoverRating * CoverScoreMult. CoverRating is 1 for each enemy I have full cover from and 0.5 for each enemy I have half cover from.")]
	public float m_movementCoverScoreMult = 40f;
	[Tooltip("Not per actor, only applied once. If I have to sprint to reach this square, subtract this much from the score")]
	public float m_movementSprintScorePenalty = 70f;
	[Tooltip("Not per actor, only applied once. If I am already in this square, subtract this much from the score")]
	public float m_movementStandStillScorePenalty = 30f;
	[Header("Distance definitions - nearby is only for the targeting conditions, engagement is for ignoring far away actors")]
	[Tooltip("Nearby is only relevant for the following behavior requirements: Requirement_NearbyEnemyHP_StrictTarget, Requirement_NearbyAllyHP_StrictTarget, Requirement_NearbyEnemyCount, Requirement_NearbyAllyCount")]
	public float m_nearbyDefinition = 4f;
	[Tooltip("Characters outside this range will not be part of a square's score calculations. Unless nobody is in this range, then all characters are included.")]
	public float m_engagementRange = 10f;
	public string DebugName { get; set; }
	public ActorData BehaviorDemandedTarget { get; set; }
}
#endif