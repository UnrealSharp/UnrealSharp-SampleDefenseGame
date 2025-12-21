using ManagedMiniJam1742;
using ManagedMiniJam1742.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedMiniJam1724.Tools;

[UClass]
public partial class USelectTool : UTool
{
    private FVector selectStartPawnLocation;
    private FVector selectStartLocation;
    private FVector selectStartDirection;

    private FVector selectStartHitPosition;
    private bool selectStarted;

    private List<AUnitCharacter> selectedUnits = [];

    public override void OnEquip()
    {
        base.OnEquip();

        selectedUnits.Clear();
    }

    public override void OnStartAction(ActionType actionType)
    {
        if (actionType == ActionType.Primary)
        {
            selectedUnits.Clear();

            if (PlayerController.ConvertMouseLocationToWorldSpace(out var mouseWorldLocation, out var mouseWorldDirection))
            {
                selectStartPawnLocation = Pawn.ActorLocation;
                selectStartLocation = mouseWorldLocation;
                selectStartDirection = mouseWorldDirection;

                var startLocation = Pawn.ActorLocation;
                var endLocation = mouseWorldLocation + (mouseWorldDirection * 100000);

                if (LineTraceForObjects(startLocation, endLocation, [EObjectTypeQuery.ObjectTypeQuery7], false, [Pawn], EDrawDebugTrace.None, out var hit, true))
                {
                    selectStartHitPosition = hit.ImpactPoint;
                    selectStarted = true;
                }
            }
        }
    }

    public override void Tick(float delta)
    {
        base.Tick(delta);

        if (Pawn == null) return;
        if (!Pawn.IsValid()) return;


        if (selectStarted)
        {
            if (PlayerController.ConvertMouseLocationToWorldSpace(out var mouseWorldLocation, out var mouseWorldDirection))
            {
                var startLocation = Pawn.ActorLocation;
                var endLocation = mouseWorldLocation + (mouseWorldDirection * 100000);

                if (LineTraceForObjects(startLocation, endLocation, [EObjectTypeQuery.ObjectTypeQuery7], false, [Pawn], EDrawDebugTrace.None, out var hit, true))
                {
                    var selectEndHitPosition = hit.ImpactPoint + new FVector(0, 0, 500);

                    FVector boxCenter = (selectStartHitPosition + selectEndHitPosition) / 2.0f;
                    FVector boxExtent = (selectEndHitPosition - selectStartHitPosition) / 2.0f;
                    FVector boxExtentAbs = new FVector(double.Abs(boxExtent.X), double.Abs(boxExtent.Y), double.Abs(boxExtent.Z));

                    DrawDebugBox(boxCenter, boxExtent, FLinearColor.Green, FRotator.ZeroRotator);

                    GetAllActorsOfClass< AUnitCharacter>(out var units);

                    foreach (var unit in units)
                    {
                        if (unit.Team == ETeam.Green)
                        {
                            unit.GetActorBounds(true, out var origin, out var extent);

                            var unitBox = MathLibrary.MakeBoxWithOrigin(origin, extent);
                            var selectionBox = MathLibrary.MakeBoxWithOrigin(boxCenter, boxExtentAbs);

                            if (MathLibrary.Box_Intersects(selectionBox, unitBox))
                            {
                                DrawDebugBox(origin, extent, FLinearColor.Green, FRotator.ZeroRotator);
                            }
                        }
                    }
                }
            }
        }

        foreach(var selectedUnit in selectedUnits)
        {
            if (selectedUnit != null && selectedUnit.IsValid())
            {
                selectedUnit.GetActorBounds(true, out var selectedUnitOrigin, out var selectedUnitExtent);

                DrawDebugBox(selectedUnitOrigin, selectedUnitExtent, FLinearColor.Green, FRotator.ZeroRotator);

                if (selectedUnit is ADumpTruckCharacter)
                {
                    GetAllActorsOfClass<APlasticResource>(out var plasticResourceActors);

                    foreach (var plasticResourceActor in plasticResourceActors)
                    {
                        plasticResourceActor.GetActorBounds(true, out var plasticResourceOrigin, out var plasticResourceExtent);

                        var animatedOffset = new FVector(0, 0, double.Cos(TimeSeconds * 5) * 100);

                        var centerTop = plasticResourceActor.ActorLocation + new FVector(0, 0, plasticResourceExtent.Z + 100) + animatedOffset;
                        var centerTopOffset = centerTop + new FVector(0, 0, 400);

                        DrawDebugArrow(centerTopOffset, centerTop, 500, FLinearColor.Red, 0f, 50);
                    }
                }
            }
        }
    }

