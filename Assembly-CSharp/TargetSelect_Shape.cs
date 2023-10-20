using System.Collections.Generic;
using AbilityContextNamespace;
using UnityEngine;

public class TargetSelect_Shape : GenericAbility_TargetSelectBase
{
    public delegate BoardSquare CenterSquareDelegate(AbilityTarget currentTarget, ActorData caster);
    public delegate bool IsMovingShapeDelegate(ActorData caster);
    public delegate BoardSquare GetMoveStartSquareDelegate(AbilityTarget currentTarget, ActorData caster);
    public delegate Vector3 GetMoveStartFreePosDelegate(AbilityTarget currentTarget, ActorData caster);

    [Separator("Targeting Properties")] public AbilityAreaShape m_shape = AbilityAreaShape.Three_x_Three;
    public List<AbilityAreaShape> m_additionalShapes = new List<AbilityAreaShape>();
    [Header("-- For require targeting on actors")]
    public bool m_requireTargetingOnActor;
    public bool m_canTargetOnEnemies = true;
    public bool m_canTargetOnAllies = true;
    public bool m_canTargetOnSelf = true;
    public bool m_ignoreLosToTargetActor;
    [Separator("Show targeter arc?")] public bool m_showTargeterArc;
    [Separator("Use Move Shape Targeter? (for moving a shape similar to Grey drone)")]
    public bool m_useMoveShapeTargeter;
    public float m_moveLineWidth = 1f;
    [Separator("Sequences")] public GameObject m_castSequencePrefab;

    public CenterSquareDelegate m_centerSquareDelegate;
    public IsMovingShapeDelegate m_isMovingShapeDelegate;
    public GetMoveStartSquareDelegate m_moveStartSquareDelegate;
    public GetMoveStartFreePosDelegate m_moveStartFreePosDelegate;

    private const string c_shapeLayer = "ShapeLayer";
    public static ContextNameKeyPair s_cvarShapeLayer = new ContextNameKeyPair("ShapeLayer");

    private TargetSelectMod_Shape m_targetSelMod;
    private List<AbilityAreaShape> m_shapesList = new List<AbilityAreaShape>();

    public override string GetUsageForEditor()
    {
        return GetContextUsageStr(
            "ShapeLayer",
            "on every hit actor, smallest shape index that actor is hit in (0-based). " +
            "Shapes are sorted from smallest to largest");
    }

    public override void ListContextNamesForEditor(List<string> names)
    {
        names.Add("ShapeLayer");
    }

    public override void Initialize()
    {
        InitShapesList();
    }

    private void InitShapesList()
    {
        m_shapesList = new List<AbilityAreaShape>();
        m_shapesList.Add(GetShape());
        List<AbilityAreaShape> collection = m_targetSelMod != null && m_useTargetDataOverride
            ? m_targetSelMod.m_additionalShapesOverrides
            : m_additionalShapes;
        m_shapesList.AddRange(collection);
        m_shapesList.Sort();
    }

