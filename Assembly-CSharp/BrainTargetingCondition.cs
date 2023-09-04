// ROGUES
// SERVER
using System;

// added in rogues
#if SERVER
[Serializable]
public class BrainTargetingCondition
{
	public TargetingPriority m_priority = TargetingPriority.Priority_Medium;

	public TargetingConditionType m_conditionType;

	public Condition m_conditional = Condition.EqualTo;

	public int m_conditionValue;
}
#endif
