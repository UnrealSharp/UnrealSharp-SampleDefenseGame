using UnrealSharp.AIModule;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;
using ManagedMiniJam1742.Structures;
using UnrealSharp.Attributes;
using ManagedMiniJam1742.Units;

namespace ManagedMiniJam1742.AI.Tasks;

[UClass]
public partial class UHarvestPlasticTask : UMyAITask
{
    public enum HarvestAIState
    {
        GotoPlastic,
        HarvestPlastic,
        GotoBase
    }

    public APlasticResource Target { get; set; }
    public HarvestAIState CurrentState { get; private set; }

    private AHQStructure hqStructure;

    private TimeSpan nextHarvestTime;

    protected override void OnStart()
    {
        hqStructure = GetActorOfClass<AHQStructure>();
        CurrentState = HarvestAIState.GotoPlastic;
    }

    protected override void OnTick(float delta)
    {
        if (CurrentState == HarvestAIState.GotoPlastic)
        {
            if (Target == null || !Target.IsValid())
            {
                Complete();
                return;
            }
            else
            {
                Controller.GetActorBounds(true, out var truckBoxOrigin, out var truckBoxExtent);
                Target.GetActorBounds(true, out var targetBoxOrigin, out var targetBoxExtent);

                var truckBox = MathLibrary.MakeBoxWithOrigin(truckBoxOrigin, truckBoxExtent);
                var targetResourceBox = MathLibrary.MakeBoxWithOrigin(targetBoxOrigin, targetBoxExtent);

                Target.StaticMeshComponent.GetClosestPointOnCollision(Controller.ControlledPawn.ActorLocation, out var closetPointOnBody);

                if (FVector.Distance(Controller.ControlledPawn.ActorLocation, closetPointOnBody) < 500)
                {
                    Controller.StopMovement();
                    CurrentState = HarvestAIState.HarvestPlastic;

                    if (Controller.ControlledPawn is ADumpTruckCharacter dumpTruck)
                    {
                        dumpTruck.HarvestBeamComponent.SetVectorParameter("Beam End", closetPointOnBody);
                        dumpTruck.HarvestBeamComponent.Activate();
                    }
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
        else if (CurrentState == HarvestAIState.HarvestPlastic)
        {
            if (Target == null || !Target.IsValid())
            {
                CurrentState = HarvestAIState.GotoBase;
                Controller.StopMovement();

                if (Controller.ControlledPawn is ADumpTruckCharacter dumpTruck)
                {
                    dumpTruck.HarvestBeamComponent.Deactivate();
                }
            }
            else
            {
                if (TimeSpan.FromSeconds(TimeSeconds) >= nextHarvestTime)
                {
                    if (Controller.ControlledPawn is ADumpTruckCharacter dumpTruck)
                    {
                        if (dumpTruck.CarryingPlastic >= dumpTruck.CarryingPlasticLimit)
                        {
                            dumpTruck.HarvestBeamComponent.Deactivate();
                            Target.StopHarvesting();

                            CurrentState = HarvestAIState.GotoBase;
                        }
                        else
                        {
                            int amountHarvested = Target.Harvest(Math.Min(20, dumpTruck.CarryingPlasticLimit - dumpTruck.CarryingPlastic));
                            dumpTruck.CarryingPlastic += amountHarvested;
                            nextHarvestTime = TimeSpan.FromSeconds(TimeSeconds + 2);
                        }
                    }
                }
            }
        }
        else if (CurrentState == HarvestAIState.GotoBase)
        {
            if (hqStructure == null || !hqStructure.IsValid())
            {
                Complete();
                return;
            }

            hqStructure.StaticMeshComponent.GetClosestPointOnCollision(Controller.ControlledPawn.ActorLocation, out var closetPointOnBody);

            if (FVector.Distance(Controller.ControlledPawn.ActorLocation, closetPointOnBody) < 500)
            {
                {
                    if (Controller.ControlledPawn is ADumpTruckCharacter dumpTruck)
                    {
                        hqStructure.PlasticStockpile += dumpTruck.CarryingPlastic;
                        dumpTruck.CarryingPlastic = 0;
                    }
                }

                if (Target == null || !Target.IsValid())
                {
                    Complete(true);

                    if (Controller.ControlledPawn is ADumpTruckCharacter dumpTruck)
                    {
                        dumpTruck.HarvestBeamComponent.Deactivate();
                    }
                }
                else
                {
                    CurrentState = HarvestAIState.GotoPlastic;
                }
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
        if (Controller.ControlledPawn is ADumpTruckCharacter dumpTruck)
        {
            dumpTruck.HarvestBeamComponent.Deactivate();
        }

        Controller.StopMovement();
        Target.StopHarvesting();
    }
}
