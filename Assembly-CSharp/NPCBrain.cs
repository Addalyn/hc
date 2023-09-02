using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCBrain : MonoBehaviour, IGameEventListener
{
	[Tooltip("Create new states in your scene, then add them to this list.")]
	public List<StateEntry> StateTable = new List<StateEntry>();
	[Tooltip("ID of the starting state for this FSM. NullStateID will use the first state in StateTable. Make sure StartingState exists in the StateTable")]
	public StateID StartingState;

	private GameObject m_allocatedStateTableParent;

	[HideInInspector]
	public FSMSystem fsm { get; private set; }
	[HideInInspector]
	public NPCBrain NextBrain { get; internal set; }

	private void Start()
	{
		if (GetComponent<BotController>() == null)
		{
			name += " [Prime]";
			enabled = false;
		}
	}

	public void OnDestroy()
	{
		GameEventManager.Get().RemoveAllListenersFrom(this);
		if (m_allocatedStateTableParent != null)
		{
			Destroy(m_allocatedStateTableParent);
			m_allocatedStateTableParent = null;
		}
		if (fsm != null)
		{
			fsm.DestroyAllStates();
		}
	}

	public bool CanTransistion(Transition trans)
	{
		return fsm != null && fsm.CanTransistion(trans);
	}

	public void OnGameEvent(GameEventManager.EventType eventType, GameEventManager.GameEventArgs args)
	{
		if (this != null && fsm != null && enabled)
		{
			fsm.OnGameEvent(eventType, args);
		}
	}

	public void SetTransition(Transition t)
	{
		fsm.PerformTransition(t, this);
	}

	public void SetPendingTransition(Transition t)
	{
		fsm.SetPendingTransition(t);
	}

	public Transition GetPendingTransition()
	{
		return fsm.GetPendingTransition();
	}

	public virtual NPCBrain Create(BotController bot, Transform destination)
	{
		return null;
	}

	public virtual IEnumerator DecideTurn()
	{
		yield break;
	}

	public virtual void SelectBotAbilityMods()
	{
		GetComponent<BotController>().SelectBotAbilityMods_Brainless();
	}

	public virtual void SelectBotCards()
	{
		GetComponent<BotController>().SelectBotCards_Brainless();
	}

	public IEnumerator FSMTakeTurn()
	{
		if (fsm != null)
		{
			yield return StartCoroutine(fsm.TakeTurn());
		}
		else
		{
			yield return StartCoroutine(DecideTurn());
		}
	}

	protected virtual void MakeFSM(NPCBrain brainInstance)
	{
	}
}
