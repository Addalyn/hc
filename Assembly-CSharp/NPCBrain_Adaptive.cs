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
		int i;
		for (int actionType = 6; actionType > -1; actionType = i)
		{
			AbilityData.ActionType actionType2 = (AbilityData.ActionType)actionType;
			Ability abilityOfActionType = abilityData.GetAbilityOfActionType(actionType2);
			if (!(abilityOfActionType == null))
			{
				if (m_allowedAbilities != null && m_allowedAbilities.Length != 0)
				{
					bool flag = false;
					int[] allowedAbilities = m_allowedAbilities;
					for (i = 0; i < allowedAbilities.Length; i++)
					{
						if (allowedAbilities[i] == actionType)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						goto IL_29C;
					}
				}
				if (abilityData.ValidateActionIsRequestable(actionType2))
				{
					if (abilityOfActionType.GetNumTargets() == 0)
					{
						yield return StartCoroutine(ScoreZeroTargetAbility(actionType2));
					}
					else if (abilityOfActionType.GetNumTargets() == 1
					         || abilityOfActionType is RampartGrab
					         || abilityOfActionType is ThiefBasicAttack  // custom
					         || abilityOfActionType is FishManCone)  // custom
					{
						Ability.TargetingParadigm targetingParadigm = abilityOfActionType.GetTargetingParadigm(0);
						if (targetingParadigm == Ability.TargetingParadigm.Position)
						{
							yield return StartCoroutine(ScoreSinglePositionTargetAbility(actionType2));
						}
						else if (targetingParadigm == Ability.TargetingParadigm.BoardSquare)
						{
							yield return StartCoroutine(ScoreSingleBoardSquareTargetAbility(actionType2));
						}
						else if (targetingParadigm == Ability.TargetingParadigm.Direction)
						{
							yield return StartCoroutine(ScoreSingleDirectionTargetAbility(actionType2));
						}
					}
					else
					{
						yield return StartCoroutine(ScoreMultiTargetAbility(actionType2));
					}
					if (iterationStartTime + config.MaxAIIterationTime < Time.realtimeSinceStartup)
					{
						yield return null;
						iterationStartTime = Time.realtimeSinceStartup;
					}
				}
			}
			IL_29C:
			i = actionType - 1;
		}
		AbilityData.ActionType actionType3 = AbilityData.ActionType.INVALID_ACTION;
		PotentialChoice potentialChoice = null;
		foreach (KeyValuePair<AbilityData.ActionType, PotentialChoice> keyValuePair in m_potentialChoices)
		{
			if (keyValuePair.Value != null && !keyValuePair.Value.freeAction && (potentialChoice == null || keyValuePair.Value.score > potentialChoice.score))
			{
				actionType3 = keyValuePair.Key;
				potentialChoice = keyValuePair.Value;
			}
		}
		BoardSquare dashTarget = null;
		if (actionType3 != AbilityData.ActionType.INVALID_ACTION && potentialChoice != null && potentialChoice.score > 0f)
		{
			Ability abilityOfActionType2 = abilityData.GetAbilityOfActionType(actionType3);
			dashTarget = potentialChoice.destinationSquare;
			int num = (int)(1 + actionType3);
			if (num > 5)
			{
				num -= 2;
			}
			if (m_logReasoning)
			{
				Log.Info("{0} choosing ability {1} - {2} - Reasoning:\n{3}Final score: {4}", actorData.DisplayName, num, abilityOfActionType2.m_abilityName, potentialChoice.reasoning, potentialChoice.score);
			}
			if (m_sendReasoningToTeamChat)
			{
				UIQueueListPanel.UIPhase uiphaseFromAbilityPriority = UIQueueListPanel.GetUIPhaseFromAbilityPriority(abilityOfActionType2.GetRunPriority());
				string text = "<color=red>";
				if (uiphaseFromAbilityPriority == UIQueueListPanel.UIPhase.Prep)
				{
					text = "<color=green>";
				}
				else if (uiphaseFromAbilityPriority == UIQueueListPanel.UIPhase.Evasion)
				{
					text = "<color=yellow>";
				}
				ServerGameManager.Get().SendUnlocalizedConsoleMessage(string.Format("<color=white>{0}</color> choosing ability {5}{1} - {2}</color> - Reasoning:\n{3}Final Score: <color=yellow>{4}</color>", actorData.DisplayName, num, abilityOfActionType2.m_abilityName, potentialChoice.reasoning, potentialChoice.score, text), Team.Invalid, ConsoleMessageType.TeamChat, actorData.DisplayName);
			}
			if (abilityOfActionType2.IsSimpleAction())
			{
				GetComponent<ServerActorController>().ProcessCastSimpleActionRequest(actionType3, true);
			}
			else
			{
				botController.RequestAbility(potentialChoice.targetList, actionType3);
			}
		}
		BotManager.Get().BotAIAbilitySelected(actorData, dashTarget, m_optimalRange);
		foreach (KeyValuePair<AbilityData.ActionType, PotentialChoice> keyValuePair2 in m_potentialChoices)
		{
			if (keyValuePair2.Value != null && keyValuePair2.Value.freeAction)
			{
				if (m_allowedAbilities != null && m_allowedAbilities.Length != 0)
				{
					bool flag2 = false;
					int[] allowedAbilities = m_allowedAbilities;
					for (i = 0; i < allowedAbilities.Length; i++)
					{
						if (allowedAbilities[i] == (int)keyValuePair2.Key)
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						continue;
					}
				}
				bool flag3 = false;
				Ability abilityOfActionType3 = abilityData.GetAbilityOfActionType(keyValuePair2.Key);
				if (!(abilityOfActionType3 == null))
				{
					if (abilityOfActionType3 is RageBeastSelfHeal)
					{
						if (actorData.HitPoints < 65)
						{
							flag3 = true;
						}
					}
					else
					{
						flag3 = true;
					}
					if (flag3)
					{
						if (abilityOfActionType3.IsSimpleAction())
						{
							GetComponent<ServerActorController>().ProcessCastSimpleActionRequest(keyValuePair2.Key, true);
						}
						else
						{
							botController.RequestAbility(keyValuePair2.Value.targetList, keyValuePair2.Key);
						}
					}
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
		    || ability.Targeter is AbilityUtil_Targeter_Grid) // custom TODO BOTS Scoundrel trapwire
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
		if (thisAbility.Targeter is AbilityUtil_Targeter_ChargeAoE
		    || thisAbility.Targeter is AbilityUtil_Targeter_Charge
		    || thisAbility.Targeter is AbilityUtil_Targeter_Shape
		    || thisAbility.Targeter is AbilityUtil_Targeter_RocketJump
		    || thisAbility.Targeter is AbilityUtil_Targeter_ScoundrelEvasionRoll)
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
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_TeslaPrison targeter)
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
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_TrackerDrone)
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
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_AoE_AroundActor)
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
			case AbilityUtil_Targeter_ThiefFanLaser targeterThiefFanLaser: // TODO BOTS Thief's primary cannot hit two targets
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
			{
				if (ability.Targeters.Count != 2)
				{
					goto default;
				}

				if (!(ability.Targeters[1] is AbilityUtil_Targeter_ConeOrLaser)
				    && !(ability.Targeters[1] is AbilityUtil_Targeter_ChargeAoE))
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
						else if (ability is ThiefOnTheRun)
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
								if (boardSquare2 == actorData.GetCurrentBoardSquare())
								{
									continue;
								}

								if (!abilityData.IsTargetSquareInRangeOfAbilityFromSquare(
									    boardSquare2, actorData.GetCurrentBoardSquare(), range, minRange2))
								{
									continue;
								}

								AbilityTarget secondTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
								if (ability.CustomTargetValidation(actorData, secondTarget, 1, currentTargets))
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
		
		// custom
		if (tempAbilityResults.Ability.CanRunInPhase(AbilityPriority.Evasion))
		{
			AdjustScoreForEvasion(caster, potentialChoice, tempAbilityResults.Ability);
		}
		// end custom
		
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

				if (effect is SorceressDamageFieldEffect sorceressDamageFieldEffect)
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
				}
				else
				{
					Log.Warning($"World effect is not supported by bots: {effect.GetType()} ({effect.Parent.Ability?.GetType()}{effect.Parent.Passive?.GetType()})"); // custom
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
				if (!(effect is StandardActorEffect actorEffect))
				{
					Log.Warning($"Actor effect is not supported by bots: {effect.GetType()} ({effect.Parent.Ability?.GetType()}{effect.Parent.Passive?.GetType()})"); // custom
					continue;
				}

				float absorb = actorEffect.m_data.m_absorbAmount;
				if (absorb != 0f)
				{
					float score = potentialChoice.score;
					potentialChoice.numTeamTargetsHit++;
					for (int i = 0; i < actorEffect.m_data.m_duration; i++)
					{
						absorb /= 2f;
						potentialChoice.score += absorb;
					}

					potentialChoice.score += (1f - actorToHitResult.Key.GetHitPointPercent()) * 0.1f;
					float shieldScore = potentialChoice.score - score;
					potentialChoice.reasoning += $"Adding {shieldScore} for generic shielding.\n";
				}

				if (actorEffect.m_data != null && actorEffect.m_data.m_statusChanges != null &&
				    actorEffect.m_data.m_statusChanges.Length != 0)
				{
					foreach (StatusType status in actorEffect.m_data.m_statusChanges)
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
								potentialChoice.score += 2f * actorEffect.m_data.m_duration;
								potentialChoice.reasoning += "Adding 2 score for a slow effect.\n";
								break;
							}
							case StatusType.Weakened:
							{
								potentialChoice.score += 13f * actorEffect.m_data.m_duration;
								potentialChoice.reasoning += "Adding 13 score for a weakened effect.\n";
								break;
							}
							case StatusType.Empowered:
							{
								potentialChoice.score += 13f * actorEffect.m_data.m_duration;
								potentialChoice.reasoning += "Adding 13 score for a might effect.\n";
								break;
							}
							case StatusType.Unstoppable:
							{
								potentialChoice.score += 7f * actorEffect.m_data.m_duration;
								potentialChoice.reasoning += "Adding 7 score for an Unstoppable effect.\n";
								break;
							}
							case StatusType.Hasted:
							{
								potentialChoice.score += 8f * actorEffect.m_data.m_duration;
								potentialChoice.reasoning += "Adding 8 score for a haste effect.\n";
								break;
							}
						}
					}
				}
			}
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
	public bool HasLOSToEnemiesFromSquare(ActorData actorData, BoardSquare square)
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
		ActorData result = null;
		if (actor.m_characterType == CharacterType.Spark)
		{
			list = actor.GetComponent<SparkBeamTrackerComponent>().GetBeamActors();
		}
		if (list != null)
		{
			foreach (ActorData actorData in list)
			{
				if (actorData != null && !actorData.IsDead() && ((friendly && actorData.GetTeam() == actor.GetTeam()) || (!friendly && actorData.GetTeam() != actor.GetTeam())))
				{
					result = actorData;
					break;
				}
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
	private bool HasNoDashOffCooldown(ActorData actor, bool includeCards)
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
	protected float ConvertDamageToScore(ActorData caster, ActorData target, int amount)
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
