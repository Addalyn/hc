using System.Collections.Generic;
using UnityEngine;

public class SenseiBideAttachedSequence : Sequence
{
	[Header("-- Whether switch between different levels after spawn --", order = 1)]
	[Separator("For Main Fx", true, order = 0)]
	public bool m_switchBetweenLevelsAfterSpawn = true;

	[Header("-- Vfx Prefabs, will use first as base, and rest used for subsequent levels --")]
	public List<GameObject> m_fxPrefabs;

	[Header("    For alternative version of base, if not advanced to higher levels will switch to this one")]
	public GameObject m_fxPrefabAfterFirstTurn;

	[JointPopup("Main FX attach joint")]
	public JointPopupProperty m_fxJoint;

	[Tooltip("Check if Fx Prefab should stay attached to the joint. If unchecked, the Fx Prefab will start with the joint position and rotation.")]
	public bool m_fxAttachToJoint;

	[AnimEventPicker]
	[Header("-- Anim Events --")]
	[Tooltip("Animation event (if any) to wait for before starting the sequence. Search project for EventObjects.")]
	public Object m_startEvent;

	[AnimEventPicker]
	[Tooltip("Animation event (if any) to wait for before stopping the sequence. Search project for EventObjects.")]
	public Object m_stopEvent;

	[Header("-- Spawn Delay (ignored if there is Start Event) --")]
	public float m_startDelayTime;

	private float m_timeToSpawnVfx = -1f;

	public bool m_useRootOrientation;

	[AudioEvent(false)]
	public string m_audioEvent;

	[Separator("For Impact Fx", true)]
	public GameObject m_hitFxPrefab;

	[JointPopup("hit FX attach joint")]
	public JointPopupProperty m_hitFxJoint;

	public bool m_hitAlignedWithCaster;

	[AnimEventPicker]
	public Object m_hitEvent;

	public float m_hitDelay;

	[Header("-- Team restrictions for Hit VFX on Targets --")]
	public HitVFXSpawnTeam m_hitVfxSpawnTeamMode;

	[AudioEvent(false)]
	public string m_hitAudioEvent;

	public PhaseTimingParameters m_phaseTimingParameters;

	private List<GameObject> m_fxInstances;

	private GameObject m_alternateBaseFxInstance;

	private List<GameObject> m_hitFxInstances;

	private float m_hitSpawnTime = -1f;

	private bool m_attemptedToSpawnHitFx;

	private Sensei_SyncComponent m_syncComp;

	private int m_lastActiveIndex = -1;

