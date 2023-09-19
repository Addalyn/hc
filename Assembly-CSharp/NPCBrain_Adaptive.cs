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
		CharacterType? characterType = bot.GetComponent<ActorData>()?.m_characterType;
		if (primaryAbility != null && Board.Get() != null)
		{
			nPCBrain_Adaptive.m_optimalRange = primaryAbility.GetRangeInSquares(0) * Board.Get().squareSize * 0.8f;
			Log.Info($"Setting optimal range for {characterType} to {nPCBrain_Adaptive.m_optimalRange} " +
			         $"({(nPCBrain_Adaptive.m_optimalRange >= 4.5f ? MovementType.Ranged : MovementType.Melee)})");
		}
		
		if (characterType.HasValue)
		{
			// TODO BOTS get character config
			CharacterConfig characterConfig = GameManager.Get().GameplayOverrides.GetCharacterConfig(characterType.Value);
			if (characterConfig == null)
			{
				Log.Warning($"Failed to check {characterType}'s role for bot configuration");
			}
			else if (characterConfig.CharacterRole == CharacterRole.Support)
			{
				nPCBrain_Adaptive.m_movementType = MovementType.Support;
				Log.Info($"Setting movement type for {characterType} to {nPCBrain_Adaptive.m_movementType}");
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
			float num5 = 0.0174532924f * (num2 + num4);
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
		List<AbilityTarget> list = new List<AbilityTarget>();
		Vector3 vector = GetComponent<ActorData>().GetFreePos();
		if (startingPos != Vector3.zero)
		{
			vector = startingPos;
		}
		if (numDegrees <= 0)
		{
			numDegrees = 72;
		}
		float num = 360f / numDegrees;
		for (int i = 0; i < numDegrees; i++)
		{
			float num2 = 0.0174532924f * i * num;
			float num3 = Mathf.Sin(num2);
			float num4 = Mathf.Cos(num2);
			Vector3 targetWorldPos = vector;
			targetWorldPos.x += num3;
			targetWorldPos.z += num4;
			list.Add(AbilityTarget.CreateAbilityTargetFromWorldPos(targetWorldPos, vector));
		}
		return list;
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
			float num4 = 0.0174532924f * i * num3;
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
			float num4 = 0.0174532924f * i * num3;
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
			float num8 = 0.0174532924f * j * num7;
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
	public List<AbilityTarget> GeneratePotentialAbilityTargetLocations(float range, bool includeEnemies, bool includeFriendlies, bool includeSelf)
	{
		List<AbilityTarget> list = new List<AbilityTarget>();
		ActorData component = GetComponent<ActorData>();
		BoardSquare currentBoardSquare = component.GetCurrentBoardSquare();
		List<ActorData> potentialTargets = new List<ActorData>();
		if (includeEnemies)
		{
			foreach (ActorData enemyActor in component.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList())
			{
				if (GetEnemyPlayerAliveAndVisibleMultiplier(enemyActor) != 0f
				    && currentBoardSquare.HorizontalDistanceOnBoardTo(enemyActor.GetCurrentBoardSquare()) <= range)
				{
					potentialTargets.Add(enemyActor);
				}
			}
		}
		if (includeFriendlies)
		{
			foreach (ActorData allyActor in GameFlowData.Get().GetAllTeamMembers(component.GetTeam()))
			{
				if (!allyActor.IsDead() && allyActor != component && !allyActor.IgnoreForAbilityHits)
				{
					BoardSquare allySquare = allyActor.GetCurrentBoardSquare();
					if (allySquare != null && currentBoardSquare.HorizontalDistanceOnBoardTo(allySquare) <= range)
					{
						potentialTargets.Add(allyActor);
					}
				}
			}
		}
		if (includeSelf)
		{
			potentialTargets.Add(component);
		}
		foreach (ActorData targetA in potentialTargets)
		{
			list.Add(AbilityTarget.CreateAbilityTargetFromBoardSquare(targetA.GetCurrentBoardSquare(), component.GetFreePos()));
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
					foreach (AbilityTarget item in AbilityTarget.CreateAbilityTargetsFromActorDataList(new List<ActorData>
					{
						targetA,
						targetB
					}, component))
					{
						list.Add(item);
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
							foreach (AbilityTarget item2 in AbilityTarget.CreateAbilityTargetsFromActorDataList(new List<ActorData>
							{
								targetA,
								targetB,
								targetC
							}, component))
							{
								list.Add(item2);
							}
						}
					}
				}
			}
		}
		return list;
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
					else if (abilityOfActionType.GetNumTargets() == 1 || abilityOfActionType is RampartGrab)
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
		Ability abilityOfActionType = GetComponent<AbilityData>().GetAbilityOfActionType(thisAction);
		ActorData component = GetComponent<ActorData>();
		List<AbilityTarget> list = AbilityTarget.AbilityTargetList(abilityOfActionType.CreateAbilityTargetForSimpleAction(component));
		AbilityResults tempAbilityResults = new AbilityResults(component, abilityOfActionType, null, s_gatherRealResults, true);
		abilityOfActionType.GatherAbilityResults(list, component, ref tempAbilityResults);
		PotentialChoice potentialChoice = ScoreResults(tempAbilityResults, component, true);
		potentialChoice.freeAction = abilityOfActionType.IsFreeAction();
		potentialChoice.targetList = list;
		if (potentialChoice.score != 0f || potentialChoice.freeAction)
		{
			m_potentialChoices[thisAction] = potentialChoice;
		}
		yield break;
	}

	// added in rogues
	protected virtual IEnumerator ScoreSinglePositionTargetAbility(AbilityData.ActionType thisAction)
	{
		AbilityData component = GetComponent<AbilityData>();
		Ability thisAbility = component.GetAbilityOfActionType(thisAction);
		ActorData actorData = GetComponent<ActorData>();
		PotentialChoice retVal = null;
		HydrogenConfig config = HydrogenConfig.Get();
		List<AbilityTarget> list = null;
		if (thisAbility.Targeter is AbilityUtil_Targeter_ChargeAoE || thisAbility.Targeter is AbilityUtil_Targeter_Charge || thisAbility.Targeter is AbilityUtil_Targeter_Shape || thisAbility.Targeter is AbilityUtil_Targeter_BazookaGirlDelayedMissile || thisAbility.Targeter is AbilityUtil_Targeter_MultipleShapes)
		{
			float range = thisAbility.m_targetData[0].m_range;
			float minRange = thisAbility.m_targetData[0].m_minRange;
			Vector3 vector = new Vector3(range * Board.Get().squareSize * 2f, 2f, range * Board.Get().squareSize * 2f);
			Vector3 position = actorData.transform.position;
			position.y = 0f;
			Bounds bounds = new Bounds(position, vector);
			List<BoardSquare> squaresInBox = Board.Get().GetSquaresInBox(bounds);
			List<AbilityTarget> list2 = new List<AbilityTarget>();
			using (List<BoardSquare>.Enumerator enumerator = squaresInBox.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BoardSquare boardSquare = enumerator.Current;
					if ((!(boardSquare == actorData.GetCurrentBoardSquare()) || (!(thisAbility.Targeter is AbilityUtil_Targeter_ChargeAoE) && !(thisAbility.Targeter is AbilityUtil_Targeter_Charge))) && (boardSquare.HorizontalDistanceInSquaresTo_Squared(actorData.GetCurrentBoardSquare()) > 2f || !(thisAbility.Targeter is AbilityUtil_Targeter_Charge) || !(boardSquare.OccupantActor != null) || boardSquare.OccupantActor.GetTeam() != actorData.GetTeam()) && component.IsTargetSquareInRangeOfAbilityFromSquare(boardSquare, actorData.GetCurrentBoardSquare(), range, minRange))
					{
						AbilityTarget abilityTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
						if (thisAbility.CustomTargetValidation(actorData, abilityTarget, 0, null))
						{
							if (list == null)
							{
								list = new List<AbilityTarget>();
							}
							list2.Clear();
							list2.Add(abilityTarget);
							if (component.ValidateActionRequest(thisAction, list2, false))
							{
								list.Add(abilityTarget);
							}
						}
					}
				}
				goto IL_374;
			}
		}
		if (thisAbility.Targeter is AbilityUtil_Targeter_AoE_AroundActor)
		{
			List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(actorData.GetTeam());
			List<AbilityTarget> list3 = new List<AbilityTarget>();
			foreach (ActorData actorData2 in allTeamMembers)
			{
				BoardSquare currentBoardSquare = actorData2.GetCurrentBoardSquare();
				if (!actorData2.IsDead() && !(currentBoardSquare == null) && !actorData2.IgnoreForAbilityHits)
				{
					AbilityTarget abilityTarget2 = AbilityTarget.CreateAbilityTargetFromActor(actorData2, actorData);
					if (thisAbility.CustomTargetValidation(actorData, abilityTarget2, 0, null))
					{
						if (list == null)
						{
							list = new List<AbilityTarget>();
						}
						list3.Clear();
						list3.Add(abilityTarget2);
						if (component.ValidateActionRequest(thisAction, list3, false))
						{
							list.Add(abilityTarget2);
						}
					}
				}
			}
		}
		IL_374:
		if (list != null)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (AbilityTarget item in list)
			{
				List<AbilityTarget> list4 = new List<AbilityTarget>();
				list4.Add(item);
				AbilityResults abilityResults = new AbilityResults(actorData, thisAbility, null, s_gatherRealResults, true);
				thisAbility.GatherAbilityResults(list4, actorData, ref abilityResults);
				if (thisAbility.m_chainAbilities != null && thisAbility.m_chainAbilities.Length != 0)
				{
					for (int i = 0; i < thisAbility.m_chainAbilities.Length; i++)
					{
						AbilityResults abilityResults2 = new AbilityResults(actorData, thisAbility.m_chainAbilities[i], null, s_gatherRealResults, true);
						thisAbility.m_chainAbilities[i].GatherAbilityResults(list4, actorData, ref abilityResults2);
						foreach (KeyValuePair<ActorData, ActorHitResults> keyValuePair in abilityResults2.m_actorToHitResults)
						{
							abilityResults.m_actorToHitResults.Add(keyValuePair.Key, keyValuePair.Value);
						}
						foreach (KeyValuePair<ActorData, int> keyValuePair2 in abilityResults2.DamageResults)
						{
							if (abilityResults.DamageResults.ContainsKey(keyValuePair2.Key))
							{
								Dictionary<ActorData, int> damageResults = abilityResults.DamageResults;
								ActorData key = keyValuePair2.Key;
								damageResults[key] += keyValuePair2.Value;
							}
							else
							{
								abilityResults.DamageResults[keyValuePair2.Key] = keyValuePair2.Value;
							}
						}
					}
				}
				PotentialChoice potentialChoice = ScoreResults(abilityResults, actorData, false);
				potentialChoice.freeAction = thisAbility.IsFreeAction();
				potentialChoice.targetList = list4;
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
			List<AbilityTarget>.Enumerator enumerator3 = default(List<AbilityTarget>.Enumerator);
		}
		if (retVal != null && retVal.score > 0f)
		{
			m_potentialChoices[thisAction] = retVal;
		}
		yield break;
		yield break;
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
		List<AbilityTarget> list = null;
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
			using (List<BoardSquare>.Enumerator enumerator = Board.Get().GetSquaresInBox(bounds).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BoardSquare boardSquare = enumerator.Current;
					if (!(boardSquare == actorData.GetCurrentBoardSquare()) && component.IsTargetSquareInRangeOfAbilityFromSquare(boardSquare, actorData.GetCurrentBoardSquare(), range, minRange) && boardSquare.IsValidForGameplay() && (!(thisAbility is SparkEnergized) || actorData.GetComponent<SparkBeamTrackerComponent>().GetBeamActors().Contains(boardSquare.OccupantActor)))
					{
						AbilityTarget abilityTarget = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare, actorData.GetFreePos());
						if (thisAbility.CustomTargetValidation(actorData, abilityTarget, 0, null))
						{
							if (list == null)
							{
								list = new List<AbilityTarget>();
							}
							list.Add(abilityTarget);
						}
					}
				}
				goto IL_57B;
			}
		}
		if (thisAbility.Targeter is AbilityUtil_Targeter_TeslaPrison)
		{
			using (List<ActorData>.Enumerator enumerator2 = actorData.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList().GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					ActorData actorData2 = enumerator2.Current;
					if (GetEnemyPlayerAliveAndVisibleMultiplier(actorData2) != 0f)
					{
						AbilityTarget target = AbilityTarget.CreateAbilityTargetFromBoardSquare(actorData2.GetCurrentBoardSquare(), actorData.GetFreePos());
						foreach (BoardSquare boardSquare2 in AreaEffectUtils.GetSquaresInShape(((AbilityUtil_Targeter_TeslaPrison)thisAbility.Targeter).m_shapeForActorHits, target, true, actorData))
						{
							if (boardSquare2 != null && boardSquare2.IsValidForGameplay())
							{
								AbilityTarget abilityTarget2 = AbilityTarget.CreateAbilityTargetFromBoardSquare(boardSquare2, actorData.GetFreePos());
								List<AbilityTarget> currentTargets = AbilityTarget.AbilityTargetList(abilityTarget2);
								if (thisAbility.CustomTargetValidation(actorData, abilityTarget2, 0, currentTargets))
								{
									if (list == null)
									{
										list = new List<AbilityTarget>();
									}
									list.Add(abilityTarget2);
								}
							}
						}
					}
				}
				goto IL_57B;
			}
		}
		if (thisAbility.Targeter is AbilityUtil_Targeter_TrackerDrone)
		{
			foreach (AbilityTarget abilityTarget3 in GeneratePotentialAbilityTargetLocations(thisAbility.m_targetData[0].m_range, true, false, false))
			{
				List<AbilityTarget> currentTargets2 = AbilityTarget.AbilityTargetList(abilityTarget3);
				if (thisAbility.CustomTargetValidation(actorData, abilityTarget3, 0, currentTargets2))
				{
					if (list == null)
					{
						list = new List<AbilityTarget>();
					}
					list.Add(abilityTarget3);
				}
			}
			if (GameFlowData.Get().CurrentTurn == 1)
			{
				int x = Board.Get().GetMaxX() / 2;
				int y = Board.Get().GetMaxY() / 2;
				BoardSquare squareFromIndex = Board.Get().GetSquareFromIndex(x, y);
				if (squareFromIndex != null && squareFromIndex.IsValidForGameplay())
				{
					AbilityTarget item = AbilityTarget.CreateAbilityTargetFromBoardSquare(squareFromIndex, actorData.GetFreePos());
					if (list == null)
					{
						list = new List<AbilityTarget>();
					}
					list.Add(item);
				}
			}
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_AoE_AroundActor)
		{
			List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(actorData.GetTeam());
			List<AbilityTarget> list2 = new List<AbilityTarget>();
			foreach (ActorData actorData3 in allTeamMembers)
			{
				BoardSquare currentBoardSquare = actorData3.GetCurrentBoardSquare();
				if (!actorData3.IsDead() && !(currentBoardSquare == null) && !actorData3.IgnoreForAbilityHits)
				{
					AbilityTarget abilityTarget4 = AbilityTarget.CreateAbilityTargetFromActor(actorData3, actorData);
					if (thisAbility.CustomTargetValidation(actorData, abilityTarget4, 0, null))
					{
						if (list == null)
						{
							list = new List<AbilityTarget>();
						}
						list2.Clear();
						list2.Add(abilityTarget4);
						if (component.ValidateActionRequest(thisAction, list2, false))
						{
							list.Add(abilityTarget4);
						}
					}
				}
			}
		}
		IL_57B:
		if (list != null)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (AbilityTarget abilityTarget5 in list)
			{
				List<AbilityTarget> list3 = new List<AbilityTarget>();
				list3.Add(abilityTarget5);
				AbilityResults abilityResults = new AbilityResults(actorData, thisAbility, null, s_gatherRealResults, true);
				thisAbility.GatherAbilityResults(list3, actorData, ref abilityResults);
				int count = abilityResults.DamageResults.Count;
				PotentialChoice potentialChoice = ScoreResults(abilityResults, actorData, false);
				potentialChoice.freeAction = thisAbility.IsFreeAction();
				potentialChoice.targetList = list3;
				float num = 1f;
				if (potentialChoice != null && thisAbility.Targeter is AbilityUtil_Targeter_TrackerDrone)
				{
					num = 0.375f;
					BoardSquare square = Board.Get().GetSquare(abilityTarget5.GridPos);
					potentialChoice.score += square.HorizontalDistanceInSquaresTo(currentSquare) * 0.05f;
				}
				if (potentialChoice != null && (thisAbility.Targeter is AbilityUtil_Targeter_TeslaPrison || thisAbility.Targeter is AbilityUtil_Targeter_TrackerDrone))
				{
					BoardSquare square2 = Board.Get().GetSquare(abilityTarget5.GridPos);
					foreach (ActorData actorData4 in actorData.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList())
					{
						if (GetEnemyPlayerAliveAndVisibleMultiplier(actorData4) != 0f)
						{
							BoardSquare currentBoardSquare2 = actorData4.GetCurrentBoardSquare();
							float num2 = square2.HorizontalDistanceInSquaresTo(currentBoardSquare2);
							if (num2 == 0f)
							{
								potentialChoice.score += 30f * num;
							}
							if (num2 <= 1f)
							{
								potentialChoice.score += 35f * num;
							}
							else if (num2 < 2f)
							{
								potentialChoice.score += 40f * num;
							}
							else if (num2 < 3f)
							{
								potentialChoice.score += 20f * num;
							}
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
			List<AbilityTarget>.Enumerator enumerator4 = default(List<AbilityTarget>.Enumerator);
		}
		if (retVal != null && retVal.score > 0f)
		{
			m_potentialChoices[thisAction] = retVal;
		}
		yield break;
		yield break;
	}

	// added in rogues
	private bool IsSquareOccupiedByAliveActor(BoardSquare square)
	{
		bool result = false;
		foreach (ActorData actorData in GameFlowData.Get().GetActors())
		{
			if (!actorData.IsDead() && square == actorData.CurrentBoardSquare)
			{
				result = true;
				break;
			}
		}
		return result;
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
				if (thisAbility.GetEvasionTeleportType() != ActorData.TeleportType.NotATeleport && square.IsInBrush() && BrushCoordinator.Get().IsRegionFunctioning(square.BrushRegion))
				{
					num += 20f;
				}
				List<ActorData> list = actorData.GetOtherTeams().SelectMany(otherTeam => GameFlowData.Get().GetAllTeamMembers(otherTeam)).ToList();
				foreach (ActorData actorData2 in list)
				{
					if (GetEnemyPlayerAliveAndVisibleMultiplier(actorData2) != 0f)
					{
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
				choice.reasoning += string.Format("Adding {0} based on the quality of the evade (destination position & world effects at start and destination)\n", num);
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
		AbilityData component = GetComponent<AbilityData>();
		Ability thisAbility = component.GetAbilityOfActionType(thisAction);
		ActorData actorData = GetComponent<ActorData>();
		PotentialChoice retVal = null;
		HydrogenConfig config = HydrogenConfig.Get();
		bool includeFriendlies = true;
		bool includeEnemies = true;
		bool includeSelf = false;
		if (thisAbility is TutorialAttack)
		{
			includeFriendlies = false;
		}
		List<AbilityTarget> list = null;
		if (thisAbility.Targeter is AbilityUtil_Targeter_Laser)
		{
			if (thisAbility is BlasterDelayedLaser)
			{
				list = GeneratePotentialAbilityTargetLocationsCircle(360);
			}
			else
			{
				AbilityUtil_Targeter_Laser abilityUtil_Targeter_Laser = (AbilityUtil_Targeter_Laser)thisAbility.Targeter;
				list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_Laser.m_distance, includeEnemies, includeFriendlies, includeSelf);
			}
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_Blindfire)
		{
			AbilityUtil_Targeter_Blindfire abilityUtil_Targeter_Blindfire = (AbilityUtil_Targeter_Blindfire)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_Blindfire.m_coneLengthRadiusInSquares, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_ChainLightningLaser)
		{
			AbilityUtil_Targeter_ChainLightningLaser abilityUtil_Targeter_ChainLightningLaser = (AbilityUtil_Targeter_ChainLightningLaser)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_ChainLightningLaser.m_distance, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_MultipleCones)
		{
			AbilityUtil_Targeter_MultipleCones abilityUtil_Targeter_MultipleCones = (AbilityUtil_Targeter_MultipleCones)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocationsCircleVolume(abilityUtil_Targeter_MultipleCones.m_maxConeLengthRadius * Board.Get().squareSize, actorData.GetCurrentBoardSquare().transform.position);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_ThiefFanLaser)
		{
			AbilityUtil_Targeter_ThiefFanLaser abilityUtil_Targeter_ThiefFanLaser = (AbilityUtil_Targeter_ThiefFanLaser)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_ThiefFanLaser.m_rangeInSquares, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_BounceLaser)
		{
			AbilityUtil_Targeter_BounceLaser abilityUtil_Targeter_BounceLaser = (AbilityUtil_Targeter_BounceLaser)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_BounceLaser.m_maxDistancePerBounce, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_BounceActor)
		{
			AbilityUtil_Targeter_BounceActor abilityUtil_Targeter_BounceActor = (AbilityUtil_Targeter_BounceActor)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_BounceActor.m_maxDistancePerBounce, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_LaserWithCone)
		{
			AbilityUtil_Targeter_LaserWithCone abilityUtil_Targeter_LaserWithCone = (AbilityUtil_Targeter_LaserWithCone)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_LaserWithCone.m_distance, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_DirectionCone)
		{
			AbilityUtil_Targeter_DirectionCone abilityUtil_Targeter_DirectionCone = (AbilityUtil_Targeter_DirectionCone)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_DirectionCone.m_coneLengthRadius, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_CrossBeam)
		{
			AbilityUtil_Targeter_CrossBeam abilityUtil_Targeter_CrossBeam = (AbilityUtil_Targeter_CrossBeam)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_CrossBeam.m_distanceInSquares, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_ClaymoreKnockbackLaser)
		{
			AbilityUtil_Targeter_ClaymoreKnockbackLaser abilityUtil_Targeter_ClaymoreKnockbackLaser = (AbilityUtil_Targeter_ClaymoreKnockbackLaser)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_ClaymoreKnockbackLaser.GetLaserRange(), includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_ClaymoreCharge)
		{
			AbilityUtil_Targeter_ClaymoreCharge abilityUtil_Targeter_ClaymoreCharge = (AbilityUtil_Targeter_ClaymoreCharge)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_ClaymoreCharge.m_dashRangeInSquares, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_ClaymoreSlam)
		{
			AbilityUtil_Targeter_ClaymoreSlam abilityUtil_Targeter_ClaymoreSlam = (AbilityUtil_Targeter_ClaymoreSlam)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_ClaymoreSlam.m_laserRange, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_RampartGrab)
		{
			AbilityUtil_Targeter_RampartGrab abilityUtil_Targeter_RampartGrab = (AbilityUtil_Targeter_RampartGrab)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_RampartGrab.m_laserRange, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_TricksterLaser)
		{
			AbilityUtil_Targeter_TricksterLaser abilityUtil_Targeter_TricksterLaser = (AbilityUtil_Targeter_TricksterLaser)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_TricksterLaser.m_distance, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_TricksterCones)
		{
			AbilityUtil_Targeter_TricksterCones abilityUtil_Targeter_TricksterCones = (AbilityUtil_Targeter_TricksterCones)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_TricksterCones.m_coneInfo.m_radiusInSquares * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_StretchCone)
		{
			AbilityUtil_Targeter_StretchCone abilityUtil_Targeter_StretchCone = (AbilityUtil_Targeter_StretchCone)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_StretchCone.m_maxLengthSquares * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_ConeOrLaser)
		{
			if (thisAbility is SoldierConeOrLaser)
			{
				SoldierConeOrLaser soldierConeOrLaser = (SoldierConeOrLaser)thisAbility;
				list = GeneratePotentialAbilityTargetLocationsCircleNearFar_Separate(20, 72, soldierConeOrLaser.m_coneDistThreshold);
			}
			else
			{
				list = GeneratePotentialAbilityTargetLocations(thisAbility.GetRangeInSquares(0) * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
			}
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_ExoTether)
		{
			AbilityUtil_Targeter_ExoTether abilityUtil_Targeter_ExoTether = (AbilityUtil_Targeter_ExoTether)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_ExoTether.GetDistance(), includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_SweepSingleClickCone)
		{
			AbilityUtil_Targeter_SweepSingleClickCone abilityUtil_Targeter_SweepSingleClickCone = (AbilityUtil_Targeter_SweepSingleClickCone)thisAbility.Targeter;
			if (actorData.TechPoints >= 70 || abilityUtil_Targeter_SweepSingleClickCone.m_syncComponent.m_anchored)
			{
				list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_SweepSingleClickCone.m_rangeInSquares * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
			}
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_DashThroughWall)
		{
			AbilityUtil_Targeter_DashThroughWall abilityUtil_Targeter_DashThroughWall = (AbilityUtil_Targeter_DashThroughWall)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations((abilityUtil_Targeter_DashThroughWall.m_dashRangeInSquares + abilityUtil_Targeter_DashThroughWall.m_extraTotalDistanceIfThroughWalls) * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_ReverseStretchCone)
		{
			AbilityUtil_Targeter_ReverseStretchCone abilityUtil_Targeter_ReverseStretchCone = (AbilityUtil_Targeter_ReverseStretchCone)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_ReverseStretchCone.m_maxLengthSquares * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
		}
		else if (thisAbility.Targeter is AbilityUtil_Targeter_AoE_Smooth_FixedOffset)
		{
			AbilityUtil_Targeter_AoE_Smooth_FixedOffset abilityUtil_Targeter_AoE_Smooth_FixedOffset = (AbilityUtil_Targeter_AoE_Smooth_FixedOffset)thisAbility.Targeter;
			list = GeneratePotentialAbilityTargetLocations(abilityUtil_Targeter_AoE_Smooth_FixedOffset.m_maxOffsetFromCaster * Board.Get().squareSize, includeEnemies, includeFriendlies, includeSelf);
		}
		if (list != null)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			foreach (AbilityTarget item in list)
			{
				List<AbilityTarget> list2 = new List<AbilityTarget>();
				list2.Add(item);
				AbilityResults tempAbilityResults = new AbilityResults(actorData, thisAbility, null, s_gatherRealResults, true);
				thisAbility.GatherAbilityResults(list2, actorData, ref tempAbilityResults);
				PotentialChoice potentialChoice = ScoreResults(tempAbilityResults, actorData, false);
				potentialChoice.freeAction = thisAbility.IsFreeAction();
				potentialChoice.targetList = list2;
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
			List<AbilityTarget>.Enumerator enumerator = default(List<AbilityTarget>.Enumerator);
		}
		if (retVal != null && retVal.score > 0f)
		{
			m_potentialChoices[thisAction] = retVal;
		}
		yield break;
		yield break;
	}

	// added in rogues
	protected IEnumerator ScoreMultiTargetAbility(AbilityData.ActionType thisAction)
	{
		PotentialChoice potentialChoice = null;
		if (potentialChoice != null)
		{
			m_potentialChoices[thisAction] = potentialChoice;
		}
		yield break;
	}

	// added in rogues
	public virtual PotentialChoice ScoreResults(AbilityResults tempAbilityResults, ActorData caster, bool ignoreOverhealing)
	{
		Dictionary<ActorData, int> damageResults = tempAbilityResults.DamageResults;
		PotentialChoice potentialChoice = new PotentialChoice();
		potentialChoice.damageTotal = 0;
		potentialChoice.numEnemyTargetsHit = 0;
		potentialChoice.healingTotal = 0;
		potentialChoice.numTeamTargetsHit = 0;
		potentialChoice.score = 0f;
		potentialChoice.reasoning = "";
		int currentTurn = GameFlowData.Get().CurrentTurn;
		
		// rogues
		// if (tempAbilityResults.Ability.m_additionalAIScore > 0f)
		// {
		// 	potentialChoice.score += tempAbilityResults.Ability.m_additionalAIScore;
		// 	PotentialChoice potentialChoice2 = potentialChoice;
		// 	potentialChoice2.reasoning += string.Format("Adding {0} for authored additional AI score\n", tempAbilityResults.Ability.m_additionalAIScore);
		// }
		
		foreach (ActorData actorData in damageResults.Keys)
		{
			int num = damageResults[actorData];
			if (num > 0)
			{
				PotentialChoice potentialChoice3 = potentialChoice;
				potentialChoice3.reasoning += string.Format("This ability does instant healing (amount: {0})\n", num);
				if (tempAbilityResults.Ability is Card_Standard_Ability)
				{
					Card_Standard_Ability card_Standard_Ability = (Card_Standard_Ability)tempAbilityResults.Ability;
					if (card_Standard_Ability.m_applyEffect && card_Standard_Ability.m_effect.m_healingPerTurn > 0 && card_Standard_Ability.m_effect.m_duration > 1)
					{
						int num2 = (int)Mathf.Floor(card_Standard_Ability.m_effect.m_healingPerTurn * (card_Standard_Ability.m_effect.m_duration - 1) * 0.5f);
						PotentialChoice potentialChoice4 = potentialChoice;
						potentialChoice4.reasoning += string.Format("Adding {0} for heal over time (second wind) to amount.\n", num2);
						num += num2;
					}
				}
				if (ignoreOverhealing)
				{
					int num3 = actorData.GetMaxHitPoints() - actorData.GetHitPointsToDisplay();
					if (num > num3)
					{
						PotentialChoice potentialChoice5 = potentialChoice;
						potentialChoice5.reasoning += string.Format("Reduce amount by {0} because we're ignoring overhealing.\n", num - num3);
						num = num3;
					}
				}
				float num4 = num * 1f + (1f - actorData.GetHitPointPercent()) * 0.1f;
				potentialChoice.score += num4;
				PotentialChoice potentialChoice6 = potentialChoice;
				potentialChoice6.reasoning += string.Format("Score set to: {0} (added {1})\n", potentialChoice.score, num4);
				potentialChoice.healingTotal += num;
				potentialChoice.numTeamTargetsHit++;
			}
			else if (num < 0)
			{
				if (actorData.GetTeam() != caster.GetTeam() && GetEnemyPlayerAliveAndVisibleMultiplier(actorData) > 0f)
				{
					float num5 = ConvertDamageToScore(caster, actorData, num);
					potentialChoice.score += num5;
					PotentialChoice potentialChoice7 = potentialChoice;
					potentialChoice7.reasoning += string.Format("Added {0} score for damage done.  Score is now: {1} \n", num5, potentialChoice.score);
					if (actorData.GetHitPointsToDisplay() <= -num)
					{
						PotentialChoice potentialChoice8 = potentialChoice;
						potentialChoice8.reasoning += string.Format("The last add included a fatal damage flat bonus of {0}\n", 2f);
					}
					potentialChoice.damageTotal += -num;
					potentialChoice.numEnemyTargetsHit++;
					if (tempAbilityResults.Ability is RampartGrab)
					{
						float num6 = (actorData.GetFreePos() - caster.GetFreePos()).magnitude / 2f;
						potentialChoice.score += num6;
						PotentialChoice potentialChoice9 = potentialChoice;
						potentialChoice9.reasoning += string.Format("Added distance bonus for pull: {0}\n", num6);
					}
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
		foreach (KeyValuePair<ActorData, ActorHitResults> keyValuePair in tempAbilityResults.m_actorToHitResults)
		{
			if (keyValuePair.Value.m_effects != null)
			{
				foreach (Effect effect in keyValuePair.Value.m_effects)
				{
					if (effect is StandardActorEffect)
					{
						StandardActorEffect standardActorEffect = (StandardActorEffect)effect;
						float num7 = standardActorEffect.m_data.m_absorbAmount;
						if (num7 != 0f)
						{
							float score = potentialChoice.score;
							potentialChoice.numTeamTargetsHit++;
							for (int i = 0; i < standardActorEffect.m_data.m_duration; i++)
							{
								num7 /= 2f;
								potentialChoice.score += num7;
							}
							potentialChoice.score += (1f - keyValuePair.Key.GetHitPointPercent()) * 0.1f;
							float num8 = potentialChoice.score - score;
							PotentialChoice potentialChoice10 = potentialChoice;
							potentialChoice10.reasoning += string.Format("Adding {0} for generic shielding.\n", num8);
						}
						if (standardActorEffect.m_data != null && standardActorEffect.m_data.m_statusChanges != null && standardActorEffect.m_data.m_statusChanges.Length != 0)
						{
							for (int j = 0; j < standardActorEffect.m_data.m_statusChanges.Length; j++)
							{
								if (standardActorEffect.m_data.m_statusChanges[j] == StatusType.InvisibleToEnemies)
								{
									potentialChoice.score += 9f;
									PotentialChoice potentialChoice11 = potentialChoice;
									potentialChoice11.reasoning += "Adding 9 score for an invisibility effect.\n";
								}
								else if (standardActorEffect.m_data.m_statusChanges[j] == StatusType.Snared)
								{
									potentialChoice.score += 2f * standardActorEffect.m_data.m_duration;
									PotentialChoice potentialChoice12 = potentialChoice;
									potentialChoice12.reasoning += "Adding 2 score for a slow effect.\n";
								}
								else if (standardActorEffect.m_data.m_statusChanges[j] == StatusType.Weakened)
								{
									potentialChoice.score += 13f * standardActorEffect.m_data.m_duration;
									PotentialChoice potentialChoice13 = potentialChoice;
									potentialChoice13.reasoning += "Adding 13 score for a weakened effect.\n";
								}
								else if (standardActorEffect.m_data.m_statusChanges[j] == StatusType.Empowered)
								{
									potentialChoice.score += 13f * standardActorEffect.m_data.m_duration;
									PotentialChoice potentialChoice14 = potentialChoice;
									potentialChoice14.reasoning += "Adding 13 score for a might effect.\n";
								}
								else if (standardActorEffect.m_data.m_statusChanges[j] == StatusType.Unstoppable)
								{
									potentialChoice.score += 7f * standardActorEffect.m_data.m_duration;
									PotentialChoice potentialChoice15 = potentialChoice;
									potentialChoice15.reasoning += "Adding 7 score for an Unstoppable effect.\n";
								}
								else if (standardActorEffect.m_data.m_statusChanges[j] == StatusType.Hasted)
								{
									potentialChoice.score += 8f * standardActorEffect.m_data.m_duration;
									PotentialChoice potentialChoice16 = potentialChoice;
									potentialChoice16.reasoning += "Adding 8 score for a haste effect.\n";
								}
							}
						}
					}
				}
			}
		}
		foreach (KeyValuePair<ActorData, ActorHitResults> keyValuePair2 in tempAbilityResults.m_actorToHitResults)
		{
			if (keyValuePair2.Value.m_powerUpsToSteal != null)
			{
				foreach (ServerAbilityUtils.PowerUpStealData powerUpStealData in keyValuePair2.Value.m_powerUpsToSteal)
				{
					if (powerUpStealData.m_powerUp.TeamAllowedForPickUp(caster.GetTeam()))
					{
						if (powerUpStealData.m_powerUp.m_ability is PowerUp_Heal_Ability)
						{
							potentialChoice.score += ((PowerUp_Heal_Ability)powerUpStealData.m_powerUp.m_ability).m_healAmount;
							PotentialChoice potentialChoice17 = potentialChoice;
							potentialChoice17.reasoning += string.Format("Adding {0} for stealing a heal power up.\n", ((PowerUp_Heal_Ability)powerUpStealData.m_powerUp.m_ability).m_healAmount);
						}
						else if (powerUpStealData.m_powerUp.m_ability is PowerUp_Standard_Ability)
						{
							PowerUp_Standard_Ability powerUp_Standard_Ability = (PowerUp_Standard_Ability)powerUpStealData.m_powerUp.m_ability;
							if (powerUp_Standard_Ability.m_healAmount != 0)
							{
								potentialChoice.score += powerUp_Standard_Ability.m_healAmount;
								PotentialChoice potentialChoice18 = potentialChoice;
								potentialChoice18.reasoning += string.Format("Adding {0} for stealing a heal power up.\n", powerUp_Standard_Ability.m_healAmount);
							}
							else if (powerUp_Standard_Ability.m_effect != null && powerUp_Standard_Ability.m_effect.m_statusChanges != null && powerUp_Standard_Ability.m_effect.m_statusChanges.Length >= 0)
							{
								for (int k = 0; k < powerUp_Standard_Ability.m_effect.m_statusChanges.Length; k++)
								{
									if (powerUp_Standard_Ability.m_effect.m_statusChanges[k] == StatusType.Empowered)
									{
										potentialChoice.score += 16f;
										PotentialChoice potentialChoice19 = potentialChoice;
										potentialChoice19.reasoning += "Adding 16 score for stealing a might power up.\n";
									}
									else if (powerUp_Standard_Ability.m_effect.m_statusChanges[k] == StatusType.Hasted)
									{
										potentialChoice.score += 9f;
										PotentialChoice potentialChoice20 = potentialChoice;
										potentialChoice20.reasoning += "Adding 9 score for stealing a haste power up.\n";
									}
									else if (powerUp_Standard_Ability.m_effect.m_statusChanges[k] == StatusType.Energized)
									{
										potentialChoice.score += 6f;
										PotentialChoice potentialChoice21 = potentialChoice;
										potentialChoice21.reasoning += "adding 6 score for stealing an energized power up.\n";
									}
								}
							}
							else
							{
								potentialChoice.score += 5f;
								PotentialChoice potentialChoice22 = potentialChoice;
								potentialChoice22.reasoning += "Adding 5 score for stealing an unknown power up.\n";
							}
						}
						else
						{
							potentialChoice.score += 5f;
							PotentialChoice potentialChoice23 = potentialChoice;
							potentialChoice23.reasoning += "Adding 5 score for stealing an unknown power up.\n";
						}
					}
				}
			}
		}
		foreach (KeyValuePair<Vector3, PositionHitResults> keyValuePair3 in tempAbilityResults.m_positionToHitResults)
		{
			if (keyValuePair3.Value != null && keyValuePair3.Value.m_effects != null)
			{
				foreach (Effect effect2 in keyValuePair3.Value.m_effects)
				{
					if (effect2 != null && effect2 is SorceressDamageFieldEffect)
					{
						float score2 = potentialChoice.score;
						BoardSquare squareFromVec = Board.Get().GetSquareFromVec3(keyValuePair3.Key);
						SorceressDamageFieldEffect sorceressDamageFieldEffect = (SorceressDamageFieldEffect)effect2;
						Vector3 centerOfShape = AreaEffectUtils.GetCenterOfShape(sorceressDamageFieldEffect.m_shape, squareFromVec.ToVector3(), squareFromVec);
						List<ActorData> actorsInShape = AreaEffectUtils.GetActorsInShape(sorceressDamageFieldEffect.m_shape, centerOfShape, squareFromVec, sorceressDamageFieldEffect.m_penetrateLoS, caster, caster.GetOtherTeams(), null);
						if (actorsInShape.Count() <= 1)
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
						float num9 = potentialChoice.score - score2;
						PotentialChoice potentialChoice24 = potentialChoice;
						potentialChoice24.reasoning += string.Format("Added {0} score for Aurora damage field.\n", num9);
					}
				}
			}
		}
		if (potentialChoice.numEnemyTargetsHit > 1 && potentialChoice.score != 0f)
		{
			float score3 = potentialChoice.score;
			potentialChoice.score += 0.01f * potentialChoice.numEnemyTargetsHit;
			float num10 = potentialChoice.score - score3;
			PotentialChoice potentialChoice25 = potentialChoice;
			potentialChoice25.reasoning += string.Format("Adding a small bonus based on the number of enemy targets hit ({0}).\n", num10);
		}
		if (potentialChoice.numTeamTargetsHit > 1 && potentialChoice.score != 0f)
		{
			float score4 = potentialChoice.score;
			potentialChoice.score += 0.01f * potentialChoice.numTeamTargetsHit;
			float num11 = potentialChoice.score - score4;
			PotentialChoice potentialChoice26 = potentialChoice;
			potentialChoice26.reasoning += string.Format("Adding a small bonus based on the number of friendly targets hit ({0}).\n", num11);
		}
		return potentialChoice;
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
		HashSet<BoardSquare>.Enumerator enumerator = default(HashSet<BoardSquare>.Enumerator);
		if (bestSquare != startingSquare)
		{
			turnSM.SelectMovementSquareForMovement(bestSquare); // , true in rogues
			BotManager.Get().SelectDestination(actorData, bestSquare);
		}
		yield break;
		yield break;
	}

	// added in rogues
	private float GetEnemyPlayerAliveAndVisibleMultiplier(ActorData enemyActor)
	{
		BoardSquare currentBoardSquare = enemyActor.GetCurrentBoardSquare();
		if (enemyActor.IsDead() || currentBoardSquare == null || enemyActor.IgnoreForAbilityHits)
		{
			return 0f;
		}
		ActorData component = GetComponent<ActorData>();
		ActorStatus component2 = enemyActor.GetComponent<ActorStatus>();
		bool flag = !component.GetFogOfWar().IsVisible(currentBoardSquare);
		bool flag2 = enemyActor.IsInBrush();
		bool flag3 = component2.HasStatus(StatusType.InvisibleToEnemies);
		bool flag4 = component2.HasStatus(StatusType.Revealed);
		bool flag5 = component.GetActorStatus().HasStatus(StatusType.SeeInvisible);
		if ((!flag2 && !flag && (!flag3 || flag5)) || flag4)
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
			int cooldownRemaining = abilityEntry.GetCooldownRemaining();
			if (abilityEntry != null && abilityEntry.ability != null && abilityEntry.ability.CanRunInPhase(AbilityPriority.Evasion) && cooldownRemaining == 0)
			{
				return false;
			}
		}
		Ability abilityOfActionType = abilityData.GetAbilityOfActionType(AbilityData.ActionType.CARD_1);
		return !includeCards || !(abilityOfActionType != null);
	}

	// added in rogues
	protected float ConvertDamageToScore(ActorData caster, ActorData target, int amount)
	{
		float num = Mathf.Abs(amount);
		float num2 = num * 1f;
		float num3 = (1f - target.GetHitPointPercent()) * 0.1f;
		num2 += num3;
		if (target.GetHitPointsToDisplay() <= num)
		{
			num2 += 2f;
		}
		return num2;
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
