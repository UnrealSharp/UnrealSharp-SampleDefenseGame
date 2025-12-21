using ManagedMiniJam1742.AI.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742.AI;

[UClass]
public partial class AEnemyAdvancedAIController : AAdvancedAIController
{
    public override void BeginPlay()
    {
        base.BeginPlay();

        DefaultTask = typeof(UConquerTask);
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        CheckForTargetNearbyAndAttack();
    }

    private void CheckForTargetNearbyAndAttack()
    {
        if (ControlledPawn is not AUnitCharacter unitCharacter) return;
        if (HasCurrentTask<UAttackTask>()) return;

        SphereOverlapActors(ControlledPawn.ActorLocation, unitCharacter.Range, [EObjectTypeQuery.ObjectTypeQuery3, EObjectTypeQuery.ObjectTypeQuery8],
            typeof(AActor), [ControlledPawn], out var actors);

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
            StopTasks();

            var attackTask = CreateTask<UAttackTask>();
            attackTask.SetTarget(opposingTeamNearestUnit);
            AddTask(attackTask);
        }
        else if (unitCharacter.CanSeeTarget(opposingTeamNearestStructure, out _))
        {
            StopTasks();

            var attackTask = CreateTask<UAttackTask>();
            attackTask.SetTarget(opposingTeamNearestStructure);
            AddTask(attackTask);
        }
    }
}