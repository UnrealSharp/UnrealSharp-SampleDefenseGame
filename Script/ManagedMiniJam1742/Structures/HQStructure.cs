using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;

namespace ManagedMiniJam1742.Structures;

[UClass]
public partial class AHQStructure : AStructure
{
    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite)]
    public partial int PlasticStockpile { get; set; }

    private TSubclassOf<AUnitCharacter> unitClassBuilding;
    private bool isBuildingUnit;
    private TimeSpan timeUntilUnitBuilt;

    [UFunction(FunctionFlags.BlueprintEvent)]
    public partial void OnUnitBuilt(AUnitCharacter newUnit);
    public partial void OnUnitBuilt_Implementation(AUnitCharacter newUnit) { }

    public bool CanBuildUnit(TSubclassOf<AUnitCharacter> unitClass)
    {
        if (isBuildingUnit) return false;
        if (PlasticStockpile < unitClass.DefaultObject.Cost) return false;

        return true;
    }

    public void BuildUnit(TSubclassOf<AUnitCharacter> unitClass)
    {
        if (isBuildingUnit) return;
        if (PlasticStockpile < unitClass.DefaultObject.Cost) return;

        timeUntilUnitBuilt = TimeSpan.FromSeconds(TimeSeconds + unitClass.DefaultObject.BuildTime);
        unitClassBuilding = unitClass;
        PlasticStockpile -= unitClass.DefaultObject.Cost;
        isBuildingUnit = true;
    }

    public bool CanBuildStructure(FVector location, TSubclassOf<AStructure> structureClass)
    {
        if (PlasticStockpile < structureClass.DefaultObject.Cost) return false;

        GetAllActorsOfClass<AStructure>(out var structures);
        foreach(var structure in structures)
        {
            if (FVector.Distance(structure.ActorLocation, location) < 1000)
            {
                return false;
            }
        }

        GetAllActorsOfClass<AUnitCharacter>(out var units);
        foreach (var unit in units)
        {
            if (FVector.Distance(unit.ActorLocation, location) < 1000)
            {
                return false;
            }
        }

        GetAllActorsOfClass<AEnemySpawner>(out var spawners);
        foreach (var spawner in spawners)
        {
            if (FVector.Distance(spawner.ActorLocation, location) < 5000)
            {
                return false;
            }
        }

        return true;
    }

    public void BuildStructure(TSubclassOf<AStructure> structureClass, FVector location)
    {
        if (!CanBuildStructure(location, structureClass)) return;

        var transform = new FTransform(FQuat.Identity, location, FVector.One);

        PlasticStockpile -= structureClass.DefaultObject.Cost;

        var structure = SpawnActor(structureClass, transform, ESpawnActorCollisionHandlingMethod.AlwaysSpawn);
        structure.OnBuilt();
    }


    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        var timeTimespan = TimeSpan.FromSeconds(TimeSeconds);

        if (isBuildingUnit && timeTimespan >= timeUntilUnitBuilt)
        {
            isBuildingUnit = false;

            var spawnLocation = MathLibrary.TransformLocation(ActorTransform, new FVector(0, 1405.0, 0));

            var yaw = double.DegreesToRadians(ActorRotation.Yaw);
            var pitch = double.DegreesToRadians(ActorRotation.Pitch);
            var roll = double.DegreesToRadians(ActorRotation.Roll);

            var transform = new FTransform(FQuat.CreateFromYawPitchRoll(yaw, pitch, roll), spawnLocation, FVector.One);
            var newUnit = SpawnActor(unitClassBuilding, transform, ESpawnActorCollisionHandlingMethod.AlwaysSpawn);

            OnUnitBuilt(newUnit);
        }
    }
}
