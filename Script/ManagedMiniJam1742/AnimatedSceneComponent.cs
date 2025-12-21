using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742;

[UClass]
public partial class UAnimatedSceneComponent : USceneComponent
{
    [UProperty(PropertyFlags.EditAnywhere)]
    public partial TSubclassOf<UScriptAnimation> Animation { get; set; }

    public bool IsPlaying { get; private set; }

    private UScriptAnimation currentAnimation;

    private FVector originalLocation;
    private FRotator originaRotation;

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void PlayAnimation()
    {
        if (IsPlaying) return;

        if (!Animation.IsValid) return;

        originalLocation = RelativeLocation;
        originaRotation = RelativeRotation;

        currentAnimation = NewObject(this, Animation);
        currentAnimation.Start();

        IsPlaying = true;
    }

    [UFunction(FunctionFlags.BlueprintCallable)]
    public void StopAnimation()
    {
        if (!IsPlaying) return;

        if (currentAnimation == null) return;
        if (!currentAnimation.IsValid()) return;

        SetRelativeLocationAndRotation(originalLocation, originaRotation, false, out _, false);

        currentAnimation.End();
        IsPlaying = false;
    }

    public override void Tick(float deltaSeconds)
    {
        base.Tick(deltaSeconds);

        if (IsPlaying)
        {
            currentAnimation?.Tick(deltaSeconds);

            SetRelativeLocationAndRotation(originalLocation + currentAnimation.Location,
                originaRotation + currentAnimation.Rotation, false, out _, false);
        }
    }

}
