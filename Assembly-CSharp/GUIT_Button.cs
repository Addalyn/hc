using UnityEngine;

[RequireComponent(typeof(GUITexture))]
public class GUIT_Button : MonoBehaviour
{
	public Color labelColor;

	public Texture t_on;

	public Texture t_off;

	public Texture t_on_over;

	public Texture t_off_over;

	public GameObject callbackObject;

	public string callback;

	private bool over;

	public bool on;

	private void Awake()
	{
		GetComponentInChildren<GUIText>().material.color = labelColor;
		UpdateImage();
	}

	private void Update()
	{
		if (GetComponent<GUITexture>().GetScreenRect().Contains(Input.mousePosition))
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
					if (!over)
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
						OnOver();
					}
					if (Input.GetMouseButtonDown(0))
					{
						OnClick();
					}
					return;
				}
			}
		}
		if (!over)
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
			OnOut();
			return;
		}
	}

	private void OnClick()
	{
		on = !on;
		callbackObject.SendMessage(callback);
		UpdateImage();
	}

	private void OnOver()
	{
		over = true;
		UpdateImage();
	}

	private void OnOut()
	{
		over = false;
		UpdateImage();
	}

	private void UpdateImage()
	{
		if (over)
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
					GUITexture component = GetComponent<GUITexture>();
					Texture texture;
					if (on)
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
						texture = t_on_over;
					}
					else
					{
						texture = t_off_over;
					}
					component.texture = texture;
					return;
				}
				}
			}
		}
		GUITexture component2 = GetComponent<GUITexture>();
		Texture texture2;
		if (on)
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
			texture2 = t_on;
		}
		else
		{
			texture2 = t_off;
		}
		component2.texture = texture2;
	}

	public void UpdateState(bool b)
	{
		on = b;
		UpdateImage();
	}
}
