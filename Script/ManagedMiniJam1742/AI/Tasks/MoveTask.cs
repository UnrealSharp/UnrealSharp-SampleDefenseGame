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
public partial class UMoveTask : UMyAITask
{
    public FVector MoveToLocation { get; set; }
    public bool AttackMove { get; set; }

    protected override void OnStart()
    {
        base.OnStart();

        var status = Controller.MoveToLocation(MoveToLocation);

        if (status == UnrealSharp.AIModule.EPathFollowingRequestResult.Failed)
        {
            Complete(false);
        }
    }

    protected override void OnTick(float delta)
    {
        base.OnTick(delta);

        if (Controller.ControlledPawn is not AUnitCharacter unitCharacter) return;

        if (AttackMove)
        {
            SphereOverlapActors(Controller.ControlledPawn.ActorLocation, unitCharacter.Range, [EObjectTypeQuery.ObjectTypeQuery3],
                typeof(AUnitCharacter), [Controller.ControlledPawn], out var actors);

            var opposingTeamNearestUnit = actors.Select(a => (AUnitCharacter)a)
                .Where(u => u.Team != unitCharacter.Team)
                .OrderBy(u => u.GetDistanceTo(Controller.ControlledPawn))
                .FirstOrDefault();

            if (unitCharacter.CanSeeTarget(opposingTeamNearestUnit, out _))
            {
                var moveTask = Controller.CreateTask<UMoveTask>();
                moveTask.MoveToLocation = MoveToLocation;
                moveTask.AttackMove = AttackMove;

                var attackTask = Controller.CreateTask<UAttackTask>();
                attackTask.SetTarget(opposingTeamNearestUnit);
                attackTask.ContinueWithTask(moveTask);

                Controller.AddTask(attackTask);

                Complete();
                return;
            }
        }

        if (Controller.MoveStatus == UnrealSharp.AIModule.EPathFollowingStatus.Idle)
        {
            Complete(true);
        }
    }

    protected override void OnStop()
    {
        base.OnStop();

        Controller.StopMovement();
    }
}
