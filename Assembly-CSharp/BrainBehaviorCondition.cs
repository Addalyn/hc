// ROGUES
// SERVER
using System;

// added in rogues
#if SERVER
[Serializable]
public class BrainBehaviorCondition
{
	public BehaviorRequirementConditionType m_conditionType;
	public Condition m_conditional = Condition.EqualTo;
	public int m_conditionValue;
}
#endif