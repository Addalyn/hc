using System;
using UnityEngine;

namespace CameraManagerInternal
{
    public class IsometricCamera : MonoBehaviour, IGameEventListener
    {
        public float m_movementCatchUpMult = 1.75f;
        public float m_startHorzDist = 12f;
        public float m_maxHorzDist = 15f;
        public float m_maxVertDist = 15f;
        public float m_defaultRotationY = 50f;
        public float m_easeInTime = 1f;
        public float m_heightAdjustMaxSpeed = 8f;
        public float m_movementHorzDist = 10f;

        public AnimationCurve m_vertCurve = new AnimationCurve();
        public AnimationCurve m_tiltCurve = new AnimationCurve();
        public AnimationCurve m_horzCurve = new AnimationCurve();
        public AnimationCurve m_fovCurve = new AnimationCurve();

        public float m_minZoomParamForCoverZoomOffset = 0.6f;
        public float m_coverHeightEaseTime = 1.4f;
        public float m_zoomEaseTime = 0.5f;
        public float m_transitionInTime = 0.3f;
        public float m_inputMoveEaseTime = 0.3f;

        public float m_NO_EDIT_zoomInTilt;
        public float m_NO_EDIT_zoomOutTilt;
        public float m_NO_EDIT_zoomLevel;

        private Vector3 m_defaultEulerRotation = new Vector3(50f, 50f, 0f);
        private float m_maxDistFarHorz;
        private float m_maxDistFarVert;
        private float m_transitionInTimeLeft;
        private Vector3 m_transitionInPosition;
        private Quaternion m_transitionInRotation;
        private float m_transitionFOV;

        private GameObject m_targetObject;
        private CameraManager.CameraTargetReason m_targetReason;
        private ActorData m_targetObjectActor;
        private Vector3 m_targetObjectOffset;

        private bool m_cutToTarget = true;
        private bool m_respawnedBeforeUpdateEnd;
        private float m_zoomGoalValue;
        private bool m_needNameplateSortUpdate;
        private float m_nextNameplateSortUpdateTime = -1f;

        private const float c_minNameplateSortInterval = 0.6f;

        private EasedOutFloat m_zoomParameter = new EasedOutFloat(1f);
        private EasedFloat m_zoomParameterScale = new EasedFloat(1f);
        private EasedOutFloat m_zoomVertOffsetForAnimatedActor = new EasedOutFloat(1f);
        private EasedOutVector3 m_targetPosition = new EasedOutVector3(Vector3.zero);
        private bool m_targetWithBoardSquareWasNeverSet = true;

        internal Vector3 TargetPosition => m_targetPosition;

        private void Awake()
        {
            if (m_tiltCurve == null || m_tiltCurve.keys.Length < 1)
            {
                Log.Warning("Please remove misconfigured IsometricCamera component from " + gameObject);
                enabled = false;
            }
        }

        private void Start()
        {
            if (GameManager.IsEditorAndNotGame())
            {
                enabled = false;
                return;
            }

            UpdateNoEdit();
            m_maxDistFarHorz = 100f * m_maxHorzDist;
            m_maxDistFarVert = 100f * m_maxVertDist;
            m_zoomParameter = new EasedOutFloat(CalcZoomParameter(m_startHorzDist));
            m_zoomGoalValue = m_zoomParameter.GetEndValue();
            m_defaultEulerRotation.x = CalcZoomRotationX();
            m_defaultEulerRotation.y = m_defaultRotationY;
            if (CameraControls.Get() != null)
            {
                CameraControls.Get().m_desiredRotationEulerAngles = m_defaultEulerRotation;
            }

            if (GameFlowData.Get() != null)
            {
                ActorData activeOwnedActorData = GameFlowData.Get().activeOwnedActorData;
                if (activeOwnedActorData != null)
                {
                    CameraManager.Get().OnActiveOwnedActorChange(activeOwnedActorData);
                    ResetCameraRotation();
                }
            }

            GameEventManager.Get().AddListener(this, GameEventManager.EventType.CharacterRespawn);
        }

