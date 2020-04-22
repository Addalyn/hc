using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISkinSelectButton : UICharacterVisualsSelectButton
{
	public Image m_characterIcon;

	public Slider m_progressionSlider;

	public _ButtonSwapSprite m_theButton;

	public int m_skinIndex;

	public UISkinData m_skinData;

	private UISkinBrowserPanel m_uiSkinBrowserPanel;

	protected override void Start()
	{
		base.Start();
		if (m_progressionSlider != null)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			m_progressionSlider.interactable = false;
		}
		if (!(m_theButton != null))
		{
			return;
		}
		while (true)
		{
			switch (1)
			{
			case 0:
				continue;
			}
			m_theButton.callback = OnSkinClicked;
			return;
		}
	}

	public void OnSkinClicked(BaseEventData data)
	{
		UIFrontEnd.PlaySound(FrontEndButtonSounds.CharacterSelectModAdd);
		m_uiSkinBrowserPanel.SkinClicked(this);
	}

	public void Setup(CharacterResourceLink m_theCharacter, UISkinData skinData, int skinIndex, UISkinBrowserPanel parent)
	{
		m_skinIndex = skinIndex;
		m_skinData = skinData;
		UIManager.SetGameObjectActive(m_lockedIcon, !skinData.m_isAvailable);
		m_uiSkinBrowserPanel = parent;
		if (m_characterIcon != null)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (skinData.m_skinImage == null)
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
				m_characterIcon.sprite = m_theCharacter.GetLoadingProfileIcon();
			}
			else
			{
				m_characterIcon.sprite = skinData.m_skinImage;
			}
		}
		if (m_progressionSlider != null)
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
			m_progressionSlider.minValue = 0f;
			m_progressionSlider.maxValue = 1f;
			m_progressionSlider.value = skinData.m_progressPct;
		}
		m_unlockTooltipTitle = string.Format(StringUtil.TR("SkinName", "Global"), m_theCharacter.GetSkinName(skinIndex));
		m_unlockTooltipText = m_theCharacter.GetSkinDescription(skinIndex);
		if (m_unlockTooltipText.IsNullOrEmpty())
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
			m_unlockTooltipText = string.Empty;
			int num = 0;
			if (skinData.m_unlockCharacterLevel > 1)
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
				m_unlockTooltipText = m_unlockTooltipText + string.Format(StringUtil.TR("UnlockedAtCharacterLevel", "Global"), skinData.m_unlockCharacterLevel) + Environment.NewLine;
				num++;
			}
			if (skinData.m_gameCurrencyCost > 0)
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
				m_unlockTooltipText = m_unlockTooltipText + string.Format(StringUtil.TR("BuyForNumberISO", "Global"), skinData.m_gameCurrencyCost) + Environment.NewLine;
				num++;
			}
			if (num > 1)
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
				m_unlockTooltipText = StringUtil.TR("ObtainedByMethods", "Global") + Environment.NewLine + m_unlockTooltipText + Environment.NewLine;
			}
		}
		else
		{
			m_unlockTooltipText += Environment.NewLine;
		}
		if (skinData.m_flavorText.IsNullOrEmpty())
		{
			return;
		}
		while (true)
		{
			switch (4)
			{
			case 0:
				continue;
			}
			m_unlockTooltipText = m_unlockTooltipText + "<i>" + skinData.m_flavorText + "</i>";
			return;
		}
	}
}
