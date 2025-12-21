using ManagedMiniJam1724;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742;

[UClass]
public partial class AGhostActor : AActor
{
    [UProperty(PropertyFlags.EditAnywhere, DefaultComponent = true, RootComponent = true)]
    public partial UStaticMeshComponent StaticMeshComponent { get; set; }

    private AResourceManager resourceManager;

    public override void BeginPlay()
    {
        base.BeginPlay();

        resourceManager = AResourceManager.Get();

        StaticMeshComponent.CollisionEnabled = ECollisionEnabled.NoCollision;
    }

    public void SetupMesh(UStaticMesh mesh)
    {
        StaticMeshComponent.SetStaticMesh(mesh);

        for (int i = 0; i < StaticMeshComponent.Materials.Count; i++)
        {
            StaticMeshComponent.SetMaterial(i, resourceManager.GhostMaterial);
        }

        StaticMeshComponent.CollisionEnabled = ECollisionEnabled.NoCollision;
    }

    public void SetCanBuild(bool canBuild)
    {
        MaterialLibrary.SetScalarParameterValue(resourceManager.GhostMaterialParams, "CanBuild", canBuild ? 1.0f : 0.0f);
    }
}
