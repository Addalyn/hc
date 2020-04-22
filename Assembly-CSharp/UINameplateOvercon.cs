using UnityEngine;
using UnityEngine.UI;

public class UINameplateOvercon : MonoBehaviour
{
	public Image m_foregroundImg;

	public GameObject m_customPrefabParent;

	private CanvasGroup m_canvasGroup;

	private bool m_initialized;

	private float m_timeToDestroy = -1f;

	private void Awake()
	{
		m_canvasGroup = GetComponent<CanvasGroup>();
		if (!(m_canvasGroup != null))
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
			m_canvasGroup.blocksRaycasts = false;
			m_canvasGroup.interactable = false;
			return;
		}
	}

	public void Initialize(ActorData actor, UIOverconData.NameToOverconEntry entry)
	{
		m_initialized = true;
		if (entry != null)
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					if (m_foregroundImg != null && !string.IsNullOrEmpty(entry.m_staticSpritePath))
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
						Sprite sprite = Resources.Load(entry.m_staticSpritePath, typeof(Sprite)) as Sprite;
						if (sprite != null)
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
							m_foregroundImg.sprite = sprite;
							Color color = m_foregroundImg.color;
							color.a = entry.m_initialAlpha;
							m_foregroundImg.color = color;
						}
						else if (Application.isEditor)
						{
							Debug.LogWarning("Did not find overcon sprite at: " + entry.m_staticSpritePath);
						}
					}
					if (!string.IsNullOrEmpty(entry.m_customPrefabPath))
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
						if (m_customPrefabParent != null)
						{
							GameObject gameObject = Resources.Load(entry.m_customPrefabPath, typeof(GameObject)) as GameObject;
							if (gameObject != null)
							{
								GameObject gameObject2 = Object.Instantiate(gameObject);
								if (gameObject2 != null)
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
									gameObject2.transform.SetParent(m_customPrefabParent.transform);
									gameObject2.transform.localPosition = new Vector3(0f, entry.m_customPrefabHeightOffset, 0f);
								}
							}
							else if (Application.isEditor)
							{
								Debug.LogWarning("Did not find overcon prefab at: " + entry.m_customPrefabPath);
							}
						}
					}
					m_timeToDestroy = Time.time + ((!(entry.m_ageInSeconds <= 0f)) ? entry.m_ageInSeconds : 8f);
					return;
				}
			}
		}
		m_timeToDestroy = Time.time;
	}

	public void SetCanvasGroupVisibility(bool visible)
	{
		if (!(m_canvasGroup != null))
		{
			return;
		}
		while (true)
		{
			switch (6)
			{
			case 0:
				continue;
			}
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (visible)
			{
				while (true)
				{
					switch (2)
					{
					case 0:
						break;
					default:
						m_canvasGroup.alpha = 1f;
						m_canvasGroup.blocksRaycasts = true;
						m_canvasGroup.interactable = true;
						return;
					}
				}
			}
			m_canvasGroup.alpha = 0f;
			m_canvasGroup.blocksRaycasts = false;
			m_canvasGroup.interactable = false;
			return;
		}
	}

	private void OnDestroy()
	{
		if (!(m_customPrefabParent != null))
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			Object.Destroy(m_customPrefabParent);
			return;
		}
	}

	private void Update()
	{
		if (!m_initialized)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (Time.time >= m_timeToDestroy)
			{
				while (true)
				{
					switch (4)
					{
					case 0:
						continue;
					}
					Object.Destroy(base.gameObject);
					return;
				}
			}
			return;
		}
	}
}