	internal override void Initialize(IExtraSequenceParams[] extraParams)
	{
		ActorData actorData = null;
		foreach (IExtraSequenceParams extraSequenceParams in extraParams)
		{
			OverridePhaseTimingParams(m_phaseTimingParameters, extraSequenceParams);
			if (extraSequenceParams is ActorIndexExtraParam)
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
				if (GameFlowData.Get() != null)
				{
					ActorIndexExtraParam actorIndexExtraParam = extraSequenceParams as ActorIndexExtraParam;
					int actorIndex = actorIndexExtraParam.m_actorIndex;
					actorData = GameFlowData.Get().FindActorByActorIndex(actorIndex);
				}
			}
		}
		while (true)
		{
			switch (7)
			{
			case 0:
				continue;
			}
			if (actorData != null)
			{
				while (true)
				{
					switch (2)
					{
					case 0:
						break;
					default:
						m_syncComp = actorData.GetComponent<Sensei_SyncComponent>();
						return;
					}
				}
			}
			if (Application.isEditor)
			{
				while (true)
				{
					switch (2)
					{
					case 0:
						continue;
					}
					Debug.LogError("Did not find Sensei for sensei ult sequence");
					return;
				}
			}
			return;
		}
	}

	public int GetCurrnetFxIndex()
	{
		if (m_syncComp != null && m_fxPrefabs.Count > 1)
		{
			while (true)
			{
				switch (7)
				{
				case 0:
					break;
				default:
				{
					if (1 == 0)
					{
						/*OpCode not supported: LdMemberToken*/;
					}
					int num = m_fxPrefabs.Count - 1;
					float syncBideExtraDamagePct = m_syncComp.m_syncBideExtraDamagePct;
					if (syncBideExtraDamagePct >= 0.99f)
					{
						while (true)
						{
							switch (4)
							{
							case 0:
								break;
							default:
								return num;
							}
						}
					}
					float num2 = 1f / (float)num;
					return Mathf.FloorToInt(syncBideExtraDamagePct / num2);
				}
				}
			}
		}
		return 0;
	}

	public override void FinishSetup()
	{
		if (!(m_startEvent == null))
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
			if (!m_phaseTimingParameters.ShouldSequenceBeActive())
			{
				return;
			}
			if (m_startDelayTime <= 0f)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						break;
					default:
						SpawnFX();
						return;
					}
				}
			}
			m_timeToSpawnVfx = GameTime.time + m_startDelayTime;
			return;
		}
	}

	private bool IsHitFXVisibleForActor(ActorData hitTarget)
	{
		return IsHitFXVisibleWrtTeamFilter(hitTarget, m_hitVfxSpawnTeamMode);
	}

	internal override void OnTurnStart(int currentTurn)
	{
		m_phaseTimingParameters.OnTurnStart(currentTurn);
	}

	internal override void OnAbilityPhaseStart(AbilityPriority abilityPhase)
	{
		m_phaseTimingParameters.OnAbilityPhaseStart(abilityPhase);
		if (m_startEvent == null)
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
			if (m_phaseTimingParameters.ShouldSpawnSequence(abilityPhase))
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
				if (m_phaseTimingParameters.ShouldSequenceBeActive())
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
					SpawnFX();
				}
			}
		}
		if (!m_phaseTimingParameters.ShouldStopSequence(abilityPhase))
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
			if (m_fxInstances != null)
			{
				while (true)
				{
					switch (7)
					{
					case 0:
						continue;
					}
					StopFX();
					return;
				}
			}
			return;
		}
	}

	private void Update()
	{
		if (!m_initialized)
		{
			return;
		}
		if (m_timeToSpawnVfx > 0f && GameTime.time >= m_timeToSpawnVfx)
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
			m_timeToSpawnVfx = -1f;
			SpawnFX();
		}
		if (m_hitSpawnTime > 0f)
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
			if (GameTime.time > m_hitSpawnTime)
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
				SpawnHitFX();
				m_hitSpawnTime = -1f;
			}
		}
		int num = GetCurrnetFxIndex();
		if (m_switchBetweenLevelsAfterSpawn)
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
			if (m_fxInstances != null)
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
				if (m_fxInstances.Count > 1)
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
					int count = m_fxInstances.Count;
					num = Mathf.Min(num, count - 1);
					if (num != m_lastActiveIndex)
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
						if (m_lastActiveIndex >= 0)
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
							if (m_lastActiveIndex < count)
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
								if (m_fxInstances[m_lastActiveIndex] != null)
								{
									m_fxInstances[m_lastActiveIndex].SetActive(false);
								}
							}
						}
						if (num <= 0)
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
							if (!(m_alternateBaseFxInstance == null))
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
								if (base.AgeInTurns > 0)
								{
									goto IL_01c5;
								}
							}
						}
						if (m_fxInstances[num] != null)
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
							m_fxInstances[num].SetActive(true);
						}
						goto IL_01c5;
					}
				}
			}
		}
		goto IL_0205;
		IL_032a:
		if (m_fxInstances == null)
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
			for (int i = 0; i < m_fxInstances.Count; i++)
			{
				GameObject gameObject = m_fxInstances[i];
				if (!(gameObject != null))
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
				if (gameObject.GetComponent<FriendlyEnemyVFXSelector>() != null && base.Caster != null)
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
					gameObject.GetComponent<FriendlyEnemyVFXSelector>().Setup(base.Caster.GetTeam());
				}
				if (!m_useRootOrientation)
				{
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
				if (base.Caster != null)
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
					gameObject.transform.rotation = base.Caster.transform.rotation;
				}
			}
			return;
		}
		IL_0205:
		if (num == 0)
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
			if (base.AgeInTurns > 0 && m_alternateBaseFxInstance != null)
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
				if (!m_alternateBaseFxInstance.activeSelf)
				{
					m_alternateBaseFxInstance.SetActive(true);
				}
				if (m_fxInstances != null && m_fxInstances.Count > 0)
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
					if (m_fxInstances[0].activeSelf)
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
						m_fxInstances[0].SetActive(false);
					}
				}
			}
		}
		if (m_fxInstances != null && m_fxAttachToJoint)
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
			if (m_fxJoint.IsInitialized())
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
				if (base.Caster != null)
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
					if (ShouldHideForActorIfAttached(base.Caster))
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
						SetSequenceVisibility(false);
						goto IL_032a;
					}
				}
			}
		}
		ProcessSequenceVisibility();
		goto IL_032a;
		IL_01c5:
		Debug.LogWarning("Setting index from " + m_lastActiveIndex + " to " + num);
		m_lastActiveIndex = num;
		goto IL_0205;
	}

	private void StopFX()
	{
		if (m_fxInstances == null)
		{
			return;
		}
		for (int i = 0; i < m_fxInstances.Count; i++)
		{
			GameObject gameObject = m_fxInstances[i];
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
		}
	}

	private void SpawnFX()
	{
		if (base.Caster != null)
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
			if (!m_fxJoint.IsInitialized())
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
				m_fxJoint.Initialize(base.Caster.gameObject);
			}
			if (m_fxPrefabs != null)
			{
				m_fxInstances = new List<GameObject>();
				List<GameObject> list = m_fxPrefabs;
				int currnetFxIndex = GetCurrnetFxIndex();
				currnetFxIndex = Mathf.Clamp(currnetFxIndex, 0, m_fxPrefabs.Count - 1);
				if (!m_switchBetweenLevelsAfterSpawn)
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
					if (m_fxPrefabs.Count > 1)
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
						GameObject item = m_fxPrefabs[currnetFxIndex];
						List<GameObject> list2 = new List<GameObject>();
						list2.Add(item);
						list = list2;
					}
				}
				for (int i = 0; i < list.Count; i++)
				{
					GameObject fxPrefab = list[i];
					GameObject gameObject = InstantiateAttachedFx(fxPrefab);
					if (m_switchBetweenLevelsAfterSpawn)
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
						gameObject.SetActive(i == currnetFxIndex);
						if (i == currnetFxIndex)
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
							m_lastActiveIndex = i;
						}
					}
					m_fxInstances.Add(gameObject);
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
				if (m_fxPrefabAfterFirstTurn != null)
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
					m_alternateBaseFxInstance = InstantiateAttachedFx(m_fxPrefabAfterFirstTurn);
					m_alternateBaseFxInstance.SetActive(false);
				}
			}
			if (!string.IsNullOrEmpty(m_audioEvent))
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
				AudioManager.PostEvent(m_audioEvent, base.Caster.gameObject);
			}
		}
		if (!(m_hitSpawnTime < 0f))
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
			if (m_attemptedToSpawnHitFx)
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
				if (m_hitEvent == null)
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
					if (m_hitDelay <= 0f)
					{
						while (true)
						{
							switch (2)
							{
							case 0:
								break;
							default:
								m_hitSpawnTime = GameTime.time;
								return;
							}
						}
					}
				}
				m_hitSpawnTime = GameTime.time + m_hitDelay;
				return;
			}
		}
	}

	private GameObject InstantiateAttachedFx(GameObject fxPrefab)
	{
		GameObject gameObject = null;
		if (m_fxJoint.m_jointObject != null)
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
			if (m_fxJoint.m_jointObject.transform.localScale != Vector3.zero && m_fxAttachToJoint)
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
				gameObject = InstantiateFX(fxPrefab);
				AttachToBone(gameObject, m_fxJoint.m_jointObject);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				goto IL_0111;
			}
		}
		Vector3 position = m_fxJoint.m_jointObject.transform.position;
		Quaternion quaternion = default(Quaternion);
		quaternion = m_fxJoint.m_jointObject.transform.rotation;
		gameObject = InstantiateFX(fxPrefab, position, quaternion);
		Sequence.SetAttribute(gameObject, "abilityAreaLength", (base.TargetPos - position).magnitude);
		goto IL_0111;
		IL_0111:
		Sequence.SetAttribute(gameObject, "targetDiameter", base.Caster.GetActorModelData().GetModelSize());
		return gameObject;
	}

	private void SpawnHitFX()
	{
		if (!m_attemptedToSpawnHitFx)
		{
			if (m_hitFxInstances == null)
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
				if (1 == 0)
				{
					/*OpCode not supported: LdMemberToken*/;
				}
				m_hitFxInstances = new List<GameObject>();
			}
			if (base.Targets != null)
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
				for (int i = 0; i < base.Targets.Length; i++)
				{
					Vector3 targetHitPosition = GetTargetHitPosition(i, m_hitFxJoint);
					Vector3 position = base.Caster.transform.position;
					if ((position - base.Targets[i].transform.position).magnitude < 0.1f)
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
						position -= base.Caster.transform.forward * 0.5f;
					}
					Vector3 vector = targetHitPosition - position;
					vector.y = 0f;
					vector.Normalize();
					ActorModelData.ImpulseInfo impulseInfo = new ActorModelData.ImpulseInfo(targetHitPosition, vector);
					Quaternion quaternion;
					if (m_hitAlignedWithCaster)
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
						quaternion = Quaternion.LookRotation(vector);
					}
					else
					{
						quaternion = Quaternion.identity;
					}
					Quaternion rotation = quaternion;
					bool flag = IsHitFXVisibleForActor(base.Targets[i]);
					if ((bool)m_hitFxPrefab)
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
						if (flag)
						{
							m_hitFxInstances.Add(InstantiateFX(m_hitFxPrefab, targetHitPosition, rotation));
						}
					}
					if (flag)
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
						string hitAudioEvent = m_hitAudioEvent;
						if (!string.IsNullOrEmpty(hitAudioEvent))
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
							AudioManager.PostEvent(hitAudioEvent, base.Targets[i].gameObject);
						}
					}
					if (base.Targets[i] != null)
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
						base.Source.OnSequenceHit(this, base.Targets[i], impulseInfo);
					}
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
			base.Source.OnSequenceHit(this, base.TargetPos);
		}
		m_attemptedToSpawnHitFx = true;
	}

	protected override void OnAnimationEvent(Object parameter, GameObject sourceObject)
	{
		if (!m_phaseTimingParameters.ShouldSequenceBeActive())
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
			if (m_startEvent == parameter)
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
				SpawnFX();
			}
			else if (m_stopEvent == parameter)
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
				StopFX();
			}
			if (m_hitEvent == parameter)
			{
				SpawnHitFX();
			}
			return;
		}
	}

	private void OnDisable()
	{
		if (m_fxInstances != null)
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
			for (int i = 0; i < m_fxInstances.Count; i++)
			{
				GameObject gameObject = m_fxInstances[i];
				if (gameObject != null)
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
					Object.Destroy(gameObject.gameObject);
				}
			}
			m_fxInstances.Clear();
		}
		if (m_alternateBaseFxInstance != null)
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
			Object.Destroy(m_alternateBaseFxInstance);
			m_alternateBaseFxInstance = null;
		}
		if (m_hitFxInstances == null)
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
			for (int j = 0; j < m_hitFxInstances.Count; j++)
			{
				GameObject gameObject2 = m_hitFxInstances[j];
				if (gameObject2 != null)
				{
					Object.Destroy(gameObject2.gameObject);
				}
			}
			while (true)
			{
				switch (7)
				{
				case 0:
					continue;
				}
				m_hitFxInstances.Clear();
				return;
			}
		}
	}
}
