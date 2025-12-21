using ManagedMiniJam1724;
using ManagedMiniJam1742.Structures;
using ManagedMiniJam1742.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.UMG;

namespace ManagedMiniJam1742.UI;

[UClass]
public partial class UHUDUserWidget : UUserWidget
{
    [UFunction(FunctionFlags.BlueprintCallable | FunctionFlags.BlueprintPure)]
    public int GetCurrentPlasticStockpile()
    {
        GetAllActorsOfClass<AHQStructure>(out var hQStructures);
        return hQStructures.Sum(hqStruct => hqStruct.PlasticStockpile);
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    public bool BuildUnit(TSubclassOf<AUnitCharacter> unitClass)
    {
        var hqStructure = GetActorOfClass<AHQStructure>();

        if (hqStructure == null || !hqStructure.IsValid()) return false;
        if (!hqStructure.CanBuildUnit(unitClass)) return false;

        hqStructure.BuildUnit(unitClass);

        return true;
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void BuildStructure(TSubclassOf<AStructure> structureClass)
    {
        if (OwningPlayerPawn is not APawnCameraView cameraPawn) return;

        var buildTool = cameraPawn.CreateTool<UBuildTool>();
        buildTool.StructureClass = structureClass;

        cameraPawn.EquipTool(buildTool);
    }
}
