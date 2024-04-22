using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class PrefabResourceLink
{
	private static Dictionary<string, SavedResourceLink> s_loadedResourceLinks = new Dictionary<string, SavedResourceLink>();
	[SerializeField]
	private string m_resourcePath;
	[SerializeField]
	private string m_GUID;
	[SerializeField]
	private string m_debugPrefabPath;

	public string GUID => m_GUID;
	internal string ResourcePath => m_resourcePath;
	public bool IsEmpty => string.IsNullOrEmpty(m_resourcePath);

	public void SetValues(string resourcePath, string GUID, string prefabPath)
	{
		if (Application.isEditor)
		{
			m_resourcePath = resourcePath;
			m_GUID = GUID;
			m_debugPrefabPath = prefabPath;
		}
	}

	public GameObject GetPrefab(bool returnNullOnLoadFail = false)
	{
		SavedResourceLink savedResourceLink = null;
		if (string.IsNullOrEmpty(m_resourcePath))
		{
			Log.Warning("Attempted to get prefab for NULL or empty resource path, ignoring.");
		}
		else if (!s_loadedResourceLinks.TryGetValue(m_resourcePath, out savedResourceLink))
		{
			GameObject gameObject = Resources.Load(m_resourcePath) as GameObject;
			if (gameObject == null)
			{
				if (returnNullOnLoadFail)
				{
					if (Application.isEditor)
					{
						Log.Error("Failed to load Resource Link prefab from " + m_resourcePath);
					}
					return null;
				}
				throw new ApplicationException("Failed to load Resource Link prefab from " + m_resourcePath);
			}

			savedResourceLink = AddLoadedLink(gameObject);
		}

		return savedResourceLink != null
			? savedResourceLink.prefabReference
			: null;
	}

	public IEnumerator PreLoadPrefabAsync()
	{
		if (string.IsNullOrEmpty(m_resourcePath))
		{
			Log.Warning("Attempted to load NULL or empty resource path, ignoring.");
		}
		else if (!s_loadedResourceLinks.TryGetValue(m_resourcePath, out _))
		{
			ResourceRequest request = Resources.LoadAsync(m_resourcePath);
			yield return request;
			if (request == null || request.asset == null)
			{
				Log.Error("Prefab load failed for {0}", m_resourcePath);
			}
			else
			{
				AddLoadedLink(request.asset as GameObject);
			}
		}
	}

	internal void UnloadPrefab()
	{
		if (s_loadedResourceLinks.TryGetValue(m_resourcePath, out SavedResourceLink savedResourceLink)
		    && savedResourceLink != null
		    && savedResourceLink.prefabReference != null)
		{
			s_loadedResourceLinks.Remove(m_resourcePath);
		}
	}

	internal static bool HasLoadedResourceLinkForPath(string resourcePath)
	{
		return s_loadedResourceLinks != null
		       && s_loadedResourceLinks.ContainsKey(resourcePath);
	}

	private SavedResourceLink AddLoadedLink(GameObject loadedLinkObject)
	{
		if (loadedLinkObject == null)
		{
			Log.Error("Could not load saved Resource Link from: " + m_resourcePath);
			return null;
		}
		SavedResourceLink component = loadedLinkObject.GetComponent<SavedResourceLink>();
		if (component == null)
		{
			Log.Error("Could not load saved Resource Link at [" + m_resourcePath + "] does not have a SavedResourceLink component");
			return null;
		}
		if (component.prefabReference == null)
		{
			Log.Error(
				$"Resource Link at [{m_resourcePath}] has a null prefab reference.  " +
				$"This can happen if the referenced prefab was deleted.  " +
				$"The original path was [{m_debugPrefabPath}]");
			return null;
		}
		if (s_loadedResourceLinks.ContainsKey(m_resourcePath))
		{
			Log.Error("Prefab resource link already contains a loaded entry for path - replacing it: " + m_resourcePath);
			s_loadedResourceLinks[m_resourcePath] = component;
		}
		else
		{
			s_loadedResourceLinks.Add(m_resourcePath, component);
		}
		return component;
	}

	public GameObject InstantiatePrefab(bool returnNullOnNullPrefab = false)
	{
		GameObject prefab = GetPrefab(returnNullOnNullPrefab);
		if (prefab == null)
		{
			return null;
		}
		return Object.Instantiate(prefab);
	}

	internal static void Stream(IBitStream stream, ref PrefabResourceLink link)
	{
		if (link == null)
		{
			link = new PrefabResourceLink();
		}
		stream.Serialize(ref link.m_resourcePath);
		stream.Serialize(ref link.m_GUID);
	}

	internal static void UnloadAll()
	{
		s_loadedResourceLinks.Clear();
	}

	public override string ToString()
	{
		return Application.isEditor && !string.IsNullOrEmpty(m_debugPrefabPath)
			? m_debugPrefabPath
			: $"GUID: {m_GUID}";
	}

	public string GetResourcePath()
	{
		return m_resourcePath;
	}
}
