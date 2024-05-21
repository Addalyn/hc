using System.Collections.Generic;
using UnityEngine;

public class TargetSelect_ChargeSingleStep : GenericAbility_TargetSelectBase
{
    [Separator("Targeting Properties")]
    public AbilityAreaShape m_destShape;
    [Separator("Sequences")]
    public GameObject m_castSequencePrefab;

    private TargetSelectMod_ChargeSingleStep m_targetSelMod;

    public override string GetUsageForEditor()
    {
        return "Intended for single click charge abilities. Can add shape field to hit targets on destination.";
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        return new List<AbilityUtil_Targeter>
        {
            new AbilityUtil_Targeter_Charge(
                ability,
                GetDestShape(),
                IgnoreLos(),
                AbilityUtil_Targeter_Shape.DamageOriginType.CenterOfShape,
                IncludeEnemies(),
                IncludeAllies())
        };
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_ChargeSingleStep;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }

    public AbilityAreaShape GetDestShape()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_destShapeMod.GetModifiedValue(m_destShape)
            : m_destShape;
    }

    public override bool HandleCustomTargetValidation(
        Ability ability,
        ActorData caster,
        AbilityTarget target,
        int targetIndex,
        List<AbilityTarget> currentTargets)
    {
        BoardSquare targetSquare = Board.Get().GetSquare(target.GridPos);
        if (targetSquare != null
            && targetSquare.IsValidForGameplay()
            && targetSquare != caster.GetCurrentBoardSquare())
        {
            return KnockbackUtils.CanBuildStraightLineChargePath(
                caster,
                targetSquare,
                caster.GetCurrentBoardSquare(),
                false,
                out _);
        }

        return false;
    }

    public override ActorData.MovementType GetMovementType()
    {
        return ActorData.MovementType.Charge;
    }
}