using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.Attributes;
using UnrealSharp.Engine;

namespace ManagedMiniJam1742;

[UClass]
public partial class UGameEventSubSystem : UGameInstanceSubsystem
{
    public Action<AUnitCharacter> OnUnitKilled;
    public Action<AStructure> OnStructureDestroyed;
}
