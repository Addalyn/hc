// ROGUES
// SERVER
using System.Collections.Generic;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class ActorCover : NetworkBehaviour
{
	// reactor
	private bool[] m_hasCover = new bool[4];
	// rogues
	// private ThinCover.CoverType[] m_hasCover = new ThinCover.CoverType[4];
	
	private bool[] m_cachedHasCoverFromBarriers = new bool[4];
	private SyncListTempCoverInfo m_syncTempCoverProviders = new SyncListTempCoverInfo();
	private List<CoverDirections> m_tempCoverProviders = new List<CoverDirections>();
	private List<CoverDirections> m_tempCoverIgnoreMinDist = new List<CoverDirections>();
	
	// added in rogues
	// private List<CoverDirections> m_adjacentCoverProviders = new List<CoverDirections>();
	
	private GameObject m_coverParent;
	private ActorData m_owner;
	
	// reactor
	private GameObject[] m_mouseOverCoverObjs = new GameObject[4];
	private GameObject[] m_actorCoverObjs = new GameObject[4];
	private List<ParticleSystemRenderer[]> m_actorCoverSymbolRenderers = new List<ParticleSystemRenderer[]>();
	// rogues
	// private GameObject[,] m_mouseOverCoverObjs = new GameObject[3, 4];
	// private GameObject[,] m_actorCoverObjs = new GameObject[3, 4];
	// private List<ParticleSystemRenderer[]>[] m_actorCoverSymbolRenderers = {
	// 	new List<ParticleSystemRenderer[]>(),
	// 	new List<ParticleSystemRenderer[]>(),
	// 	new List<ParticleSystemRenderer[]>()
	// };

	private static Vector3[] m_coverDir = new Vector3[4];
	private float m_coverHeight = 2f;
	private float m_coverDirIndicatorHideTime = -1f;
	private float m_coverDirIndicatorFadeStartTime = -1f;
	private float m_coverDirIndicatorSpawnTime = -1f;
	private GameObject m_coverDirHighlight;
	private MeshRenderer[] m_coverDirIndicatorRenderers;
	private EasedFloatCubic m_coverDirIndicatorOpacity = new EasedFloatCubic(1f);
	
	// removed in rogues
	private static int kListm_syncTempCoverProviders = 0x55B6FA50;
	
	// reactor
	static ActorCover()
	{
		RegisterSyncListDelegate(typeof(ActorCover), kListm_syncTempCoverProviders, InvokeSyncListm_syncTempCoverProviders);
		NetworkCRC.RegisterBehaviour("ActorCover", 0);
	}
	// rogues
	// public ActorCover()
	// {
	// 	base.InitSyncObject(m_syncTempCoverProviders);
	// }

	public bool HasAnyCover(bool recalculate = false)
	{
		if (recalculate)
		{
			RecalculateCover();
		}
		bool result = false;
		foreach (bool hasCover in m_hasCover)
		{
			if (hasCover)  // hasCover != ThinCover.CoverType.None in rogues
			{
				result = true;
			}
		}
		return result;
	}

	// changed in rogues (to accomodate more cover types)
	private void Awake()
	{
		m_coverParent = GameObject.Find("CoverParent");
		if (!m_coverParent)
		{
			m_coverParent = new GameObject("CoverParent");
		}
		for (int i = 0; i < 4; i++)
		{
			m_hasCover[i] = false;
			m_cachedHasCoverFromBarriers[i] = false;
		}
		InitCoverObjs(m_mouseOverCoverObjs, HighlightUtils.Get().m_coverIndicatorPrefab);
		InitCoverObjs(m_actorCoverObjs, HighlightUtils.Get().m_coverShieldOnlyPrefab);
		foreach (GameObject coverObj in m_actorCoverObjs)
		{
			m_actorCoverSymbolRenderers.Add(coverObj != null
				? coverObj.GetComponentsInChildren<ParticleSystemRenderer>()
				: new ParticleSystemRenderer[0]);
		}
		m_coverDir[(int)CoverDirections.X_NEG] = Vector3.left; 
		m_coverDir[(int)CoverDirections.X_POS] = Vector3.right; 
		m_coverDir[(int)CoverDirections.Y_NEG] = Vector3.back;
		m_coverDir[(int)CoverDirections.Y_POS] = Vector3.forward;
		m_owner = GetComponent<ActorData>();
		m_syncTempCoverProviders.InitializeBehaviour(this, kListm_syncTempCoverProviders);
	}

	public override void OnStartClient()
	{
		m_syncTempCoverProviders.Callback = SyncListCallbackTempCoverProviders;
	}

	private void SyncListCallbackTempCoverProviders(SyncList<TempCoverInfo>.Operation op, int index)
	{
		ResetTempCoverListFromSyncList();
	}

	// changed in rogues (to accomodate more cover types)
	private void InitCoverObjs(GameObject[] coverObjs, GameObject coverPrefab)
	{
		coverObjs[0] = CreateCoverIndicatorObject(-90f, coverPrefab);
		coverObjs[1] = CreateCoverIndicatorObject(90f, coverPrefab);
		coverObjs[2] = CreateCoverIndicatorObject(180f, coverPrefab);
		coverObjs[3] = CreateCoverIndicatorObject(0f, coverPrefab);
		coverObjs[0].SetActive(false);
		coverObjs[1].SetActive(false);
		coverObjs[2].SetActive(false);
		coverObjs[3].SetActive(false);
		coverObjs[0].transform.parent = m_coverParent.transform;
		coverObjs[1].transform.parent = m_coverParent.transform;
		coverObjs[2].transform.parent = m_coverParent.transform;
		coverObjs[3].transform.parent = m_coverParent.transform;
	}

	public static void ResetParticleTime(GameObject particleObject)
	{
		foreach (ParticleSystem particleSystem in particleObject.GetComponentsInChildren<ParticleSystem>())
		{
			particleSystem.Clear();
			particleSystem.time = 0f;
		}
	}

	private GameObject CreateCoverIndicatorObject(float yRotation, GameObject coverPrefab)
	{
		GameObject coverObj = Instantiate(coverPrefab);
		coverObj.transform.Rotate(Vector3.up, yRotation);
		return coverObj;
	}

	private void SetCoverMeshColor(GameObject particleObject, Color color)
	{
		if (particleObject == null)
		{
			return;
		}
		ParticleSystemRenderer[] particleSystemRenderers = particleObject.GetComponentsInChildren<ParticleSystemRenderer>();
		foreach (ParticleSystemRenderer particleSystemRenderer in particleSystemRenderers)
		{
			AbilityUtil_Targeter.SetMaterialColor(particleSystemRenderer.materials, color);
		}
	}

	public void DisableCover()
	{
		for (int i = 0; i < 4; i++)
		{
			m_hasCover[i] = false; // ThinCover.CoverType.None in rogues
			m_cachedHasCoverFromBarriers[i] = false;
		}
	}

	public Vector3 GetCoverOffset(CoverDirections dir)
	{
		return GetCoverOffsetStatic(dir);
	}

	public static Vector3 GetCoverOffsetStatic(CoverDirections dir)
	{
		float length = Board.Get().squareSize * 0.5f;
		Vector3 offset = Vector3.zero;
		if (dir == CoverDirections.X_POS)
		{
			offset = new Vector3(length, 0f, 0f);
		}
		else if (dir == CoverDirections.X_NEG)
		{
			offset = new Vector3(-length, 0f, 0f);
		}
		else if (dir == CoverDirections.Y_POS)
		{
			offset = new Vector3(0f, 0f, length);
		}
		else if (dir == CoverDirections.Y_NEG)
		{
			offset = new Vector3(0f, 0f, -length);
		}
		return offset;
	}

	public static Quaternion GetCoverRotation(CoverDirections dir)
	{
		switch (dir)
		{
			case CoverDirections.X_POS:
				return Quaternion.LookRotation(Vector3.left);
			case CoverDirections.X_NEG:
				return Quaternion.LookRotation(Vector3.right);
			case CoverDirections.Y_POS:
				return Quaternion.LookRotation(Vector3.back);
			default:
				return Quaternion.LookRotation(Vector3.forward);
		}
	}
	
#if SERVER
	// added in rogues
	public bool HasNonThinCover(BoardSquare currentSquare, int xDelta, int yDelta)
	{
		BoardSquare boardSquare = Board.Get().GetSquareFromIndex(currentSquare.x + xDelta, currentSquare.y + yDelta);
		if (boardSquare == null)
		{
			return false;
		}
		return boardSquare.height - currentSquare.height >= 1;
	}
#endif
	
	public bool HasNonThinCover(BoardSquare currentSquare, int xDelta, int yDelta, bool halfHeight)
	{
		BoardSquare boardSquare = Board.Get().GetSquareFromIndex(currentSquare.x + xDelta, currentSquare.y + yDelta);
		if (boardSquare == null)
		{
			return false;
		}
		int coverHeight = boardSquare.height - currentSquare.height;
		return halfHeight ? coverHeight == 1 : coverHeight == 2;
	}

	// tweaked in rogues
	public float CoverRating(BoardSquare square)
	{
		// TODO BOTS enemies can be too far 
		List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(m_owner.GetEnemyTeam());
		float num = 0f;
		foreach (ActorData actorData in allTeamMembers)
		{
			if (actorData.IsDead() || actorData.GetCurrentBoardSquare() == null)
			{
				continue;
			}
			Vector3 vector = actorData.GetCurrentBoardSquare().transform.position - square.transform.position;
			if (vector.magnitude <= Board.Get().squareSize * 1.5f)
			{
				continue;
			}
			if (Mathf.Abs(vector.x) > Mathf.Abs(vector.z))
			{
				if (vector.x < 0f
				    && (HasNonThinCover(square, -1, 0, true)
				        || square.GetThinCover(CoverDirections.X_NEG) == ThinCover.CoverType.Half)
				    || vector.x > 0f
				    && (HasNonThinCover(square, 1, 0, true)
				        || square.GetThinCover(CoverDirections.X_POS) == ThinCover.CoverType.Half))
				{
					num += 1f;
				}
				else if (vector.x < 0f
				    && (HasNonThinCover(square, -1, 0, false)
				        || square.GetThinCover(CoverDirections.X_NEG) == ThinCover.CoverType.Full)
				    || vector.x > 0f
				    && (HasNonThinCover(square, 1, 0, false)
				        || square.GetThinCover(CoverDirections.X_POS) == ThinCover.CoverType.Full))
				{
					num += 0.5f;
				}
			}
			else
			{
				if (vector.z < 0f
				    && (HasNonThinCover(square, 0, -1, true)
				        || square.GetThinCover(CoverDirections.Y_NEG) == ThinCover.CoverType.Half)
				    || vector.z > 0f
				    && (HasNonThinCover(square, 0, 1, true)
				        || square.GetThinCover(CoverDirections.Y_POS) == ThinCover.CoverType.Half))
				{
					num += 1f;
				}
				else if ((vector.z < 0f
				     && (HasNonThinCover(square, 0, -1, false)
				         || square.GetThinCover(CoverDirections.Y_NEG) == ThinCover.CoverType.Full))
				    || vector.z > 0f
				    && (HasNonThinCover(square, 0, 1, false)
				        || square.GetThinCover(CoverDirections.Y_POS) == ThinCover.CoverType.Full))
				{
					num += 0.5f;
				}
			}
		}
		return num;
	}
	
#if SERVER
	// custom
	public float CoverRatingWithTempCover(BoardSquare square, CoverDirections tempCoverDirection, bool tempCoverIgnoreMinDist)
	{
		List<ActorData> allTeamMembers = GameFlowData.Get().GetAllTeamMembers(m_owner.GetEnemyTeam());
		float num = 0f;
		foreach (ActorData actorData in allTeamMembers)
		{
			if (actorData.IsDead() || actorData.GetCurrentBoardSquare() == null)
			{
				continue;
			}
			Vector3 vector = actorData.GetCurrentBoardSquare().transform.position - square.transform.position;
			if (vector.magnitude <= Board.Get().squareSize * 1.5f)
			{
				if (!tempCoverIgnoreMinDist)
				{
					continue;
				}
				bool isCoveredByTempCover = Mathf.Abs(vector.x) > Mathf.Abs(vector.z)
					? vector.x < 0f && tempCoverDirection == CoverDirections.X_NEG
					  || vector.x > 0f && tempCoverDirection == CoverDirections.X_POS
					: vector.z < 0f && tempCoverDirection == CoverDirections.Y_NEG
					  || vector.x > 0f && tempCoverDirection == CoverDirections.Y_POS;
				if (!isCoveredByTempCover)
				{
					continue;
				}
			}
			if (Mathf.Abs(vector.x) > Mathf.Abs(vector.z))
			{
				if (vector.x < 0f
				    && (HasNonThinCover(square, -1, 0, true)
				        || square.GetThinCover(CoverDirections.X_NEG) == ThinCover.CoverType.Half
				        || tempCoverDirection == CoverDirections.X_NEG)
				    || vector.x > 0f
				    && (HasNonThinCover(square, 1, 0, true)
				        || square.GetThinCover(CoverDirections.X_POS) == ThinCover.CoverType.Half
				        || tempCoverDirection == CoverDirections.X_POS))
				{
					num += 1f;
				}
				else if (vector.x < 0f
				    && (HasNonThinCover(square, -1, 0, false)
				        || square.GetThinCover(CoverDirections.X_NEG) == ThinCover.CoverType.Full)
				    || vector.x > 0f
				    && (HasNonThinCover(square, 1, 0, false)
				        || square.GetThinCover(CoverDirections.X_POS) == ThinCover.CoverType.Full))
				{
					num += 0.5f;
				}
			}
			else
			{
				if (vector.z < 0f
				    && (HasNonThinCover(square, 0, -1, true)
				        || square.GetThinCover(CoverDirections.Y_NEG) == ThinCover.CoverType.Half
				        || tempCoverDirection == CoverDirections.Y_NEG)
				    || vector.z > 0f
				    && (HasNonThinCover(square, 0, 1, true)
				        || square.GetThinCover(CoverDirections.Y_POS) == ThinCover.CoverType.Half
				        || tempCoverDirection == CoverDirections.Y_POS))
				{
					num += 1f;
				}
				else if ((vector.z < 0f
				     && (HasNonThinCover(square, 0, -1, false)
				         || square.GetThinCover(CoverDirections.Y_NEG) == ThinCover.CoverType.Full))
				    || vector.z > 0f
				    && (HasNonThinCover(square, 0, 1, false)
				        || square.GetThinCover(CoverDirections.Y_POS) == ThinCover.CoverType.Full))
				{
					num += 0.5f;
				}
			}
		}
		return num;
	}
	
	// added in rogues
	public int AmountOfCover(BoardSquare square)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			CoverDirections coverDirections = (CoverDirections)i;
			if (square.GetThinCover(coverDirections) != ThinCover.CoverType.None)
			{
				num++;
			}
			else if ((coverDirections == CoverDirections.X_NEG && HasNonThinCover(square, -1, 0)) || (coverDirections == CoverDirections.X_POS && HasNonThinCover(square, 1, 0)) || (coverDirections == CoverDirections.Y_NEG && HasNonThinCover(square, 0, -1)) || (coverDirections == CoverDirections.Y_POS && HasNonThinCover(square, 0, 1)))
			{
				num++;
			}
		}
		return num;
	}
