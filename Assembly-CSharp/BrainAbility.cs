// ROGUES
// SERVER
using System;
using System.Collections.Generic;

// added in rogues
#if SERVER
[Serializable]
public class BrainAbility
{
	public List<BrainTargetingCondition> m_targetingConditions;

	public bool m_includeSelf;

	public bool m_includeAllies;

	public bool m_includeEnemies = true;
}
#endif
