using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterColor
{
	public string m_name;
	public Color m_UIDisplayColor;
	[AssetFileSelector("Assets/UI/Textures/Resources/QuestRewards/", "QuestRewards/", ".png")]
	public string m_iconResourceString;
	public PrefabResourceLink m_heroPrefab;
	public string m_description;
	public string m_flavorText;
	public StyleLevelType m_styleLevel;
	public bool m_isHidden;
	public GameBalanceVars.ColorUnlockData m_colorUnlockData;
	public int m_sortOrder;
	public int m_requiredLevelForEquip;
	public Sprite m_loadingProfileIcon;
	[Header("-- Prefab Replacements --")]
	public PrefabResourceLink[] m_satellitePrefabs;
	[Header("-- Linked Colors --")]
	public List<CharacterLinkedColor> m_linkedColors;
	public PrefabReplacement[] m_replacementSequences;

	public static string GetIconResourceStringForStyleLevelType(StyleLevelType type)
	{
		switch (type)
		{
			case StyleLevelType.Advanced:
				return "skin_advancedIcon";
			case StyleLevelType.Expert:
				return "skin_expertIcon";
			case StyleLevelType.Mastery:
				return "skin_MasteryIcon";
			default:
				return string.Empty;
		}
	}

	public int DebugGetIsoPriceFromUnlockCondition()
	{
		if (m_colorUnlockData == null
		    || m_colorUnlockData.m_unlockData == null
		    || m_colorUnlockData.m_unlockData.UnlockConditions == null)
		{
			return 0;
		}
		foreach (GameBalanceVars.UnlockCondition unlockCondition in m_colorUnlockData.m_unlockData.UnlockConditions)
		{
			if (unlockCondition.ConditionType == GameBalanceVars.UnlockData.UnlockType.Purchase)
			{
				return unlockCondition.typeSpecificData2;
			}
		}
		return 0;
	}
}
