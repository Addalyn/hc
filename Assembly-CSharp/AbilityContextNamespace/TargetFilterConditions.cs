// ROGUES
// SERVER
using System;
using System.Collections.Generic;

namespace AbilityContextNamespace
{
    [Serializable]
    public class TargetFilterConditions
    {
        public TeamFilter m_teamFilter;
        public List<NumericContextValueCompareCond> m_numCompareConditions = new List<NumericContextValueCompareCond>();

        public TargetFilterConditions GetCopy()
        {
            TargetFilterConditions targetFilterConditions = MemberwiseClone() as TargetFilterConditions;
            // ReSharper disable once PossibleNullReferenceException
            targetFilterConditions.m_numCompareConditions = new List<NumericContextValueCompareCond>();
            foreach (NumericContextValueCompareCond cond in m_numCompareConditions)
            {
                targetFilterConditions.m_numCompareConditions.Add(cond.GetCopy());
            }

            return targetFilterConditions;
        }

        public void AddTooltipTokens(List<TooltipTokenEntry> tokens, string prefix)
        {
        }

        public string GetInEditorDesc(string indent)
        {
            string desc = indent + "Team = " + m_teamFilter + "\n";
            if (m_numCompareConditions != null)
            {
                foreach (NumericContextValueCompareCond numCompareCondition in m_numCompareConditions)
                {
                    if (numCompareCondition.m_compareOp != ContextCompareOp.Ignore)
                    {
                        desc = string.Concat(
                            desc,
                            indent,
                            // reactor
                            InEditorDescHelper.ContextVarName(numCompareCondition.m_contextName, !numCompareCondition.m_nonActorSpecificContext),
                            // rogues
                            // InEditorDescHelper.ContextVarName(numCompareCondition.m_contextName),
                            " is ",
                            numCompareCondition.m_compareOp,
                            " ",
                            InEditorDescHelper.ColoredString(numCompareCondition.m_testValue),
                            "\n");
                    }
                }
            }

            return desc;
        }
    }
}