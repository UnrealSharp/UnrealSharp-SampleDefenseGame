using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742.Structures;

[UClass]
public partial class AGuardTowerStructure : AStructure
{
    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float Damage { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float RateOfFire { get; set; }

    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float Range { get; set; }

    private UGameEventSubSystem gameEvents;

    private AActor shootAtTarget;
    private TimeSpan nextFireTime;

    public override void BeginPlay()
    {
        base.BeginPlay();

        gameEvents = GetGameInstanceSubsystem<UGameEventSubSystem>();
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        var time = TimeSpan.FromSeconds(TimeSeconds);

        if (time >= nextFireTime)
        {
            SphereOverlapActors(ActorLocation, Range, [EObjectTypeQuery.ObjectTypeQuery3],
                typeof(AUnitCharacter), [this], out var actors);

            shootAtTarget = actors.Select(a => (AUnitCharacter)a)
                .Where(u => u.Team != Team)
                .OrderBy(u => u.GetDistanceTo(this))
                .FirstOrDefault();

            if (shootAtTarget != null && shootAtTarget.IsValid())
            {
                SetActorRotation(MathLibrary.FindLookAtRotation(ActorLocation, ActorLocation), false);
                OnShoot(shootAtTarget, shootAtTarget.ActorLocation);

                if (shootAtTarget is AUnitCharacter targetUnit)
                {
                    DoDamage(targetUnit);
                }
            }

            nextFireTime = time + TimeSpan.FromSeconds(RateOfFire);
        }
    }

    [UFunction(FunctionFlags.BlueprintEvent)]
    public partial void OnShoot(AActor target, FVector hitLocation);
    public partial void OnShoot_Implementation(AActor target, FVector hitLocation) { }


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
}