        private void OnDestroy()
        {
            if (GameEventManager.Get() != null)
            {
                GameEventManager.Get().RemoveListener(this, GameEventManager.EventType.CharacterRespawn);
            }
        }

        public void OnGameEvent(GameEventManager.EventType eventType, GameEventManager.GameEventArgs args)
        {
            if (eventType == GameEventManager.EventType.CharacterRespawn)
            {
                GameEventManager.CharacterRespawnEventArgs characterRespawnEventArgs =
                    (GameEventManager.CharacterRespawnEventArgs)args;
                if (GameFlowData.Get().activeOwnedActorData == characterRespawnEventArgs.respawningCharacter)
                {
                    m_respawnedBeforeUpdateEnd = true;
                }
            }
        }

        public void SetTargetObject(GameObject targetObject, CameraManager.CameraTargetReason reason)
        {
            SetTargetObject(targetObject, reason, false);
        }

        public void SetTargetObject(
            GameObject targetObject,
            CameraManager.CameraTargetReason reason,
            bool putUnderMouse)
        {
            if (CameraManager.CamDebugTraceOn && targetObject != m_targetObject)
            {
                CameraManager.LogForDebugging(
                    "SetTargetObject to " + (targetObject != null ? targetObject.name : "NULL"),
                    CameraManager.CameraLogType.Isometric);
            }

            ActorData actorData = targetObject != null ? targetObject.GetComponent<ActorData>() : null;
            if (actorData != null)
            {
                if (m_targetWithBoardSquareWasNeverSet && actorData.GetCurrentBoardSquare() != null)
                {
                    ResetCameraRotation();
                    m_cutToTarget = true;
                    m_targetWithBoardSquareWasNeverSet = false;
                }

                m_zoomVertOffsetForAnimatedActor = new EasedOutFloat(
                    GetZoomVertOffsetForActiveAnimatedActor(
                        (float)m_zoomParameter * (float)m_zoomParameterScale > m_minZoomParamForCoverZoomOffset));
            }

            m_targetObject = targetObject;
            m_targetObjectActor = actorData;
            m_targetReason = reason;
            if (putUnderMouse)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                EasedOutVector3 targetPosition = m_targetPosition;
                if (new Plane(Vector3.up, targetPosition).Raycast(ray, out float enter))
                {
                    m_targetObjectOffset = targetPosition - ray.GetPoint(enter);
                }
            }
            else
            {
                m_targetObjectOffset = Vector3.zero;
            }

            if (m_targetObject != null && CameraManager.Get().ShouldAutoCameraMove())
            {
                m_targetPosition.EaseTo(m_targetObject.transform.position + m_targetObjectOffset, m_easeInTime);
            }
        }

        public void SetTargetPosition(Vector3 pos, float easeInTime)
        {
            m_targetPosition.EaseTo(pos, easeInTime);
        }

        public float GetInitialYAngle()
        {
            float result = m_defaultRotationY;
            Vector3 characterPos = GameFlowData.Get().activeOwnedActorData != null
                ? GameFlowData.Get().activeOwnedActorData.transform.position
                : transform.position;
            Vector3 center = CameraManager.Get().CameraPositionBounds.center;
            bool roundTo45 = false;
            if (SinglePlayerManager.Get() != null
                && SinglePlayerCoordinator.Get().m_initialCameraRotationTarget != null)
            {
                center = SinglePlayerCoordinator.Get().m_initialCameraRotationTarget.transform.position;
                roundTo45 = true;
            }

            Vector3 forward = center - characterPos;
            forward.y = 0f;
            float magnitude = forward.magnitude;
            if (!Mathf.Approximately(magnitude, 0f))
            {
                forward /= magnitude;
                Quaternion quaternion = Quaternion.LookRotation(forward);
                if (roundTo45)
                {
                    result = (int)quaternion.eulerAngles.y / 45 * 45;
                }
                else
                {
                    int angleFromDefault = (int)(quaternion.eulerAngles.y - m_defaultRotationY);
                    int finalAngle = angleFromDefault / 90 * 90;
                    if (angleFromDefault - finalAngle > finalAngle + 90 - angleFromDefault)
                    {
                        finalAngle += 90;
                    }

                    result = finalAngle;
                }
            }

            return result;
        }

