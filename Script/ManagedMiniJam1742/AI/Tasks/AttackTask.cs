using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.Chaos;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742.AI.Tasks;

[UClass]
public partial class UAttackTask : UMyAITask
{
    public enum TargetType
    {
        Unit,
        Structure
    }

    public AUnitCharacter TargetUnit { get; private set; }
    public AStructure TargetStructure { get; private set; }

    private TargetType targetType;

    protected override void OnTick(float delta)
    {
        base.OnTick(delta);

        if (Controller.ControlledPawn is not AUnitCharacter unitCharacter) return;

        var targetActor = GetTargetActor();

        if (!unitCharacter.CanSeeTarget(targetActor, out _))
        {
            Complete(true);
            return;
        }

        if (!unitCharacter.IsShooting)
        {
            if (targetType == TargetType.Unit)
            {
                unitCharacter.StartShootAtTarget(TargetUnit);
            }
            else if (targetType == TargetType.Structure)
            {
                unitCharacter.StartShootAtTarget(TargetStructure);
            }
        }
    }

    public void SetTarget(AUnitCharacter target)
    {
        TargetUnit = target;
        targetType = TargetType.Unit;
    }

    public void SetTarget(AStructure target)
    {
        TargetStructure = target;
        targetType = TargetType.Structure;
    }

    public AActor GetTargetActor()
    {
        return targetType == TargetType.Unit ? TargetUnit : TargetStructure;
    }
}
