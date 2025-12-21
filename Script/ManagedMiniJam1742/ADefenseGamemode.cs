using ManagedMiniJam1724;
using ManagedMiniJam1742.AI;
using ManagedMiniJam1742.AI.Tasks;
using ManagedMiniJam1742.Structures;
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
using UnrealSharp.UnrealSharpCore;

namespace ManagedMiniJam1742;

[UClass]
public partial class ADefenseGamemode : AGameMode
{
    public record struct Wave(int AmountToSpawn, float TimeBetweenSpawns, float TimeBeforeNextWave);

    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public partial float TimeUntilNextWave { get; set; }

    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public partial int CurrentWaveNum { get; set; }

    [UProperty(PropertyFlags.BlueprintReadOnly)]
    public partial int Score { get; set; }

    private IList<AEnemySpawner> enemySpawners;
    private float nextSpawnTick;

    private AResourceManager resourceManager;

    private List<Wave> waves = [
            new Wave(10, 10, 30),
            new Wave(15, 8, 20),
            new Wave(20, 5, 15),
            new Wave(15, 5, 20),
            new Wave(20, 3, 20),
            new Wave(25, 5, 20),
            new Wave(30, 3, 20),
        ];

    private int alreadySpawnedUnitCount;

    private List<AActor> currentWaveUnits;

    private AHQStructure hqStructure;

    public override void BeginPlay()
    {
        base.BeginPlay();

        currentWaveUnits = [];
        CurrentWaveNum = 0;

        Score = 0;

        var gameEvents = GetGameInstanceSubsystem<UGameEventSubSystem>();
        gameEvents.OnUnitKilled += OnUnitKilled;
        gameEvents.OnStructureDestroyed += OnStructureDestroyed;

        hqStructure = GetActorOfClass<AHQStructure>();

        resourceManager = AResourceManager.Get();

        GetAllActorsOfClass(out enemySpawners);

        TimeUntilNextWave = (float)(TimeSeconds + waves[0].TimeBeforeNextWave);
    }

    private void OnUnitKilled(AUnitCharacter target)
    {
        if (target.Team == hqStructure.Team)
        {
            Score -= 100;
        }
        else
        {
            Score += 100;
        }
    }

    private void OnStructureDestroyed(AStructure target)
    {
        if (target.Team == hqStructure.Team)
        {
            Score -= 1000;
        }
        else
        {
            Score += 1000;
        }
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        if (CurrentWaveNum >= waves.Count) return;

        var currentWave = waves[CurrentWaveNum];

        float currentTime = (float)TimeSeconds;

        if (currentTime >= TimeUntilNextWave)
        {
            if (alreadySpawnedUnitCount < currentWave.AmountToSpawn)
            {
                if (currentTime >= nextSpawnTick)
                {
                    var randomSpawn = enemySpawners.Where(s => s.IsActiveSpawner()).RandomElement();

                    if (randomSpawn.FindRandomSpawnableLocation(out var randomLocation))
                    {
                        currentWaveUnits.Add(SpawnEnemyUnit(randomLocation));

                        alreadySpawnedUnitCount++;

                        nextSpawnTick = currentTime + currentWave.TimeBetweenSpawns;
                    }
                }
            }
            else
            {
                if (currentWaveUnits.All(a => a == null || a.IsDestroyed))
                {
                    TimeUntilNextWave = currentTime + currentWave.TimeBeforeNextWave;
                    currentWaveUnits.Clear();
                    alreadySpawnedUnitCount = 0;
                    CurrentWaveNum++;
                    Score += CurrentWaveNum * 1000;
                }
            }
        }
    }

    private AActor SpawnEnemyUnit(FVector location)
    {
        var transform = new FTransform(FQuat.Identity, location + new FVector(0, 0, 150), FVector.One);

        var spawnParams = new FCSSpawnActorParameters();
        spawnParams.SpawnMethod = ESpawnActorCollisionHandlingMethod.AlwaysSpawn;

        var soldierUnit = SpawnActorDeferred(transform, resourceManager.SoldierUnitClass, spawnParams, character =>
        {
            character.AIControllerClass = typeof(AEnemyAdvancedAIController);
            character.Team = ETeam.Yellow;
        });

        return soldierUnit;
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    public bool IsInBetweenRounds()
    {
        return (float)TimeSeconds < TimeUntilNextWave;
    }

    public override bool ReadyToEndMatch()
    {
        return (hqStructure == null || hqStructure.IsDestroyed) ||
            CurrentWaveNum >= waves.Count;
    }

    [UFunction(FunctionFlags.BlueprintCallable | FunctionFlags.BlueprintPure)]
    public int GetWaveEnemiesLeft()
    {
        int amountKilled = alreadySpawnedUnitCount - currentWaveUnits.Count(a => a != null && a.IsValid());

        var currentWave = waves[CurrentWaveNum];
        return currentWave.AmountToSpawn - amountKilled;
    }

    [UFunction(FunctionFlags.BlueprintCallable | FunctionFlags.BlueprintPure)]
    public int GetWaveEnemiesAmount()
    {
        var currentWave = waves[CurrentWaveNum];
        return currentWave.AmountToSpawn;
    }

    [UFunction(FunctionFlags.BlueprintCallable | FunctionFlags.BlueprintPure)]
    public bool IsDefeated() => hqStructure == null || hqStructure.IsDestroyed;
}