        public void ResetCameraRotation()
        {
            CameraControls.Get().m_desiredRotationEulerAngles.y = m_defaultRotationY;
            Vector3 characterPos = GameFlowData.Get().activeOwnedActorData != null
                ? GameFlowData.Get().activeOwnedActorData.transform.position
                : transform.position;
            Vector3 center = CameraManager.Get().CameraPositionBounds.center;
            bool roundTo45 = false;
            if (SinglePlayerManager.Get() != null
                && SinglePlayerCoordinator.Get().m_initialCameraRotationTarget != null)
            {
                center = SinglePlayerCoordinator.Get().m_initialCameraRotationTarget.transform.position;
                roundTo45 = true;
            }

            Vector3 forward = center - characterPos;
            forward.y = 0f;
            float magnitude = forward.magnitude;
            if (Mathf.Approximately(magnitude, 0f))
            {
                return;
            }

            forward /= magnitude;
            Quaternion quaternion = Quaternion.LookRotation(forward);
            if (roundTo45)
            {
                CameraControls.Get().m_desiredRotationEulerAngles.y = (int)quaternion.eulerAngles.y / 45 * 45;
            }
            else
            {
                int angleFromDefault = (int)(quaternion.eulerAngles.y - m_defaultRotationY);
                int finalAngle = angleFromDefault / 90 * 90;
                if (angleFromDefault - finalAngle > finalAngle + 90 - angleFromDefault)
                {
                    finalAngle += 90;
                }

                CameraControls.Get().m_desiredRotationEulerAngles.y = finalAngle;
            }
        }

        internal bool AllowCameraShake()
        {
            return m_targetPosition.EaseFinished();
        }

        private void UpdateNoEdit()
        {
            m_NO_EDIT_zoomInTilt = Mathf.Atan(m_tiltCurve.keys[0].value) * Mathf.Rad2Deg;
            m_NO_EDIT_zoomOutTilt = Mathf.Atan(m_tiltCurve.keys[m_tiltCurve.keys.Length - 1].value) * Mathf.Rad2Deg;
            m_NO_EDIT_zoomLevel = (float)m_zoomParameter * (float)m_zoomParameterScale;
        }

        private bool IsInMovementPhase()
        {
            ActionBufferPhase currentActionPhase = ServerClientUtils.GetCurrentActionPhase();
            return currentActionPhase == ActionBufferPhase.Movement
                   || currentActionPhase == ActionBufferPhase.MovementChase
                   || currentActionPhase == ActionBufferPhase.AbilitiesWait
                   || currentActionPhase == ActionBufferPhase.MovementWait;
        }

