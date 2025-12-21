using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnrealSharp.AIModule;
using UnrealSharp.Attributes;
using UnrealSharp.CoreUObject;

namespace ManagedMiniJam1742.AI;

[UClass]
public partial class UMyAITask : UObject
{
    public AAdvancedAIController Controller { get; set; }

    public bool Finished { get; private set; }

    public bool FinishedSuccessfully { get; private set; }

    public bool HasStarted { get; private set; }

    public UMyAITask ContinueTask { get; private set; }

    public void Start()
    {
        HasStarted = true;
        OnStart();
    }

    public void Tick(float delta)
    {
        if (HasStarted && !Finished)
        {
            OnTick(delta);
        }
    }

    public void Complete(bool successful = false) 
    {
        HasStarted = false;
        Finished = true;
        OnStop();
    }

    public void ContinueWithTask(UMyAITask task)
    {
        ContinueTask = task;
    }

    protected virtual void OnStart() { }
    protected virtual void OnTick(float delta) { }
    protected virtual void OnStop() { }
}
