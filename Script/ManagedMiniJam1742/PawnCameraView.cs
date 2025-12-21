using ManagedMiniJam1724.Tools;
using System;
using System.Collections;
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
public partial class APawnCameraView : APawn
{
    private UTool currentTool { get; set; }

    private AResourceManager resourceManager;

    public override void BeginPlay()
    {
        base.BeginPlay();

        CreateAndEquipTool(typeof(USelectTool));

        resourceManager = AResourceManager.Get();

        if (InputComponent is not UEnhancedInputComponent enhancedInputComponent) throw new Exception("Player input is not configured for EnhancedInput");
        if (Controller is not APlayerController playerController) throw new Exception("Controller is not player");

        var enhancedInputSubsystem = GetLocalPlayerSubsystem<UEnhancedInputLocalPlayerSubsystem>(playerController);

        enhancedInputSubsystem.AddMappingContext(resourceManager.MappingContext, 0);

        playerController.ShowMouseCursor = true;

        enhancedInputComponent.BindAction(resourceManager.MoveAction, ETriggerEvent.Triggered, MoveActionTriggered);

        enhancedInputComponent.BindAction(resourceManager.PrimaryAction, ETriggerEvent.Started, PrimaryActionBegin);
        enhancedInputComponent.BindAction(resourceManager.PrimaryAction, ETriggerEvent.Completed, PrimaryActionEnd);

        enhancedInputComponent.BindAction(resourceManager.SecondaryAction, ETriggerEvent.Started, SecondaryActionBegin);
        enhancedInputComponent.BindAction(resourceManager.SecondaryAction, ETriggerEvent.Completed, SecondaryActionEnd);
    }

    [UFunction]
    public void MoveActionTriggered(FInputActionValue value, float elapsedTime, float triggeredTime, UInputAction sender)
    {
        var vector = value.GetAxis2D();

        var newLocation = ActorLocation + new FVector(vector.X, vector.Y, 0) * 100f;

        SetActorLocation(newLocation, false, out _, false);
    }


    [UFunction]
    public void PrimaryActionBegin(FInputActionValue value, float elapsedTime, float triggeredTime, UInputAction sender)
    {
        currentTool?.OnStartAction(UTool.ActionType.Primary);
    }

    [UFunction]
    public void PrimaryActionEnd(FInputActionValue value, float elapsedTime, float triggeredTime, UInputAction sender)
    {
        currentTool?.OnEndAction(UTool.ActionType.Primary);
    }

    [UFunction]
    public void SecondaryActionBegin(FInputActionValue value, float elapsedTime, float triggeredTime, UInputAction sender)
    {
        currentTool?.OnStartAction(UTool.ActionType.Secondary);
    }

    [UFunction]
    public void SecondaryActionEnd(FInputActionValue value, float elapsedTime, float triggeredTime, UInputAction sender)
    {
        currentTool?.OnEndAction(UTool.ActionType.Secondary);
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    public UTool CreateTool(TSubclassOf<UTool> toolClass)
    {
        if (Controller is not APlayerController playerController) throw new Exception("Controller is not player");

        var newTool = NewObject(this, toolClass);
        newTool.PlayerController = playerController;
        newTool.Pawn = this;
        return newTool;
    }

    public T CreateTool<T>() where T : UTool
    {
        if (Controller is not APlayerController playerController) throw new Exception("Controller is not player");

        var newTool = NewObject<T>(this, typeof(T));
        newTool.PlayerController = playerController;
        newTool.Pawn = this;
        return newTool;
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void EquipTool(UTool tool)
    {
        currentTool?.OnUnEquip();
        currentTool = tool;
        tool.OnEquip();
    }

    public void CreateAndEquipTool(TSubclassOf<UTool> toolClass)
    {
        if (Controller is not APlayerController playerController) throw new Exception("Controller is not player");

        var newTool = NewObject(this, toolClass);
        newTool.PlayerController = playerController;
        newTool.Pawn = this;

        currentTool?.OnUnEquip();
        currentTool = newTool;
        newTool.OnEquip();
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        currentTool?.Tick(deltaSeconds);
    }
}
