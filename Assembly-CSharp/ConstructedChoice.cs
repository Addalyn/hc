// ROGUES
// SERVER
using System.Collections.Generic;

// added in rogues
#if SERVER
public class ConstructedChoice : NPCBrain_Adaptive.PotentialChoice
{
	public int lowestTargetHP;
	public int highestTargetHP;
	public int bestHitChance;
	public int bestCritChance;
	public List<int> statusesOnSelf;
	public List<int> statusesOnTargets;
	public int lethalCount;
	public int distanceToNearestTarget;
	public int distanceToFarthestTarget;

	public override string ToString()
	{
		string text = string.Empty;
		if (!targetList.IsNullOrEmpty())
		{
			text += "targetList: ";
			foreach (AbilityTarget abilityTarget in targetList)
			{
				text = text + abilityTarget.GetDebugString() + "\n";
			}
		}
		if (damageTotal > 0)
		{
			text = string.Concat(text, "dmg ", damageTotal, "\n");
		}
		if (healingTotal > 0)
		{
			text = string.Concat(text, "heal ", healingTotal, "\n");
		}
		if (lethalCount > 0)
		{
			text = string.Concat(text, "kills ", lethalCount, "\n");
		}
		return text;
	}
}
#endif