    public bool RequireTargetingOnActor()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_requireTargetingOnActorMod.GetModifiedValue(m_requireTargetingOnActor)
            : m_requireTargetingOnActor;
    }

    public bool CanTargetOnEnemies()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_canTargetOnEnemiesMod.GetModifiedValue(m_canTargetOnEnemies)
            : m_canTargetOnEnemies;
    }

    public bool CanTargetOnAllies()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_canTargetOnAlliesMod.GetModifiedValue(m_canTargetOnAllies)
            : m_canTargetOnAllies;
    }

    public bool CanTargetOnSelf()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_canTargetOnSelfMod.GetModifiedValue(m_canTargetOnSelf)
            : m_canTargetOnSelf;
    }

    public bool IgnoreLosToTargetActor()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_ignoreLosToTargetActorMod.GetModifiedValue(m_ignoreLosToTargetActor)
            : m_ignoreLosToTargetActor;
    }

    public float GetMoveLineWidth()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_moveLineWidthMod.GetModifiedValue(m_moveLineWidth)
            : m_moveLineWidth;
    }

    public override List<AbilityUtil_Targeter> CreateTargeters(Ability ability)
    {
        if (!m_useMoveShapeTargeter)
        {
            AbilityUtil_Targeter_MultipleShapes targeter = new AbilityUtil_Targeter_MultipleShapes(
                ability,
                m_shapesList,
                new List<AbilityTooltipSubject> { AbilityTooltipSubject.Primary },
                IgnoreLos(),
                IncludeEnemies(),
                IncludeAllies(),
                IncludeCaster());
            targeter.SetAffectedGroups(IncludeEnemies(), IncludeAllies(), IncludeCaster());
            targeter.m_alwaysIncludeShapeCenterActor = RequireTargetingOnActor();
            targeter.SetShowArcToShape(m_showTargeterArc);
            return new List<AbilityUtil_Targeter> { targeter };
        }
        else
        {
            AbilityUtil_Targeter_MovingShape targeter = new AbilityUtil_Targeter_MovingShape(
                ability,
                GetShape(),
                IgnoreLos(),
                GetMoveLineWidth());
            targeter.SetAffectedGroups(IncludeEnemies(), IncludeAllies(), IncludeCaster());
            if (!IncludeAllies() && !IncludeCaster())
            {
                targeter.m_affectsCaster = AbilityUtil_Targeter.AffectsActor.Never;
            }

            targeter.SetShowArcToShape(m_showTargeterArc);
            return new List<AbilityUtil_Targeter> { targeter };
        }
    }

    public override bool HandleCanCastValidation(Ability ability, ActorData caster)
    {
        if (!RequireTargetingOnActor())
        {
            return true;
        }

        return ability.HasTargetableActorsInDecision(
            caster,
            CanTargetOnEnemies(),
            CanTargetOnAllies(),
            CanTargetOnSelf(),
            Ability.ValidateCheckPath.Ignore,
            !IgnoreLosToTargetActor(),
            false,
            true);
    }

    public override bool HandleCustomTargetValidation(
        Ability ability,
        ActorData caster,
        AbilityTarget target,
        int targetIndex,
        List<AbilityTarget> currentTargets)
    {
        if (!RequireTargetingOnActor())
        {
            return true;
        }

        BoardSquare targetSquare = Board.Get().GetSquare(target.GridPos);
        ActorData targetActor = targetSquare != null
            ? targetSquare.OccupantActor
            : null;
        return targetActor != null
               && ability.CanTargetActorInDecision(
                   caster,
                   targetActor,
                   CanTargetOnEnemies(),
                   CanTargetOnAllies(),
                   CanTargetOnSelf(),
                   Ability.ValidateCheckPath.Ignore,
                   !IgnoreLosToTargetActor(),
                   false,
                   true);
    }

    protected override void OnTargetSelModApplied(TargetSelectModBase modBase)
    {
        m_targetSelMod = modBase as TargetSelectMod_Shape;
    }

    protected override void OnTargetSelModRemoved()
    {
        m_targetSelMod = null;
    }

    public AbilityAreaShape GetShape()
    {
        return m_targetSelMod != null
            ? m_targetSelMod.m_shapeMod.GetModifiedValue(m_shape)
            : m_shape;
    }

    public BoardSquare GetShapeCenterSquare(AbilityTarget target, ActorData caster)
    {
        return m_centerSquareDelegate != null
            ? m_centerSquareDelegate(target, caster)
            : Board.Get().GetSquare(target.GridPos);
    }

    public bool IsMovingShape(ActorData caster)
    {
        return m_isMovingShapeDelegate != null && m_isMovingShapeDelegate(caster);
    }

    public BoardSquare GetMoveStartSquare(AbilityTarget target, ActorData caster)
    {
        return m_moveStartSquareDelegate != null
            ? m_moveStartSquareDelegate(target, caster)
            : caster.GetCurrentBoardSquare();
    }

    public Vector3 GetMoveStartFreePos(AbilityTarget target, ActorData caster)
    {
        return m_moveStartFreePosDelegate != null
            ? m_moveStartFreePosDelegate(target, caster)
            : caster.GetFreePos();
    }

