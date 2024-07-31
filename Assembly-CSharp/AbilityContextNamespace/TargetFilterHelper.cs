// ROGUES
// SERVER
using System.Collections.Generic;
using UnityEngine;

namespace AbilityContextNamespace
{
    public class TargetFilterHelper
    {
        public static bool ActorMeetsConditions(
            TargetFilterConditions filters,
            ActorData targetActor,
            ActorData caster,
            ActorHitContext actorHitContext,
            ContextVars abilityContext)
        {
            return targetActor != null
                   && caster != null
                   && PassesTeamFilter(filters.m_teamFilter, targetActor, caster)
                   && PassContextCompareFilters(filters.m_numCompareConditions, actorHitContext, abilityContext);
        }

        public static bool PassesTeamFilter(TeamFilter teamFilter, ActorData targetActor, ActorData caster)
        {
            if (targetActor != null && caster != null)
            {
                bool isAlly = targetActor.GetTeam() == caster.GetTeam();
                bool isSelf = targetActor == caster;
                return teamFilter == TeamFilter.Any
                       // reactor
                       || teamFilter == TeamFilter.EnemyIncludingTarget && !isAlly
                       // rogues
                       // || (teamFilter == TeamFilter.EnemyIncludingTarget || teamFilter == TeamFilter.EnemyExcludingTarget) && !isAlly
                       || teamFilter == TeamFilter.AllyIncludingSelf && isAlly
                       || teamFilter == TeamFilter.AllyExcludingSelf && isAlly && !isSelf
                       || teamFilter == TeamFilter.SelfOnly && isSelf;
            }

            return false;
        }

        // inlined in reactor
        private static bool TestCompareCondition(ContextCompareOp op, float testValue, float contextValue)
        {
            return (op == ContextCompareOp.Equals && testValue == contextValue)
                   || (op == ContextCompareOp.EqualsRoundToInt && Mathf.RoundToInt(testValue) == Mathf.RoundToInt(contextValue))
                   || (op == ContextCompareOp.GreaterThan && contextValue > testValue)
                   || (op == ContextCompareOp.GreaterThanOrEqual && contextValue >= testValue)
                   || (op == ContextCompareOp.LessThan && contextValue < testValue)
                   || (op == ContextCompareOp.LessThanOrEqual && contextValue <= testValue);
        }

        public static bool PassContextCompareFilters(
            List<NumericContextValueCompareCond> conditions,
            ActorHitContext actorHitContext,
            ContextVars abilityContext)
        {
            bool result = true;
            if (conditions == null)
            {
                return true;
            }

            foreach (NumericContextValueCompareCond condition in conditions)
            {
                if (!result)
                {
                    return false;
                }

                if (condition.m_compareOp == ContextCompareOp.Ignore
                    || string.IsNullOrEmpty(condition.m_contextName))
                {
                    continue;
                }

                int contextKey = condition.GetContextKey();

                ContextVars contextVars = actorHitContext?.m_contextVars; // NOTE CHANGE null check added in rogues
                
                // reactor
                if (condition.m_nonActorSpecificContext)
                {
                    contextVars = abilityContext;
                }
                // rogues
                // if (ContextKeys.IsNonActorSpecific(contextKey))
                // {
                //     contextVars = abilityContext;
                // }

                float actualValue = 0f;
                bool isValuePresent = false;
                
                if (contextVars == null) // NOTE CHANGE null check added in rogues
                {
                    continue;
                }
                
                // rogues
                // if (condition.GetContextKey() == ContextKeys.s_TargeterIndex.GetKey())
                // {
                //     foreach (int num2 in actorHitContext.m_targeterIndices)
                //     {
                //         if (TestCompareCondition(
                //                 condition.m_compareOp,
                //                 condition.m_testValue,
                //                 num2))
                //         {
                //             return true;
                //         }
                //     }
                // }
                // else if (condition.GetContextKey() == ContextKeys.s_SegmentID.GetKey())
                // {
                //     foreach (int num3 in actorHitContext.m_segmentIndices)
                //     {
                //         if (TestCompareCondition(
                //                 condition.m_compareOp,
                //                 condition.m_testValue,
                //                 num3))
                //         {
                //             return true;
                //         }
                //     }
                // }
                // else
                if (contextVars.HasVar(contextKey, ContextValueType.Int))
                {
                    actualValue = contextVars.GetValueInt(contextKey);
                    isValuePresent = true;
                }
                else if (contextVars.HasVar(contextKey, ContextValueType.Float))
                {
                    actualValue = contextVars.GetValueFloat(contextKey);
                    isValuePresent = true;
                }

                float testValue = condition.m_testValue;
                ContextCompareOp compareOp = condition.m_compareOp;
                if (isValuePresent && !TestCompareCondition(compareOp, testValue, actualValue))
                {
                    result = false;
                }

                // reactor
                if (!isValuePresent && !condition.m_ignoreIfNoContext)
                // rogues
                // if (!isValuePresent)
                {
                    result = false;
                }
            }

            return result;
        }
    }
}