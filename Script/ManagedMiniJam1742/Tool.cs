using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;
using UnrealSharp.Engine;

namespace ManagedMiniJam1724;

[UClass(ClassFlags.Abstract)]
public partial class UTool : UObject
{
    public enum ActionType
    {
        Primary, Secondary
    }

    public APawnCameraView Pawn { get; set; }
    public APlayerController PlayerController { get; set; }

    public virtual void OnStartAction(ActionType actionType) { }

    public virtual void OnEndAction(ActionType actionType) { }

    public virtual void OnEquip() { }

    public virtual void Tick(float delta) { }

    public virtual void OnUnEquip() { }
}

