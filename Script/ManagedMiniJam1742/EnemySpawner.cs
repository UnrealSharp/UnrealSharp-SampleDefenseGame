using ManagedMiniJam1742.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;
using UnrealSharp.NavigationSystem;

namespace ManagedMiniJam1742;

[UClass]
public partial class AEnemySpawner : AActor
{
    [UProperty(PropertyFlags.EditAnywhere, DefaultComponent = true, RootComponent = true)]
    public partial USphereComponent SphereComponent { get; set; }

    [UProperty(PropertyFlags.EditAnywhere)]
    public partial int ActiveOnWave { get; set; }

    private AHQStructure hqStructure;
    private IReadOnlyList<FVector> pathPoints;

    private float nextPathUpdate;

    public override void BeginPlay()
    {
        base.BeginPlay();

        pathPoints = [];

        SphereComponent.CollisionEnabled = ECollisionEnabled.NoCollision;

        hqStructure = GetActorOfClass<AHQStructure>();

        nextPathUpdate = 0;
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        if (GameMode is not ADefenseGamemode defenseGamemode) return;

        if (!IsActiveSpawner()) return;

        if (!defenseGamemode.IsInBetweenRounds()) return;

        if (TimeSeconds >= nextPathUpdate)
        {
            hqStructure.StaticMeshComponent.GetClosestPointOnCollision(ActorLocation, out var closetPointOnBody);

            var path = UNavigationSystemV1.FindPathToLocationSynchronously(ActorLocation, closetPointOnBody);

            if (!path.IsDestroyed && path.IsValid())
            {
                pathPoints = path.PathPoints;
            }

            nextPathUpdate = (float)TimeSeconds + 5.0f;
        }

        if (pathPoints == null) return;
        if (pathPoints.Count <= 0) return;

        if (pathPoints.Count == 1)
        {
            var currentPathPoint = pathPoints[0];

            DrawPath(currentPathPoint, new FVector(hqStructure.ActorLocation.X, hqStructure.ActorLocation.Y, currentPathPoint.Z));
        }
        else
        {
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                var currentPathPoint = pathPoints[i];
                var nextPathPoint = pathPoints[i + 1];

                DrawPath(currentPathPoint, nextPathPoint);
            }
        }
    }

    private void DrawPath(FVector startPath, FVector endPath)
    {
        var direction = FVector.Normalize(endPath - startPath);

        var dist = FVector.Distance(endPath, startPath);

        if (double.IsInfinity(dist)) return;

        for (int i = 0; i < double.Ceiling(dist / 1000); i++)
        {
            var startPosition = startPath + (direction * (i * 1000)) + new FVector(0, 0, 100);

            //DrawDebugSphere(startPosition, 100);

            var color = MathLibrary.LinearColorLerp(FLinearColor.Red, FLinearColor.DarkRed, float.Cos((float)TimeSeconds * 5));

            //DrawDebugSphere(startPosition, 100);
            DrawDebugArrow(startPosition, startPosition + (direction * 100), 10000, color, 0, 30);
        }
    }

    public bool FindRandomSpawnableLocation(out FVector randomLocation)
    {
        return UNavigationSystemV1.GetRandomReachablePointInRadius(ActorLocation, out randomLocation, SphereComponent.ScaledSphereRadius);
    }

    [UFunction(FunctionFlags.BlueprintPure | FunctionFlags.BlueprintCallable)]
    public bool IsActiveSpawner()
    {
        if (GameMode is not ADefenseGamemode defenseGamemode) return false;
        return defenseGamemode.CurrentWaveNum >= ActiveOnWave;
    }
}
