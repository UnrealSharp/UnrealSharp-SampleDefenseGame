using ManagedMiniJam1742.AI;
using ManagedMiniJam1742.AI.Tasks;
using ManagedMiniJam1742.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;
using UnrealSharp.Niagara;

namespace ManagedMiniJam1742.Units;

[UClass]
public partial class ADumpTruckCharacter : AUnitCharacter
{
    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite)]
    public partial int CarryingPlastic { get; set; }

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite)]
    public partial int CarryingPlasticLimit { get; set; }

    private int previousCarryingPlastic;

    public UNiagaraComponent HarvestBeamComponent { get; private set; }

    private APlasticResource targetResource;
    private AAdvancedAIController aiController;

    public ADumpTruckCharacter()
    {
        CarryingPlasticLimit = 100;
    }

    [UFunction(FunctionFlags.BlueprintEvent)]
    public partial void OnCarryPlasticChanged();
    public partial void OnCarryPlasticChanged_Implementation() { }


    public override void BeginPlay()
    {
        base.BeginPlay();

        HarvestBeamComponent = GetComponentByClass<UNiagaraComponent>();

        if (Controller is not AAdvancedAIController aiController) return;
        this.aiController = aiController;
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        if (CarryingPlastic != previousCarryingPlastic)
        {
            OnCarryPlasticChanged();
            previousCarryingPlastic = CarryingPlastic;
        }
    }

    public override void GoToActor(AActor actor)
    {
        if (actor is APlasticResource plasticResource)
        {
            aiController.StopTasks();

            var task = aiController.CreateTask<UHarvestPlasticTask>();
            task.Target = plasticResource;
            aiController.AddTask(task);
        }
        else
        {
            aiController.StopTasks();
        }
    }
}