    public override void OnEndAction(ActionType actionType)
    {
        if (actionType == ActionType.Primary)
        {
            selectStarted = false;

            if (PlayerController.ConvertMouseLocationToWorldSpace(out var mouseWorldLocation, out var mouseWorldDirection))
            {
                var startLocation = Pawn.ActorLocation;
                var endLocation = mouseWorldLocation + (mouseWorldDirection * 100000);

                if (LineTraceForObjects(startLocation, endLocation, [EObjectTypeQuery.ObjectTypeQuery7], false, [Pawn], EDrawDebugTrace.None, out var hit, true))
                {
                    var selectEndHitPosition = hit.ImpactPoint + new FVector(0, 0, 500);

                    FVector boxCenter = (selectStartHitPosition + selectEndHitPosition) / 2.0f;
                    FVector boxExtent = (selectEndHitPosition - selectStartHitPosition) / 2.0f;
                    FVector boxExtentAbs = new FVector(double.Abs(boxExtent.X), double.Abs(boxExtent.Y), double.Abs(boxExtent.Z));

                    GetAllActorsOfClass<AUnitCharacter>(out var actors);

                    selectedUnits.Clear();
                    foreach (var actor in actors)
                    {
                        actor.GetActorBounds(true, out var origin, out var extent);

                        var unitBox = MathLibrary.MakeBoxWithOrigin(origin, extent);
                        var selectionBox = MathLibrary.MakeBoxWithOrigin(boxCenter, boxExtentAbs);

                        if (MathLibrary.Box_Intersects(selectionBox, unitBox))
                        {
                            if (actor is AUnitCharacter unit && unit.Team == ETeam.Green)
                            {
                                selectedUnits.Add(unit);
                            }
                        }
                    }
                }
            }
        }
        else if (actionType == ActionType.Secondary)
        {
            if (PlayerController.ConvertMouseLocationToWorldSpace(out var worldLocation, out var worldDirection))
            {
                var startLocation = Pawn.ActorLocation;
                var endLocation = worldLocation + (worldDirection * 100000);

                ETraceTypeQuery traceChannel = ETraceChannel.Camera.ToQuery();
                if (LineTraceByChannel(startLocation, endLocation, traceChannel, false, [Pawn], EDrawDebugTrace.None, out FHitResult hit, true))
                {
                    if (selectedUnits.Count > 0)
                    {
                        if (selectedUnits.Any(u => u != null && u.IsValid()))
                        {
                            PlaySound2D(AResourceManager.Get().UnitConfirmationSounds.RandomElement());
                        }

                        if (hit.Actor is APlasticResource plasticResource)
                        {
                            foreach (var selectedUnit in selectedUnits)
                            {
                                if (selectedUnit != null && selectedUnit.IsValid())
                                {
                                    selectedUnit.GoToActor(plasticResource);
                                }
                            }
                        }
                        else if (hit.Actor is AStructure structure)
                        {
                            foreach (var selectedUnit in selectedUnits)
                            {
                                if (selectedUnit != null && selectedUnit.IsValid())
                                {
                                    selectedUnit.GoToActor(structure);
                                }
                            }
                        }
                        else if (hit.Actor is AUnitCharacter unitCharacter)
                        {
                            foreach (var selectedUnit in selectedUnits)
                            {
                                if (selectedUnit != null && selectedUnit.IsValid())
                                {
                                    selectedUnit.GoToActor(unitCharacter);
                                }
                            }
                        }
                        else
                        {
                            foreach (var selectedUnit in selectedUnits)
                            {
                                if (selectedUnit != null && selectedUnit.IsValid())
                                {
                                    selectedUnit.GoToLocation(hit.ImpactPoint);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