        private void Update()
        {
            Vector3 position = transform.position;
            Quaternion rotationThisFrame = transform.rotation;
            Vector3 positionDelta = Vector3.zero;
            float zoomDelta = 0f;
            if (CameraControls.Get() == null || GameFlowData.Get() == null)
            {
                return;
            }

            if (!m_targetWithBoardSquareWasNeverSet
                || GameFlowData.Get().LocalPlayerData != null && GameFlowData.Get().activeOwnedActorData == null)
            {
                CameraControls.Get().CalcDesiredTransform(
                    transform,
                    out positionDelta,
                    out rotationThisFrame,
                    out zoomDelta);
            }

            if (!Mathf.Approximately(zoomDelta, 0f))
            {
                float value = CalcZoomParameterDelta(zoomDelta)
                              + m_zoomParameter.GetEndValue() * (float)m_zoomParameterScale;
                value = Mathf.Clamp01(value);
                m_zoomParameter.EaseTo(value, m_zoomEaseTime);
                m_zoomGoalValue = value;
                m_zoomParameterScale = new EasedFloat(1f);
            }
            else if (!Mathf.Approximately(m_zoomGoalValue, m_zoomParameter.GetEndValue()))
            {
                m_zoomParameter.EaseTo(m_zoomGoalValue, m_zoomEaseTime);
            }

            if (positionDelta.sqrMagnitude > float.Epsilon)
            {
                CameraManager.Get().SecondsRemainingToPauseForUserControl = 0.5f;
                CameraManager.Get().OnPlayerMovedCamera();
                Vector3 endValue = m_targetPosition.GetEndValue() + positionDelta;
                GameplayData gameplayData = GameplayData.Get();
                endValue.x = Mathf.Clamp(endValue.x, gameplayData.m_minimumPositionX, gameplayData.m_maximumPositionX);
                endValue.z = Mathf.Clamp(endValue.z, gameplayData.m_minimumPositionZ, gameplayData.m_maximumPositionZ);
                m_targetPosition.EaseTo(endValue, m_inputMoveEaseTime);
                SetTargetObject(null, CameraManager.CameraTargetReason.ReachedTargetObj);
            }
            else if (m_targetObject != null)
            {
                Vector3 focusPosition = m_targetObject.transform.position + m_targetObjectOffset;
                if (!m_cutToTarget)
                {
                    bool focus = m_respawnedBeforeUpdateEnd
                                 || InputManager.Get().IsKeyBindingHeld(KeyPreference.CameraCenterOnAction)
                                 || m_targetReason == CameraManager.CameraTargetReason.UserFocusingOnActor
                                 || m_targetReason == CameraManager.CameraTargetReason.CtfTurninRegionSpawned
                                 || m_targetReason == CameraManager.CameraTargetReason.CtfFlagTurnedIn;
                    if (m_targetObjectActor != null)
                    {
                        if (m_targetObjectActor.GetActorMovement().AmMoving())
                        {
                            BoardSquarePathInfo aestheticPath =
                                m_targetObjectActor.GetActorMovement().GetAestheticPath();
                            if (aestheticPath != null && aestheticPath.square != null)
                            {
                                if (CameraManager.Get().ShouldAutoCameraMove())
                                {
                                    focusPosition = aestheticPath.square.GetOccupantRefPos() + m_targetObjectOffset;
                                    Vector3 startValue =
                                        (focusPosition - m_targetPosition) * Time.deltaTime * m_movementCatchUpMult
                                        + m_targetPosition;
                                    m_targetPosition = new EasedOutVector3(startValue);
                                    focus = false;
                                }
                                else if (InputManager.Get().IsKeyBindingHeld(KeyPreference.CameraCenterOnAction))
                                {
                                    BoardSquarePathInfo pathEndpoint = aestheticPath.GetPathEndpoint();
                                    if (pathEndpoint != null && pathEndpoint.square != null)
                                    {
                                        focusPosition = pathEndpoint.square.GetOccupantRefPos() + m_targetObjectOffset;
                                    }
                                }
                            }
                        }
                        else if (!focus && CameraManager.Get().ShouldAutoCameraMove() && IsInMovementPhase())
                        {
                            float magnitude = (focusPosition - m_targetPosition).magnitude;
                            if (magnitude > 0f)
                            {
                                m_targetPosition = new EasedOutVector3(
                                    magnitude < 1f
                                        ? focusPosition
                                        : (focusPosition - m_targetPosition) * Mathf.Min(
                                            1f,
                                            Time.deltaTime * 2f * m_movementCatchUpMult) + m_targetPosition);
                            }
                        }
                    }

                    if (focus)
                    {
                        m_targetPosition.EaseTo(focusPosition, m_easeInTime);
                    }
                }
                else
                {
                    m_targetPosition = new EasedOutVector3(focusPosition);
                    m_zoomVertOffsetForAnimatedActor = new EasedOutFloat(
                        GetZoomVertOffsetForActiveAnimatedActor(
                            (float)m_zoomParameter * (float)m_zoomParameterScale > m_minZoomParamForCoverZoomOffset));
                    m_cutToTarget = false;
                }
            }

            Vector3 zoomOffset = CalcZoomOffsetForActiveAnimatedActor(rotationThisFrame);
            if (!CameraControls.Get().IsTiltUserControlled())
            {
                rotationThisFrame =
                    Quaternion.Euler(new Vector3(CalcZoomRotationX(), rotationThisFrame.eulerAngles.y, 0f));
            }

            Vector3 targetPosition = m_targetPosition + zoomOffset;
            if (m_transitionInTimeLeft > 0f)
            {
                float alpha = Easing.ExpoEaseInOut(
                    m_transitionInTime - m_transitionInTimeLeft,
                    0f,
                    1f,
                    m_transitionInTime);
                transform.position = Vector3.Lerp(m_transitionInPosition, targetPosition, alpha);
                transform.rotation = Quaternion.Slerp(m_transitionInRotation, rotationThisFrame, alpha);
                if (Camera.main != null)
                {
                    Camera.main.fieldOfView = (CalcFOV() - m_transitionFOV) * alpha + m_transitionFOV;
                }

                m_transitionInTimeLeft -= Time.deltaTime;
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = rotationThisFrame;
                if (Camera.main != null)
                {
                    Camera.main.fieldOfView = CalcFOV();
                }
            }

            if ((position - transform.position).sqrMagnitude > float.Epsilon)
            {
                m_needNameplateSortUpdate = true;
            }

            if (m_needNameplateSortUpdate
                && (m_nextNameplateSortUpdateTime < 0f || Time.time > m_nextNameplateSortUpdateTime))
            {
                if (HUD_UI.Get() != null
                    && HUD_UI.Get().m_mainScreenPanel != null
                    && HUD_UI.Get().m_mainScreenPanel.m_nameplatePanel != null)
                {
                    HUD_UI.Get().m_mainScreenPanel.m_nameplatePanel.SortNameplates();
                }

                m_needNameplateSortUpdate = false;
                m_nextNameplateSortUpdateTime = Time.time + 0.6f;
            }

            m_respawnedBeforeUpdateEnd = false;
            if (ActorDebugUtils.Get() != null
                && ActorDebugUtils.Get().ShowingCategory(ActorDebugUtils.DebugCategory.CameraManager))
            {
                ActorDebugUtils.DebugCategoryInfo debugCategoryInfo = ActorDebugUtils.Get()
                    .GetDebugCategoryInfo(ActorDebugUtils.DebugCategory.CameraManager);
                debugCategoryInfo.m_stringToDisplay =
                    "Updating Isometric Camera:\n\n"
                    + $"Position: {transform.position} | Rotation: {transform.rotation.eulerAngles}\n"
                    + $"\tEased Position: {m_targetPosition}\n"
                    + $"FOV: {Camera.main.fieldOfView}\n"
                    + $"Zoom: {m_zoomParameter}\n"
                    + $"\tZoom Offset: {zoomOffset}\n"
                    + $"\tZoom Goal: {m_zoomGoalValue}\n";
            }
        }

