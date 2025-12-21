using ManagedMiniJam1742.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.EnhancedInput;

namespace ManagedMiniJam1724;

[UClass]
public partial class AResourceManager : AActor
{
    public static AResourceManager Get()
    {
        return GetActorOfClass<AResourceManager>();
    }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UInputMappingContext MappingContext { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UInputAction MoveAction { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UInputAction PrimaryAction { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UInputAction SecondaryAction { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UMaterial DissolveMaterial { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial TArray<USoundWave> UnitConfirmationSounds { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial USoundWave StructureBuiltConfirmationSound { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UMaterial YellowPlastic { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UMaterial GreenPlastic { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial TSubclassOf<ASoldierCharacter> SoldierUnitClass { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UMaterial GhostMaterial { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial UMaterialParameterCollection GhostMaterialParams { get; set; }
}
