using LobbyGameClientMessages;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientCharacterPreloader : MonoBehaviour
{
	private Dictionary<CharacterResourceLink, List<CharacterVisualInfo>> m_linksToSkins = new Dictionary<CharacterResourceLink, List<CharacterVisualInfo>>();

	private Coroutine m_coroutine;

	private void Start()
	{
		if (Application.isEditor)
		{
			return;
		}
		while (true)
		{
			ClientGameManager.Get().OnGameInfoNotification += HandleGameInfoNotification;
			GameManager.Get().OnGameStatusChanged += HandleGameStatusChanged;
			return;
		}
	}

	private void OnDestroy()
	{
		if (Application.isEditor)
		{
			return;
		}
		while (true)
		{
			if (ClientGameManager.Get() != null)
			{
				ClientGameManager.Get().OnGameInfoNotification -= HandleGameInfoNotification;
			}
			if (GameManager.Get() != null)
			{
				GameManager.Get().OnGameStatusChanged -= HandleGameStatusChanged;
			}
			return;
		}
	}

	public void HandleGameInfoNotification(GameInfoNotification notification)
	{
		GameManager gameManager = GameManager.Get();
		if (!(gameManager == null))
		{
			if (gameManager.TeamInfo != null && gameManager.TeamInfo.TeamAPlayerInfo != null)
			{
				if (gameManager.TeamInfo.TeamBPlayerInfo != null && !(GameWideData.Get() == null) && gameManager.GameStatus >= GameStatus.FreelancerSelecting)
				{
					if (gameManager.GameStatus <= GameStatus.LoadoutSelecting)
					{
						Dictionary<CharacterResourceLink, List<CharacterVisualInfo>> dictionary = new Dictionary<CharacterResourceLink, List<CharacterVisualInfo>>(10);
						IEnumerator<LobbyPlayerInfo> enumerator = gameManager.TeamInfo.TeamAPlayerInfo.GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								LobbyPlayerInfo current = enumerator.Current;
								if (current != null && current.CharacterInfo != null)
								{
									if (!current.CharacterInfo.CharacterType.IsValid())
									{
									}
									else
									{
										CharacterResourceLink characterResourceLink = GameWideData.Get().GetCharacterResourceLink(current.CharacterInfo.CharacterType);
										if (characterResourceLink != null)
										{
											if (!dictionary.ContainsKey(characterResourceLink))
											{
												dictionary[characterResourceLink] = new List<CharacterVisualInfo>();
											}
											List<CharacterVisualInfo> list = dictionary[characterResourceLink];
											list.Add(current.CharacterInfo.CharacterSkin);
										}
									}
								}
							}
						}
						finally
						{
							if (enumerator != null)
							{
								while (true)
								{
									switch (2)
									{
									case 0:
										break;
									default:
										enumerator.Dispose();
										goto end_IL_0176;
									}
								}
							}
							end_IL_0176:;
						}
						IEnumerator<LobbyPlayerInfo> enumerator2 = gameManager.TeamInfo.TeamBPlayerInfo.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								LobbyPlayerInfo current2 = enumerator2.Current;
								if (current2 != null)
								{
									if (current2.CharacterInfo != null)
									{
										if (!current2.CharacterInfo.CharacterType.IsValid())
										{
										}
										else
										{
											CharacterResourceLink characterResourceLink2 = GameWideData.Get().GetCharacterResourceLink(current2.CharacterInfo.CharacterType);
											if (characterResourceLink2 != null)
											{
												if (!dictionary.ContainsKey(characterResourceLink2))
												{
													dictionary[characterResourceLink2] = new List<CharacterVisualInfo>();
												}
												List<CharacterVisualInfo> list2 = dictionary[characterResourceLink2];
												list2.Add(current2.CharacterInfo.CharacterSkin);
											}
										}
									}
								}
							}
						}
						finally
						{
							if (enumerator2 != null)
							{
								while (true)
								{
									switch (7)
									{
									case 0:
										break;
									default:
										enumerator2.Dispose();
										goto end_IL_026c;
									}
								}
							}
							end_IL_026c:;
						}
						if (!Different(dictionary, m_linksToSkins))
						{
							return;
						}
						while (true)
						{
							if (m_coroutine != null)
							{
								StopCoroutine(m_coroutine);
								m_coroutine = null;
							}
							m_linksToSkins = dictionary;
							m_coroutine = StartCoroutine(CharacterPreloadCoroutine());
							return;
						}
					}
				}
			}
		}
		StopPreloading();
	}

	private void HandleGameStatusChanged(GameStatus gameStatus)
	{
		if (gameStatus <= GameStatus.LoadoutSelecting)
		{
			return;
		}
		while (true)
		{
			StopPreloading();
			return;
		}
	}

	private void StopPreloading()
	{
		if (m_coroutine == null)
		{
			return;
		}
		while (true)
		{
			StopCoroutine(m_coroutine);
			m_coroutine = null;
			m_linksToSkins.Clear();
			return;
		}
	}

	private static bool Different(Dictionary<CharacterResourceLink, List<CharacterVisualInfo>> lhs, Dictionary<CharacterResourceLink, List<CharacterVisualInfo>> rhs)
	{
		if (lhs.Count != rhs.Count)
		{
			while (true)
			{
				switch (4)
				{
				case 0:
					break;
				default:
					return true;
				}
			}
		}
		using (Dictionary<CharacterResourceLink, List<CharacterVisualInfo>>.Enumerator enumerator = lhs.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<CharacterResourceLink, List<CharacterVisualInfo>> current = enumerator.Current;
				if (!rhs.ContainsKey(current.Key))
				{
					while (true)
					{
						switch (5)
						{
						case 0:
							break;
						default:
							return true;
						}
					}
				}
				List<CharacterVisualInfo> value = current.Value;
				List<CharacterVisualInfo> list = rhs[current.Key];
				if (value.Count != list.Count)
				{
					while (true)
					{
						switch (2)
						{
						case 0:
							break;
						default:
							return true;
						}
					}
				}
				for (int i = 0; i < value.Count; i++)
				{
					if (!list.Contains(value[i]))
					{
						while (true)
						{
							switch (4)
							{
							case 0:
								break;
							default:
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	private IEnumerator CharacterPreloadCoroutine()
	{
		yield return new WaitForSeconds(2f);
		/*Error: Unable to find new state assignment for yield return*/;
	}

	private void HandleCharacterResourceLoaded(LoadedCharacterSelection loadedCharacter)
	{
	}
}
