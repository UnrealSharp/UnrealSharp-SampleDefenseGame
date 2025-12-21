using ManagedMiniJam1724;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742;

[UClass]
public partial class APlasticResource : AActor
{
    [UProperty(PropertyFlags.EditAnywhere, DefaultComponent = true, RootComponent = true)]
    public partial UStaticMeshComponent StaticMeshComponent { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial int Plastic { get; set; }

    private AResourceManager resourceManager;

    public APlasticResource()
    {
        Plastic = 1000;
    }

    public override void BeginPlay()
    {
        base.BeginPlay();

        resourceManager = GetActorOfClass<AResourceManager>();
    }

    public int Harvest(int amount)
    {
        if (StaticMeshComponent != null)
        {
            if (resourceManager != null)
            {
                StaticMeshComponent.OverlayMaterial = resourceManager.DissolveMaterial;
            }
        }

        int maxCanHarvest = Math.Min(amount, Plastic);

        Plastic -= maxCanHarvest;

        if (Plastic <= 0)
        {
            DestroyActor();
        }

        return maxCanHarvest;
    }

    public void StopHarvesting()
    {
        if (StaticMeshComponent != null)
        {
            if (resourceManager != null)
            {
                StaticMeshComponent.OverlayMaterial = null;
            }
        }
    }
}
