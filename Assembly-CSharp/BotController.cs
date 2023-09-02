using System.Collections.Generic;
using UnityEngine;

public class BotController : MonoBehaviour
{
	public float m_combatRange = 7f;
	public float m_idealRange = 5f;
	public float m_retreatFromRange = 15f;

	[HideInInspector]
	public Stack<NPCBrain> previousBrainStack;

	private int m_aiStartedForTurn;

	private void Start()
	{
		ActorData component = GetComponent<ActorData>();
		BotDifficulty botDifficulty = BotDifficulty.Expert;
		bool botCanTaunt = false;
		foreach (LobbyPlayerInfo lobbyPlayerInfo in GameManager.Get().TeamInfo.TeamPlayerInfo)
		{
			if (component.PlayerData != null
			    && component.PlayerData.LookupDetails() != null
			    && lobbyPlayerInfo.PlayerId == component.PlayerData.LookupDetails().m_lobbyPlayerInfoId)
			{
				botDifficulty = lobbyPlayerInfo.Difficulty;
				botCanTaunt = lobbyPlayerInfo.BotCanTaunt;
				break;
			}
		}
		previousBrainStack = new Stack<NPCBrain>();
		if (GetComponent<NPCBrain>() == null)
		{
			if (component.GetClassName() != "Sniper"
			    && component.GetClassName() != "RageBeast"
			    && component.GetClassName() != "Scoundrel"
			    && component.GetClassName() != "RobotAnimal"
			    && component.GetClassName() != "NanoSmith"
			    && component.GetClassName() != "Thief"
			    && component.GetClassName() != "BattleMonk"
			    && component.GetClassName() != "BazookaGirl"
			    && component.GetClassName() != "SpaceMarine"
			    && component.GetClassName() != "Gremlins"
			    && component.GetClassName() != "Tracker"
			    && component.GetClassName() != "DigitalSorceress"
			    && component.GetClassName() != "Spark"
			    && component.GetClassName() != "Claymore"
			    && component.GetClassName() != "Rampart"
			    && component.GetClassName() != "Trickster"
			    && component.GetClassName() != "Blaster"
			    && component.GetClassName() != "FishMan"
			    && component.GetClassName() != "Thief"
			    && component.GetClassName() != "Soldier"
			    && component.GetClassName() != "Exo"
			    && component.GetClassName() != "Martyr"
			    && component.GetClassName() != "Sensei"
			    && component.GetClassName() != "TeleportingNinja"
			    && component.GetClassName() != "Manta"
			    && component.GetClassName() != "Valkyrie"
			    && component.GetClassName() != "Archer"
			    && component.GetClassName() != "Samurai"
			    && component.GetClassName() != "Cleric"
			    && component.GetClassName() != "Neko"
			    && component.GetClassName() != "Scamp"
			    && component.GetClassName() != "Dino"
			    && component.GetClassName() != "Iceborg"
			    && component.GetClassName() != "Fireborg")
			{
				Log.Info("Using Generic AI for {0}", component.GetClassName());
				return;
			}
			NPCBrain_Adaptive.Create(this, component.transform, botDifficulty, botCanTaunt);
			Log.Info("Making Adaptive AI for {0} at difficulty {1}, can taunt: {2}",
				component.GetClassName(), botDifficulty.ToString(), botCanTaunt);
			if (IAmTheOnlyBotOnATwoPlayerTeam(component))
			{
				component.GetComponent<NPCBrain_Adaptive>().SendDecisionToTeamChat(true);
			}
		}
	}

	public BoardSquare GetClosestEnemyPlayerSquare(bool includeInvisibles, out int numEnemiesInRange)
	{
		numEnemiesInRange = 0;
		ActorData actorData = GetComponent<ActorData>();
		List<ActorData> enemies = GameFlowData.Get().GetAllTeamMembers(actorData.GetEnemyTeam());
		BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
		BoardSquare closestEnemySquare = null;
		foreach (ActorData enemyActor in enemies)
		{
			BoardSquare enemySquare = enemyActor.GetCurrentBoardSquare();
			if (!enemyActor.IsDead() && enemySquare != null)
			{
				float distToEnemy = currentBoardSquare.HorizontalDistanceOnBoardTo(enemySquare);
				if (distToEnemy <= m_combatRange)
				{
					numEnemiesInRange++;
				}
				if (!includeInvisibles && !actorData.GetFogOfWar().IsVisible(enemySquare))
				{
					continue;
				}
				if (closestEnemySquare == null)
				{
					closestEnemySquare = enemySquare;
				}
				else
				{
					float dist = currentBoardSquare.HorizontalDistanceOnBoardTo(closestEnemySquare);
					if (distToEnemy < dist)
					{
						closestEnemySquare = enemySquare;
					}
				}
			}
		}
		return closestEnemySquare;
	}

