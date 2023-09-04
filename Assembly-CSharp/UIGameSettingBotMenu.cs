using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGameSettingBotMenu : UITooltipBase
{
	public TextMeshProUGUI[] m_botSelection;

	private UITeamMemberEntry m_teamMemberEntry;

	private List<LobbyPlayerInfo> m_playerList;

	public void Awake()
	{
		for (int i = 0; i < m_botSelection.Length; i++)
		{
			UIEventTriggerUtils.AddListener(m_botSelection[i].gameObject, EventTriggerType.PointerClick, BotMenuItemClicked);
			UIEventTriggerUtils.AddListener(m_botSelection[i].gameObject, EventTriggerType.PointerEnter, BotMenuItemEnter);
			UIEventTriggerUtils.AddListener(m_botSelection[i].gameObject, EventTriggerType.PointerExit, BotMenuItemExit);
		}
	}

	public void Setup(UITeamMemberEntry entry, Image button)
	{
		m_teamMemberEntry = entry;
		if (m_playerList == null)
		{
			m_playerList = new List<LobbyPlayerInfo>();
		}
		m_playerList.Clear();
		int num = 0;
		int controllingPlayerId = 0;
		if (entry.m_playerInfo.IsRemoteControlled)
		{
			controllingPlayerId = entry.m_playerInfo.ControllingPlayerId;
			m_playerList.Add(null);
			m_botSelection[0].text = StringUtil.TR("Bot", "Global");
			num = 1;
		}

		foreach (LobbyPlayerInfo player in GameManager.Get().TeamInfo.TeamPlayerInfo)
		{
			if (!player.IsNPCBot
			    && !player.IsRemoteControlled
			    && controllingPlayerId != player.PlayerId
			    && !player.IsSpectator)
			{
				m_playerList.Add(player);
				m_botSelection[num].text = player.GetHandle();
				UIManager.SetGameObjectActive(m_botSelection[num], true);
				num++;
			}
		}
		for (int i = num; i < m_botSelection.Length; i++)
		{
			UIManager.SetGameObjectActive(m_botSelection[i], false);
		}
	}

	private void BotMenuItemEnter(BaseEventData data)
	{
		int num = 0;
		while (true)
		{
			if (num < m_botSelection.Length)
			{
				if (m_botSelection[num].gameObject == (data as PointerEventData).pointerCurrentRaycast.gameObject)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		m_botSelection[num].color = Color.white;
	}

	private void BotMenuItemExit(BaseEventData data)
	{
		ClearButtonSelections((data as PointerEventData).pointerCurrentRaycast.gameObject);
	}

	private void ClearButtonSelections(GameObject gObj)
	{
		for (int i = 0; i < m_botSelection.Length; i++)
		{
			if (m_botSelection[i].gameObject != gObj)
			{
				m_botSelection[i].color = Color.grey;
			}
		}
		while (true)
		{
			switch (6)
			{
			default:
				return;
			case 0:
				break;
			}
		}
	}

	private void BotMenuItemClicked(BaseEventData data)
	{
		for (int i = 0; i < m_botSelection.Length; i++)
		{
			if (m_botSelection[i].gameObject == (data as PointerEventData).pointerCurrentRaycast.gameObject)
			{
				m_teamMemberEntry.SetControllingPlayerInfo(m_playerList[i]);
			}
		}
		while (true)
		{
			SetVisible(false);
			return;
		}
	}

	private void OnDisable()
	{
		ClearButtonSelections(null);
		m_teamMemberEntry.ClearSelection(null);
	}
}
