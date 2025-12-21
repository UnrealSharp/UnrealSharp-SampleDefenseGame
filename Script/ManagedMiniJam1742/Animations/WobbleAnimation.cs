using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;

namespace ManagedMiniJam1742.Animations;

[UClass]
public partial class UWobbleAnimation : UScriptAnimation
{
    public override void Tick(float delta)
    {
        base.Tick(delta);

        Location = new UnrealSharp.CoreUObject.FVector(0, 0, (double.Cos(TimeSeconds * 10) * 50) + 50);
        Rotation = new UnrealSharp.CoreUObject.FRotator(0, 0, (double.Sin(TimeSeconds * 10) * 15));
    }
}