#if SERVER
    //rogues
    public override List<ServerClientUtils.SequenceStartData> CreateSequenceStartData(
        List<AbilityTarget> targets,
        ActorData caster,
        ServerAbilityUtils.AbilityRunData additionalData,
        Sequence.IExtraSequenceParams[] extraSequenceParams = null)
    {
        List<ServerClientUtils.SequenceStartData> list = new List<ServerClientUtils.SequenceStartData>();
        BoardSquare shapeCenterSquare = GetShapeCenterSquare(targets[0], caster);
        Vector3 centerOfShape = AreaEffectUtils.GetCenterOfShape(GetShape(), targets[0].FreePos, shapeCenterSquare);
        ServerClientUtils.SequenceStartData item = new ServerClientUtils.SequenceStartData(
            m_castSequencePrefab,
            centerOfShape,
            additionalData.m_abilityResults.HitActorsArray(),
            caster,
            additionalData.m_sequenceSource,
            extraSequenceParams);
        list.Add(item);
        return list;
    }

    //rogues
    public override void CalcHitTargets(
        List<AbilityTarget> targets,
        ActorData caster,
        List<NonActorTargetInfo> nonActorTargetInfo)
    {
        ResetContextData();
        base.CalcHitTargets(targets, caster, nonActorTargetInfo);
        List<List<ActorData>> hitActors = GetHitActors(targets, caster, nonActorTargetInfo);
        BoardSquare shapeCenterSquare = GetShapeCenterSquare(targets[0], caster);
        Vector3 centerOfShape = AreaEffectUtils.GetCenterOfShape(GetShape(), targets[0].FreePos, shapeCenterSquare);
        ContextVars nonActorSpecificContext = GetNonActorSpecificContext();
        // nonActorSpecificContext.SetValue(this.m_centerPosContextKey.GetKey(), centerOfShape);
        List<BarrierPoseInfo> barrierPosesForRegularPolygon = BarrierPoseInfo.GetBarrierPosesForRegularPolygon(
            centerOfShape,
            AreaEffectUtils.GetNumberOfSidesForShape(GetShape()),
            AreaEffectUtils.GetWidthForShape(GetShape()) * 0.5f * Board.SquareSizeStatic);
        //if (!barrierPosesForRegularPolygon.IsNullOrEmpty<BarrierPoseInfo>())
        //{
        //    nonActorSpecificContext.SetValue(TargetSelect_Shape.s_cvarShapeSideWidth.GetKey(), barrierPosesForRegularPolygon[0].widthInWorld);
        //    for (int i = 0; i < barrierPosesForRegularPolygon.Count; i++)
        //    {
        //        nonActorSpecificContext.SetValue(TargetSelect_Shape.s_cvarShapeSideCenters[i].GetKey(), barrierPosesForRegularPolygon[i].midpoint);
        //        nonActorSpecificContext.SetValue(TargetSelect_Shape.s_cvarShapeSideFacingDirs[i].GetKey(), barrierPosesForRegularPolygon[i].facingDirection);
        //    }
        //}
        for (int i = 0; i < hitActors.Count; i++)
        {
            foreach (ActorData actor in hitActors[i])
            {
                AddHitActor(actor, centerOfShape);
                SetActorContext(actor, s_cvarShapeLayer.GetKey(), i);
            }
        }

        bool isMovingShape = IsMovingShape(caster);
        BoardSquare moveStartSquare = GetMoveStartSquare(targets[0], caster);
        if (isMovingShape && moveStartSquare != null)
        {
            Vector3 vector = moveStartSquare.ToVector3();
            Vector3 endPos = centerOfShape;
            List<Team> relevantTeams = TargeterUtils.GetRelevantTeams(caster, IncludeAllies(), IncludeEnemies());
            List<ActorData> actorsInShape = AreaEffectUtils.GetActorsInShape(
                m_shape,
                vector,
                moveStartSquare,
                IgnoreLos(),
                caster,
                relevantTeams,
                nonActorTargetInfo);
            foreach (ActorData actor in actorsInShape)
            {
                if (!HasContextForActor(caster))
                {
                    AddHitActor(actor, vector);
                }
            }

            List<ActorData> actorsInRadiusOfLine = AreaEffectUtils.GetActorsInRadiusOfLine(
                vector,
                endPos,
                0f,
                0f,
                0.5f * m_moveLineWidth,
                IgnoreLos(),
                caster,
                relevantTeams,
                nonActorTargetInfo);
            foreach (ActorData actor in actorsInRadiusOfLine)
            {
                if (!HasContextForActor(caster))
                {
                    AddHitActor(actor, vector);
                }
            }
        }

        if (IncludeCaster() && !GetActorHitContextMap().ContainsKey(caster))
        {
            AddHitActor(caster, caster.GetFreePos());
            SetActorContext(caster, s_cvarShapeLayer.GetKey(), 0);
        }
    }

    //rogues
    public List<List<ActorData>> GetHitActors(
        List<AbilityTarget> targets,
        ActorData caster,
        List<NonActorTargetInfo> nonActorTargetInfo)
    {
        Vector3 freePos = targets[0].FreePos;
        BoardSquare shapeCenterSquare = GetShapeCenterSquare(targets[0], caster);
        AreaEffectUtils.GetActorsInShapeLayers(
            m_shapesList,
            freePos,
            shapeCenterSquare,
            IgnoreLos(),
            caster,
            TargeterUtils.GetRelevantTeams(caster, IncludeAllies(), IncludeEnemies()),
            out List<List<ActorData>> actorsInLayers,
            nonActorTargetInfo);
        if (m_requireTargetingOnActor
            && shapeCenterSquare != null
            && shapeCenterSquare.OccupantActor != null
            && actorsInLayers.Count > 0)
        {
            ActorData occupantActor = shapeCenterSquare.OccupantActor;
            if (!actorsInLayers[0].Contains(occupantActor))
            {
                actorsInLayers[0].Add(occupantActor);
            }
        }

        return actorsInLayers;
    }
#endif
}