        internal void ForceTransformAtDefaultAngle(Vector3 targetPos, float yEuler)
        {
            BoardSquare boardSquareSafe = Board.Get().GetSquareFromPos(targetPos.x, targetPos.z);
            targetPos.y = boardSquareSafe != null ? boardSquareSafe.height : Board.Get().BaselineHeight;
            m_targetPosition.EaseTo(targetPos, 1.0f / 60);
            CameraControls.Get().m_desiredRotationEulerAngles.y = yEuler;
            SetTargetObject(null, CameraManager.CameraTargetReason.ForcingTransform);
        }

        public void OnTransitionIn(CameraTransitionType type)
        {
            switch (type)
            {
                case CameraTransitionType.Cut:
                    return;
                case CameraTransitionType.Move:
                    if (CameraManager.Get().SecondsRemainingToPauseForUserControl > 0f)
                    {
                        Vector3 b = CalcZoomOffsetForActiveAnimatedActor(transform.rotation);
                        Vector3 startValue = transform.position - b;
                        startValue.y = Board.Get().BaselineHeight + m_targetObjectOffset.y;
                        m_targetPosition = new EasedOutVector3(startValue);
                    }

                    m_transitionInTimeLeft = m_transitionInTime;
                    m_transitionInPosition = transform.position;
                    m_transitionInRotation = transform.rotation;
                    m_transitionFOV = Camera.main.fieldOfView;
                    return;
            }
        }

        public void OnTransitionOut()
        {
            CameraControls.Get().m_desiredRotationEulerAngles = transform.rotation.eulerAngles;
            m_transitionInTimeLeft = 0f;
        }

