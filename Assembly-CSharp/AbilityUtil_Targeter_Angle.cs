using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUtil_Targeter_Angle : AbilityUtil_Targeter
{
	private struct AngleEntry
	{
		public float angle;

		public Vector3 vec;
	}

	public float m_width = 1f;

	public float m_distance = 15f;

	public bool m_penetrateLoS;

	public bool m_affectsCaster;

	public int m_maxTargets = -1;

	private static float s_nearAcceleratingMinSpeed = 2f;

	private static float s_farAcceleratingMinSpeed = 4f;

	private static float s_brakeMinSpeed = 0.5f;

	private static float s_nearAcceleratingMaxSpeed = 4f;

	private static float s_farAcceleratingMaxSpeed = 120f;

	private static float s_brakeMaxSpeed = 2f;

	private static float s_nearAcceleration = 40f;

	private static float s_farAcceleration = 600f;

	private static float s_brakeAcceleration = 6f;

	private static float s_brakeDistance = 0.05f;

	private static float s_farDistance = 0.5f;

	private float m_curSpeed;

	private float m_curAngle;

	private static int s_numAngles = 32;

	private static List<AngleEntry> s_angles;

	public AbilityUtil_Targeter_Angle(Ability ability, float width, float distance, bool penetrateLoS, int maxTargets = -1, bool affectsAllies = false, bool affectsCaster = false)
		: base(ability)
	{
		InitAngles();
		m_width = width;
		m_distance = distance;
		m_penetrateLoS = penetrateLoS;
		m_maxTargets = maxTargets;
		m_affectsAllies = affectsAllies;
		m_affectsCaster = affectsCaster;
		m_curAngle = float.MinValue;
	}

	private static void InitAngles()
	{
		if (s_angles == null)
		{
			s_angles = new List<AngleEntry>(s_numAngles);
			float num = s_numAngles;
			for (int i = 0; i < s_numAngles; i++)
			{
				float angle = (float)i * ((float)Math.PI * 2f) / num;
				Vector3 vec = VectorUtils.AngleRadToVector(angle);
				vec.Normalize();
				AngleEntry item = default(AngleEntry);
				item.angle = angle;
				item.vec = vec;
				s_angles.Add(item);
			}
		}
	}

	private static Vector3 GetValidDirOf(Vector3 freeDir)
	{
		float num = VectorUtils.HorizontalAngle_Rad(freeDir);
		int num2 = Mathf.RoundToInt(num * (float)s_numAngles / ((float)Math.PI * 2f));
		while (true)
		{
			if (num2 < s_numAngles)
			{
				if (num2 >= 0)
				{
					break;
				}
			}
			num2 = ((num2 < s_numAngles) ? (num2 + s_numAngles) : (num2 - s_numAngles));
		}
		AngleEntry angleEntry = s_angles[num2];
		return angleEntry.vec;
	}

	public float RotateHighlightTowards(float goalAngle)
	{
		if (m_curAngle != goalAngle)
		{
			if (m_curAngle != float.MinValue)
			{
				float deltaTime = Time.deltaTime;
				float num = Mathf.Abs(goalAngle - m_curAngle);
				if (num > (float)Math.PI)
				{
					if (m_curAngle > goalAngle)
					{
						m_curAngle -= (float)Math.PI * 2f;
					}
					else
					{
						m_curAngle += (float)Math.PI * 2f;
					}
					num = Mathf.Abs(goalAngle - m_curAngle);
				}
				bool flag = num >= s_farDistance;
				bool flag2 = num <= s_brakeDistance;
				float num2;
				float min;
				float max;
				if (flag)
				{
					num2 = s_farAcceleration;
					min = s_farAcceleratingMinSpeed;
					max = s_farAcceleratingMaxSpeed;
				}
				else if (!flag2)
				{
					num2 = s_nearAcceleration;
					min = s_nearAcceleratingMinSpeed;
					max = s_nearAcceleratingMaxSpeed;
				}
				else
				{
					num2 = 0f - s_brakeAcceleration;
					min = s_brakeMinSpeed;
					max = s_brakeMaxSpeed;
				}
				float value = m_curSpeed + num2 * deltaTime;
				value = Mathf.Clamp(value, min, max);
				float num3 = value * deltaTime;
				float num4 = (goalAngle - m_curAngle) / num;
				if (num3 >= num)
				{
					while (true)
					{
						switch (5)
						{
						case 0:
							break;
						default:
							m_curSpeed = s_nearAcceleratingMinSpeed;
							return goalAngle;
						}
					}
				}
				m_curSpeed = value;
				return m_curAngle + num4 * num3;
			}
		}
		m_curSpeed = s_nearAcceleratingMinSpeed;
		return goalAngle;
	}

	public VectorUtils.LaserCoords CurrentLaserCoordinates(AbilityTarget currentTarget, ActorData targetingActor)
	{
		Vector3 travelBoardSquareWorldPositionForLos = targetingActor.GetLoSCheckPos();
		Vector3 vector;
		if (currentTarget == null)
		{
			vector = targetingActor.transform.forward;
		}
		else
		{
			vector = currentTarget.AimDirection;
		}
		Vector3 freeDir = vector;
		Vector3 validDirOf = GetValidDirOf(freeDir);
		float maxDistanceInWorld = m_distance * Board.Get().squareSize;
		float widthInWorld = m_width * Board.Get().squareSize;
		return VectorUtils.GetLaserCoordinates(travelBoardSquareWorldPositionForLos, validDirOf, maxDistanceInWorld, widthInWorld, m_penetrateLoS, targetingActor);
	}

	public override void UpdateTargeting(AbilityTarget currentTarget, ActorData targetingActor)
	{
		VectorUtils.LaserCoords coords = CurrentLaserCoordinates(currentTarget, targetingActor);
		float widthInWorld = m_width * Board.Get().squareSize;
		ClearActorsInRange();
		List<ActorData> actors = AreaEffectUtils.GetActorsInBox(coords.start, coords.end, m_width, m_penetrateLoS, targetingActor, GetAffectedTeams());
		TargeterUtils.RemoveActorsInvisibleToClient(ref actors);
		if (actors.Contains(targetingActor))
		{
			actors.Remove(targetingActor);
		}
		VectorUtils.LaserCoords points = TargeterUtils.TrimTargetsAndGetLaserCoordsToFarthestTarget(ref actors, m_maxTargets, coords);
		float magnitude = (points.end - points.start).magnitude;
		UpdateHighlight(widthInWorld, magnitude, points);
		using (List<ActorData>.Enumerator enumerator = actors.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ActorData current = enumerator.Current;
				AddActorInRange(current, points.start, targetingActor);
			}
		}
		if (!m_affectsCaster)
		{
			return;
		}
		while (true)
		{
			AddActorInRange(targetingActor, points.start, targetingActor);
			return;
		}
	}

	private void UpdateHighlight(float widthInWorld, float cursorLength, VectorUtils.LaserCoords points)
	{
		float y = 0.1f - BoardSquare.s_LoSHeightOffset;
		if (base.Highlight == null)
		{
			base.Highlight = HighlightUtils.Get().CreateRectangularCursor(widthInWorld, cursorLength);
		}
		else
		{
			HighlightUtils.Get().ResizeRectangularCursor(widthInWorld, cursorLength, base.Highlight);
		}
		Vector3 normalized = (points.end - points.start).normalized;
		base.Highlight.transform.position = points.start + new Vector3(0f, y, 0f);
		float goalAngle = VectorUtils.HorizontalAngle_Rad(normalized);
		m_curAngle = RotateHighlightTowards(goalAngle);
		Vector3 forward = VectorUtils.AngleRadToVector(m_curAngle);
		base.Highlight.transform.rotation = Quaternion.LookRotation(forward);
	}

	public override void DrawGizmos(AbilityTarget currentTarget, ActorData targetingActor)
	{
		VectorUtils.LaserCoords laserCoords = CurrentLaserCoordinates(currentTarget, targetingActor);
		float num = m_width * Board.Get().squareSize;
		float y = 0.1f - BoardSquare.s_LoSHeightOffset;
		Vector3 vector = laserCoords.start + new Vector3(0f, y, 0f);
		Vector3 vector2 = laserCoords.end + new Vector3(0f, y, 0f);
		Vector3 vector3 = vector2 - vector;
		float magnitude = vector3.magnitude;
		vector3.Normalize();
		Vector3 a = Vector3.Cross(vector3, Vector3.up);
		Vector3 a2 = (vector + vector2) * 0.5f;
		Vector3 b = a * (num / 2f);
		Vector3 b2 = vector3 * (magnitude / 2f);
		Vector3 vector4 = a2 - b - b2;
		Vector3 vector5 = a2 - b + b2;
		Vector3 vector6 = a2 + b - b2;
		Vector3 vector7 = a2 + b + b2;
		Gizmos.color = Color.red;
		Gizmos.DrawLine(vector4, vector5);
		Gizmos.DrawLine(vector5, vector7);
		Gizmos.DrawLine(vector7, vector6);
		Gizmos.DrawLine(vector6, vector4);
		Gizmos.color = Color.yellow;
		List<BoardSquare> squaresInBox = AreaEffectUtils.GetSquaresInBox(vector, vector2, m_width / 2f, m_penetrateLoS, targetingActor);
		using (List<BoardSquare>.Enumerator enumerator = squaresInBox.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				BoardSquare current = enumerator.Current;
				if (current.IsValidForGameplay())
				{
					Gizmos.DrawWireCube(current.ToVector3(), new Vector3(0.05f, 0.1f, 0.05f));
				}
			}
			while (true)
			{
				switch (6)
				{
				default:
					return;
				case 0:
					break;
				}
			}
		}
	}
}
