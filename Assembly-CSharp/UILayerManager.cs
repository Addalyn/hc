using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UILayerManager
{
	public CanvasLayerInfo[] CanvasLayers;

	private GameObject LayersContainer;

	[NonSerialized]
	private UICameraLayerInfo m_parentInfo;

	private bool init;

	public UICameraLayerInfo ParentInfo => m_parentInfo;

	public int ObjectLayerValue
	{
		get
		{
			int num = 0;
			if (m_parentInfo.CamType == RenderMode.WorldSpace)
			{
				return 12;
			}
			return 5;
		}
	}

	public int SetSceneVisible(IEnumerable<SceneType> aScenes, bool visible, SceneVisibilityParameters parameters)
	{
		int num = 0;
		for (int i = 0; i < CanvasLayers.Length; i++)
		{
			num += CanvasLayers[i].SetSceneVisible(aScenes, visible, parameters);
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
			return num;
		}
	}

	public void Init(UICameraLayerInfo info)
	{
		if (init)
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
			init = true;
			m_parentInfo = info;
			LayersContainer = new GameObject();
			LayersContainer.name = "Layer Container";
			UIManager.ReparentTransform(LayersContainer.transform, m_parentInfo.CameraLayerContainer.gameObject.transform);
			List<KeyValuePair<int, int>> list = new List<KeyValuePair<int, int>>();
			for (int i = 0; i < CanvasLayers.Length; i++)
			{
				CanvasLayers[i].ScenesContainer = new GameObject();
				CanvasLayers[i].ScenesContainer.name = "(Canvas Container)" + CanvasLayers[i].CanvasLayerName;
				UIManager.ReparentTransform(CanvasLayers[i].ScenesContainer.transform, LayersContainer.gameObject.transform);
				CanvasLayers[i].Init(this);
				KeyValuePair<int, int> item = new KeyValuePair<int, int>(i, CanvasLayers[i].LayerPriority);
				list.Add(item);
			}
			while (true)
			{
				switch (3)
				{
				case 0:
					continue;
				}
				if (list.Count > 1)
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
					if (_003C_003Ef__am_0024cache0 == null)
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
						_003C_003Ef__am_0024cache0 = delegate(KeyValuePair<int, int> keyA, KeyValuePair<int, int> keyB)
						{
							if (keyA.Value > keyB.Value)
							{
								return 1;
							}
							if (keyA.Value < keyB.Value)
							{
								while (true)
								{
									switch (6)
									{
									case 0:
										break;
									default:
										if (1 == 0)
										{
											/*OpCode not supported: LdMemberToken*/;
										}
										return -1;
									}
								}
							}
							return 0;
						};
					}
					list.Sort(_003C_003Ef__am_0024cache0);
					for (int j = 0; j < list.Count; j++)
					{
						CanvasLayers[list[j].Key].ScenesContainer.transform.SetAsLastSibling();
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
				LayersContainer.SetLayerRecursively(ObjectLayerValue);
				return;
			}
		}
	}

	public Canvas GetBatchCanvas(IUIScene theScene, CanvasBatchType type)
	{
		Canvas canvas = null;
		int num = 0;
		while (true)
		{
			if (num < CanvasLayers.Length)
			{
				canvas = CanvasLayers[num].GetBatchCanvas(theScene, type);
				if (canvas != null)
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
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					break;
				}
				num++;
				continue;
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
			break;
		}
		return canvas;
	}

	public Canvas GetDefaultCanvas(IUIScene theScene)
	{
		Canvas canvas = null;
		int num = 0;
		while (true)
		{
			if (num < CanvasLayers.Length)
			{
				canvas = CanvasLayers[num].GetDefaultCanvas(theScene);
				if (canvas != null)
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
					break;
				}
				num++;
				continue;
			}
			while (true)
			{
				switch (4)
				{
				case 0:
					continue;
				}
				break;
			}
			break;
		}
		return canvas;
	}

	public Canvas GetDefaultCanvas(SceneType theScene)
	{
		Canvas canvas = null;
		int num = 0;
		while (true)
		{
			if (num < CanvasLayers.Length)
			{
				canvas = CanvasLayers[num].GetDefaultCanvas(theScene);
				if (canvas != null)
				{
					break;
				}
				num++;
				continue;
			}
			while (true)
			{
				switch (4)
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
			break;
		}
		return canvas;
	}

	public int GetNameplateCanvasLayer()
	{
		int num = -1;
		for (int i = 0; i < CanvasLayers.Length; i++)
		{
			num = CanvasLayers[i].GetNameplateCanvasLayer();
			if (num != -1)
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
				break;
			}
		}
		return num;
	}

	public RuntimeSceneInfo RegisterUIScene(IUIScene scene)
	{
		for (int i = 0; i < CanvasLayers.Length; i++)
		{
			RuntimeSceneInfo runtimeSceneInfo = CanvasLayers[i].RegisterUIScene(scene);
			if (runtimeSceneInfo == null)
			{
				continue;
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
				runtimeSceneInfo.RuntimeSceneContainer.SetLayerRecursively(ObjectLayerValue);
				return runtimeSceneInfo;
			}
		}
		return null;
	}

	public List<UISceneDisplayInfo> SetGameState(UIManager.ClientState newState)
	{
		List<UISceneDisplayInfo> list = new List<UISceneDisplayInfo>();
		for (int i = 0; i < CanvasLayers.Length; i++)
		{
			list.AddRange(CanvasLayers[i].SetGameState(newState));
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
			return list;
		}
	}
}
