using CameraManagerInternal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraManager : MonoBehaviour, IGameEventListener
{
	internal enum AbilityCinematicState
	{
		Default,
		Never,
		Always
	}

	public enum CameraTargetReason
	{
		CameraCenterKeyHeld,
		AbilitySoftTargeting,
		ClientActorRespawned,
		MustSelectRespawnLoc,
		ChangedActiveActor,
		CtfTurninRegionSpawned,
		ReachedTargetObj,
		IsoCamEnabled,
		ForcingTransform,
		UserFocusingOnActor,
		SinglePlayerStateScriptCommand,
		CtfFlagTurnedIn
	}

	public enum CameraShakeIntensity
	{
		Small,
		Large,
		None
	}

	public enum CameraLogType
	{
		None,
		Ability,
		Isometric,
		MergeBounds,
		SimilarBounds
	}

	public GameObject m_gameCameraPrefab;

	public GameObject m_faceCameraPrefab;

	public GameObject m_tauntBackgroundCameraPrefab;

	public bool m_useTauntBackground;

	internal AbilityCinematicState m_abilityCinematicState;

	private static CameraManager s_instance;

	private bool m_useAbilitiesCameraOutOfCinematics;

	private int m_abilityAnimationsBetweenCamEvents;

	private float m_secondsRemaingUnderUserControl = -1f;

	private List<CameraShot.CharacterToAnimParamSetActions> m_animParamSettersOnTurnTick = new List<CameraShot.CharacterToAnimParamSetActions>();

	private bool m_useCameraToggleKey = true;

	private bool m_useRightClickToToggle;

	public const float c_cameraToggleGracePeriod = 1.5f;

	private Bounds m_savedMoveCamBound;

	private int m_savedMoveCamBoundTurn = -1;

	internal float DefaultFOV
	{
		get;
		private set;
	}

	public Camera FaceCamera
	{
		get;
		private set;
	}

	internal GameObject AudioListener
	{
		get;
		private set;
	}

	internal CameraShotSequence ShotSequence
	{
		get;
		private set;
	}

	internal CameraFaceShot FaceShot
	{
		get;
		private set;
	}

	internal Bounds CameraPositionBounds
	{
		get;
		private set;
	}

	internal TauntBackgroundCamera TauntBackgroundCamera
	{
		get;
		private set;
	}

	internal float SecondsRemainingToPauseForUserControl
	{
		get
		{
			return m_secondsRemaingUnderUserControl;
		}
		set
		{
			m_secondsRemaingUnderUserControl = value;
		}
	}

	public bool UseCameraToggleKey => m_useCameraToggleKey;

	public static bool CamDebugTraceOn => false;

	internal bool InFaceShot(ActorData actor)
	{
		int result;
		if (FaceShot != null)
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
			result = ((FaceShot.Actor == actor) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	internal bool WillRespondToInput()
	{
		return GetIsometricCamera().enabled;
	}

	internal bool IsPlayingShotSequence()
	{
		return ShotSequence != null;
	}

	internal bool InCinematic()
	{
		return ShotSequence != null;
	}

	internal ActorData GetCinematicTargetActor()
	{
		object result;
		if (ShotSequence == null)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			result = null;
		}
		else
		{
			result = ShotSequence.Actor;
		}
		return (ActorData)result;
	}

	internal int GetCinematicActionAnimIndex()
	{
		return (!(ShotSequence == null)) ? ShotSequence.m_animIndexTauntTrigger : (-1);
	}

	internal bool ShouldHideBrushVfx()
	{
		int result;
		if (TauntBackgroundCamera != null)
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
			result = ((ShotSequence != null) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	internal static CameraManager Get()
	{
		return s_instance;
	}

	protected void Awake()
	{
		if (s_instance != null)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			Debug.LogError("CameraManager instance was not null on Awake(), please check to make sure there is only 1 instance of GameSceneSingletons object");
		}
		s_instance = this;
		if ((bool)GameFlowData.Get() && (bool)VisualsLoader.Get() && VisualsLoader.Get().LevelLoaded())
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
			OnVisualSceneLoaded();
		}
		else
		{
			base.enabled = false;
		}
		GameEventManager.Get().AddListener(this, GameEventManager.EventType.VisualSceneLoaded);
	}

	protected void Start()
	{
		if (!(Camera.main != null))
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
			DefaultFOV = Camera.main.fieldOfView;
			return;
		}
	}

	private void ClearCameras()
	{
		if (Camera.main != null)
		{
			UnityEngine.Object.DestroyImmediate(Camera.main.gameObject);
		}
		if (FaceCamera != null)
		{
			UnityEngine.Object.DestroyImmediate(FaceCamera.gameObject);
		}
	}

	private void OnDestroy()
	{
		ClearCameras();
		s_instance = null;
		if (GameEventManager.Get() != null)
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
			GameEventManager.Get().RemoveListener(this, GameEventManager.EventType.VisualSceneLoaded);
		}
		s_instance = null;
	}

	void IGameEventListener.OnGameEvent(GameEventManager.EventType eventType, GameEventManager.GameEventArgs args)
	{
		if (eventType != GameEventManager.EventType.VisualSceneLoaded)
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
			OnVisualSceneLoaded();
			return;
		}
	}

	public void EnableOffFogOfWarEffect(bool enable)
	{
		if (!(Camera.main != null))
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
			if (Camera.main.gameObject.GetComponent<FogOfWarEffect>() != null)
			{
				Camera.main.gameObject.GetComponent<FogOfWarEffect>().enabled = enable;
			}
			return;
		}
	}

	private void OnVisualSceneLoaded()
	{
		object obj;
		if (Camera.main == null)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			obj = null;
		}
		else
		{
			obj = Camera.main.gameObject;
		}
		GameObject gameObject = (GameObject)obj;
		if (gameObject != null)
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
			if (gameObject.GetComponent<IsometricCamera>() == null)
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
				if (NetworkClient.active)
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
					Log.Error("Environment scene is missing an instance of GameCamera.prefab, bloom may be active when loading into the level on Graphics Quality: Low until an instance is put in the environment scene.");
				}
				if (m_gameCameraPrefab != null)
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
					UnityEngine.Object.DestroyImmediate(gameObject);
					UnityEngine.Object.Instantiate(m_gameCameraPrefab);
					gameObject = ((!(Camera.main == null)) ? Camera.main.gameObject : null);
				}
			}
		}
		if (gameObject == null)
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
			if (NetworkClient.active)
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
				Log.Error("Environment scene is missing an instance of GameCamera.prefab, bloom may be active when loading into the level on Graphics Quality: Low until an instance is put in the environment scene.");
			}
			if (m_gameCameraPrefab == null)
			{
				while (true)
				{
					switch (4)
					{
					case 0:
						continue;
					}
					throw new ApplicationException("There is no game camera prefab assigned in the CameraManager!");
				}
			}
			GameObject y = UnityEngine.Object.Instantiate(m_gameCameraPrefab);
			if (Camera.main == null)
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
				Log.Error("Failed to switch to game camera; main camera is null");
			}
			else if (Camera.main.gameObject != y)
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
				Log.Error("Failed to switch to game camera; main camera is '{0}'", Camera.main);
			}
		}
		UnityEngine.Object.DontDestroyOnLoad(Camera.main.gameObject);
		if (FaceCamera == null)
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
			GameObject gameObject2 = UnityEngine.Object.Instantiate(m_faceCameraPrefab);
			UnityEngine.Object.DontDestroyOnLoad(gameObject2);
			object faceCamera;
			if (gameObject2 == null)
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
				faceCamera = null;
			}
			else
			{
				faceCamera = gameObject2.GetComponent<Camera>();
			}
			FaceCamera = (Camera)faceCamera;
			FaceCamera.gameObject.SetActive(false);
		}
		if (Camera.main != null)
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
			DefaultFOV = Camera.main.fieldOfView;
			RenderSettings.fog = false;
			if (NetworkClient.active)
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
				if (m_tauntBackgroundCameraPrefab != null && m_useTauntBackground)
				{
					GameObject gameObject3 = UnityEngine.Object.Instantiate(m_tauntBackgroundCameraPrefab);
					TauntBackgroundCamera = gameObject3.GetComponent<TauntBackgroundCamera>();
					if (TauntBackgroundCamera == null)
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
						Debug.LogError("Did not find taunt background camera component");
						UnityEngine.Object.Destroy(gameObject3);
					}
					else
					{
						gameObject3.SetActive(false);
					}
				}
			}
		}
		Board board = Board.Get();
		BoardSquare boardSquare = Board.Get().GetBoardSquare(Board.Get().GetMaxX() / 2, Board.Get().GetMaxY() / 2);
		if (boardSquare != null)
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
			IsometricCamera isometricCamera = GetIsometricCamera();
			if (isometricCamera != null)
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
				if (isometricCamera.enabled)
				{
					Vector3 position = boardSquare.gameObject.transform.position;
					float x = position.x;
					float y2 = Board.Get().BaselineHeight;
					Vector3 position2 = boardSquare.gameObject.transform.position;
					Vector3 pos = new Vector3(x, y2, position2.z);
					isometricCamera.SetTargetPosition(pos, 0f);
				}
			}
		}
		BoardSquare boardSquare2 = board.GetBoardSquare(0, 0);
		BoardSquare boardSquare3 = board.GetBoardSquare(board.GetMaxX() - 1, board.GetMaxY() - 1);
		Bounds cameraBounds = boardSquare2.CameraBounds;
		Bounds cameraBounds2 = boardSquare3.CameraBounds;
		Bounds cameraPositionBounds = cameraBounds;
		cameraPositionBounds.Encapsulate(cameraBounds2);
		CameraPositionBounds = cameraPositionBounds;
		if (AudioListener == null)
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
			if (AudioListenerController.Get() != null)
			{
				AudioListener = AudioListenerController.Get().gameObject;
			}
		}
		base.enabled = true;
		GameEventManager.Get().FireEvent(GameEventManager.EventType.GameCameraCreatedPre, null);
		GameEventManager.Get().FireEvent(GameEventManager.EventType.GameCameraCreated, null);
		GameEventManager.Get().FireEvent(GameEventManager.EventType.GameCameraCreatedPost, null);
	}

	internal bool IsOnCamera(Bounds bounds)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
		return GeometryUtility.TestPlanesAABB(planes, bounds);
	}

	internal FlyThroughCamera GetFlyThroughCamera()
	{
		object result;
		if (Camera.main != null)
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
			result = Camera.main.GetComponent<FlyThroughCamera>();
		}
		else
		{
			result = null;
		}
		return (FlyThroughCamera)result;
	}

	internal DebugCamera GetDebugCamera()
	{
		object result;
		if (Camera.main != null)
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
			result = Camera.main.GetComponent<DebugCamera>();
		}
		else
		{
			result = null;
		}
		return (DebugCamera)result;
	}

	internal IsometricCamera GetIsometricCamera()
	{
		return (!(Camera.main != null)) ? null : Camera.main.GetComponent<IsometricCamera>();
	}

	internal AbilitiesCamera GetAbilitiesCamera()
	{
		return (!(Camera.main != null)) ? null : Camera.main.GetComponent<AbilitiesCamera>();
	}

	internal FadeObjectsCameraComponent GetFadeObjectsCamera()
	{
		object result;
		if (Camera.main != null)
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
			result = Camera.main.GetComponent<FadeObjectsCameraComponent>();
		}
		else
		{
			result = null;
		}
		return (FadeObjectsCameraComponent)result;
	}

	internal void OnSpecialCameraShotBehaviorEnable(CameraTransitionType transitionInType)
	{
		IsometricCamera isometricCamera = GetIsometricCamera();
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		if (abilitiesCamera.enabled)
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
			abilitiesCamera.OnTransitionOut();
			abilitiesCamera.enabled = false;
		}
		if (!isometricCamera.enabled)
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
			isometricCamera.OnTransitionOut();
			isometricCamera.enabled = false;
			return;
		}
	}

	internal void OnSpecialCameraShotBehaviorDisable(CameraTransitionType transitionInType)
	{
		Camera.main.fieldOfView = Get().DefaultFOV;
		m_useAbilitiesCameraOutOfCinematics = ShouldUseAbilitiesCameraOutOfCinematics();
		if (DebugParameters.Get() != null && DebugParameters.Get().GetParameterAsBool("DebugCamera"))
		{
			return;
		}
		if (m_useAbilitiesCameraOutOfCinematics)
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
					EnableAbilitiesCamera(transitionInType);
					return;
				}
			}
		}
		EnableIsometricCamera(transitionInType);
	}

	public bool ShouldAutoCameraMove()
	{
		if (AccountPreferences.Get() != null)
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
			if (AccountPreferences.Get().GetBool(BoolPreference.AutoCameraCenter))
			{
				goto IL_00c0;
			}
		}
		if (GameManager.Get() != null && GameManager.Get().GameConfig != null)
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
			if (GameManager.Get().GameConfig.GameType == GameType.Tutorial)
			{
				goto IL_00c0;
			}
			while (true)
			{
				switch (2)
				{
				case 0:
					continue;
				}
				break;
			}
		}
		int result;
		if (GameFlowData.Get().activeOwnedActorData != null)
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
			result = ((GameFlowData.Get().activeOwnedActorData.GetActorTurnSM().CurrentState == TurnStateEnum.PICKING_RESPAWN) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		goto IL_00c1;
		IL_00c1:
		return (byte)result != 0;
		IL_00c0:
		result = 1;
		goto IL_00c1;
	}

	internal void OnActionPhaseChange(ActionBufferPhase newPhase, bool requestAbilityCamera)
	{
		if (DebugParameters.Get() == null || !DebugParameters.Get().GetParameterAsBool("DebugCamera"))
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			if (requestAbilityCamera && ShouldUseAbilitiesCameraOutOfCinematics())
			{
				EnableAbilitiesCamera();
			}
			else
			{
				EnableIsometricCamera();
			}
		}
		FadeObjectsCameraComponent fadeObjectsCamera = GetFadeObjectsCamera();
		if (fadeObjectsCamera != null)
		{
			fadeObjectsCamera.ResetDesiredVisibleObjects();
		}
		if (m_abilityAnimationsBetweenCamEvents > 0)
		{
			Log.Warning("Camera manager: phase change to {0} with {1} abilities between camera start and end tags. Expected zero", newPhase.ToString(), m_abilityAnimationsBetweenCamEvents);
			m_abilityAnimationsBetweenCamEvents = 0;
		}
	}

	internal void SaveMovementCameraBound(Bounds bound)
	{
		m_savedMoveCamBound = bound;
		m_savedMoveCamBoundTurn = GameFlowData.Get().CurrentTurn;
	}

	internal void SetTargetForMovementIfNeeded()
	{
		if (!ShouldSetCameraForMovement() || GameFlowData.Get().CurrentTurn != m_savedMoveCamBoundTurn)
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
			SetTarget(m_savedMoveCamBound);
			return;
		}
	}

	internal void SaveMovementCameraBoundForSpectator(Bounds bound)
	{
		if (m_savedMoveCamBoundTurn == GameFlowData.Get().CurrentTurn)
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
					m_savedMoveCamBound.Encapsulate(bound);
					return;
				}
			}
		}
		m_savedMoveCamBound = bound;
		m_savedMoveCamBoundTurn = GameFlowData.Get().CurrentTurn;
	}

	internal void SwitchCameraForMovement()
	{
		if (!ShouldSetCameraForMovement())
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
			if (!(SecondsRemainingToPauseForUserControl <= 0f))
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
				if (Get().ShouldAutoCameraMove())
				{
					while (true)
					{
						switch (3)
						{
						case 0:
							continue;
						}
						Get().OnActionPhaseChange(ActionBufferPhase.Movement, true);
						return;
					}
				}
				return;
			}
		}
	}

	private bool ShouldSetCameraForMovement()
	{
		int result;
		if (GameManager.Get() != null)
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
			result = ((GameManager.Get().GameConfig.GameType != GameType.Tutorial) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	internal void AddAnimParamSetActions(CameraShot.CharacterToAnimParamSetActions animParamSetActions)
	{
		if (animParamSetActions == null)
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
			m_animParamSettersOnTurnTick.Add(animParamSetActions);
			return;
		}
	}

	internal void OnTurnTick()
	{
		if (NetworkClient.active)
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
			using (List<CameraShot.CharacterToAnimParamSetActions>.Enumerator enumerator = m_animParamSettersOnTurnTick.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CameraShot.CharacterToAnimParamSetActions current = enumerator.Current;
					if (current != null)
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
						if (current.m_actor != null)
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
							CameraShot.SetAnimParamsForActor(current.m_actor, current.m_animSetActions);
						}
					}
				}
				while (true)
				{
					switch (5)
					{
					case 0:
						continue;
					}
					break;
				}
			}
		}
		m_animParamSettersOnTurnTick.Clear();
	}

	private void EnableFlyThroughCamera()
	{
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		IsometricCamera isometricCamera = GetIsometricCamera();
		DebugCamera debugCamera = GetDebugCamera();
		FlyThroughCamera flyThroughCamera = GetFlyThroughCamera();
		if (!flyThroughCamera.enabled)
		{
			abilitiesCamera.enabled = false;
			isometricCamera.enabled = false;
			debugCamera.enabled = false;
			flyThroughCamera.enabled = true;
		}
	}

	private void EnableDebugCamera()
	{
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		IsometricCamera isometricCamera = GetIsometricCamera();
		DebugCamera debugCamera = GetDebugCamera();
		FlyThroughCamera flyThroughCamera = GetFlyThroughCamera();
		if (!debugCamera.enabled)
		{
			abilitiesCamera.enabled = false;
			isometricCamera.enabled = false;
			flyThroughCamera.enabled = false;
			debugCamera.enabled = true;
		}
	}

	private void EnableAbilitiesCamera(CameraTransitionType transitionInType = CameraTransitionType.Move)
	{
		if (Camera.main == null)
		{
			while (true)
			{
				switch (4)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					return;
				}
			}
		}
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		IsometricCamera isometricCamera = GetIsometricCamera();
		DebugCamera debugCamera = GetDebugCamera();
		FlyThroughCamera flyThroughCamera = GetFlyThroughCamera();
		if (abilitiesCamera == null)
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
			abilitiesCamera = Camera.main.gameObject.AddComponent<AbilitiesCamera>();
			abilitiesCamera.enabled = false;
			Log.Warning("Missing AbilitiesCamera component on main camera. Generating dynamically for now.");
		}
		m_useAbilitiesCameraOutOfCinematics = true;
		debugCamera.enabled = false;
		flyThroughCamera.enabled = false;
		if (abilitiesCamera.enabled)
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
			if (CamDebugTraceOn)
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
				LogForDebugging("<color=white>Enable Abilities Camera</color>, transition type: " + transitionInType);
			}
			if (isometricCamera.enabled)
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
				isometricCamera.OnTransitionOut();
				isometricCamera.enabled = false;
			}
			abilitiesCamera.enabled = true;
			abilitiesCamera.OnTransitionIn(transitionInType);
			return;
		}
	}

	private void EnableIsometricCamera(CameraTransitionType transitionInType = CameraTransitionType.Move)
	{
		if (Camera.main == null)
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					return;
				}
			}
		}
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		IsometricCamera isometricCamera = GetIsometricCamera();
		DebugCamera debugCamera = GetDebugCamera();
		FlyThroughCamera flyThroughCamera = GetFlyThroughCamera();
		if (debugCamera != null)
		{
			debugCamera.enabled = false;
		}
		if (flyThroughCamera != null)
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
			flyThroughCamera.enabled = false;
		}
		if (abilitiesCamera == null)
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
			abilitiesCamera = Camera.main.gameObject.AddComponent<AbilitiesCamera>();
			abilitiesCamera.enabled = false;
			Log.Warning("Missing IsometricCamera component on main camera. Generating dynamically for now.");
		}
		m_useAbilitiesCameraOutOfCinematics = false;
		if (isometricCamera.enabled)
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
			if (!(GameFlowData.Get().LocalPlayerData != null))
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
				if (CamDebugTraceOn)
				{
					LogForDebugging("<color=white>Enable Isometric Camera</color>, transition type: " + transitionInType);
				}
				bool flag = false;
				if (abilitiesCamera.enabled)
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
					flag = (abilitiesCamera.GetSecondsRemainingToPauseForUserControl() > 0f);
					abilitiesCamera.OnTransitionOut();
					abilitiesCamera.enabled = false;
				}
				isometricCamera.enabled = true;
				isometricCamera.OnTransitionIn(transitionInType);
				if (!(GameFlowData.Get().activeOwnedActorData != null))
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
					GameObject targetObject;
					if (!abilitiesCamera.IsDisabledUntilSetTarget)
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
						if (!flag)
						{
							targetObject = GameFlowData.Get().activeOwnedActorData.gameObject;
							goto IL_01ae;
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
					targetObject = null;
					goto IL_01ae;
					IL_01ae:
					isometricCamera.SetTargetObject(targetObject, CameraTargetReason.IsoCamEnabled);
					return;
				}
			}
		}
	}

	internal void OnActiveOwnedActorChange(ActorData actor)
	{
		FadeObjectsCameraComponent fadeObjectsCamera = GetFadeObjectsCamera();
		if (fadeObjectsCamera != null)
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
			fadeObjectsCamera.ResetDesiredVisibleObjects();
		}
		if (SecondsRemainingToPauseForUserControl <= 0f)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					break;
				default:
					SetTargetObject(actor.gameObject, CameraTargetReason.ChangedActiveActor);
					return;
				}
			}
		}
		SetTargetObject(null, CameraTargetReason.ChangedActiveActor);
	}

	internal void OnActorMoved(ActorData actor)
	{
		FadeObjectsCameraComponent fadeObjectsCamera = GetFadeObjectsCamera();
		if (!(fadeObjectsCamera != null))
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
			fadeObjectsCamera.MarkForResetVisibleObjects();
			return;
		}
	}

	internal void OnSelectedAbilityChanged(Ability ability)
	{
		FadeObjectsCameraComponent fadeObjectsCamera = GetFadeObjectsCamera();
		if (!(fadeObjectsCamera != null))
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
			fadeObjectsCamera.ResetDesiredVisibleObjects();
			return;
		}
	}

	internal void OnNewTurnSMState()
	{
		FadeObjectsCameraComponent fadeObjectsCamera = GetFadeObjectsCamera();
		if (!(fadeObjectsCamera != null))
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
			fadeObjectsCamera.ResetDesiredVisibleObjects();
			return;
		}
	}

	internal bool IsOnMainCamera(ActorData a)
	{
		if (a == null)
		{
			while (true)
			{
				switch (5)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					return false;
				}
			}
		}
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
		Bounds cameraBounds = a.GetTravelBoardSquare().CameraBounds;
		return GeometryUtility.TestPlanesAABB(planes, cameraBounds);
	}

	internal void SetTargetObject(GameObject target, CameraTargetReason reason)
	{
		IsometricCamera isometricCamera = GetIsometricCamera();
		if (!(isometricCamera != null))
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
			if (isometricCamera.enabled)
			{
				while (true)
				{
					switch (6)
					{
					case 0:
						continue;
					}
					isometricCamera.SetTargetObject(target, reason, false);
					return;
				}
			}
			return;
		}
	}

	internal void SetTargetObjectToMouse(GameObject target, CameraTargetReason reason)
	{
		if (CamDebugTraceOn)
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
			object str;
			if (target != null)
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
				str = target.name;
			}
			else
			{
				str = "NULL";
			}
			LogForDebugging("CameraManager.SetTargetObjectToMouse " + (string)str);
		}
		IsometricCamera isometricCamera = GetIsometricCamera();
		if (!(isometricCamera != null))
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
			if (isometricCamera.enabled)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						continue;
					}
					isometricCamera.SetTargetObject(target, reason, true);
					return;
				}
			}
			return;
		}
	}

	public void SetTargetPosition(Vector3 pos, float easeInTime = 0f)
	{
		IsometricCamera isometricCamera = GetIsometricCamera();
		if (!(isometricCamera != null))
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
			if (isometricCamera.enabled)
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						continue;
					}
					isometricCamera.SetTargetPosition(pos, easeInTime);
					isometricCamera.SetTargetObject(null, CameraTargetReason.ReachedTargetObj);
					return;
				}
			}
			return;
		}
	}

	internal void SetTarget(Bounds bounds, bool quickerTransition = false, bool useLowPosition = false)
	{
		if (CamDebugTraceOn)
		{
			LogForDebugging("CameraManager.SetTarget " + bounds.ToString() + " | quicker transition: " + quickerTransition + " | useLowPosition: " + useLowPosition);
		}
		if (Camera.main == null)
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
					return;
				}
			}
		}
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		if (abilitiesCamera == null)
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
			abilitiesCamera = Camera.main.gameObject.AddComponent<AbilitiesCamera>();
			abilitiesCamera.enabled = false;
		}
		abilitiesCamera.SetTarget(bounds, quickerTransition, useLowPosition);
	}

	internal Bounds GetTarget()
	{
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		if (abilitiesCamera == null)
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
			abilitiesCamera = Camera.main.gameObject.AddComponent<AbilitiesCamera>();
			abilitiesCamera.enabled = false;
		}
		return abilitiesCamera.GetTarget();
	}

	public void PlayCameraShake(CameraShakeIntensity intensity)
	{
		if (intensity == CameraShakeIntensity.None)
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
			if (Camera.main.gameObject.GetComponent<CameraShake>() == null)
			{
				Camera.main.gameObject.AddComponent<CameraShake>();
			}
			switch (intensity)
			{
			default:
				return;
			case CameraShakeIntensity.Small:
				Camera.main.gameObject.GetComponent<CameraShake>().Play(0.1f, 0.025f, 0.5f);
				return;
			case CameraShakeIntensity.Large:
				break;
			}
			while (true)
			{
				switch (4)
				{
				case 0:
					continue;
				}
				Camera.main.gameObject.GetComponent<CameraShake>().Play(0.3f, 0.1f, 0.75f);
				return;
			}
		}
	}

	internal bool AllowCameraShake()
	{
		DebugCamera debugCamera = GetDebugCamera();
		if (debugCamera != null)
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
			if (debugCamera.enabled && !debugCamera.AllowCameraShake())
			{
				while (true)
				{
					switch (3)
					{
					case 0:
						break;
					default:
						return false;
					}
				}
			}
		}
		IsometricCamera isometricCamera = GetIsometricCamera();
		if (isometricCamera != null)
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
			if (isometricCamera.enabled)
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
				if (!isometricCamera.AllowCameraShake())
				{
					while (true)
					{
						switch (4)
						{
						case 0:
							break;
						default:
							return false;
						}
					}
				}
			}
		}
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		if (abilitiesCamera != null)
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
			if (abilitiesCamera.enabled && abilitiesCamera.IsMovingAutomatically())
			{
				while (true)
				{
					switch (5)
					{
					case 0:
						break;
					default:
						return m_abilityAnimationsBetweenCamEvents > 0;
					}
				}
			}
		}
		return true;
	}

	public void OnAnimationEvent(ActorData animatedActor, UnityEngine.Object eventObject)
	{
		if (eventObject.name == "CameraShakeSmallEvent")
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					break;
				default:
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					PlayCameraShake(CameraShakeIntensity.Small);
					return;
				}
			}
		}
		if (eventObject.name == "CameraShakeLargeEvent")
		{
			PlayCameraShake(CameraShakeIntensity.Large);
			return;
		}
		if (eventObject.name == "CamStartEvent")
		{
			while (true)
			{
				switch (1)
				{
				case 0:
					break;
				default:
					m_abilityAnimationsBetweenCamEvents++;
					return;
				}
			}
		}
		if (!(eventObject.name == "CamEndEvent"))
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
			m_abilityAnimationsBetweenCamEvents--;
			if (m_abilityAnimationsBetweenCamEvents < 0)
			{
				Log.Warning("Camera manger: ability animation CamStart CamEnd count  mismatch");
				m_abilityAnimationsBetweenCamEvents = 0;
			}
			return;
		}
	}

	public bool OnAbilityAnimationStart(ActorData animatedActor, int animationIndex, Vector3 targetPos, bool requestCinematicCam, int cinematicRequested)
	{
		bool result = false;
		int num;
		if (DebugParameters.Get() != null)
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
			num = (DebugParameters.Get().GetParameterAsBool("DebugCamera") ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		if (ShotSequence == null)
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
			if (!flag)
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
				if (requestCinematicCam)
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
					if (m_abilityCinematicState == AbilityCinematicState.Default)
					{
						goto IL_0098;
					}
					while (true)
					{
						switch (5)
						{
						case 0:
							continue;
						}
						break;
					}
				}
				if (m_abilityCinematicState == AbilityCinematicState.Always)
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
					goto IL_0098;
				}
			}
		}
		goto IL_0235;
		IL_0098:
		TauntCameraSet tauntCamSetData = animatedActor.m_tauntCamSetData;
		CameraShotSequence cameraShotSequence = null;
		CameraShot[] array = null;
		int altCamShotIndex = -1;
		if (tauntCamSetData != null)
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
			for (int i = 0; i < tauntCamSetData.m_tauntCameraShotSequences.Length; i++)
			{
				CameraShotSequence cameraShotSequence2 = tauntCamSetData.m_tauntCameraShotSequences[i] as CameraShotSequence;
				if (!(cameraShotSequence2 != null))
				{
					continue;
				}
				while (true)
				{
					switch (5)
					{
					case 0:
						continue;
					}
					break;
				}
				if (cameraShotSequence2.m_tauntNumber != cinematicRequested)
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
					break;
				}
				if (cameraShotSequence2.m_animIndexTauntTrigger == animationIndex)
				{
					cameraShotSequence = cameraShotSequence2;
					array = cameraShotSequence2.m_cameraShots;
					break;
				}
				if (cameraShotSequence2.m_alternateCameraShots == null || cameraShotSequence2.m_alternateCameraShots.Length <= 0)
				{
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
				for (int j = 0; j < cameraShotSequence2.m_alternateCameraShots.Length; j++)
				{
					if (cameraShotSequence2.m_alternateCameraShots[j].m_altAnimIndexTauntTrigger == animationIndex)
					{
						cameraShotSequence = cameraShotSequence2;
						array = cameraShotSequence2.m_alternateCameraShots[j].m_altCameraShots;
						altCamShotIndex = j;
						break;
					}
				}
			}
		}
		if (cameraShotSequence != null)
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
			if (array != null)
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
				if (array.Length > 0)
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
					ShotSequence = cameraShotSequence;
					ShotSequence.Begin(animatedActor, altCamShotIndex);
					result = true;
					HUD_UI.Get().SetHUDVisibility(false, false);
					if (animatedActor.GetIsHumanControlled())
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
						HUD_UI.Get().SetupTauntBanner(animatedActor);
					}
					HUD_UI.Get().SetTauntBannerVisibility(animatedActor.GetIsHumanControlled());
				}
			}
		}
		goto IL_0235;
		IL_0235:
		return result;
	}

	internal void BeginFaceShot(CameraFaceShot faceShot, ActorData actor)
	{
		if (faceShot == null)
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
			if (actor != null && FaceCamera != null)
			{
				FaceShot = faceShot;
				faceShot.Begin(actor, FaceCamera);
			}
			return;
		}
	}

	private bool IsCameraCenterKeyHeld()
	{
		bool flag = false;
		if (GameFlowData.Get() != null && GameFlowData.Get().gameState == GameState.EndingGame)
		{
			flag = true;
		}
		int result;
		if (!flag)
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
			if (1 == 0)
			{
				/*OpCode not supported: LdMemberToken*/;
			}
			result = (InputManager.Get().IsKeyBindingHeld(KeyPreference.CameraCenterOnAction) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		return (byte)result != 0;
	}

	internal bool DoesAnimIndexTriggerTauntCamera(ActorData actor, int animIndex, int tauntNumber)
	{
		TauntCameraSet tauntCamSetData = actor.m_tauntCamSetData;
		if (tauntCamSetData != null)
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
			if (tauntCamSetData.m_tauntCameraShotSequences != null)
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
				for (int i = 0; i < tauntCamSetData.m_tauntCameraShotSequences.Length; i++)
				{
					CameraShotSequence cameraShotSequence = tauntCamSetData.m_tauntCameraShotSequences[i] as CameraShotSequence;
					if (!(cameraShotSequence != null))
					{
						continue;
					}
					while (true)
					{
						switch (7)
						{
						case 0:
							continue;
						}
						break;
					}
					if (cameraShotSequence.m_tauntNumber != tauntNumber)
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
						break;
					}
					if (cameraShotSequence.m_animIndexTauntTrigger == animIndex)
					{
						while (true)
						{
							switch (2)
							{
							case 0:
								break;
							default:
								return true;
							}
						}
					}
					if (cameraShotSequence.m_alternateCameraShots == null)
					{
						continue;
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
					if (cameraShotSequence.m_alternateCameraShots.Length <= 0)
					{
						continue;
					}
					while (true)
					{
						switch (7)
						{
						case 0:
							continue;
						}
						break;
					}
					for (int j = 0; j < cameraShotSequence.m_alternateCameraShots.Length; j++)
					{
						if (cameraShotSequence.m_alternateCameraShots[j].m_altAnimIndexTauntTrigger != animIndex)
						{
							continue;
						}
						while (true)
						{
							switch (1)
							{
							case 0:
								continue;
							}
							return true;
						}
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
		}
		return false;
	}

	internal void OnPlayerMovedCamera()
	{
		if (!(UIMainScreenPanel.Get() != null))
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
			if (UIMainScreenPanel.Get().m_autoCameraButton != null)
			{
				while (true)
				{
					switch (4)
					{
					case 0:
						continue;
					}
					UIMainScreenPanel.Get().m_autoCameraButton.OnPlayerMovedCamera();
					return;
				}
			}
			return;
		}
	}

	public void Update()
	{
		int num;
		if (DebugParameters.Get() != null)
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
			num = (DebugParameters.Get().GetParameterAsBool("DebugCamera") ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		if (flag)
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
			if (!GetDebugCamera().enabled)
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
				EnableDebugCamera();
				goto IL_012a;
			}
		}
		if (AppState.GetCurrent() == AppState_InGameDeployment.Get())
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
			if (GetFlyThroughCamera() != null)
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
				EnableFlyThroughCamera();
				goto IL_012a;
			}
		}
		if (!flag)
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
			if (GetDebugCamera() != null)
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
				if (GetDebugCamera().enabled)
				{
					goto IL_0123;
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
		}
		if (!(GetFlyThroughCamera() == null))
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
			if (!GetFlyThroughCamera().enabled)
			{
				goto IL_012a;
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
		goto IL_0123;
		IL_012a:
		if (SecondsRemainingToPauseForUserControl > 0f)
		{
			SecondsRemainingToPauseForUserControl -= Time.deltaTime;
		}
		if (GameFlowData.Get() == null)
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
			if (Camera.main == null)
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
				if (flag)
				{
					return;
				}
				if (GetFlyThroughCamera().enabled)
				{
					while (true)
					{
						switch (7)
						{
						default:
							return;
						case 0:
							break;
						}
					}
				}
				if (ShotSequence != null)
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
					if (!ShotSequence.Update())
					{
						ShotSequence = null;
						HUD_UI.Get().SetHUDVisibility(true, true);
						HUD_UI.Get().SetTauntBannerVisibility(false);
					}
				}
				if (FaceShot != null && !FaceShot.Update(FaceCamera))
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
					FaceCamera.gameObject.SetActive(false);
					FaceShot = null;
				}
				AccountPreferences accountPreferences = AccountPreferences.Get();
				bool flag2;
				int num3;
				if (UIMainScreenPanel.Get() != null && UIMainScreenPanel.Get().m_autoCameraButton != null && GameManager.Get().GameConfig.GameType != GameType.Tutorial)
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
					int num2;
					if (m_useCameraToggleKey)
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
						num2 = (InputManager.Get().IsKeyBindingNewlyHeld(KeyPreference.CameraToggleAutoCenter) ? 1 : 0);
					}
					else
					{
						num2 = 0;
					}
					flag2 = ((byte)num2 != 0);
					if (m_useRightClickToToggle)
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
						if (!flag2)
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
							if (InterfaceManager.Get() != null)
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
								if (InterfaceManager.Get().ShouldHandleMouseClick())
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
									if (Input.GetMouseButtonUp(1))
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
										if (GameFlowData.Get() != null)
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
											if (GameFlowData.Get().gameState == GameState.BothTeams_Resolve)
											{
												num3 = ((GameFlowData.Get().GetPause() || GameFlowData.Get().GetTimeInState() >= 1.5f) ? 1 : 0);
												goto IL_036b;
											}
										}
									}
									num3 = 0;
									goto IL_036b;
								}
							}
						}
					}
					goto IL_036c;
				}
				goto IL_03cf;
				IL_03cf:
				if (ShotSequence == null)
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
					m_useAbilitiesCameraOutOfCinematics = ShouldUseAbilitiesCameraOutOfCinematics();
					if (!flag)
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
						if (m_useAbilitiesCameraOutOfCinematics)
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
							EnableAbilitiesCamera();
						}
						else
						{
							EnableIsometricCamera();
						}
					}
					if (IsCameraCenterKeyHeld())
					{
						ActorData activeOwnedActorData = GameFlowData.Get().activeOwnedActorData;
						CameraManager cameraManager = Get();
						object target;
						if (activeOwnedActorData == null)
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
							target = null;
						}
						else
						{
							target = activeOwnedActorData.gameObject;
						}
						cameraManager.SetTargetObject((GameObject)target, CameraTargetReason.CameraCenterKeyHeld);
						if (ControlpadGameplay.Get() != null)
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
							ControlpadGameplay.Get().OnCameraCenteredOnActor(activeOwnedActorData);
						}
					}
				}
				if (!(AudioListener != null))
				{
					return;
				}
				if (ShotSequence != null)
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
					if (ShotSequence.Actor != null)
					{
						while (true)
						{
							switch (5)
							{
							case 0:
								break;
							default:
								AudioListener.transform.position = ShotSequence.Actor.transform.position;
								return;
							}
						}
					}
				}
				Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
				float num4 = Vector3.Dot(Vector3.down, Camera.main.transform.forward);
				if (num4 < 0.258819f)
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
					Vector3 direction = Quaternion.AngleAxis(-75f, Camera.main.transform.right) * Vector3.down;
					ray = new Ray(Camera.main.transform.position, direction);
				}
				if (!new Plane(Vector3.up, -Board.Get().BaselineHeight).Raycast(ray, out float enter))
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
					enter = 3f;
				}
				AudioListener.transform.position = ray.GetPoint(enter);
				return;
				IL_036c:
				if (flag2)
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
					bool flag3 = !accountPreferences.GetBool(BoolPreference.AutoCameraCenter);
					accountPreferences.SetBool(BoolPreference.AutoCameraCenter, flag3);
					UIMainScreenPanel.Get().m_autoCameraButton.RefreshAutoCameraButton();
					AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
					if (flag3)
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
						if (abilitiesCamera != null)
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
							abilitiesCamera.OnAutoCenterCameraPreferenceSet();
						}
					}
				}
				goto IL_03cf;
				IL_036b:
				flag2 = ((byte)num3 != 0);
				goto IL_036c;
			}
		}
		IL_0123:
		EnableIsometricCamera();
		goto IL_012a;
	}

	private bool ShouldUseAbilitiesCameraOutOfCinematics()
	{
		bool flag = false;
		if (Camera.main == null)
		{
			return false;
		}
		AbilitiesCamera abilitiesCamera = GetAbilitiesCamera();
		if (abilitiesCamera != null)
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
			if (abilitiesCamera.IsDisabledUntilSetTarget)
			{
				while (true)
				{
					switch (5)
					{
					case 0:
						break;
					default:
						return false;
					}
				}
			}
		}
		bool flag2 = GameFlowData.Get().gameState == GameState.BothTeams_Resolve;
		ActionBufferPhase currentActionPhase = ServerClientUtils.GetCurrentActionPhase();
		int num;
		if (currentActionPhase != ActionBufferPhase.AbilitiesWait)
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
			if (currentActionPhase != ActionBufferPhase.Movement)
			{
				num = ((currentActionPhase == ActionBufferPhase.MovementChase) ? 1 : 0);
				goto IL_0086;
			}
		}
		num = 1;
		goto IL_0086;
		IL_0173:
		return false;
		IL_0086:
		bool flag3 = (byte)num != 0;
		bool flag4 = !TheatricsManager.Get().AbilityPhaseHasNoAnimations();
		if (flag2)
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
			if (!flag3)
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
				if (!flag4)
				{
					goto IL_0173;
				}
			}
			bool flag5 = GameManager.Get() != null && GameManager.Get().GameConfig.GameType == GameType.Tutorial;
			if (!ShouldAutoCameraMove())
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
				if (!Get().GetAbilitiesCamera().enabled)
				{
					while (true)
					{
						switch (6)
						{
						case 0:
							break;
						default:
							return false;
						}
					}
				}
			}
			if (flag5)
			{
				while (true)
				{
					int result;
					switch (5)
					{
					case 0:
						break;
					default:
						{
							int num2;
							if (currentActionPhase != ActionBufferPhase.MovementWait)
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
								num2 = ((currentActionPhase == ActionBufferPhase.Done) ? 1 : 0);
							}
							else
							{
								num2 = 1;
							}
							bool flag6 = (byte)num2 != 0;
							if (flag4)
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
								if (!flag3)
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
									result = ((!flag6) ? 1 : 0);
									goto IL_016c;
								}
							}
							result = 0;
							goto IL_016c;
						}
						IL_016c:
						return (byte)result != 0;
					}
				}
			}
			return true;
		}
		goto IL_0173;
	}

	public static bool BoundSidesWithinDistance(Bounds currentBound, Bounds compareToBound, float mergeSizeThresh, out Vector3 maxBoundDiff, out Vector3 minBoundDiff)
	{
		bool result = false;
		Vector3 a = currentBound.center + currentBound.extents;
		Vector3 a2 = currentBound.center - currentBound.extents;
		Vector3 b = compareToBound.center + compareToBound.extents;
		Vector3 b2 = compareToBound.center - compareToBound.extents;
		maxBoundDiff = a - b;
		minBoundDiff = a2 - b2;
		if (Mathf.Abs(maxBoundDiff.x) <= mergeSizeThresh)
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
			if (Mathf.Abs(maxBoundDiff.z) <= mergeSizeThresh)
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
				if (Mathf.Abs(minBoundDiff.x) <= mergeSizeThresh)
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
					if (Mathf.Abs(minBoundDiff.z) <= mergeSizeThresh)
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
						result = true;
					}
				}
			}
		}
		return result;
	}

	private void OnDrawGizmos()
	{
		if (!ShouldDrawGizmosForCurrentCamera())
		{
			return;
		}
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(CameraPositionBounds.center, CameraPositionBounds.size);
		Gizmos.DrawWireSphere(CameraPositionBounds.center, 1f);
		if (!(AudioListener != null))
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
			Gizmos.DrawWireSphere(AudioListener.transform.position, 1f);
			return;
		}
	}

	public static bool ShouldDrawGizmosForCurrentCamera()
	{
		if (Application.isEditor)
		{
			while (true)
			{
				switch (6)
				{
				case 0:
					break;
				default:
				{
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					LayerMask mask = 1 << LayerMask.NameToLayer("Default");
					return (Camera.current.cullingMask & (int)mask) != 0;
				}
				}
			}
		}
		return false;
	}

	public static void LogForDebugging(string str, CameraLogType cameraType = CameraLogType.None)
	{
		string text;
		if (cameraType == CameraLogType.None)
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
			text = string.Empty;
		}
		else
		{
			text = "[" + cameraType.ToString() + "]";
		}
		string text2 = text;
		Debug.LogWarning("<color=magenta>Camera " + text2 + " | </color>" + str + "\n@time= " + Time.time);
	}
}
