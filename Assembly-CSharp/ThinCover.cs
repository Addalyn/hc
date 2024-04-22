using System;
using UnityEngine;

public class ThinCover : MonoBehaviour, IGameEventListener
{
	public enum CoverType
	{
		None,
		Half,
		Full
	}

	public CoverType m_coverType;

	private void Awake()
	{
		GameEventManager.Get().AddListener(this, GameEventManager.EventType.GameFlowDataStarted);
	}

	private void OnDestroy()
	{
		GameEventManager.Get().RemoveListener(this, GameEventManager.EventType.GameFlowDataStarted);
	}

	void IGameEventListener.OnGameEvent(GameEventManager.EventType eventType, GameEventManager.GameEventArgs args)
	{
		if (eventType != GameEventManager.EventType.GameFlowDataStarted)
		{
			return;
		}
		if (transform == null)
		{
			Debug.LogError("ThinCover recieving GameFlowDataStarted game event, but its transform is null.");
			return;
		}
		if (GameFlowData.Get() == null)
		{
			Debug.LogError("ThinCover recieving GameFlowDataStarted game event, but GameFlowData is null.");
			return;
		}
		if (GameFlowData.Get().GetThinCoverRoot() == null)
		{
			Debug.LogError("ThinCover recieving GameFlowDataStarted game event, but GameFlowData's ThinCoverRoot is null.");
			return;
		}
		try
		{
			transform.parent = GameFlowData.Get().GetThinCoverRoot().transform;
			UpdateBoardSquare();
		}
		catch (NullReferenceException)
		{
			Debug.LogError("Caught System.NullReferenceException for ThinCover receiving GameFlowDataStarted game event.  Highly unexpected!");
		}
	}

	private void UpdateBoardSquare()
	{
		Vector3 position = transform.position;
		float squareSize = Board.Get().squareSize;
		float xFloat = position.x / squareSize;
		float yFloat = position.z / squareSize;
		int x = Mathf.RoundToInt(xFloat);
		int y = Mathf.RoundToInt(yFloat);
		float xRemainder = xFloat - x;
		float yRemainder = yFloat - y;
		Board board = Board.Get();
		if (Mathf.Abs(xRemainder) > Mathf.Abs(yRemainder))
		{
			if (xRemainder > 0f)
			{
				board.SetThinCover(x, y, ActorCover.CoverDirections.X_POS, m_coverType);
				if (y + 1 < board.GetMaxY())
				{
					board.SetThinCover(x + 1, y, ActorCover.CoverDirections.X_NEG, m_coverType);
				}
			}
			else
			{
				board.SetThinCover(x, y, ActorCover.CoverDirections.X_NEG, m_coverType);
				if (x - 1 >= 0)
				{
					board.SetThinCover(x - 1, y, ActorCover.CoverDirections.X_POS, m_coverType);
				}
			}
		}
		else if (yRemainder > 0f)
		{
			board.SetThinCover(x, y, ActorCover.CoverDirections.Y_POS, m_coverType);
			if (y + 1 < board.GetMaxY())
			{
				board.SetThinCover(x, y + 1, ActorCover.CoverDirections.Y_NEG, m_coverType);
			}
		}
		else
		{
			board.SetThinCover(x, y, ActorCover.CoverDirections.Y_NEG, m_coverType);
			if (y - 1 >= 0)
			{
				board.SetThinCover(x, y - 1, ActorCover.CoverDirections.Y_POS, m_coverType);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (!CameraManager.ShouldDrawGizmosForCurrentCamera())
		{
			return;
		}
		Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.localRotation, Vector3.one);
		Gizmos.DrawWireCube(
			Vector3.zero,
			m_coverType == CoverType.Half
				? new Vector3(1.5f, 1f, 0.1f)
				: new Vector3(1.5f, 2f, 0.1f));
		Gizmos.matrix = Matrix4x4.identity;
		Gizmos.DrawIcon(
			transform.position,
			m_coverType == CoverType.Half
				? "icon_HalfCover.png"
				: "icon_FullCover.png");
	}
}