	public BoardSquare GetRetreatSquare()
	{
		ActorData actorData = GetComponent<ActorData>();
		List<ActorData> enemies = GameFlowData.Get().GetAllTeamMembers(actorData.GetEnemyTeam());
		BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
		Vector3 dirToEnemies = new Vector3(0f, 0f, 0f);
		foreach (ActorData enemyActor in enemies)
		{
			BoardSquare enemySquare = enemyActor.GetCurrentBoardSquare();
			if (!enemyActor.IsDead() && enemySquare != null)
			{
				float distToEnemy = currentBoardSquare.HorizontalDistanceOnBoardTo(enemySquare);
				if (distToEnemy <= m_retreatFromRange)
				{
					Vector3 dirToEnemy = enemySquare.ToVector3() - currentBoardSquare.ToVector3();
					dirToEnemy.Normalize();
					dirToEnemies += dirToEnemy;
				}
			}
		}
		Vector3 dirFromEnemies = -dirToEnemies;
		dirFromEnemies.Normalize();
		Vector3 retreatPos = currentBoardSquare.ToVector3() + dirFromEnemies * m_retreatFromRange;
		return Board.Get().GetClosestValidForGameplaySquareTo(retreatPos.x, retreatPos.z);
	}

	public BoardSquare GetAdvanceSquare()
	{
		BoardSquare closestEnemyPlayerSquare = GetClosestEnemyPlayerSquare(true, out int numEnemiesInRange);
		if (closestEnemyPlayerSquare == null)
		{
			return null;
		}
		Vector3 closestEnemyPos = closestEnemyPlayerSquare.ToVector3();
		ActorData actorData = GetComponent<ActorData>();
		BoardSquare currentBoardSquare = actorData.GetCurrentBoardSquare();
		Vector3 currentPos = currentBoardSquare.ToVector3();
		Vector3 dirToClosestEnemy = closestEnemyPos - currentPos;
		if (numEnemiesInRange > 1)
		{
			float distToClosestEnemy = dirToClosestEnemy.magnitude;
			if (distToClosestEnemy > m_idealRange)
			{
				dirToClosestEnemy.Normalize();
				dirToClosestEnemy *= m_idealRange;
			}
		}
		Vector3 dirFromAllies = Vector3.zero;
		List<ActorData> allies = GameFlowData.Get().GetAllTeamMembers(actorData.GetTeam());
		foreach (ActorData allyActor in allies)
		{
			if (allyActor.IsDead() && allyActor != actorData)
			{
				continue;
			}
			BoardSquare allySquare = allyActor.GetCurrentBoardSquare();
			if (allySquare != null && currentBoardSquare.HorizontalDistanceOnBoardTo(allySquare) < m_idealRange)
			{
				Vector3 dirToAlly = allySquare.ToVector3() - currentPos;
				dirToAlly.Normalize();
				dirFromAllies -= dirToAlly * 1.5f;
			}
		}
		Vector3 advancePos = currentPos + dirToClosestEnemy + dirFromAllies;
		BoardSquare advanceSquare = Board.Get().GetSquareClosestToPos(advancePos.x, advancePos.z);
		return Board.Get().GetClosestValidForGameplaySquareTo(advanceSquare);
	}

	public void SelectBotAbilityMods()
	{
		NPCBrain component = GetComponent<NPCBrain>();
		if (component != null)
		{
			component.SelectBotAbilityMods();
		}
		else
		{
			SelectBotAbilityMods_Brainless();
		}
	}

	public void SelectBotCards()
	{
		NPCBrain component = GetComponent<NPCBrain>();
		if (component != null)
		{
			component.SelectBotCards();
		}
		else
		{
			SelectBotCards_Brainless();
		}
	}

	public void SelectBotAbilityMods_Brainless()
	{
		ActorData actorData = GetComponent<ActorData>();
		CharacterModInfo selectedMods = default(CharacterModInfo);
		int i = 0;
		foreach (Ability ability in actorData.GetAbilityData().GetAbilitiesAsList())
		{
			AbilityMod defaultModForAbility = AbilityModManager.Get().GetDefaultModForAbility(ability);
			int mod = defaultModForAbility != null ? defaultModForAbility.m_abilityScopeId : -1;
			selectedMods.SetModForAbility(i, mod);
			i++;
		}
		actorData.m_selectedMods = selectedMods;
	}

	public void SelectBotCards_Brainless()
	{
		ActorData component = GetComponent<ActorData>();
		CharacterCardInfo cardInfo = default(CharacterCardInfo);
		cardInfo.PrepCard = CardManagerData.Get().GetDefaultPrepCardType();
		cardInfo.CombatCard = CardManagerData.Get().GetDefaultCombatCardType();
		cardInfo.DashCard = CardManagerData.Get().GetDefaultDashCardType();
		CardManager.Get().SetDeckAndGiveCards(component, cardInfo);
	}

	public bool IAmTheOnlyBotOnATwoPlayerTeam(ActorData actorData)
	{
		PlayerDetails playerDetails = actorData.PlayerData.LookupDetails();
		if (playerDetails == null)
		{
			return false;
		}
		bool haveOneTeammate = false;
		foreach (LobbyPlayerInfo lobbyPlayerInfo in GameManager.Get().TeamInfo.TeamPlayerInfo)
		{
			if (lobbyPlayerInfo.TeamId == actorData.GetTeam()
			    && lobbyPlayerInfo.PlayerId != playerDetails.m_lobbyPlayerInfoId)
			{
				if (haveOneTeammate)
				{
					return false;
				}

				if (lobbyPlayerInfo.IsAIControlled)
				{
					return false;
				}

				haveOneTeammate = true;
			}
		}
		return true;
	}
}
