using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.UMG;

namespace ManagedMiniJam1742.UI;

[UClass]
public partial class UHealthBarWidget : UUserWidget
{
    [UProperty(PropertyFlags.BlueprintReadWrite)]
    public partial float HealthPercentage { get; set; }
}
