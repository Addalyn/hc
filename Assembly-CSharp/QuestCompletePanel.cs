using LobbyGameClientMessages;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestCompletePanel : UIScene
{
	public QuestItem[] m_questItems;
	public TextMeshProUGUI m_contractCompletedLabel;
	private static QuestCompletePanel s_instance;
	private const float c_completionDisplaySeconds = 10f;
	private List<QuestCompleteData> m_recentlyCompletedQuests;
	private List<QuestCompleteNotification> m_savedNotificationsForGameOver = new List<QuestCompleteNotification>();
	private bool m_initialized;

	public static QuestCompletePanel Get()
	{
		return s_instance;
	}

	public override SceneType GetSceneType()
	{
		return SceneType.QuestComplete;
	}

	public override void Awake()
	{
		s_instance = this;
		m_recentlyCompletedQuests = new List<QuestCompleteData>();
		m_initialized = false;
		ClientGameManager.Get().OnQuestCompleteNotification += HandleQuestCompleteNotification;
		ClientGameManager.Get().OnQuestProgressChanged += HandleQuestProgressChanged;
		base.Awake();
	}

	private void Start()
	{
		if (!SteamManager.Initialized
		    || !ClientGameManager.Get().IsPlayerAccountDataAvailable()
		    || GameManager.Get() == null
		    || !GameManager.Get().GameplayOverrides.EnableSteamAchievements)
		{
			return;
		}

		foreach (KeyValuePair<int, QuestMetaData> current in ClientGameManager.Get().GetPlayerAccountData().QuestComponent.QuestMetaDatas)
		{
			if (current.Value.CompletedCount > 0)
			{
				QuestTemplate questTemplate = QuestWideData.Get().GetQuestTemplate(current.Key);
				if (questTemplate.AchievmentType != AchievementType.None)
				{
					SteamUserStats.SetAchievement("AR_QUEST_ID_" + current.Key);
				}
			}
		}
		foreach (QuestProgress value in ClientGameManager.Get().GetPlayerAccountData().QuestComponent.Progress.Values)
		{
			QuestItem.GetQuestProgress(value.Id, out int currentProgress, out int _);
			if (currentProgress > 0
			    && QuestWideData.Get().GetQuestTemplate(value.Id).AchievmentType != AchievementType.None)
			{
				SteamUserStats.SetStat("AR_QUEST_ID_" + value.Id, currentProgress);
			}
		}
	}

	private void OnDestroy()
	{
		if (ClientGameManager.Get() != null)
		{
			ClientGameManager.Get().OnQuestCompleteNotification -= HandleQuestCompleteNotification;
			ClientGameManager.Get().OnQuestProgressChanged -= HandleQuestProgressChanged;
		}
		s_instance = null;
	}

	public void RemoveQuestCompleteNotification(int questId)
	{
		foreach (QuestCompleteData current in m_recentlyCompletedQuests)
		{
			if (current.questId == questId)
			{
				m_recentlyCompletedQuests.Remove(current);
				break;
			}
		}
		Setup(true);
	}

	public int TotalQuestsToDisplayForGameOver()
	{
		return m_savedNotificationsForGameOver.Count;
	}

	public void DisplayGameOverQuestComplete(int index)
	{
		for (int i = 0; i < m_savedNotificationsForGameOver.Count; i++)
		{
			if (i <= index && m_savedNotificationsForGameOver[i].questId > 0)
			{
				DisplayNewQuestComplete(m_savedNotificationsForGameOver[i]);
				m_savedNotificationsForGameOver[i].questId = -1;
			}
		}
	}

	private void DisplayNewQuestComplete(QuestCompleteNotification message)
	{
		QuestTemplate questTemplate = QuestWideData.Get().GetQuestTemplate(message.questId);
		if (questTemplate != null && questTemplate.HideCompletion) // TODO CLIENT questTemplate == null || questTemplate.HideCompletion ?
		{
			return;
		}

		m_recentlyCompletedQuests.Add(new QuestCompleteData
		{
			questId = message.questId,
			rejectedCount = message.rejectedCount,
			fadeTime = Time.time + 10f
		});
		if (questTemplate.AchievmentType != AchievementType.None
		    && SteamManager.Initialized
		    && GameManager.Get() != null
		    && GameManager.Get().GameplayOverrides.EnableSteamAchievements)
		{
			SteamUserStats.SetAchievement("AR_QUEST_ID_" + questTemplate.Index);
		}
		Setup();
		if (UIPlayerNavPanel.Get() != null)
		{
			UIPlayerNavPanel.Get().NotifyQuestCompleted(m_recentlyCompletedQuests[0]);
		}
	}

	private void HandleQuestCompleteNotification(QuestCompleteNotification message)
	{
		if (UIGameOverScreen.Get() != null && UIGameOverScreen.Get().IsVisible)
		{
			m_savedNotificationsForGameOver.Add(message);
		}
		else
		{
			DisplayNewQuestComplete(message);
		}
	}

	private void HandleQuestProgressChanged(QuestProgress[] questProgresses)
	{
		if (!SteamManager.Initialized
		    || GameManager.Get() == null
		    || !GameManager.Get().GameplayOverrides.EnableSteamAchievements)
		{
			return;
		}

		foreach (QuestProgress questProgress in questProgresses)
		{
			QuestTemplate questTemplate = QuestWideData.Get().GetQuestTemplate(questProgress.Id);
			if (questTemplate.AchievmentType != AchievementType.None)
			{
				QuestItem.GetQuestProgress(questTemplate.Index, out int currentProgress, out int _);
				SteamUserStats.SetStat("AR_QUEST_ID_" + questTemplate.Index, currentProgress);
			}
		}
	}

	public void AddSpecialQuestNotification(int questId)
	{
		if (UIGameOverScreen.Get() != null && UIGameOverScreen.Get().IsVisible)
		{
			m_savedNotificationsForGameOver.Add(new QuestCompleteNotification
			{
				questId = questId,
				rejectedCount = 0
			});
			return;
		}
		
		QuestTemplate questTemplate = QuestWideData.Get().GetQuestTemplate(questId);
		if (questTemplate != null && questTemplate.HideCompletion)
		{
			return;
		}
		
		m_recentlyCompletedQuests.Add(new QuestCompleteData
		{
			questId = questId,
			rejectedCount = 0,
			fadeTime = Time.time + 10f
		});
		Setup();
	}

	private void Setup(bool removedQuestCompleteNotification = false)
	{
		for (int i = 0; i < m_questItems.Length; i++)
		{
			QuestItem questItem = m_questItems[i];
			if (i < m_recentlyCompletedQuests.Count)
			{
				QuestCompleteData questCompleteData = m_recentlyCompletedQuests[i];
				UIManager.SetGameObjectActive(questItem, true);
				questItem.SetState(QuestItemState.Finished);
				if (removedQuestCompleteNotification)
				{
					UIAnimationEventManager.Get().PlayAnimation(
						questItem.m_animatorController,
						"contractItemDefaultIDLE",
						null,
						string.Empty);
				}
				questItem.SetQuestId(questCompleteData.questId, questCompleteData.rejectedCount, true, removedQuestCompleteNotification);
				if (questItem.m_expandArrow != null)
				{
					UIManager.SetGameObjectActive(questItem.m_expandArrow, false);
				}
			}
			else
			{
				UIManager.SetGameObjectActive(questItem, false);
			}
		}
		if (m_recentlyCompletedQuests.Count > 0)
		{
			if (m_contractCompletedLabel != null)
			{
				UIManager.SetGameObjectActive(m_contractCompletedLabel, true);
			}
		}
		else if (m_contractCompletedLabel != null)
		{
			UIManager.SetGameObjectActive(m_contractCompletedLabel, false);
		}
	}

	private void Update()
	{
		bool updated = false;
		if (!m_initialized)
		{
			m_initialized = true;
			updated = true;
		}
		bool isGameOverScreenVisible = false;
		if (UIGameOverScreen.Get() != null)
		{
			isGameOverScreenVisible = UIGameOverScreen.Get().IsVisible;
		}
		else
		{
			if (SteamManager.Initialized
			    && GameManager.Get() != null
			    && GameManager.Get().GameplayOverrides.EnableSteamAchievements)
			{
				foreach (QuestCompleteNotification notify in m_savedNotificationsForGameOver)
				{
					int questId = notify.questId;
					if (questId < 0)
					{
						continue;
					}
					QuestTemplate questTemplate = QuestWideData.Get().GetQuestTemplate(questId);
					if (questTemplate.AchievmentType != AchievementType.None)
					{
						SteamUserStats.SetAchievement("AR_QUEST_ID_" + questId);
					}
				}
			}
			m_savedNotificationsForGameOver.Clear();
		}
		if (m_recentlyCompletedQuests.Count > 0 && !isGameOverScreenVisible)
		{
			while (m_recentlyCompletedQuests.Count > 0)
			{
				if (m_recentlyCompletedQuests[0].fadeTime < Time.time)
				{
					m_recentlyCompletedQuests.RemoveAt(0);
					updated = true;
					continue;
				}
				break;
			}
		}

		if (updated)
		{
			Setup();
		}
	}
}
