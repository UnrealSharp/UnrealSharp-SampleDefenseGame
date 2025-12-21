using ManagedMiniJam1742.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;
using ManagedMiniJam1742.Structures;

namespace ManagedMiniJam1742.AI.Tasks;


[UClass]
public partial class UConquerTask : UMyAITask
{
    private AHQStructure hqStructure;

    protected override void OnStart()
    {
        base.OnStart();

        hqStructure = GetActorOfClass<AHQStructure>();
    }

    protected override void OnTick(float delta)
    {
        base.OnTick(delta);

        if (Controller.ControlledPawn is not AUnitCharacter character) return;

        if (hqStructure == null || !hqStructure.IsValid())
        {
            Complete(true);
            return;
        }
        else
        {
            Controller.GetActorBounds(true, out var truckBoxOrigin, out var truckBoxExtent);
            hqStructure.GetActorBounds(true, out var targetBoxOrigin, out var targetBoxExtent);

            var truckBox = MathLibrary.MakeBoxWithOrigin(truckBoxOrigin, truckBoxExtent);
            var targetResourceBox = MathLibrary.MakeBoxWithOrigin(targetBoxOrigin, targetBoxExtent);

            hqStructure.StaticMeshComponent.GetClosestPointOnCollision(Controller.ControlledPawn.ActorLocation, out var closetPointOnBody);

            if (FVector.Distance(Controller.ControlledPawn.ActorLocation, closetPointOnBody) < 1000)
            {
                Controller.StopTasks();

                var attackTask = Controller.CreateTask<UAttackTask>();
                attackTask.SetTarget(hqStructure);
                Controller.AddTask(attackTask);
            }
            else
            {
                if (Controller.MoveStatus == EPathFollowingStatus.Idle)
                {
                    var moveToLocation = closetPointOnBody + (FVector.Normalize(Controller.ControlledPawn.ActorLocation - closetPointOnBody) * 100);

                    if (UNavigationSystemV1.GetRandomReachablePointInRadius(moveToLocation, out var randomLocation, 100))
                    {
                        Controller.MoveToLocation(randomLocation);
                    }
                }
            }
        }
    }

    protected override void OnStop()
    {
        base.OnStop();

        Controller.StopMovement();
    }
}