        public void OnReconnect()
        {
            m_targetWithBoardSquareWasNeverSet = false;
        }

        private float GetMaxDistanceHorizontal()
        {
            return DebugParameters.Get() == null || !DebugParameters.Get().GetParameterAsBool("CameraFarZoom")
                ? m_maxHorzDist
                : m_maxDistFarHorz;
        }

        private float GetMaxDistanceVertical()
        {
            return DebugParameters.Get() != null && DebugParameters.Get().GetParameterAsBool("CameraFarZoom")
                ? m_maxDistFarVert
                : m_maxVertDist;
        }

        private float GetHorzOffsetForActiveActor()
        {
            return GameFlowData.Get() != null
                   && GameFlowData.Get().activeOwnedActorData != null
                   && GameFlowData.Get().activeOwnedActorData.GetActorModelData() != null
                ? GameFlowData.Get().activeOwnedActorData.GetActorModelData().GetCameraHorzOffset()
                : 1.5f;
        }

        private float CalcZoomParameter(double distance)
        {
            float maxDistanceHorizontal = GetMaxDistanceHorizontal();
            float horzOffsetForActiveActor = GetHorzOffsetForActiveActor();
            double num = maxDistanceHorizontal != horzOffsetForActiveActor
                ? (distance - horzOffsetForActiveActor) / (maxDistanceHorizontal - horzOffsetForActiveActor)
                : 0.0;
            return Mathf.Clamp((float)num, 0f, 1f);
        }

        private float CalcZoomParameterDelta(float distanceDelta)
        {
            return distanceDelta > 0f
                ? CalcZoomParameter(GetHorzOffsetForActiveActor() + distanceDelta)
                : -1f * (1f - CalcZoomParameter(GetMaxDistanceHorizontal() + distanceDelta));
        }

        private float GetZoomVertOffsetForActiveAnimatedActor(bool forceStandingOffset)
        {
            return GameFlowData.Get() != null
                   && GameFlowData.Get().activeOwnedActorData != null
                ? GameFlowData.Get().activeOwnedActorData.GetActorModelData().GetCameraVertOffset(forceStandingOffset)
                : 1.2f;
        }

        public Vector3 CalcZoomOffsetForActiveAnimatedActor(Quaternion camRotation)
        {
            float horzOffsetForActiveActor = GetHorzOffsetForActiveActor();
            Vector3 result = ExtractRotationY(camRotation) * -Vector3.forward;
            float zoom = (float)m_zoomParameter * (float)m_zoomParameterScale;
            float horzRange = GetMaxDistanceHorizontal() - horzOffsetForActiveActor;
            result *= m_horzCurve.Evaluate(zoom) * horzRange + horzOffsetForActiveActor;
            bool forceStandingOffset = zoom > m_minZoomParamForCoverZoomOffset;
            m_zoomVertOffsetForAnimatedActor.EaseTo(
                GetZoomVertOffsetForActiveAnimatedActor(forceStandingOffset),
                m_coverHeightEaseTime);
            horzRange = GetMaxDistanceVertical() - (float)m_zoomVertOffsetForAnimatedActor;
            result.y = m_vertCurve.Evaluate(zoom) * horzRange + (float)m_zoomVertOffsetForAnimatedActor;
            return result;
        }

        private float CalcZoomRotationX()
        {
            return Mathf.Atan(m_tiltCurve.Evaluate((float)m_zoomParameter * (float)m_zoomParameterScale))
                   * Mathf.Rad2Deg;
        }

        private float CalcFOV()
        {
            return m_fovCurve.Evaluate((float)m_zoomParameter * (float)m_zoomParameterScale) * 100f;
        }

        private static Quaternion ExtractRotationY(Quaternion q)
        {
            return Quaternion.Euler(new Vector3(0f, q.eulerAngles.y, 0f));
        }

        private void OnDrawGizmos()
        {
            if (CameraManager.ShouldDrawGizmosForCurrentCamera()
                && GameFlowData.Get() != null
                && GameFlowData.Get().gameState >= GameState.BothTeams_Decision)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(m_targetPosition, 0.5f);
            }
        }
    }
}