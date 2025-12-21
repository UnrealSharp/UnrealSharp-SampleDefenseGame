using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742.AI.Tasks;

[UClass]
public partial class UIdleTask : UMyAITask
{

    protected override void OnTick(float delta)
    {
        base.OnTick(delta);

        if (Controller.ControlledPawn is not AUnitCharacter unitCharacter) return;

        SphereOverlapActors(Controller.ControlledPawn.ActorLocation, unitCharacter.Range, [EObjectTypeQuery.ObjectTypeQuery3, EObjectTypeQuery.ObjectTypeQuery8],
            typeof(AActor), [Controller.ControlledPawn], out var actors);

        var opposingTeamNearestUnit = actors
            .Where(a => a is AUnitCharacter)
            .OfType<AUnitCharacter>()
            .Where(u => u.Team != unitCharacter.Team)
            .MinBy(u => u.GetDistanceTo(unitCharacter));

        var opposingTeamNearestStructure = actors
            .Where(a => a is AStructure)
            .OfType<AStructure>()
            .Where(s => s.Team != unitCharacter.Team)
            .MinBy(s => s.GetDistanceTo(unitCharacter));

        if (unitCharacter.CanSeeTarget(opposingTeamNearestUnit, out _))
        {
            var attackTask = Controller.CreateTask<UAttackTask>();
            attackTask.SetTarget(opposingTeamNearestUnit);
            Controller.AddTask(attackTask);
        }

        Complete(true);
    }
}
