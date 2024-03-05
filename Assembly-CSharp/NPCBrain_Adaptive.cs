// ROGUES
// SERVER
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCBrain_Adaptive : NPCBrain
{
	// removed in rogues
	public string m_name;
	// removed in rogues
	public BotDifficulty m_botDifficulty = BotDifficulty.Hard;
	// removed in rogues
	public bool m_canTaunt;
	// removed in rogues
	public bool m_inactiveUntilPlayerEncountered;
	// removed in rogues
	public float m_evasionScoreTweak = 1f;
	// removed in rogues
	public float m_damageScoreTweak = 1f;
	// removed in rogues
	public float m_healingScoreTweak = 1f;
	// removed in rogues
	public float m_shieldingScoreTweak = 1f;
	// removed in rogues
	public float m_multipleEnemyTweak = 1f;
	// removed in rogues
	public float m_multipleAllyTweak = 1f;
	
#if SERVER	
	// added in rogues
	public MovementType m_movementType;
	// added in rogues
	public float m_optimalRange = 7.5f;
#endif
	
	public int[] m_allowedAbilities;
	public bool m_logReasoning; // internal in rogues
	public bool m_sendReasoningToTeamChat; // internal in rogues
	
	// removed in rogues
	[HideInInspector]
	public bool m_playerEncountered;
	
	protected static bool s_gatherRealResults = true; // private in reactor
	
#if SERVER	
	// added in rogues
	protected Dictionary<AbilityData.ActionType, PotentialChoice> m_potentialChoices;
	// added in rogues
	private Vector3 s_vectorZero = new Vector3(0f, 0f, 0f);

	// added in rogues
	private const float damageMultiplier = 1f;
	// added in rogues
	private const float healingMultiplier = 1f;
	// added in rogues
	private const float damageNearDeathMultiplier = 0.1f;
	// added in rogues
	private const float damageFarFromDeathMultiplier = 10f;
	// added in rogues
	private const float healingNearDeathMultiplier = 0.1f;
	// added in rogues
	private const float fatalDamageFlatBonus = 2f;
#endif

	public bool isReplacingHuman { get; set; }

	public override NPCBrain Create(BotController bot, Transform destination)
	{
		NPCBrain_Adaptive nPCBrain_Adaptive = bot.gameObject.AddComponent<NPCBrain_Adaptive>();
		
		// removed in rogues
		nPCBrain_Adaptive.m_botDifficulty = m_botDifficulty;
		nPCBrain_Adaptive.m_inactiveUntilPlayerEncountered = m_inactiveUntilPlayerEncountered;
		nPCBrain_Adaptive.m_playerEncountered = false;
		nPCBrain_Adaptive.m_evasionScoreTweak = m_evasionScoreTweak;
		nPCBrain_Adaptive.m_damageScoreTweak = m_damageScoreTweak;
		nPCBrain_Adaptive.m_healingScoreTweak = m_healingScoreTweak;
		nPCBrain_Adaptive.m_shieldingScoreTweak = m_shieldingScoreTweak;
		nPCBrain_Adaptive.m_multipleEnemyTweak = m_multipleEnemyTweak;
		nPCBrain_Adaptive.m_multipleAllyTweak = m_multipleAllyTweak;
		// end removed in rogues
		
		nPCBrain_Adaptive.m_allowedAbilities = m_allowedAbilities;
		
		// reactor
		MakeFSM(nPCBrain_Adaptive);
		if (GameManager.Get().GameConfig.HasGameOption(GameOptionFlag.EnableTeamAIOutput))
		{
			nPCBrain_Adaptive.m_sendReasoningToTeamChat = true;
		}
		// rogues
		nPCBrain_Adaptive.m_movementType = m_movementType;
		// nPCBrain_Adaptive.m_sendReasoningToTeamChat = true;
		
		return nPCBrain_Adaptive;
	}
	
	// removed in rogues
	public static NPCBrain Create(BotController bot, Transform destination, BotDifficulty botDifficulty, bool canTaunt)
	{
		NPCBrain_Adaptive nPCBrain_Adaptive = bot.gameObject.AddComponent<NPCBrain_Adaptive>();
		nPCBrain_Adaptive.m_botDifficulty = botDifficulty;
		nPCBrain_Adaptive.m_canTaunt = canTaunt;
		nPCBrain_Adaptive.m_inactiveUntilPlayerEncountered = false;
		nPCBrain_Adaptive.m_playerEncountered = false;
		nPCBrain_Adaptive.MakeFSM(nPCBrain_Adaptive);
		if (GameManager.Get().GameConfig.HasGameOption(GameOptionFlag.EnableTeamAIOutput))
		{
			nPCBrain_Adaptive.m_sendReasoningToTeamChat = true;
		}
		
#if SERVER
		// custom
		Ability primaryAbility = bot.GetComponent<AbilityData>()?.m_ability0;
		ActorData actorData = bot.GetComponent<ActorData>();
		CharacterType? characterType = actorData?.m_characterType;
		if (primaryAbility != null && Board.Get() != null)
		{
			nPCBrain_Adaptive.m_optimalRange = primaryAbility.GetRangeInSquares(0) * Board.Get().squareSize * 0.8f;
			Log.Info($"Setting optimal range for {characterType} to {nPCBrain_Adaptive.m_optimalRange} " +
			         $"({(nPCBrain_Adaptive.m_optimalRange >= 4.5f ? MovementType.Ranged : MovementType.Melee)})");
		}
		
		if (characterType.HasValue)
		{
			CharacterResourceLink characterResourceLink = actorData.GetCharacterResourceLink();
			if (characterResourceLink == null)
			{
				Log.Warning($"Failed to check {characterType}'s role for bot configuration");
			}
			else
			{
				if (characterResourceLink.m_characterRole == CharacterRole.Support)
				{
					nPCBrain_Adaptive.m_movementType = MovementType.Support;
					Log.Info($"Setting movement type for {characterType} to {nPCBrain_Adaptive.m_movementType}");
				}
			}
		}
#endif
		
		return nPCBrain_Adaptive;
	}

	// rogues
	// public static NPCBrain CreateDefault(BotController bot, Transform destination)
	// {
	// 	NPCBrain_Adaptive nPCBrain_Adaptive = bot.gameObject.AddComponent<NPCBrain_Adaptive>();
	// 	nPCBrain_Adaptive.m_sendReasoningToTeamChat = true;
	// 	return nPCBrain_Adaptive;
	// }

	public void SendDecisionToTeamChat(bool val)
	{
		m_sendReasoningToTeamChat = val;
	}

#if SERVER	
	// added in rogues
	public List<AbilityTarget> GeneratePotentialAbilityTargetLocationsCircle(int numDegrees)
	{
		return GeneratePotentialAbilityTargetLocationsCircle(numDegrees, s_vectorZero);
	}

	// added in rogues
	public List<AbilityTarget> GeneratePotentialAbilityTargetLocationsArc(float arcDegrees, int numChecks, Vector3 startingPos, float centerDegree)
	{
		List<AbilityTarget> list = new List<AbilityTarget>();
		Vector3 vector = GetComponent<ActorData>().GetFreePos();
		float num = arcDegrees * 0.5f;
		float num2 = centerDegree - num;
		float num3 = arcDegrees / numChecks;
		if (startingPos != Vector3.zero)
		{
			vector = startingPos;
		}
		for (int i = 0; i < numChecks; i++)
		{
			float num4 = i * num3;
			float num5 = Mathf.Deg2Rad * (num2 + num4);
			float num6 = Mathf.Sin(num5);
			float num7 = Mathf.Cos(num5);
			Vector3 targetWorldPos = vector;
			targetWorldPos.x += num6;
			targetWorldPos.z += num7;
			list.Add(AbilityTarget.CreateAbilityTargetFromWorldPos(targetWorldPos, vector));
		}
		return list;
	}

	// added in rogues
	public List<AbilityTarget> GeneratePotentialAbilityTargetLocationsCircle(int numDegrees, Vector3 startingPos)
	{
		List<AbilityTarget> result = new List<AbilityTarget>();
		Vector3 pos = startingPos != Vector3.zero
			? startingPos
			: GetComponent<ActorData>().GetFreePos();
		
		if (numDegrees <= 0)
		{
			numDegrees = 72;
		}
		float step = 360f / numDegrees;
		for (int i = 0; i < numDegrees; i++)
		{
			float angleRad = Mathf.Deg2Rad * i * step;
			float sin = Mathf.Sin(angleRad);
			float cos = Mathf.Cos(angleRad);
			Vector3 targetWorldPos = pos;
			targetWorldPos.x += sin;
			targetWorldPos.z += cos;
			result.Add(AbilityTarget.CreateAbilityTargetFromWorldPos(targetWorldPos, pos));
		}
		return result;
	}

	// added in rogues
	public List<AbilityTarget> GeneratePotentialAbilityTargetLocationsCircleNearFar(int numDegrees, float threshold)
	{
		List<AbilityTarget> list = new List<AbilityTarget>();
		Vector3 freePos = GetComponent<ActorData>().GetFreePos();
		float num = threshold * 0.5f;
		float num2 = threshold * 1.5f;
		if (numDegrees <= 0)
		{
			numDegrees = 72;
		}
		float num3 = 360f / numDegrees;
		for (int i = 0; i < numDegrees; i++)
		{
			float num4 = Mathf.Deg2Rad * i * num3;
			float num5 = Mathf.Sin(num4);
			float num6 = Mathf.Cos(num4);
			Vector3 targetWorldPos = freePos;
			targetWorldPos.x += num5 * num;
			targetWorldPos.z += num6 * num;
			list.Add(AbilityTarget.CreateAbilityTargetFromWorldPos(targetWorldPos, freePos));
			Vector3 targetWorldPos2 = freePos;
			targetWorldPos2.x += num5 * num2;
			targetWorldPos2.z += num6 * num2;
			list.Add(AbilityTarget.CreateAbilityTargetFromWorldPos(targetWorldPos2, freePos));
		}
		return list;
	}

	// added in rogues
	public List<AbilityTarget> GeneratePotentialAbilityTargetLocationsCircleNearFar_Separate(int numDegreesNear, int numDegreesFar, float threshold)
	{
		if (numDegreesFar == numDegreesNear)
		{
			return GeneratePotentialAbilityTargetLocationsCircleNearFar(numDegreesNear, threshold);
		}
		if (numDegreesNear <= 0)
		{
			numDegreesNear = 72;
		}
		if (numDegreesFar <= 0)
		{
			numDegreesFar = 72;
		}
		List<AbilityTarget> list = new List<AbilityTarget>();
		Vector3 freePos = GetComponent<ActorData>().GetFreePos();
		float num = threshold * 0.5f;
		float num2 = threshold * 1.5f;
		float num3 = 360f / numDegreesNear;
		for (int i = 0; i < numDegreesNear; i++)
		{
			float num4 = Mathf.Deg2Rad * i * num3;
			float num5 = Mathf.Sin(num4);
			float num6 = Mathf.Cos(num4);
			Vector3 targetWorldPos = freePos;
			targetWorldPos.x += num5 * num;
			targetWorldPos.z += num6 * num;
			list.Add(AbilityTarget.CreateAbilityTargetFromWorldPos(targetWorldPos, freePos));
		}
		float num7 = 360f / numDegreesFar;
		for (int j = 0; j < numDegreesFar; j++)
		{
			float num8 = Mathf.Deg2Rad * j * num7;
			float num9 = Mathf.Sin(num8);
			float num10 = Mathf.Cos(num8);
			Vector3 targetWorldPos2 = freePos;
			targetWorldPos2.x += num9 * num2;
			targetWorldPos2.z += num10 * num2;
			list.Add(AbilityTarget.CreateAbilityTargetFromWorldPos(targetWorldPos2, freePos));
		}
		return list;
	}

	// added in rogues
	public List<AbilityTarget> GeneratePotentialAbilityTargetLocationsCircleVolume(float maxRange, Vector3 startingPos, float overridePct = 0.1f)
	{
		List<AbilityTarget> list = new List<AbilityTarget>();
		float num = maxRange * maxRange;
		for (float num2 = -1f; num2 <= 1f; num2 += overridePct)
		{
			for (float num3 = -1f; num3 <= 1f; num3 += overridePct)
			{
				Vector3 vector = new Vector3(startingPos.x + maxRange * num2, startingPos.y, startingPos.z + maxRange * num3);
				if ((vector - startingPos).sqrMagnitude <= num)
				{
					list.Add(AbilityTarget.CreateAbilityTargetFromWorldPos(vector, startingPos));
				}
			}
		}
		return list;
	}

	// added in rogues
	// Aim between up to three valid targets
	public List<AbilityTarget> GeneratePotentialAbilityTargetLocations(float range, bool includeEnemies, bool includeFriendlies, bool includeSelf)
	{
		List<AbilityTarget> result = new List<AbilityTarget>();
		ActorData actorData = GetComponent<ActorData>();
		List<ActorData> potentialTargets = GetPotentialTargets(actorData, range, includeEnemies, includeFriendlies, includeSelf);
		foreach (ActorData targetA in potentialTargets)
		{
			result.Add(AbilityTarget.CreateAbilityTargetFromBoardSquare(targetA.GetCurrentBoardSquare(), actorData.GetFreePos()));
			bool skipB = false;
			foreach (ActorData targetB in potentialTargets)
			{
				if (!skipB)
				{
					if (targetB == targetA)
					{
						skipB = true;
					}
				}
				else
				{
					foreach (AbilityTarget target in AbilityTarget.CreateAbilityTargetsFromActorDataList(new List<ActorData>
					{
						targetA,
						targetB
					}, actorData))
					{
						result.Add(target);
					}
					bool skipC = false;
					foreach (ActorData targetC in potentialTargets)
					{
						if (!skipC)
						{
							if (targetC == targetB)
							{
								skipC = true;
							}
						}
						else
						{
							foreach (AbilityTarget target in AbilityTarget.CreateAbilityTargetsFromActorDataList(new List<ActorData>
							{
								targetA,
								targetB,
								targetC
							}, actorData))
							{
								result.Add(target);
							}
						}
					}
				}
			}
		}
		return result;
	}

	// inlined in rogues
	private List<ActorData> GetPotentialTargets(
		ActorData actorData,
		float range,
		bool includeEnemies,
		bool includeFriendlies,
		bool includeSelf)
	{
		BoardSquare currentSquare = actorData.GetCurrentBoardSquare();
		List<ActorData> potentialTargets = new List<ActorData>();
		if (includeEnemies)
		{
			foreach (ActorData enemyActor in actorData.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList())
			{
				if (GetEnemyPlayerAliveAndVisibleMultiplier(enemyActor) != 0f
				    && currentSquare.HorizontalDistanceOnBoardTo(enemyActor.GetCurrentBoardSquare()) <= range)
				{
					potentialTargets.Add(enemyActor);
				}
			}
		}
		if (includeFriendlies)
		{
			foreach (ActorData allyActor in GameFlowData.Get().GetAllTeamMembers(actorData.GetTeam()))
			{
				if (!allyActor.IsDead() && allyActor != actorData && !allyActor.IgnoreForAbilityHits)
				{
					BoardSquare allySquare = allyActor.GetCurrentBoardSquare();
					if (allySquare != null && currentSquare.HorizontalDistanceOnBoardTo(allySquare) <= range)
					{
						potentialTargets.Add(allyActor);
					}
				}
			}
		}
		if (includeSelf)
		{
			potentialTargets.Add(actorData);
		}

		return potentialTargets;
	}

	// added in rogues
	public override IEnumerator DecideAbilities()
	{
		HydrogenConfig config = HydrogenConfig.Get();
		float iterationStartTime = Time.realtimeSinceStartup;
		ActorData actorData = GetComponent<ActorData>();
		bool useFastBotAI = config.UseFastBotAI;
		AbilityData abilityData = GetComponent<AbilityData>();
		ActorTurnSM component = GetComponent<ActorTurnSM>();
		BotController botController = GetComponent<BotController>();
		if (!abilityData || !actorData || !component || !botController)
		{
			yield break;
		}
		if (m_potentialChoices == null)
		{
			m_potentialChoices = new Dictionary<AbilityData.ActionType, PotentialChoice>();
		}
		if (GetLinkedActorIfAny(actorData, true))
		{
			yield break;
		}
		for (int abilityIndex = (int)AbilityData.ActionType.ABILITY_6; abilityIndex >= (int)AbilityData.ActionType.ABILITY_0; abilityIndex--)
		{
			AbilityData.ActionType actionType = (AbilityData.ActionType)abilityIndex;
			Ability abilityOfActionType = abilityData.GetAbilityOfActionType(actionType);
			if (abilityOfActionType == null)
			{
				continue;
			}
			if (m_allowedAbilities != null
			    && m_allowedAbilities.Length != 0
			    && m_allowedAbilities.All(allowedAbility => allowedAbility != abilityIndex))
			{
				continue;
			}
			if (!abilityData.ValidateActionIsRequestable(actionType))
			{
				continue;
			}
			
			if (abilityOfActionType.GetNumTargets() == 0)
			{
				yield return StartCoroutine(ScoreZeroTargetAbility(actionType));
			}
			else if (abilityOfActionType.GetNumTargets() == 1
			         || abilityOfActionType is RampartGrab
			         || abilityOfActionType is ThiefBasicAttack  // custom
			         || abilityOfActionType is FishManCone)  // custom
			{
				Ability.TargetingParadigm targetingParadigm = abilityOfActionType.GetTargetingParadigm(0);
				if (targetingParadigm == Ability.TargetingParadigm.Position)
				{
					yield return StartCoroutine(ScoreSinglePositionTargetAbility(actionType));
				}
				else if (targetingParadigm == Ability.TargetingParadigm.BoardSquare)
				{
					yield return StartCoroutine(ScoreSingleBoardSquareTargetAbility(actionType));
				}
				else if (targetingParadigm == Ability.TargetingParadigm.Direction)
				{
					yield return StartCoroutine(ScoreSingleDirectionTargetAbility(actionType));
				}
			}
			else
			{
				yield return StartCoroutine(ScoreMultiTargetAbility(actionType));
			}
			if (iterationStartTime + config.MaxAIIterationTime < Time.realtimeSinceStartup)
			{
				yield return null;
				iterationStartTime = Time.realtimeSinceStartup;
			}
		}
		AbilityData.ActionType bestPrimaryActionType = AbilityData.ActionType.INVALID_ACTION;
		PotentialChoice bestPrimaryChoice = null;
		foreach (KeyValuePair<AbilityData.ActionType, PotentialChoice> keyValuePair in m_potentialChoices)
		{
			if (keyValuePair.Value != null
			    && !keyValuePair.Value.freeAction
			    && (bestPrimaryChoice == null || keyValuePair.Value.score > bestPrimaryChoice.score))
			{
				bestPrimaryActionType = keyValuePair.Key;
				bestPrimaryChoice = keyValuePair.Value;
			}
		}

		// custom
		for (AbilityData.ActionType j = AbilityData.ActionType.ABILITY_0; j <= AbilityData.ActionType.CARD_2; j++)
		{
			if (j == AbilityData.ActionType.ABILITY_5
			    || j == AbilityData.ActionType.ABILITY_6)
			{
				continue;
			}
			
			string abilityName = abilityData.GetAbilityOfActionType(j)?.m_abilityName ?? "N/A";
			PotentialChoice choice = m_potentialChoices.TryGetValue(j);
			if (choice != null)
			{
				Log.Info("{0} best target for ability {1} - {2} - Reasoning:\n{3}Final score: {4}",
					actorData.DisplayName,
					j,
					abilityName,
					choice.reasoning,
					choice.score);
			}
			else if (!abilityData.ValidateActionIsRequestable(j))
			{
				Log.Info("{0} ability not requestable {1} - {2}",
					actorData.DisplayName,
					j,
					abilityName);
			}
			else
			{
				Log.Warning("{0} no options for ability {1} - {2}",
					actorData.DisplayName,
					j,
					abilityName);
			}
		}
		// end custom
		
		BoardSquare dashTarget = null;
		if (bestPrimaryActionType != AbilityData.ActionType.INVALID_ACTION
		    && bestPrimaryChoice != null
		    && bestPrimaryChoice.score > 0f)
		{
			Ability ability = abilityData.GetAbilityOfActionType(bestPrimaryActionType);
			dashTarget = bestPrimaryChoice.destinationSquare;
			int abilityVisualId = (int)(1 + bestPrimaryActionType);
			if (abilityVisualId > 5)
			{
				abilityVisualId -= 2;
			}
			if (m_logReasoning)
			{
				Log.Info("{0} choosing ability {1} - {2} - Reasoning:\n{3}Final score: {4}",
					actorData.DisplayName, abilityVisualId, ability.m_abilityName, bestPrimaryChoice.reasoning, bestPrimaryChoice.score);
			}
			if (m_sendReasoningToTeamChat)
			{
				UIQueueListPanel.UIPhase uiphaseFromAbilityPriority = UIQueueListPanel.GetUIPhaseFromAbilityPriority(ability.GetRunPriority());
				string text = "<color=red>";
				if (uiphaseFromAbilityPriority == UIQueueListPanel.UIPhase.Prep)
				{
					text = "<color=green>";
				}
				else if (uiphaseFromAbilityPriority == UIQueueListPanel.UIPhase.Evasion)
				{
					text = "<color=yellow>";
				}
				ServerGameManager.Get().SendUnlocalizedConsoleMessage(
					$"<color=white>{actorData.DisplayName}</color> " +
					$"choosing ability {text}{abilityVisualId} - {ability.m_abilityName}</color> - " +
					$"Reasoning:\n{bestPrimaryChoice.reasoning}" +
					$"Final Score: <color=yellow>{bestPrimaryChoice.score}</color>",
					Team.Invalid,
					ConsoleMessageType.TeamChat,
					actorData.DisplayName);
			}
			if (ability.IsSimpleAction())
			{
				GetComponent<ServerActorController>().ProcessCastSimpleActionRequest(bestPrimaryActionType, true);
			}
			else
			{
				botController.RequestAbility(bestPrimaryChoice.targetList, bestPrimaryActionType);
			}
		}
		BotManager.Get().BotAIAbilitySelected(actorData, dashTarget, m_optimalRange);
		foreach (KeyValuePair<AbilityData.ActionType, PotentialChoice> potentialChoice in m_potentialChoices)
		{
			if (potentialChoice.Value == null || !potentialChoice.Value.freeAction)
			{
				continue;
			}
			
			if (m_allowedAbilities != null
			    && m_allowedAbilities.Length != 0
			    && m_allowedAbilities.All(allowedAbility => allowedAbility != (int)potentialChoice.Key))
			{
				continue;
			}
			
			Ability ability = abilityData.GetAbilityOfActionType(potentialChoice.Key);
			if (ability == null)
			{
				continue;
			}

			bool castFreeAction = !(ability is RageBeastSelfHeal) || actorData.HitPoints < 65;
			if (castFreeAction)
			{
				if (ability.IsSimpleAction())
				{
					GetComponent<ServerActorController>().ProcessCastSimpleActionRequest(potentialChoice.Key, true);
				}
				else
				{
					botController.RequestAbility(potentialChoice.Value.targetList, potentialChoice.Key);
				}
			}
		}
		m_potentialChoices.Clear();
	}

	// added in rogues
	protected virtual IEnumerator ScoreZeroTargetAbility(AbilityData.ActionType thisAction)
	{
		Ability ability = GetComponent<AbilityData>().GetAbilityOfActionType(thisAction);
		ActorData actorData = GetComponent<ActorData>();
		List<AbilityTarget> targets = AbilityTarget.AbilityTargetList(ability.CreateAbilityTargetForSimpleAction(actorData));
		AbilityResults tempAbilityResults = new AbilityResults(actorData, ability, null, s_gatherRealResults, true);
		ability.GatherAbilityResults(targets, actorData, ref tempAbilityResults);
		PotentialChoice potentialChoice = ScoreResults(tempAbilityResults, actorData, true);
		potentialChoice.freeAction = ability.IsFreeAction();
		potentialChoice.targetList = targets;
		// custom
		if (tempAbilityResults.Ability.CanRunInPhase(AbilityPriority.Evasion))
		{
			AdjustScoreForEvasion(actorData, potentialChoice, tempAbilityResults.Ability);
		}
		// end custom
		if (potentialChoice.score != 0f || potentialChoice.freeAction)
		{
			m_potentialChoices[thisAction] = potentialChoice;
		}
		yield break;
	}

	// added in rogues
	protected virtual IEnumerator ScoreSinglePositionTargetAbility(AbilityData.ActionType thisAction)
	{
		AbilityData abilityData = GetComponent<AbilityData>();
		Ability ability = abilityData.GetAbilityOfActionType(thisAction);
		ActorData actorData = GetComponent<ActorData>();
		PotentialChoice retVal = null;
		HydrogenConfig config = HydrogenConfig.Get();
		List<AbilityTarget> potentialTargets = null;
		if (ability.Targeter is AbilityUtil_Targeter_ChargeAoE
		    || ability.Targeter is AbilityUtil_Targeter_Charge
		    || ability.Targeter is AbilityUtil_Targeter_Shape
		    || ability.Targeter is AbilityUtil_Targeter_BazookaGirlDelayedMissile
		    || ability.Targeter is AbilityUtil_Targeter_MultipleShapes
		    || ability.Targeter is AbilityUtil_Targeter_Grid) // custom
		{
			float range = ability.m_targetData[0].m_range;
			float minRange = ability.m_targetData[0].m_minRange;
			Vector3 boundsSize = new Vector3(range * Board.Get().squareSize * 2f, 2f, range * Board.Get().squareSize * 2f);
			Vector3 boundsPosition = actorData.transform.position;
			boundsPosition.y = 0f;
			Bounds bounds = new Bounds(boundsPosition, boundsSize);
			List<BoardSquare> squaresInBox = Board.Get().GetSquaresInBox(bounds);
			List<AbilityTarget> tempAbilityTargets = new List<AbilityTarget>();
			foreach (BoardSquare boardSquare in squaresInBox)
			{
				if (boardSquare == actorData.GetCurrentBoardSquare()
				    && (ability.Targeter is AbilityUtil_Targeter_ChargeAoE
				        || ability.Targeter is AbilityUtil_Targeter_Charge))
				{
					continue;
				}

				if (boardSquare.HorizontalDistanceInSquaresTo_Squared(actorData.GetCurrentBoardSquare()) <= 2f
				    && ability.Targeter is AbilityUtil_Targeter_Charge
				    && boardSquare.OccupantActor != null
				    && boardSquare.OccupantActor.GetTeam() == actorData.GetTeam())
				{
					continue;
				}

				if (!abilityData.IsTargetSquareInRangeOfAbilityFromSquare(
					    boardSquare, actorData.GetCurrentBoardSquare(), range, minRange))
				{
					continue;
				}

				AbilityTarget abilityTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
				if (ability.CustomTargetValidation(actorData, abilityTarget, 0, null))
				{
					if (potentialTargets == null)
					{
						potentialTargets = new List<AbilityTarget>();
					}
					tempAbilityTargets.Clear();
					tempAbilityTargets.Add(abilityTarget);
					if (abilityData.ValidateActionRequest(thisAction, tempAbilityTargets, false))
					{
						potentialTargets.Add(abilityTarget);
					}
				}
			}
		}
		else if (ability.Targeter is AbilityUtil_Targeter_AoE_AroundActor)
		{
			List<ActorData> allies = GameFlowData.Get().GetAllTeamMembers(actorData.GetTeam());
			List<AbilityTarget> tempAbilityTargets = new List<AbilityTarget>();
			foreach (ActorData ally in allies)
			{
				BoardSquare allySquare = ally.GetCurrentBoardSquare();
				if (!ally.IsDead()
				    && allySquare != null
				    && !ally.IgnoreForAbilityHits)
				{
					AbilityTarget target = AbilityTarget.CreateAbilityTargetFromActor(ally, actorData);
					if (ability.CustomTargetValidation(actorData, target, 0, null))
					{
						if (potentialTargets == null)
						{
							potentialTargets = new List<AbilityTarget>();
						}
						tempAbilityTargets.Clear();
						tempAbilityTargets.Add(target);
						if (abilityData.ValidateActionRequest(thisAction, tempAbilityTargets, false))
						{
							potentialTargets.Add(target);
						}
					}
				}
			}
		}
		else if (ability.Targeter is AbilityUtil_Targeter_RampartKnockbackBarrier)
		{
			List<BoardSquare> targetSquares = new List<BoardSquare>(4);
			BoardSquare boardSquare = actorData.GetCurrentBoardSquare();
			Board.Get().GetCardinalAdjacentSquares(boardSquare.x, boardSquare.y, ref targetSquares);
			potentialTargets = new List<AbilityTarget>(4);
			foreach (BoardSquare secondTargetSquare in targetSquares)
			{
				var target = AbilityTarget.CreateAbilityTargetFromBoardSquare(
					secondTargetSquare, boardSquare.ToVector3());
				potentialTargets.Add(target);
			}
		}
		else
		{
			Log.Error($"Single position targeter is not supported by bots: {ability.Targeter.GetType()} ({ability.GetType()})"); // custom
		}
		
		if (potentialTargets != null)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (AbilityTarget item in potentialTargets)
			{
				List<AbilityTarget> targetList = new List<AbilityTarget> { item };
				AbilityResults abilityResults = new AbilityResults(actorData, ability, null, s_gatherRealResults, true);
				ability.GatherAbilityResults(targetList, actorData, ref abilityResults);
				if (!ability.m_chainAbilities.IsNullOrEmpty())
				{
					foreach (Ability chainAbility in ability.m_chainAbilities)
					{
						AbilityResults chainAbilityResults = new AbilityResults(actorData, chainAbility, null, s_gatherRealResults, true);
						chainAbility.GatherAbilityResults(targetList, actorData, ref chainAbilityResults);
						foreach (KeyValuePair<ActorData, ActorHitResults> hitResult in chainAbilityResults.m_actorToHitResults)
						{
							abilityResults.m_actorToHitResults.Add(hitResult.Key, hitResult.Value);
						}
						foreach (KeyValuePair<ActorData, int> damageResult in chainAbilityResults.DamageResults)
						{
							if (abilityResults.DamageResults.ContainsKey(damageResult.Key))
							{
								abilityResults.DamageResults[damageResult.Key] += damageResult.Value;
							}
							else
							{
								abilityResults.DamageResults[damageResult.Key] = damageResult.Value;
							}
						}
					}
				}
				PotentialChoice potentialChoice = ScoreResults(abilityResults, actorData, false);
				potentialChoice.freeAction = ability.IsFreeAction();
				potentialChoice.targetList = targetList;
				// custom
				if (abilityResults.Ability.CanRunInPhase(AbilityPriority.Evasion))
				{
					AdjustScoreForEvasion(actorData, potentialChoice, abilityResults.Ability);
				}
				// end custom
				if (retVal == null || retVal.score < potentialChoice.score)
				{
					retVal = potentialChoice;
				}
				if (realtimeSinceStartup + config.MaxAIIterationTime < Time.realtimeSinceStartup)
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
			}
		}
		if (retVal != null && retVal.score > 0f)
		{
			m_potentialChoices[thisAction] = retVal;
		}
	}

	// added in rogues
	protected virtual IEnumerator ScoreSingleBoardSquareTargetAbility(AbilityData.ActionType thisAction)
	{
		PotentialChoice retVal = null;
		AbilityData component = GetComponent<AbilityData>();
		Ability thisAbility = component.GetAbilityOfActionType(thisAction);
		ActorData actorData = GetComponent<ActorData>();
		BoardSquare currentSquare = actorData.GetCurrentBoardSquare();
		HydrogenConfig config = HydrogenConfig.Get();
		List<AbilityTarget> potentialTargets = null;
		switch (thisAbility.Targeter)
		{
			case AbilityUtil_Targeter_ChargeAoE _:
			case AbilityUtil_Targeter_Charge _:
			case AbilityUtil_Targeter_Shape _:
			case AbilityUtil_Targeter_RocketJump _:
			case AbilityUtil_Targeter_ScoundrelEvasionRoll _:
			{
				float range = thisAbility.m_targetData[0].m_range;
				float minRange = thisAbility.m_targetData[0].m_minRange;
				Vector3 vector = new Vector3(range * Board.Get().squareSize * 2f, 2f, range * Board.Get().squareSize * 2f);
				Vector3 position = actorData.transform.position;
				position.y = 0f;
				Bounds bounds = new Bounds(position, vector);
				foreach (BoardSquare boardSquare in Board.Get().GetSquaresInBox(bounds))
				{
					if (boardSquare == actorData.GetCurrentBoardSquare())
					{
						continue;
					}

					if (!component.IsTargetSquareInRangeOfAbilityFromSquare(boardSquare,
						    actorData.GetCurrentBoardSquare(), range, minRange) || !boardSquare.IsValidForGameplay())
					{
						continue;
					}

					if (thisAbility is SparkEnergized
					    && !actorData.GetComponent<SparkBeamTrackerComponent>().GetBeamActors().Contains(boardSquare.OccupantActor))
					{
						continue;
					}
				
					AbilityTarget target = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
					if (thisAbility.CustomTargetValidation(actorData, target, 0, null))
					{
						if (potentialTargets == null)
						{
							potentialTargets = new List<AbilityTarget>();
						}
						potentialTargets.Add(target);
					}
				}

				break;
			}
			case AbilityUtil_Targeter_TeslaPrison targeter:
			{
				List<ActorData> enemies = actorData.GetOtherTeams()
					.SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam))
					.ToList();
			
				foreach (ActorData enemy in enemies)
				{
					if (GetEnemyPlayerAliveAndVisibleMultiplier(enemy) == 0f)
					{
						continue;
					}
				
					AbilityTarget targetOnEnemy = AbilityTarget.CreateAbilityTargetFromBoardSquare(enemy.GetCurrentBoardSquare(), actorData.GetFreePos());
					List<BoardSquare> squaresInShape = AreaEffectUtils.GetSquaresInShape(targeter.m_shapeForActorHits, targetOnEnemy, true, actorData);
					foreach (BoardSquare square in squaresInShape)
					{
						if (square == null || !square.IsValidForGameplay())
						{
							continue;
						}
						AbilityTarget target = AbilityTarget.CreateAbilityTargetFromBoardSquare(square, actorData.GetFreePos());
						if (thisAbility.CustomTargetValidation(actorData, target, 0, AbilityTarget.AbilityTargetList(target)))
						{
							if (potentialTargets == null)
							{
								potentialTargets = new List<AbilityTarget>();
							}
							potentialTargets.Add(target);
						}
					}
				}

				break;
			}
			case AbilityUtil_Targeter_TrackerDrone _:
			{
				List<AbilityTarget> targets = GeneratePotentialAbilityTargetLocations(thisAbility.m_targetData[0].m_range, true, false, false);
				foreach (AbilityTarget target in targets)
				{
					if (thisAbility.CustomTargetValidation(actorData, target, 0, AbilityTarget.AbilityTargetList(target)))
					{
						if (potentialTargets == null)
						{
							potentialTargets = new List<AbilityTarget>();
						}
						potentialTargets.Add(target);
					}
				}
				if (GameFlowData.Get().CurrentTurn == 1)
				{
					int x = Board.Get().GetMaxX() / 2;
					int y = Board.Get().GetMaxY() / 2;
					BoardSquare centerSquare = Board.Get().GetSquareFromIndex(x, y);
					if (centerSquare != null && centerSquare.IsValidForGameplay())
					{
						AbilityTarget item = AbilityTarget.CreateAbilityTargetFromBoardSquare(centerSquare, actorData.GetFreePos());
						if (potentialTargets == null)
						{
							potentialTargets = new List<AbilityTarget>();
						}
						potentialTargets.Add(item);
					}
				}

				break;
			}
			case AbilityUtil_Targeter_AoE_AroundActor _:
			{
				List<ActorData> allies = GameFlowData.Get().GetAllTeamMembers(actorData.GetTeam());
				List<AbilityTarget> tempTargetList = new List<AbilityTarget>();
				foreach (ActorData ally in allies)
				{
					BoardSquare currentBoardSquare = ally.GetCurrentBoardSquare();
					if (!ally.IsDead()
					    && currentBoardSquare != null
					    && !ally.IgnoreForAbilityHits)
					{
						AbilityTarget target = AbilityTarget.CreateAbilityTargetFromActor(ally, actorData);
						if (thisAbility.CustomTargetValidation(actorData, target, 0, null))
						{
							if (potentialTargets == null)
							{
								potentialTargets = new List<AbilityTarget>();
							}
							tempTargetList.Clear();
							tempTargetList.Add(target);
							if (component.ValidateActionRequest(thisAction, tempTargetList, false))
							{
								potentialTargets.Add(target);
							}
						}
					}
				}

				break;
			}
			case AbilityUtil_Targeter_ValkyrieGuard _:
			{
				List<BoardSquare> targetSquares = new List<BoardSquare>(4);
				BoardSquare boardSquare = actorData.GetCurrentBoardSquare();
				Board.Get().GetCardinalAdjacentSquares(boardSquare.x, boardSquare.y, ref targetSquares);
				potentialTargets = new List<AbilityTarget>(4);
				foreach (BoardSquare secondTargetSquare in targetSquares)
				{
					var target = AbilityTarget.CreateAbilityTargetFromBoardSquare(
						secondTargetSquare, boardSquare.ToVector3());
					potentialTargets.Add(target);
				}

				break;
			}
			default:
				Log.Error($"Single board square targeter is not supported by bots: {thisAbility.Targeter.GetType()} ({thisAbility.GetType()})"); // custom
				break;
		}
		
		if (potentialTargets != null)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (AbilityTarget target in potentialTargets)
			{
				List<AbilityTarget> targetList = new List<AbilityTarget> { target };
				AbilityResults abilityResults = new AbilityResults(actorData, thisAbility, null, s_gatherRealResults, true);
				thisAbility.GatherAbilityResults(targetList, actorData, ref abilityResults);
				PotentialChoice potentialChoice = ScoreResults(abilityResults, actorData, false);
				potentialChoice.freeAction = thisAbility.IsFreeAction();
				potentialChoice.targetList = targetList;
				// custom
				if (abilityResults.Ability.CanRunInPhase(AbilityPriority.Evasion))
				{
					AdjustScoreForEvasion(actorData, potentialChoice, abilityResults.Ability);
				}
				// end custom
				float weight = 1f;
				if (potentialChoice != null
				    && thisAbility.Targeter is AbilityUtil_Targeter_TrackerDrone)
				{
					weight = 0.375f;
					BoardSquare square = Board.Get().GetSquare(target.GridPos);
					potentialChoice.score += square.HorizontalDistanceInSquaresTo(currentSquare) * 0.05f;
				}
				if (potentialChoice != null
				    && (thisAbility.Targeter is AbilityUtil_Targeter_TeslaPrison
				        || thisAbility.Targeter is AbilityUtil_Targeter_TrackerDrone))
				{
					BoardSquare square = Board.Get().GetSquare(target.GridPos);
					List<ActorData> enemies = actorData.GetOtherTeams()
						.SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam))
						.ToList();
					foreach (ActorData enemy in enemies)
					{
						if (GetEnemyPlayerAliveAndVisibleMultiplier(enemy) == 0f)
						{
							continue;
						}
						
						BoardSquare enemySquare = enemy.GetCurrentBoardSquare();
						float dist = square.HorizontalDistanceInSquaresTo(enemySquare);
						if (dist == 0f)
						{
							potentialChoice.score += 30f * weight;
						}
						if (dist <= 1f)
						{
							potentialChoice.score += 35f * weight;
						}
						else if (dist < 2f)
						{
							potentialChoice.score += 40f * weight;
						}
						else if (dist < 3f)
						{
							potentialChoice.score += 20f * weight;
						}
					}
				}
				if (retVal == null || retVal.score < potentialChoice.score)
				{
					retVal = potentialChoice;
				}
				if (realtimeSinceStartup + config.MaxAIIterationTime < Time.realtimeSinceStartup)
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
			}
		}
		if (retVal != null && retVal.score > 0f)
		{
			m_potentialChoices[thisAction] = retVal;
		}
	}

	// added in rogues
	private bool IsSquareOccupiedByAliveActor(BoardSquare square)
	{
		foreach (ActorData actorData in GameFlowData.Get().GetActors())
		{
			if (!actorData.IsDead() && square == actorData.CurrentBoardSquare)
			{
				return true;
			}
		}
		return false;
	}

	// added in rogues
	private PotentialChoice AdjustScoreForEvasion(ActorData actorData, PotentialChoice choice, Ability thisAbility)
	{
		AbilityData component = GetComponent<AbilityData>();
		if (GameFlowData.Get().CurrentTurn == 1)
		{
			choice.score -= 200f;
			choice.reasoning += "Subtracting 200 score - don't evade on turn 1!\n";
		}
		if (GameFlowData.Get().CurrentTurn == actorData.NextRespawnTurn)
		{
			choice.score /= 2f;
			choice.reasoning += "Reduce the score by 50% - don't evade on respawn turns.\n";
		}
		bool flag = m_optimalRange > 4.5;
		if (flag && actorData.GetHitPointPercent() > 0.8)
		{
			choice.score /= 2f;
			choice.reasoning += "Reduce the score by 50% - ranged characters shouldn't really use their evade at high health even if they will do damage.\n";
		}
		ActorCover component2 = actorData.GetComponent<ActorCover>();
		if (flag && component2 != null && component2.AmountOfCover(actorData.CurrentBoardSquare) > 1 && actorData.GetHitPointPercent() > 0.75f)
		{
			choice.score /= 2f;
			choice.reasoning += "Reduce the score by 50% - don't use shift if you have decent cover and a ranged";
		}
		float num = 0f;
		BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
		BoardSquare square = Board.Get().GetSquare(choice.targetList[0].GridPos);
		if (actorData.m_characterType == CharacterType.Spark && choice.targetList.Count == 2)
		{
			square = Board.Get().GetSquare(choice.targetList[1].GridPos);
			if (GameFlowData.Get().CurrentTurn == 2)
			{
				choice.score -= 200f;
				choice.reasoning += "Subtracting 200 score - quark evading on turn 2 is generally bad.\n";
			}
		}
		if (actorData.m_characterType == CharacterType.Gremlins && choice.targetList.Count > 0)
		{
			square = Board.Get().GetSquare(choice.targetList[choice.targetList.Count - 1].GridPos);
		}
		if (actorData.m_characterType == CharacterType.Sensei && choice.targetList.Count > 0)
		{
			square = Board.Get().GetSquare(choice.targetList[choice.targetList.Count - 1].GridPos);
		}
		// custom
		if (actorData.m_characterType == CharacterType.Neko && choice.targetList.Count > 0)
		{
			square = Board.Get().GetSquare(choice.targetList[choice.targetList.Count - 1].GridPos);
		}
		// end custom
		choice.destinationSquare = square;
		if (!IsSquareOccupiedByAliveActor(square))
		{
			float num2 = 0f;
			if (actorData.GetHitPointPercent() < 0.5)
			{
				if (thisAbility.GetEvasionTeleportType() != ActorData.TeleportType.NotATeleport
				    && square.IsInBrush()
				    && BrushCoordinator.Get().IsRegionFunctioning(square.BrushRegion))
				{
					num += 20f;
				}
				List<ActorData> list = actorData.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList();
				foreach (ActorData actorData2 in list)
				{
					if (GetEnemyPlayerAliveAndVisibleMultiplier(actorData2) == 0f)
					{
						continue;
					}
					BoardSquare currentBoardSquare2 = actorData2.GetCurrentBoardSquare();
					Vector3 vector = currentBoardSquare.transform.position - currentBoardSquare2.transform.position;
					Vector3 vector2 = square.transform.position - currentBoardSquare2.transform.position;
					float magnitude = vector.magnitude;
					float magnitude2 = vector2.magnitude;
					if (magnitude <= 6f * Board.Get().squareSize)
					{
						num2 += 1f;
					}
					vector.Normalize();
					vector2.Normalize();
					float num3 = Vector3.Dot(vector, vector2);
					float num4;
					if (num3 > 0.9f)
					{
						num4 = 0.25f;
					}
					else if (num3 > 0.25f)
					{
						num4 = Mathf.Sqrt(num3);
					}
					else if (choice.score > 0f)
					{
						num4 = 0.5f;
					}
					else
					{
						num4 = 0.5f + (num3 + 1f) / 2.5f;
					}
					if (actorData.m_characterType == CharacterType.Tracker)
					{
						num4 += 1f;
					}
					if (choice.score == 0f)
					{
						num4 *= 20f;
					}
					if (magnitude < 4.5f)
					{
						num += 20f * (Mathf.Abs(magnitude2 - magnitude) / 10f) * num4 / (1f * list.Count);
					}
					else if (magnitude < 9f)
					{
						num += 10f * (Mathf.Abs(magnitude2 - magnitude) / 10f) * num4 / (1f * list.Count);
					}
					else
					{
						num += 5f * (Mathf.Abs(magnitude2 - magnitude) / 10f) * num4 / (1f * list.Count);
					}
					num *= 0.5f;
				}
			}
			foreach (Effect effect in ServerEffectManager.Get().WorldEffects)
			{
				int num5 = 0;
				int num6 = 0;
				if (!(effect.Caster == null) && effect.Caster.GetTeam() == actorData.GetEnemyTeam())
				{
					if (effect is SorceressDamageFieldEffect)
					{
						SorceressDamageFieldEffect sorceressDamageFieldEffect = (SorceressDamageFieldEffect)effect;
						if (sorceressDamageFieldEffect.AffectedSquares.Contains(currentBoardSquare))
						{
							num5 = sorceressDamageFieldEffect.m_damage;
						}
						if (sorceressDamageFieldEffect.AffectedSquares.Contains(square))
						{
							num6 = sorceressDamageFieldEffect.m_damage;
						}
					}
					else
					{
						if (effect.TargetSquare == currentBoardSquare)
						{
							num5 = 10;
						}
						if (effect.TargetSquare == square)
						{
							num6 = 10;
						}
					}
					num += num5;
					num -= num6;
				}
			}
			if (num2 == 0f && choice.score < 10f)
			{
				num = 0f;
			}
			else if (num2 >= 2f && choice.score < 10f)
			{
				num *= 1.25f;
			}
			if (num != 0f)
			{
				if (actorData.GetHitPointPercent() < 0.4f && choice.score == 0f)
				{
					choice.score += 20f * num2;
					choice.reasoning += "Adding score if you are low health and your evade does no damage so you will not shoot and just dodge";
				}
				choice.score += num;
				choice.reasoning +=
					$"Adding {num} based on the quality of the evade (destination position & world effects at start and destination)\n";
			}
		}
		if (actorData.GetCharacterResourceLink().m_characterRole != CharacterRole.Tank && actorData.GetHitPointPercent() > 0.9f && actorData.TechPoints < 100)
		{
			choice.score *= 0.5f;
			choice.reasoning += "Not a frontline & over 90% hp: Dividing score by 2.\n";
		}
		if (!HasNoDashOffCooldown(actorData, false) && component.GetActionTypeOfAbility(thisAbility) == AbilityData.ActionType.CARD_1)
		{
			choice.score *= 0.25f;
			choice.reasoning += "Reduce the score by 75% - don't use a card shift if you have a normal shift";
		}
		return choice;
	}

	// added in rogues
	protected IEnumerator ScoreSingleDirectionTargetAbility(AbilityData.ActionType thisAction)
	{
		AbilityData abilityData = GetComponent<AbilityData>();
		Ability ability = abilityData.GetAbilityOfActionType(thisAction);
		ActorData actorData = GetComponent<ActorData>();
		PotentialChoice retVal = null;
		bool includeFriendlies = true;
		bool includeEnemies = true;
		bool includeSelf = false;
		if (ability is TutorialAttack)
		{
			includeFriendlies = false;
		}
		List<AbilityTarget> potentialTargets = null;
		switch (ability.Targeter)
		{
			case AbilityUtil_Targeter_ExoTether targeterExoTether: // custom -- moved up as it was unreachable in rogues
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterExoTether.GetDistance(), includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_Laser targeterLaser:
				potentialTargets = ability is BlasterDelayedLaser
					? GeneratePotentialAbilityTargetLocationsCircle(360)
					: GeneratePotentialAbilityTargetLocations(targeterLaser.m_distance, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_Blindfire targeterBlindfire:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterBlindfire.m_coneLengthRadiusInSquares, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_ChainLightningLaser targeterChainLightningLaser:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterChainLightningLaser.m_distance, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_MultipleCones targeterMultipleCones:
				potentialTargets = GeneratePotentialAbilityTargetLocationsCircleVolume(targeterMultipleCones.m_maxConeLengthRadius * Board.Get().squareSize, actorData.GetCurrentBoardSquare().transform.position);
				break;
			case AbilityUtil_Targeter_ThiefFanLaser targeterThiefFanLaser:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterThiefFanLaser.m_rangeInSquares, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_BounceLaser targeterBounceLaser:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterBounceLaser.m_maxDistancePerBounce, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_BounceActor targeterBounceActor:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterBounceActor.m_maxDistancePerBounce, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_LaserWithCone targeterLaserWithCone:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterLaserWithCone.m_distance, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_DirectionCone targeterDirectionCone:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterDirectionCone.m_coneLengthRadius, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_CrossBeam targeterCrossBeam:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterCrossBeam.m_distanceInSquares, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_ClaymoreKnockbackLaser targeterClaymoreKnockbackLaser:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterClaymoreKnockbackLaser.GetLaserRange(), includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_ClaymoreCharge targeterClaymoreCharge:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterClaymoreCharge.m_dashRangeInSquares, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_ClaymoreSlam targeterClaymoreSlam:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterClaymoreSlam.m_laserRange, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_RampartGrab targeterRampartGrab:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterRampartGrab.m_laserRange, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_TricksterLaser targeterTricksterLaser:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterTricksterLaser.m_distance, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_TricksterCones targeterTricksterCones:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterTricksterCones.m_coneInfo.m_radiusInSquares * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_StretchCone targeterStretchCone:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterStretchCone.m_maxLengthSquares * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_ConeOrLaser _:
				potentialTargets = ability is SoldierConeOrLaser soldierConeOrLaser
					? GeneratePotentialAbilityTargetLocationsCircleNearFar_Separate(20, 72, soldierConeOrLaser.m_coneDistThreshold)
					: GeneratePotentialAbilityTargetLocations(ability.GetRangeInSquares(0) * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
				break;
			// unreachable in rogues
			// case AbilityUtil_Targeter_ExoTether targeterExoTether:
			// 	potentialTargets = GeneratePotentialAbilityTargetLocations(targeterExoTether.GetDistance(), includeEnemies, includeFriendlies, includeSelf);
			// 	break;
			case AbilityUtil_Targeter_SweepSingleClickCone targeterSweepSingleClickCone:
				if (actorData.TechPoints >= 70 || targeterSweepSingleClickCone.m_syncComponent.m_anchored)
				{
					potentialTargets = GeneratePotentialAbilityTargetLocations(targeterSweepSingleClickCone.m_rangeInSquares * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
				}
				break;
			case AbilityUtil_Targeter_DashThroughWall targeterDashThroughWall:
				potentialTargets = GeneratePotentialAbilityTargetLocations((targeterDashThroughWall.m_dashRangeInSquares + targeterDashThroughWall.m_extraTotalDistanceIfThroughWalls) * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_ReverseStretchCone targeterReverseStretchCone:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterReverseStretchCone.m_maxLengthSquares * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_AoE_Smooth_FixedOffset targeterAoESmoothFixedOffset:
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterAoESmoothFixedOffset.m_maxOffsetFromCaster * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_SpaceMarineBasicAttack targeterSpaceMarineBasicAttack: // custom
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterSpaceMarineBasicAttack.m_lengthInSquares, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_Line targeterLine: // custom TODO BOTS Sniper drone trap
				potentialTargets = GeneratePotentialAbilityTargetLocations(targeterLine.m_lineRange, includeEnemies, includeFriendlies, includeSelf);
				break;
			case AbilityUtil_Targeter_RampartKnockbackBarrier targeterRampartKnockbackBarrier: // custom TODO BOTS Rampart wall
				potentialTargets = GeneratePotentialAbilityTargetLocations(8f, includeEnemies, includeFriendlies, includeSelf);
				break;
			default:
				Log.Error($"Single direction targeter is not supported by bots: {ability.Targeter.GetType()} ({ability.GetType()})"); // custom
				break;
		}
		
		if (potentialTargets != null)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (AbilityTarget target in potentialTargets)
			{
				List<AbilityTarget> targetList = new List<AbilityTarget> { target };
				AbilityResults tempAbilityResults = new AbilityResults(actorData, ability, null, s_gatherRealResults, true);
				ability.GatherAbilityResults(targetList, actorData, ref tempAbilityResults);
				PotentialChoice potentialChoice = ScoreResults(tempAbilityResults, actorData, false);
				potentialChoice.freeAction = ability.IsFreeAction();
				potentialChoice.targetList = targetList;
				// custom
				if (tempAbilityResults.Ability.CanRunInPhase(AbilityPriority.Evasion))
				{
					AdjustScoreForEvasion(actorData, potentialChoice, tempAbilityResults.Ability);
				}
				// end custom
				if (retVal == null || retVal.score < potentialChoice.score)
				{
					retVal = potentialChoice;
				}
				if (realtimeSinceStartup + HydrogenConfig.Get().MaxAIIterationTime < Time.realtimeSinceStartup)
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
			}
		}
		if (retVal != null && retVal.score > 0f)
		{
			m_potentialChoices[thisAction] = retVal;
		}
	}

	// added in rogues
	protected IEnumerator ScoreMultiTargetAbility(AbilityData.ActionType thisAction)
	{
		// custom
		AbilityData abilityData = GetComponent<AbilityData>();
		Ability ability = abilityData.GetAbilityOfActionType(thisAction);
		
		// TODO BOTS broken code
		// rogues
		// PotentialChoice potentialChoice = null;
		// if (potentialChoice != null)
		// {
		// 	m_potentialChoices[thisAction] = potentialChoice;
		// }
		
		// custom
		ActorData actorData = GetComponent<ActorData>();
		PotentialChoice retVal = null;
		bool includeFriendlies = true;
		bool includeEnemies = true;
		bool includeSelf = false;
		List<List<AbilityTarget>> potentialTargets = new List<List<AbilityTarget>>();
		switch (ability.Targeter)
		{
			case AbilityUtil_Targeter_BendingLaser targeterBendingLaser:
			{
				List<AbilityTarget> targets = GeneratePotentialAbilityTargetLocationsCircleVolume(
					targeterBendingLaser.m_maxDistanceBeforeBend,
					actorData.GetFreePos(),
					0.1f);
				List<AbilityTarget> emptyList = new List<AbilityTarget>();
				foreach (AbilityTarget firstTarget in targets)
				{
					if (!ability.CustomTargetValidation(actorData, firstTarget, 0, emptyList))
					{
						continue;
					}

					List<AbilityTarget> secondTargets = GeneratePotentialAbilityTargetLocations(
						targeterBendingLaser.m_maxTotalDistance,
						includeEnemies,
						includeFriendlies,
						includeSelf);
					List<AbilityTarget> currentTargets = new List<AbilityTarget> { firstTarget };
					foreach (AbilityTarget secondTarget in secondTargets)
					{
						if (!ability.CustomTargetValidation(actorData, secondTarget, 1, currentTargets))
						{
							continue;
						}

						potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
					}
				}

				break;
			}
			case AbilityUtil_Targeter_DashAndAim _:
			{
				List<BoardSquare> squares = AreaEffectUtils.GetSquaresInRadius(
					actorData.GetCurrentBoardSquare(),
					ability.GetRangeInSquares(0),
					false,
					actorData);
				List<AbilityTarget> emptyList = new List<AbilityTarget>();
				foreach (BoardSquare firstTargetSquare in squares)
				{
					AbilityTarget firstTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(firstTargetSquare, actorData.GetFreePos());
					if (!ability.CustomTargetValidation(actorData, firstTarget, 0, emptyList))
					{
						continue;
					}

					List<AbilityTarget> secondTargets = GeneratePotentialAbilityTargetLocationsCircle(36);
					foreach (AbilityTarget secondTarget in secondTargets)
					{
						potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
					}
				}

				break;
			}
			case AbilityUtil_Targeter_DirectionCone targeterDirectionCone:
			{
				if (ability.Targeters.Count != 2 || !(ability.Targeters[1] is AbilityUtil_Targeter_Laser targeterLaser))
				{
					goto default;
				}
				
				List<AbilityTarget> targets = GeneratePotentialAbilityTargetLocations(
					targeterDirectionCone.m_coneLengthRadius,
					includeEnemies,
					includeFriendlies,
					includeSelf);
				List<AbilityTarget> secondTargets = GeneratePotentialAbilityTargetLocations(
					targeterLaser.m_distance,
					includeEnemies,
					includeFriendlies,
					includeSelf);
				foreach (AbilityTarget firstTarget in targets)
				{
					foreach (AbilityTarget secondTarget in secondTargets)
					{
						potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
					}
				}

				break;
			}
			case AbilityUtil_Targeter_ChargeAoE targeterChargeAoE:
			case AbilityUtil_Targeter_BombingRun targeterBombingRun:
			{
				if (ability.Targeters.Count != 2
				    && !(ability is TricksterCatchMeIfYouCan && ability.Targeters.Count == 3))
				{
					goto default;
				}

				if (!(ability.Targeters[1] is AbilityUtil_Targeter_ConeOrLaser)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_ChargeAoE)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_BombingRun)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_RampartKnockbackBarrier)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_Barrier))
				{
					goto default;
				}

				float range = ability.m_targetData[0].m_range;
				float minRange = ability.m_targetData[0].m_minRange;
				Vector3 boundsSize = new Vector3(range * Board.Get().squareSize * 2f, 2f, range * Board.Get().squareSize * 2f);
				Vector3 boundsPosition = actorData.transform.position;
				boundsPosition.y = 0f;
				Bounds bounds = new Bounds(boundsPosition, boundsSize);
				List<BoardSquare> squaresInBox = Board.Get().GetSquaresInBox(bounds);
				foreach (BoardSquare boardSquare in squaresInBox)
				{
					if (boardSquare == actorData.GetCurrentBoardSquare())
					{
						continue;
					}

					if (!abilityData.IsTargetSquareInRangeOfAbilityFromSquare(
						    boardSquare, actorData.GetCurrentBoardSquare(), range, minRange))
					{
						continue;
					}

					AbilityTarget firstTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
					if (ability.CustomTargetValidation(actorData, firstTarget, 0, null))
					{
						if (ability is SoldierDashAndOverwatch soldierDashAndOverwatch)
						{
							List<AbilityTarget> secondTargets = GeneratePotentialAbilityTargetLocationsCircleNearFar_Separate(
								20, 
								72,
								soldierDashAndOverwatch.m_coneDistThreshold);
							foreach (AbilityTarget secondTarget in secondTargets)
							{
								potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
							}
						}
						else if (ability is ThiefOnTheRun || ability is GremlinsBombingRun)
						{
							float range2 = ability.m_targetData[1].m_range;
							float minRange2 = ability.m_targetData[1].m_minRange;
							Vector3 boundsSize2 = new Vector3(range2 * Board.Get().squareSize * 2f, 2f, range2 * Board.Get().squareSize * 2f);
							Vector3 boundsPosition2 = actorData.transform.position;
							boundsPosition2.y = 0f;
							Bounds bounds2 = new Bounds(boundsPosition2, boundsSize2);
							List<BoardSquare> squaresInBox2 = Board.Get().GetSquaresInBox(bounds2);
							List<AbilityTarget> currentTargets = new List<AbilityTarget> {firstTarget};
							foreach (BoardSquare boardSquare2 in squaresInBox2)
							{
								if (boardSquare2 == boardSquare)
								{
									continue;
								}

								if (!abilityData.IsTargetSquareInRangeOfAbilityFromSquare(
									    boardSquare2, actorData.GetCurrentBoardSquare(), range, minRange2))
								{
									continue;
								}

								AbilityTarget secondTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare2, boardSquare.ToVector3());
								if (ability.CustomTargetValidation(actorData, secondTarget, 1, currentTargets))
								{
									potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
								}
							}
						}
						else if (ability is TricksterCatchMeIfYouCan)
						{
							List<AbilityTarget> firstTargetAsList = AbilityTarget.AbilityTargetList(firstTarget);
							NestedAddTarget(squaresInBox, actorData, ability, firstTargetAsList, potentialTargets);
						}
						else if (ability is RampartDashAndAimShield)
						{
							List<BoardSquare> secondTargetSquares = new List<BoardSquare>(4);
							Board.Get().GetCardinalAdjacentSquares(boardSquare.x, boardSquare.y, ref secondTargetSquares);
							foreach (BoardSquare secondTargetSquare in secondTargetSquares)
							{
								var secondTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(
									secondTargetSquare, boardSquare.ToVector3());
								potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
							}
						}
						else
						{
							goto default;
						}
					}
				}

				break;
			}
			case AbilityUtil_Targeter_Charge targeterCharge:
			{
				if (ability.Targeters.Count != 2)
				{
					goto default;
				}

				if (!(ability.Targeters[1] is AbilityUtil_Targeter_ValkyrieGuard)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_StretchCone)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_Charge))
				{
					goto default;
				}

				float range = ability.m_targetData[0].m_range;
				float minRange = ability.m_targetData[0].m_minRange;
				Vector3 boundsSize = new Vector3(range * Board.Get().squareSize * 2f, 2f, range * Board.Get().squareSize * 2f);
				Vector3 boundsPosition = actorData.transform.position;
				boundsPosition.y = 0f;
				Bounds bounds = new Bounds(boundsPosition, boundsSize);
				List<BoardSquare> squaresInBox = Board.Get().GetSquaresInBox(bounds);
				foreach (BoardSquare boardSquare in squaresInBox)
				{
					if (boardSquare == actorData.GetCurrentBoardSquare())
					{
						continue;
					}

					if (!abilityData.IsTargetSquareInRangeOfAbilityFromSquare(
						    boardSquare, actorData.GetCurrentBoardSquare(), range, minRange))
					{
						continue;
					}

					AbilityTarget firstTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
					if (ability.CustomTargetValidation(actorData, firstTarget, 0, null))
					{
						if (ability is ValkyrieDashAoE)
						{
							List<BoardSquare> secondTargetSquares = new List<BoardSquare>(4);
							Board.Get().GetCardinalAdjacentSquares(
								boardSquare.x, boardSquare.y, ref secondTargetSquares);
							foreach (BoardSquare secondTargetSquare in secondTargetSquares)
							{
								var secondTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(
									secondTargetSquare, boardSquare.ToVector3());
								potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
							}
						}
						else if (ability is BlasterDashAndBlast
						         && ability.Targeters[1] is AbilityUtil_Targeter_StretchCone targeterStretchCone)
						{
							List<AbilityTarget> potentialSecondTargets = GeneratePotentialAbilityTargetLocations(
								targeterStretchCone.m_maxLengthSquares * Board.Get().squareSize,
								includeEnemies,
								includeFriendlies,
								includeSelf);
							
							foreach (AbilityTarget secondTarget in potentialSecondTargets)
							{
								potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
							}
						}
						else if (ability is SparkDash || ability is SenseiYingYangDash) // TODO BOTS we could optimize first targeter
						{
							float range2 = ability.m_targetData[1].m_range;
							float minRange2 = ability.m_targetData[1].m_minRange;
							Vector3 boundsSize2 = new Vector3(range2 * Board.Get().squareSize * 2f, 2f, range2 * Board.Get().squareSize * 2f);
							Vector3 boundsPosition2 = boardSquare.ToVector3();
							boundsPosition2.y = 0f;
							Bounds bounds2 = new Bounds(boundsPosition2, boundsSize2);
							List<BoardSquare> squaresInBox2 = Board.Get().GetSquaresInBox(bounds2);
							List<AbilityTarget> firstTargetAsList = AbilityTarget.AbilityTargetList(firstTarget);
							foreach (BoardSquare boardSquare2 in squaresInBox2)
							{
								if (boardSquare2 == boardSquare)
								{
									continue;
								}

								if (!abilityData.IsTargetSquareInRangeOfAbilityFromSquare(
									    boardSquare2, boardSquare, range2, minRange2))
								{
									continue;
								}
								
								AbilityTarget secondTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare2, boardSquare.ToVector3());
								if (ability.CustomTargetValidation(actorData, secondTarget, 1, firstTargetAsList))
								{
									potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
								}
							}
						}
						else
						{
							goto default;
						}
					}
				}

				break;
			}
			case AbilityUtil_Targeter_Shape _:
			case AbilityUtil_Targeter_GremlinsBombInCone _:
			case AbilityUtil_Targeter_CapsuleAoE _:
			{
				if (ability.Targeters.Count != 2
				    && !(ability is ThiefSmokeBomb && ability.Targeters.Count == 3)
				    && !(ability is ArcherArrowRain && ability.Targeters.Count == 3)
				    && !(ability is GremlinsMultiTargeterApocolypse && ability.Targeters.Count >= 4))
				{
					goto default;
				}

				if (!(ability.Targeters[1] is AbilityUtil_Targeter_Shape)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_Barrier)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_GremlinsBombInCone)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_ChargeAoE)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_SoldierCardinalLines)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_CapsuleAoE)) 
				{
					goto default;
				}

				ICollection<BoardSquare> targetSquares;
				float range = ability.m_targetData[0].m_range;
				float minRange = ability.m_targetData[0].m_minRange;
				if (ability is GremlinsMultiTargeterApocolypse) // gotta limit the number of squares we operate upon
				// TODO still to much + coroutinize?
				{
					List<ActorData> enemyTargets = GetPotentialTargets(actorData,  range, true, false, false);
					targetSquares = new HashSet<BoardSquare>();
					List<BoardSquare> adjacentSquares = new List<BoardSquare>(8);
					foreach (ActorData enemyTarget in enemyTargets)
					{
						adjacentSquares.Clear();
						BoardSquare enemySquare = enemyTarget.GetCurrentBoardSquare();
						targetSquares.Add(enemySquare);
						Board.Get().GetAllAdjacentSquares(enemySquare.x, enemySquare.y, ref adjacentSquares);
						foreach (BoardSquare adjacentSquare in adjacentSquares)
						{
							if (adjacentSquare.IsValidForGameplay())
							{
								targetSquares.Add(adjacentSquare);
							}
						}
					}
				}
				else
				{
					Vector3 boundsSize = new Vector3(range * Board.Get().squareSize * 2f, 2f, range * Board.Get().squareSize * 2f);
					Vector3 boundsPosition = actorData.transform.position;
					boundsPosition.y = 0f;
					Bounds bounds = new Bounds(boundsPosition, boundsSize);
					targetSquares = Board.Get().GetSquaresInBox(bounds);
				}
				foreach (BoardSquare boardSquare in targetSquares)
				{
					if (boardSquare == actorData.GetCurrentBoardSquare() && !(ability is NanoSmithBarrier))
					{
						continue;
					}

					if (!abilityData.IsTargetSquareInRangeOfAbilityFromSquare(
						    boardSquare, actorData.GetCurrentBoardSquare(), range, minRange))
					{
						continue;
					}

					AbilityTarget firstTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
					List<AbilityTarget> firstTargetAsList = AbilityTarget.AbilityTargetList(firstTarget);
					if (ability.CustomTargetValidation(actorData, firstTarget, 0, null))
					{
						if (ability is ThiefSmokeBomb
						    || ability is GremlinsMultiTargeterBasicAttack
						    || ability is GremlinsMultiTargeterApocolypse)
						{
							if (!NestedAddTarget(targetSquares, actorData, ability, firstTargetAsList, potentialTargets))
							{
								goto default;
							}
						}
						else if (ability is NanoSmithBarrier 
						         || ability is SoldierCardinalLine
						         || ability is NinjaShurikenOrDash)
						{
							bool isAllDirections = ability is NinjaShurikenOrDash;
							bool onlyValid = ability is NinjaShurikenOrDash;
							List<BoardSquare> secondTargetSquares = new List<BoardSquare>(isAllDirections ? 8 : 4);
							if (isAllDirections)
							{
								Board.Get().GetAllAdjacentSquares(boardSquare.x, boardSquare.y, ref secondTargetSquares);
							}
							else
							{
								Board.Get().GetCardinalAdjacentSquares(boardSquare.x, boardSquare.y, ref secondTargetSquares);
							}
							foreach (BoardSquare secondTargetSquare in secondTargetSquares)
							{
								if (onlyValid && !secondTargetSquare.IsValidForGameplay())
								{
									continue;
								}
								var secondTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(
									secondTargetSquare, boardSquare.ToVector3());
								potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget });
							}
						}
						else if (ability is ArcherArrowRain archerArrowRain)
						{
							if (ability.GetNumTargets() < 2 || ability.GetNumTargets() > 3)
							{
								goto default;
							}
							List<BoardSquare> secondTargetSquares = AreaEffectUtils.GetSquaresInRadius(boardSquare, archerArrowRain.GetMaxRangeBetween(), true, actorData);
							foreach (BoardSquare secondTargetSquare in secondTargetSquares)
							{
								float dist = Vector3.Distance(boardSquare.ToVector3(), secondTargetSquare.ToVector3());
								if (dist >= archerArrowRain.GetMinRangeBetween() * Board.Get().squareSize)
								{
									AbilityTarget secondTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(secondTargetSquare, actorData.GetFreePos());
									if (ability.CustomTargetValidation(actorData, secondTarget, 1, firstTargetAsList))
									{
										List<AbilityTarget> prevTargets = new List<AbilityTarget> { firstTarget, secondTarget };

										if (ability.GetNumTargets() == prevTargets.Count)
										{
											potentialTargets.Add(prevTargets);
										}
										else
										{
											List<BoardSquare> thirdTargetSquares = AreaEffectUtils.GetSquaresInRadius(secondTargetSquare, archerArrowRain.GetMaxRangeBetween(), true, actorData);
											foreach (BoardSquare thirdTargetSquare in thirdTargetSquares)
											{
												float dist2 = Vector3.Distance(secondTargetSquare.ToVector3(), thirdTargetSquare.ToVector3());
												if (dist2 >= archerArrowRain.GetMinRangeBetween() * Board.Get().squareSize)
												{
													AbilityTarget thirdTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(thirdTargetSquare, actorData.GetFreePos());
													if (ability.CustomTargetValidation(actorData, thirdTarget, 2, prevTargets))
													{
														potentialTargets.Add(new List<AbilityTarget> { firstTarget, secondTarget, thirdTarget });
													}
												}
											}
										}
									}
								}
							}
						}
						else
						{
							goto default;
						}
					}
				}

				break;
			}
			case AbilityUtil_Targeter_Laser targeterLaser:
			{
				if (ability.Targeters.Count != 3 || ability.GetTargetData().Length != 2)
				{
					goto default;
				}

				if (!(ability.Targeters[1] is AbilityUtil_Targeter_NekoCharge)) 
				{
					goto default;
				}
				
				List<AbilityTarget> potentialFirstTargets = GeneratePotentialAbilityTargetLocations(
					targeterLaser.m_distance, includeEnemies, false, false);
				potentialFirstTargets = potentialFirstTargets
					.Where(t => ability.CustomTargetValidation(actorData, t, 0, null))
					.ToList();
				
				float range = ability.m_targetData[1].m_range;
				float minRange = ability.m_targetData[1].m_minRange;
				Vector3 boundsSize = new Vector3(range * Board.Get().squareSize * 2f, 2f, range * Board.Get().squareSize * 2f);
				Vector3 boundsPosition = actorData.transform.position;
				boundsPosition.y = 0f;
				Bounds bounds = new Bounds(boundsPosition, boundsSize);
				List<BoardSquare> squaresInBox = Board.Get().GetSquaresInBox(bounds);
				List<AbilityTarget> potentialSecondTargets = new List<AbilityTarget>();
				foreach (BoardSquare boardSquare in squaresInBox)
				{
					if (boardSquare == actorData.GetCurrentBoardSquare())
					{
						continue;
					}

					if (!abilityData.IsTargetSquareInRangeOfAbilityFromSquare(
						    boardSquare, actorData.GetCurrentBoardSquare(), range, minRange))
					{
						continue;
					}

					AbilityTarget secondTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
					if (ability.CustomTargetValidation(actorData, secondTarget, 1, null))
					{
						potentialSecondTargets.Add(secondTarget);
					}
				}
				
				foreach (AbilityTarget potentialFirstTarget in potentialFirstTargets)
				{
					foreach (AbilityTarget potentialSecondTarget in potentialSecondTargets)
					{
						if (ability.CustomTargetValidation(actorData, potentialSecondTarget, 1,
							    AbilityTarget.AbilityTargetList(potentialFirstTarget)))
						{
							potentialTargets.Add(new List<AbilityTarget> { potentialFirstTarget, potentialSecondTarget });
						}
					}
				}

				break;
			}
			default:
			{
				Log.Error($"Multi targeter is not supported by bots: {ability.Targeter.GetType()} ({ability.GetType()})");
				break;
			}
		}
		
		if (potentialTargets.Count > 0)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (List<AbilityTarget> targetList in potentialTargets)
			{
				if (!abilityData.ValidateActionRequest(thisAction, targetList, false))
				{
					continue;
				}
				AbilityResults tempAbilityResults = new AbilityResults(actorData, ability, null, s_gatherRealResults, true);
				ability.GatherAbilityResults(targetList, actorData, ref tempAbilityResults);
				PotentialChoice potentialChoice = ScoreResults(tempAbilityResults, actorData, false);
				potentialChoice.freeAction = ability.IsFreeAction();
				potentialChoice.targetList = targetList;
				// custom
				if (tempAbilityResults.Ability.CanRunInPhase(AbilityPriority.Evasion))
				{
					AdjustScoreForEvasion(actorData, potentialChoice, tempAbilityResults.Ability);
				}
				// end custom
				if (retVal == null || retVal.score < potentialChoice.score)
				{
					retVal = potentialChoice;
				}
				if (realtimeSinceStartup + HydrogenConfig.Get().MaxAIIterationTime < Time.realtimeSinceStartup)
				{
					yield return null;
					realtimeSinceStartup = Time.realtimeSinceStartup;
				}
			}
		}
		if (retVal != null && retVal.score > 0f)
		{
			m_potentialChoices[thisAction] = retVal;
		}
		// end custom
	}
	
	// custom
	private static bool NestedAddTarget(
		ICollection<BoardSquare> potentialTargetSquares,
		ActorData actorData,
		Ability ability,
		List<AbilityTarget> prevTargets,
		List<List<AbilityTarget>> result)
	{
		foreach (BoardSquare targetSquare in potentialTargetSquares)
		{
			if (prevTargets.Any(t => t.GridPos.Equals(targetSquare.GetGridPos())))
			{
				continue;
			}
								
			AbilityTarget target = AbilityTarget.CreateAbilityTargetFromBoardSquare(targetSquare, actorData.GetFreePos());
			if (ability.CustomTargetValidation(actorData, target, prevTargets.Count, prevTargets))
			{
				List<AbilityTarget> targets = new List<AbilityTarget>(prevTargets.Count + 1);
				targets.AddRange(prevTargets);
				targets.Add(target);
				if (ability.Targeters.Count == targets.Count)
				{
					result.Add(targets);
				}
				else if (ability.Targeters.Count > targets.Count)
				{
					if (!NestedAddTarget(potentialTargetSquares, actorData, ability, targets, result))
					{
						return false;
					}
				}
				else
				{
					Log.Error($"NestedAddTarget for {ability.m_abilityName} is broken!");
					return false;
				}
			}
		}

		return true;
	}

	// added in rogues
	public virtual PotentialChoice ScoreResults(AbilityResults tempAbilityResults, ActorData caster, bool ignoreOverhealing)
	{
		Dictionary<ActorData, int> damageResults = tempAbilityResults.DamageResults;
		PotentialChoice potentialChoice = new PotentialChoice
		{
			damageTotal = 0,
			numEnemyTargetsHit = 0,
			healingTotal = 0,
			numTeamTargetsHit = 0,
			score = 0f,
			reasoning = ""
		};
		// int currentTurn = GameFlowData.Get().CurrentTurn;
		
		// rogues
		// if (tempAbilityResults.Ability.m_additionalAIScore > 0f)
		// {
		// 	potentialChoice.score += tempAbilityResults.Ability.m_additionalAIScore;
		// 	potentialChoice.reasoning += $"Adding {tempAbilityResults.Ability.m_additionalAIScore} for authored additional AI score\n";
		// }
		
		// all was inlined in rogues
		ScoreDamageAndHealing(tempAbilityResults, caster, ignoreOverhealing, damageResults, potentialChoice);
		ScoreActorEffects(tempAbilityResults, potentialChoice);
		ScoreStolenPowerups(tempAbilityResults, caster, potentialChoice);
		ScoreWorldEffects(tempAbilityResults, caster, potentialChoice);
		ScoreBarriers(tempAbilityResults, caster, potentialChoice); // custom
		
		ScoreTargetNum(potentialChoice);

		return potentialChoice;
	}

	private static void ScoreTargetNum(PotentialChoice potentialChoice)
	{
		if (potentialChoice.numEnemyTargetsHit > 1 && potentialChoice.score != 0f)
		{
			float score = potentialChoice.score;
			potentialChoice.score += 0.01f * potentialChoice.numEnemyTargetsHit;
			float enemiesHitScore = potentialChoice.score - score;
			potentialChoice.reasoning +=
				$"Adding a small bonus based on the number of enemy targets hit ({enemiesHitScore}).\n";
		}

		if (potentialChoice.numTeamTargetsHit > 1 && potentialChoice.score != 0f)
		{
			float score = potentialChoice.score;
			potentialChoice.score += 0.01f * potentialChoice.numTeamTargetsHit;
			float alliesHitScore = potentialChoice.score - score;
			potentialChoice.reasoning +=
				$"Adding a small bonus based on the number of friendly targets hit ({alliesHitScore}).\n";
		}
	}

	// custom
	private void ScoreBarriers(AbilityResults tempAbilityResults, ActorData caster, PotentialChoice potentialChoice)
	{
		foreach (KeyValuePair<Vector3, PositionHitResults> posToHitResult in tempAbilityResults.m_positionToHitResults)
		{
			if (posToHitResult.Value == null || posToHitResult.Value.m_barriers == null)
			{
				continue;
			}

			HashSet<ActorData> actorsInRange = new HashSet<ActorData>();
			int maxHits = 1;
			int damage = 0;
			foreach (Barrier barrier in posToHitResult.Value.m_barriers)
			{
				if (barrier == null)
				{
					continue;
				}

				List<ActorData> actorsInRadiusOfLine = AreaEffectUtils.GetActorsInRadiusOfLine(
					barrier.GetEndPos1(),
					barrier.GetEndPos2(),
					1.5f,
					1.5f,
					1.5f,
					false,
					caster,
					caster.GetEnemyTeamAsList(),
					null);
				actorsInRange.UnionWith(actorsInRadiusOfLine);
				maxHits = barrier.MaxHits;
				damage = barrier.OnEnemyMovedThrough.m_damage;
			}

			if (damage == 0)
			{
				Log.Warning($"Barrier is not supported by bots: " +
				            $"({tempAbilityResults.Ability.GetType()})"); // custom
			}

			float damageScore = 0;
			float dashScore = 0;
			foreach (ActorData target in actorsInRange)
			{
				damageScore += ConvertDamageToScore(caster, target, (int)(damage * 0.7f));
				// TODO BOTS also include barrier.OnEnemyMovedThrough.m_effect
				
				if (IsLikelyToDash(target, false))
				{
					dashScore += 8;
				}
			}
			
			if (maxHits > 0 && maxHits < actorsInRange.Count)
			{
				float factor = maxHits / (actorsInRange.Count - 0.85f);
				damageScore *= factor;
				dashScore *= factor;
			}

			potentialChoice.score += damageScore + dashScore;
			potentialChoice.reasoning += $"Added {damageScore} score for projected damage.\n";
			potentialChoice.reasoning += $"Added {dashScore} score for projected dashes.\n";
		}
	}

	// custom
	private static bool IsLikelyToDash(ActorData target, bool includeCards, float exposednessThreshold = 1.5f)
	{
		// TODO BOTS health?
		float exposednessRating = GetExposednessRating(target);
		bool hasNoDashOffCooldown = HasNoDashOffCooldown(target, includeCards);
		return !hasNoDashOffCooldown && exposednessRating > exposednessThreshold;
	}

	// custom
	private static float GetExposednessRating(ActorData target)
	{
		ActorCover actorCover = target.GetActorCover();
		if (actorCover == null)
		{
			return 0;
		}
		float coverRating = actorCover.CoverRating(target.GetCurrentBoardSquare());
		int threatRating = GameFlowData.Get()
			.GetAllTeamMembers(target.GetEnemyTeam())
			.Count(a => !a.IsDead() && a.GetCurrentBoardSquare() != null);
		return threatRating - coverRating;
	}

	private void ScoreWorldEffects(AbilityResults tempAbilityResults, ActorData caster, PotentialChoice potentialChoice)
	{
		foreach (KeyValuePair<Vector3, PositionHitResults> posToHitResult in tempAbilityResults.m_positionToHitResults)
		{
			if (posToHitResult.Value == null || posToHitResult.Value.m_effects == null)
			{
				continue;
			}

			foreach (Effect effect in posToHitResult.Value.m_effects)
			{
				if (effect == null)
				{
					continue;
				}

				switch (effect)
				{
					case SorceressDamageFieldEffect sorceressDamageFieldEffect:
					{
						float score = potentialChoice.score;
						BoardSquare pos = Board.Get().GetSquareFromVec3(posToHitResult.Key);
						Vector3 centerOfShape =
							AreaEffectUtils.GetCenterOfShape(sorceressDamageFieldEffect.m_shape, pos.ToVector3(), pos);
						List<ActorData> actorsInShape = AreaEffectUtils.GetActorsInShape(
							sorceressDamageFieldEffect.m_shape,
							centerOfShape,
							pos,
							sorceressDamageFieldEffect.m_penetrateLoS,
							caster,
							caster.GetOtherTeams(),
							null);
						if (actorsInShape.Count <= 1)
						{
							potentialChoice.score -= 10f;
						}
						else
						{
							foreach (ActorData target in actorsInShape)
							{
								potentialChoice.score += ConvertDamageToScore(caster, target, 5);
							}
						}

						float damageFieldScore = potentialChoice.score - score;
						potentialChoice.reasoning += $"Added {damageFieldScore} score for Aurora damage field.\n";
						break;
					}
					case BazookaGirlDelayedBombDropsEffect bazookaEffect:
					{
						HashSet<BoardSquare> hitSquares = new HashSet<BoardSquare>();
						BazookaGirlDroppedBombInfo bombInfo = bazookaEffect.m_bombInfo;
						foreach (ActorData targetActor in bazookaEffect.m_targetActors)
						{
							List<BoardSquare> squaresInShape = AreaEffectUtils.GetSquaresInShape(
								bombInfo.m_shape,
								targetActor.GetFreePos(),
								targetActor.GetTravelBoardSquare(),
								bombInfo.m_penetrateLos,
								caster);
							hitSquares.UnionWith(squaresInShape);
						}
						foreach (BoardSquare square in hitSquares)
						{
							ActorData hitActor = AreaEffectUtils.GetTargetableActorOnSquare(square, true, false, caster);
							if (hitActor == null)
							{
								continue;
							}

							float factor = IsLikelyToDash(hitActor, true) ? 0.2f : 0.9f;
							float damageScore = ConvertDamageToScore(caster, hitActor, (int)(bombInfo.m_damageAmount * factor));
							potentialChoice.score += damageScore;
							potentialChoice.reasoning += $"Added {damageScore} score for projected damage.\n";
						}
						
						break;
					}
					case StandardGroundEffect _:
					case StandardMultiAreaGroundEffect _:
					{
						// calculating based on current positioning
						float additionalScore = 0;
						float additionalCoverScore = 0;

						GroundEffectField fieldInfo;
						ICollection<BoardSquare> squaresInShape;
						if (effect is StandardGroundEffect standardGroundEffect)
						{
							fieldInfo = standardGroundEffect.m_fieldInfo;
							squaresInShape = standardGroundEffect.GetSquaresInShape();
						}
						else if (effect is StandardMultiAreaGroundEffect standardMultiAreaGroundEffect)
						{
							fieldInfo = standardMultiAreaGroundEffect.m_fieldInfo;
							squaresInShape = standardMultiAreaGroundEffect.GetSquaresInShape();
						}
						else
						{
							goto default;
						}
						
						ActorCover actorCover = caster.GetComponent<ActorCover>();

						foreach (BoardSquare affectedSquare in squaresInShape)
						{
							if (actorCover.AmountOfCover(affectedSquare) > 1)
							{
								additionalCoverScore += 2;
							}
							
							ActorData target = AreaEffectUtils.GetTargetableActorOnSquare(
								affectedSquare, 
								fieldInfo.IncludeEnemies(),
								fieldInfo.IncludeAllies(),
								caster);
							
							if (target == null)
							{
								continue;
							}

							if (target.GetTeam() != caster.GetTeam())
							{
								additionalScore += ConvertDamageToScore(caster, target, fieldInfo.damageAmount);
							}
						}
						
						if (additionalScore == 0)
						{
							additionalScore = -10f;
						}

						potentialChoice.score += additionalScore + additionalCoverScore;
						potentialChoice.reasoning += $"Added {additionalScore} score for projected damage.\n";
						potentialChoice.reasoning += $"Added {additionalCoverScore} score for covered squares in cover.\n";
						break;
					}
					case BlasterDelayedLaserEffect blasterDelayedLaserEffect:
					{
						float damageFieldScore = 0;
						foreach (ActorData target in blasterDelayedLaserEffect.FindHitActors(null))
						{
							damageFieldScore += ConvertDamageToScore(caster, target, 5);
						}

						potentialChoice.score += damageFieldScore;
						potentialChoice.reasoning += $"Added {damageFieldScore} score for Elle's drone.\n";
						break;
					}
					case NekoBoomerangDiscEffect _:
					{
						// TODO BOT NEKO
						break;
					}
					case NekoHomingDiscEffect nekoHomingDiscEffect:
					{
						ActorData target = AreaEffectUtils.GetTargetableActorOnSquare(nekoHomingDiscEffect.TargetSquare, true, false, caster);
						if (target != null)
						{
							float factor = IsLikelyToDash(target, true) ? 0.8f : 0.3f;
							float damageScore = ConvertDamageToScore(caster, target, (int)(nekoHomingDiscEffect.m_returnTripDamage * factor));
							potentialChoice.score += damageScore;
							potentialChoice.reasoning += $"Added {damageScore} score for projected damage.\n";
						}

						break;
					}
					default:
						Log.Warning($"World effect is not supported by bots: {effect.GetType()}" +
						            $"({effect.Parent.Ability?.GetType()}{effect.Parent.Passive?.GetType()})"); // custom
						break;
				}
			}
		}
	}

	private static void ScoreStolenPowerups(
		AbilityResults tempAbilityResults,
		ActorData caster,
		PotentialChoice potentialChoice)
	{
		foreach (KeyValuePair<ActorData, ActorHitResults> actorToHitResult in tempAbilityResults.m_actorToHitResults)
		{
			if (actorToHitResult.Value.m_powerUpsToSteal == null)
			{
				continue;
			}

			foreach (ServerAbilityUtils.PowerUpStealData powerUpStealData in actorToHitResult.Value.m_powerUpsToSteal)
			{
				if (!powerUpStealData.m_powerUp.TeamAllowedForPickUp(caster.GetTeam()))
				{
					continue;
				}

				if (powerUpStealData.m_powerUp.m_ability is PowerUp_Heal_Ability ability)
				{
					potentialChoice.score += ability.m_healAmount;
					potentialChoice.reasoning += $"Adding {ability.m_healAmount} for stealing a heal power up.\n";
				}
				else if (powerUpStealData.m_powerUp.m_ability is PowerUp_Standard_Ability powerUpStandardAbility)
				{
					if (powerUpStandardAbility.m_healAmount != 0)
					{
						potentialChoice.score += powerUpStandardAbility.m_healAmount;
						potentialChoice.reasoning +=
							$"Adding {powerUpStandardAbility.m_healAmount} for stealing a heal power up.\n";
					}
					else if (powerUpStandardAbility.m_effect != null
					         && powerUpStandardAbility.m_effect.m_statusChanges != null
					         && powerUpStandardAbility.m_effect.m_statusChanges.Length >= 0)
					{
						foreach (StatusType status in powerUpStandardAbility.m_effect.m_statusChanges)
						{
							switch (status)
							{
								case StatusType.Empowered:
								{
									potentialChoice.score += 16f;
									potentialChoice.reasoning += "Adding 16 score for stealing a might power up.\n";
									break;
								}
								case StatusType.Hasted:
								{
									potentialChoice.score += 9f;
									potentialChoice.reasoning += "Adding 9 score for stealing a haste power up.\n";
									break;
								}
								case StatusType.Energized:
								{
									potentialChoice.score += 6f;
									potentialChoice.reasoning += "adding 6 score for stealing an energized power up.\n";
									break;
								}
							}
						}
					}
					else
					{
						potentialChoice.score += 5f;
						potentialChoice.reasoning += "Adding 5 score for stealing an unknown power up.\n";
					}
				}
				else
				{
					potentialChoice.score += 5f;
					potentialChoice.reasoning += "Adding 5 score for stealing an unknown power up.\n";
				}
			}
		}
	}

	private static void ScoreActorEffects(AbilityResults tempAbilityResults, PotentialChoice potentialChoice)
	{
		foreach (KeyValuePair<ActorData, ActorHitResults> actorToHitResult in tempAbilityResults.m_actorToHitResults)
		{
			if (actorToHitResult.Value.m_effects == null)
			{
				continue;
			}

			foreach (Effect effect in actorToHitResult.Value.m_effects)
			{
				if (ScoreCustomActorEffect(tempAbilityResults.Caster, tempAbilityResults.Ability, effect, potentialChoice))
				{
					continue;
				}
				
				StandardActorEffectData effectData = GetEffectDataFromActorEffect(effect);
				if (effectData == null)
				{
					Log.Warning($"Actor effect is not supported by bots: {effect.GetType()} " +
					            $"({effect.Parent.Ability?.GetType()}{effect.Parent.Passive?.GetType()})"); // custom
					continue;
				}
				
				float absorb = effectData.m_absorbAmount;
				if (absorb != 0f)
				{
					float score = potentialChoice.score;
					potentialChoice.numTeamTargetsHit++;
					for (int i = 0; i < effectData.m_duration; i++)
					{
						absorb /= 2f;
						potentialChoice.score += absorb;
					}

					potentialChoice.score += (1f - actorToHitResult.Key.GetHitPointPercent()) * 0.1f;
					float shieldScore = potentialChoice.score - score;
					potentialChoice.reasoning += $"Adding {shieldScore} for generic shielding.\n";
				}

				if (effectData.m_statusChanges != null &&
				    effectData.m_statusChanges.Length != 0)
				{
					foreach (StatusType status in effectData.m_statusChanges)
					{
						switch (status)
						{
							case StatusType.InvisibleToEnemies:
							{
								potentialChoice.score += 9f;
								potentialChoice.reasoning += "Adding 9 score for an invisibility effect.\n";
								break;
							}
							case StatusType.Snared:
							{
								potentialChoice.score += 2f * effectData.m_duration;
								potentialChoice.reasoning += "Adding 2 score for a slow effect.\n";
								break;
							}
							case StatusType.Weakened:
							{
								potentialChoice.score += 13f * effectData.m_duration;
								potentialChoice.reasoning += "Adding 13 score for a weakened effect.\n";
								break;
							}
							case StatusType.Empowered:
							{
								potentialChoice.score += 13f * effectData.m_duration;
								potentialChoice.reasoning += "Adding 13 score for a might effect.\n";
								break;
							}
							case StatusType.Unstoppable:
							{
								potentialChoice.score += 7f * effectData.m_duration;
								potentialChoice.reasoning += "Adding 7 score for an Unstoppable effect.\n";
								break;
							}
							case StatusType.Hasted:
							{
								potentialChoice.score += 8f * effectData.m_duration;
								potentialChoice.reasoning += "Adding 8 score for a haste effect.\n";
								break;
							}
						}
					}
				}
			}
		}
	}

	private static StandardActorEffectData GetEffectDataFromActorEffect(Effect effect)
	{
		switch (effect)
		{
			case StandardActorEffect actorEffect:
				return actorEffect.m_data;
			case BazookaGirlStickyBombEffect stickyBombEffect:
				return stickyBombEffect.m_bombInfo.onExplodeEffect.m_effectData;
			default:
				return null;
		}
	}

	private static bool ScoreCustomActorEffect(
		ActorData caster,
		Ability ability,
		Effect effect,
		PotentialChoice potentialChoice)
	{
		ActorData target = effect.Target;
		if (target == null)
		{
			return false;
		}
		switch (effect)
		{
			case DelayedAoeKnockbackEffect _:
			{
				if (HasLOSToEnemiesFromSquare(target, target.CurrentBoardSquare))
				{
					potentialChoice.score += 10;
					potentialChoice.reasoning += "Adding 10 score for LOS to enemies.\n";
				}

				CharacterRole targetRole = target.GetCharacterResourceLink().m_characterRole;
				if (targetRole == CharacterRole.Tank)
				{
					potentialChoice.score += 4;
					potentialChoice.reasoning += "Adding 4 score for frontline.\n";
				}
				else if (targetRole == CharacterRole.Assassin)
				{
					potentialChoice.score += 2;
					potentialChoice.reasoning += "Adding 2 score for firepower.\n";
				}

				return true;
			}
			case NanoSmithWeaponsOfWarEffect _:
			{
				return true; // TODO BOTS could also add damage estimation to Helio's ult
			}
			case BlasterOverchargeEffect _:
			{
				
				BlasterOvercharge blasterOverchargeAbility = caster.GetAbilityData().m_ability2 as BlasterOvercharge;
				if (blasterOverchargeAbility == null)
				{
					return false;
				}

				potentialChoice.score += blasterOverchargeAbility.GetExtraDamage();
				potentialChoice.reasoning +=
					$"Adding {blasterOverchargeAbility.GetExtraDamage()} score for projected damage.\n";
				return true;
			}
			case ArcherHealingReactionEffect archerHealingReactionEffect:
			{
				int numLosToEnemy = GetNumLOSToEnemy(target);
				if (numLosToEnemy > 1)
				{
					float score = numLosToEnemy * archerHealingReactionEffect.ReactionHealing * 0.75f;
					potentialChoice.score += score;
					potentialChoice.reasoning += $"Adding {score} score for projected healing.\n";
				}

				return false; // also process standard effect data
			}
			case MartyrAoeOnReactHitEffect martyrAoeOnReactHitEffect:
			{
				List<ActorData> targets = AreaEffectUtils.GetActorsInRadius(
					target.GetFreePos(),
					martyrAoeOnReactHitEffect.m_aoeRadius,
					martyrAoeOnReactHitEffect.m_penetrateLos,
					caster,
					caster.GetOtherTeams(),
					null);
				float damageScore = 0;
				foreach (ActorData actorData in targets)
				{
					damageScore += ConvertDamageToScore(caster, actorData, martyrAoeOnReactHitEffect.m_damageAmount);
				}

				potentialChoice.score += damageScore;
				potentialChoice.reasoning += $"Added {damageScore} score for projected damage.\n";
				// TODO BOTS martyrAoeOnReactHitEffect.m_enemyHitEffect
				return false; // also process standard effect data
			}
			case ClaymoreDirtyFightingTargetEffect claymoreDirtyFightingTargetEffect:
			{
				float damageScore = ConvertDamageToScore(caster, target, claymoreDirtyFightingTargetEffect.m_damageAmount);
				float dashScore = 0;
				if (IsLikelyToDash(target, true))
				{
					dashScore += 8;
					potentialChoice.reasoning += $"Added {dashScore} score for predicted dash.\n";
				}
				potentialChoice.score += damageScore + dashScore;
				potentialChoice.reasoning += $"Added {damageScore} score for projected damage.\n";
				return false; // also process standard effect data
			}
			case ExoTetherEffect exoTetherEffect:
			{
				float factor = IsLikelyToDash(target, true) ? 0.8f : 0.3f;
				float damageScore = ConvertDamageToScore(caster, target, (int)(exoTetherEffect.m_baseBreakDamage * factor));
				potentialChoice.score += damageScore;
				potentialChoice.reasoning += $"Added {damageScore} score for projected damage.\n";
				
				return false; // also process standard effect data
			}
			default:
				return false;
		}
	}

	private void ScoreDamageAndHealing(
		AbilityResults tempAbilityResults,
		ActorData caster,
		bool ignoreOverhealing,
		Dictionary<ActorData, int> damageResults,
		PotentialChoice potentialChoice)
	{
		foreach (ActorData actorData in damageResults.Keys)
		{
			int healthDelta = damageResults[actorData];
			if (healthDelta > 0)
			{
				PotentialChoice potentialChoice3 = potentialChoice;
				potentialChoice3.reasoning += $"This ability does instant healing (amount: {healthDelta})\n";
				if (tempAbilityResults.Ability is Card_Standard_Ability cardStandardAbility
				    && cardStandardAbility.m_applyEffect
				    && cardStandardAbility.m_effect.m_healingPerTurn > 0
				    && cardStandardAbility.m_effect.m_duration > 1)
				{
					int healOverTimeScore = (int)Mathf.Floor(cardStandardAbility.m_effect.m_healingPerTurn *
					                                         (cardStandardAbility.m_effect.m_duration - 1) * 0.5f);
					potentialChoice.reasoning +=
						$"Adding {healOverTimeScore} for heal over time (second wind) to amount.\n";
					healthDelta += healOverTimeScore;
				}

				if (ignoreOverhealing)
				{
					int maxHPToRestore = actorData.GetMaxHitPoints() - actorData.GetHitPointsToDisplay();
					if (healthDelta > maxHPToRestore)
					{
						potentialChoice.reasoning +=
							$"Reduce amount by {healthDelta - maxHPToRestore} because we're ignoring overhealing.\n";
						healthDelta = maxHPToRestore;
					}
				}

				float healScore = healthDelta * 1f + (1f - actorData.GetHitPointPercent()) * 0.1f;
				potentialChoice.score += healScore;
				potentialChoice.reasoning += $"Score set to: {potentialChoice.score} (added {healScore})\n";
				potentialChoice.healingTotal += healthDelta;
				potentialChoice.numTeamTargetsHit++;
			}
			else if (healthDelta < 0)
			{
				if (actorData.GetTeam() == caster.GetTeam() || GetEnemyPlayerAliveAndVisibleMultiplier(actorData) <= 0f)
				{
					continue;
				}

				float damageScore = ConvertDamageToScore(caster, actorData, healthDelta);
				potentialChoice.score += damageScore;
				potentialChoice.reasoning +=
					$"Added {damageScore} score for damage done.  Score is now: {potentialChoice.score} \n";
				if (actorData.GetHitPointsToDisplay() <= -healthDelta)
				{
					// applied in ConvertDamageToScore
					potentialChoice.reasoning += $"The last add included a fatal damage flat bonus of {2f}\n";
				}

				potentialChoice.damageTotal += -healthDelta;
				potentialChoice.numEnemyTargetsHit++;
				if (tempAbilityResults.Ability is RampartGrab)
				{
					float pullScore = (actorData.GetFreePos() - caster.GetFreePos()).magnitude / 2f;
					potentialChoice.score += pullScore;
					potentialChoice.reasoning += $"Added distance bonus for pull: {pullScore}\n";
				}
			}
			else if (actorData.GetTeam() == caster.GetTeam())
			{
				potentialChoice.numTeamTargetsHit++;
			}
			else
			{
				potentialChoice.numEnemyTargetsHit++;
			}
		}
	}

	// added in rogues
	public static float CalculateMaxAvailableRangeForActor(ActorData actorData)
	{
		AbilityData abilityData = actorData.GetAbilityData();
		float num = 0f;
		for (int i = 0; i <= 4; i++)
		{
			Ability abilityOfActionType = abilityData.GetAbilityOfActionType((AbilityData.ActionType)i);
			if (abilityOfActionType != null && abilityData.GetCooldownRemaining((AbilityData.ActionType)i) == 0)
			{
				float rangeInSquares = abilityOfActionType.GetRangeInSquares(0);
				if (rangeInSquares > num)
				{
					num = rangeInSquares;
				}
			}
		}
		return Mathf.Max(num - 0.25f, 0f);
	}

	// added in rogues
	public override bool ShouldDoAbilityBeforeMovement()
	{
		// custom
		return true;
		// rogues
		// ActorData component = GetComponent<ActorData>();
		// return HasEnemyInRangeWithNoCover(component);
	}

	public override IEnumerator DecideMovement()
	{
		ActorData actorData = GetComponent<ActorData>();
		MovementType movementType = m_movementType;
		if (movementType == MovementType.Auto)
		{
			movementType = ((m_optimalRange >= 4.5f) ? MovementType.Ranged : MovementType.Melee);
		}
		switch (movementType)
		{
		case MovementType.Ranged:
			yield return StartCoroutine(DoRangedMovement(actorData, m_optimalRange));
			break;
		case MovementType.Support:
			yield return StartCoroutine(DoSupportMovement(actorData, m_optimalRange));
			break;
		case MovementType.Melee:
			yield return StartCoroutine(DoMeleeMovement(actorData, m_optimalRange));
			break;
		case MovementType.Stationary:
			yield return StartCoroutine(DoStationaryMovement(actorData));
			break;
		default:
			yield break;
		}
	}

	// added in rogues
	// TODO BOTS unused
	private Dictionary<BoardSquare, PowerUp> GetPowerUpsInSquares(ActorData actorData, HashSet<BoardSquare> squares)
	{
		Dictionary<BoardSquare, PowerUp> dictionary = new Dictionary<BoardSquare, PowerUp>();
		foreach (BoardSquare boardSquare in squares)
		{
			PowerUp powerUpInPos = PowerUpManager.Get().GetPowerUpInPos(boardSquare.GetGridPos());
			if (powerUpInPos != null && powerUpInPos.TeamAllowedForPickUp(actorData.GetTeam()))
			{
				dictionary[boardSquare] = powerUpInPos;
			}
			else
			{
				powerUpInPos = SpoilsManager.Get().GetPowerUpInPos(boardSquare);
				if (powerUpInPos != null && powerUpInPos.TeamAllowedForPickUp(actorData.GetEnemyTeam()))
				{
					dictionary[boardSquare] = powerUpInPos;
				}
			}
		}
		return dictionary;
	}

	// added in rogues
	// was used for deciding to attack before moving
	public bool HasEnemyInRangeWithNoCover(ActorData actorData)
	{
		bool result = false;
		BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
		float num = CalculateMaxAvailableRangeForActor(actorData);
		if (currentBoardSquare != null)
		{
			foreach (ActorData actorData2 in actorData.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList())
			{
				BoardSquare currentBoardSquare2 = actorData2.GetCurrentBoardSquare();
				if (currentBoardSquare2 != null && currentBoardSquare.GetLOS(currentBoardSquare2.x, currentBoardSquare2.y))
				{
					Vector3 dir = new Vector3(currentBoardSquare.x - currentBoardSquare2.x, 0f, currentBoardSquare.y - currentBoardSquare2.y);
					dir.y = 0f;
					if (dir.magnitude <= num)
					{
						if (Board.Get().GetSquaresAreAdjacent(currentBoardSquare, currentBoardSquare2))
						{
							result = true;
							break;
						}
						if (!actorData2.GetComponent<ActorCover>().IsDirInCover(dir))
						{
							result = true;
							break;
						}
					}
				}
			}
		}
		return result;
	}

	// added in rogues
	public static bool HasLOSToEnemiesFromSquare(ActorData actorData, BoardSquare square) // non-static in rogues
	{
		bool result = false;
		if (square != null)
		{
			foreach (ActorData actorData2 in actorData.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList())
			{
				BoardSquare currentBoardSquare = actorData2.GetCurrentBoardSquare();
				if (currentBoardSquare != null && square.GetLOS(currentBoardSquare.x, currentBoardSquare.y))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	// custom
	public static int GetNumLOSToEnemy(ActorData enemy)
	{
		if (enemy == null)
		{
			return 0;
		}
		BoardSquare square = enemy.GetCurrentBoardSquare();
		if (square == null)
		{
			return 0;
		}
		
		List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(enemy.GetEnemyTeam());
		int result = 0;
		foreach (ActorData actorData in allTeamMembers)
		{
			BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
			if (currentBoardSquare != null && square.GetLOS(currentBoardSquare.x, currentBoardSquare.y))
			{
				result++;
			}
		}
		return result;
	}

	// added in rogues
	public IEnumerator DoStationaryMovement(ActorData actorData)
	{
		BoardSquare currentBoardSquare = actorData.CurrentBoardSquare;
		if (currentBoardSquare != null)
		{
			actorData.GetActorTurnSM().SelectMovementSquareForMovement(currentBoardSquare);  // , true in rogues
			BotManager.Get().SelectDestination(actorData, currentBoardSquare);
		}
		yield break;
	}

	// added in rogues
	public IEnumerator DoMeleeMovement(ActorData actorData, float optimalRange)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		ActorMovement actorMovement = actorData.GetActorMovement();
		ActorTurnSM turnSM = actorData.GetActorTurnSM();
		HydrogenConfig hydrogenConfig = HydrogenConfig.Get();
		HashSet<BoardSquare> squaresCanMoveTo = actorMovement.SquaresCanMoveTo;
		HashSet<BoardSquare> squaresCanMoveToWithQueuedAbility = actorMovement.SquaresCanMoveToWithQueuedAbility;
		float num = actorMovement.CalculateMaxHorizontalMovement();
		ActorData actorData2 = null;
		float num2 = 0f;
		Vector3 position = actorData.transform.position;
		position.y = 0f;
		BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
		float num3 = CalculateMaxAvailableRangeForActor(actorData);
		BoardSquare bestDestSquare = null;
		foreach (ActorData actorData3 in actorData.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList())
		{
			if (!actorData3.IsDead() && !(actorData3.GetCurrentBoardSquare() == null) && !actorData3.IgnoreForAbilityHits)
			{
				BoardSquare pendingDestinationOrCurrentSquare = BotManager.Get().GetPendingDestinationOrCurrentSquare(actorData3);
				Vector3 position2 = pendingDestinationOrCurrentSquare.transform.position;
				position2.y = 0f;
				float magnitude = (position2 - position).magnitude;
				float num4 = Mathf.Max(num - magnitude, 0.5f);
				BoardSquare closestMoveableSquareTo = actorMovement.GetClosestMoveableSquareTo(pendingDestinationOrCurrentSquare, true, true, true);
				BoardSquare boardSquare;
				if (closestMoveableSquareTo.HorizontalDistanceOnBoardTo(pendingDestinationOrCurrentSquare) < num3)
				{
					boardSquare = closestMoveableSquareTo;
					num4 *= 10f;
				}
				else
				{
					BoardSquare closestMoveableSquareTo2 = actorMovement.GetClosestMoveableSquareTo(pendingDestinationOrCurrentSquare, true, false, true);
					if (closestMoveableSquareTo2.HorizontalDistanceInSquaresTo(currentBoardSquare) < num3)
					{
						num4 *= 1.5f;
					}
					boardSquare = closestMoveableSquareTo2;
				}
				float hitPointPercent = actorData3.GetHitPointPercent();
				if (hitPointPercent < 0.5f && hitPointPercent > 0.1f)
				{
					num4 *= hitPointPercent + 1f;
				}
				if (actorData2 == null || num4 > num2)
				{
					actorData2 = actorData3;
					num2 = num4;
					bestDestSquare = boardSquare;
				}
			}
		}
		if (realtimeSinceStartup + hydrogenConfig.MaxAIIterationTime < Time.realtimeSinceStartup)
		{
			yield return null;
			realtimeSinceStartup = Time.realtimeSinceStartup;
		}
		if (actorData.RemainingHorizontalMovement != 0f && bestDestSquare != null)
		{
			turnSM.SelectMovementSquareForMovement(bestDestSquare); // , true in rogues
			BotManager.Get().SelectDestination(actorData, bestDestSquare);
		}
	}

	// added in rogues
	private ActorData GetLinkedActorIfAny(ActorData actor, bool friendly)
	{
		List<ActorData> list = null;
		if (actor.m_characterType == CharacterType.Spark)
		{
			list = actor.GetComponent<SparkBeamTrackerComponent>().GetBeamActors();
		}
		if (list == null)
		{
			return null;
		}
		ActorData result = null;
		foreach (ActorData actorData in list)
		{
			if (actorData != null
			    && !actorData.IsDead()
			    && ((friendly && actorData.GetTeam() == actor.GetTeam())
			        || (!friendly && actorData.GetTeam() != actor.GetTeam())))
			{
				result = actorData;
				break;
			}
		}
		return result;
	}

	// added in rogues
	public IEnumerator DoSupportMovement(ActorData actorData, float optimalRange)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		ActorMovement actorMovement = actorData.GetActorMovement();
		ActorTurnSM turnSM = actorData.GetActorTurnSM();
		HydrogenConfig hydrogenConfig = HydrogenConfig.Get();
		HashSet<BoardSquare> squaresCanMoveTo = actorMovement.SquaresCanMoveTo;
		HashSet<BoardSquare> squaresCanMoveToWithQueuedAbility = actorMovement.SquaresCanMoveToWithQueuedAbility;
		float num = actorMovement.CalculateMaxHorizontalMovement();
		ActorData bestFriendlyPlayer = GetLinkedActorIfAny(actorData, true);
		float num2 = 0f;
		Vector3 position = actorData.transform.position;
		position.y = 0f;
		if (bestFriendlyPlayer == null)
		{
			foreach (ActorData actorData2 in GameFlowData.Get().GetAllTeamMembers(actorData.GetTeam()))
			{
				if (!actorData2.IsDead() && !(actorData2.GetCurrentBoardSquare() == null) && !(actorData2 == actorData) && actorData2.GetCharacterResourceLink().m_characterRole != CharacterRole.Support)
				{
					BoardSquare pendingDestinationOrCurrentSquare = BotManager.Get().GetPendingDestinationOrCurrentSquare(actorData2);
					Vector3 position2 = pendingDestinationOrCurrentSquare.transform.position;
					position2.y = 0f;
					float magnitude = (position2 - position).magnitude;
					float num3 = Mathf.Max(num - magnitude, 0.5f);
					if (squaresCanMoveToWithQueuedAbility.Contains(pendingDestinationOrCurrentSquare))
					{
						num3 *= 10f;
					}
					if (squaresCanMoveTo.Contains(pendingDestinationOrCurrentSquare))
					{
						num3 *= 1.5f;
					}
					float hitPointPercent = actorData2.GetHitPointPercent();
					if (hitPointPercent < 0.5f && hitPointPercent > 0.1f)
					{
						num3 *= hitPointPercent + 1f;
					}
					if (actorData2 == null || num3 > num2)
					{
						bestFriendlyPlayer = actorData2;
						num2 = num3;
					}
				}
			}
			if (realtimeSinceStartup + hydrogenConfig.MaxAIIterationTime < Time.realtimeSinceStartup)
			{
				yield return null;
				realtimeSinceStartup = Time.realtimeSinceStartup;
			}
		}
		if (actorData.RemainingHorizontalMovement != 0f && !actorData.HasQueuedChase() && bestFriendlyPlayer != null)
		{
			BoardSquare closestMoveableSquareTo = actorData.GetActorMovement().GetClosestMoveableSquareTo(bestFriendlyPlayer.GetCurrentBoardSquare(), true, false, false);
			turnSM.SelectMovementSquareForMovement(closestMoveableSquareTo); // , true in rogues
			BotManager.Get().SelectDestination(actorData, closestMoveableSquareTo);
		}
	}

	// added in rogues
	public IEnumerator DoRangedMovement(ActorData actorData, float optimalRange)
	{
		ActorMovement actorMovement = actorData.GetActorMovement();
		ActorTurnSM turnSM = actorData.GetActorTurnSM();
		HydrogenConfig config = HydrogenConfig.Get();
		float remainingHorizontalMovement = actorData.RemainingHorizontalMovement;
		HashSet<BoardSquare> squaresCanMoveTo = actorMovement.SquaresCanMoveTo;
		HashSet<BoardSquare> nonSprintSquares = actorMovement.SquaresCanMoveToWithQueuedAbility;
		if (optimalRange < 4.5f)
		{
			optimalRange = 4.5f;
		}
		float bestSquareScore = -99999f;
		BoardSquare startingSquare = actorData.GetCurrentBoardSquare();
		BoardSquare bestSquare = startingSquare;
		List<ActorData> players = GameFlowData.Get().GetActors();
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		foreach (BoardSquare boardSquare in squaresCanMoveTo)
		{
			float num = 0f;
			float num2 = 0f;
			float num3 = 99999f;
			ActorData actorData2 = null;
			int num4 = 0;
			bool flag = false;
			foreach (ActorData actorData3 in players)
			{
				if (!(actorData3 == actorData) && actorData3 && actorData3.GetCurrentBoardSquare() && !actorData3.IsDead())
				{
					num4++;
					float num5 = boardSquare.HorizontalDistanceOnBoardTo(BotManager.Get().GetPendingDestinationOrCurrentSquare(actorData3));
					if (actorData3.GetTeam() == actorData.GetTeam())
					{
						if (num5 < optimalRange - 2f)
						{
							num += 90f * num5 / (1f * optimalRange - 2f);
						}
						else if (num5 > optimalRange - 1f)
						{
							num += 90f - Mathf.Pow(num5 - (optimalRange - 2f), 1.25f);
						}
						else
						{
							num += 100f - (optimalRange - 3f) * Mathf.Abs(num5 - (optimalRange - 3f));
						}
					}
					else
					{
						float num6 = 0f;
						if (num5 < optimalRange - 2.5)
						{
							num6 += 75f * num5 / (optimalRange - 2.5f);
							flag = true;
						}
						else if (num5 > optimalRange)
						{
							num6 += 90f - Mathf.Pow(num5 - (optimalRange - 1f), 1.25f);
						}
						else
						{
							num6 += 100f - 3f * Mathf.Abs(num5 - (optimalRange - 2f));
							flag = true;
						}
						num += num6;
						if (num5 < num3)
						{
							num2 = num6;
							num3 = num5;
							actorData2 = actorData3;
						}
					}
				}
			}
			if (num4 > 0)
			{
				num /= num4;
			}
			ActorCover component = actorData.GetComponent<ActorCover>();
			if (component != null)
			{
				num += component.CoverRating(boardSquare) * 40f;  // CoverRating(boardSquare, 100f) in rogues
			}
			if (boardSquare.OccupantActor != null && boardSquare.OccupantActor != actorData)
			{
				num -= 300f;
			}
			if (!HasLOSToEnemiesFromSquare(actorData, boardSquare))
			{
				num -= 200f;
			}
			if (nonSprintSquares.Contains(boardSquare) && flag)
			{
				num += 70f;
			}
			if (actorData2 != null)
			{
				num += num2;
			}
			if (bestSquareScore < num)
			{
				bestSquareScore = num;
				bestSquare = boardSquare;
			}
			if (realtimeSinceStartup + config.MaxAIIterationTime < Time.realtimeSinceStartup)
			{
				yield return null;
				realtimeSinceStartup = Time.realtimeSinceStartup;
			}
		}
		if (bestSquare != startingSquare)
		{
			turnSM.SelectMovementSquareForMovement(bestSquare); // , true in rogues
			BotManager.Get().SelectDestination(actorData, bestSquare);
		}
	}

	// added in rogues
	private float GetEnemyPlayerAliveAndVisibleMultiplier(ActorData enemyActor)
	{
		BoardSquare currentBoardSquare = enemyActor.GetCurrentBoardSquare();
		if (enemyActor.IsDead() || currentBoardSquare == null || enemyActor.IgnoreForAbilityHits)
		{
			return 0f;
		}
		ActorData actorData = GetComponent<ActorData>();
		ActorStatus enemyActorStatus = enemyActor.GetComponent<ActorStatus>();
		if (!enemyActor.IsInBrush()
		    && actorData.GetFogOfWar().IsVisible(currentBoardSquare)
		    && (!enemyActorStatus.HasStatus(StatusType.InvisibleToEnemies) || actorData.GetActorStatus().HasStatus(StatusType.SeeInvisible)))
		{
			return 1f;
		}

		if (enemyActorStatus.HasStatus(StatusType.Revealed))
		{
			return 1f;
		}

		Vector3 serverLastKnownPosVec = enemyActor.GetServerLastKnownPosVec();
		Vector3 position = currentBoardSquare.transform.position;
		serverLastKnownPosVec.y = 0f;
		position.y = 0f;
		if ((serverLastKnownPosVec - position).magnitude < Board.Get().m_squareSize)
		{
			return 1f;
		}
		return 0f;
	}

	// added in rogues
	private static bool HasNoDashOffCooldown(ActorData actor, bool includeCards) // non-static in rogues
	{
		AbilityData abilityData = actor.GetAbilityData();
		foreach (AbilityData.AbilityEntry abilityEntry in abilityData.abilityEntries)
		{
			if (abilityEntry != null
			    && abilityEntry.ability != null
			    && abilityEntry.ability.CanRunInPhase(AbilityPriority.Evasion)
			    && abilityEntry.GetCooldownRemaining() == 0)
			{
				return false;
			}
		}
		Ability abilityOfActionType = abilityData.GetAbilityOfActionType(AbilityData.ActionType.CARD_1);
		return !includeCards || abilityOfActionType == null;
	}

	// added in rogues
	protected static float ConvertDamageToScore(ActorData caster, ActorData target, int amount) // non-static in rogues
	{
		float num = Mathf.Abs(amount);
		float result = num + (1f - target.GetHitPointPercent()) * 0.1f;
		if (target.GetHitPointsToDisplay() <= num) // is fatal
		{
			result += 2f;
		}
		return result;
	}

	// added in rogues
	public void PickRespawnSquare()
	{
		ActorData component = GetComponent<ActorData>();
		List<BoardSquare> list = new List<BoardSquare>(component.respawnSquares);
		foreach (GameObject gameObject in GameFlowData.Get().GetPlayers())
		{
			ActorData component2 = gameObject.GetComponent<ActorData>();
			if (component2 != null && component2.IsDead() && component2 != component)
			{
				list.Remove(component2.RespawnPickedPositionSquare);
			}
		}
		if (m_optimalRange < 4.5f)
		{
			List<ActorData> list2 = component.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList();
			float num = 2f;
			ActorData actorData = null;
			foreach (ActorData actorData2 in list2)
			{
				if (!actorData2.IsDead() && !actorData2.IgnoreForAbilityHits && !(actorData2.GetCurrentBoardSquare() == null) && actorData2.GetHitPointPercent() < num)
				{
					num = actorData2.GetHitPointPercent();
					actorData = actorData2;
				}
			}
			if (actorData != null)
			{
				BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
				float num2 = 999999f;
				BoardSquare boardSquare = null;
				foreach (BoardSquare boardSquare2 in list)
				{
					float num3 = boardSquare2.HorizontalDistanceOnBoardTo(currentBoardSquare);
					if (num3 < num2)
					{
						num2 = num3;
						boardSquare = boardSquare2;
					}
				}
				if (boardSquare != null)
				{
					GetComponent<ServerActorController>().ProcessPickedRespawnRequest(boardSquare.x, boardSquare.y);
					return;
				}
			}
		}
		else
		{
			List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(component.GetTeam());
			float num4 = 999999f;
			BoardSquare boardSquare3 = null;
			foreach (BoardSquare boardSquare4 in list)
			{
				foreach (ActorData actorData3 in allTeamMembers)
				{
					if (!actorData3.IsDead() && !(actorData3 == component) && !actorData3.IgnoreForAbilityHits)
					{
						BoardSquare currentBoardSquare2 = actorData3.GetCurrentBoardSquare();
						if (!(currentBoardSquare2 == null))
						{
							float num5 = boardSquare4.HorizontalDistanceOnBoardTo(currentBoardSquare2);
							if (num5 <= num4)
							{
								num4 = num5;
								boardSquare3 = boardSquare4;
							}
						}
					}
				}
			}
			if (boardSquare3 != null)
			{
				GetComponent<ServerActorController>().ProcessPickedRespawnRequest(boardSquare3.x, boardSquare3.y);
				return;
			}
		}
		int index = (100 + GameFlowData.Get().CurrentTurn + component.respawnSquares[0].x + component.respawnSquares[0].y) % component.respawnSquares.Count;
		GetComponent<ServerActorController>().ProcessPickedRespawnRequest(component.respawnSquares[index].x, component.respawnSquares[index].y);
	}

	// added in rogues
	public enum MovementType
	{
		Auto,
		Ranged,
		Support,
		Melee,
		Stationary
	}

	// added in rogues
	public class PotentialChoice
	{
		public List<AbilityTarget> targetList;

		public float score;

		public int numEnemyTargetsHit;

		public int damageTotal;

		public int numTeamTargetsHit;

		public int healingTotal;

		public bool freeAction;

		public float coolness = 0.05f;

		public string reasoning;

		public BoardSquare destinationSquare;

		public int GetTargetListCount()
		{
			if (targetList != null)
			{
				return targetList.Count;
			}
			return 0;
		}
	}
#endif
}