#endif
	
	// changed in rogues (to accomodate more cover types)
	internal void UpdateCoverHighlights(BoardSquare currentSquare)
	{
		ActorData owner = m_owner;
		if (currentSquare == null || !currentSquare.IsValidForGameplay())
		{
			foreach (GameObject mouseOverCoverObj in m_mouseOverCoverObjs)
			{
				if (mouseOverCoverObj)
				{
					mouseOverCoverObj.SetActive(false);
				}
			}

			return;
		}
		
		ActorTurnSM actorTurnSM = owner.GetActorTurnSM();
		if (actorTurnSM == null)
		{
			return;
		}
		
		bool amTargetingAction = actorTurnSM.AmTargetingAction();
		List<BoardSquare> cardinalAdjacentSquares = null;
		Board.Get().GetCardinalAdjacentSquares(currentSquare.x, currentSquare.y, ref cardinalAdjacentSquares);
		if (cardinalAdjacentSquares != null)
		{
			foreach (BoardSquare square in cardinalAdjacentSquares)
			{
				if (square == null) continue;
				CoverDirections coverDirection = GetCoverDirection(currentSquare, square);
				int elevation = square.height - currentSquare.height;
				GameObject mouseOverCoverObj = coverDirection < (CoverDirections)m_mouseOverCoverObjs.Length
					? m_mouseOverCoverObjs[(int)coverDirection]
					: null;
				if (mouseOverCoverObj == null) continue;
				if ((elevation >= 1 || currentSquare.GetThinCover(coverDirection) != ThinCover.CoverType.None)
				    && !amTargetingAction && actorTurnSM.CurrentState != TurnStateEnum.PICKING_RESPAWN)
				{
					Vector3 pos = new Vector3(currentSquare.worldX, currentSquare.height + m_coverHeight, currentSquare.worldY);
					pos += GetCoverOffset(coverDirection);
					if (mouseOverCoverObj.transform.position != pos || !mouseOverCoverObj.activeSelf)
					{
						mouseOverCoverObj.transform.position = pos;
						mouseOverCoverObj.SetActive(true);
						ResetParticleTime(mouseOverCoverObj);
					}
				}
				else
				{
					mouseOverCoverObj.SetActive(false);
				}
			}
		}
	}

	// changed in rogues (to accomodate more cover types)
	private void Update()
	{
		if (m_coverDirHighlight != null && m_coverDirIndicatorRenderers != null)
		{
			float indicatorOpacity = m_coverDirIndicatorOpacity * GetCoverDirInitialOpacity();
			foreach (MeshRenderer meshRenderer in m_coverDirIndicatorRenderers)
			{
				if (meshRenderer != null)
				{
					AbilityUtil_Targeter.SetMaterialOpacity(meshRenderer.materials, indicatorOpacity);
				}
			}
			float particleOpacity = m_coverDirIndicatorOpacity * GetCoverDirParticleInitialOpacity();
			for (int j = 0; j < m_hasCover.Length; j++)
			{
				if (j >= m_actorCoverSymbolRenderers.Count)
				{
					break;
				}
				if (m_hasCover[j])
				{
					foreach (ParticleSystemRenderer particleSystemRenderer in m_actorCoverSymbolRenderers[j])
					{
						AbilityUtil_Targeter.SetMaterialOpacity(particleSystemRenderer.materials, particleOpacity);
					}
				}
			}
		}
		if (m_coverDirIndicatorSpawnTime > 0f && Time.time > m_coverDirIndicatorSpawnTime)
		{
			ShowAllRelevantCoverIndicator();
			m_coverDirIndicatorSpawnTime = -1f;
		}
		if (m_coverDirIndicatorFadeStartTime > 0f && Time.time > m_coverDirIndicatorFadeStartTime)
		{
			m_coverDirIndicatorOpacity.EaseTo(0f, GetCoverDirIndicatorDuration() - GetCoverDirFadeoutStartDelay());
			m_coverDirIndicatorFadeStartTime = -1f;
		}
		if (m_coverDirIndicatorHideTime > 0f && Time.time > m_coverDirIndicatorHideTime)
		{
			HideRelevantCover();
			DestroyCoverDirHighlight();
			m_coverDirIndicatorHideTime = -1f;
		}
	}

	// changed in rogues (to accomodate more cover types)
	public void ShowRelevantCover(Vector3 damageOrigin)
	{
		List<CoverDirections> coverDirectionsList = new List<CoverDirections>();
		if (IsInCoverWrt(damageOrigin, ref coverDirectionsList))
		{
			BoardSquare currentBoardSquare = m_owner.GetCurrentBoardSquare();
			for (int i = 0; i < 4; i++)
			{
				CoverDirections coverDirections = (CoverDirections)i;
				if (coverDirectionsList.Contains(coverDirections))
				{
					Vector3 a = new Vector3(currentBoardSquare.worldX, currentBoardSquare.height + m_coverHeight, currentBoardSquare.worldY);
					m_actorCoverObjs[i].transform.position = a + GetCoverOffset(coverDirections);
					m_actorCoverObjs[i].SetActive(true);
				}
				else
				{
					m_actorCoverObjs[i].SetActive(false);
				}
			}
		}
		else
		{
			HideRelevantCover();
		}
	}

	public void StartShowMoveIntoCoverIndicator()
	{
		if (HasAnyCover())
		{
			m_coverDirIndicatorSpawnTime = Time.time + GetCoverDirIndicatorSpawnDelay();
		}
		else
		{
			m_coverDirIndicatorSpawnTime = -1f;
		}
	}

	// changed in rogues (to accomodate more cover types)
	public static GameObject CreateCoverDirIndicator(bool[] hasCoverFlags, Color color, float radiusInSquares)
	{
		float coverProtectionAngle = GameplayData.Get() != null ? GameplayData.Get().m_coverProtectionAngle : 110f;
		float coverProtectionAngleOffset = coverProtectionAngle - 90f;
		GameObject gameObject = new GameObject("CoverDirHighlightParent");
		int numCovers = 0;
		bool flag = false;
		foreach (bool hasCover in hasCoverFlags)
		{
			if (hasCover)
			{
				numCovers++;
			}
		}
		if (numCovers == 2)
		{
			flag = hasCoverFlags[1] == hasCoverFlags[0];
		}
		float borderStartOffset = 0.7f;
		GameObject indicatorMesh = HighlightUtils.Get().CreateDynamicConeMesh(radiusInSquares, coverProtectionAngle, false);
		HighlightUtils.Get().SetDynamicConeMeshBorderActive(indicatorMesh, false);
		UIDynamicCone component = indicatorMesh.GetComponent<UIDynamicCone>();
		if (component != null)
		{
			component.SetBorderStartOffset(borderStartOffset);
		}
		Vector3 forward = Vector3.forward;
		if (numCovers <= 3)
		{
			if (!flag)
			{
				Vector3 a = Vector3.zero;
				for (int j = 0; j < m_coverDir.Length; j++)
				{
					if (hasCoverFlags[j])
					{
						a += m_coverDir[j];
					}
				}
				forward = (a / numCovers).normalized;
			}
		}
		if (numCovers == 2 && flag)
		{
			GameObject gameObject3 = HighlightUtils.Get().CreateDynamicConeMesh(radiusInSquares, coverProtectionAngle, false);
			HighlightUtils.Get().SetDynamicConeMeshBorderActive(gameObject3, false);
			UIDynamicCone component2 = gameObject3.GetComponent<UIDynamicCone>();
			if (component2 != null)
			{
				component2.SetBorderStartOffset(borderStartOffset);
			}

			foreach (MeshRenderer meshRenderer in gameObject3.GetComponentsInChildren<MeshRenderer>())
			{
				if (HighlightUtils.Get() != null)
				{
					AbilityUtil_Targeter.SetMaterialColor(meshRenderer.materials, color);
				}
				AbilityUtil_Targeter.SetMaterialOpacity(meshRenderer.materials, GetCoverDirInitialOpacity());
			}
			if (hasCoverFlags[1])
			{
				forward = Vector3.right;
				gameObject3.transform.localRotation = Quaternion.LookRotation(Vector3.left);
			}
			else
			{
				forward = Vector3.forward;
				gameObject3.transform.localRotation = Quaternion.LookRotation(Vector3.back);
			}
			gameObject3.transform.parent = gameObject.transform;
			gameObject3.transform.localPosition = Vector3.zero;
		}
		else if (numCovers == 2)
		{
			HighlightUtils.Get().AdjustDynamicConeMesh(indicatorMesh, radiusInSquares, 180f + coverProtectionAngleOffset);
		}
		else if (numCovers == 3)
		{
			HighlightUtils.Get().AdjustDynamicConeMesh(indicatorMesh, radiusInSquares, 270f + coverProtectionAngleOffset);
		}
		else if (numCovers == 4)
		{
			HighlightUtils.Get().AdjustDynamicConeMesh(indicatorMesh, radiusInSquares, 360f);
		}
		
		indicatorMesh.transform.parent = gameObject.transform;
		indicatorMesh.transform.localRotation = Quaternion.LookRotation(forward);
		indicatorMesh.transform.localPosition = Vector3.zero;
		foreach (MeshRenderer meshRenderer in indicatorMesh.GetComponentsInChildren<MeshRenderer>())
		{
			if (HighlightUtils.Get() != null)
			{
				AbilityUtil_Targeter.SetMaterialColor(meshRenderer.materials, color);
			}
			AbilityUtil_Targeter.SetMaterialOpacity(meshRenderer.materials, GetCoverDirInitialOpacity());
		}
		return gameObject;
	}

	// changed in rogues (to accomodate more cover types)
	private void ShowCoverIndicatorForDirection(CoverDirections dir)
	{
		BoardSquare boardSquare = m_owner != null ? m_owner.GetCurrentBoardSquare() : null;
		if (boardSquare != null
		    && dir < (CoverDirections)m_hasCover.Length
		    && dir < (CoverDirections)m_actorCoverObjs.Length
		    && m_actorCoverObjs[(int)dir] != null)
		{
			if (m_hasCover[(int)dir])
			{
				Vector3 a = new Vector3(boardSquare.worldX, boardSquare.height + m_coverHeight, boardSquare.worldY);
				m_actorCoverObjs[(int)dir].transform.position = a + GetCoverOffset(dir);
				m_actorCoverObjs[(int)dir].SetActive(true);
				ResetParticleTime(m_actorCoverObjs[(int)dir]);
			}
			else
			{
				m_actorCoverObjs[(int)dir].SetActive(false);
			}
		}
	}

	private void ShowAllRelevantCoverIndicator()
	{
		if (!HasAnyCover())
		{
			return;
		}
		ShowCoverIndicatorForDirection(CoverDirections.X_NEG);
		ShowCoverIndicatorForDirection(CoverDirections.X_POS);
		ShowCoverIndicatorForDirection(CoverDirections.Y_NEG);
		ShowCoverIndicatorForDirection(CoverDirections.Y_POS);
		DestroyCoverDirHighlight();
		BoardSquare boardSquare = m_owner != null ? m_owner.GetCurrentBoardSquare() : null;
		if (boardSquare != null)
		{
			Vector3 position = boardSquare.ToVector3();
			position.y = HighlightUtils.GetHighlightHeight();
			m_coverDirHighlight = CreateCoverDirIndicator(m_hasCover, HighlightUtils.Get().m_coverDirIndicatorColor, GetCoverDirIndicatorRadius());
			m_coverDirHighlight.transform.position = position;
			m_coverDirIndicatorRenderers = m_coverDirHighlight.GetComponentsInChildren<MeshRenderer>();
		}
		m_coverDirIndicatorOpacity = new EasedFloatCubic(1f);
		m_coverDirIndicatorOpacity.EaseTo(1f, 0.1f);
		m_coverDirIndicatorHideTime = Time.time + GetCoverDirIndicatorDuration();
		m_coverDirIndicatorFadeStartTime = Time.time + GetCoverDirFadeoutStartDelay();
	}

	// changed in rogues (to accomodate more cover types)
	public void HideRelevantCover()
	{
		for (int i = 0; i < 4; i++)
		{
			m_actorCoverObjs[i].SetActive(false);
		}
	}

	private void DestroyCoverDirHighlight()
	{
		m_coverDirIndicatorRenderers = null;
		if (m_coverDirHighlight != null)
		{
			HighlightUtils.DestroyObjectAndMaterials(m_coverDirHighlight);
		}
	}

	private static float GetCoverDirInitialOpacity()
	{
		if (HighlightUtils.Get() != null)
		{
			return HighlightUtils.Get().m_coverDirIndicatorInitialOpacity;
		}
		return 0.08f;
	}

	private static float GetCoverDirParticleInitialOpacity()
	{
		if (HighlightUtils.Get() != null)
		{
			return HighlightUtils.Get().m_coverDirParticleInitialOpacity;
		}
		return 0.5f;
	}

	private static float GetCoverDirIndicatorDuration()
	{
		if (HighlightUtils.Get() != null)
		{
			return Mathf.Max(0.1f, HighlightUtils.Get().m_coverDirIndicatorDuration);
		}
		return 3f;
	}

	private static float GetCoverDirIndicatorSpawnDelay()
	{
		if (HighlightUtils.Get() != null)
		{
			return Mathf.Max(0f, HighlightUtils.Get().m_coverDirIndicatorStartDelay);
		}
		return 0f;
	}

	private static float GetCoverDirFadeoutStartDelay()
	{
		float b = 1f;
		if (HighlightUtils.Get() != null)
		{
			b = HighlightUtils.Get().m_coverDirFadeoutStartDelay;
		}
		return Mathf.Min(GetCoverDirIndicatorDuration(), b);
	}

	private static float GetCoverDirIndicatorRadius()
	{
		if (HighlightUtils.Get() != null)
		{
			return HighlightUtils.Get().m_coverDirIndicatorRadiusInSquares;
		}
		return 3f;
	}

	public void AddTempCoverProvider(CoverDirections direction, bool ignoreMinDist)
	{
		if (NetworkServer.active)
		{
			TempCoverInfo item = new TempCoverInfo(direction, ignoreMinDist);
			m_syncTempCoverProviders.Add(item);
			ResetTempCoverListFromSyncList();
		}
		RecalculateCover();
	}

	public void RemoveTempCoverProvider(CoverDirections direction, bool ignoreMinDist)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		bool flag = false;
		for (int i = m_syncTempCoverProviders.Count - 1; i >= 0; i--)
		{
			if (m_syncTempCoverProviders[i].m_coverDir == direction
			    && m_syncTempCoverProviders[i].m_ignoreMinDist == ignoreMinDist)
			{
				m_syncTempCoverProviders.RemoveAt(i);
				flag = true;
				break;
			}
		}
		if (flag)
		{
			ResetTempCoverListFromSyncList();
			RecalculateCover();
		}
		else
		{
			Log.Warning("RemoveTempCoverProvider did not find matching entry to remove");
		}
	}

	public void ClearTempCoverProviders()
	{
		if (NetworkServer.active)
		{
			m_syncTempCoverProviders.Clear();
			ResetTempCoverListFromSyncList();
		}
		RecalculateCover();
	}

	public void RecalculateCover()
	{
		ActorData owner = m_owner;
		UpdateCoverFromBarriers();
		// CalculateAdjacentCoverProviders(); // rogues
		BoardSquare currentBoardSquare = owner.GetCurrentBoardSquare();
		CalcCover(out m_hasCover, currentBoardSquare, m_tempCoverProviders, m_tempCoverIgnoreMinDist,  m_cachedHasCoverFromBarriers, true); // m_adjacentCoverProviders, m_cachedHasCoverFromBarriers, true in rogues
	}

	private void ResetTempCoverListFromSyncList()
	{
		m_tempCoverProviders.Clear();
		m_tempCoverIgnoreMinDist.Clear();
		for (int i = 0; i < m_syncTempCoverProviders.Count; i++)
		{
			if (m_syncTempCoverProviders[i].m_ignoreMinDist)
			{
				m_tempCoverIgnoreMinDist.Add(m_syncTempCoverProviders[i].m_coverDir);
			}
			else
			{
				m_tempCoverProviders.Add(m_syncTempCoverProviders[i].m_coverDir);
			}
		}
	}

	public void UpdateCoverFromBarriers()
	{
		for (int i = 0; i < m_cachedHasCoverFromBarriers.Length; i++)
		{
			m_cachedHasCoverFromBarriers[i] = false;
		}
		BoardSquare currentBoardSquare = m_owner.GetCurrentBoardSquare();
		if (BarrierManager.Get() != null && currentBoardSquare != null)
		{
			BarrierManager.Get().UpdateCachedCoverDirections(m_owner, currentBoardSquare, ref m_cachedHasCoverFromBarriers);
		}
	}

	// changed in rogues (to accomodate more cover types)
	internal static bool CalcCoverLevelGeoOnly(out bool[] hasCover, BoardSquare square)
	{
		return CalcCover(out hasCover, square, null, null, null, true);
	}

	// changed in rogues (to accomodate more cover types)
	internal static bool CalcCover(
		out bool[] hasCover,
		BoardSquare square,
		List<CoverDirections> tempCoversNormal,
		List<CoverDirections> tempCoversIgnoreMinDist,
		bool[] coverDirFromBarriers,
		bool minDistOk)
	{
		hasCover = new bool[4];
		if (square == null)
		{
			return false;
		}
		bool hasAnyCover = false;
		List<BoardSquare> cardinalAdjacentSquares = null;
		Board.Get().GetCardinalAdjacentSquares(square.x, square.y, ref cardinalAdjacentSquares);
		foreach (BoardSquare boardSquare in cardinalAdjacentSquares)
		{
			CoverDirections coverDirection = GetCoverDirection(square, boardSquare);
			int elevation = boardSquare.height - square.height;
			if (!minDistOk)
			{
				bool ignoreMinDist = tempCoversIgnoreMinDist != null && tempCoversIgnoreMinDist.Contains(coverDirection);
				hasCover[(int)coverDirection] = ignoreMinDist;
				hasAnyCover = hasAnyCover || ignoreMinDist;
			}
			else
			{
				if (elevation < 1
				    && square.GetThinCover(coverDirection) == ThinCover.CoverType.None
				    && (tempCoversNormal == null || !tempCoversNormal.Contains(coverDirection))
				    && (tempCoversIgnoreMinDist == null || !tempCoversIgnoreMinDist.Contains(coverDirection))
				    && (coverDirFromBarriers == null || !coverDirFromBarriers[(int)coverDirection]))
				{
					hasCover[(int)coverDirection] = false;
				}
				else
				{
					hasCover[(int)coverDirection] = true;
					hasAnyCover = true;
				}
			}
		}
		return hasAnyCover;
	}

	public static CoverDirections GetCoverDirection(BoardSquare srcSquare, BoardSquare destSquare)
	{
		CoverDirections result;
		if (srcSquare.x > destSquare.x)
		{
			result = CoverDirections.X_NEG;
		}
		else if (srcSquare.x < destSquare.x)
		{
			result = CoverDirections.X_POS;
		}
		else if (srcSquare.y > destSquare.y)
		{
			result = CoverDirections.Y_NEG;
		}
		else if (srcSquare.y < destSquare.y)
		{
			result = CoverDirections.Y_POS;
		}
		else
		{
			result = CoverDirections.INVALID;
		}
		return result;
	}

	public bool IsInCoverWrt(Vector3 damageOrigin)  // , out HitChanceBracketType strongestCover in rogues
	{
		List<CoverDirections> list = null;
		return IsInCoverWrt(damageOrigin, ref list); // , out strongestCover in rogues
	}

	public static bool IsInCoverWrt(
		Vector3 damageOrigin,
		BoardSquare targetSquare,
		List<CoverDirections> tempCoverProviders,
		List<CoverDirections> tempCoversIgnoreMinDist,
		// List<CoverDirections> adjacentCovers, // rogues
		bool[] coverDirFromBarriers
		// , out HitChanceBracketType strongestCover // rogues
		)
	{
		List<CoverDirections> list = null;
		return IsInCoverWrt(
			damageOrigin,
			targetSquare,
			ref list,
			tempCoverProviders,
			tempCoversIgnoreMinDist,
			// adjacentCovers, // rogues
			coverDirFromBarriers
			// , out strongestCover // rogues
			);
	}

	public bool IsInCoverWrt(Vector3 damageOrigin, ref List<CoverDirections> coverDirections) // , out HitChanceBracketType strongestCover in rogues
	{
		ActorData component = GetComponent<ActorData>();
		BoardSquare currentBoardSquare = component.GetCurrentBoardSquare();
		return IsInCoverWrt(
			damageOrigin,
			currentBoardSquare,
			ref coverDirections,
			m_tempCoverProviders,
			m_tempCoverIgnoreMinDist,
			// m_adjacentCoverProviders,  // rogues
			m_cachedHasCoverFromBarriers
			// , out strongestCover  // rogues
			);
	}

	public bool IsInCoverForDirection(CoverDirections dir)
	{
		return m_hasCover[(int)dir]; // > ThinCover.CoverType.None in rogues
	}

	public static bool IsInCoverWrt(
		Vector3 damageOrigin,
		BoardSquare targetSquare,
		ref List<CoverDirections> coverDirections,
		List<CoverDirections> tempCoverProviders,
		List<CoverDirections> tempCoversIgnoreMinDist,
		// List<CoverDirections> adjacentCovers, // rogues
		bool[] coverDirFromBarriers
		// , out HitChanceBracketType strongestCover // rogues
		)
	{
		// strongestCover = HitChanceBracketType.Default; // rogues
		if (targetSquare == null)
		{
			return false;
		}
		Vector3 b = targetSquare.ToVector3();
		Vector3 vector = damageOrigin - b;
		vector.y = 0f;
		float sqrMagnitude = vector.sqrMagnitude;
		float num = GameplayData.Get().m_coverMinDistance * Board.Get().squareSize;
		float num2 = num * num;
		bool flag = sqrMagnitude >= num2;
		if (!flag && (tempCoversIgnoreMinDist == null || tempCoversIgnoreMinDist.Count == 0))
		{
			return false;
		}
		int numCoverSourcesByDirectionOnly = GetNumCoverSourcesByDirectionOnly(
			damageOrigin,
			targetSquare,
			ref coverDirections,
			tempCoverProviders,
			tempCoversIgnoreMinDist,
			// adjacentCovers,  // rpgues
			coverDirFromBarriers,
			flag
			// , out strongestCover  // rpgues
			);
		return numCoverSourcesByDirectionOnly > 0;
	}

	public bool IsInCoverWrtDirectionOnly(Vector3 damageOrigin, BoardSquare targetSquare) // , out HitChanceBracketType strongestCover in rogues
	{
		List<CoverDirections> list = null;
		return GetNumCoverSourcesByDirectionOnly(
			       damageOrigin,
			       targetSquare,
			       ref list,
			       m_tempCoverProviders,
			       m_tempCoverIgnoreMinDist,
			       // m_adjacentCoverProviders,
			       m_cachedHasCoverFromBarriers,
			       true
			       // , out strongestCover
			       ) >
		       0;
	}

	// rogues
	// private static HitChanceBracketType CoverTypeToHitChanceBracketType(ThinCover.CoverType coverType)
	// {
	// 	switch (coverType)
	// 	{
	// 	case ThinCover.CoverType.Half:
	// 	case ThinCover.CoverType.HalfThick:
	// 		return HitChanceBracketType.HalfCover;
	// 	case ThinCover.CoverType.Full:
	// 	case ThinCover.CoverType.FullThick:
	// 		return HitChanceBracketType.FullCover;
	// 	}
	// 	return HitChanceBracketType.Default;
	// }

	// changed in rogues (to accomodate more cover types)
	private static int GetNumCoverSourcesByDirectionOnly(Vector3 damageOrigin, BoardSquare targetSquare, ref List<CoverDirections> coverDirections, List<CoverDirections> tempCoverProviders, List<CoverDirections> tempCoverIgnoreMinDist /*, List<CoverDirections> adjacentCovers*/, bool[] coverDirFromBarriers, bool minDistOk/*, out HitChanceBracketType strongestCover*/)
	{
		int num = 0;
		Vector3 vector = targetSquare.ToVector3();
		bool flag = damageOrigin.x < vector.x;
		bool flag2 = damageOrigin.x > vector.x;
		bool flag3 = damageOrigin.z < vector.z;
		bool flag4 = damageOrigin.z > vector.z;
		Vector2 vector2 = new Vector2(damageOrigin.x - vector.x, damageOrigin.z - vector.z);
		Vector2 normalized = vector2.normalized;
		float num2 = 0.5f * Board.Get().squareSize;
		float num3 = GameplayData.Get().m_coverProtectionAngle / 2f;
		bool[] array;
		CalcCover(out array, targetSquare, tempCoverProviders, tempCoverIgnoreMinDist, coverDirFromBarriers, minDistOk);
		if (array[1] && flag && vector2.x < -num2)
		{
			Vector2 lhs = new Vector2(-1f, 0f);
			float num4 = Mathf.Acos(Vector2.Dot(lhs, normalized));
			float num5 = num4 * 57.29578f;
			if (num5 <= num3)
			{
				num++;
				if (coverDirections != null)
				{
					coverDirections.Add(CoverDirections.X_NEG);
				}
			}
		}
		if (array[0] && flag2 && vector2.x > num2)
		{
			Vector2 lhs2 = new Vector2(1f, 0f);
			float num6 = Mathf.Acos(Vector2.Dot(lhs2, normalized));
			float num7 = num6 * 57.29578f;
			if (num7 <= num3)
			{
				num++;
				if (coverDirections != null)
				{
					coverDirections.Add(CoverDirections.X_POS);
				}
			}
		}
		if (array[3] && flag3 && vector2.y < -num2)
		{
			Vector2 lhs3 = new Vector2(0f, -1f);
			float num8 = Mathf.Acos(Vector2.Dot(lhs3, normalized));
			float num9 = num8 * 57.29578f;
			if (num9 <= num3)
			{
				num++;
				if (coverDirections != null)
				{
					coverDirections.Add(CoverDirections.Y_NEG);
				}
			}
		}
		if (array[2] && flag4 && vector2.y > num2)
		{
			Vector2 lhs4 = new Vector2(0f, 1f);
			float num10 = Mathf.Acos(Vector2.Dot(lhs4, normalized));
			float num11 = num10 * 57.29578f;
			if (num11 <= num3)
			{
				num++;
				if (coverDirections != null)
				{
					coverDirections.Add(CoverDirections.Y_POS);
				}
			}
		}
		return num;
	}

	public bool IsDirInCover(Vector3 dir)
	{
		float angle_deg = VectorUtils.HorizontalAngle_Deg(dir);
		foreach (CoverRegion coverRegion in GetCoveredRegions())
		{
			if (coverRegion.IsDirInCover(angle_deg))
			{
				return true;
			}
		}
		return false;
	}

	public List<CoverRegion> GetCoveredRegions()
	{
		List<CoverRegion> list = new List<CoverRegion>();
		ActorData owner = m_owner;
		if (owner == null)
		{
			Debug.LogError("Trying to get the covered regions for a null actor.");
			return list;
		}
		if (owner.IsDead())
		{
			Debug.LogError(string.Concat("Trying to get the covered regions for the dead actor ", owner.DisplayName, ", a ", owner.name, "."));
			return list;
		}
		if (owner.GetCurrentBoardSquare() == null)
		{
			Debug.LogError(string.Concat("Trying to get the covered regions for the (alive) actor ", owner.DisplayName, ", a ", owner.name, ", but the square is null."));
			return list;
		}
		BoardSquare currentBoardSquare = owner.GetCurrentBoardSquare();
		Vector3 center = currentBoardSquare.ToVector3();
		float num = GameplayData.Get().m_coverProtectionAngle / 2f;
		for (int i = 0; i < 4; i++)
		{
			if (m_hasCover[i]) //  != ThinCover.CoverType.None in rogues
			{
				CoverDirections dir = (CoverDirections)i;
				float centerAngleOfDirection = GetCenterAngleOfDirection(dir);
				CoverRegion item = new CoverRegion(center, centerAngleOfDirection - num, centerAngleOfDirection + num);
				list.Add(item);
			}
		}
		if (list.Count != 0 && list.Count != 1)
		{
			if (list.Count == 4)
			{
				return new List<CoverRegion>
				{
					new CoverRegion(center, -720f, 720f)
				};
			}
			if (list.Count == 3)
			{
				float num2 = list[0].m_startAngle;
				float num3 = list[0].m_endAngle;
				foreach (CoverRegion coverRegion in list)
				{
					num2 = Mathf.Min(num2, coverRegion.m_startAngle);
					num3 = Mathf.Max(num3, coverRegion.m_endAngle);
				}
				return new List<CoverRegion>
				{
					new CoverRegion(center, num2, num3)
				};
			}
			if (list.Count == 2)
			{
				CoverRegion coverRegion2 = list[0];
				CoverRegion coverRegion3 = list[1];
				if ((coverRegion2.m_startAngle > coverRegion3.m_startAngle || coverRegion3.m_startAngle > coverRegion2.m_endAngle)
				    && (coverRegion2.m_startAngle > coverRegion3.m_endAngle || coverRegion3.m_endAngle > coverRegion2.m_endAngle))
				{
					return list;
				}
				float startAngle = Mathf.Min(coverRegion2.m_startAngle, coverRegion3.m_startAngle);
				float endAngle = Mathf.Max(coverRegion2.m_endAngle, coverRegion3.m_endAngle);
				return new List<CoverRegion>
				{
					new CoverRegion(center, startAngle, endAngle)
				};
			}
			Log.Error(string.Concat("Actor ", owner.DisplayName, " in cover in ", list.Count, " directions."));
			return list;
		}
		return list;
	}

	public void ClampConeToValidCover(float coneDirAngleDegrees, float coneWidthDegrees, out float newDirAngleDegrees, out Vector3 newConeDir)
	{
		float num = coneDirAngleDegrees;
		List<CoverRegion> coveredRegions = GetCoveredRegions();
		bool flag = false;
		float angle_deg = coneDirAngleDegrees - coneWidthDegrees / 2f;
		float angle_deg2 = coneDirAngleDegrees + coneWidthDegrees / 2f;
		foreach (CoverRegion coverRegion in coveredRegions)
		{
			if (coverRegion.IsDirInCover(angle_deg) && coverRegion.IsDirInCover(angle_deg2))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			foreach (CoverRegion coverRegion2 in coveredRegions)
			{
				if (coverRegion2.IsDirInCover(coneDirAngleDegrees))
				{
					if (coverRegion2.IsDirInCover(angle_deg) && !coverRegion2.IsDirInCover(angle_deg2))
					{
						num = coverRegion2.m_endAngle - coneWidthDegrees / 2f;
					}
					else if (!coverRegion2.IsDirInCover(angle_deg) && coverRegion2.IsDirInCover(angle_deg2))
					{
						num = coverRegion2.m_startAngle + coneWidthDegrees / 2f;
					}
				}
			}
		}
		newDirAngleDegrees = num;
		newConeDir = VectorUtils.AngleDegreesToVector(num);
	}

	private static float GetCenterAngleOfDirection(CoverDirections dir)
	{
		float result;
		switch (dir)
		{
		case CoverDirections.X_POS:
			result = 0f;
			break;
		case CoverDirections.X_NEG:
			result = 180f;
			break;
		case CoverDirections.Y_POS:
			result = 90f;
			break;
		case CoverDirections.Y_NEG:
			result = 270f;
			break;
		default:
			result = 0f;
			break;
		}
		return result;
	}

	public static bool DoesCoverDirectionProvideCoverFromPos(CoverDirections dir, Vector3 coveredPos, Vector3 attackOriginPos)
	{
		float num = GameplayData.Get().m_coverProtectionAngle / 2f;
		float centerAngleOfDirection = GetCenterAngleOfDirection(dir);
		CoverRegion coverRegion = new CoverRegion(coveredPos, centerAngleOfDirection - num, centerAngleOfDirection + num);
		return coverRegion.IsInCoverFromPos(attackOriginPos);
	}

	// rogues
	// public void CalculateAdjacentCoverProviders()
	// {
	// 	m_adjacentCoverProviders.Clear();
	// 	List<BoardSquare> list = new List<BoardSquare>();
	// 	if (m_owner.GetCurrentBoardSquare() != null)
	// 	{
	// 		Board.Get().GetAllAdjacentSquares(m_owner.GetCurrentBoardSquare().x, m_owner.GetCurrentBoardSquare().y, ref list);
	// 		foreach (BoardSquare boardSquare in list)
	// 		{
	// 			if (boardSquare.OccupantActor != null && boardSquare.OccupantActor.GetTeam() != m_owner.GetEnemyTeam() && boardSquare.OccupantActor.m_grantCover)
	// 			{
	// 				m_adjacentCoverProviders.Add(GetCoverDirection(m_owner.GetCurrentBoardSquare(), boardSquare));
	// 			}
	// 		}
	// 	}
	// }

	// reactor
	private void UNetVersion()
	{
	}
	// rogues
	// private void MirrorProcessed()
	// {
	// }

	// removed in rogues
	protected static void InvokeSyncListm_syncTempCoverProviders(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("SyncList m_syncTempCoverProviders called on server.");
			return;
		}
		((ActorCover)obj).m_syncTempCoverProviders.HandleMsg(reader);
	}

	// removed in rogues
	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteStructSyncListTempCoverInfo_None(writer, m_syncTempCoverProviders);
			return true;
		}
		bool flag = false;
		if ((syncVarDirtyBits & 1U) != 0U)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(syncVarDirtyBits);
				flag = true;
			}
			GeneratedNetworkCode._WriteStructSyncListTempCoverInfo_None(writer, m_syncTempCoverProviders);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(syncVarDirtyBits);
		}
		return flag;
	}

	// removed in rogues
	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			GeneratedNetworkCode._ReadStructSyncListTempCoverInfo_None(reader, m_syncTempCoverProviders);
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if ((num & 1) != 0)
		{
			GeneratedNetworkCode._ReadStructSyncListTempCoverInfo_None(reader, m_syncTempCoverProviders);
		}
	}

	public enum CoverDirections
	{
		INVALID = -1,
		X_POS,
		X_NEG,
		Y_POS,
		Y_NEG,
		NUM,
		FIRST = 0
	}
}
