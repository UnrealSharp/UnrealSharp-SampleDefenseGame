using ManagedMiniJam1724;
using ManagedMiniJam1724.Tools;
using ManagedMiniJam1742.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742.Tools;

[UClass]
public partial class UBuildTool : UTool
{
    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial TSubclassOf<AStructure> StructureClass { get; set; }

    private AGhostActor ghostActor;

    public override void OnEquip()
    {
        base.OnEquip();

        if (!StructureClass.IsValid)
        {
            Pawn.CreateAndEquipTool(typeof(USelectTool));
            return;
        }

        var groundVector = GetMouseGroundVector();

        var transform = new FTransform(FQuat.Identity, groundVector, FVector.One);

        ghostActor = SpawnActor<AGhostActor>(typeof(AGhostActor), transform, ESpawnActorCollisionHandlingMethod.AlwaysSpawn);

        ghostActor.SetupMesh(StructureClass.DefaultObject.StaticMeshComponent.StaticMesh);
    }

    public override void OnStartAction(ActionType actionType)
    {
        base.OnStartAction(actionType);
    }

    public override void Tick(float delta)
    {
        base.Tick(delta);

        var groundVector = GetMouseGroundVector();

        if (ghostActor != null && ghostActor.IsValid() && StructureClass.IsValid)
        {
            ghostActor.SetActorLocation(groundVector, false, out _, false);

            var hqStructure = GetActorOfClass<AHQStructure>();

            ghostActor.SetCanBuild(hqStructure.CanBuildStructure(groundVector, StructureClass));
        }
    }

    public FVector GetMouseGroundVector()
    {
        if (PlayerController.ConvertMouseLocationToWorldSpace(out var worldLocation, out var worldDirection))
        {
            var startLocation = Pawn.ActorLocation;
            var endLocation = worldLocation + (worldDirection * 100000);

            ETraceTypeQuery traceChannel = ETraceChannel.Camera.ToQuery();
            if (LineTraceForObjects(startLocation, endLocation, [EObjectTypeQuery.ObjectTypeQuery7], false, [Pawn], EDrawDebugTrace.None, out var hit, true))
            {
                return hit.ImpactPoint;
            }
        }

        return FVector.Zero;
    }


    public override void OnEndAction(ActionType actionType)
    {
        base.OnEndAction(actionType);

        if (actionType == ActionType.Primary)
        {
            var hqStructure = GetActorOfClass<AHQStructure>();

            var groundVector = GetMouseGroundVector();

            hqStructure.BuildStructure(StructureClass, groundVector);

            Pawn.CreateAndEquipTool(typeof(USelectTool));
        }
        else if (actionType == ActionType.Secondary)
        {
            Pawn.CreateAndEquipTool(typeof(USelectTool));
            return;
        }

    }

    public override void OnUnEquip()
    {
        base.OnUnEquip();

        if (ghostActor != null && ghostActor.IsValid())
        {
            ghostActor.DestroyActor();
        }
    }
}