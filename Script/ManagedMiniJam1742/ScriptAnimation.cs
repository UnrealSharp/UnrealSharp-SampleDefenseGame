using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;

namespace ManagedMiniJam1742;

[UClass]
public partial class UScriptAnimation : UObject
{
    public FVector Location { get; set; }
    public FRotator Rotation { get; set; }

    public virtual void Start() { }

    public virtual void Tick(float delta) { }

    public virtual void End() { }
}
