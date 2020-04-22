using TMPro;
using UnityEngine;

public class UIMouseTargetingCursor : MonoBehaviour
{
	public TextMeshProUGUI m_clicksLeftLabel;

	public Animator m_animator;

	private Vector2 originalSizeOfCursor;

	private float originalFontSize;

	private bool initialized;

	private bool m_isVisible;

	private void Init()
	{
		if (initialized)
		{
			return;
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
			initialized = true;
			originalSizeOfCursor = (base.gameObject.transform as RectTransform).sizeDelta;
			originalFontSize = m_clicksLeftLabel.fontSize;
			return;
		}
	}

	public void ShowTargetCursor()
	{
		if (!m_isVisible)
		{
			m_isVisible = true;
			UIManager.SetGameObjectActive(this, true);
			m_animator.Play("MouseCursorTargetDefaultCLICK");
			DoUpdate();
		}
	}

	public void HideTargetCursor()
	{
		m_isVisible = false;
		if (!m_animator.isActiveAndEnabled)
		{
			return;
		}
		while (true)
		{
			switch (7)
			{
			case 0:
				continue;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			m_animator.Play("MouseCursorTargetDefaultEXIT", 0);
			return;
		}
	}

	public void DoUpdate(bool fromUpdate = false)
	{
		Init();
		if (!m_isVisible)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			Canvas componentInParent = base.gameObject.GetComponentInParent<Canvas>();
			Vector3 mousePosition = Input.mousePosition;
			float x = mousePosition.x / (float)Screen.width;
			Vector3 mousePosition2 = Input.mousePosition;
			Vector2 vector = new Vector2(x, mousePosition2.y / (float)Screen.height - 1f);
			Vector2 sizeDelta = (componentInParent.transform as RectTransform).sizeDelta;
			Vector2 anchoredPosition = new Vector2(vector.x * sizeDelta.x, vector.y * sizeDelta.y);
			Vector2 vector2 = new Vector2((float)Screen.width / sizeDelta.x, (float)Screen.height / sizeDelta.y);
			(base.gameObject.transform as RectTransform).sizeDelta = new Vector2(originalSizeOfCursor.x / vector2.x, originalSizeOfCursor.y / vector2.y);
			m_clicksLeftLabel.fontSize = originalFontSize / vector2.y;
			float y = anchoredPosition.y;
			Vector2 sizeDelta2 = (base.gameObject.transform as RectTransform).sizeDelta;
			anchoredPosition.y = y - sizeDelta2.y;
			if (fromUpdate)
			{
				Cursor.visible = false;
			}
			(base.gameObject.transform as RectTransform).anchoredPosition = anchoredPosition;
			int num = 1;
			if (GameFlowData.Get() != null)
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
				if (GameFlowData.Get().activeOwnedActorData != null)
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
					AbilityData abilityData = GameFlowData.Get().activeOwnedActorData.GetAbilityData();
					ActorTurnSM actorTurnSM = GameFlowData.Get().activeOwnedActorData.GetActorTurnSM();
					Ability selectedAbility = abilityData.GetSelectedAbility();
					if (abilityData != null)
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
						if (actorTurnSM != null)
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
							if (selectedAbility != null)
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
								int expectedNumberOfTargeters = selectedAbility.GetExpectedNumberOfTargeters();
								int targetSelectionIndex = actorTurnSM.GetTargetSelectionIndex();
								num = expectedNumberOfTargeters - targetSelectionIndex;
							}
						}
					}
				}
			}
			if (!(m_clicksLeftLabel != null))
			{
				return;
			}
			while (true)
			{
				switch (5)
				{
				case 0:
					continue;
				}
				if (fromUpdate)
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
					if (!(m_clicksLeftLabel.text != num.ToString()))
					{
						goto IL_02b2;
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
				m_animator.Play("MouseCursorTargetDefaultCLICK");
				goto IL_02b2;
				IL_02b2:
				m_clicksLeftLabel.text = num.ToString();
				return;
			}
		}
	}

	private void LateUpdate()
	{
		DoUpdate(true);
	}
}
