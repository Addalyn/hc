using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISeasonsLevelHeaderElement : MonoBehaviour
{
	public HorizontalLayoutGroup m_gridContainer;

	public TextMeshProUGUI m_headerTitle;

	public Image[] m_rewardImages;

	public UISeasonRepeatingRewardInfo[] infos;

	private void Awake()
	{
		for (int i = 0; i < m_rewardImages.Length; i++)
		{
			int index = i;
			m_rewardImages[i].GetComponent<UITooltipHoverObject>().Setup(TooltipType.InventoryItem, (UITooltipBase tooltip) => TooltipSetup(tooltip, index));
		}
		while (true)
		{
			switch (2)
			{
			case 0:
				continue;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			return;
		}
	}

	public void Setup(int imageIndex, UISeasonRepeatingRewardInfo rewardInfo)
	{
		if (infos == null)
		{
			infos = new UISeasonRepeatingRewardInfo[m_rewardImages.Length];
		}
		if (-1 >= imageIndex)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (imageIndex < m_rewardImages.Length)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						continue;
					}
					m_rewardImages[imageIndex].sprite = rewardInfo.GetDisplaySprite();
					infos[imageIndex] = rewardInfo;
					return;
				}
			}
			return;
		}
	}

	private bool TooltipSetup(UITooltipBase tooltip, int index)
	{
		SeasonReward seasonRewardReference = infos[index].GetSeasonRewardReference();
		if (seasonRewardReference is SeasonItemReward)
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					break;
				default:
				{
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					InventoryItemTemplate itemTemplate = InventoryWideData.Get().GetItemTemplate((seasonRewardReference as SeasonItemReward).ItemReward.ItemTemplateId);
					(tooltip as UIInventoryItemTooltip).Setup(itemTemplate);
					return true;
				}
				}
			}
		}
		return false;
	}
}
