using ManagedMiniJam1724;
using ManagedMiniJam1742.AI;
using ManagedMiniJam1742.AI.Tasks;
using ManagedMiniJam1742.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.UMG;
using static ManagedMiniJam1742.AI.Tasks.UAttackTask;

namespace ManagedMiniJam1742;

[UClass]
public partial class AUnitCharacter : ACharacter
{
    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial ETeam Team { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float Range { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float Damage { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float VehicleDamage { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float StructureDamage { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float RateOfFire { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float Health { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial int Cost { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float BuildTime { get; set; }

    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public partial bool IsShooting { get; private set; }

    [UProperty(PropertyFlags.EditAnywhere | PropertyFlags.BlueprintReadWrite, DefaultComponent = true, AttachmentComponent = nameof(CapsuleComponent))]
    public partial UWidgetComponent HealthBarComponent { get; set; }

    private AAdvancedAIController aiController;
    private AResourceManager resourceManager;

    private UGameEventSubSystem gameEvents;

    private AActor? shootAtTarget;
    private TimeSpan nextFireTime;

    private float initialHealth;
    private double healthBarVisibleUtillTimestamp;

    public override void ConstructionScript()
    {
        base.ConstructionScript();

        resourceManager = AResourceManager.Get();

        if (resourceManager == null) return;
        var staticMeshComponents = GetComponentsByClass<UStaticMeshComponent>();
        foreach (var staticMeshComponent in staticMeshComponents)
        {
            for (int i = 0; i < staticMeshComponent.NumMaterials; i++)
            {
                if (Team == ETeam.Green)
                {
                    staticMeshComponent.SetMaterial(i, resourceManager.GreenPlastic);
                }
                else if (Team == ETeam.Yellow)
                {
                    staticMeshComponent.SetMaterial(i, resourceManager.YellowPlastic);
                }
            }
        }
    }

    public override void BeginPlay()
    {
        base.BeginPlay();

        gameEvents = GetGameInstanceSubsystem<UGameEventSubSystem>();

        initialHealth = Health;

        UpdateHealthUI();

        if (Controller is not AAdvancedAIController aiController) return;
        this.aiController = aiController;
    }

    [UFunction(FunctionFlags.BlueprintEvent)]
    public partial void OnUnitShoot(AActor target, FVector hitLocation);
    public partial void OnUnitShoot_Implementation(AActor target, FVector hitLocation) { }

    public void DoDamage(AUnitCharacter target)
    {
        target.Health -= float.Min(Damage, Health);
        target.OnTakeDamage(this);
        target.UpdateHealthUI(true);
        if (target.Health <= 0)
        {
            gameEvents.OnUnitKilled?.Invoke(target);
            target.DestroyActor();
        }
    }

    public void DoDamage(AStructure target)
    {
        target.Health -= float.Min(StructureDamage, Health);
        target.OnTakeDamage(this);
        target.UpdateHealthUI(true);
        if (target.Health <= 0)
        {
            gameEvents.OnStructureDestroyed?.Invoke(target);
            target.DestroyActor();
        }
    }

    [UFunction(FunctionFlags.BlueprintEvent)]
    public partial void OnTakeDamage(AActor attacker);
    public partial void OnTakeDamage_Implementation(AActor attacker) { }


    public void UpdateHealthUI(bool uiVisible = false)
    {
        if (HealthBarComponent.Widget is not UHealthBarWidget healthBarWidget) return;

        healthBarWidget.HealthPercentage = Health / initialHealth;

        HealthBarComponent.SetVisibility(uiVisible);

        healthBarVisibleUtillTimestamp = TimeSeconds + 2.0;
    }

    [UFunction]
    public void ChangeTeam(ETeam newTeam)
    {
        var primitiveComponents = GetComponentsByClass<UStaticMeshComponent>();
        foreach (var primitiveComponent in primitiveComponents)
        {
            for (int i = 0; i < primitiveComponent.NumMaterials; i++)
            {
                if (newTeam == ETeam.Green)
                {
                    primitiveComponent.SetMaterial(i, resourceManager.GreenPlastic);
                }
                else if (newTeam == ETeam.Yellow)
                {
                    primitiveComponent.SetMaterial(i, resourceManager.YellowPlastic);
                }
            }
        }

        Team = newTeam;
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);        

        var animator = GetComponentByClass<UAnimatedSceneComponent>();

        if (animator != null)
        {
            if (Velocity.Length() > 50)
            {
                if (!animator.IsPlaying)
                {
                    animator.PlayAnimation();
                }
            }
            else
            {
                if (animator.IsPlaying)
                {
                    animator.StopAnimation();
                }
            }
        }

        if (IsShooting)
        {
            if (shootAtTarget != null && shootAtTarget.IsValid())
            {
                ShootTick(deltaSeconds);
            }
            else
            {
                StopShootAtTarget();
            }
        }

        if (HealthBarComponent.IsVisible() && TimeSeconds > healthBarVisibleUtillTimestamp)
        {
            HealthBarComponent.SetVisibility(false);
        }
    }

    public virtual void GoToLocation(FVector dest)
    {
        var previouslyAttacking = aiController.HasCurrentTask<UAttackTask>();

        aiController.StopTasks();

        var moveTask = aiController.CreateTask<UMoveTask>();
        moveTask.MoveToLocation = dest;
        moveTask.AttackMove = !previouslyAttacking;
        aiController.AddTask(moveTask);
    }

    public virtual void GoToActor(AActor actor)
    {
        aiController.MoveToActor(actor);
    }

    public void StartShootAtTarget(AActor target)
    {
        shootAtTarget = target;
        IsShooting = true;
    }

    private void ShootTick(float deltaSeconds)
    {
        var time = TimeSpan.FromSeconds(TimeSeconds);

        if (time >= nextFireTime)
        {
            bool foundTarget = false;
            List<EObjectTypeQuery> hitObjects = [EObjectTypeQuery.ObjectTypeQuery1, EObjectTypeQuery.ObjectTypeQuery2,
                EObjectTypeQuery.ObjectTypeQuery3, EObjectTypeQuery.ObjectTypeQuery4, EObjectTypeQuery.ObjectTypeQuery5, EObjectTypeQuery.ObjectTypeQuery8];

            if (CanSeeTarget(shootAtTarget, out var hit))
            {
                SetActorRotation(MathLibrary.FindLookAtRotation(ActorLocation, shootAtTarget.ActorLocation), false);
                OnUnitShoot(shootAtTarget, hit.ImpactPoint);

                if (shootAtTarget is AUnitCharacter targetUnit)
                {
                    DoDamage(targetUnit);
                }
                else if (shootAtTarget is AStructure targetStructure)
                {
                    DoDamage(targetStructure);
                }

                foundTarget = true;
            }

            if (!foundTarget)
            {
                StopShootAtTarget();
            }

            nextFireTime = time + TimeSpan.FromSeconds(RateOfFire);
        }
    }

    public void StopShootAtTarget()
    {
        shootAtTarget = null;
        IsShooting = false;
    }

    public bool CanSeeTarget(AActor? target, out FHitResult hitResult)
    {
        hitResult = new FHitResult();

        if (target == null) return false;
        if (!target.IsValid()) return false;
        if (GetDistanceTo(target) > Range) return false;

        List<EObjectTypeQuery> hitObjects = [EObjectTypeQuery.ObjectTypeQuery1, EObjectTypeQuery.ObjectTypeQuery2,
            EObjectTypeQuery.ObjectTypeQuery3, EObjectTypeQuery.ObjectTypeQuery4, EObjectTypeQuery.ObjectTypeQuery5, EObjectTypeQuery.ObjectTypeQuery8];

        Controller.ControlledPawn.GetActorBounds(true, out var unitOrigin, out var unitExtent);
        target.GetActorBounds(true, out var targetOrigin, out var targetExtent);

        if (LineTraceForObjects(unitOrigin, targetOrigin,
            hitObjects, false, [Controller.ControlledPawn], EDrawDebugTrace.None, out hitResult, true))
        {
            if (hitResult.Actor == target)
            {
                return true;
            }
        }

        return false;
    }
}