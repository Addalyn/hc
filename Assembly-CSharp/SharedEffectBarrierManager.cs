using System.Collections.Generic;
using UnityEngine.Networking;

public class SharedEffectBarrierManager : NetworkBehaviour
{
	private enum DirtyBit : uint
	{
		EndedEffects = 1u,
		EndedBarriers = 2u,
		All = uint.MaxValue
	}

	public int m_numTurnsInMemory = 3;

	private List<int> m_endedEffectGuidsSync;

	private List<int> m_endedBarrierGuidsSync;

	private List<int> m_endedEffectTurnRanges;
	private List<int> m_endedBarrierTurnRanges;

	private static SharedEffectBarrierManager s_instance;

	private void Awake()
	{
		s_instance = this;
		m_endedEffectGuidsSync = new List<int>();
		m_endedBarrierGuidsSync = new List<int>();
		m_endedEffectTurnRanges = new List<int>(new int[m_numTurnsInMemory]);
		m_endedBarrierTurnRanges = new List<int>(new int[m_numTurnsInMemory]);
	}

	private void OnDestroy()
	{
		if (s_instance == this)
		{
			s_instance = null;
		}
	}

	public static SharedEffectBarrierManager Get()
	{
		return s_instance;
	}

	public void EndEffect(int effectGuid)
	{
		m_endedEffectGuidsSync.Add(effectGuid);
		SetDirtyBit(DirtyBit.EndedEffects);
	}

	public void EndBarrier(int barrierGuid)
	{
		m_endedBarrierGuidsSync.Add(barrierGuid);
		SetDirtyBit(DirtyBit.EndedBarriers);
	}

	public void UpdateTurn()
	{
		ShiftRange(m_endedBarrierGuidsSync, m_endedBarrierTurnRanges);
		ShiftRange(m_endedEffectGuidsSync, m_endedEffectTurnRanges);
	}

	private static void ShiftRange(List<int> guids, List<int> ranges)
	{
		int shift = ranges[0];
		for (int i = 0; i < ranges.Count - 1; ++i)
		{
			ranges[i] = ranges[i + 1] - shift;
		}
		guids.RemoveRange(0, shift);
		ranges[ranges.Count - 1] = guids.Count;
	}

	private void SetDirtyBit(DirtyBit bit)
	{
		SetDirtyBit((uint)bit);
	}

	private bool IsBitDirty(uint setBits, DirtyBit bitToTest)
	{
		return ((int)setBits & (int)bitToTest) != 0;
	}

	private void OnEndedEffectGuidsSync()
	{
		if (m_endedEffectGuidsSync.Count > 100)
		{
			Log.Error("Remembering more than 100 effects?");
		}
		if (ClientEffectBarrierManager.Get() != null)
		{
			for (int i = 0; i < m_endedEffectGuidsSync.Count; i++)
			{
				ClientEffectBarrierManager.Get().EndEffect(m_endedEffectGuidsSync[i]);
			}
		}
	}

	private void OnEndedBarrierGuidsSync()
	{
		if (m_endedBarrierGuidsSync.Count > 50)
		{
			Log.Error("Remembering more than 50 barriers?");
		}
		if (ClientEffectBarrierManager.Get() != null)
		{
			for (int i = 0; i < m_endedBarrierGuidsSync.Count; i++)
			{
				ClientEffectBarrierManager.Get().EndBarrier(m_endedBarrierGuidsSync[i]);
			}
		}	
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		if (!initialState)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		uint bitMask = (uint)(initialState ? -1 : (int)base.syncVarDirtyBits);
		if (IsBitDirty(bitMask, DirtyBit.EndedEffects))
		{
			short value = (short)m_endedEffectGuidsSync.Count;
			writer.Write(value);
			using (List<int>.Enumerator enumerator = m_endedEffectGuidsSync.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int current = enumerator.Current;
					writer.Write(current);
				}
			}
		}
		if (IsBitDirty(bitMask, DirtyBit.EndedBarriers))
		{
			short value2 = (short)m_endedBarrierGuidsSync.Count;
			writer.Write(value2);
			foreach (int item in m_endedBarrierGuidsSync)
			{
				writer.Write(item);
			}
		}
		return bitMask != 0;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		uint setBits = uint.MaxValue;
		if (!initialState)
		{
			setBits = reader.ReadPackedUInt32();
		}
		if (IsBitDirty(setBits, DirtyBit.EndedEffects))
		{
			m_endedEffectGuidsSync.Clear();
			short num = reader.ReadInt16();
			for (short num2 = 0; num2 < num; num2 = (short)(num2 + 1))
			{
				int item = reader.ReadInt32();
				m_endedEffectGuidsSync.Add(item);
			}
		}
		if (IsBitDirty(setBits, DirtyBit.EndedBarriers))
		{
			m_endedBarrierGuidsSync.Clear();
			short num3 = reader.ReadInt16();
			for (short num4 = 0; num4 < num3; num4 = (short)(num4 + 1))
			{
				int item2 = reader.ReadInt32();
				m_endedBarrierGuidsSync.Add(item2);
			}
		}
		OnEndedEffectGuidsSync();
		OnEndedBarrierGuidsSync();
	}

	private void UNetVersion()
	{
	}
}
