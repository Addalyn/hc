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
		bool flag = false;
		foreach (LobbyPlayerInfo item in GameManager.Get().TeamInfo.TeamPlayerInfo)
		{
			if (!(component.PlayerData == null))
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
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				if (component.PlayerData.LookupDetails() == null)
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
				}
				else if (item.PlayerId == component.PlayerData.LookupDetails().m_lobbyPlayerInfoId)
				{
					while (true)
					{
						switch (2)
						{
						case 0:
							break;
						default:
							botDifficulty = item.Difficulty;
							flag = item.BotCanTaunt;
							goto end_IL_0027;
						}
					}
				}
			}
		}
		previousBrainStack = new Stack<NPCBrain>();
		if (!(GetComponent<NPCBrain>() == null))
		{
			return;
		}
		while (true)
		{
			switch (3)
			{
			case 0:
				continue;
			}
			if (!(component.GetName() == "Sniper") && !(component.GetName() == "RageBeast") && !(component.GetName() == "Scoundrel"))
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
				if (!(component.GetName() == "RobotAnimal"))
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
					if (!(component.GetName() == "NanoSmith"))
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
						if (!(component.GetName() == "Thief"))
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
							if (!(component.GetName() == "BattleMonk") && !(component.GetName() == "BazookaGirl"))
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
								if (!(component.GetName() == "SpaceMarine"))
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
									if (!(component.GetName() == "Gremlins"))
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
										if (!(component.GetName() == "Tracker"))
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
											if (!(component.GetName() == "DigitalSorceress"))
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
												if (!(component.GetName() == "Spark") && !(component.GetName() == "Claymore") && !(component.GetName() == "Rampart"))
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
													if (!(component.GetName() == "Trickster"))
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
														if (!(component.GetName() == "Blaster"))
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
															if (!(component.GetName() == "FishMan"))
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
																if (!(component.GetName() == "Thief"))
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
																	if (!(component.GetName() == "Soldier"))
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
																		if (!(component.GetName() == "Exo"))
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
																			if (!(component.GetName() == "Martyr") && !(component.GetName() == "Sensei"))
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
																				if (!(component.GetName() == "TeleportingNinja") && !(component.GetName() == "Manta"))
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
																					if (!(component.GetName() == "Valkyrie"))
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
																						if (!(component.GetName() == "Archer"))
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
																							if (!(component.GetName() == "Samurai"))
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
																								if (!(component.GetName() == "Cleric"))
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
																									if (!(component.GetName() == "Neko"))
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
																										if (!(component.GetName() == "Scamp"))
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
																											if (!(component.GetName() == "Dino"))
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
																												if (!(component.GetName() == "Iceborg"))
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
																													if (!(component.GetName() == "Fireborg"))
																													{
																														Log.Info("Using Generic AI for {0}", component.GetName());
																														return;
																													}
																													while (true)
																													{
																														switch (6)
																														{
																														case 0:
																															continue;
																														}
																														break;
																													}
																												}
																											}
																										}
																									}
																								}
																							}
																						}
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			NPCBrain_Adaptive.Create(this, component.transform, botDifficulty, flag);
			Log.Info("Making Adaptive AI for {0} at difficulty {1}, can taunt: {2}", component.GetName(), botDifficulty.ToString(), flag);
			if (IAmTheOnlyBotOnATwoPlayerTeam(component))
			{
				while (true)
				{
					switch (4)
					{
					case 0:
						continue;
					}
					component.GetComponent<NPCBrain_Adaptive>().SendDecisionToTeamChat(true);
					return;
				}
			}
			return;
		}
	}

	public BoardSquare GetClosestEnemyPlayerSquare(bool includeInvisibles, out int numEnemiesInRange)
	{
		numEnemiesInRange = 0;
		ActorData component = GetComponent<ActorData>();
		List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(component.GetOpposingTeam());
		BoardSquare currentBoardSquare = component.GetCurrentBoardSquare();
		BoardSquare boardSquare = null;
		using (List<ActorData>.Enumerator enumerator = allTeamMembers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ActorData current = enumerator.Current;
				BoardSquare currentBoardSquare2 = current.GetCurrentBoardSquare();
				if (!current.IsDead())
				{
					if (currentBoardSquare2 == null)
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
						if (1 == 0)
						{
							/*OpCode not supported: LdMemberToken*/;
						}
					}
					else
					{
						float num = currentBoardSquare.HorizontalDistanceOnBoardTo(currentBoardSquare2);
						if (num <= m_combatRange)
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
							numEnemiesInRange++;
						}
						if (!includeInvisibles)
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
							if (!component.GetFogOfWar().IsVisible(currentBoardSquare2))
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
								continue;
							}
						}
						if (boardSquare == null)
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
							boardSquare = currentBoardSquare2;
						}
						else
						{
							float num2 = currentBoardSquare.HorizontalDistanceOnBoardTo(boardSquare);
							if (num < num2)
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
								boardSquare = currentBoardSquare2;
							}
						}
					}
				}
			}
			while (true)
			{
				switch (1)
				{
				case 0:
					break;
				default:
					return boardSquare;
				}
			}
		}
	}

	public BoardSquare GetRetreatSquare()
	{
		ActorData component = GetComponent<ActorData>();
		List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(component.GetOpposingTeam());
		BoardSquare currentBoardSquare = component.GetCurrentBoardSquare();
		Vector3 a = new Vector3(0f, 0f, 0f);
		using (List<ActorData>.Enumerator enumerator = allTeamMembers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ActorData current = enumerator.Current;
				BoardSquare currentBoardSquare2 = current.GetCurrentBoardSquare();
				if (!current.IsDead())
				{
					if (currentBoardSquare2 == null)
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
					}
					else
					{
						float num = currentBoardSquare.HorizontalDistanceOnBoardTo(currentBoardSquare2);
						if (num <= m_retreatFromRange)
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
							Vector3 vector = currentBoardSquare2.ToVector3() - currentBoardSquare.ToVector3();
							vector.Normalize();
							a += vector;
						}
					}
				}
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
		Vector3 a2 = -a;
		a2.Normalize();
		Vector3 vector2 = currentBoardSquare.ToVector3() + a2 * m_retreatFromRange;
		return Board.Get()._0013(vector2.x, vector2.z);
	}

	public BoardSquare GetAdvanceSquare()
	{
		int numEnemiesInRange;
		BoardSquare closestEnemyPlayerSquare = GetClosestEnemyPlayerSquare(true, out numEnemiesInRange);
		if (closestEnemyPlayerSquare == null)
		{
			return null;
		}
		Vector3 a = closestEnemyPlayerSquare.ToVector3();
		ActorData component = GetComponent<ActorData>();
		BoardSquare currentBoardSquare = component.GetCurrentBoardSquare();
		Vector3 vector = currentBoardSquare.ToVector3();
		Vector3 b = a - vector;
		if (numEnemiesInRange > 1)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			float magnitude = b.magnitude;
			if (magnitude > m_idealRange)
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
				b.Normalize();
				b *= m_idealRange;
			}
		}
		Vector3 zero = Vector3.zero;
		List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(component.GetTeam());
		using (List<ActorData>.Enumerator enumerator = allTeamMembers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ActorData current = enumerator.Current;
				if (!current.IsDead() || current == component)
				{
					BoardSquare currentBoardSquare2 = current.GetCurrentBoardSquare();
					if (currentBoardSquare2 != null && currentBoardSquare.HorizontalDistanceOnBoardTo(currentBoardSquare2) < m_idealRange)
					{
						Vector3 a2 = currentBoardSquare2.ToVector3() - vector;
						a2.Normalize();
						zero -= a2 * 1.5f;
					}
				}
			}
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				break;
			}
		}
		Vector3 vector2 = vector + b + zero;
		BoardSquare boardSquareUnsafe = Board.Get().GetBoardSquareUnsafe(vector2.x, vector2.z);
		return Board.Get()._0018(boardSquareUnsafe);
	}

	public void SelectBotAbilityMods()
	{
		NPCBrain component = GetComponent<NPCBrain>();
		if (component != null)
		{
			while (true)
			{
				switch (3)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					component.SelectBotAbilityMods();
					return;
				}
			}
		}
		SelectBotAbilityMods_Brainless();
	}

	public void SelectBotCards()
	{
		NPCBrain component = GetComponent<NPCBrain>();
		if (component != null)
		{
			while (true)
			{
				switch (2)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					component.SelectBotCards();
					return;
				}
			}
		}
		SelectBotCards_Brainless();
	}

	public void SelectBotAbilityMods_Brainless()
	{
		ActorData component = GetComponent<ActorData>();
		AbilityData abilityData = component.GetAbilityData();
		List<Ability> abilitiesAsList = abilityData.GetAbilitiesAsList();
		CharacterModInfo selectedMods = default(CharacterModInfo);
		int num = 0;
		using (List<Ability>.Enumerator enumerator = abilitiesAsList.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Ability current = enumerator.Current;
				AbilityMod defaultModForAbility = AbilityModManager.Get().GetDefaultModForAbility(current);
				int num2;
				if (defaultModForAbility != null)
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
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					num2 = defaultModForAbility.m_abilityScopeId;
				}
				else
				{
					num2 = -1;
				}
				int mod = num2;
				selectedMods.SetModForAbility(num, mod);
				num++;
			}
			while (true)
			{
				switch (6)
				{
				case 0:
					continue;
				}
				break;
			}
		}
		component.m_selectedMods = selectedMods;
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
			while (true)
			{
				switch (2)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					return false;
				}
			}
		}
		bool flag = false;
		using (List<LobbyPlayerInfo>.Enumerator enumerator = GameManager.Get().TeamInfo.TeamPlayerInfo.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				LobbyPlayerInfo current = enumerator.Current;
				if (current.TeamId != actorData.GetTeam())
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
				}
				else if (current.PlayerId == playerDetails.m_lobbyPlayerInfoId)
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
				}
				else
				{
					if (flag)
					{
						while (true)
						{
							switch (4)
							{
							case 0:
								break;
							default:
								return false;
							}
						}
					}
					if (current.IsAIControlled)
					{
						while (true)
						{
							switch (5)
							{
							case 0:
								break;
							default:
								return false;
							}
						}
					}
					flag = true;
				}
			}
			while (true)
			{
				switch (2)
				{
				case 0:
					continue;
				}
				break;
			}
		}
		return true;
	}
}
